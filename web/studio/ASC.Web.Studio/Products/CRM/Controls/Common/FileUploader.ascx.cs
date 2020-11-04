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
using System.Text;
using System.Web;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;

namespace ASC.Web.CRM.Controls.Common
{
    public partial class FileUploader : BaseUserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Common/FileUploader.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/js/uploader/jquery.fileupload.js");
            Page.RegisterBodyScripts("~/js/uploader/jquery.fileuploadmanager.js");

            RegisterScript();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(" var uploadFileSizeLimit = \"{0}\"; ", SetupInfo.MaxUploadSize);
            Page.RegisterInlineScript(sb.ToString(), onReady: false);
        }
    }
}