/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Core;
using ASC.Core.Users;
using ASC.Web.People.Classes;
using ASC.Web.People.Core;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;


namespace ASC.Web.People.UserControls
{
    public partial class SideNavigationPanel : UserControl
    {
        protected List<UserInfo> Profiles;
        protected List<MyGroup> Groups;

        protected bool HasPendingProfiles;
        protected bool EnableAddUsers;
        protected bool CurrentUserAdmin;

        public static string Location
        {
            get { return PeopleProduct.ProductPath + "UserControls/SideNavigationPanel.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitData();

            Page.RegisterBodyScripts(ResolveUrl("~/products/people/js/sideNavigationPanel.js"));

            GroupRepeater.DataSource = Groups;
            GroupRepeater.DataBind();

            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }

        private void InitData()
        {
            Groups = CoreContext.UserManager.GetDepartments().Select(r => new MyGroup(r)).ToList();
            Groups.Sort((group1, group2) => String.Compare(group1.Title, group2.Title, StringComparison.Ordinal));

            Profiles = CoreContext.UserManager.GetUsers().ToList();

            HasPendingProfiles = Profiles.FindAll(u => u.ActivationStatus == EmployeeActivationStatus.Pending).Count > 0;
            EnableAddUsers =  TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;
            CurrentUserAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
        }
    }
}