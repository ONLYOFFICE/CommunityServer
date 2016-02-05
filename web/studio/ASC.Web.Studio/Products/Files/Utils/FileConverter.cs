/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
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
            get { return FileUtility.ExtsMustConvert.Any() && !string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl) && TenantExtra.GetTenantQuota().DocsEdition; }
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
            var docKey = DocumentServiceHelper.GetDocKey(file.ID, file.Version, file.ModifiedOn);
            string convertUri;
            DocumentServiceConnector.GetConvertedUri(fileUri, file.ConvertedExtension, toExtension, docKey, false, out convertUri);

            if (WorkContext.IsMono && ServicePointManager.ServerCertificateValidationCallback == null)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, c, n, p) => true; //HACK: http://ubuntuforums.org/showthread.php?t=1841740
            }
            return new ResponseStream(((HttpWebRequest)WebRequest.Create(convertUri)).GetResponse());
        }

        public static File ExecDuplicate(File file, string shareLinkKey)
        {
            var toFolderId = file.FolderID;

            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                if (!Global.GetFilesSecurity().CanRead(file))
                {
                    var readLink = FileShareLink.Check(shareLinkKey, true, fileDao, out file);
                    if (file == null)
                    {
                        throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);
                    }
                    if (!readLink)
                    {
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                    }
                    toFolderId = Global.FolderMy;
                }
                if (Global.EnableUploadFilter && !FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(file.Title)))
                {
                    throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);
                }
                var toFolder = folderDao.GetFolder(toFolderId);
                if (toFolder == null)
                {
                    throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);
                }
                if (!Global.GetFilesSecurity().CanCreate(toFolder))
                {
                    throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
                }

                var fileUri = PathProvider.GetFileStreamUrl(file);
                var fileExtension = file.ConvertedExtension;
                var toExtension = FileUtility.GetInternalExtension(file.Title);
                var docKey = DocumentServiceHelper.GetDocKey(file.ID, file.Version, file.ModifiedOn);

                string convertUri;
                DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, false, out convertUri);

                var newFile = new File
                {
                    FolderID = toFolder.ID,
                    Title = FileUtility.ReplaceFileExtension(file.Title, toExtension),
                    Comment = string.Format(FilesCommonResource.CommentConvert, file.Title),
                };

                var req = (HttpWebRequest)WebRequest.Create(convertUri);

                // hack. http://ubuntuforums.org/showthread.php?t=1841740
                if (WorkContext.IsMono)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                }

                using (var editedFileStream = new ResponseStream(req.GetResponse()))
                {
                    newFile.ContentLength = editedFileStream.Length;
                    newFile = fileDao.SaveFile(newFile, editedFileStream);
                }

                FileMarker.MarkAsNew(newFile);
                return newFile;
            }
        }

        public static void ExecAsync(File file, bool deleteAfter)
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
                    throw new Exception(FilesCommonResource.ErrorMassage_Reconverting);
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
                    Delete = deleteAfter
                };
                conversionQueue.Add(file, queueResult);
                cache.Insert(GetKey(file), queueResult, TimeSpan.FromMinutes(30));

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

        private static String FileJsonSerializer(File file)
        {
            return FileJsonSerializer(file, false, string.Empty);
        }

        private static String FileJsonSerializer(File file, bool removeoriginal, string folderTitle)
        {
            EntryManager.SetFileStatus(file);
            return string.Format("{{ \"file\": {{ \"id\": \"{0}\", \"title\": \"{1}\", \"version\": \"{2}\", \"fileXml\": \"{3}\"}}, \"removeOriginal\": {4}, \"folderId\": \"{5}\", \"folderTitle\": \"{6}\" }}",
                file.ID, file.Title, file.Version,
                File.Serialize(file).Replace('"', '\''), removeoriginal.ToString().ToLower(),
                file.FolderID, folderTitle);
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

                        conversionQueue.Where(x => !string.IsNullOrEmpty(x.Value.Processed) &&
                                                    (x.Value.Progress == 100 && DateTime.Now - x.Value.StopDateTime > TimeSpan.FromMinutes(1) ||
                                                    DateTime.Now - x.Value.StopDateTime > TimeSpan.FromMinutes(30)))
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

                    foreach (var file in filesIsConverting)
                    {
                        var fileUri = file.ID.ToString();
                        string convertedFileUrl;
                        int operationResultProgress;
                        object folderId;
                        var currentFolder = false;

                        try
                        {
                            int tenantId;
                            IAccount account;

                            lock (locker)
                            {
                                if (!conversionQueue.Keys.Contains(file)) continue;

                                var operationResult = conversionQueue[file];
                                if (!string.IsNullOrEmpty(operationResult.Processed)) continue;

                                operationResult.Processed = "1";
                                tenantId = operationResult.TenantId;
                                account = operationResult.Account;

                                cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(30));
                            }

                            CoreContext.TenantManager.SetCurrentTenant(tenantId);
                            SecurityContext.AuthenticateMe(account);

                            var user = CoreContext.UserManager.GetUsers(account.ID);
                            var culture = string.IsNullOrEmpty(user.CultureName) ? CoreContext.TenantManager.GetCurrentTenant().GetCulture() : CultureInfo.GetCultureInfo(user.CultureName);
                            Thread.CurrentThread.CurrentCulture = culture;
                            Thread.CurrentThread.CurrentUICulture = culture;

                            var fileSecurity = Global.GetFilesSecurity();
                            if (!fileSecurity.CanRead(file) && file.RootFolderType != FolderType.BUNCH)
                            {
                                //No rights in CRM after upload before attach
                                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                            }
                            if (file.ContentLength > SetupInfo.AvailableFileSize)
                            {
                                throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeConvert, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));
                            }

                            folderId = Global.FolderMy;
                            using (var folderDao = Global.DaoFactory.GetFolderDao())
                            {
                                var parent = folderDao.GetFolder(file.FolderID);
                                if (parent != null
                                    && fileSecurity.CanCreate(parent))
                                {
                                    folderId = parent.ID;
                                    currentFolder = true;
                                }
                            }
                            if (Equals(folderId, 0)) throw new SecurityException(FilesCommonResource.ErrorMassage_FolderNotFound);

                            fileUri = PathProvider.GetFileStreamUrl(file);

                            var toExtension = FileUtility.GetInternalExtension(file.Title);
                            var fileExtension = file.ConvertedExtension;
                            var docKey = DocumentServiceHelper.GetDocKey(file.ID, file.Version, file.ModifiedOn);

                            operationResultProgress = DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, true, out convertedFileUrl);
                            operationResultProgress = Math.Min(operationResultProgress, 100);
                        }
                        catch (Exception exception)
                        {
                            Global.Logger.ErrorFormat("Error convert {0} with url {1}: {2}", file.ID, fileUri, exception);
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
                                        operationResult.Result = FileJsonSerializer(file);
                                        operationResult.Progress = 100;
                                        operationResult.StopDateTime = DateTime.Now;
                                        operationResult.Error = exception.Message;
                                        cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(30));
                                    }
                                }
                            }
                            continue;
                        }

                        if (operationResultProgress < 100)
                        {
                            lock (locker)
                            {
                                if (conversionQueue.Keys.Contains(file))
                                {
                                    var operationResult = conversionQueue[file];
                                    operationResult.Processed = "";
                                    operationResult.Progress = operationResultProgress;
                                    cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(30));
                                }
                            }
                            continue;
                        }

                        using (var fileDao = Global.DaoFactory.GetFileDao())
                        using (var folderDao = Global.DaoFactory.GetFolderDao())
                        {
                            var newFileTitle = FileUtility.ReplaceFileExtension(file.Title, FileUtility.GetInternalExtension(file.Title));

                            File newFile = null;
                            if (FilesSettings.UpdateIfExist && (!currentFolder || !file.ProviderEntry))
                            {
                                newFile = fileDao.GetFile(folderId, newFileTitle);
                                if (newFile != null && Global.GetFilesSecurity().CanEdit(newFile) && !EntryManager.FileLockedForMe(newFile.ID) && !FileTracker.IsEditing(newFile.ID))
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
                            newFile.Title = newFileTitle;
                            newFile.ConvertedType = null;
                            newFile.Comment = string.Format(FilesCommonResource.CommentConvert, file.Title);

                            var operationResultError = string.Empty;
                            try
                            {
                                var req = (HttpWebRequest)WebRequest.Create(convertedFileUrl);

                                if (WorkContext.IsMono && ServicePointManager.ServerCertificateValidationCallback == null)
                                {
                                    ServicePointManager.ServerCertificateValidationCallback += (s, c, n, p) => true; //HACK: http://ubuntuforums.org/showthread.php?t=1841740
                                }

                                using (var convertedFileStream = new ResponseStream(req.GetResponse()))
                                {
                                    newFile.ContentLength = convertedFileStream.Length;
                                    newFile = fileDao.SaveFile(newFile, convertedFileStream);
                                }

                                FilesMessageService.Send(newFile, MessageInitiator.DocsService, MessageAction.FileConverted, newFile.Title);
                                FileMarker.MarkAsNew(newFile);

                                using (var tagDao = Global.DaoFactory.GetTagDao())
                                {
                                    var tags = tagDao.GetTags(file.ID, FileEntryType.File, TagType.System).ToList();
                                    if (tags.Any())
                                    {
                                        tags.ForEach(r => r.EntryId = newFile.ID);
                                        tagDao.SaveTags(tags.ToArray());
                                    }
                                }

                                operationResultProgress = 100;
                            }
                            catch (WebException e)
                            {
                                using (var response = e.Response)
                                {
                                    var httpResponse = (HttpWebResponse)response;
                                    var errorString = String.Format("Error code: {0}", httpResponse.StatusCode);

                                    if (httpResponse.StatusCode != HttpStatusCode.NotFound)
                                    {
                                        using (var data = response.GetResponseStream())
                                        {
                                            var text = new StreamReader(data).ReadToEnd();
                                            errorString += String.Format(" Error message: {0}", text);
                                        }
                                    }

                                    operationResultProgress = 100;
                                    operationResultError = errorString;

                                    Global.Logger.ErrorFormat("{0} ConvertUrl: {1} fromUrl: {2}: {3}", errorString, convertedFileUrl, fileUri, e);
                                    throw new Exception(errorString);
                                }
                            }
                            finally
                            {
                                var fileSecurity = Global.GetFilesSecurity();
                                var removeOriginal = !FilesSettings.StoreOriginalFiles && fileSecurity.CanDelete(file) && currentFolder && !EntryManager.FileLockedForMe(file.ID);
                                var folderTitle = folderDao.GetFolder(newFile.FolderID).Title;

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
                                            operationResult.Result = FileJsonSerializer(newFile, removeOriginal, folderTitle);
                                            operationResult.StopDateTime = DateTime.Now;
                                            operationResult.Processed = "1";
                                            operationResult.Progress = operationResultProgress;
                                            if (!string.IsNullOrEmpty(operationResultError)) { operationResult.Error = operationResultError; }
                                            cache.Insert(GetKey(file), operationResult, TimeSpan.FromMinutes(30));
                                        }
                                    }
                                }

                                if (removeOriginal)
                                {
                                    FileMarker.RemoveMarkAsNewForAll(file);
                                    fileDao.DeleteFile(file.ID);
                                }
                            }
                        }
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

        private static string GetKey(File f)
        {
            return string.Format("fileConvertation-{0}-{1}", f.ID, f.Version);
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
            public DateTime StopDateTime { get; set; }
            public int TenantId { get; set; }
            public IAccount Account { get; set; }
            public bool Delete { get; set; }
        }
    }
}