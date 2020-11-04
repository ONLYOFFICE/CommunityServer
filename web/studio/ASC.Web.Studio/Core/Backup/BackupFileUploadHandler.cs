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
        private const long MaxBackupFileSize = 1024L * 1024L * 1024L;

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (context.Request.Files.Count == 0)
            {
                return Error("No files.");
            }

            if (BackupHelper.ExceedsMaxAvailableSize(TenantProvider.CurrentTenantID))
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