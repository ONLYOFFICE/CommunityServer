/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
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
            ForceSave = 6, // not used
            CorruptedForceSave = 7, //not used
        }

        [DebuggerDisplay("{Status} - {Key}")]
        public class TrackerData
        {
            public List<Action> Actions;
            public string ChangesUrl;
            public object History;
            public string Key;
            public MailMergeData MailMerge;
            public TrackerStatus Status;
            public string Url;
            public List<string> Users;
            public string UserData;

            [DebuggerDisplay("{Type} - {UserId}")]
            public class Action
            {
                public string Type;
                public string UserId;
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

        public static string ProcessData(string fileId, TrackerData fileData)
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
            var usersDrop = new List<Guid>();

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
                    usersDrop.Add(userId);
                }
            }

            if (usersDrop.Any())
            {
                if (!DocumentServiceHelper.DropUser(fileData.Key, usersDrop, fileId))
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

        private static string ProcessSave(string fileId, TrackerData fileData)
        {
            Guid userId;
            var comments = new List<string>();
            if (fileData.Status == TrackerStatus.Corrupted)
                comments.Add(FilesCommonResource.ErrorMassage_SaveCorrupted);

            if (fileData.Users == null || fileData.Users.Count == 0 || !Guid.TryParse(fileData.Users[0], out userId))
            {
                userId = FileTracker.GetEditingBy(fileId).FirstOrDefault();
            }

            try
            {
                SecurityContext.AuthenticateMe(userId);
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
            var saved = false;

            FileTracker.Remove(fileId);

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
                }
                catch (Exception ex)
                {
                    Global.Logger.Error(string.Format("DocService save error. Version update. File id: '{0}'. UserId: {1}. DocKey '{2}'", fileId, userId, fileData.Key), ex);
                }
            }
            else
            {
                try
                {
                    file = EntryManager.SaveEditing(fileId, null, DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url), null, string.Empty, string.Join("; ", comments), false);
                    saved = fileData.Status == TrackerStatus.MustSave;
                }
                catch (Exception ex)
                {
                    Global.Logger.Error(string.Format("DocService save error. File id: '{0}'. UserId: {1}. DocKey '{2}'. DownloadUri: {3}", fileId, userId, fileData.Key, fileData.Url), ex);

                    StoringFileAfterError(fileId, userId.ToString(), DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));
                }
            }

            if (file != null)
            {
                var user = CoreContext.UserManager.GetUsers(userId);
                if (user != null)
                    FilesMessageService.Send(file, MessageInitiator.DocsService, MessageAction.UserFileUpdated, user.DisplayUserName(false), file.Title);

                SaveHistory(file, (fileData.History ?? "").ToString(), DocumentServiceConnector.ReplaceDocumentAdress(fileData.ChangesUrl));
            }

            Global.SocketManager.FilesChangeEditors(fileId, true);

            return saved
                       ? "0" //error:0 - saved
                       : "1"; //error:1 - some error
        }

        private static string ProcessMailMerge(string fileId, TrackerData fileData)
        {
            Guid userId;
            if (fileData.Users == null || fileData.Users.Count == 0 || !Guid.TryParse(fileData.Users[0], out userId))
            {
                userId = FileTracker.GetEditingBy(fileId).FirstOrDefault();
            }

            var sended = false;

            try
            {
                SecurityContext.AuthenticateMe(userId);

                if (string.IsNullOrEmpty(fileData.Url)) throw new ArgumentException("emptry url");

                if (fileData.MailMerge == null) throw new ArgumentException("MailMerge is null");

                var message = fileData.MailMerge.Message;
                Stream attach = null;
                switch (fileData.MailMerge.Type)
                {
                    case MailMergeType.AttachDocx:
                    case MailMergeType.AttachPdf:
                        var downloadRequest = (HttpWebRequest) WebRequest.Create(DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));

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
                        var httpWebRequest = (HttpWebRequest) WebRequest.Create(DocumentServiceConnector.ReplaceDocumentAdress(fileData.Url));

                        // hack. http://ubuntuforums.org/showthread.php?t=1841740
                        if (WorkContext.IsMono)
                        {
                            ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                        }

                        using (var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
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
                sended = true;
            }
            catch (Exception ex)
            {
                Global.Logger.Error(
                    string.Format("DocService mailMerge{0} error: userId - {1}, url - {2}",
                                  (fileData.MailMerge == null ? "" : " " + fileData.MailMerge.RecordIndex + "/" + fileData.MailMerge.RecordCount),
                                  userId, fileData.Url),
                    ex);
            }

            if (fileData.MailMerge != null &&
                fileData.MailMerge.RecordIndex == fileData.MailMerge.RecordCount - 1)
            {
                var errorCount = fileData.MailMerge.RecordErrorCount;
                if (!sended) errorCount++;

                NotifyClient.SendMailMergeEnd(userId, fileData.MailMerge.RecordCount, errorCount);
            }

            return sended
                       ? "0" //error:0 - sended
                       : "1"; //error:1 - some error
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

                using (var response = req.GetResponse())
                using (var fileStream = new ResponseStream(response))
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
