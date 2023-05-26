/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web.UI;
using System.Web.UI.WebControls;

using ASC.Web.Community.Modules.Forum.Resources;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;

namespace ASC.Web.Community.Forum
{
    public partial class ForumMasterPage : MasterPage
    {
        public string SearchText { get; set; }

        public PlaceHolder ActionsPlaceHolder { get; set; }

        public string CurrentPageCaption
        {
            get { return ForumContainer.CurrentPageCaption; }
            set { ForumContainer.CurrentPageCaption = value; }
        }

        public ForumMasterPage()
        {
            ActionsPlaceHolder = new PlaceHolder();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _scriptProvider.SettingsID = ForumManager.Settings.ID;
            if (Page is NewPost || Page is EditTopic)
                _scriptProvider.RegistrySearchHelper = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumContainer.Options.InfoType = InfoType.Alert;
            var sb = new StringBuilder();
            sb.Append(" ForumMakerProvider.All='" + ForumResource.All + "'; ");
            sb.Append(" ForumMakerProvider.ConfirmMessage='" + ForumResource.ConfirmMessage + "'; ");
            sb.Append(" ForumMakerProvider.SaveButton='" + ForumResource.SaveButton + "'; ");
            sb.Append(" ForumMakerProvider.CancelButton='" + ForumResource.CancelButton + "'; ");
            sb.Append(" ForumMakerProvider.NameEmptyString='" + ForumResource.NameEmptyString + "'; ");
            sb.Append(" ForumContainer_PanelInfoID = '" + ForumContainer.GetInfoPanelClientID() + "'; ");

            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/Products/Community/App_Themes/dark/dark-style.less");
            }
            else
            {
                Page.RegisterStyle(ForumManager.BaseVirtualPath + "/App_Themes/default/style.less");
            }
            Page.RegisterBodyScripts(ForumManager.BaseVirtualPath + "/js/forummaker.js")
                .RegisterInlineScript(sb.ToString());

            SearchText = "";

            if (!String.IsNullOrEmpty(Request["search"]))
                SearchText = Request["search"];
        }
    }
}