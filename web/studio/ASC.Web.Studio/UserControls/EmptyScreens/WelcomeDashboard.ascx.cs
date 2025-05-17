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
using System.Web;
using System.Web.UI;

using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Masters.MasterManagement;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.EmptyScreens
{
    public partial class WelcomeDashboard : UserControl
    {
        public static string Location
        {
            get { return VirtualPathUtility.ToAbsolute("~/UserControls/EmptyScreens/WelcomeDashboard.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/EmptyScreens/js/dashboard.js", "~/js/third-party/slick.min.js");

            var collaboratorPopupSettings = CollaboratorSettings.LoadForCurrentUser();
            collaboratorPopupSettings.FirstVisit = false;
            collaboratorPopupSettings.SaveForCurrentUser();

            var quota = TenantExtra.GetTenantQuota();
            var isAdministrator = CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID);
            var showDemonstration = !CoreContext.Configuration.Personal && !CoreContext.Configuration.CustomMode && !CoreContext.Configuration.Standalone && quota.Trial;

            ProductDemo = !string.IsNullOrEmpty(SetupInfo.DemoOrder) && isAdministrator && showDemonstration;
        }

        protected bool ProductDemo;

    }
}