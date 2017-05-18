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
using System.Diagnostics;
using System.Linq;
using ASC.ActiveDirectory;
using ASC.ActiveDirectory.Novell;
using ASC.Api.Attributes;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Import.LDAP;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using Resources;

namespace ASC.Api.Settings
{
    public partial class SettingsApi
    {
        /// <summary>
        /// Returns current portal LDAP settings
        /// </summary>
        /// <short>
        /// Get LDAP settings
        /// </short>
        /// <returns>LDAPSupportSettings object</returns>
        [Read("ldap")]
        public LDAPSupportSettings GetLdapSettings()
        {
            CheckLdapPermissions();

            var settings = SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);

            settings.Password = null;
            settings.PasswordBytes = null;

            if (settings.IsDefault)
                return settings;

            var defaultSettings = settings.GetDefault();

            if (settings.Equals(defaultSettings))
                settings.IsDefault = true;

            return settings;
        }

        /// <summary>
        /// Start sync users and groups process by LDAP
        /// </summary>
        /// <short>
        /// Sync LDAP
        /// </short>
        [Read("ldap/sync")]
        public LDAPSupportSettingsResult SyncLdap()
        {
            CheckLdapPermissions();

            var operations = LDAPTasks.GetTasks()
                .Where(t => t.GetProperty<int>(LDAPOperation.OWNER) == TenantProvider.CurrentTenantID);

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                throw new InvalidOperationException(Resource.LdapSettingsTooManyOperations);
            }

            var ldapSettings =
                SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);

            var op = new LDAPSaveSyncOperation(ldapSettings, syncOnly: true);

            return QueueTask(op);
        }

        /// <summary>
        /// Save LDAP settings and start import/sync users and groups process by LDAP
        /// </summary>
        /// <short>
        /// Save LDAP settings
        /// </short>
        /// <param name="settings">LDAPSupportSettings serialized string</param>
        /// <param name="acceptCertificate">Flag permits errors of checking certificates</param>
        [Create("ldap")]
        public LDAPSupportSettingsResult SaveLdapSettings(string settings, bool acceptCertificate)
        {
            CheckLdapPermissions();

            var operations = LDAPTasks.GetTasks()
                .Where(t => t.GetProperty<int>(LDAPOperation.OWNER) == TenantProvider.CurrentTenantID);

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                throw new InvalidOperationException(Resource.LdapSettingsTooManyOperations);
            }

            var ldapSettings = JsonConvert.DeserializeObject<LDAPSupportSettings>(settings);

            var op = new LDAPSaveSyncOperation(ldapSettings, acceptCertificate: acceptCertificate);

            return QueueTask(op);
        }

        /// <summary>
        /// Returns LDAP sync process status
        /// </summary>
        /// <short>
        /// Get LDAP sync process status
        /// </short>
        /// <returns>LDAPSupportSettingsResult object</returns>
        [Read("ldap/status")]
        public LDAPSupportSettingsResult GetLdapSyncStatus()
        {
            CheckLdapPermissions();

            return ToLdapSettingsResult();
        }

        /// <summary>
        /// Returns LDAP default settings
        /// </summary>
        /// <short>
        /// Get LDAP default settings
        /// </short>
        /// <returns>LDAPSupportSettings object</returns>
        [Read("ldap/default")]
        public LDAPSupportSettings GetDefaultLdapSettings()
        {
            CheckLdapPermissions();

            return new LDAPSupportSettings().GetDefault() as LDAPSupportSettings;
        }

        private static LDAPSupportSettingsResult ToLdapSettingsResult()
        {
            var operations = LDAPTasks.GetTasks().ToList();

            foreach (var o in operations)
            {
                if (!string.IsNullOrEmpty(o.InstanseId) &&
                    Process.GetProcesses().Any(p => p.Id == int.Parse(o.InstanseId)))
                    continue;

                o.SetProperty(LDAPOperation.PROGRESS, 100);
                LDAPTasks.RemoveTask(o.Id);
            }

            var operation =
                operations
                    .FirstOrDefault(t => t.GetProperty<int>(LDAPOperation.OWNER) == TenantProvider.CurrentTenantID);

            if (operation == null)
            {
                return new LDAPSupportSettingsResult
                {
                    Id = null,
                    Completed = true,
                    Percents = 100,
                    Status = "",
                    Error = Resource.LdapSettingsInternalServerError,
                    Source = ""
                };
            }

            if (DistributedTaskStatus.Running < operation.Status)
            {
                operation.SetProperty(LDAPOperation.PROGRESS, 100);
                LDAPTasks.RemoveTask(operation.Id);
            }

            var certificateConfirmRequest = operation.GetProperty<NovellLdapCertificateConfirmRequest>(LDAPOperation.CERT_REQUEST);

            var result = new LDAPSupportSettingsResult
            {
                Id = operation.Id,
                Completed = operation.GetProperty<bool>(LDAPOperation.FINISHED),
                Percents = operation.GetProperty<int>(LDAPOperation.PROGRESS),
                Status = operation.GetProperty<string>(LDAPOperation.RESULT),
                Error = operation.GetProperty<string>(LDAPOperation.ERROR),
                CertificateConfirmRequest = certificateConfirmRequest,
                Source = operation.GetProperty<string>(LDAPOperation.SOURCE)
            };

            return result;
        }

        private static void CheckLdapPermissions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                (CoreContext.Configuration.Standalone &&
                 !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Ldap");
            }
        }

        private LDAPSupportSettingsResult QueueTask(LDAPOperation op)
        {
            LDAPTasks.QueueTask(op.RunJob, op.GetDistributedTask());
            return ToLdapSettingsResult();
        }
    }
}
