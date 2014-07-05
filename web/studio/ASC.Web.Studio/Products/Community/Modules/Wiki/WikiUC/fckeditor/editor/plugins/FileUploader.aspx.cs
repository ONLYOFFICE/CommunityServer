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
using System.Web.UI;
using ASC.Web.Studio.Core;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Data.Storage;
using System.IO;
using ASC.Core;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki.UC
{
    public class FileUploadResult
    {
        public FileUploadResult()
        {
            ErrorText = string.Empty;
            WebPath = string.Empty;
            LocalPath = string.Empty;
        }

        public string WebPath { get; set; }
        public string LocalPath { get; set; }
        public string ErrorText { get; set; }
    }

    public partial class FileUploader : Page
    {
        public static long MaxUploadSize
        {
            get { return SetupInfo.MaxUploadSize; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            var result = new FileUploadResult();
            if (Request.Files.Count > 0 && !string.IsNullOrEmpty(Request["hfUserID"]))
            {
                try
                {
                    //string uploadedUserName;
                    var content = new byte[Request.Files[0].ContentLength];

                    if (content.Length > MaxUploadSize && MaxUploadSize > 0)
                    {
                        result.ErrorText = WikiUCResource.wikiErrorFileSizeLimitText;
                    }
                    else
                    {
                        Request.Files[0].InputStream.Read(content, 0, Request.Files[0].ContentLength);
                        string localPath;
                        result.WebPath = TempFileContentSave(content, out localPath);
                        result.LocalPath = localPath;
                    }


                    Response.StatusCode = 200;
                    Response.Write(AjaxPro.JavaScriptSerializer.Serialize(result));
                }
                catch (Exception)
                {
                }
            }
            Response.End();
        }

        private static string TempFileContentSave(byte[] fileContent, out string filaLocation)
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString();
            var storage = StorageFactory.GetStorage(tenantId, WikiSection.Section.DataStorage.ModuleName);
            string result;

            using (var ms = new MemoryStream(fileContent))
            {
                result = storage.SaveTemp(WikiSection.Section.DataStorage.TempDomain, out filaLocation, ms).ToString();
            }

            return result;
        }
    }
}