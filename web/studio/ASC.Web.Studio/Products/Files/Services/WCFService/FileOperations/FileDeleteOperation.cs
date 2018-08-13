/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileDeleteOperation : FileOperation
    {
        private object _trashId;
        private readonly bool _ignoreException;
        private readonly bool _immediately;
        private readonly Dictionary<string, string> _headers;

        public override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }


        public FileDeleteOperation(List<object> folders, List<object> files, bool ignoreException = false, bool holdResult = true, bool immediately = false, Dictionary<string, string> headers = null)
            : base(folders, files, holdResult)
        {
            _ignoreException = ignoreException;
            _immediately = immediately;
            _headers = headers;
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
                Status += string.Format("folder_{0}{1}", root.ID, SPLIT_CHAR);
            }

            DeleteFiles(Files);
            DeleteFolders(Folders);
        }

        private void DeleteFolders(IEnumerable<object> folderIds)
        {
            foreach (var folderId in folderIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var folder = FolderDao.GetFolder(folderId);
                object canCalculate = null;
                if (folder == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FolderNotFound;
                }
                else if (folder.FolderType != FolderType.DEFAULT && folder.FolderType != FolderType.BUNCH)
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                }
                else if (!_ignoreException && !FilesSecurity.CanDelete(folder))
                {
                    canCalculate = FolderDao.CanCalculateSubitems(folderId) ? null : folderId;

                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                }
                else
                {
                    canCalculate = FolderDao.CanCalculateSubitems(folderId) ? null : folderId;

                    FileMarker.RemoveMarkAsNewForAll(folder);
                    if (folder.ProviderEntry && folder.ID.Equals(folder.RootFolderId))
                    {
                        if (ProviderDao != null)
                        {
                            ProviderDao.RemoveProviderInfo(folder.ProviderId);
                            FilesMessageService.Send(folder, _headers, MessageAction.ThirdPartyDeleted, folder.ID.ToString(), folder.ProviderKey);
                        }

                        ProcessedFolder(folderId);
                    }
                    else
                    {
                        if (!_immediately && FolderDao.UseTrashForRemove(folder))
                        {
                            var files = FileDao.GetFiles(folder.ID);
                            if (!_ignoreException && files.Exists(FileTracker.IsEditing))
                            {
                                Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFolder;
                            }
                            else
                            {
                                FolderDao.MoveFolder(folder.ID, _trashId);
                                FilesMessageService.Send(folder, _headers, MessageAction.FolderMovedToTrash, folder.Title);

                                ProcessedFolder(folderId);
                            }
                        }
                        else
                        {
                            if (FolderDao.UseRecursiveOperation(folder.ID, null))
                            {
                                DeleteFiles(FileDao.GetFiles(folder.ID));
                                DeleteFolders(FolderDao.GetFolders(folder.ID).Select(f => f.ID).ToList());

                                if (FolderDao.IsEmpty(folder.ID))
                                {
                                    FolderDao.DeleteFolder(folder.ID);
                                    ProcessedFolder(folderId);
                                }
                            }
                            else
                            {
                                FolderDao.DeleteFolder(folder.ID);
                                ProcessedFolder(folderId);
                            }
                        }
                    }
                }
                ProgressStep(canCalculate);
            }
        }

        private void DeleteFiles(IEnumerable<object> fileIds)
        {
            foreach (var fileId in fileIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var file = FileDao.GetFile(fileId);
                if (file == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else if (!_ignoreException && EntryManager.FileLockedForMe(file.ID))
                {
                    Error = FilesCommonResource.ErrorMassage_LockedFile;
                }
                else if (!_ignoreException && FileTracker.IsEditing(file.ID))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFile;
                }
                else if (!_ignoreException && !FilesSecurity.CanDelete(file))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                }
                else
                {
                    FileMarker.RemoveMarkAsNewForAll(file);
                    if (!_immediately && FileDao.UseTrashForRemove(file))
                    {
                        FileDao.MoveFile(file.ID, _trashId);
                        FilesMessageService.Send(file, _headers, MessageAction.FileMovedToTrash, file.Title);
                    }
                    else
                    {
                        try
                        {
                            FileDao.DeleteFile(file.ID);
                        }
                        catch (Exception ex)
                        {
                            Error = ex.Message;
                            Logger.Error(Error, ex);
                        }
                    }
                    ProcessedFile(fileId);
                }
                ProgressStep(fileId: FolderDao.CanCalculateSubitems(fileId) ? null : fileId);
            }
        }
    }
}