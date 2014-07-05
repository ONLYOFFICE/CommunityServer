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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Studio.Helpers;

using Microsoft.Practices.ServiceLocation;
using log4net;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Projects.Engine
{
    public class FileEngine
    {
        private readonly IDataStore projectsStore;

        public FileEngine(String dbId, int tenantID)
        {
            projectsStore = StorageFactory.GetStorage(tenantID.ToString(CultureInfo.InvariantCulture), dbId); 
        }

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

            docService.DeleteItems(new ItemList<string> {"folder_" + folderId}, true);
        }

        public void RemoveFile(object id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.DeleteFile(id);
                dao.DeleteFolder(id);
                try
                {
                    projectsStore.Delete(GetThumbPath(id));
                }
                catch
                {
                }
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

        internal void SetThumbUrls(List<File> files)
        {
            files.ForEach(f =>
                              {
                                  if (f != null && FileUtility.GetFileTypeByFileName(f.Title) == FileType.Image)
                                  {
                                      f.ThumbnailURL = projectsStore.GetUri(GetThumbPath(f.ID)).ToString();
                                  }
                              });
        }

        internal void GenerateImageThumb(File file)
        {
            if (file == null || FileUtility.GetFileTypeByFileName(file.Title) != FileType.Image) return;

            try
            {
                using (var filedao = FilesIntegration.GetFileDao())
                {
                    using (var stream = filedao.GetFileStream(file))
                    {
                        var ii = new ImageInfo();
                        ImageHelper.GenerateThumbnail(stream, GetThumbPath(file.ID), ref ii, 128, 96, projectsStore);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web.Projects").Error(ex);
            }
        }

        private static string GetThumbPath(object fileId)
        {
            var s = fileId.ToString().PadRight(6, '0');
            return "thumbs/" + s.Substring(0, 2) + "/" + s.Substring(2, 2) + "/" + s.Substring(4) + "/" + fileId.ToString() + ".jpg";
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


        private IFileSecurity GetFileSecurity(int projectId)
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