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
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location)]
    [AjaxNamespace("PasswordSettingsController")]
    public partial class PasswordSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/PasswordSettings/PasswordSettings.ascx";

        protected bool Enabled
        {
            get { return SetupInfo.IsVisibleSettings("PasswordSettings"); }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Enabled) return;

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/PasswordSettings/js/passwordsettings.js")
                .RegisterStyle("~/UserControls/Management/PasswordSettings/css/passwordsettings.less");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        [AjaxMethod]
        public object SavePasswordSettings(string objData)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var passwordSettingsObj = jsSerializer.Deserialize<Web.Core.Utility.PasswordSettings>(objData);
                passwordSettingsObj.Save();

                MessageService.Send(HttpContext.Current.Request, MessageAction.PasswordStrengthSettingsUpdated);

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

        [AjaxMethod]
        public string LoadPasswordSettings()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            var passwordSettingsObj = Web.Core.Utility.PasswordSettings.Load();

            return serializer.Serialize(passwordSettingsObj);
        }
    }
}