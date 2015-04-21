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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;
using ASC.Web.Studio.Core;
using File = ASC.Files.Core.File;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal class GoogleDriveFileDao : GoogleDriveDaoBase, IFileDao
    {
        public GoogleDriveFileDao(GoogleDriveDaoSelector.GoogleDriveInfo providerInfo, GoogleDriveDaoSelector googleDriveDaoSelector)
            : base(providerInfo, googleDriveDaoSelector)
        {
        }

        public void InvalidateCache(object fileId)
        {
            var driveId = MakeDriveId(fileId);
            CacheReset(driveId, true);
        }

        public File GetFile(object fileId)
        {
            return GetFile(fileId, 1);
        }

        public File GetFile(object fileId, int fileVersion)
        {
            return ToFile(GetDriveEntry(fileId));
        }

        public File GetFile(object parentId, string title)
        {
            return ToFile(GetDriveEntries(parentId, false)
                              .FirstOrDefault(file => file.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)));
        }

        public List<File> GetFileHistory(object fileId)
        {
            return new List<File> { GetFile(fileId) };
        }

        public List<File> GetFiles(object[] fileIds)
        {
            return fileIds.Select(GetDriveEntry).Select(ToFile).ToList();
        }

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        public Stream GetFileStream(File file, long offset)
        {
            var driveId = MakeDriveId(file.ID);
            CacheReset(driveId, true);
            var driveFile = GetDriveEntry(file.ID);
            if (driveFile == null) throw new ArgumentNullException("file", Web.Files.Resources.FilesCommonResource.ErrorMassage_FileNotFound);
            if (driveFile is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFile).Error);

            var fileStream = GoogleDriveProviderInfo.Storage.DownloadStream(driveFile);

            if (fileStream.CanSeek)
                file.ContentLength = fileStream.Length; // hack for google drive

            if (fileStream.CanSeek && offset > 0)
                fileStream.Seek(offset, SeekOrigin.Begin);

            return fileStream;
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            throw new NotSupportedException();
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            return false;
        }

        public File SaveFile(File file, Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException("fileStream");

            Google.Apis.Drive.v2.Data.File newDriveFile = null;

            if (file.ID != null)
            {
                newDriveFile = GoogleDriveProviderInfo.Storage.SaveStream(MakeDriveId(file.ID), fileStream, file.Title);
            }

            else if (file.FolderID != null)
            {
                newDriveFile = GoogleDriveProviderInfo.Storage.InsertEntry(fileStream, file.Title, MakeDriveId(file.FolderID));
            }

            CacheInsert(newDriveFile);
            var parentDriveId = GetParentDriveId(newDriveFile);
            if (parentDriveId != null) CacheReset(parentDriveId, false);

            return ToFile(newDriveFile);
        }

        public void DeleteFile(object fileId)
        {
            var driveFile = GetDriveEntry(fileId);
            var id = MakeId(driveFile.Id);

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

            if (!(driveFile is ErrorDriveEntry))
                GoogleDriveProviderInfo.Storage.DeleteEntry(driveFile.Id);

            CacheReset(driveFile.Id);
            var parentDriveId = GetParentDriveId(driveFile);
            if (parentDriveId != null) CacheReset(parentDriveId, false);
        }

        public bool IsExist(string title, object folderId)
        {
            return GetDriveEntries(folderId, false)
                .Any(file => file.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        public object MoveFile(object fileId, object toFolderId)
        {
            var driveFile = GetDriveEntry(fileId);
            if (driveFile is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFile).Error);

            var toDriveFolder = GetDriveEntry(toFolderId);
            if (toDriveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)toDriveFolder).Error);

            var fromFolderDriveId = GetParentDriveId(driveFile);

            GoogleDriveProviderInfo.Storage.InsertEntryIntoFolder(driveFile.Id, toDriveFolder.Id);
            if (fromFolderDriveId != null)
            {
                GoogleDriveProviderInfo.Storage.RemoveEntryFromFolder(driveFile.Id, fromFolderDriveId);
            }

            CacheReset(driveFile.Id);
            CacheReset(fromFolderDriveId, false);
            CacheReset(toDriveFolder.Id, false);

            return MakeId(driveFile.Id);
        }

        public File CopyFile(object fileId, object toFolderId)
        {
            var driveFile = GetDriveEntry(fileId);
            if (driveFile is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)driveFile).Error);

            var toDriveFolder = GetDriveEntry(toFolderId);
            if (toDriveFolder is ErrorDriveEntry) throw new Exception(((ErrorDriveEntry)toDriveFolder).Error);

            var newDriveFile = GoogleDriveProviderInfo.Storage.CopyEntry(toDriveFolder.Id, driveFile.Id);

            CacheInsert(newDriveFile);
            CacheReset(toDriveFolder.Id, false);

            return ToFile(newDriveFile);
        }

        public object FileRename(object fileId, string newTitle)
        {
            var driveFile = GetDriveEntry(fileId);
            driveFile.Title = newTitle;

            driveFile = GoogleDriveProviderInfo.Storage.UpdateEntry(driveFile);

            CacheInsert(driveFile);
            var parentDriveId = GetParentDriveId(driveFile);
            if (parentDriveId != null) CacheReset(parentDriveId, false);

            return MakeId(driveFile.Id);
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

        public bool UseTrashForRemove(File file)
        {
            return false;
        }

        #region chunking

        private File RestoreIds(File file)
        {
            if (file == null) return null;

            if (file.ID != null)
                file.ID = MakeId(file.ID.ToString());

            if (file.FolderID != null)
                file.FolderID = MakeId(file.FolderID.ToString());

            return file;
        }

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            if (SetupInfo.ChunkUploadSize > contentLength)
                return new ChunkedUploadSession(RestoreIds(file), contentLength) { UseChunks = false };

            var uploadSession = new ChunkedUploadSession(file, contentLength);

            Google.Apis.Drive.v2.Data.File driveFile;
            if (file.ID != null)
            {
                driveFile = GetDriveEntry(file.ID);
            }
            else
            {
                var folder = GetDriveEntry(file.FolderID);
                driveFile = GoogleDriveProviderInfo.Storage.FileConstructor(file.Title, null, folder.Id);
            }

            var googleDriveSession = GoogleDriveProviderInfo.Storage.CreateResumableSession(driveFile, contentLength);
            if (googleDriveSession != null)
            {
                uploadSession.Items["GoogleDriveSession"] = googleDriveSession;
            }
            else
            {
                uploadSession.Items["TempPath"] = Path.GetTempFileName();
            }

            uploadSession.File = RestoreIds(uploadSession.File);
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

            if (uploadSession.Items.ContainsKey("GoogleDriveSession"))
            {
                var googleDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("GoogleDriveSession");
                GoogleDriveProviderInfo.Storage.Transfer(googleDriveSession, stream, chunkLength);
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
                uploadSession.File = RestoreIds(uploadSession.File);
            }
        }

        public File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("GoogleDriveSession"))
            {
                var googleDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("GoogleDriveSession");

                CacheInsert(googleDriveSession.File);
                var parentDriveId = GetParentDriveId(googleDriveSession.File);
                if (parentDriveId != null) CacheReset(parentDriveId, false);

                return ToFile(googleDriveSession.File);
            }

            using (var fs = new FileStream(uploadSession.GetItemOrDefault<string>("TempPath"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                return SaveFile(uploadSession.File, fs);
            }
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.Items.ContainsKey("GoogleDriveSession"))
            {
                var googleDriveSession = uploadSession.GetItemOrDefault<ResumableUploadSession>("GoogleDriveSession");

                if (googleDriveSession.Status != ResumableUploadSessionStatus.Completed)
                {
                    googleDriveSession.Status = ResumableUploadSessionStatus.Aborted;
                }
            }
            else if (uploadSession.Items.ContainsKey("TempPath"))
            {
                System.IO.File.Delete(uploadSession.GetItemOrDefault<string>("TempPath"));
            }
        }

        #endregion


        #region Only in TMFileDao

        public IEnumerable<File> Search(string text, FolderType folderType)
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

        public bool IsExistOnStorage(File file)
        {
            return true;
        }

        public void SaveEditHistory(File file, string changes, Stream differenceStream)
        {
            //Do nothing
        }

        public List<EditHistory> GetEditHistory(object fileId, int fileVersion)
        {
            return null;
        }

        public string GetDifferenceUrl(File file)
        {
            return null;
        }

        #endregion
    }
}