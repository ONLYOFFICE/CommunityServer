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
using System.Linq;

using ASC.Core.Tenants;
using ASC.Core;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;

using ASC.Web.Core;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileDeleteOperation : FileOperation
    {
        private object _trashId;
        private readonly bool _ignoreException;
        private readonly bool _immediately;
        private readonly bool _isEmptyTrash;
        private readonly Dictionary<string, string> _headers;

        public override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }


        public FileDeleteOperation(List<object> folders, List<object> files, bool ignoreException = false, bool holdResult = true, bool immediately = false, Dictionary<string, string> headers = null, bool isEmptyTrash = false)
            : base(folders, files, holdResult)
        {
            _ignoreException = ignoreException;
            _immediately = immediately;
            _headers = headers;
            _isEmptyTrash = isEmptyTrash;
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
            if (_isEmptyTrash)
            {
                DeleteFiles(Files);
                DeleteFolders(Folders);
                MessageService.Send(_headers, MessageAction.TrashEmptied);
            }
            else
            {
                DeleteFiles(Files, true);
                DeleteFolders(Folders, true);
            }
        }

        private void DeleteFolders(IEnumerable<object> folderIds, bool isNeedSendActions = false)
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
                            if (isNeedSendActions)
                            {
                                FilesMessageService.Send(folder, _headers, MessageAction.ThirdPartyDeleted, folder.ID.ToString(), folder.ProviderKey);
                            }
                        }

                        ProcessedFolder(folderId);
                    }
                    else
                    {
                        var immediately = _immediately || !FolderDao.UseTrashForRemove(folder);
                        if (immediately && FolderDao.UseRecursiveOperation(folder.ID, null))
                        {
                            DeleteFiles(FileDao.GetFiles(folder.ID));
                            DeleteFolders(FolderDao.GetFolders(folder.ID).Select(f => f.ID).ToList());

                            if (FolderDao.IsEmpty(folder.ID))
                            {
                                FolderDao.DeleteFolder(folder.ID);
                                FilesMessageService.Send(folder, _headers, MessageAction.FolderDeleted, folder.Title);

                                ProcessedFolder(folderId);
                            }
                        }
                        else
                        {
                            var files = FileDao.GetFiles(folder.ID, new OrderBy(SortedByType.AZ, true), FilterType.FilesOnly, false, Guid.Empty, string.Empty, false, true);
                            string tmpError;
                            if (!_ignoreException && WithError(files, true, out tmpError))
                            {
                                Error = tmpError;
                            }
                            else
                            {
                                if (immediately)
                                {
                                    FolderDao.DeleteFolder(folder.ID);
                                    if (isNeedSendActions)
                                    {
                                        FilesMessageService.Send(folder, _headers, MessageAction.FolderDeleted, folder.Title);
                                    }
                                }
                                else
                                {
                                    FolderDao.MoveFolder(folder.ID, _trashId, CancellationToken);
                                    if (isNeedSendActions)
                                    {
                                        FilesMessageService.Send(folder, _headers, MessageAction.FolderMovedToTrash, folder.Title);
                                    }
                                }

                                ProcessedFolder(folderId);
                            }
                        }
                    }
                }
                ProgressStep(canCalculate);
            }
        }

        private void DeleteFiles(IEnumerable<object> fileIds, bool isNeedSendActions = false)
        {
            foreach (var fileId in fileIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var file = FileDao.GetFile(fileId);
                string tmpError;
                if (file == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else if (!_ignoreException && WithError(new[] { file }, false, out tmpError))
                {
                    Error = tmpError;
                }
                else
                {
                    FileMarker.RemoveMarkAsNewForAll(file);
                    if (!_immediately && FileDao.UseTrashForRemove(file))
                    {
                        FileDao.MoveFile(file.ID, _trashId);
                        if (file.RootFolderType == FolderType.COMMON)
                        {
                            CoreContext.TenantManager.SetTenantQuotaRow(
                                new TenantQuotaRow
                                {
                                    Tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                                    Path = string.Format("/{0}/{1}", FileConstant.ModuleId, ""),
                                    Counter = file.ContentLength,
                                    Tag = WebItemManager.DocumentsProductID.ToString(),
                                    UserId = SecurityContext.CurrentAccount.ID
                                },
                                true);
                        }
                        if (file.RootFolderType == FolderType.USER && SecurityContext.CurrentAccount.ID != file.RootFolderCreator)
                        {
                            CoreContext.TenantManager.SetTenantQuotaRow(
                                new TenantQuotaRow
                                {
                                    Tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                                    Path = string.Format("/{0}/{1}", FileConstant.ModuleId, ""),
                                    Counter = -1 * file.ContentLength,
                                    Tag = WebItemManager.DocumentsProductID.ToString(),
                                    UserId = file.RootFolderCreator
                                },
                                true);
                            CoreContext.TenantManager.SetTenantQuotaRow(
                                new TenantQuotaRow
                                {
                                    Tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                                    Path = string.Format("/{0}/{1}", FileConstant.ModuleId, ""),
                                    Counter = file.ContentLength,
                                    Tag = WebItemManager.DocumentsProductID.ToString(),
                                    UserId = SecurityContext.CurrentAccount.ID
                                },
                                true);
                        }

                        if (isNeedSendActions)
                        {
                            FilesMessageService.Send(file, _headers, MessageAction.FileMovedToTrash, file.Title);
                        }
                        if (file.ThumbnailStatus == Thumbnail.Waiting)
                        {
                            file.ThumbnailStatus = Thumbnail.NotRequired;
                            FileDao.SaveThumbnail(file, null);
                        }
                    }
                    else
                    {
                        try
                        {
                            FileDao.DeleteFile(file.ID, file.GetFileQuotaOwner());

                            if (_headers != null)
                            {
                                if (isNeedSendActions)
                                {
                                    FilesMessageService.Send(file, _headers, MessageAction.FileDeleted, file.Title);
                                }
                            }
                            else
                            {
                                FilesMessageService.Send(file, MessageInitiator.AutoCleanUp, MessageAction.FileDeleted, file.Title);
                            }
                        }
                        catch (Exception ex)
                        {
                            Error = ex.Message;
                            Logger.Error(Error, ex);
                        }

                        LinkDao.DeleteAllLink(file.ID);
                    }
                    ProcessedFile(fileId);
                }
                ProgressStep(fileId: FolderDao.CanCalculateSubitems(fileId) ? null : fileId);
            }
        }

        private bool WithError(IEnumerable<File> files, bool folder, out string error)
        {
            error = null;
            foreach (var file in files)
            {
                if (!FilesSecurity.CanDelete(file))
                {
                    error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                    return true;
                }
                if (EntryManager.FileLockedForMe(file.ID))
                {
                    error = FilesCommonResource.ErrorMassage_LockedFile;
                    return true;
                }
                if (FileTracker.IsEditing(file.ID))
                {
                    error = folder ? FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFolder : FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFile;
                    return true;
                }
            }
            return false;
        }

    }
}