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