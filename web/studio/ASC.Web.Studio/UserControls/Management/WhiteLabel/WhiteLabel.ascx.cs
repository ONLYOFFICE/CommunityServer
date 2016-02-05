/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/
using System;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio;
using ASC.Web.Studio.PublicResources;


namespace ASC.Web.UserControls.WhiteLabel
{
    [ManagementControl(ManagementType.WhiteLabel, Location, SortOrder = 100)]
    public partial class WhiteLabel : UserControl
    {
        public const string Location = "~/UserControls/Management/WhiteLabel/WhiteLabel.ascx";

        public static bool AvailableControl
        {
            get
            {
                return TenantLogoManager.WhiteLabelEnabled && !CoreContext.Configuration.Standalone;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AvailableControl)
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            Page.RegisterBodyScripts("~/js/uploader/ajaxupload.js");
            Page.RegisterBodyScripts("~/usercontrols/management/whitelabel/js/whitelabel.js");

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
                var whiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);

                return whiteLabelSettings.LogoText != null ? whiteLabelSettings.LogoText : TenantWhiteLabelSettings.DefaultLogo;
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