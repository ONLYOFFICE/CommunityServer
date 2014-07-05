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
            Drop
        }

        public enum TrackerStatus
        {
            NotFound = 0,
            Editing = 1,
            MustSave = 2,
            Corrupted = 3,
            Closed = 4,
        }

        public class TrackerData
        {
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
                Global.Logger.Error("DocService return null");
                throw new ArgumentException("DocService return null", "trackDataString");
            }

            var data = JObject.Parse(trackDataString);
            if (data == null)
            {
                Global.Logger.Error("DocService response is incorrect");
                throw new ArgumentException("DocService response is incorrect", "trackDataString");
            }

            var fileData = data.ToObject<TrackerData>();
            var userId = Guid.Empty;
            switch (fileData.Status)
            {
                case TrackerStatus.NotFound:
                case TrackerStatus.Closed:
                    FileTracker.Remove(fileId);
                    break;

                case TrackerStatus.Editing:
                    var users = FileTracker.GetEditingBy(fileId);

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
                            Global.Logger.DebugFormat("DocService drop fileId '{0}' docKey '{1}' for user {2} : {3}", fileId, fileData.Key, user, e.Message);
                            if (!Drop(fileData.Key, user, fileId))
                            {
                                Global.Logger.Error("DocService drop failed for user " + user);
                            }
                        }
                    }

                    foreach (var removeUserId in users)
                    {
                        FileTracker.Remove(fileId, userId: removeUserId);
                    }
                    break;

                case TrackerStatus.MustSave:
                case TrackerStatus.Corrupted:
                    if (fileData.Users != null && fileData.Users.Count > 0)
                    {
                        Guid.TryParse(fileData.Users[0], out userId);
                    }

                    SecurityContext.AuthenticateMe(userId);

                    try
                    {
                        var file = EntryManager.SaveEditing(fileId, -1, userId, fileData.Url, isNew, string.Empty,
                                                            fileData.Status == TrackerStatus.Corrupted ? FilesCommonResource.ErrorMassage_SaveCorrupted : String.Empty,
                                                            false);
                        var user = CoreContext.UserManager.GetUsers(userId);
                        if (file != null && user != null)
                        {
                            FilesMessageService.Send(file, MessageInitiator.DocsService, MessageAction.UserFileUpdated, user.DisplayUserName(false), file.Title);
                        }
                    }
                    catch (Exception ex)
                    {
                        Global.Logger.Error(string.Format("DocService save error. File id: '{0}'. UserId: {1}. DocKey '{2}'. DownloadUri: {3}", fileId, userId, fileData.Key, fileData.Url), ex);

                        StoringFileAfterError(fileId, userId.ToString(), fileData.Url);
                    }

                    FileTracker.Remove(fileId);

                    break;
            }
        }

        public static bool Drop(string docKeyForTrack, string userId, string fileId = null)
        {
            return Command(TrackMethod.Drop, docKeyForTrack, fileId, null, userId);
        }

        private static bool Command(TrackMethod method, string docKeyForTrack, string fileId = null, string callbackUrl = null, string userId = null)
        {
            Global.Logger.DebugFormat("DocService command {0} fileId '{1}' docKey '{2}'", method, fileId, docKeyForTrack);
            try
            {
                var response = DocumentServiceConnector.CommandRequest(method.ToString().ToLower(), docKeyForTrack, callbackUrl, userId);
                Global.Logger.DebugFormat("DocService command response: '{0}'", response);

                var jResponse = JObject.Parse(response);

                var result = (CommandResultTypes)jResponse.Value<int>("error");
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
                var fileName = fileId + FileUtility.GetFileExtension(downloadUri);
                var path = string.Format(@"save_crash\{0}\{1}_{2}",
                                         DateTime.UtcNow.ToString("yyyy_MM_dd"),
                                         userId,
                                         fileName);

                var store = Global.GetStore();
                var req = (HttpWebRequest)WebRequest.Create(downloadUri);
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
    }
}