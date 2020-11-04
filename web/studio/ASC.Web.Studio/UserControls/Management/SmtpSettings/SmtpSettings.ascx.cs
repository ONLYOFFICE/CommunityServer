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
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Newtonsoft.Json;
using SmtpSettingsConfig = ASC.Core.Configuration.SmtpSettings;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.SmtpSettings, Location)]
    [AjaxNamespace("SmtpSettings")]
    public partial class SmtpSettings : UserControl
    {
        // ReSharper disable once InconsistentNaming
        public const string Location = "~/UserControls/Management/SmtpSettings/SmtpSettings.ascx";

        private SmtpSettingsConfig current;
        protected SmtpSettingsConfig CurrentSmtpSettings
        {
            get
            {
                if (current != null)
                {
                    return current;
                }

                current = CoreContext.Configuration.SmtpSettings;

                if (current.IsDefaultSettings && !CoreContext.Configuration.Standalone)
                {
                    current = SmtpSettingsConfig.Empty;
                }

                return current;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.SmtpSettings.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl, "~/UserControls/Management/SmtpSettings/js/smtpsettings.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(),
                "smtpsettings_style",
                string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\">",
                    WebPath.GetPath("UserControls/Management/SmtpSettings/css/smtpsettings.css")), false);
            Page.RegisterInlineScript(GetSmtpSettingsInitInlineScript(), true, false);
        }

        public bool IsMailServerAvailable
        {
            get { return SetupInfo.IsVisibleSettings("AdministrationPage") && SetupInfo.IsVisibleSettings("SmtpSettingsWithsMailServer") && CoreContext.Configuration.Standalone; }
        }

        protected string GetSmtpSettingsInitInlineScript()
        {
            var sbScript = new StringBuilder();
            sbScript.AppendLine(
                "\r\nif (typeof (window.SmtpSettingsConstants) === 'undefined') { window.SmtpSettingsConstants = {};}")
                .AppendFormat("window.SmtpSettingsConstants.IsMailServerAvailable = {0};\r\n",
                    JsonConvert.SerializeObject(IsMailServerAvailable));

            return sbScript.ToString();
        }
    }
}