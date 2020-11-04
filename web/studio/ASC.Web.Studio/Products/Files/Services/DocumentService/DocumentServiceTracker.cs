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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;
using CommandMethod = ASC.Web.Core.Files.DocumentService.CommandMethod;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.DocumentService
{
    public class DocumentServiceTracker
    {
        #region Class

        public enum TrackerStatus
        {
            NotFound = 0,
            Editing = 1,
            MustSave = 2,
            Corrupted = 3,
            Closed = 4,
            MailMerge = 5,
            ForceSave = 6,
            CorruptedForceSave = 7,
        }

        [DebuggerDisplay("{Status} - {Key}")]
        public class TrackerData
        {
            public List<Action> Actions;
            public string ChangesUrl;
            public ForceSaveInitiator ForceSaveType;
            public object History;
            public string Key;
            public MailMergeData MailMerge;
            public TrackerStatus Status;
            public string Token;
            public string Url;
            public List<string> Users;
            public string UserData;
            public bool Encrypted;

            [DebuggerDisplay("{Type} - {UserId}")]
            public class Action
            {
                public string Type;
                public string UserId;
            }

            public enum ForceSaveInitiator
            {
                Command = 0,
                User = 1,
                Timer = 2
            }
        }

        public enum MailMergeType
        {
            Html = 0,
            AttachDocx = 1,
            AttachPdf = 2,
        }

        [DebuggerDisplay("{From}")]
        public class MailMergeData
        {
            public int RecordCount;
            public int RecordErrorCount;
            public int RecordIndex;

            public string From;
            public string Subject;
            public string To;
            public MailMergeType Type;

            public string Title; //attach
            public string Message; //attach
        }

        [Serializable]
        [DataContract(Name = "response", Namespace = "")]
        public class TrackResponse
        {
            [DataMember(Name = "error")]
            public int Error
            {
                set { }
                get
                {
                    return string.IsNullOrEmpty(Message)
                               ? 0 //error:0 - sended
                               : 1; //error:1 - some error
                }
            }

            [DataMember(Name = "message", EmitDefaultValue = false)]
            public string Message = null;

            public static string Serialize(TrackResponse response)
            {
                using (var ms = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof (TrackResponse));
                    serializer.WriteObject(ms, response);
                    ms.Seek(0, SeekOrigin.Begin);
                    return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                }
            }
        }

        #endregion

        public static string GetCallbackUrl(string fileId)
        {
            var callbackUrl = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath
                                                                    + "?" + FilesLinkUtility.Action + "=track"
                                                                    + "&" + FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(fileId)
                                                                    + "&" + FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(fileId));
            callbackUrl = DocumentServiceConnector.ReplaceCommunityAdress(callbackUrl);
            return callbackUrl;
        }

        public static bool StartTrack(string fileId, string docKeyForTrack)
        {
            var callbackUrl = GetCallbackUrl(fileId);
            return DocumentServiceConnector.Command(CommandMethod.Info, docKeyForTrack, fileId, callbackUrl);
        }

        public static TrackResponse ProcessData(string fileId, TrackerData fileData)
        {
            switch (fileData.Status)
            {
                case TrackerStatus.NotFound:
                case TrackerStatus.Closed:
                    FileTracker.Remove(fileId);
                    Global.SocketManager.FilesChangeEditors(fileId, true);
                    break;

                case TrackerStatus.Editing:
                    ProcessEdit(fileId, fileData);
                    break;

                case TrackerStatus.MustSave:
                case TrackerStatus.Corrupted:
                case TrackerStatus.ForceSave:
                case TrackerStatus.CorruptedForceSave:
                    return ProcessSave(fileId, fileData);

                case TrackerStatus.MailMerge:
                    return ProcessMailMerge(fileId, fileData);
            }
            return null;
        }

        private static void ProcessEdit(string fileId, TrackerData fileData)
        {
            if (ThirdPartySelector.GetAppByFileId(fileId) != null)
            {
                return;
            }

            var users = FileTracker.GetEditingBy(fileId);
            var usersDrop = new List<string>();

            string docKey;
            var app = ThirdPartySelector.GetAppByFileId(fileId);
            if (app == null)
            {
                File fileStable;
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    fileStable = fileDao.GetFileStable(fileId);
                }

                docKey = DocumentServiceHelper.GetDocKey(fileStable);
            }
            else
            {
                docKey = fileData.Key;
            }

            if (!fileData.Key.Equals(docKey))
            {
                Global.Logger.InfoFormat("DocService editing file {0} ({1}) with key {2} for {3}", fileId, docKey, fileData.Key, string.Join(", ", fileData.Users));
                usersDrop = fileData.Users;
            }
            else
            {
                foreach (var user in fileData.Users)
                {
                    Guid userId;
                    if (!Guid.TryParse(user, out userId))
                    {
                        Global.Logger.Error("DocService userId is not Guid: " + user);
                        continue;
                    }
                    users.Remove(userId);

                    try
                    {
                        var doc = FileShareLink.CreateKey(fileId);
                        EntryManager.TrackEditing(fileId, userId, userId, doc);
                    }
                    catch (Exception e)
                    {
                        Global.Logger.DebugFormat("Drop command: fileId '{0}' docKey '{1}' for user {2} : {3}", fileId, fileData.Key, user, e.Message);
                        usersDrop.Add(userId.ToString());
                    }
                }
            }

            if (usersDrop.Any())
            {
                if (!DocumentServiceHelper.DropUser(fileData.Key, usersDrop.ToArray(), fileId))
                {
                    Global.Logger.Error("DocService drop failed for users " + string.Join(",", usersDrop));
                }
            }

            foreach (var removeUserId in users)
            {
                FileTracker.Remove(fileId, userId: removeUserId);
            }
            Global.SocketManager.FilesChangeEditors(fileId);
        }

        private static TrackResponse ProcessSave(string fileId, TrackerData fileData)
        {
            Guid userId;
            var comments = new List<string>();
            if (fileData.Status == TrackerStatus.Corrupted
                || fileData.Status == TrackerStatus.CorruptedForceSave)
                comments.Add(FilesCommonResource.ErrorMassage_SaveCorrupted);

            var forcesave = fileData.Status == TrackerStatus.ForceSave || fileData.Status == TrackerStatus.CorruptedForceSave;

            if (fileData.Users == null || fileData.Users.Count == 0 || !Guid.TryParse(fileData.Users[0], out userId))
            {
                userId = FileTracker.GetEditingBy(fileId).FirstOrDefault();
            }

            var app = ThirdPartySelector.GetAppByFileId(fileId);
            if (app == null)
            {
                File fileStable;
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    fileStable = fileDao.GetFileStable(fileId);
                }

                var docKey = DocumentServiceHelper.GetDocKey(fileStable);
                if (!string.IsNullOrEmpty(docKey) && !fileData.Key.Equals(docKey))
                {
                    Global.Logger.ErrorFormat("DocService saving file {0} ({1}) with key {2}", fileId, docKey, fileData.Key);

                    StoringFileAfterError(fileId, userId.ToString(), DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));
                    return new TrackResponse { Message = "Expected key " + docKey };
                }
            }

            UserInfo user = null;
            try
            {
                SecurityContext.AuthenticateMe(userId);

                user = CoreContext.UserManager.GetUsers(userId);
                var culture = string.IsNullOrEmpty(user.CultureName) ? CoreContext.TenantManager.GetCurrentTenant().GetCulture() : CultureInfo.GetCultureInfo(user.CultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                Global.Logger.Info("DocService save error: anonymous author - " + userId, ex);
                if (!userId.Equals(ASC.Core.Configuration.Constants.Guest.ID))
                {
                    comments.Add(FilesCommonResource.ErrorMassage_SaveAnonymous);
                }
            }

            File file = null;
            var saveMessage = "Not saved";

            if (string.IsNullOrEmpty(fileData.Url))
            {
                try
                {
                    comments.Add(FilesCommonResource.ErrorMassage_SaveUrlLost);

                    file = EntryManager.CompleteVersionFile(fileId, 0, false, false);

                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    {
                        fileDao.UpdateComment(file.ID, file.Version, string.Join("; ", comments));
                    }

                    file = null;
                    Global.Logger.ErrorFormat("DocService save error. Empty url. File id: '{0}'. UserId: {1}. DocKey '{2}'", fileId, userId, fileData.Key);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error(string.Format("DocService save error. Version update. File id: '{0}'. UserId: {1}. DocKey '{2}'", fileId, userId, fileData.Key), ex);
                }
            }
            else
            {
                if (fileData.Encrypted)
                {
                    comments.Add(FilesCommonResource.CommentEditEncrypt);
                }

                var forcesaveType = ForcesaveType.None;
                if (forcesave)
                {
                    switch (fileData.ForceSaveType)
                    {
                        case TrackerData.ForceSaveInitiator.Command:
                            forcesaveType = ForcesaveType.Command;
                            break;
                        case TrackerData.ForceSaveInitiator.Timer:
                            forcesaveType = ForcesaveType.Timer;
                            break;
                        case TrackerData.ForceSaveInitiator.User:
                            forcesaveType = ForcesaveType.User;
                            break;
                    }
                    comments.Add(fileData.ForceSaveType == TrackerData.ForceSaveInitiator.User
                                     ? FilesCommonResource.CommentForcesave
                                     : FilesCommonResource.CommentAutosave);
                }

                try
                {
                    file = EntryManager.SaveEditing(fileId, null, DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url), null, string.Empty, string.Join("; ", comments), false, fileData.Encrypted, forcesaveType);
                    saveMessage = fileData.Status == TrackerStatus.MustSave || fileData.Status == TrackerStatus.ForceSave ? null : "Status " + fileData.Status;
                }
                catch (Exception ex)
                {
                    Global.Logger.Error(string.Format("DocService save error. File id: '{0}'. UserId: {1}. DocKey '{2}'. DownloadUri: {3}", fileId, userId, fileData.Key, fileData.Url), ex);
                    saveMessage = ex.Message;

                    StoringFileAfterError(fileId, userId.ToString(), DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));
                }
            }

            if (!forcesave)
                FileTracker.Remove(fileId);

            if (file != null)
            {
                if (user != null)
                    FilesMessageService.Send(file, MessageInitiator.DocsService, MessageAction.UserFileUpdated, user.DisplayUserName(false), file.Title);

                if (!forcesave)
                    SaveHistory(file, (fileData.History ?? "").ToString(), DocumentServiceConnector.ReplaceDocumentAdress(fileData.ChangesUrl));
            }

            Global.SocketManager.FilesChangeEditors(fileId, !forcesave);

            var result = new TrackResponse { Message = saveMessage };
            return result;
        }

        private static TrackResponse ProcessMailMerge(string fileId, TrackerData fileData)
        {
            Guid userId;
            if (fileData.Users == null || fileData.Users.Count == 0 || !Guid.TryParse(fileData.Users[0], out userId))
            {
                userId = FileTracker.GetEditingBy(fileId).FirstOrDefault();
            }

            string saveMessage;

            try
            {
                SecurityContext.AuthenticateMe(userId);

                var user = CoreContext.UserManager.GetUsers(userId);
                var culture = string.IsNullOrEmpty(user.CultureName) ? CoreContext.TenantManager.GetCurrentTenant().GetCulture() : CultureInfo.GetCultureInfo(user.CultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                if (string.IsNullOrEmpty(fileData.Url)) throw new ArgumentException("emptry url");

                if (fileData.MailMerge == null) throw new ArgumentException("MailMerge is null");

                var message = fileData.MailMerge.Message;
                Stream attach = null;
                switch (fileData.MailMerge.Type)
                {
                    case MailMergeType.AttachDocx:
                    case MailMergeType.AttachPdf:
                        var downloadRequest = (HttpWebRequest)WebRequest.Create(DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));

                        // hack. http://ubuntuforums.org/showthread.php?t=1841740
                        if (WorkContext.IsMono)
                        {
                            ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                        }

                        using (var downloadStream = new ResponseStream(downloadRequest.GetResponse()))
                        {
                            const int bufferSize = 2048;
                            var buffer = new byte[bufferSize];
                            int readed;
                            attach = new MemoryStream();
                            while ((readed = downloadStream.Read(buffer, 0, bufferSize)) > 0)
                            {
                                attach.Write(buffer, 0, readed);
                            }
                            attach.Position = 0;
                        }

                        if (string.IsNullOrEmpty(fileData.MailMerge.Title))
                        {
                            fileData.MailMerge.Title = "Attach";
                        }

                        var attachExt = fileData.MailMerge.Type == MailMergeType.AttachDocx ? ".docx" : ".pdf";
                        var curExt = FileUtility.GetFileExtension(fileData.MailMerge.Title);
                        if (curExt != attachExt)
                        {
                            fileData.MailMerge.Title += attachExt;
                        }

                        break;

                    case MailMergeType.Html:
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));

                        // hack. http://ubuntuforums.org/showthread.php?t=1841740
                        if (WorkContext.IsMono)
                        {
                            ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                        }

                        using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                        using (var stream = httpWebResponse.GetResponseStream())
                            if (stream != null)
                                using (var reader = new StreamReader(stream, Encoding.GetEncoding(Encoding.UTF8.WebName)))
                                {
                                    message = reader.ReadToEnd();
                                }
                        break;
                }

                using (var mailMergeTask =
                    new MailMergeTask
                        {
                            From = fileData.MailMerge.From,
                            Subject = fileData.MailMerge.Subject,
                            To = fileData.MailMerge.To,
                            Message = message,
                            AttachTitle = fileData.MailMerge.Title,
                            Attach = attach
                        })
                {
                    var response = mailMergeTask.Run();
                    Global.Logger.InfoFormat("DocService mailMerge {0}/{1} send: {2}",
                                             fileData.MailMerge.RecordIndex + 1, fileData.MailMerge.RecordCount, response);
                }
                saveMessage = null;
            }
            catch (Exception ex)
            {
                Global.Logger.Error(
                    string.Format("DocService mailMerge{0} error: userId - {1}, url - {2}",
                                  (fileData.MailMerge == null ? "" : " " + fileData.MailMerge.RecordIndex + "/" + fileData.MailMerge.RecordCount),
                                  userId, fileData.Url),
                    ex);
                saveMessage = ex.Message;
            }

            if (fileData.MailMerge != null &&
                fileData.MailMerge.RecordIndex == fileData.MailMerge.RecordCount - 1)
            {
                var errorCount = fileData.MailMerge.RecordErrorCount;
                if (!string.IsNullOrEmpty(saveMessage)) errorCount++;

                NotifyClient.SendMailMergeEnd(userId, fileData.MailMerge.RecordCount, errorCount);
            }

            return new TrackResponse { Message = saveMessage };
        }


        private static void StoringFileAfterError(string fileId, string userId, string downloadUri)
        {
            try
            {
                var fileName = Global.ReplaceInvalidCharsAndTruncate(fileId + FileUtility.GetFileExtension(downloadUri));
                var path = string.Format(@"save_crash\{0}\{1}_{2}",
                                         DateTime.UtcNow.ToString("yyyy_MM_dd"),
                                         userId,
                                         fileName);

                var store = Global.GetStore();
                var req = (HttpWebRequest)WebRequest.Create(downloadUri);

                // hack. http://ubuntuforums.org/showthread.php?t=1841740
                if (WorkContext.IsMono)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                }

                using (var fileStream = new ResponseStream(req.GetResponse()))
                {
                    store.Save(FileConstant.StorageDomainTmp, path, fileStream);
                }
                Global.Logger.DebugFormat("DocService storing to {0}", path);
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocService Error on save file to temp store", ex);
            }
        }

        private static void SaveHistory(File file, string changes, string differenceUrl)
        {
            if (file == null) return;
            if (file.ProviderEntry) return;
            if (string.IsNullOrEmpty(changes) || string.IsNullOrEmpty(differenceUrl)) return;

            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var req = (HttpWebRequest)WebRequest.Create(differenceUrl);

                    // hack. http://ubuntuforums.org/showthread.php?t=1841740
                    if (WorkContext.IsMono)
                    {
                        ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                    }

                    using (var differenceStream = new ResponseStream(req.GetResponse()))
                    {
                        fileDao.SaveEditHistory(file, changes, differenceStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocService save history error", ex);
            }
        }
    }
}
