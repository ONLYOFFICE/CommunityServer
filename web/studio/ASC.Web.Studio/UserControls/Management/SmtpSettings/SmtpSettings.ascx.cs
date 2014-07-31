/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using SmtpSettingsConfig = ASC.Core.Configuration.SmtpSettings;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ThirdPartyAuthorization, Location, SortOrder = 50)]
    [AjaxNamespace("SmtpSettings")]
    public partial class SmtpSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/SmtpSettings/SmtpSettings.ascx";

        protected SmtpSettingsModel CurrentSmtpSettings
        {
            get
            {
                return !CoreContext.Configuration.SmtpSettings.IsDefaultSettings
                           ? ToSmtpSettingsModel(CoreContext.Configuration.SmtpSettings)
                           : new SmtpSettingsModel();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/smtpsettings/js/smtpsettings.js"));
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "smtpsettings_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("usercontrols/management/smtpsettings/css/smtpsettings.css") + "\">", false);
        }

        [AjaxMethod]
        public void Save(SmtpSettingsModel settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            CoreContext.Configuration.SmtpSettings = ToSmtpSettingsConfig(settings);
        }

        [AjaxMethod]
        public void Test(SmtpSettingsModel settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var config = ToSmtpSettingsConfig(settings);

            using (var smtpClient = new SmtpClient(config.Host, config.Port))
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = config.EnableSSL;
                if (config.IsRequireAuthentication)
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(config.CredentialsUserName, config.CredentialsUserPassword);
                }

                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var toAddress = new MailAddress(currentUser.Email);
                var fromAddress = new MailAddress(config.SenderAddress, config.SenderDisplayName);
                smtpClient.Send(new MailMessage(fromAddress, toAddress));
            }
        }

        [AjaxMethod]
        public void RestoreDefaults()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            CoreContext.Configuration.SmtpSettings = null; // this should set settings only for current tenant!
        }

        private static SmtpSettingsConfig ToSmtpSettingsConfig(SmtpSettingsModel settingsModel)
        {
            var settingsConfig = new SmtpSettingsConfig(settingsModel.Host, settingsModel.Port ?? SmtpSettingsConfig.DefaultSmtpPort,
                                                        settingsModel.SenderAddress, settingsModel.SenderDisplayName)
                {
                    EnableSSL = settingsModel.EnableSSL
                };

            if (!string.IsNullOrEmpty(settingsModel.CredentialsUserName) && !string.IsNullOrEmpty(settingsModel.CredentialsUserPassword))
            {
                settingsConfig.SetCredentials(settingsModel.CredentialsUserName, settingsModel.CredentialsUserPassword);
            }
            
            return settingsConfig;
        }

        private static SmtpSettingsModel ToSmtpSettingsModel(SmtpSettingsConfig settingsConfig)
        {
            return new SmtpSettingsModel
                {
                    Host = settingsConfig.Host,
                    Port = settingsConfig.Port,
                    SenderAddress = settingsConfig.SenderAddress,
                    SenderDisplayName = settingsConfig.SenderDisplayName,
                    CredentialsUserName = settingsConfig.CredentialsUserName,
                    CredentialsUserPassword = settingsConfig.CredentialsUserPassword,
                    EnableSSL = settingsConfig.EnableSSL
                };
        }

        public class SmtpSettingsModel
        {
            public string Host { get; set; }

            public int? Port { get; set; }

            public string SenderAddress { get; set; }

            public string SenderDisplayName { get; set; }

            public string CredentialsUserName { get; set; }

            public string CredentialsUserPassword { get; set; }

            public bool EnableSSL { get; set; }

            public bool IsRequireAuthentication
            {
                get { return !string.IsNullOrEmpty(CredentialsUserName) && !string.IsNullOrEmpty(CredentialsUserPassword); }
            }
        }
    }
}