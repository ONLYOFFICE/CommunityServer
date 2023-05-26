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
using System.Text;
using System.Web;
using System.Web.UI;

using AjaxPro;

using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.Management.SingleSignOnSettings
{
    //TODO: Remove this or re-write like in Control Panel? 

    [ManagementControl(ManagementType.SingleSignOnSettings, Location)]
    [AjaxNamespace("SsoSettingsController")]
    public partial class SingleSignOnSettings : UserControl
    {
        protected SsoSettingsV2 Settings { get; private set; }
        protected SsoSettingsV2 DefaultSettings { get; private set; }
        protected SsoMetadata Metadata { get; private set; }

        protected object Constants { get; private set; }
        protected string ErrorMessage { get; private set; }
        protected bool isSloPost { get; private set; }
        protected bool isSloRedirect { get; private set; }
        protected bool isSsoPost { get; private set; }
        protected bool isSsoRedirect { get; private set; }

        protected SsoNameIdFormatType SsoNameIdFormatTypes { get; private set; }

        protected const string Location = "~/UserControls/Management/SingleSignOnSettings/SingleSignOnSettings.ascx";
        protected bool isAvailable
        {
            get
            {
                return CoreContext.Configuration.Standalone || TenantExtra.GetTenantQuota().Sso;
            }
        }
        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }
            Page.RegisterStyle(
                "~/UserControls/Management/SingleSignOnSettings/css/singlesignonsettings.less");

            AjaxPro.Utility.RegisterTypeForAjax(typeof(SingleSignOnSettings), Page);
            Page.RegisterBodyScripts(
                "~/js/uploader/jquery.fileupload.js",
                "~/UserControls/Management/SingleSignOnSettings/js/singlesignonsettings.js"
                );

            try
            {
                Settings = SsoSettingsV2.Load();
                DefaultSettings = new SsoSettingsV2().GetDefault() as SsoSettingsV2;
                Constants = new
                {
                    SsoNameIdFormatType = new SsoNameIdFormatType(),
                    SsoBindingType = new SsoBindingType(),
                    SsoSigningAlgorithmType = new SsoSigningAlgorithmType(),
                    SsoEncryptAlgorithmType = new SsoEncryptAlgorithmType(),
                    SsoSpCertificateActionType = new SsoSpCertificateActionType(),
                    SsoIdpCertificateActionType = new SsoIdpCertificateActionType()
                };
                Metadata = new SsoMetadata();

                isSsoPost = (Settings.IdpSettings.SsoBinding == "" || Settings.IdpSettings.SsoBinding == SsoBindingType.Saml20HttpPost);
                isSsoRedirect = Settings.IdpSettings.SsoBinding == SsoBindingType.Saml20HttpRedirect;

                isSloPost = (Settings.IdpSettings.SloBinding == "" || Settings.IdpSettings.SloBinding == SsoBindingType.Saml20HttpPost);
                isSloRedirect = (Settings.IdpSettings.SloBinding == SsoBindingType.Saml20HttpRedirect);

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            ssoSpCertificatePopupContainer.Options.IsPopup = true;
            ssoIdpCertificatePopupContainer.Options.IsPopup = true;
            ssoSettingsInviteContainer.Options.IsPopup = true;
            ssoSettingsTurnOffContainer.Options.IsPopup = true;

            HelpLink = CommonLinkUtility.GetHelpLink();

            var script = string.Format("SsoSettings.init({0},{1},{2},{3},{4});",
                JsonConvert.SerializeObject(Settings),
                JsonConvert.SerializeObject(DefaultSettings),
                JsonConvert.SerializeObject(Constants),
                JsonConvert.SerializeObject(Metadata),
                JsonConvert.SerializeObject(ErrorMessage));

            if (isAvailable)
            {
                Page.RegisterInlineScript(script);
            }
        }

        protected string RenderSsoNameIdFormatSelector(bool enableSso, SsoSettingsV2 settings)
        {
            var sb = new StringBuilder();
            var currentNameIdFormat = settings.IdpSettings.NameIdFormat;
            sb.AppendFormat("<select id=\"ssoNameIdFormat\" class=\"comboBox\" data-default=\"{0}\"" + (enableSso ? "" : "disabled") + ">", currentNameIdFormat);

            var fields = typeof(SsoNameIdFormatType).GetFields();
            var instance = new SsoNameIdFormatType();
            foreach (var field in fields)
            {
                var value = field.GetValue(instance);
                sb.AppendFormat("<option " + (String.Equals(currentNameIdFormat, value) ? "selected" : "") + " value=\"{0}\">{1}</option>", value, value);
            }

            sb.Append("</select>");

            return sb.ToString();
        }

        protected string RenderSignVerifyingAlgorithmSelector(string id, SsoSettingsV2 settings)
        {
            var sb = new StringBuilder();
            var currentAlgorithm = id == "ssoDefaultSignVerifyingAlgorithm" ? settings.IdpCertificateAdvanced.VerifyAlgorithm : settings.SpCertificateAdvanced.SigningAlgorithm;
            sb.AppendFormat("<select id=\"" + id + "\" class=\"comboBox\" data-default=\"{0}\"" + (settings.EnableSso ? "" : "disabled") + ">", currentAlgorithm);

            var fields = typeof(SsoSigningAlgorithmType).GetFields();
            foreach (var field in fields)
            {
                var value = field.GetValue(typeof(SsoSigningAlgorithmType));
                var name = value.ToString().Split('#')[1];
                sb.AppendFormat("<option " + (String.Equals(currentAlgorithm, value) ? "selected" : "") + " value=\"{0}\">{1}</option>", value, name);
            }

            sb.Append("</select>");

            return sb.ToString();
        }

        protected string RenderEncryptAlgorithmTypeSelector(string id, SsoSettingsV2 settings)
        {
            var sb = new StringBuilder();
            var currentAlgorithm = id == "ssoEncryptAlgorithm" ? settings.SpCertificateAdvanced.EncryptAlgorithm : settings.SpCertificateAdvanced.DecryptAlgorithm;

            sb.AppendFormat("<select id=\"" + id + "\" class=\"comboBox\" data-default=\"{0}\"" + (settings.EnableSso ? "" : "disabled") + ">", currentAlgorithm);

            var fields = typeof(SsoEncryptAlgorithmType).GetFields();
            foreach (var field in fields)
            {
                var value = field.GetValue(typeof(SsoEncryptAlgorithmType));
                var name = value.ToString().Split('#')[1];
                sb.AppendFormat("<option " + (String.Equals(currentAlgorithm, value) ? "selected" : "") + " value=\"{0}\">{1}</option>", value, name);
            }

            sb.Append("</select>");

            return sb.ToString();
        }



    }
}