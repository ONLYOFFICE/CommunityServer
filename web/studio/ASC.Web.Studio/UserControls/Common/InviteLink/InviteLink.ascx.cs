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

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;


namespace ASC.Web.Studio.UserControls.Common.InviteLink
{
    public partial class InviteLink : System.Web.UI.UserControl
    {
        protected string LinkText;
        protected bool EnableAddUsers;
        protected bool EnableAddVisitors;

        public static string Location
        {
            get { return "~/UserControls/Common/InviteLink/InviteLink.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LinkText = CustomNamingPeople.Substitute<Resource>("InviteUsersToPortalLink").HtmlEncode();
            if (CoreContext.Configuration.Personal)
            {
                EnableAddUsers = true;
                EnableAddVisitors = true;
            }
            else
            {
                EnableAddUsers = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;
                EnableAddVisitors = CoreContext.Configuration.Standalone || TenantStatisticsProvider.GetVisitorsCount() < TenantExtra.GetTenantQuota().ActiveUsers * Constants.CoefficientOfVisitors;
            }
        }
    }
}