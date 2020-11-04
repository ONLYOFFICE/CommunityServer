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


using ASC.Web.Files.Classes;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using System;
using System.Web.UI;

namespace ASC.Web.Files.Controls
{
    public partial class MainMenu : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("MainMenu/MainMenu.ascx"); }
        }

        public bool EnableThirdParty;
        public bool Desktop;

        protected void Page_Load(object sender, EventArgs e)
        {
            ControlHolder.Controls.Add(LoadControl(TreeBuilder.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));

            if (!Desktop)
            {
                var helpCenter = (HelpCenter) LoadControl(HelpCenter.Location);
                helpCenter.IsSideBar = true;
                sideHelpCenter.Controls.Add(helpCenter);

                sideSupport.Controls.Add(LoadControl(Support.Location));
                UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            }
        }
    }
}