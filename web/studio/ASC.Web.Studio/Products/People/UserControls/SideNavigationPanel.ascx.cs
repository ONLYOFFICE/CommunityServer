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
using System.Linq;
using System.Web;
using System.Web.UI;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.People.Core;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;


namespace ASC.Web.People.UserControls
{
    public partial class SideNavigationPanel : UserControl
    {
        protected bool HasPendingProfiles;
        protected bool EnableAddUsers;
        protected bool CurrentUserFullAdmin;
        protected bool CurrentUserAdmin;
        protected bool EnableAddVisitors;
        protected string CurrentPage { get; set; }
        protected bool IsBirthdaysAvailable { get; set; }

        public static string Location
        {
            get { return PeopleProduct.ProductPath + "UserControls/SideNavigationPanel.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitData();
            InitPermission();
            InitCurrentPage();

            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }

        private void InitData()
        {
            HasPendingProfiles = CoreContext.UserManager.GetUsers().Any(u => u.ActivationStatus == EmployeeActivationStatus.Pending);
            EnableAddUsers = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;
            CurrentUserFullAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
            CurrentUserAdmin = CurrentUserFullAdmin || WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
            EnableAddVisitors = CoreContext.Configuration.Standalone || TenantStatisticsProvider.GetVisitorsCount() < TenantExtra.GetTenantQuota().ActiveUsers * Constants.CoefficientOfVisitors;
        }

        private void InitPermission()
        {
            IsBirthdaysAvailable = WebItemManager.Instance.GetSubItems(PeopleProduct.ID).Any(item => item.ID == WebItemManager.BirthdaysProductID);
        }

        private void InitCurrentPage()
        {
            var currentPath = HttpContext.Current.Request.Path;
            if (currentPath.IndexOf("Default.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "people";
            }
            else if (currentPath.IndexOf("Birthdays.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "birthdays";
            }
            else if (currentPath.IndexOf("Help.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "help";
            }
            else if (currentPath.IndexOf("CardDavSettings.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "carddav";
            }
            else
            {
                CurrentPage = "people";
            }
        }
    }
}