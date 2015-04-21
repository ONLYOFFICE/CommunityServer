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


using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Api;
using ASC.Web.Files.Services.WCFService;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Projects.Engine
{
    public class FileEngine
    {
        public object GetRoot(int projectId)
        {
            return FilesIntegration.RegisterBunch("projects", "project", projectId.ToString());
        }

        public IEnumerable<object> GetRoots(IEnumerable<int> projectIds)
        {
            return FilesIntegration.RegisterBunchFolders("projects", "project", projectIds.Select(id => id.ToString(CultureInfo.InvariantCulture)));
        }

        public File GetFile(object id, int version)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var file = 0 < version ? dao.GetFile(id, version) : dao.GetFile(id);
                return file;
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
            var docService = ServiceLocator.Current.GetInstance<IFileStorageService>();

            docService.DeleteItems("delete", new ItemList<string> {"folder_" + folderId}, true);
        }

        public void RemoveFile(object id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.DeleteFile(id);
                dao.DeleteFolder(id);
            }
        }

        public Folder SaveFolder(Folder folder)
        {
            using (var dao = FilesIntegration.GetFolderDao())
            {
                folder.ID = dao.SaveFolder(folder);
                return folder;
            }
        }

        internal static Hashtable GetFileListInfoHashtable(IEnumerable<File> uploadedFiles)
        {
            var fileListInfoHashtable = new Hashtable();

            if (uploadedFiles != null)
                foreach (var file in uploadedFiles)
                {
                    var fileInfo = String.Format("{0} ({1})", file.Title, Path.GetExtension(file.Title).ToUpper());
                    fileListInfoHashtable.Add(fileInfo, file.ViewUrl);
                }

            return fileListInfoHashtable;
        }


        public bool CanCreate(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanCreate(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanDelete(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanDelete(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanEdit(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanEdit(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanRead(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanRead(file, SecurityContext.CurrentAccount.ID);
        }


        private static IFileSecurity GetFileSecurity(int projectId)
        {
            return SecurityAdapterProvider.GetFileSecurity(projectId);
        }

        internal FileShare GetFileShare(FileEntry file, int projectId)
        {
            if (!CanRead(file, projectId)) return FileShare.Restrict;
            if (!CanCreate(file, projectId) || !CanEdit(file, projectId)) return FileShare.Read;
            if (!CanDelete(file, projectId)) return FileShare.ReadWrite;

            return FileShare.None;
        }
    }
}