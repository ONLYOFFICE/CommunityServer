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
using ASC.Web.Projects.Classes;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using Resources;

namespace ASC.Web.Projects.Controls.Common
{
    public partial class ProjectDocumentsPopup : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Common/ProjectDocumentsPopup.ascx"); }
        }

        protected string FrameUrl;

        protected void Page_Load(object sender, EventArgs e)
        {
            _documentUploader.Options.IsPopup = true;
            InitScripts();
        }

        private void InitScripts()
        {
            FrameUrl = FileChoice.GetUrl(filterType: FilterType.FilesOnly, root: FolderType.Projects, multiple: true, successButton: UserControlsCommonResource.AttachFiles);

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            Page.RegisterStyle("~/Products/Projects/App_Themes/default/css/projectdocumentspopup.less")
                .RegisterBodyScripts("~/Products/Projects/js/projectdocumentspopup.js");
        }
    }
}