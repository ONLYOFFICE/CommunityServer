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
using System.Collections.Generic;
using System.Linq;

using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Common.InviteLink;


namespace ASC.Web.Projects.Controls.Common
{
    public partial class NavigationSidePanel : BaseUserControl
    {
        public Project Project { get { return Page.Project; } }

        public List<Project> MyProjects { get; set; }

        protected bool ShowCreateButton { get; set; }

        protected bool IsProjectAdmin { get; set; }

        protected bool IsFullAdmin { get; set; }

        protected bool IsOutsider { get; set; }

        protected bool DisplayAppsBanner;

        protected void Page_Load(object sender, EventArgs e)
        {
            MyProjects = Page.RequestContext.CurrentUserProjects.ToList();

            InitControls();

            IsProjectAdmin = Page.ProjectSecurity.CurrentUserAdministrator;
            IsFullAdmin = Page.Participant.IsFullAdmin;
            IsOutsider = Page.Participant.UserInfo.IsOutsider();
            ShowCreateButton = !Page.Participant.UserInfo.IsOutsider();


            var mobileAppRegistrator = new CachedMobileAppInstallRegistrator(new MobileAppInstallRegistrator());
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var isRegistered = mobileAppRegistrator.IsInstallRegistered(currentUser.Email, null);

            DisplayAppsBanner =
                SetupInfo.DisplayMobappBanner("projects")
                && !CoreContext.Configuration.Standalone
                && !isRegistered;
        }

        private void InitControls()
        {
            if (Page is TMDocs)
            {
                RenderFolderTree();
            }

            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }

        private void RenderFolderTree()
        {
            var tree = (Files.Controls.TreeBuilder) LoadControl(Files.Controls.TreeBuilder.Location);
            tree.FolderIDCurrentRoot = Files.Classes.Global.FolderProjects;
            placeHolderFolderTree.Controls.Add(tree);
        }
    }
}