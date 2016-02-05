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
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileMoveCopyOperation : FileOperation
    {
        private readonly string toFolderId;
        private readonly bool copy;
        private readonly FileConflictResolveType resolveType;
        private readonly List<Guid> recipients;
        private readonly List<FileEntry> needToMark = new List<FileEntry>();

        private readonly Dictionary<string, string> headers;

        public override FileOperationType OperationType
        {
            get { return copy ? FileOperationType.Copy : FileOperationType.Move; }
        }

        public FileMoveCopyOperation(List<object> folders, List<object> files, string toFolderId, bool copy, FileConflictResolveType resolveType, Dictionary<string, string> headers)
            : base(folders, files)
        {
            this.toFolderId = toFolderId;
            this.copy = copy;
            this.resolveType = resolveType;
            this.headers = headers;

            var toFolder = Global.DaoFactory.GetFolderDao().GetFolder(toFolderId);
            if (toFolder != null && toFolder.RootFolderType == FolderType.BUNCH)
            {
                recipients = Global.GetProjectTeam(toFolder).ToList();
            }
        }

        protected override void Do()
        {
            Status += string.Format("folder_{0}{1}", toFolderId, SPLIT_CHAR);

            //TODO: check on each iteration?
            var toFolder = FolderDao.GetFolder(toFolderId);
            if (toFolder == null) return;
            if (!FilesSecurity.CanCreate(toFolder)) throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);

            if (FolderDao.GetParentFolders(toFolder.ID).Any(parent => Folders.Contains(parent.ID.ToString())))
            {
                Error = FilesCommonResource.ErrorMassage_FolderCopyError;
                return;
            }

            if (copy)
            {
                Folder rootFrom = null;
                if (0 < Folders.Count) rootFrom = FolderDao.GetRootFolder(Folders[0]);
                if (0 < Files.Count) rootFrom = FolderDao.GetRootFolderByFile(Files[0]);
                if (rootFrom != null && rootFrom.FolderType == FolderType.TRASH) throw new InvalidOperationException("Can not copy from Trash.");
                if (toFolder.RootFolderType == FolderType.TRASH) throw new InvalidOperationException("Can not copy to Trash.");
            }

            MoveOrCopyFolders(Folders, toFolder, copy);
            MoveOrCopyFiles(Files, toFolder, copy);

            needToMark.Distinct().ToList().ForEach(x => FileMarker.MarkAsNew(x, recipients));
        }

        private void MoveOrCopyFolders(ICollection folderIds, Folder toFolder, bool copy)
        {
            if (folderIds.Count == 0) return;

            var toFolderId = toFolder.ID;
            var isToFolder = Equals(toFolderId.ToString(), this.toFolderId);

            foreach (var folderId in folderIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var folder = FolderDao.GetFolder(folderId);
                if (folder == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FolderNotFound;
                }
                else if (!FilesSecurity.CanRead(folder))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFolder;
                }
                else if (!Equals((folder.ParentFolderID ?? string.Empty).ToString(), toFolderId.ToString()) || resolveType == FileConflictResolveType.Duplicate)
                {
                    try
                    {
                        //if destination folder contains folder with same name then merge folders
                        var conflictFolder = FolderDao.GetFolder(folder.Title, toFolderId);

                        if (copy || conflictFolder != null)
                        {
                            Folder newFolder;
                            if (conflictFolder != null)
                            {
                                newFolder = conflictFolder;

                                if (isToFolder)
                                    needToMark.Add(conflictFolder);
                            }
                            else
                            {
                                newFolder = FolderDao.CopyFolder(folder.ID, toFolderId);
                                FilesMessageService.Send(folder, toFolder, headers, MessageAction.FolderCopied, folder.Title, toFolder.Title);

                                if (isToFolder)
                                    needToMark.Add(newFolder);

                                if (ProcessedFolder(folderId))
                                {
                                    Status += string.Format("folder_{0}{1}", newFolder.ID, SPLIT_CHAR);
                                }
                            }

                            if (FolderDao.UseRecursiveOperation(folder.ID, toFolderId))
                            {
                                MoveOrCopyFiles(FileDao.GetFiles(folder.ID, false), newFolder, copy);
                                MoveOrCopyFolders(FolderDao.GetFolders(folder.ID).Select(f => f.ID).ToList(), newFolder, copy);

                                if (!copy)
                                {
                                    if (FolderDao.GetItemsCount(folder.ID, true) == 0 && FilesSecurity.CanDelete(folder))
                                    {
                                        FolderDao.DeleteFolder(folder.ID);
                                        if (ProcessedFolder(folderId))
                                        {
                                            Status += string.Format("folder_{0}{1}", newFolder.ID, SPLIT_CHAR);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (conflictFolder != null)
                                {
                                    object newFolderId;
                                    if (copy)
                                    {
                                        newFolder = FolderDao.CopyFolder(folder.ID, toFolderId);
                                        newFolderId = newFolder.ID;
                                        FilesMessageService.Send(folder, toFolder, headers, MessageAction.FolderCopiedWithOverwriting, folder.Title, toFolder.Title);

                                        if (isToFolder)
                                            needToMark.Add(newFolder);
                                    }
                                    else
                                    {
                                        newFolderId = FolderDao.MoveFolder(folder.ID, toFolderId);
                                        FilesMessageService.Send(folder, toFolder, headers, MessageAction.FolderMovedWithOverwriting, folder.Title, toFolder.Title);

                                        if (isToFolder)
                                            needToMark.Add(FolderDao.GetFolder(newFolderId));
                                    }

                                    if (ProcessedFolder(folderId))
                                    {
                                        Status += string.Format("folder_{0}{1}", newFolderId, SPLIT_CHAR);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (FilesSecurity.CanDelete(folder))
                            {
                                FileMarker.RemoveMarkAsNewForAll(folder);

                                var newFolderId = FolderDao.MoveFolder(folder.ID, toFolderId);
                                FilesMessageService.Send(folder, toFolder, headers, MessageAction.FolderMoved, folder.Title, toFolder.Title);

                                if (isToFolder)
                                    needToMark.Add(FolderDao.GetFolder(newFolderId));

                                if (ProcessedFolder(folderId))
                                {
                                    Status += string.Format("folder_{0}{1}", newFolderId, SPLIT_CHAR);
                                }
                            }
                            else
                            {
                                Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;

                        Logger.Error(Error, ex);
                    }
                }
                ProgressStep();
            }
        }

        private void MoveOrCopyFiles(ICollection fileIds, Folder toFolder, bool copy)
        {
            if (fileIds.Count == 0) return;

            var toFolderId = toFolder.ID;
            foreach (var fileId in fileIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var file = FileDao.GetFile(fileId);
                if (file == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else if (!FilesSecurity.CanRead(file))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFile;
                }
                else if (Global.EnableUploadFilter
                         && !FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(file.Title)))
                {
                    Error = FilesCommonResource.ErrorMassage_NotSupportedFormat;
                }
                else
                {
                    var parentFolder = FolderDao.GetFolder(file.FolderID);
                    try
                    {
                        var conflict = resolveType == FileConflictResolveType.Duplicate
                                           ? null
                                           : FileDao.GetFile(toFolderId, file.Title);
                        if (conflict != null && !FilesSecurity.CanEdit(conflict))
                        {
                            Error = FilesCommonResource.ErrorMassage_SecurityException;
                        }
                        else if (conflict != null && EntryManager.FileLockedForMe(conflict.ID))
                        {
                            Error = FilesCommonResource.ErrorMassage_LockedFile;
                        }
                        else if (conflict == null)
                        {
                            if (copy)
                            {
                                File newFile = null;
                                try
                                {
                                    newFile = FileDao.CopyFile(file.ID, toFolderId); //Stream copy will occur inside dao
                                    FilesMessageService.Send(file, toFolder, headers, MessageAction.FileCopied, file.Title, parentFolder.Title, toFolder.Title);

                                    if (Equals(newFile.FolderID.ToString(), this.toFolderId))
                                    {
                                        needToMark.Add(newFile);
                                    }

                                    if (ProcessedFile(fileId))
                                    {
                                        Status += string.Format("file_{0}{1}", newFile.ID, SPLIT_CHAR);
                                    }
                                }
                                catch
                                {
                                    if (newFile != null) FileDao.DeleteFile(newFile.ID);
                                    throw;
                                }
                            }
                            else
                            {
                                if (EntryManager.FileLockedForMe(file.ID))
                                {
                                    Error = FilesCommonResource.ErrorMassage_LockedFile;
                                }
                                else if (FileTracker.IsEditing(file.ID))
                                {
                                    Error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                                }
                                else if (FilesSecurity.CanDelete(file))
                                {
                                    FileMarker.RemoveMarkAsNewForAll(file);

                                    var newFileId = FileDao.MoveFile(file.ID, toFolderId);
                                    FilesMessageService.Send(file, toFolder, headers, MessageAction.FileMoved, file.Title, parentFolder.Title, toFolder.Title);

                                    if (Equals(toFolderId.ToString(), this.toFolderId))
                                    {
                                        needToMark.Add(FileDao.GetFile(newFileId));
                                    }

                                    if (ProcessedFile(fileId))
                                    {
                                        Status += string.Format("file_{0}{1}", newFileId, SPLIT_CHAR);
                                    }
                                }
                                else
                                {
                                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                                }
                            }
                        }
                        else
                        {
                            if (resolveType == FileConflictResolveType.Overwrite)
                            {
                                if (EntryManager.FileLockedForMe(conflict.ID))
                                {
                                    Error = FilesCommonResource.ErrorMassage_LockedFile;
                                }
                                else if (FileTracker.IsEditing(conflict.ID))
                                {
                                    Error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                                }
                                else
                                {
                                    var newFile = conflict;
                                    newFile.Version++;
                                    newFile.ContentLength = conflict.ContentLength;

                                    using (var stream = FileDao.GetFileStream(file))
                                    {
                                        newFile = FileDao.SaveFile(newFile, stream);
                                    }

                                    needToMark.Add(newFile);

                                    if (copy)
                                    {
                                        FilesMessageService.Send(file, toFolder, headers, MessageAction.FileCopiedWithOverwriting, file.Title, parentFolder.Title, toFolder.Title);
                                        if (ProcessedFile(fileId))
                                        {
                                            Status += string.Format("file_{0}{1}", newFile.ID, SPLIT_CHAR);
                                        }
                                    }
                                    else
                                    {
                                        if (Equals(file.FolderID.ToString(), toFolderId.ToString()))
                                        {
                                            if (ProcessedFile(fileId))
                                            {
                                                Status += string.Format("file_{0}{1}", newFile.ID, SPLIT_CHAR);
                                            }
                                        }
                                        else
                                        {
                                            if (EntryManager.FileLockedForMe(file.ID))
                                            {
                                                Error = FilesCommonResource.ErrorMassage_LockedFile;
                                            }
                                            else if (FileTracker.IsEditing(file.ID))
                                            {
                                                Error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                                            }
                                            else if (FilesSecurity.CanDelete(file))
                                            {
                                                FileDao.DeleteFile(file.ID);
                                                FileDao.DeleteFolder(file.ID);

                                                FilesMessageService.Send(file, toFolder, headers, MessageAction.FileMovedWithOverwriting, file.Title, parentFolder.Title, toFolder.Title);

                                                if (ProcessedFile(fileId))
                                                {
                                                    Status += string.Format("file_{0}{1}", newFile.ID, SPLIT_CHAR);
                                                }
                                            }
                                            else
                                            {
                                                Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (resolveType == FileConflictResolveType.Skip)
                            {
                                //nothing
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;
                        Logger.Error(Error, ex);
                    }
                }
                ProgressStep();
            }
        }
    }
}