/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    internal class FileDeleteOperation : FileOperation
    {
        private object _trashId;
        private readonly bool ignoreException;

        private readonly Dictionary<string, string> httpRequestHeaders;

        protected override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }

        public FileDeleteOperation(Tenant tenant, List<object> folders, List<object> files)
            : base(tenant, folders, files)
        {
            ignoreException = false;
        }

        public FileDeleteOperation(Tenant tenant, List<object> folders, List<object> files, bool ignoreException)
            : base(tenant, folders, files)
        {
            this.ignoreException = ignoreException;
        }

        public FileDeleteOperation(Tenant tenant, List<object> folders, List<object> files, bool ignoreException, Dictionary<string, string> httpRequestHeaders)
            : this(tenant, folders, files, ignoreException)
        {
            this.httpRequestHeaders = httpRequestHeaders;
        }

        protected override void Do()
        {
            _trashId = FolderDao.GetFolderIDTrash(true);
            Folder root = null;
            if (0 < Folders.Count)
            {
                root = FolderDao.GetRootFolder(Folders[0]);
            }
            else if (0 < Files.Count)
            {
                root = FolderDao.GetRootFolderByFile(Files[0]);
            }
            if (root != null)
            {
                Status += string.Format("folder_{0}{1}", root.ID, SplitCharacter);
            }

            DeleteFiles(Files);
            DeleteFolders(Folders);
        }

        private void DeleteFolders(List<object> folderIds)
        {
            if (folderIds.Count == 0) return;

            foreach (var folderId in folderIds)
            {
                if (Canceled) return;

                var folder = FolderDao.GetFolder(folderId);
                if (folder == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FolderNotFound;
                }
                else if (!ignoreException && !FilesSecurity.CanDelete(folder))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                }
                else
                {
                    FileMarker.RemoveMarkAsNewForAll(folder);
                    if (FolderDao.UseTrashForRemove(folder))
                    {
                        var files = FolderDao.GetFiles(folder.ID, true);
                        if (!ignoreException && files.Exists(FileTracker.IsEditing))
                        {
                            Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFolder;
                        }
                        else
                        {
                            FolderDao.MoveFolder(folder.ID, _trashId);
                            FilesMessageService.Send(folder, httpRequestHeaders, MessageAction.FolderMovedToTrash, folder.Title);

                            ProcessedFolder(folderId);
                            ResultedFolder(folderId);
                        }
                    }
                    else
                    {
                        if (FolderDao.UseRecursiveOperation(folder.ID, null))
                        {
                            DeleteFiles(FolderDao.GetFiles(folder.ID, false));
                            DeleteFolders(FolderDao.GetFolders(folder.ID).Select(f => f.ID).ToList());

                            if (FolderDao.GetItemsCount(folder.ID, true) == 0)
                            {
                                FolderDao.DeleteFolder(folder.ID);

                                ProcessedFolder(folderId);
                            }
                        }
                        else
                        {
                            if (folder.ProviderEntry && folder.ID.Equals(folder.RootFolderId))
                            {
                                ProviderDao.RemoveProviderInfo(folder.ProviderId);
                            }
                            else
                            {
                                FolderDao.DeleteFolder(folder.ID);
                            }

                            ProcessedFolder(folderId);
                        }
                    }
                }
                ProgressStep();
            }
        }

        private void DeleteFiles(List<object> fileIds)
        {
            if (fileIds.Count == 0) return;

            foreach (var fileId in fileIds)
            {
                if (Canceled) return;

                var file = FileDao.GetFile(fileId);
                if (file == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else if (!ignoreException && EntryManager.FileLockedForMe(file.ID))
                {
                    Error = FilesCommonResource.ErrorMassage_LockedFile;
                }
                else if (!ignoreException && FileTracker.IsEditing(file.ID))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFile;
                }
                else if (!ignoreException && !FilesSecurity.CanDelete(file))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                }
                else
                {
                    FileMarker.RemoveMarkAsNewForAll(file);
                    if (FileDao.UseTrashForRemove(file))
                    {
                        FileDao.MoveFile(file.ID, _trashId);

                        FilesMessageService.Send(file, httpRequestHeaders, MessageAction.FileMovedToTrash, file.Title);
                        ResultedFile(fileId);
                    }
                    else
                    {
                        try
                        {
                            FileDao.DeleteFile(file.ID);
                            FileDao.DeleteFolder(file.ID);
                        }
                        catch (Exception ex)
                        {
                            Error = ex.Message;

                            Logger.Error(Error, ex);
                        }
                    }
                    ProcessedFile(fileId);
                }
                ProgressStep();
            }
        }
    }
}