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
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using ASC.Core.Users;
using System.Web;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class InvitePanel : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/InvitePanel/InvitePanel.ascx"; }
        }

        protected bool EnableInviteLink = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

        protected string GeneratedUserLink;
        protected string GeneratedVisitorLink;


        protected void Page_Load(object sender, EventArgs e)
        {
            // Move to CommonBodyScripts.ascx.cs
            //    Page.RegisterBodyScripts("~/UserControls/Management/InvitePanel/js/invitepanel.js")
            Page.RegisterStyle("~/UserControls/Management/InvitePanel/css/invitepanel.less");

            GeneratedUserLink = GenerateLink(EmployeeType.User);
            GeneratedVisitorLink = GenerateLink(EmployeeType.Visitor);
        }

        public static string GenerateLink(EmployeeType employeeType)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()
                || WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID))
                return CommonLinkUtility.GetConfirmationUrl(string.Empty, ConfirmType.LinkInvite, (int)employeeType, SecurityContext.CurrentAccount.ID)
                       + String.Format("&emplType={0}", (int)employeeType);
            return null;
        }
    }
}