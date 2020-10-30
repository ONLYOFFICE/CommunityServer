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


using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.People.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using System;
using System.Linq;
using System.Web.UI;


namespace ASC.Web.People.UserControls
{
    public partial class SideButtonsPanel : UserControl
    {
        protected bool HasPendingProfiles;
        protected bool EnableAddUsers;
        protected bool CurrentUserFullAdmin;
        protected bool CurrentUserAdmin;

        public static string Location
        {
            get { return PeopleProduct.ProductPath + "UserControls/SideButtonsPanel.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitData();
        }

        private void InitData()
        {
            HasPendingProfiles = CoreContext.UserManager.GetUsers().Any(u => u.ActivationStatus == EmployeeActivationStatus.Pending);
            EnableAddUsers =  TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;
            CurrentUserFullAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
            CurrentUserAdmin = CurrentUserFullAdmin || WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
        }
    }
}