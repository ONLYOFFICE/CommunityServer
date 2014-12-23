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

using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;
using System;
using System.Web;


namespace ASC.Web.CRM.Classes
{
    public class ContactPhotoHandler : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var fileUploadResult = new FileUploadResult();

            if (!FileToUpload.HasFilesToUpload(context)) return fileUploadResult;

            var file = new FileToUpload(context);

            if (String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                throw new InvalidOperationException("Invalid file.");

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

            var contactId = Convert.ToInt32(context.Request["contactID"]);
            var uploadOnly = Convert.ToBoolean(context.Request["uploadOnly"]);
            var tmpDirName = Convert.ToString(context.Request["tmpDirName"]);
            string photoUri;

            try
            {
                if (contactId != 0)
                {
                    photoUri = ContactPhotoManager.UploadPhoto(file.InputStream, contactId, uploadOnly);
                }
                else
                {
                    if (String.IsNullOrEmpty(tmpDirName))
                    {
                        tmpDirName = Guid.NewGuid().ToString();
                    }
                    photoUri = ContactPhotoManager.UploadPhoto(file.InputStream, tmpDirName);
                }

                fileUploadResult.Success = true;
                fileUploadResult.Data = photoUri;
            }
            catch (Exception e)
            {
                fileUploadResult.Success = false;
                fileUploadResult.Message = e.Message.HtmlEncode();
                return fileUploadResult;
            }

            var contact = Global.DaoFactory.GetContactDao().GetByID(contactId);
            if (contact != null)
            {
                var messageAction = contact is Company ? MessageAction.CompanyUpdatedPhoto : MessageAction.PersonUpdatedPhoto;
                MessageService.Send(context.Request, messageAction, contact.GetTitle());
            }

            return fileUploadResult;
        }
    }

}