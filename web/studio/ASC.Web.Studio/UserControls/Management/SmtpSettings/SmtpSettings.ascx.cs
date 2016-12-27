/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Configuration;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Data.Storage;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using MailKit;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using Resources;
using SmtpSettingsConfig = ASC.Core.Configuration.SmtpSettings;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.SmtpSettings, Location)]
    [AjaxNamespace("SmtpSettings")]
    public partial class SmtpSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/SmtpSettings/SmtpSettings.ascx";
        public const string FakePassword = "";

        protected SmtpSettingsModel CurrentSmtpSettings
        {
            get { return ToSmtpSettingsModel(CoreContext.Configuration.SmtpSettings, true); }
        }

        protected SmtpSettingsModel FullSmtpSettings
        {
            get { return ToSmtpSettingsModel(CoreContext.Configuration.SmtpSettings); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.SmtpSettings.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl, "~/usercontrols/management/smtpsettings/js/smtpsettings.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(),
                "smtpsettings_style",
                string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\">",
                    WebPath.GetPath("usercontrols/management/smtpsettings/css/smtpsettings.css")), false);
            Page.RegisterInlineScript(GetSmtpSettingsInitInlineScript(), true, false);
        }

        public bool IsMailServerAvailable
        {
            get { return SetupInfo.IsVisibleSettings("AdministrationPage") && SetupInfo.IsVisibleSettings("SmtpSettingsWithsMailServer"); }
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

        [AjaxMethod]
        public SmtpSettingsModel Save(SmtpSettingsModel settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var smtpSettings = ToSmtpSettingsConfig(settings);
            CoreContext.Configuration.SmtpSettings = smtpSettings;
            var result = ToSmtpSettingsModel(smtpSettings);
            result.CredentialsUserPassword = "";
            if (string.IsNullOrEmpty(result.SenderDisplayName))
                result.SenderDisplayName = SmtpSettingsConfig.DefaultSenderDisplayName;

            return result;
        }

        [AjaxMethod]
        public void Test(SmtpSettingsModel settings)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.SmtpSettings.ToString()))
                throw new BillingException(Resource.ErrorNotAllowedOption, "Smtp");

            var config = ToSmtpSettingsConfig(settings ?? CurrentSmtpSettings);

            if (config.EnableAuth && config.CredentialsUserPassword.Equals(FakePassword))
            {
                var model = ToSmtpSettingsModel(config);
                model.CredentialsUserPassword = FullSmtpSettings.CredentialsUserPassword;
                config = ToSmtpSettingsConfig(model);
            }

            var sslCertificatePermit = ConfigurationManager.AppSettings["mail.certificate-permit"] != null &&
                                    Convert.ToBoolean(ConfigurationManager.AppSettings["mail.certificate-permit"]);

            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var toAddress = new MailboxAddress(currentUser.UserName, currentUser.Email);
            var fromAddress = new MailboxAddress(config.SenderDisplayName, config.SenderAddress);

            var mimeMessage = new MimeMessage
            {
                Subject = Core.Notify.WebstudioNotifyPatternResource.subject_smtp_test
            };

            mimeMessage.From.Add(fromAddress);

            mimeMessage.To.Add(toAddress);

            var bodyBuilder = new MimeKit.BodyBuilder
            {
                TextBody = Core.Notify.WebstudioNotifyPatternResource.pattern_smtp_test
            };

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            mimeMessage.Headers.Add("Auto-Submitted", "auto-generated");

            using (var client = new MailKit.Net.Smtp.SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                    sslCertificatePermit ||
                    MailKit.MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                Timeout = (int) TimeSpan.FromSeconds(30).TotalMilliseconds
            })
            {
                client.Connect(config.Host, config.Port,
                    config.EnableSSL ? SecureSocketOptions.Auto : SecureSocketOptions.None);

                if (config.EnableAuth)
                {
                    client.Authenticate(config.CredentialsUserName,
                        config.CredentialsUserPassword);
                }

                client.Send(FormatOptions.Default, mimeMessage, CancellationToken.None);
            }
        }

        [AjaxMethod]
        public SmtpSettingsModel RestoreDefaults()
        {
            if (CoreContext.Configuration.SmtpSettings.IsDefaultSettings)
                return CurrentSmtpSettings;

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            CoreContext.Configuration.SmtpSettings = null; // this should set settings only for current tenant!
            return CurrentSmtpSettings;
        }

        private static SmtpSettingsConfig ToSmtpSettingsConfig(SmtpSettingsModel settingsModel)
        {
            var settingsConfig = new SmtpSettingsConfig(
                settingsModel.Host,
                settingsModel.Port ?? SmtpSettingsConfig.DefaultSmtpPort,
                settingsModel.SenderAddress,
                settingsModel.SenderDisplayName)
            {
                EnableSSL = settingsModel.EnableSSL,
                EnableAuth = settingsModel.EnableAuth
            };

            if (settingsModel.EnableAuth)
            {
                settingsConfig.SetCredentials(settingsModel.CredentialsUserName, settingsModel.CredentialsUserPassword);
            }

            return settingsConfig;
        }

        private static SmtpSettingsModel ToSmtpSettingsModel(SmtpSettingsConfig settingsConfig, bool hidePassword = false)
        {
            return new SmtpSettingsModel
            {
                Host = settingsConfig.Host,
                Port = settingsConfig.Port,
                SenderAddress = settingsConfig.SenderAddress,
                SenderDisplayName = settingsConfig.SenderDisplayName,
                CredentialsUserName = settingsConfig.CredentialsUserName,
                CredentialsUserPassword = hidePassword ? FakePassword : settingsConfig.CredentialsUserPassword,
                EnableSSL = settingsConfig.EnableSSL,
                EnableAuth = settingsConfig.EnableAuth
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

            public bool EnableAuth { get; set; }
        }
    }
}