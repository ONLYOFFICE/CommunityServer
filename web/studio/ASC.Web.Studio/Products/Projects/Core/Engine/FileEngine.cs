/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using ASC.Files.Core;
using ASC.Files.Core.Security;
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

        public static void RegisterFileSecurityProvider()
        {
            FilesIntegration.RegisterFileSecurityProvider(Module, Bunch, new SecurityAdapterProvider());
        }

        internal static Hashtable GetFileListInfoHashtable(IEnumerable<File> uploadedFiles)
        {
            if (uploadedFiles == null) return new Hashtable();

            var fileListInfoHashtable = new Hashtable();

            foreach (var file in uploadedFiles)
            {
                var fileInfo = String.Format("{0} ({1})", file.Title, Path.GetExtension(file.Title).ToUpper());
                fileListInfoHashtable.Add(fileInfo, file.DownloadUrl);
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