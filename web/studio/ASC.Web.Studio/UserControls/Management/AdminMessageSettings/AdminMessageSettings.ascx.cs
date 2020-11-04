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
using ASC.Core.Common.Settings;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 300)]
    [AjaxNamespace("AdmMessController")]
    public partial class AdminMessageSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/AdminMessageSettings/AdminMessageSettings.ascx";

        protected StudioAdminMessageSettings _studioAdmMessNotifSettings;

        protected bool Enabled;

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts("~/UserControls/Management/AdminMessageSettings/js/admmess.js")
                .RegisterStyle("~/UserControls/Management/AdminMessageSettings/css/admmess.less");

            _studioAdmMessNotifSettings = StudioAdminMessageSettings.Load();

            Enabled = !TenantAccessSettings.Load().Anyone;

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        [AjaxMethod]
        public object SaveSettings(bool turnOn)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                new StudioAdminMessageSettings {Enable = turnOn}.Save();

                MessageService.Send(HttpContext.Current.Request, MessageAction.AdministratorMessageSettingsUpdated);

                return new
                    {
                        Status = 1,
                        Message = Resources.Resource.SuccessfullySaveSettingsMessage
                    };
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}