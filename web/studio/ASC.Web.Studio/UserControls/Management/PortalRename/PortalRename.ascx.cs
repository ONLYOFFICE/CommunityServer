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
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Core;
using System.Web;
using System.Configuration;
using ASC.Web.Studio.Core;
using System.Web.UI;


namespace ASC.Web.Studio.UserControls.Management
{
    public partial class PortalRename : UserControl
    {
        public const string Location = "~/UserControls/Management/PortalRename/PortalRename.ascx";

        protected bool Enabled;

        protected string CurrentTenantAlias {
            get {
                return CoreContext.TenantManager.GetCurrentTenant().TenantAlias;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/PortalRename/js/portalrename.js")
                .RegisterStyle("~/UserControls/Management/PortalRename/css/portalrename.less");

            Enabled = SetupInfo.IsVisibleSettings("PortalRename");
        }
    }
}