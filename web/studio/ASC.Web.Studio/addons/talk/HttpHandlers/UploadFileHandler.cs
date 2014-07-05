/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

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
                var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "talk");
                var fileURL = storage.Save(Path.Combine(MD5Hash, fileName), file.InputStream).ToString();
                fileName = Path.GetFileName(fileURL);

                return new FileUploadResult
                    {
                        FileName = fileName,
                        Data = FileSizeComment.FilesSizeToString(file.InputStream.Length),
                        FileURL = CommonLinkUtility.GetFullAbsolutePath(fileURL),
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

        private static String MD5Hash
        {
            get
            {
                var data = MD5.Create().ComputeHash(Encoding.Default.GetBytes(SecurityContext.CurrentAccount.ID.ToString()));
                var sBuilder = new StringBuilder();

                for (Int32 i = 0, n = data.Length; i < n; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }
    }
}