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
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;


namespace ASC.Web.UserControls.WhiteLabel
{
    [ManagementControl(ManagementType.WhiteLabel, Location, SortOrder = 100)]
    public partial class WhiteLabel : UserControl
    {
        public const string Location = "~/UserControls/Management/WhiteLabel/WhiteLabel.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.Standalone || !TenantLogoManager.WhiteLabelEnabled)
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            Page.RegisterBodyScripts("~/js/uploader/ajaxupload.js",
                "~/UserControls/Management/WhiteLabel/js/whitelabel.js");

            RegisterScript();
        }

        private bool? isRetina;
        protected bool IsRetina
        {
            get
            {
                if (isRetina.HasValue) return isRetina.Value;
                isRetina = TenantLogoManager.IsRetina(Request);
                return isRetina.Value;
            }
            set
            {
                isRetina = value;
            }
        }
        protected string LogoText {
        get
            {
                var whiteLabelSettings = TenantWhiteLabelSettings.Load();

                return whiteLabelSettings.LogoText != null ? whiteLabelSettings.LogoText : TenantWhiteLabelSettings.DefaultLogoText;
            }
        }

        protected bool WhiteLabelEnabledForPaid = TenantLogoManager.WhiteLabelPaid;

        private void RegisterScript()
        {
            Page.RegisterInlineScript(@"jq('input.fileuploadinput').attr('accept', 'image/png,image/jpeg,image/gif');");

            Page.RegisterInlineScript(String.Format("jq(function () {{ WhiteLabelManager.Init(\"{0}\"); }});", WhiteLabelResource.SuccessfullySaveWhiteLabelSettingsMessage.HtmlEncode()));
        }
    }
}