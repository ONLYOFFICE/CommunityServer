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
using System.Runtime.Remoting.Messaging;
using System.Web;
using ASC.Files.Core;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;

namespace ASC.Web.CRM.Classes
{
    public class FileUploaderHandler : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var fileUploadResult = new FileUploadResult();

            if (!FileToUpload.HasFilesToUpload(context)) return fileUploadResult;

            var file = new FileToUpload(context);

            if (String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                throw new InvalidOperationException("Invalid file.");

            if (0 < SetupInfo.MaxUploadSize && SetupInfo.MaxUploadSize < file.ContentLength)
                throw FileSizeComment.FileSizeException;

            if (CallContext.GetData("CURRENT_ACCOUNT") == null)
                CallContext.SetData("CURRENT_ACCOUNT", new Guid(context.Request["UserID"]));


            var fileName = file.FileName.LastIndexOf('\\') != -1
                               ? file.FileName.Substring(file.FileName.LastIndexOf('\\') + 1)
                               : file.FileName;

            var document = new File
                {
                    Title = fileName,
                    FolderID = Global.DaoFactory.GetFileDao().GetRoot(),
                    ContentLength = file.ContentLength
                };

            document = Global.DaoFactory.GetFileDao().SaveFile(document, file.InputStream);

            fileUploadResult.Data = document.ID;
            fileUploadResult.FileName = document.Title;
            fileUploadResult.FileURL = document.FileDownloadUrl;


            fileUploadResult.Success = true;


            return fileUploadResult;
        }
    }
}