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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using ASC.Core.Users;

namespace ASC.Web.Files.Services.DocumentService
{
    public class DocumentServiceTracker
    {
        #region Class

        public enum TrackMethod
        {
            Info,
            Drop,
            Saved,
        }

        public enum TrackerStatus
        {
            NotFound = 0,
            Editing = 1,
            MustSave = 2,
            Corrupted = 3,
            Closed = 4,
        }

        [DebuggerDisplay("{Status}")]
        public class TrackerData
        {
            public string ChangesUrl;
            public string ChangesHistory;
            public string Key;
            public TrackerStatus Status;
            public string Url;
            public List<string> Users;
        }

        public enum CommandResultTypes
        {
            NoError = 0,
            DocumentIdError = 1,
            ParseError = 2,
            CommandError = 3
        };

        #endregion


        public static bool StartTrack(string fileId, string docKeyForTrack, bool isNew)
        {
            var callbackUrl = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath
                                                                    + "?" + FilesLinkUtility.Action + "=track"
                                                                    + "&vkey=" + HttpUtility.UrlEncode(Signature.Create(fileId, StudioKeySettings.GetSKey()))
                                                                    + "&new=" + isNew.ToString().ToLower());

            return Command(TrackMethod.Info, docKeyForTrack, fileId, callbackUrl);
        }

        public static void ProcessData(string fileId, bool isNew, string trackDataString)
        {
            if (string.IsNullOrEmpty(trackDataString))
            {
                throw new ArgumentException("DocService return null");
            }

            var data = JObject.Parse(trackDataString);
            if (data == null)
            {
                throw new ArgumentException("DocService response is incorrect");
            }

            var fileData = data.ToObject<TrackerData>();
            Guid userId;
            switch (fileData.Status)
            {
                case TrackerStatus.NotFound:
                case TrackerStatus.Closed:
                    FileTracker.Remove(fileId);
                    break;

                case TrackerStatus.Editing:
                    if (ThirdPartySelector.GetAppByFileId(fileId) != null)
                    {
                        break;
                    }

                    var users = FileTracker.GetEditingBy(fileId);
                    var usersDrop = new List<string>();

                    foreach (var user in fileData.Users)
                    {
                        if (!Guid.TryParse(user, out userId))
                        {
                            Global.Logger.Error("DocService userId is not Guid: " + user);
                            continue;
                        }
                        users.Remove(userId);

                        try
                        {
                            var shareLinkKey = FileShareLink.CreateKey(fileId);
                            EntryManager.TrackEditing(fileId, userId, userId, isNew, shareLinkKey);
                        }
                        catch (Exception e)
                        {
                            Global.Logger.DebugFormat("Drop command: fileId '{0}' docKey '{1}' for user {2} : {3}", fileId, fileData.Key, user, e.Message);
                            usersDrop.Add(user);
                        }
                    }

                    if (usersDrop.Any())
                    {
                        var dropString = "[\"" + string.Join("\",\"", usersDrop) + "\"]";
                        if (!Drop(fileData.Key, dropString, fileId))
                        {
                            Global.Logger.Error("DocService drop failed for users " + dropString);
                        }
                    }

                    foreach (var removeUserId in users)
                    {
                        FileTracker.Remove(fileId, userId: removeUserId);
                    }
                    break;

                case TrackerStatus.MustSave:
                case TrackerStatus.Corrupted:
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
                        Global.Logger.Warn("DocService save error: anonymous author - " + userId, ex);
                        if (!userId.Equals(ASC.Core.Configuration.Constants.Guest.ID))
                        {
                            comments.Add(FilesCommonResource.ErrorMassage_SaveAnonymous);
                        }
                    }

                    File file = null;
                    var saved = false;

                    if (string.IsNullOrEmpty(fileData.Url))
                    {
                        try
                        {
                            comments.Add(FilesCommonResource.ErrorMassage_SaveUrlLost);

                            FileTracker.Remove(fileId);

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
                            file = EntryManager.SaveEditing(fileId, -1, userId, fileData.Url, isNew, string.Empty, string.Join("; ", comments), false);
                            saved = fileData.Status == TrackerStatus.MustSave;
                        }
                        catch (Exception ex)
                        {
                            Global.Logger.Error(string.Format("DocService save error. File id: '{0}'. UserId: {1}. DocKey '{2}'. DownloadUri: {3}", fileId, userId, fileData.Key, fileData.Url), ex);

                            StoringFileAfterError(fileId, userId.ToString(), fileData.Url);
                        }
                    }

                    if (file != null)
                    {
                        var user = CoreContext.UserManager.GetUsers(userId);
                        if (user != null)
                            FilesMessageService.Send(file, MessageInitiator.DocsService, MessageAction.UserFileUpdated, user.DisplayUserName(false), file.Title);

                        SaveHistory(file, fileData.ChangesHistory, fileData.ChangesUrl);
                    }

                    FileTracker.Remove(fileId);

                    Command(TrackMethod.Saved, fileData.Key, fileId, null, userId.ToString(), saved ? "1" : "0");
                    break;
            }
        }

        public static bool Drop(string docKeyForTrack, string users, string fileId = null)
        {
            return Command(TrackMethod.Drop, docKeyForTrack, fileId, null, users);
        }

        private static bool Command(TrackMethod method, string docKeyForTrack, string fileId = null, string callbackUrl = null, string users = null, string status = null)
        {
            Global.Logger.DebugFormat("DocService command {0} fileId '{1}' docKey '{2}' callbackUrl '{3}' users '{4}' status '{5}'", method, fileId, docKeyForTrack, callbackUrl, users, status);
            try
            {
                var response = DocumentServiceConnector.CommandRequest(method.ToString().ToLower(CultureInfo.InvariantCulture), docKeyForTrack, callbackUrl, users, status);
                Global.Logger.DebugFormat("DocService command response: '{0}'", response);

                var jResponse = JObject.Parse(response);

                var result = (CommandResultTypes) jResponse.Value<int>("error");
                return result == CommandResultTypes.NoError;
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService command error", e);
                return false;
            }
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
                var req = (HttpWebRequest) WebRequest.Create(downloadUri);

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
                    var req = (HttpWebRequest) WebRequest.Create(differenceUrl);

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