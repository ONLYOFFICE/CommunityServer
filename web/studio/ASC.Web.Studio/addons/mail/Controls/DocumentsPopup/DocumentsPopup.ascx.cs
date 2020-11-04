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
using System.Web;
using System.Web.UI;
using ASC.Files.Core;
using ASC.Web.Files;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using Resources;

namespace ASC.Web.Mail.Controls
{
    public partial class DocumentsPopup : UserControl
    {
        public static string Location
        {
            get { return "~/addons/mail/Controls/DocumentsPopup/DocumentsPopup.ascx"; }
        }

        protected string FrameUrl;

        private void InitScripts()
        {
            Page.RegisterStyle("~/addons/mail/Controls/DocumentsPopup/css/documentspopup.less")
                .RegisterBodyScripts("~/addons/mail/Controls/DocumentsPopup/js/documentspopup.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            FrameUrl = FileChoice.GetUrl(filterType: FilterType.FilesOnly, multiple: true, successButton: UserControlsCommonResource.AttachFiles);

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            _documentUploader.Options.IsPopup = true;
            InitScripts();
        }
    }
}