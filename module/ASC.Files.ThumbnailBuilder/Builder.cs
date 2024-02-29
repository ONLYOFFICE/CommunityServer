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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Services.DocumentService;

using File = ASC.Files.Core.File;

namespace ASC.Files.ThumbnailBuilder
{
    internal class Builder
    {
        private readonly ConfigSection config;
        private readonly ILog logger;

        public Builder(ConfigSection configSection, ILog log)
        {
            config = configSection;
            logger = log;
        }

        public void BuildThumbnails(IEnumerable<FileData> filesWithoutThumbnails)
        {
            try
            {
                Parallel.ForEach(
                    filesWithoutThumbnails,
                    new ParallelOptions { MaxDegreeOfParallelism = config.MaxDegreeOfParallelism },
                    (fileData) => { BuildThumbnail(fileData); }
                );
            }
            catch (Exception exception)
            {
                logger.Error(string.Format("BuildThumbnails: filesWithoutThumbnails.Count: {0}.", filesWithoutThumbnails.Count()), exception);
            }
        }

        private void BuildThumbnail(FileData fileData)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(fileData.TenantId);

                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    if (fileDao == null)
                    {
                        logger.ErrorFormat("BuildThumbnail: TenantId: {0}. FileDao could not be null.", fileData.TenantId);
                        return;
                    }

                    GenerateThumbnail(fileDao, fileData);
                }
            }
            catch (Exception exception)
            {
                logger.Error(string.Format("BuildThumbnail: TenantId: {0}.", fileData.TenantId), exception);
            }
            finally
            {
                Launcher.Queue.TryRemove(fileData.FileId, out _);
            }
        }

        private void GenerateThumbnail(IFileDao fileDao, FileData fileData)
        {
            File file = null;

            try
            {
                file = fileDao.GetFile(fileData.FileId);

                if (file == null)
                {
                    logger.ErrorFormat("GenerateThumbnail: FileId: {0}. File not found.", fileData.FileId);
                    return;
                }

                if (file.ThumbnailStatus != Thumbnail.Waiting)
                {
                    logger.InfoFormat("GenerateThumbnail: FileId: {0}. Thumbnail already processed.", fileData.FileId);
                    return;
                }

                var ext = FileUtility.GetFileExtension(file.Title);

                if (!config.FormatsArray.Contains(ext) || file.Encrypted || file.RootFolderType == FolderType.TRASH || file.ContentLength > config.AvailableFileSize)
                {
                    file.ThumbnailStatus = Thumbnail.NotRequired;
                    fileDao.SaveThumbnail(file, null);
                    return;
                }

                if (IsImage(file))
                {
                    CropImage(fileDao, file);
                }
                else
                {
                    MakeThumbnail(fileDao, file);
                }
            }
            catch (Exception exception)
            {
                logger.Error(string.Format("GenerateThumbnail: FileId: {0}.", fileData.FileId), exception);
                if (file != null)
                {
                    file.ThumbnailStatus = Thumbnail.Error;
                    fileDao.SaveThumbnail(file, null);
                }
            }
        }

        private void MakeThumbnail(IFileDao fileDao, File file)
        {
            logger.DebugFormat("MakeThumbnail: FileId: {0}.", file.ID);

            string thumbnailUrl = null;
            var attempt = 1;

            do
            {
                try
                {
                    if (GetThumbnailUrl(file, Global.ThumbnailExtension, out thumbnailUrl))
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    if (exception.InnerException != null)
                    {
                        var documentServiceException = exception.InnerException as DocumentService.DocumentServiceException;
                        if (documentServiceException != null)
                        {
                            if (documentServiceException.Code == DocumentService.DocumentServiceException.ErrorCode.ConvertPassword)
                            {
                                throw new Exception(string.Format("MakeThumbnail: FileId: {0}. Encrypted file.", file.ID));
                            }
                            if (documentServiceException.Code == DocumentService.DocumentServiceException.ErrorCode.Convert)
                            {
                                throw new Exception(string.Format("MakeThumbnail: FileId: {0}. Could not convert.", file.ID));
                            }
                        }
                    }
                }

                if (attempt >= config.AttemptsLimit)
                {
                    throw new Exception(string.Format("MakeThumbnail: FileId: {0}. Attempts limmit exceeded.", file.ID));
                }
                else
                {
                    logger.DebugFormat("MakeThumbnail: FileId: {0}. Sleep {1} after attempt #{2}. ", file.ID, config.AttemptWaitInterval, attempt);
                    attempt++;
                }

                Thread.Sleep(config.AttemptWaitInterval);
            }
            while (string.IsNullOrEmpty(thumbnailUrl));

            SaveThumbnail(fileDao, file, thumbnailUrl);
        }

        private bool GetThumbnailUrl(File file, string toExtension, out string url)
        {
            var fileUri = PathProvider.GetFileStreamUrl(file);
            fileUri = DocumentServiceConnector.ReplaceCommunityAdress(fileUri);

            var fileExtension = file.ConvertedExtension;
            var docKey = DocumentServiceHelper.GetDocKey(file);

            var thumbnailAspect = config.ThumbnailAspect;
            var thumbnail = GetThumbnailData(thumbnailAspect);
            var spreadsheetLayout = GetSpreadsheetLayout(thumbnailAspect);
            var operationResultProgress = DocumentServiceConnector.GetConvertedUri(fileUri, fileExtension, toExtension, docKey, null, null, thumbnail, spreadsheetLayout, false, out url, out _);

            operationResultProgress = Math.Min(operationResultProgress, 100);
            return operationResultProgress == 100;
        }

        private DocumentService.ThumbnailData GetThumbnailData(int thumbnailAspect)
        {
            return new DocumentService.ThumbnailData
            {
                Aspect = thumbnailAspect,
                First = true,
                Height = thumbnailAspect == 3 ? config.ThumbnaillHeight : 0,
                Width = thumbnailAspect == 3 ? config.ThumbnaillWidth : 0,
            };
        }

        private DocumentService.SpreadsheetLayout GetSpreadsheetLayout(int thumbnailAspect)
        {
            return new DocumentService.SpreadsheetLayout
            {
                IgnorePrintArea = true,
                //Orientation = "landscape", // "297mm" x "210mm"
                FitToHeight = 0,
                FitToWidth = 1,
                Headings = false,
                GridLines = false,
                Margins = new DocumentService.SpreadsheetLayout.LayoutMargins
                {
                    Top = "0mm",
                    Right = "0mm",
                    Bottom = "0mm",
                    Left = "0mm"
                },
                PageSize = thumbnailAspect == 3 ?
                    new DocumentService.SpreadsheetLayout.LayoutPageSize() :
                    new DocumentService.SpreadsheetLayout.LayoutPageSize
                    {
                        Width = (config.ThumbnaillWidth * 1.5) + "mm", // 192 * 1.5 = "288mm",
                        Height = (config.ThumbnaillHeight * 1.5) + "mm" // 128 * 1.5 = "192mm"
                    }
            };
        }

        private void SaveThumbnail(IFileDao fileDao, File file, string thumbnailUrl)
        {
            logger.DebugFormat("SaveThumbnail: FileId: {0}. ThumbnailUrl {1}.", file.ID, thumbnailUrl);

            var req = (HttpWebRequest)WebRequest.Create(thumbnailUrl);

            //HACK: http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono && ServicePointManager.ServerCertificateValidationCallback == null)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, c, n, p) => true;
            }

            using (var stream = new ResponseStream(req.GetResponse()))
            {
                if (config.ThumbnailAspect == 3)
                {
                    fileDao.SaveThumbnail(file, stream);
                }
                else
                {
                    Crop(fileDao, file, stream);
                }
            }

            logger.DebugFormat("SaveThumbnail: FileId: {0}. Successfully saved.", file.ID);
        }

        private bool IsImage(File file)
        {
            var extension = FileUtility.GetFileExtension(file.Title);
            return FileUtility.ExtsImage.Contains(extension);
        }

        private void CropImage(IFileDao fileDao, File file)
        {
            logger.DebugFormat("CropImage: FileId: {0}.", file.ID);

            using (var stream = fileDao.GetFileStream(file))
            {
                Crop(fileDao, file, stream);
            }

            logger.DebugFormat("CropImage: FileId: {0}. Successfully saved.", file.ID);
        }

        private void Crop(IFileDao fileDao, File file, Stream stream)
        {
            using (var sourceBitmap = new Bitmap(stream))
            {
                using (var targetBitmap = GetImageThumbnail(sourceBitmap))
                {
                    using (var targetStream = new MemoryStream())
                    {
                        targetBitmap.Save(targetStream, System.Drawing.Imaging.ImageFormat.Png);
                        fileDao.SaveThumbnail(file, targetStream);
                    }
                }
            }
        }

        private Image GetImageThumbnail(Bitmap sourceBitmap)
        {
            //bad for small or disproportionate images
            //return sourceBitmap.GetThumbnailImage(config.ThumbnaillWidth, config.ThumbnaillHeight, () => false, IntPtr.Zero);

            var targetSize = new Size(Math.Min(sourceBitmap.Width, config.ThumbnaillWidth), Math.Min(sourceBitmap.Height, config.ThumbnaillHeight));
            var point = new Point(0, 0);
            var size = targetSize;

            if (sourceBitmap.Width > config.ThumbnaillWidth && sourceBitmap.Height > config.ThumbnaillHeight)
            {
                if (sourceBitmap.Width > sourceBitmap.Height)
                {
                    var width = (int)(config.ThumbnaillWidth * (sourceBitmap.Height / (1.0 * config.ThumbnaillHeight)));
                    size = new Size(width, sourceBitmap.Height);
                }
                else
                {
                    var height = (int)(config.ThumbnaillHeight * (sourceBitmap.Width / (1.0 * config.ThumbnaillWidth)));
                    size = new Size(sourceBitmap.Width, height);
                }
            }

            if (sourceBitmap.Width > sourceBitmap.Height)
            {
                point.X = (sourceBitmap.Width - size.Width) / 2;
            }

            var targetThumbnailSettings = new UserPhotoThumbnailSettings(point, size);

            return UserPhotoThumbnailManager.GetBitmap(sourceBitmap, targetSize, targetThumbnailSettings, InterpolationMode.Bilinear);
        }
    }
}
