/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileMoveCopyOperation : FileOperation
    {
        private readonly string _toFolderId;
        private readonly bool _copy;
        private readonly FileConflictResolveType _resolveType;
        private readonly List<FileEntry> _needToMark = new List<FileEntry>();

        private readonly Dictionary<string, string> _headers;

        public override FileOperationType OperationType
        {
            get { return _copy ? FileOperationType.Copy : FileOperationType.Move; }
        }

        public FileMoveCopyOperation(List<object> folders, List<object> files, string toFolderId, bool copy, FileConflictResolveType resolveType, bool holdResult = true, Dictionary<string, string> headers = null)
            : base(folders, files, holdResult)
        {
            _toFolderId = toFolderId;
            _copy = copy;
            _resolveType = resolveType;
            _headers = headers;
        }

        protected override void Do()
        {
            Status += string.Format("folder_{0}{1}", _toFolderId, SPLIT_CHAR);

            //TODO: check on each iteration?
            var toFolder = FolderDao.GetFolder(_toFolderId);
            if (toFolder == null) return;
            if (!FilesSecurity.CanCreate(toFolder)) throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);

            if (FolderDao.GetParentFolders(toFolder.ID).Any(parent => Folders.Contains(parent.ID.ToString())))
            {
                Error = FilesCommonResource.ErrorMassage_FolderCopyError;
                return;
            }

            if (_copy)
            {
                Folder rootFrom = null;
                if (0 < Folders.Count) rootFrom = FolderDao.GetRootFolder(Folders[0]);
                if (0 < Files.Count) rootFrom = FolderDao.GetRootFolderByFile(Files[0]);
                if (rootFrom != null && rootFrom.FolderType == FolderType.TRASH) throw new InvalidOperationException("Can not copy from Trash.");
                if (toFolder.RootFolderType == FolderType.TRASH) throw new InvalidOperationException("Can not copy to Trash.");
            }

            MoveOrCopyFolders(Folders, toFolder, _copy);
            MoveOrCopyFiles(Files, toFolder, _copy);

            _needToMark.Distinct().ToList().ForEach(x => FileMarker.MarkAsNew(x));
        }

        private void MoveOrCopyFolders(ICollection folderIds, Folder toFolder, bool copy)
        {
            if (folderIds.Count == 0) return;

            var toFolderId = toFolder.ID;
            var isToFolder = Equals(toFolderId.ToString(), _toFolderId);

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
                else if (!Equals((folder.ParentFolderID ?? string.Empty).ToString(), toFolderId.ToString()) || _resolveType == FileConflictResolveType.Duplicate)
                {
                    try
                    {
                        //if destination folder contains folder with same name then merge folders
                        var conflictFolder = FolderDao.GetFolder(folder.Title, toFolderId);
                        Folder newFolder;

                        if (copy || conflictFolder != null)
                        {
                            if (conflictFolder != null)
                            {
                                newFolder = conflictFolder;

                                if (isToFolder)
                                    _needToMark.Add(conflictFolder);
                            }
                            else
                            {
                                newFolder = FolderDao.CopyFolder(folder.ID, toFolderId, CancellationToken);
                                FilesMessageService.Send(newFolder, toFolder, _headers, MessageAction.FolderCopied, newFolder.Title, toFolder.Title);

                                if (isToFolder)
                                    _needToMark.Add(newFolder);

                                if (ProcessedFolder(folderId))
                                {
                                    Status += string.Format("folder_{0}{1}", newFolder.ID, SPLIT_CHAR);
                                }
                            }

                            if (FolderDao.UseRecursiveOperation(folder.ID, toFolderId))
                            {
                                MoveOrCopyFiles(FileDao.GetFiles(folder.ID), newFolder, copy);
                                MoveOrCopyFolders(FolderDao.GetFolders(folder.ID).Select(f => f.ID).ToList(), newFolder, copy);

                                if (!copy)
                                {
                                    if (!FilesSecurity.CanDelete(folder))
                                    {
                                        Error = FilesCommonResource.ErrorMassage_SecurityException_MoveFolder;
                                    }
                                    else if (FolderDao.IsEmpty(folder.ID))
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
                                    string tmpError;
                                    object newFolderId;
                                    if (copy)
                                    {
                                        newFolder = FolderDao.CopyFolder(folder.ID, toFolderId, CancellationToken);
                                        newFolderId = newFolder.ID;
                                        FilesMessageService.Send(newFolder, toFolder, _headers, MessageAction.FolderCopiedWithOverwriting, newFolder.Title, toFolder.Title);

                                        if (isToFolder)
                                            _needToMark.Add(newFolder);

                                        if (ProcessedFolder(folderId))
                                        {
                                            Status += string.Format("folder_{0}{1}", newFolderId, SPLIT_CHAR);
                                        }
                                    }
                                    else if (!FilesSecurity.CanDelete(folder))
                                    {
                                        Error = FilesCommonResource.ErrorMassage_SecurityException_MoveFolder;
                                    }
                                    else if (WithError(FileDao.GetFiles(folder.ID, new OrderBy(SortedByType.AZ, true), FilterType.FilesOnly, false, Guid.Empty, string.Empty, false, true), out tmpError))
                                    {
                                        Error = tmpError;
                                    }
                                    else
                                    {
                                        FileMarker.RemoveMarkAsNewForAll(folder);

                                        newFolderId = FolderDao.MoveFolder(folder.ID, toFolderId, CancellationToken);
                                        newFolder = FolderDao.GetFolder(newFolderId);
                                        FilesMessageService.Send(folder.RootFolderType != FolderType.USER ? folder : newFolder, toFolder, _headers, MessageAction.FolderMovedWithOverwriting, folder.Title, toFolder.Title);

                                        if (isToFolder)
                                            _needToMark.Add(newFolder);

                                        if (ProcessedFolder(folderId))
                                        {
                                            Status += string.Format("folder_{0}{1}", newFolderId, SPLIT_CHAR);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string tmpError;
                            if (!FilesSecurity.CanDelete(folder))
                            {
                                Error = FilesCommonResource.ErrorMassage_SecurityException_MoveFolder;
                            }
                            else if (WithError(FileDao.GetFiles(folder.ID, new OrderBy(SortedByType.AZ, true), FilterType.FilesOnly, false, Guid.Empty, string.Empty, false, true), out tmpError))
                            {
                                Error = tmpError;
                            }
                            else
                            {
                                FileMarker.RemoveMarkAsNewForAll(folder);

                                var newFolderId = FolderDao.MoveFolder(folder.ID, toFolderId, CancellationToken);
                                newFolder = FolderDao.GetFolder(newFolderId);
                                FilesMessageService.Send(folder.RootFolderType != FolderType.USER ? folder : newFolder, toFolder, _headers, MessageAction.FolderMoved, folder.Title, toFolder.Title);

                                if (isToFolder)
                                    _needToMark.Add(newFolder);

                                if (ProcessedFolder(folderId))
                                {
                                    Status += string.Format("folder_{0}{1}", newFolderId, SPLIT_CHAR);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;

                        Logger.Error(Error, ex);
                    }
                }
                ProgressStep(FolderDao.CanCalculateSubitems(folderId) ? null : folderId);
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
                        var conflict = _resolveType == FileConflictResolveType.Duplicate
                                           ? null
                                           : FileDao.GetFile(toFolderId, file.Title);
                        if (conflict == null)
                        {
                            File newFile = null;
                            if (copy)
                            {
                                try
                                {
                                    newFile = FileDao.CopyFile(file.ID, toFolderId); //Stream copy will occur inside dao
                                    FilesMessageService.Send(newFile, toFolder, _headers, MessageAction.FileCopied, newFile.Title, parentFolder.Title, toFolder.Title);

                                    if (Equals(newFile.FolderID.ToString(), _toFolderId))
                                    {
                                        _needToMark.Add(newFile);
                                    }

                                    if (ProcessedFile(fileId))
                                    {
                                        Status += string.Format("file_{0}{1}", newFile.ID, SPLIT_CHAR);
                                    }
                                }
                                catch
                                {
                                    if (newFile != null)
                                    {
                                        FileDao.DeleteFile(newFile.ID);
                                    }
                                    throw;
                                }
                            }
                            else
                            {
                                string tmpError;
                                if (WithError(new[] { file }, out tmpError))
                                {
                                    Error = tmpError;
                                }
                                else
                                {
                                    FileMarker.RemoveMarkAsNewForAll(file);

                                    var newFileId = FileDao.MoveFile(file.ID, toFolderId);
                                    newFile = FileDao.GetFile(newFileId);
                                    FilesMessageService.Send(file.RootFolderType != FolderType.USER ? file : newFile, toFolder, _headers, MessageAction.FileMoved, file.Title, parentFolder.Title, toFolder.Title);

                                    if (Equals(toFolderId.ToString(), _toFolderId))
                                    {
                                        _needToMark.Add(newFile);
                                    }

                                    if (ProcessedFile(fileId))
                                    {
                                        Status += string.Format("file_{0}{1}", newFileId, SPLIT_CHAR);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_resolveType == FileConflictResolveType.Overwrite)
                            {
                                if (!FilesSecurity.CanEdit(conflict))
                                {
                                    Error = FilesCommonResource.ErrorMassage_SecurityException;
                                }
                                else if (EntryManager.FileLockedForMe(conflict.ID))
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
                                    newFile.VersionGroup++;
                                    newFile.PureTitle = file.PureTitle;
                                    newFile.ConvertedType = file.ConvertedType;
                                    newFile.Comment = FilesCommonResource.CommentOverwrite;
                                    newFile.Encrypted = file.Encrypted;

                                    using (var stream = FileDao.GetFileStream(file))
                                    {
                                        newFile.ContentLength = stream.CanSeek ? stream.Length : file.ContentLength;

                                        newFile = FileDao.SaveFile(newFile, stream);
                                    }

                                    _needToMark.Add(newFile);

                                    if (copy)
                                    {
                                        FilesMessageService.Send(newFile, toFolder, _headers, MessageAction.FileCopiedWithOverwriting, newFile.Title, parentFolder.Title, toFolder.Title);
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
                                            string tmpError;
                                            if (WithError(new[] { file }, out tmpError))
                                            {
                                                Error = tmpError;
                                            }
                                            else
                                            {
                                                FileDao.DeleteFile(file.ID);

                                                FilesMessageService.Send(file.RootFolderType != FolderType.USER ? file : newFile, toFolder, _headers, MessageAction.FileMovedWithOverwriting, file.Title, parentFolder.Title, toFolder.Title);

                                                if (ProcessedFile(fileId))
                                                {
                                                    Status += string.Format("file_{0}{1}", newFile.ID, SPLIT_CHAR);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (_resolveType == FileConflictResolveType.Skip)
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
                ProgressStep(fileId: FolderDao.CanCalculateSubitems(fileId) ? null : fileId);
            }
        }

        private bool WithError(IEnumerable<File> files, out string error)
        {
            error = null;
            foreach (var file in files)
            {
                if (!FilesSecurity.CanDelete(file))
                {
                    error = FilesCommonResource.ErrorMassage_SecurityException_MoveFile;
                    return true;
                }
                if (EntryManager.FileLockedForMe(file.ID))
                {
                    error = FilesCommonResource.ErrorMassage_LockedFile;
                    return true;
                }
                if (FileTracker.IsEditing(file.ID))
                {
                    error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                    return true;
                }
            }
            return false;
        }
    }
}