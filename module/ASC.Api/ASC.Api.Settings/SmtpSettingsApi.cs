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
using System.Diagnostics;
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.Settings.Smtp;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Configuration;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

namespace ASC.Api.Settings
{
    public partial class SettingsApi
    {
        /// <summary>
        /// Returns the current portal SMTP settings.
        /// </summary>
        /// <short>
        /// Get the SMTP settings
        /// </short>
        /// <category>SMTP</category>
        /// <returns type="ASC.Api.Settings.Smtp.SmtpSettingsWrapper, ASC.Api.Settings">SMTP settings</returns>
        /// <path>api/2.0/settings/smtp</path>
        /// <httpMethod>GET</httpMethod>
        [Read("smtp")]
        public SmtpSettingsWrapper GetSmtpSettings()
        {
            CheckSmtpPermissions();

            var settings = ToSmtpSettings(CoreContext.Configuration.SmtpSettings, true);

            return settings;
        }

        /// <summary>
        /// Saves the SMTP settings for the current portal.
        /// </summary>
        /// <short>
        /// Save the SMTP settings
        /// </short>
        /// <category>SMTP</category>
        /// <param type="ASC.Api.Settings.Smtp.SmtpSettingsWrapper, ASC.Api.Settings.Smtp" file="ASC.Api.Settings" name="smtpSettings">SMTP settings</param>
        /// <returns type="ASC.Api.Settings.Smtp.SmtpSettingsWrapper, ASC.Api.Settings">SMTP settings</returns>
        /// <path>api/2.0/settings/smtp</path>
        /// <httpMethod>POST</httpMethod>
        [Create("smtp")]
        public SmtpSettingsWrapper SaveSmtpSettings(SmtpSettingsWrapper smtpSettings)
        {
            CheckSmtpPermissions();

            //TODO: Add validation check

            if (smtpSettings == null)
                throw new ArgumentNullException("smtpSettings");

            var settingConfig = ToSmtpSettingsConfig(smtpSettings);

            CoreContext.Configuration.SmtpSettings = settingConfig;

            var settings = ToSmtpSettings(settingConfig, true);

            return settings;
        }

        /// <summary>
        /// Resets SMTP settings of the current portal.
        /// </summary>
        /// <short>
        /// Reset the SMTP settings
        /// </short>
        /// <category>SMTP</category>
        /// <returns type="ASC.Api.Settings.Smtp.SmtpSettingsWrapper, ASC.Api.Settings">Default SMTP settings</returns>
        /// <path>api/2.0/settings/smtp</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("smtp")]
        public SmtpSettingsWrapper ResetSmtpSettings()
        {
            CheckSmtpPermissions();

            if (!CoreContext.Configuration.SmtpSettings.IsDefaultSettings)
            {
                CoreContext.Configuration.SmtpSettings = null;
            }

            var current = CoreContext.Configuration.Standalone ? CoreContext.Configuration.SmtpSettings : SmtpSettings.Empty;

            return ToSmtpSettings(current, true);
        }

        /// <summary>
        /// Tests the SMTP settings for the current portal (send test message to the user email).
        /// </summary>
        /// <short>
        /// Test the SMTP settings
        /// </short>
        /// <category>SMTP</category>
        /// <returns type="ASC.Api.Settings.Smtp.SmtpOperationStatus, ASC.Api.Settings">SMTP operation status</returns>
        /// <path>api/2.0/settings/smtp/test</path>
        /// <httpMethod>GET</httpMethod>
        [Read("smtp/test")]
        public SmtpOperationStatus TestSmtpSettings()
        {
            CheckSmtpPermissions();

            var settings = ToSmtpSettings(CoreContext.Configuration.SmtpSettings);

            var smtpTestOp = new SmtpOperation(settings, CurrentTenant, CurrentUser);

            SMTPTasks.QueueTask(smtpTestOp.RunJob, smtpTestOp.GetDistributedTask());

            return ToSmtpOperationStatus();
        }

        /// <summary>
        /// Returns the SMTP test process status.
        /// </summary>
        /// <short>
        /// Get the SMTP test process status
        /// </short>
        /// <category>SMTP</category>
        /// <returns type="ASC.Api.Settings.Smtp.SmtpOperationStatus, ASC.Api.Settings">SMTP operation status</returns>
        /// <path>api/2.0/settings/smtp/test/status</path>
        /// <httpMethod>GET</httpMethod>
        [Read("smtp/test/status")]
        public SmtpOperationStatus GetSmtpOperationStatus()
        {
            CheckSmtpPermissions();

            return ToSmtpOperationStatus();
        }

        private static SmtpOperationStatus ToSmtpOperationStatus()
        {
            var operations = SMTPTasks.GetTasks().ToList();

            foreach (var o in operations)
            {
                if (!string.IsNullOrEmpty(o.InstanseId) &&
                    Process.GetProcesses().Any(p => p.Id == int.Parse(o.InstanseId)))
                    continue;

                o.SetProperty(SmtpOperation.PROGRESS, 100);
                SMTPTasks.RemoveTask(o.Id);
            }

            var operation =
                operations
                    .FirstOrDefault(t => t.GetProperty<int>(SmtpOperation.OWNER) == TenantProvider.CurrentTenantID);

            if (operation == null)
            {
                return null;
            }

            if (DistributedTaskStatus.Running < operation.Status)
            {
                operation.SetProperty(SmtpOperation.PROGRESS, 100);
                SMTPTasks.RemoveTask(operation.Id);
            }

            var result = new SmtpOperationStatus
            {
                Id = operation.Id,
                Completed = operation.GetProperty<bool>(SmtpOperation.FINISHED),
                Percents = operation.GetProperty<int>(SmtpOperation.PROGRESS),
                Status = operation.GetProperty<string>(SmtpOperation.RESULT),
                Error = operation.GetProperty<string>(SmtpOperation.ERROR),
                Source = operation.GetProperty<string>(SmtpOperation.SOURCE)
            };

            return result;
        }

        public static SmtpSettings ToSmtpSettingsConfig(SmtpSettingsWrapper settingsWrapper)
        {
            var settingsConfig = new SmtpSettings(
                settingsWrapper.Host,
                settingsWrapper.Port ?? SmtpSettings.DefaultSmtpPort,
                settingsWrapper.SenderAddress,
                settingsWrapper.SenderDisplayName)
            {
                EnableSSL = settingsWrapper.EnableSSL,
                EnableAuth = settingsWrapper.EnableAuth
            };

            if (settingsWrapper.EnableAuth)
            {
                settingsConfig.SetCredentials(settingsWrapper.CredentialsUserName, settingsWrapper.CredentialsUserPassword);
                settingsConfig.UseNtlm = settingsWrapper.UseNtlm;
            }

            return settingsConfig;
        }

        private static SmtpSettingsWrapper ToSmtpSettings(SmtpSettings settingsConfig, bool hidePassword = false)
        {
            return new SmtpSettingsWrapper
            {
                Host = settingsConfig.Host,
                Port = settingsConfig.Port,
                SenderAddress = settingsConfig.SenderAddress,
                SenderDisplayName = settingsConfig.SenderDisplayName,
                CredentialsUserName = settingsConfig.CredentialsUserName,
                CredentialsUserPassword = hidePassword ? "" : settingsConfig.CredentialsUserPassword,
                EnableSSL = settingsConfig.EnableSSL,
                EnableAuth = settingsConfig.EnableAuth,
                UseNtlm = settingsConfig.UseNtlm
            };
        }

        private static void CheckSmtpPermissions()
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.SmtpSettings.ToString()))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Smtp");
            }

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
        }
    }
}
