/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Web;
using ASC.Common.Caching;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public static class FileConverter
    {
        private static readonly object locker = new object();
        private static readonly IDictionary<File, ConvertFileOperationResult> conversionQueue = new Dictionary<File, ConvertFileOperationResult>(new FileComparer());
        private static readonly ICache cache = AscCache.Default;

        private static Timer timer;
        private static readonly object singleThread = new object();
        private const int TIMER_PERIOD = 500;


        public static bool EnableAsUploaded
        {
            get { return FileUtility.ExtsMustConvert.Any() && !string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl); }
        }

        public static bool MustConvert(File file)
        {
            if (file == null) return false;

            var ext = FileUtility.GetFileExtension(file.Title);
            return FileUtility.ExtsMustConvert.Contains(ext);
        }

        public static bool EnableConvert(File file, string toExtension)
        {
            if (file == null || string.IsNullOrEmpty(toExtension))
            {
                return false;
            }

            if (file.Encrypted)
            {
                return false;
            }

            var fileExtension = file.ConvertedExtension;
            if (fileExtension.Trim('.').Equals(toExtension.Trim('.'), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fileExtension = FileUtility.GetFileExtension(file.Title);
            if (FileUtility.InternalExtension.Values.Contains(toExtension))
            {
                return true;
            }

            return FileUtility.ExtsConvertible.Keys.Contains(fileExtension) && FileUtility.ExtsConvertible[fileExtension].Contains(toExtension);
        }

        public static Stream Exec(File file)
        {
            return Exec(file, FileUtility.GetInternalExtension(file.Title));
        }

        public static Stream Exec(File file, string toExtension)
        {
            if (!EnableConvert(file, toExtension))
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    return fileDao.GetFileStream(file);
                }
            }

            if (file.ContentLength > SetupInfo.AvailableFileSize)
            {
                throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeConvert, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));
            }

            var fileUri = PathProvider.GetFileStreamUrl(file);
            var docKey = DocumentServiceHelper.GetDocKey(file);
            string convertUri;
            fileUri = DocumentServiceConnector.ReplaceCommunityAdress(fileUri);
            DocumentServiceConnector.GetConvertedUri(fileUri, file.ConvertedExtension, toExtension, docKey, null, false, out convertUri);

            if (WorkContext.IsMono && ServicePointManager.ServerCertificateValidationCallback == null)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, c, n, p) => true; //HACK: http://ubuntuforums.org/showthread.php?t=1841740
            }
            return new ResponseStream(((HttpWebRequest)WebRequest.Create(convertUri)).GetResponse());
        }

        public static File ExecSync(File file, string doc)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var fileSecurity = Global.GetFilesSecurity();
                if (!fileSecurity.CanRead(file))
                {
                    var readLink = FileShareLink.Check(doc, true, fileDao, out file);
                    if (file == null)
                    {
                        throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);
                    }
                    if (!readLink)
                    {
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                    }
                }
            }

            var fileUri = PathProvider.GetFileStreamUrl(file);
            var fileExtension = file.ConvertedExtension;
            var toExtension = FileUtility.GetInternalExtension(file.Title);
            var docKey = DocumentServiceHelper.GetDocKey(file);

            string convertUri;
            fileUri = DocumentServiceConnector.ReplaceCommunityAdress(fileUri);
            DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, null, false, out convertUri);

            return SaveConvertedFile(file, convertUri);
        }

        public static void ExecAsync(File file, bool deleteAfter, string password = null)
        {
            if (!MustConvert(file))
            {
                throw new ArgumentException(FilesCommonResource.ErrorMassage_NotSupportedFormat);
            }
            if (!String.IsNullOrEmpty(file.ConvertedType) || FileUtility.InternalExtension.Values.Contains(FileUtility.GetFileExtension(file.Title)))
            {
                return;
            }

            FileMarker.RemoveMarkAsNew(file);

            lock (locker)
            {
                if (conversionQueue.ContainsKey(file))
                {
                    return;
                }

                var queueResult = new ConvertFileOperationResult
                    {
                        Source = String.Format("{{\"id\":\"{0}\", \"version\":\"{1}\"}}", file.ID, file.Version),
                        OperationType = FileOperationType.Convert,
                        Error = String.Empty,
                        Progress = 0,
                        Result = String.Empty,
                        Processed = "",
                        Id = String.Empty,
                        TenantId = TenantProvider.CurrentTenantID,
                        Account = SecurityContext.CurrentAccount,
                        Delete = deleteAfter,
                        StartDateTime = DateTime.Now,
                        Url = HttpContext.Current != null ? HttpContext.Current.Request.GetUrlRewriter().ToString() : null,
                        Password = password
                    };
                conversionQueue.Add(file, queueResult);
                cache.Insert(GetKey(file), queueResult, TimeSpan.FromMinutes(10));

                if (timer == null)
                {
                    timer = new Timer(CheckConvertFilesStatus, null, 0, Timeout.Infinite);
                }
                else
                {
                    timer.Change(0, Timeout.Infinite);
                }
            }
        }

        public static bool IsConverting(File file)
        {
            if (!MustConvert(file) || !string.IsNullOrEmpty(file.ConvertedType))
            {
                return false;
            }
            var result = cache.Get<ConvertFileOperationResult>(GetKey(file));
            return result != null && result.Progress != 100 && string.IsNullOrEmpty(result.Error);
        }

        public static IEnumerable<FileOperationResult> GetStatus(IEnumerable<KeyValuePair<File, bool>> filesPair)
        {
            var fileSecurity = Global.GetFilesSecurity();
            var result = new List<FileOperationResult>();
            foreach (var pair in filesPair)
            {
                var file = pair.Key;
                var key = GetKey(file);
                var operation = cache.Get<ConvertFileOperationResult>(key);
                if (operation != null && (pair.Value || fileSecurity.CanRead(file)))
                {
                    result.Add(operation);
                    lock (locker)
                    {
                        if (operation.Progress == 100)
                        {
                            conversionQueue.Remove(file);
                            cache.Remove(key);
                        }
                    }
                }
            }
            return result;
        }

        private static String FileJsonSerializer(File file, string folderTitle)
        {
            if (file == null) return string.Empty;

            EntryManager.SetFileStatus(file);
            return
                string.Format("{{ \"id\": \"{0}\"," +
                              " \"title\": \"{1}\"," +
                              " \"version\": \"{2}\"," +
                              " \"folderId\": \"{3}\"," +
                              " \"folderTitle\": \"{4}\"," +
                              " \"fileXml\": \"{5}\" }}",
                              file.ID,
                              file.Title,
                              file.Version,
                              file.FolderID,
                              folderTitle ?? "",
                              File.Serialize(file).Replace('"', '\''));
        }

        private static void CheckConvertFilesStatus(object _)
        {
            if (Monitor.TryEnter(singleThread))
            {
                try
                {
                    List<File> filesIsConverting;
                    lock (locker)
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);

                        conversionQueue.Where(x => !string.IsNullOrEmpty(x.Value.Processed)
                                                   && (x.Value.Progress == 100 && DateTime.UtcNow - x.Value.StopDateTime > TimeSpan.FromMinutes(1) ||
                                                       DateTime.UtcNow - x.Value.StopDateTime > TimeSpan.FromMinutes(10)))
                                       .ToList()
                                       .ForEach(x =>
                                           {
                                               conversionQueue.Remove(x);
                                               cache.Remove(GetKey(x.Key));
                                           });

                        Global.Logger.DebugFormat("Run CheckConvertFilesStatus: count {0}", conversionQueue.Count);

                        if (conversionQueue.Count == 0)
                        {
                            return;
                        }

                        filesIsConverting = conversionQueue
                            .Where(x => String.IsNullOrEmpty(x.Value.Processed))
                            .Select(x => x.Key)
                            .ToList();
                    }

                    var fileSecurity = Global.GetFilesSecurity();
                    foreach (var file in filesIsConverting)
                    {
                        var fileUri = file.ID.ToString();
                        string convertedFileUrl;
                        int operationResultProgress;

                        try
                        {
                            int tenantId;
                            IAccount account;
                            string password;

                            lock (locker)
                            {
                                if (!conversionQueue.Keys.Contains(file)) continue;

                                var operationResult = conversionQueue[file];
                                if (!string.IsNullOrEmpty(operationResult.Processed)) continue;

                                operationResult.Processed = "1";
                                tenantId = operationResult.TenantId;
                                account = operationResult.Account;
                                password = operationResult.Password;

                                if (HttpContext.Current == null && !WorkContext.IsMono)
                                {
                                    HttpContext.Current = new HttpContext(
                                        new HttpRequest("hack", operationResult.Url, string.Empty),
                                        new HttpResponse(new StringWriter()));
                                }

                                cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(10));
                            }

                            CoreContext.TenantManager.SetCurrentTenant(tenantId);
                            SecurityContext.AuthenticateMe(account);

                            var user = CoreContext.UserManager.GetUsers(account.ID);
                            var culture = string.IsNullOrEmpty(user.CultureName) ? CoreContext.TenantManager.GetCurrentTenant().GetCulture() : CultureInfo.GetCultureInfo(user.CultureName);
                            Thread.CurrentThread.CurrentCulture = culture;
                            Thread.CurrentThread.CurrentUICulture = culture;

                            if (!fileSecurity.CanRead(file) && file.RootFolderType != FolderType.BUNCH)
                            {
                                //No rights in CRM after upload before attach
                                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                            }
                            if (file.ContentLength > SetupInfo.AvailableFileSize)
                            {
                                throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeConvert, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));
                            }

                            fileUri = PathProvider.GetFileStreamUrl(file);

                            var toExtension = FileUtility.GetInternalExtension(file.Title);
                            var fileExtension = file.ConvertedExtension;
                            var docKey = DocumentServiceHelper.GetDocKey(file);

                            fileUri = DocumentServiceConnector.ReplaceCommunityAdress(fileUri);
                            operationResultProgress = DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, password, true, out convertedFileUrl);
                        }
                        catch (Exception exception)
                        {
                            DocumentService.DocumentServiceException documentServiceException;
                            var password = exception.InnerException != null
                                           && ((documentServiceException = exception.InnerException as DocumentService.DocumentServiceException) != null)
                                           && documentServiceException.Code == DocumentService.DocumentServiceException.ErrorCode.ConvertPassword;

                            Global.Logger.Error(string.Format("Error convert {0} with url {1}", file.ID, fileUri), exception);
                            lock (locker)
                            {
                                if (conversionQueue.Keys.Contains(file))
                                {
                                    var operationResult = conversionQueue[file];
                                    if (operationResult.Delete)
                                    {
                                        conversionQueue.Remove(file);
                                        cache.Remove(GetKey(file));
                                    }
                                    else
                                    {
                                        operationResult.Progress = 100;
                                        operationResult.StopDateTime = DateTime.UtcNow;
                                        operationResult.Error = exception.Message;
                                        if (password) operationResult.Result = "password";
                                        cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(10));
                                    }
                                }
                            }
                            continue;
                        }

                        operationResultProgress = Math.Min(operationResultProgress, 100);
                        if (operationResultProgress < 100)
                        {
                            lock (locker)
                            {
                                if (conversionQueue.Keys.Contains(file))
                                {
                                    var operationResult = conversionQueue[file];

                                    if (DateTime.Now - operationResult.StartDateTime > TimeSpan.FromMinutes(10))
                                    {
                                        operationResult.StopDateTime = DateTime.UtcNow;
                                        operationResult.Error = FilesCommonResource.ErrorMassage_ConvertTimeout;
                                        Global.Logger.ErrorFormat("CheckConvertFilesStatus timeout: {0} ({1})", file.ID, file.ContentLengthString);
                                    }
                                    else
                                    {
                                        operationResult.Processed = "";
                                    }
                                    operationResult.Progress = operationResultProgress;
                                    cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(10));
                                }
                            }

                            Global.Logger.Debug("CheckConvertFilesStatus iteration continue");
                            continue;
                        }

                        File newFile = null;
                        var operationResultError = string.Empty;

                        try
                        {
                            newFile = SaveConvertedFile(file, convertedFileUrl);
                        }
                        catch (Exception e)
                        {
                            operationResultError = e.Message;

                            Global.Logger.ErrorFormat("{0} ConvertUrl: {1} fromUrl: {2}: {3}", operationResultError, convertedFileUrl, fileUri, e);
                            continue;
                        }
                        finally
                        {
                            lock (locker)
                            {
                                if (conversionQueue.Keys.Contains(file))
                                {
                                    var operationResult = conversionQueue[file];
                                    if (operationResult.Delete)
                                    {
                                        conversionQueue.Remove(file);
                                        cache.Remove(GetKey(file));
                                    }
                                    else
                                    {
                                        if (newFile != null)
                                        {
                                            using (var folderDao = Global.DaoFactory.GetFolderDao())
                                            {
                                                var folder = folderDao.GetFolder(newFile.FolderID);
                                                var folderTitle = fileSecurity.CanRead(folder) ? folder.Title : null;
                                                operationResult.Result = FileJsonSerializer(newFile, folderTitle);
                                            }
                                        }

                                        operationResult.Progress = 100;
                                        operationResult.StopDateTime = DateTime.UtcNow;
                                        operationResult.Processed = "1";
                                        if (!string.IsNullOrEmpty(operationResultError))
                                        {
                                            operationResult.Error = operationResultError;
                                        }
                                        cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(10));
                                    }
                                }
                            }
                        }

                        Global.Logger.Debug("CheckConvertFilesStatus iteration end");
                    }

                    lock (locker)
                    {
                        timer.Change(TIMER_PERIOD, TIMER_PERIOD);
                    }
                }
                catch (Exception exception)
                {
                    Global.Logger.Error(exception.Message, exception);
                    lock (locker)
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                finally
                {
                    Monitor.Exit(singleThread);
                }
            }
        }

        private static File SaveConvertedFile(File file, string convertedFileUrl)
        {
            var fileSecurity = Global.GetFilesSecurity();
            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                File newFile = null;
                var markAsTemplate = false;
                var newFileTitle = FileUtility.ReplaceFileExtension(file.Title, FileUtility.GetInternalExtension(file.Title));

                if (!FilesSettings.StoreOriginalFiles && fileSecurity.CanEdit(file))
                {
                    newFile = (File)file.Clone();
                    newFile.Version++;
                    markAsTemplate = FileUtility.ExtsTemplate.Contains(FileUtility.GetFileExtension(file.Title), StringComparer.CurrentCultureIgnoreCase)
                        && FileUtility.ExtsWebTemplate.Contains(FileUtility.GetFileExtension(newFileTitle), StringComparer.CurrentCultureIgnoreCase);
                }
                else
                {
                    var folderId = Global.FolderMy;

                    var parent = folderDao.GetFolder(file.FolderID);
                    if (parent != null
                        && fileSecurity.CanCreate(parent))
                    {
                        folderId = parent.ID;
                    }

                    if (Equals(folderId, 0)) throw new SecurityException(FilesCommonResource.ErrorMassage_FolderNotFound);

                    if (FilesSettings.UpdateIfExist && (parent != null && folderId != parent.ID || !file.ProviderEntry))
                    {
                        newFile = fileDao.GetFile(folderId, newFileTitle);
                        if (newFile != null && fileSecurity.CanEdit(newFile) && !EntryManager.FileLockedForMe(newFile.ID) && !FileTracker.IsEditing(newFile.ID))
                        {
                            newFile.Version++;
                        }
                        else
                        {
                            newFile = null;
                        }
                    }

                    if (newFile == null)
                    {
                        newFile = new File { FolderID = folderId };
                    }
                }

                newFile.Title = newFileTitle;
                newFile.ConvertedType = null;
                newFile.Comment = string.Format(FilesCommonResource.CommentConvert, file.Title);

                var req = (HttpWebRequest)WebRequest.Create(convertedFileUrl);

                if (WorkContext.IsMono && ServicePointManager.ServerCertificateValidationCallback == null)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (s, c, n, p) => true; //HACK: http://ubuntuforums.org/showthread.php?t=1841740
                }

                try
                {
                    using (var convertedFileStream = new ResponseStream(req.GetResponse()))
                    {
                        newFile.ContentLength = convertedFileStream.Length;
                        newFile = fileDao.SaveFile(newFile, convertedFileStream);
                    }
                }
                catch (WebException e)
                {
                    using (var response = e.Response)
                    {
                        var httpResponse = (HttpWebResponse)response;
                        var errorString = String.Format("WebException: {0}", httpResponse.StatusCode);

                        if (httpResponse.StatusCode != HttpStatusCode.NotFound)
                        {
                            using (var responseStream = response.GetResponseStream())
                            {
                                if (responseStream != null)
                                {
                                    using (var readStream = new StreamReader(responseStream))
                                    {
                                        var text = readStream.ReadToEnd();
                                        errorString += String.Format(" Error message: {0}", text);
                                    }
                                }
                            }
                        }

                        throw new Exception(errorString);
                    }
                }

                FilesMessageService.Send(newFile, MessageInitiator.DocsService, MessageAction.FileConverted, newFile.Title);
                FileMarker.MarkAsNew(newFile);

                using (var tagDao = Global.DaoFactory.GetTagDao())
                {
                    var tags = tagDao.GetTags(file.ID, FileEntryType.File, TagType.System).ToList();
                    if (tags.Any())
                    {
                        tags.ForEach(r => r.EntryId = newFile.ID);
                        tagDao.SaveTags(tags);
                    }

                    if (markAsTemplate)
                    {
                        tagDao.SaveTags(Tag.Template(SecurityContext.CurrentAccount.ID, newFile));
                    }
                }

                return newFile;
            }
        }

        private static string GetKey(File f)
        {
            return string.Format("fileConvertation-{0}", f.ID);
        }


        private class FileComparer : IEqualityComparer<File>
        {
            public bool Equals(File x, File y)
            {
                return x != null && y != null && Equals(x.ID, y.ID) && x.Version == y.Version;
            }

            public int GetHashCode(File obj)
            {
                return obj.ID.GetHashCode() + obj.Version.GetHashCode();
            }
        }

        [DataContract(Name = "operation_result", Namespace = "")]
        private class ConvertFileOperationResult : FileOperationResult
        {
            public DateTime StartDateTime { get; set; }
            public DateTime StopDateTime { get; set; }
            public int TenantId { get; set; }
            public IAccount Account { get; set; }
            public bool Delete { get; set; }
            public string Url { get; set; }
            public string Password { get; set; }
        }
    }
}