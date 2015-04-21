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
    [ManagementControl(ManagementType.SmtpSettings, Location)]
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
                smtpClient.Timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
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
                var mailMessage = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = Core.Notify.WebstudioPatternResource.subject_smtp_test,
                        Body = Core.Notify.WebstudioPatternResource.pattern_smtp_test
                    };

                smtpClient.Send(mailMessage);
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