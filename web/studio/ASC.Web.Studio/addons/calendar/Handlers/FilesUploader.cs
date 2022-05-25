/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Configuration;
using System.Web;

using ASC.Api.Calendar.Attachments;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Calendar.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;

namespace ASC.Web.Calendar.Handlers
{
    public class FilesUploader : FileUploadHandler
    {
        public static int MaxFileSizeInMegabytes 
        { 
            get
            {
                int size;
                if (int.TryParse(ConfigurationManagerExtension.AppSettings["calendar.attachment.max-upload-size"], out size)) 
                    return size;
                else return 15;
            }
        }

        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                throw new HttpException(403, "Access denied.");
            }

            var result = new FileUploadResult { Success = false };

            try
            {
                if (FileToUpload.HasFilesToUpload(context))
                {
                    var file = new FileToUpload(context);
                    var maxFileSize = MaxFileSizeInMegabytes * 1024 * 1024;

                    if (string.IsNullOrEmpty(file.FileName))
                        throw new ArgumentException("Empty file name");

                    if (maxFileSize < file.ContentLength)
                        throw new Exception(CalendarJSResource.calendarEventAttachments_fileSizeError);

                    var fileName = System.IO.Path.GetFileName(file.FileName);

                    var document = new File
                    {
                        Title = fileName,
                        FolderID = AttachmentEngine.GetTmpFolderId(),
                        ContentLength = file.ContentLength,
                        ThumbnailStatus = Thumbnail.NotRequired
                    };

                    document = AttachmentEngine.SaveFile(document, file.InputStream);

                    result.Data = new
                    {
                        id = document.ID.ToString(),
                        title = document.Title,
                        size = document.ContentLength,
                        contentType = file.FileContentType,
                        fileUrl = FileShareLink.GetLink(document) + "&tmp=true"
                    };

                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}