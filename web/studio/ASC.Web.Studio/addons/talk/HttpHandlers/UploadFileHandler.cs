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


using System.Globalization;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System;
using System.IO;
using System.Linq;
using System.Web;
using ASC.Web.Talk.Addon;

namespace ASC.Web.Talk.HttpHandlers
{
    public class UploadFileHandler : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            try
            {
                if (context.Request.Files.Count == 0)
                {
                    throw new Exception("there is no file");
                }

                var file = context.Request.Files[0];

                if (file.ContentLength > SetupInfo.MaxImageUploadSize)
                {
                    throw FileSizeComment.FileImageSizeException;
                }

                var fileName = file.FileName.Replace("~", "-");
                var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(CultureInfo.InvariantCulture), "talk");
                var md5Hash = TalkSpaceUsageStatManager.GetUserMd5Hash(SecurityContext.CurrentAccount.ID);
                var fileUrl = storage.Save(Path.Combine(md5Hash, GenerateRandomString(), fileName), file.InputStream).ToString();
                fileName = Path.GetFileName(fileUrl);

                return new FileUploadResult
                    {
                        FileName = fileName,
                        Data = FileSizeComment.FilesSizeToString(file.InputStream.Length),
                        FileURL = CommonLinkUtility.GetFullAbsolutePath(fileUrl),
                        Success = true
                    };
            }
            catch (Exception ex)
            {
                return new FileUploadResult
                    {
                        Success = false,
                        Message = ex.Message
                    };
            }
        }

        private string GenerateRandomString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}