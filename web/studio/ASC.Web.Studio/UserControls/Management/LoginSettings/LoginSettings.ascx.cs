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
using ASC.Web.Studio.Masters.MasterManagement;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 201)]
    public partial class LoginSettings : UserControl 
    {
        protected Web.Core.Utility.LoginSettings Settings { get; set; }

        public const string Location = "~/UserControls/Management/LoginSettings/LoginSettings.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/LoginSettings/js/loginsettings.js")
                .RegisterStyle("~/UserControls/Management/LoginSettings/css/loginsettings.less");

            Settings = Web.Core.Utility.LoginSettings.Load();
        }
    }
}