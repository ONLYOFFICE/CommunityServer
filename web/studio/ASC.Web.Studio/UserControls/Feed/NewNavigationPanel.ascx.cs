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
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Common.InviteLink;

namespace ASC.Web.Studio.UserControls.Feed
{
    public partial class NewNavigationPanel : UserControl
    {
        protected bool IsProductAvailable(string product)
        {
            switch (product)
            {
                case "community":
                    return WebItemSecurity.IsAvailableForMe(WebItemManager.CommunityProductID);
                case "crm":
                    return WebItemSecurity.IsAvailableForMe(WebItemManager.CRMProductID);
                case "projects":
                    return WebItemSecurity.IsAvailableForMe(WebItemManager.ProjectsProductID);
                case "documents":
                    return WebItemSecurity.IsAvailableForMe(WebItemManager.DocumentsProductID);
                default:
                    return false;
            }
        }

        public static string Location
        {
            get { return "~/UserControls/Feed/NewNavigationPanel.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }
    }
}