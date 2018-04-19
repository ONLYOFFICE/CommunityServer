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
using System.Collections.Generic;
using System.Linq;

using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
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

            IsProjectAdmin = ProjectSecurity.CurrentUserAdministrator;
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
                CreateDocsHolder.Controls.Add(LoadControl(Files.Controls.CreateMenu.Location));
                RenderFolderTree();
            }

            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
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