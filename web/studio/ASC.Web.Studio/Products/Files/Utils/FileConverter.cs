/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using System.Web.Configuration;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
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
        #region Members

        public static readonly int TimerConvertPeriod;

        private static readonly bool EnableAsUpload = bool.TrueString.Equals(WebConfigurationManager.AppSettings["files.docservice.convert-enable"] ?? "true", StringComparison.InvariantCultureIgnoreCase);

        private static readonly object LockerStatus = new object();
        private static readonly IDictionary<File, ConvertFileOperationResult> ConversionFileStatus = new Dictionary<File, ConvertFileOperationResult>(new FileComparer());

        private static readonly object LockerTimer = new object();
        private static Timer _timer;

        #endregion

        #region Methods

        static FileConverter()
        {
            int timer;
            Int32.TryParse(WebConfigurationManager.AppSettings["files.docservice.time-interval"], out timer);
            TimerConvertPeriod = timer > 0 ? timer : 3000;
        }

        public static bool EnableAsUploaded
        {
            get { return EnableAsUpload && TenantExtra.GetTenantQuota().DocsEdition; }
        }

        public static bool MustConvert(File file)
        {
            if (file == null) return false;
            var fileExtension = FileUtility.GetFileExtension(file.Title);

            return FileUtility.ExtsMustConvert.Contains(fileExtension);
        }

        public static bool EnableConvert(File file, string toExtension)
        {
            if (file == null) return false;
            var fileExtension = FileUtility.GetFileExtension(file.Title);

            if (FileUtility.InternalExtension.Values.Contains(toExtension)) return true;

            return FileUtility.ExtsConvertible.Keys.Contains(fileExtension) && FileUtility.ExtsConvertible[fileExtension].Contains(toExtension);
        }

        public static bool IsConverted(File file)
        {
            if (!String.IsNullOrEmpty(file.ConvertedType)) return true;

            return FileUtility.InternalExtension.Values.Contains(FileUtility.GetFileExtension(file.Title));
        }

        public static Stream Exec(File file)
        {
            return Exec(file, FileUtility.GetInternalExtension(file.Title));
        }

        public static Stream Exec(File file, string toExtension)
        {
            if (file.ContentLength > SetupInfo.AvailableFileSize) throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeConvert, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));

            var fileExtension = file.ConvertedExtension;

            var requiredFormat = fileExtension.Trim('.').Equals(toExtension.Trim('.'), StringComparison.OrdinalIgnoreCase);
            if (!EnableConvert(file, toExtension)
                || requiredFormat)
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    return fileDao.GetFileStream(file);
                }
            }

            var fileUri = PathProvider.GetFileStreamUrl(file);

            var docKey = DocumentServiceHelper.GetDocKey(file.ID, file.Version, file.ModifiedOn);

            string convertUri;
            DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, false, out convertUri);

            return new ResponseStream(WebRequest.Create(convertUri).GetResponse());
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

                    if (file == null) throw new ArgumentNullException("file", FilesCommonResource.ErrorMassage_FileNotFound);
                    if (!readLink) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                    toFolderId = Global.FolderMy;
                }

                if (Global.EnableUploadFilter && !FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(file.Title))) throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);

                var toFolder = folderDao.GetFolder(toFolderId);
                if (toFolder == null) throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);
                if (!Global.GetFilesSecurity().CanCreate(toFolder)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);

                File newFile = null;
                try
                {
                    var fileUri = PathProvider.GetFileStreamUrl(file);
                    var fileExtension = file.ConvertedExtension;
                    var toExtension = FileUtility.GetInternalExtension(file.Title);
                    var docKey = DocumentServiceHelper.GetDocKey(file.ID, file.Version, file.ModifiedOn);
                    string convertUri;
                    DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, false, out convertUri);

                    newFile = new File
                        {
                            FolderID = toFolder.ID,
                            Title = FileUtility.ReplaceFileExtension(file.Title, toExtension)
                        };

                    var req = (HttpWebRequest)WebRequest.Create(convertUri);
                    using (var editedFileStream = new ResponseStream(req.GetResponse()))
                    {
                        newFile.ContentLength = editedFileStream.Length;

                        newFile = fileDao.SaveFile(newFile, editedFileStream);
                    }

                    FileMarker.MarkAsNew(newFile);
                }
                catch
                {
                    if (newFile != null) fileDao.DeleteFile(newFile.ID);
                    throw;
                }
                return newFile;
            }
        }

        public static void ExecAsync(File file, bool deleteAfter)
        {
            if (!MustConvert(file))
                throw new ArgumentException(FilesCommonResource.ErrorMassage_NotSupportedFormat);

            if (IsConverted(file))
                return;

            FileMarker.RemoveMarkAsNew(file);

            lock (LockerStatus)
            {
                if (ConversionFileStatus.ContainsKey(file)) throw new Exception(FilesCommonResource.ErrorMassage_Reconverting);

                ConversionFileStatus.Add(file,
                                         new ConvertFileOperationResult
                                             {
                                                 Source = "{" + String.Format("\"id\":\"{0}\", \"version\":\"{1}\"", file.ID, file.Version) + "}",
                                                 OperationType = FileOperationType.Convert,
                                                 Error = String.Empty,
                                                 Progress = 0,
                                                 Result = String.Empty,
                                                 Processed = "",
                                                 Id = String.Empty,
                                                 TenantId = TenantProvider.CurrentTenantID,
                                                 Account = SecurityContext.CurrentAccount,
                                                 Delete = deleteAfter
                                             });
            }
            lock (LockerTimer)
            {
                if (_timer == null)
                    _timer = new Timer(CheckConvertFilesStatus, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(TimerConvertPeriod));
            }
        }

        public static bool IsConverting(File file)
        {
            if (!MustConvert(file)) return false;
            if (!String.IsNullOrEmpty(file.ConvertedType)) return false;

            lock (LockerStatus)
            {
                if (!ConversionFileStatus.ContainsKey(file)) return false;
                var opResult = ConversionFileStatus[file];
                if (!String.IsNullOrEmpty(opResult.Processed)) return false;
                if (opResult.Progress == 100) return false;
                if (!String.IsNullOrEmpty(opResult.Error)) return false;
            }

            return true;
        }

        public static IEnumerable<FileOperationResult> GetStatus(IEnumerable<KeyValuePair<File, bool>> filesPair)
        {
            lock (LockerStatus)
            {
                var fileSecurity = Global.GetFilesSecurity();

                var resultList = new List<FileOperationResult>();
                foreach (var fileItem in filesPair)
                {
                    var file = fileItem.Key;
                    var curFile = ConversionFileStatus.Keys.FirstOrDefault(k => k.Equals(file));
                    if (curFile == null) continue;
                    if (!fileItem.Value && !fileSecurity.CanRead(curFile)) continue;

                    var res = ConversionFileStatus[curFile];
                    resultList.Add(new FileOperationResult(res));
                    if (!string.IsNullOrEmpty(res.Processed))
                        ConversionFileStatus.Remove(curFile);
                }

                return resultList;
            }
        }

        private static String FileJsonSerializer(File file)
        {
            return FileJsonSerializer(file, false, string.Empty);
        }

        private static String FileJsonSerializer(File file, bool removeoriginal, string folderTitle)
        {
            EntryManager.SetFileStatus(file);

            return
                string.Format("{{ \"file\": {{ \"id\": \"{0}\", \"title\": \"{1}\", \"version\": \"{2}\", \"fileXml\": \"{3}\"}}, \"removeOriginal\": {4}, \"folderId\": \"{5}\", \"folderTitle\": \"{6}\" }}",
                              file.ID,
                              file.Title,
                              file.Version,
                              File.Serialize(file).Replace('"', '\''),
                              removeoriginal.ToString().ToLower(),
                              file.FolderID,
                              folderTitle);
        }

        private static void CheckConvertFilesStatus(Object obj)
        {
            lock (LockerTimer)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            try
            {
                List<File> filesIsConverting;

                lock (LockerStatus)
                {
                    ConversionFileStatus.Where(x => ((!String.IsNullOrEmpty(x.Value.Processed) &&
                                                      DateTime.Now.Subtract(x.Value.StopDateTime) > TimeSpan.FromMinutes(30))))
                                        .ToList().ForEach(x => ConversionFileStatus.Remove(x));

                    filesIsConverting = ConversionFileStatus.Where(x => String.IsNullOrEmpty(x.Value.Processed)).Select(x => x.Key).ToList();
                }

                if (filesIsConverting.Count == 0)
                {
                    lock (LockerTimer)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }
                    return;
                }

                foreach (var file in filesIsConverting)
                {
                    var fileUri = file.ID.ToString();
                    string convetedFileUrl;
                    int operationResultProgress;
                    object folderId;
                    var currentFolder = false;

                    try
                    {
                        int tenantId;
                        IAccount account;

                        lock (LockerStatus)
                        {
                            var operationResult = ConversionFileStatus[file];
                            if (operationResult == null) continue;
                            tenantId = operationResult.TenantId;
                            account = operationResult.Account;
                        }

                        CoreContext.TenantManager.SetCurrentTenant(tenantId);
                        SecurityContext.AuthenticateMe(account);

                        var user = CoreContext.UserManager.GetUsers(account.ID);
                        var culture = string.IsNullOrEmpty(user.CultureName)
                                          ? CoreContext.TenantManager.GetCurrentTenant().GetCulture()
                                          : CultureInfo.GetCultureInfo(user.CultureName);
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var fileSecurity = Global.GetFilesSecurity();
                        if (!fileSecurity.CanRead(file)
                            && file.RootFolderType != FolderType.BUNCH) //No rights in CRM after upload before attach
                            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                        if (file.ContentLength > SetupInfo.AvailableFileSize) throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeConvert, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));

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
                        var fileExtension = file.ConvertedType ?? FileUtility.GetFileExtension(file.Title);
                        var docKey = DocumentServiceHelper.GetDocKey(file.ID, file.Version, file.ModifiedOn);

                        operationResultProgress = DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, true, out convetedFileUrl);
                    }
                    catch (Exception exception)
                    {
                        lock (LockerStatus)
                        {
                            var operationResult = ConversionFileStatus[file];
                            if (operationResult != null)
                            {
                                if (operationResult.Delete)
                                {
                                    ConversionFileStatus.Remove(file);
                                }
                                else
                                {
                                    operationResult.Result = FileJsonSerializer(file);
                                    operationResult.Processed = "1";
                                    operationResult.StopDateTime = DateTime.Now;
                                    operationResult.Error = exception.Message;
                                }
                            }
                        }

                        var strExc = exception.Message + " in file " + fileUri;
                        Global.Logger.Error(strExc, exception);

                        continue;
                    }

                    if (operationResultProgress < 100)
                    {
                        lock (LockerStatus)
                        {
                            var operationResult = ConversionFileStatus[file];
                            if (operationResult != null)
                                operationResult.Progress = operationResultProgress;
                        }
                        continue;
                    }

                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    using (var folderDao = Global.DaoFactory.GetFolderDao())
                    {
                        var newFileTitle = FileUtility.ReplaceFileExtension(file.Title, FileUtility.GetInternalExtension(file.Title));
                        var isUpdate = false;

                        File newFile = null;

                        if (FilesSettings.UpdateIfExist && (!currentFolder || !file.ProviderEntry))
                        {
                            newFile = fileDao.GetFile(folderId, newFileTitle);
                            if (newFile != null
                                && Global.GetFilesSecurity().CanEdit(newFile)
                                && !EntryManager.FileLockedForMe(newFile.ID)
                                && (newFile.FileStatus & FileStatus.IsEditing) != FileStatus.IsEditing)
                            {
                                newFile.Version++;
                                isUpdate = true;
                            }
                            else
                            {
                                newFile = null;
                            }
                        }

                        if (newFile == null)
                            newFile = new File { FolderID = folderId };

                        newFile.Title = newFileTitle;
                        newFile.ConvertedType = FileUtility.GetInternalExtension(file.Title);

                        var operationResultError = string.Empty;
                        try
                        {
                            var req = (HttpWebRequest)WebRequest.Create(convetedFileUrl);

                            using (var convertedFileStream = new ResponseStream(req.GetResponse()))
                            {
                                newFile.ContentLength = convertedFileStream.Length;
                                newFile.Comment = string.Empty;
                                newFile = fileDao.SaveFile(newFile, convertedFileStream);
                            }

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
                            if (!isUpdate)
                                fileDao.DeleteFile(newFile.ID);

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

                                operationResultError = errorString;

                                Global.Logger.Error(errorString + "  ConvertUrl : " + convetedFileUrl + "    fromUrl : " + fileUri, e);

                                throw new Exception(errorString);
                            }
                        }
                        finally
                        {
                            var fileSecurity = Global.GetFilesSecurity();
                            var removeOriginal = !FilesSettings.StoreOriginalFiles
                                                 && fileSecurity.CanDelete(file)
                                                 && currentFolder
                                                 && !EntryManager.FileLockedForMe(file.ID);

                            var folderTitle = folderDao.GetFolder(newFile.FolderID).Title;

                            lock (LockerStatus)
                            {
                                var operationResult = ConversionFileStatus[file];

                                if (operationResult.Delete)
                                {
                                    ConversionFileStatus.Remove(file);
                                }
                                else
                                {
                                    operationResult.Result = FileJsonSerializer(newFile, removeOriginal, folderTitle);
                                    operationResult.Processed = "1";
                                    operationResult.StopDateTime = DateTime.Now;
                                    operationResult.Progress = operationResultProgress;
                                    if (!string.IsNullOrEmpty(operationResultError)) operationResult.Error = operationResultError;
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

                lock (LockerTimer)
                {
                    _timer.Change(0, TimerConvertPeriod);
                }
            }
            catch (Exception exception)
            {
                Global.Logger.Error(exception.Message, exception);
                lock (LockerTimer)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        #endregion

        #region Nested Classes

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

        [DataContract(Name = "operation_result")]
        private class ConvertFileOperationResult : FileOperationResult
        {
            public DateTime StopDateTime { get; set; }
            public int TenantId { get; set; }
            public IAccount Account { get; set; }
            public bool Delete { get; set; }
        }

        #endregion
    }
}