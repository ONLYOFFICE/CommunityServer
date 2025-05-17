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
using System.Web.UI;

using ASC.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Masters.MasterManagement;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management.ImpersonateUser
{
    [ManagementControl(ManagementType.AccessRights, Location, SortOrder = 100)]
    public partial class ImpersonateSettings : UserControl
    {
        protected bool Available { get; private set; }

        public const string Location = "~/UserControls/Management/ImpersonateUser/ImpersonateSettings.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            Available = ImpersonationSettings.Available && SecurityContext.CurrentAccount.ID == CoreContext.TenantManager.GetCurrentTenant().OwnerId;

            if (!Available) return;

            Page.RegisterBodyScripts("~/UserControls/Management/ImpersonateUser/js/impersonatesettings.js")
                .RegisterStyle("~/UserControls/Management/ImpersonateUser/css/impersonatesettings.less");
        }
    }
}