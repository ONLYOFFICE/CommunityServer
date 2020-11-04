/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.CRM.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;
using System;
using System.Web;


namespace ASC.Web.CRM.Classes
{
    public class OrganisationLogoHandler : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (!CRMSecurity.IsAdmin)
                throw CRMSecurity.CreateSecurityException();

            var fileUploadResult = new FileUploadResult();

            if (!FileToUpload.HasFilesToUpload(context)) return fileUploadResult;

            var file = new FileToUpload(context);

            if (String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                throw new InvalidOperationException(CRMErrorsResource.InvalidFile);

            if (0 < SetupInfo.MaxImageUploadSize && SetupInfo.MaxImageUploadSize < file.ContentLength)
            {
                fileUploadResult.Success = false;
                fileUploadResult.Message = FileSizeComment.GetFileImageSizeNote(CRMCommonResource.ErrorMessage_UploadFileSize, false).HtmlEncode();
                return fileUploadResult;
            }

            if (FileUtility.GetFileTypeByFileName(file.FileName) != FileType.Image)
            {
                fileUploadResult.Success = false;
                fileUploadResult.Message = CRMJSResource.ErrorMessage_NotImageSupportFormat.HtmlEncode();
                return fileUploadResult;
            }

            try
            {
                var imageData = Global.ToByteArray(file.InputStream);
                var imageFormat = ContactPhotoManager.CheckImgFormat(imageData);
                var photoUri = OrganisationLogoManager.UploadLogo(imageData, imageFormat);

                fileUploadResult.Success = true;
                fileUploadResult.Data = photoUri;
                return fileUploadResult;
            }
            catch (Exception exception)
            {
                fileUploadResult.Success = false;
                fileUploadResult.Message = exception.Message.HtmlEncode();
                return fileUploadResult;
            }
        }
    }
}