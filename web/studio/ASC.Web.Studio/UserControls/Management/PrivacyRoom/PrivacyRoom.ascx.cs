/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PrivacyRoom, Location)]
    public partial class PrivacyRoom : UserControl
    {
        public const string Location = "~/UserControls/Management/PrivacyRoom/PrivacyRoom.ascx";

        protected bool EncryptionEnabled;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.PrivacyRoom.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            Page.RegisterBodyScripts("~/UserControls/Management/PrivacyRoom/js/privacyroom.js")
                .RegisterStyle("~/UserControls/Management/PrivacyRoom/css/privacyroom.less");

            EncryptionEnabled = PrivacyRoomSettings.Enabled;
        }
    }
}