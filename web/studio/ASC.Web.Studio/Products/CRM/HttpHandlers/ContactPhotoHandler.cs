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
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;
using System;
using System.Web;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Core;
using Autofac;


namespace ASC.Web.CRM.Classes
{
    public class ContactPhotoHandler : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (!WebItemSecurity.IsAvailableForMe(ProductEntryPoint.ID))
                throw CRMSecurity.CreateSecurityException();

            var contactId = Convert.ToInt32(context.Request["contactID"]);
            Contact contact = null;
            if (contactId != 0)
            {
                using (var scope = DIHelper.Resolve())
                {
                    contact = scope.Resolve<DaoFactory>().ContactDao.GetByID(contactId);
                    if (!CRMSecurity.CanEdit(contact))
                        throw CRMSecurity.CreateSecurityException();
                }
            }

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

            var uploadOnly = Convert.ToBoolean(context.Request["uploadOnly"]);
            var tmpDirName = Convert.ToString(context.Request["tmpDirName"]);

            try
            {
                ContactPhotoManager.PhotoData photoData;
                if (contactId != 0)
                {
                    photoData = ContactPhotoManager.UploadPhoto(file.InputStream, contactId, uploadOnly);
                }
                else
                {
                    if (String.IsNullOrEmpty(tmpDirName) || tmpDirName == "null")
                    {
                        tmpDirName = Guid.NewGuid().ToString();
                    }
                    photoData = ContactPhotoManager.UploadPhotoToTemp(file.InputStream, tmpDirName);
                }

                fileUploadResult.Success = true;
                fileUploadResult.Data = photoData;
            }
            catch (Exception e)
            {
                fileUploadResult.Success = false;
                fileUploadResult.Message = e.Message.HtmlEncode();
                return fileUploadResult;
            }

            if (contact != null)
            {
                var messageAction = contact is Company ? MessageAction.CompanyUpdatedPhoto : MessageAction.PersonUpdatedPhoto;
                MessageService.Send(context.Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());
            }

            return fileUploadResult;
        }
    }
}