/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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