/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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