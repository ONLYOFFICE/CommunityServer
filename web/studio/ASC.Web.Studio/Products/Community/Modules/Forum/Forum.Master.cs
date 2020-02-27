/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            sb.Append(" ForumMakerProvider.All='" + Resources.ForumResource.All + "'; ");
            sb.Append(" ForumMakerProvider.ConfirmMessage='" + Resources.ForumResource.ConfirmMessage + "'; ");
            sb.Append(" ForumMakerProvider.SaveButton='" + Resources.ForumResource.SaveButton + "'; ");
            sb.Append(" ForumMakerProvider.CancelButton='" + Resources.ForumResource.CancelButton + "'; ");
            sb.Append(" ForumMakerProvider.NameEmptyString='" + Resources.ForumResource.NameEmptyString + "'; ");
            sb.Append(" ForumContainer_PanelInfoID = '" + ForumContainer.GetInfoPanelClientID() + "'; ");

            Page.RegisterBodyScripts(ForumManager.BaseVirtualPath + "/js/forummaker.js")
                .RegisterStyle(ForumManager.BaseVirtualPath + "/App_Themes/default/style.css")
                .RegisterInlineScript(sb.ToString());

            SearchText = "";

            if (!String.IsNullOrEmpty(Request["search"]))
                SearchText = Request["search"];
        }
    }
}