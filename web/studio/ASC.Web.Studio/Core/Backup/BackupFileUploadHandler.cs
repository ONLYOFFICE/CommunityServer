/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

            if (BackupHelper.ExceedsMaxAvailableSize)
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