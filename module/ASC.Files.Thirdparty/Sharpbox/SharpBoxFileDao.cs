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
using System.IO;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;
using ASC.Web.Studio.Core;
using AppLimit.CloudComputing.SharpBox;

namespace ASC.Files.Thirdparty.Sharpbox
{
    internal class SharpBoxFileDao : SharpBoxDaoBase, IFileDao
    {
        public SharpBoxFileDao(SharpBoxDaoSelector.SharpBoxInfo providerInfo, SharpBoxDaoSelector sharpBoxDaoSelector)
            : base(providerInfo, sharpBoxDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            SharpBoxProviderInfo.InvalidateStorage();
        }

        public Core.File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public Core.File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetFileById(fileId));
        }

        public Core.File GetFile(object parentId, string title)
        {
            return ToFile(GetFolderFiles(parentId).FirstOrDefault(x => x.Name.Contains(title)));
        }

        public List<Core.File> GetFileHistory(object fileId)
        {
            return new List<Core.File> { GetFile(fileId) };
        }

        public List<Core.File> GetFiles(object[] fileIds)
        {
            return fileIds.Select(fileId => ToFile(GetFileById(fileId))).ToList();
        }

        public Stream GetFileStream(Core.File file, long offset)
        {
            //NOTE: id here is not converted!
            var fileToDownload = GetFileById(file.ID);
            //Check length of the file
            if (fileToDownload == null)
                throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);

            //if (fileToDownload.Length > SetupInfo.AvailableFileSize)
            //{
            //    throw FileSizeComment.FileSizeException;
            //}

            var fileStream = fileToDownload.GetDataTransferAccessor().GetDownloadStream();

            if (fileStream.CanSeek)
                file.ContentLength = fileStream.Length; // hack for google drive

            if (offset > 0)
                fileStream.Seek(offset, SeekOrigin.Begin);

            return fileStream;
        }

        public Uri GetPreSignedUri(Core.File file, TimeSpan expires)
        {
            throw new NotSupportedException();
        }

        public bool IsSupportedPreSignedUri(Core.File file)
        {
            return false;
        }

        public Stream GetFileStream(Core.File file)
        {
            return GetFileStream(file, 0);
        }

        public Core.File SaveFile(Core.File file, Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException("fileStream");
            ICloudFileSystemEntry entry = null;
            if (file.ID != null)
            {
                entry = SharpBoxProviderInfo.Storage.GetFile(MakePath(file.ID), null);
            }
            else if (file.FolderID != null)
            {
                var folder = GetFolderById(file.FolderID);

                file.Title = GetAvailableTitle(file.Title, folder, IsExist);

                entry = SharpBoxProviderInfo.Storage.CreateFile(folder, file.Title);
            }
            if (entry != null)
            {
                entry.GetDataTransferAccessor().Transfer(fileStream, nTransferDirection.nUpload);
                return ToFile(entry);
            }
            return null;
        }

        public void DeleteFile(object fileId)
        {
            var file = GetFileById(fileId);
            var id = MakeId(file);

            using (var tx = DbManager.BeginTransaction())
            {
                var hashIDs = DbManager.ExecuteList(Query("files_thirdparty_id_mapping")
                                                        .Select("hash_id")
                                                        .Where(Exp.Like("id", id, SqlLike.StartWith)))
                                       .ConvertAll(x => x[0]);

                DbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", hashIDs)));
                DbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                DbManager.ExecuteNonQuery(Delete("files_security").Where(Exp.In("entry_id", hashIDs)));
                DbManager.ExecuteNonQuery(Delete("files_thirdparty_id_mapping").Where(Exp.In("hash_id", hashIDs)));

                tx.Commit();
            }

            if (!(file is ErrorEntry))
                SharpBoxProviderInfo.Storage.DeleteFileSystemEntry(file);
        }

        public bool IsExist(string title, object folderId)
        {
            return GetFolderFiles(folderId).FirstOrDefault(x => x.Name.Contains(title)) != null;
        }

        public bool IsExist(string title, ICloudDirectoryEntry folder)
        {
            try
            {
                return SharpBoxProviderInfo.Storage.GetFileSystemObject(title, folder) != null;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var oldIdValue = MakeId(GetFileById(fileId));

            SharpBoxProviderInfo.Storage.MoveFileSystemEntry(MakePath(fileId), MakePath(toFolderId));

            var newIdValue = MakeId(GetFileById(fileId));

            UpdatePathInDB(oldIdValue, newIdValue);

            return newIdValue;
        }

        public Core.File CopyFile(object fileId, object toFolderId)
        {
            var file = GetFile(fileId);
            SharpBoxProviderInfo.Storage.CopyFileSystemEntry(MakePath(fileId), MakePath(toFolderId));
            return ToFile(GetFolderById(toFolderId).FirstOrDefault(x => x.Name == file.Title));
        }

        public object FileRename(object fileId, string newTitle)
        {
            var file = GetFileById(fileId);

            var oldFileId = MakeId(file);
            var newFileId = oldFileId;

            if (SharpBoxProviderInfo.Storage.RenameFileSystemEntry(file, newTitle))
            {
                //File data must be already updated by provider
                //We can't search google files by title because root can have multiple folders with the same name
                //var newFile = SharpBoxProviderInfo.Storage.GetFileSystemObject(newTitle, file.Parent);
                newFileId = MakeId(file);
            }

            UpdatePathInDB(oldFileId, newFileId);

            return newFileId;
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            return string.Empty;
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
        }

        public bool UseTrashForRemove(Core.File file)
        {
            return false;
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(Core.File file, long contentLength)
        {
            if (SetupInfo.ChunkUploadSize > contentLength)
                return new ChunkedUploadSession(MakeId(file), contentLength) { UseChunks = false };

            var uploadSession = new ChunkedUploadSession(file, contentLength);

            ICloudFileSystemEntry sharpboxFile;
            if (file.ID != null)
            {
                sharpboxFile = GetFileById(file.ID);
            }
            else
            {
                var folder = GetFolderById(file.FolderID);
                sharpboxFile = SharpBoxProviderInfo.Storage.CreateFile(folder, GetAvailableTitle(file.Title, folder, IsExist));
            }

            var sharpboxSession = sharpboxFile.GetDataTransferAccessor().CreateResumableSession(contentLength);
            if (sharpboxSession != null)
            {
                uploadSession.Items["SharpboxSession"] = sharpboxSession;
            }
            else
            {
                uploadSession.Items["TempPath"] = Path.GetTempFileName();
            }

            uploadSession.File = MakeId(uploadSession.File);
            return uploadSession;
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                if (uploadSession.BytesTotal == 0)
                    uploadSession.BytesTotal = chunkLength;

                uploadSession.File = SaveFile(uploadSession.File, stream);
                uploadSession.BytesUploaded = chunkLength;
                return;
            }

            if (uploadSession.Items.ContainsKey("SharpboxSession"))
            {
                var sharpboxSession = uploadSession.GetItemOrDefault<IResumableUploadSession>("SharpboxSession");
                sharpboxSession.File.GetDataTransferAccessor().Transfer(sharpboxSession, stream, chunkLength);
            }
            else
            {
                var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
                using (var fs = new FileStream(tempPath, FileMode.Append))
                {
                    stream.CopyTo(fs);
                }
            }

            uploadSession.BytesUploaded += chunkLength;

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal)
            {
                uploadSession.File = FinalizeUploadSession(uploadSession);
            }
            else
            {
                uploadSession.File = MakeId(uploadSession.File);
            }
        }

        public Core.File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("SharpboxSession"))
            {
                var sharpboxSession = uploadSession.GetItemOrDefault<IResumableUploadSession>("SharpboxSession");
                return ToFile(sharpboxSession.File);
            }

            using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                return SaveFile(uploadSession.File, fs);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("SharpboxSession"))
            {
                var sharpboxSession = uploadSession.GetItemOrDefault<IResumableUploadSession>("SharpboxSession");
                sharpboxSession.File.GetDataTransferAccessor().AbortResumableSession(sharpboxSession);
            }
            else if (uploadSession.Items.ContainsKey("TempPath"))
            {
                System.IO.File.Delete(uploadSession.GetItemOrDefault<string>("TempPath"));
            }
        }

        private Core.File MakeId(Core.File file)
        {
            if (file.ID != null)
                file.ID = PathPrefix + "-" + file.ID;

            if (file.FolderID != null)
                file.FolderID = PathPrefix + "-" + file.FolderID;

            return file;
        }

        #endregion

        #region Only in TMFileDao

        public IEnumerable<Core.File> Search(string text, FolderType folderType)
        {
            return null;
        }

        public void DeleteFolder(object fileId)
        {
            //Do nothing
        }

        public void DeleteFileStream(object file)
        {
            //Do nothing
        }

        public bool IsExistOnStorage(Core.File file)
        {
            return true;
        }

        #endregion
    }
}