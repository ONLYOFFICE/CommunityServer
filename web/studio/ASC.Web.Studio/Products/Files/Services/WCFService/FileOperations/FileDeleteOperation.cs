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
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileDeleteOperation : FileOperation
    {
        private object trashId;
        private readonly bool ignoreException;
        private readonly Dictionary<string, string> headers;

        public override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }

        
        public FileDeleteOperation(List<object> folders, List<object> files, bool ignoreException = false, Dictionary<string, string> headers = null)
            : base(folders, files)
        {
            this.ignoreException = ignoreException;
            this.headers = headers;
        }

        
        protected override void Do()
        {
            trashId = FolderDao.GetFolderIDTrash(true);
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
                Status += string.Format("folder_{0}{1}", root.ID, SPLIT_CHAR);
            }

            DeleteFiles(Files);
            DeleteFolders(Folders);
        }

        private void DeleteFolders(List<object> folderIds)
        {
            foreach (var folderId in folderIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

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
                        var files = FileDao.GetFiles(folder.ID, true);
                        if (!ignoreException && files.Exists(FileTracker.IsEditing))
                        {
                            Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFolder;
                        }
                        else
                        {
                            FolderDao.MoveFolder(folder.ID, trashId);
                            FilesMessageService.Send(folder, headers, MessageAction.FolderMovedToTrash, folder.Title);

                            ProcessedFolder(folderId);
                        }
                    }
                    else
                    {
                        if (FolderDao.UseRecursiveOperation(folder.ID, null))
                        {
                            DeleteFiles(FileDao.GetFiles(folder.ID, false));
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
            foreach (var fileId in fileIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

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
                        FileDao.MoveFile(file.ID, trashId);
                        FilesMessageService.Send(file, headers, MessageAction.FileMovedToTrash, file.Title);
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