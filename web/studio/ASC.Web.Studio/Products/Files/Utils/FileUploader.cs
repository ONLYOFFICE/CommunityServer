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
using System.IO;
using System.Security;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;
using File = ASC.Files.Core.File;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public static class FileUploader
    {
        public static File Exec(string folderId, string title, long contentLength, Stream data)
        {
            return Exec(folderId, title, contentLength, data, !FilesSettings.UpdateIfExist);
        }

        public static File Exec(string folderId, string title, long contentLength, Stream data, bool createNewIfExist)
        {
            if (contentLength <= 0)
                throw new Exception(FilesCommonResource.ErrorMassage_EmptyFile);

            var file = VerifyFileUpload(folderId, title, contentLength, !createNewIfExist);

            using (var dao = Global.DaoFactory.GetFileDao())
            {
                file = dao.SaveFile(file, data);
            }

            FileMarker.MarkAsNew(file);

            if (FileConverter.EnableAsUploaded && FileConverter.MustConvert(file))
                FileConverter.ExecAsync(file, true);

            return file;
        }

        public static File VerifyFileUpload(string folderId, string fileName, bool updateIfExists)
        {
            fileName = Global.ReplaceInvalidCharsAndTruncate(fileName);

            if (Global.EnableUploadFilter && !FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(fileName)))
                throw new NotSupportedException(FilesCommonResource.ErrorMassage_NotSupportedFormat);

            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var file = fileDao.GetFile(folderId, fileName);

                if (updateIfExists && CanEdit(file))
                {
                    file.Title = fileName;
                    file.ConvertedType = null;
                    file.Version++;

                    return file;
                }

                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    var folder = folderDao.GetFolder(folderId);

                    if (folder == null)
                        throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);

                    if (!Global.GetFilesSecurity().CanCreate(folder))
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
                }

                return new File { FolderID = folderId, Title = fileName };
            }
        }

        public static File VerifyFileUpload(string folderId, string fileName, long fileSize, bool updateIfExists)
        {
            if (fileSize <= 0)
                throw new Exception(FilesCommonResource.ErrorMassage_EmptyFile);

            long maxUploadSize;
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                maxUploadSize = folderDao.GetMaxUploadSize(folderId);
            }

            if (fileSize > maxUploadSize)
                throw FileSizeComment.GetFileSizeException(maxUploadSize);

            var file = VerifyFileUpload(folderId, fileName, updateIfExists);
            file.ContentLength = fileSize;
            file.Comment = string.Empty;
            return file;
        }

        private static long GetMaxFileSize(bool chunkedUpload)
        {
            return chunkedUpload ? SetupInfo.MaxChunkedUploadSize : SetupInfo.MaxUploadSize;
        }

        private static bool CanEdit(File file)
        {
            return file != null
                   && Global.GetFilesSecurity().CanEdit(file)
                   && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                   && !EntryManager.FileLockedForMe(file.ID)
                   && !FileTracker.IsEditing(file.ID)
                   && file.RootFolderType != FolderType.TRASH;
        }

        #region chunked upload

        public static File VerifyChunkedUpload(string folderId, string fileName, long fileSize, bool updateIfExists)
        {
            long maxUploadSize;
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
               maxUploadSize = folderDao.GetMaxUploadSize(folderId, true);
            }

            if (fileSize > maxUploadSize)
                throw FileSizeComment.GetFileSizeException(maxUploadSize);

            File file = VerifyFileUpload(folderId, fileName, updateIfExists);
            file.ContentLength = fileSize;

            return file;
        }

        public static ChunkedUploadSession InitiateUpload(string folderId, string fileId, string fileName, long contentLength)
        {
            if (string.IsNullOrEmpty(folderId))
                folderId = null;

            if (string.IsNullOrEmpty(fileId))
                fileId = null;

            var file = new File {ID = fileId, FolderID = folderId, Title = fileName, ContentLength = contentLength};

            using (var dao = Global.DaoFactory.GetFileDao())
            {
                var uploadSession = dao.CreateUploadSession(file, contentLength);
                
                uploadSession.Expired = uploadSession.Created + ChunkedUploadSessionHolder.SlidingExpiration;
                uploadSession.Location = FilesLinkUtility.GetUploadChunkLocationUrl(uploadSession.Id, contentLength > 0);
                uploadSession.TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                uploadSession.UserId = SecurityContext.CurrentAccount.ID;

                ChunkedUploadSessionHolder.StoreSession(uploadSession);

                return uploadSession;
            }
        }

        public static ChunkedUploadSession UploadChunk(string uploadId, Stream stream, long chunkLength)
        {
            var uploadSession = ChunkedUploadSessionHolder.GetSession(uploadId);
            uploadSession.Expired = DateTime.UtcNow + ChunkedUploadSessionHolder.SlidingExpiration;

            if (chunkLength <= 0)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_EmptyFile);
            }

            if (chunkLength > SetupInfo.MaxUploadSize)
            {
                throw FileSizeComment.GetFileSizeException(SetupInfo.MaxUploadSize);
            }

            if (uploadSession.BytesUploaded + chunkLength > GetMaxFileSize(uploadSession))
            {
                AbortUpload(uploadSession);
                throw FileSizeComment.GetFileSizeException(GetMaxFileSize(uploadSession));
            }
            
            using (var dao = Global.DaoFactory.GetFileDao())
            {
                dao.UploadChunk(uploadSession, stream, chunkLength);
            }

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal)
            {
                FileMarker.MarkAsNew(uploadSession.File);
                ChunkedUploadSessionHolder.RemoveSession(uploadSession);
            }

            return uploadSession;
        }

        public static void AbortUpload(string uploadId)
        {
            AbortUpload(ChunkedUploadSessionHolder.GetSession(uploadId));
        }

        private static void AbortUpload(ChunkedUploadSession uploadSession)
        {
            using (var dao = Global.DaoFactory.GetFileDao())
            {
                dao.AbortUploadSession(uploadSession);
            }

            ChunkedUploadSessionHolder.RemoveSession(uploadSession);
        }

        private static long GetMaxFileSize(ChunkedUploadSession uploadSession)
        {
            return GetMaxFileSize(uploadSession.BytesTotal > 0);
        }

        #endregion
    }
}