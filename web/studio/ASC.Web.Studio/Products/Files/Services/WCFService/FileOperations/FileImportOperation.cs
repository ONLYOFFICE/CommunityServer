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
using System.IO;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    internal class FileImportOperation : FileOperation
    {
        private readonly IDocumentProvider _docProvider;
        private readonly List<DataToImport> _files;
        private readonly object _parentId;
        private readonly bool _overwrite;
        private readonly string _folderName;
        private readonly List<Guid> _markAsNewRecipientIDs;

        private readonly Dictionary<string, string> httpRequestHeaders;


        protected override FileOperationType OperationType
        {
            get { return FileOperationType.Import; }
        }


        public FileImportOperation(Tenant tenant, IDocumentProvider docProvider, List<DataToImport> files, object parentId, bool overwrite, string folderName)
            : base(tenant, null, null)
        {
            Id = Owner.ToString() + OperationType.ToString();
            Source = docProvider.Name;
            _docProvider = docProvider;
            _files = files ?? new List<DataToImport>();
            _parentId = parentId;
            _overwrite = overwrite;
            _folderName = folderName;

            var toFolderObj = Global.DaoFactory.GetFolderDao().GetFolder(_parentId);

            if (toFolderObj != null && toFolderObj.RootFolderType == FolderType.BUNCH)
                _markAsNewRecipientIDs = Global.GetProjectTeam(toFolderObj).ToList();
        }

        public FileImportOperation(Tenant tenant, IDocumentProvider docProvider, List<DataToImport> files, object parentId, bool overwrite, string folderName, Dictionary<string, string> httpRequestHeaders)
            : this(tenant, docProvider, files, parentId, overwrite, folderName)
        {
            this.httpRequestHeaders = httpRequestHeaders;
        }


        protected override double InitProgressStep()
        {
            return _files.Count == 0 ? 100d : 100d / _files.Count;
        }

        protected override void Do()
        {
            if (_files.Count == 0) return;

            var parent = FolderDao.GetFolder(_parentId);
            if (parent == null) throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);
            if (!FilesSecurity.CanCreate(parent)) throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
            if (parent.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ImportToTrash);
            if (parent.ProviderEntry) throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);

            var to =
                FolderDao.GetFolder(_folderName, _parentId)
                ?? FolderDao.SaveFolder(
                    new Folder
                        {
                            FolderType = FolderType.DEFAULT,
                            ParentFolderID = _parentId,
                            Title = _folderName
                        });

            foreach (var f in _files)
            {
                if (Canceled) return;
                try
                {
                    long size;
                    using (var stream = _docProvider.GetDocumentStream(f.ContentLink, out size))
                    {
                        if (stream == null)
                            throw new Exception("Can not import document " + f.ContentLink + ". Empty stream.");

                        if (SetupInfo.MaxUploadSize < size)
                        {
                            throw FileSizeComment.FileSizeException;
                        }

                        var folderId = to.ID;
                        var pos = f.Title.LastIndexOf('/');
                        if (0 < pos)
                        {
                            folderId = GetOrCreateHierarchy(f.Title.Substring(0, pos), to);
                            f.Title = f.Title.Substring(pos + 1);
                        }

                        f.Title = Global.ReplaceInvalidCharsAndTruncate(f.Title);
                        var file = new File
                            {
                                Title = f.Title,
                                FolderID = folderId,
                                ContentLength = size,
                            };

                        var conflict = FileDao.GetFile(file.FolderID, file.Title);
                        if (conflict != null)
                        {
                            if (_overwrite)
                            {
                                if (!FilesSecurity.CanEdit(conflict))
                                {
                                    throw new Exception(FilesCommonResource.ErrorMassage_SecurityException);
                                }
                                if ((conflict.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                                {
                                    throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
                                }
                                if (EntryManager.FileLockedForMe(conflict))
                                {
                                    throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                                }

                                file.ID = conflict.ID;
                                file.Version = conflict.Version + 1;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (size <= 0L)
                        {
                            using (var buffered = stream.GetBuffered())
                            {
                                size = buffered.Length;

                                if (SetupInfo.MaxUploadSize < size)
                                {
                                    throw FileSizeComment.FileSizeException;
                                }

                                file.ContentLength = size;
                                try
                                {
                                    file = FileDao.SaveFile(file, buffered);
                                }
                                catch(Exception error)
                                {
                                    FileDao.DeleteFile(file.ID);
                                    throw error;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                file = FileDao.SaveFile(file, stream);
                                FilesMessageService.Send(file, httpRequestHeaders, MessageAction.FileImported, parent.Title, file.Title, _docProvider.Name);
                            }
                            catch(Exception error)
                            {
                                FileDao.DeleteFile(file.ID);
                                throw error;
                            }
                        }

                        FileMarker.MarkAsNew(file, _markAsNewRecipientIDs);
                    }
                }
                catch(Exception ex)
                {
                    Error = ex.Message;
                    Logger.Error(Error, ex);
                }
                finally
                {
                    ProgressStep();
                }
            }
        }

        private object GetOrCreateHierarchy(string path, Folder root)
        {
            path = path != null ? path.Trim('/') : null;
            if (string.IsNullOrEmpty(path)) return root.ID;

            var pos = path.IndexOf("/");
            var title = 0 < pos ? path.Substring(0, pos) : path;
            path = 0 < pos ? path.Substring(pos + 1) : null;

            title = Global.ReplaceInvalidCharsAndTruncate(title);

            var folder =
                FolderDao.GetFolder(title, root.ID)
                ?? FolderDao.SaveFolder(
                    new Folder
                        {
                            ParentFolderID = root.ID,
                            Title = title,
                            FolderType = FolderType.DEFAULT
                        });

            return GetOrCreateHierarchy(path, folder);
        }
    }
}