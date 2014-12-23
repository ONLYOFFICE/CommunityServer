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
using System.Web;
using ASC.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Backup
{
    internal class BackupFileUploadHandler : IFileUploadHandler
    {
        private const long MaxBackupFileSize = 1024L*1024L*1024L;

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (context.Request.Files.Count == 0)
            {
                return Error("No files.");
            }

            if (!TenantExtra.GetTenantQuota().HasBackup)
            {
                return Error("Backup not allowed.");
            }

            if (!SecurityContext.CheckPermissions(SecutiryConstants.EditPortalSettings))
            {
                return Error("Access denied.");
            }

            HttpPostedFile file = context.Request.Files[0];

            if (file.ContentLength <= 0 || file.ContentLength > MaxBackupFileSize)
            {
                return Error("File size must be greater than 0 and less than {0} bytes", MaxBackupFileSize);
            }

            
            try
            {
                var filePath = GetFilePath();
                file.SaveAs(filePath);
                return Success(filePath);
            }
            catch (Exception error)
            {
                return Error(error.Message);
            }
        }

        private static FileUploadResult Success(string filePath)
        {
            return new FileUploadResult
                {
                    Success = true,
                    Data = filePath
                };
        }

        private static FileUploadResult Error(string messageFormat, params object[] args)
        {
            return new FileUploadResult
                {
                    Success = false,
                    Message = string.Format(messageFormat, args)
                };
        }

        private static string GetFilePath()
        {
            return Path.GetTempFileName();
        }
    }
}