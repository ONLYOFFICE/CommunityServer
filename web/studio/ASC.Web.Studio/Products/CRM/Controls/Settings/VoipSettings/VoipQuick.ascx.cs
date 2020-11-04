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
using ASC.CRM.Core;
using ASC.VoipService;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using Global = ASC.Web.CRM.Classes.Global;
using StudioResources = Resources;

namespace ASC.Web.CRM.Controls.Settings
{
    public class VoipUploadHandler : IFileUploadHandler
    {
        private const long maxFileSize = 5;

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (!VoipNumberData.Allowed || !CRMSecurity.IsAdmin)
                throw CRMSecurity.CreateSecurityException();

            if (context.Request.Files.Count == 0)
            {
                return Error("No files.");
            }

            var file = context.Request.Files[0];

            if (file.ContentLength <= 0 || file.ContentLength > maxFileSize * 1024L * 1024L)
            {
                return Error(StudioResources.Resource.FileSizeMaxExceed);
            }

            try
            {
                var path = file.FileName;

                AudioType audioType;
                if (Enum.TryParse(context.Request["audioType"], true, out audioType))
                {
                    path = Path.Combine(audioType.ToString().ToLower(), path);
                }

                var uri = Global.GetStore().Save("voip", path, file.InputStream, MimeMapping.GetMimeMapping(file.FileName), ContentDispositionUtil.GetHeaderValue(file.FileName, withoutBase: true));
                return Success(new VoipUpload
                    {
                        AudioType = audioType,
                        Name = file.FileName,
                        Path = CommonLinkUtility.GetFullAbsolutePath(uri.ToString())
                    });
            }
            catch(Exception error)
            {
                return Error(error.Message);
            }
        }

        private static FileUploadResult Success(VoipUpload file)
        {
            return new FileUploadResult
                {
                    Success = true,
                    Data = file
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
    }

    public partial class VoipQuick : BaseUserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/VoIPSettings/VoipQuick.ascx"); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!VoipNumberData.Allowed || !CRMSecurity.IsAdmin)
            {
                Response.Redirect(PathProvider.StartURL() + "Settings.aspx?type=voip.common");
            }

            buyNumberContainer.Options.IsPopup = true;
            linkNumberContainer.Options.IsPopup = true;
            deleteNumberContainer.Options.IsPopup = true;
            Page.RegisterBodyScripts("~/js/asc/core/voip.countries.js");
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("voip.quick.js"));
        }
    }
}