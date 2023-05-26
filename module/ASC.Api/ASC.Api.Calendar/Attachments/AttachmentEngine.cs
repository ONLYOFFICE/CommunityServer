/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.IO;
using System.Linq;

using ASC.Common.Logging;
using ASC.Files.Core;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Utils;

using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;

namespace ASC.Api.Calendar.Attachments
{
    public class AttachmentEngine
    {
        private static readonly ILog Logger = LogManager.GetLogger("ASC.Api.Calendar.AttachmentEngine");
        public const string Module = "calendar";
        public const string Bunch = "event";
        public const string Temp = "temp";

        public static object GetTmpFolderId()
        {
            return FilesIntegration.RegisterBunch(Module, Bunch, Temp);
        }

        public static object GetFolderId(string eventId)
        {
            return FilesIntegration.RegisterBunch(Module, Bunch, eventId);
        }

        public static IEnumerable<object> GetFolderIds(IEnumerable<string> eventIds)
        {
            return FilesIntegration.RegisterBunchFolders(Module, Bunch, eventIds);
        }

        public static IEnumerable<File> GetFiles(string eventId)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var folderId = GetFolderId(eventId);
                var fileIds = dao.GetFiles(folderId);
                return dao.GetFiles(fileIds);
            }
        }

        public static File GetFile(object fileId)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                return dao.GetFile(fileId);
            }
        }

        public static File SaveFile(File file, Stream stream)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                return dao.SaveFile(file, stream);
            }
        }

        public static void MoveFile(object fileId, object folderId)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.MoveFile(fileId, folderId);
            }
        }

        public static void DeleteFile(object fileId)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.DeleteFile(fileId);
            }
        }

        public static void DeleteFolder(string eventId)
        {
            var folderId = GetFolderId(eventId);
            DeleteFolders(new[] { folderId });
        }

        public static void DeleteFolders(IEnumerable<string> eventIds)
        {
            var folderIds = GetFolderIds(eventIds);
            DeleteFolders(folderIds);
        }

        private static void DeleteFolders(IEnumerable<object> folderIds)
        {
            try
            {
                var items = new ItemList<string>(folderIds.Select(folderId => "folder_" + folderId));
                Global.FileStorageService.DeleteItems("delete", items, true, false, true);
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR: " + ex.Message);
            }
        }

        public static void RegisterFileSecurityProvider()
        {
            FilesIntegration.RegisterFileSecurityProvider(Module, Bunch, new SecurityAdapterProvider());
        }

        public static Uri ShareFileAndGetUri(string fileId)
        {
            var file = GetFile(fileId);

            var objectId = "file_" + file.ID;
            var sharedInfo = Global.FileStorageService.GetSharedInfo(new ItemList<string> { objectId }).Find(r => r.SubjectId == FileConstant.ShareLinkId);
            if (sharedInfo == null || sharedInfo.Share == FileShare.Restrict || sharedInfo.Share == FileShare.None)
            {
                var list = new ItemList<AceWrapper>
                    {
                        new AceWrapper
                            {
                                SubjectId = FileConstant.ShareLinkId,
                                SubjectGroup = true,
                                Share = FileShare.Read
                            }
                    };
                var aceCollection = new AceCollection
                {
                    Entries = new ItemList<string> { objectId },
                    Aces = list
                };
                Global.FileStorageService.SetAceObject(aceCollection, false);
            }

            var uri = new Uri(GetUriString(file));
            return uri;
        }

        public static Uri GetUri(object fileId)
        {
            var file = GetFile(fileId);
            var uriString = GetUriString(file);
            return new Uri(uriString);
        }

        public static string GetUriString(File file)
        {
            return FileShareLink.GetLink(file);
        }
    }
}