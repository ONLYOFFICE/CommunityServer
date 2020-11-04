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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using ASC.Files.Core;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Projects.Engine
{
    public class FileEngine
    {
        private const string Module = "projects";
        private const string Bunch = "project";
        private readonly SecurityAdapterProvider securityAdapterProvider = new SecurityAdapterProvider();

        public object GetRoot(int projectId)
        {
            return FilesIntegration.RegisterBunch(Module, Bunch, projectId.ToString(CultureInfo.InvariantCulture));
        }

        public IEnumerable<object> GetRoots(IEnumerable<int> projectIds)
        {
            return FilesIntegration.RegisterBunchFolders(Module, Bunch, projectIds.Select(id => id.ToString(CultureInfo.InvariantCulture)));
        }

        public File GetFile(object id, int version = 1)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var file = 0 < version ? dao.GetFile(id, version) : dao.GetFile(id);
                return file;
            }
        }

        public IEnumerable<File> GetFiles(object[] id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                return dao.GetFiles(id);
            }
        }

        public File SaveFile(File file, Stream stream)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                return dao.SaveFile(file, stream);
            }
        }

        public void RemoveRoot(int projectId)
        {
            var folderId = GetRoot(projectId);

            //requet long operation
            try
            {
                Global.FileStorageService.DeleteItems("delete", new ItemList<string> {"folder_" + folderId}, true);
            }
            catch (Exception)
            {
                
            }
        }

        public void MoveToTrash(object id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.MoveFile(id, Global.FolderTrash);
            }
        }

        public static void RegisterFileSecurityProvider()
        {
            FilesIntegration.RegisterFileSecurityProvider(Module, Bunch, new SecurityAdapterProvider());
        }

        internal static List<Tuple<string, string>> GetFileListInfoHashtable(IEnumerable<File> uploadedFiles)
        {
            if (uploadedFiles == null) return new List<Tuple<string, string>>();

            var fileListInfoHashtable = new List<Tuple<string, string>>();

            foreach (var file in uploadedFiles)
            {
                var fileInfo = string.Format("{0} ({1})", file.Title, Path.GetExtension(file.Title).ToUpper());
                fileListInfoHashtable.Add(new Tuple<string, string>(fileInfo, file.DownloadUrl));
            }

            return fileListInfoHashtable;
        }

        internal FileShare GetFileShare(FileEntry file, int projectId)
        {
            var fileSecurity = securityAdapterProvider.GetFileSecurity(projectId);
            var currentUserId = SecurityContext.CurrentAccount.ID;
            if (!fileSecurity.CanRead(file, currentUserId)) return FileShare.Restrict;
            if (!fileSecurity.CanCreate(file, currentUserId) || !fileSecurity.CanEdit(file, currentUserId)) return FileShare.Read;
            if (!fileSecurity.CanDelete(file, currentUserId)) return FileShare.ReadWrite;

            return FileShare.None;
        }
    }
}