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
using System.Web;

using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.Api.Attributes;
using ASC.Common.Caching;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Notify.Cron;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

namespace ASC.Api.Settings
{
    public partial class SettingsApi
    {

        public static readonly ICache Cache = AscCache.Default;


        /// <summary>
        /// Returns the current portal LDAP settings.
        /// </summary>
        /// <short>
        /// Get the LDAP settings
        /// </short>
        /// <category>LDAP</category>
        /// <returns type="ASC.ActiveDirectory.Base.Settings.LdapSettings, ASC.ActiveDirectory">LDAP settings</returns>
        /// <path>api/2.0/settings/ldap</path>
        /// <httpMethod>GET</httpMethod>
        [Read("ldap")]
        public LdapSettings GetLdapSettings()
        {
            CheckLdapPermissions();

            var settings = LdapSettings.Load();

            settings = settings.Clone() as LdapSettings; // clone LdapSettings object for clear password (potencial AscCache.Memory issue)

            if (settings == null)
                return new LdapSettings().GetDefault() as LdapSettings;

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
        /// Returns the LDAP autosynchronous cron expression for the current portal if it exists.
        /// </summary>
        /// <short>
        /// Get the LDAP cron expression
        /// </short>
        /// <category>LDAP</category>
        /// <returns>Cron expression or null</returns>
        /// <path>api/2.0/settings/ldap/cron</path>
        /// <httpMethod>GET</httpMethod>
        [Read("ldap/cron")]
        public string GetLdapCronSettings()
        {
            CheckLdapPermissions();

            var settings = LdapCronSettings.Load();

            if (settings == null)
                settings = new LdapCronSettings().GetDefault() as LdapCronSettings;

            if (string.IsNullOrEmpty(settings.Cron))
                return null;

            return settings.Cron;
        }

        /// <summary>
        /// Sets the LDAP autosynchronous cron expression to the current portal.
        /// </summary>
        /// <short>
        /// Set the LDAP cron expression
        /// </short>
        /// <category>LDAP</category>
        /// <path>api/2.0/settings/ldap/cron</path>
        /// <param type="System.String, System" name="cron">Cron expression</param>
        /// <httpMethod>POST</httpMethod>
        /// <returns></returns>
        [Create("ldap/cron")]
        public void SetLdapCronSettings(string cron)
        {
            CheckLdapPermissions();

            if (!string.IsNullOrEmpty(cron))
            {
                new CronExpression(cron); // validate

                if (!LdapSettings.Load().EnableLdapAuthentication)
                {
                    throw new Exception(Resource.LdapSettingsErrorCantSaveLdapSettings);
                }
            }

            var settings = LdapCronSettings.Load();

            if (settings == null)
                settings = new LdapCronSettings();

            settings.Cron = cron;
            settings.Save();

            var t = CoreContext.TenantManager.GetCurrentTenant();
            if (!string.IsNullOrEmpty(cron))
            {
                LdapNotifyHelper.UnregisterAutoSync(t);
                LdapNotifyHelper.RegisterAutoSync(t, cron);
            }
            else
            {
                LdapNotifyHelper.UnregisterAutoSync(t);
            }
        }

        /// <summary>
        /// Synchronizes the portal data with the new information from the LDAP server.
        /// </summary>
        /// <short>
        /// Synchronize with LDAP server
        /// </short>
        /// <category>LDAP</category>
        /// <path>api/2.0/settings/ldap/sync</path>
        /// <httpMethod>GET</httpMethod>
        /// <returns type="ASC.ActiveDirectory.ComplexOperations.LdapOperationStatus, ASC.ActiveDirectory">Operation status</returns>
        [Read("ldap/sync")]
        public LdapOperationStatus SyncLdap()
        {
            CheckLdapPermissions();

            var operations = LDAPTasks.GetTasks()
                .Where(t => t.GetProperty<int>(LdapOperation.OWNER) == TenantProvider.CurrentTenantID)
                .ToList();

            var hasStarted = operations.Any(o =>
            {
                var opType = o.GetProperty<LdapOperationType>(LdapOperation.OPERATION_TYPE);

                return o.Status <= DistributedTaskStatus.Running &&
                       (opType == LdapOperationType.Sync || opType == LdapOperationType.Save);
            });

            if (hasStarted)
            {
                return GetLdapOperationStatus();
            }

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                return GetStartProcessError();
            }

            var ldapSettings = LdapSettings.Load();

            var ldapLocalization = new LdapLocalization(Resource.ResourceManager);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            Cache.Insert("REWRITE_URL" + tenant.TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));

            var op = new LdapSaveSyncOperation(ldapSettings, tenant, LdapOperationType.Sync, ldapLocalization, CurrentUser.ToString());

            return QueueTask(op);
        }

        /// <summary>
        /// Starts the process of collecting preliminary changes on the portal during the synchronization process according to the selected LDAP settings.
        /// </summary>
        /// <short>
        /// Test the LDAP synchronization
        /// </short>
        /// <category>LDAP</category>
        /// <path>api/2.0/settings/ldap/sync/test</path>
        /// <httpMethod>GET</httpMethod>
        /// <returns type="ASC.ActiveDirectory.ComplexOperations.LdapOperationStatus, ASC.ActiveDirectory">Operation status</returns>
        [Read("ldap/sync/test")]
        public LdapOperationStatus TestLdapSync()
        {
            CheckLdapPermissions();

            var operations = LDAPTasks.GetTasks()
                .Where(t => t.GetProperty<int>(LdapOperation.OWNER) == TenantProvider.CurrentTenantID)
                .ToList();

            var hasStarted = operations.Any(o =>
            {
                var opType = o.GetProperty<LdapOperationType>(LdapOperation.OPERATION_TYPE);

                return o.Status <= DistributedTaskStatus.Running &&
                       (opType == LdapOperationType.SyncTest || opType == LdapOperationType.SaveTest);
            });

            if (hasStarted)
            {
                return GetLdapOperationStatus();
            }

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                return GetStartProcessError();
            }

            var ldapSettings = LdapSettings.Load();

            var ldapLocalization = new LdapLocalization(Resource.ResourceManager);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            Cache.Insert("REWRITE_URL" + tenant.TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));

            var op = new LdapSaveSyncOperation(ldapSettings, tenant, LdapOperationType.SyncTest, ldapLocalization);

            return QueueTask(op);
        }

        /// <summary>
        /// Saves the LDAP settings specified in the request and starts importing/synchronizing users and groups by LDAP.
        /// </summary>
        /// <short>
        /// Save the LDAP settings
        /// </short>
        /// <category>LDAP</category>
        /// <param type="System.String, System" name="settings">LDAP settings in the serialized string format</param>
        /// <param type="System.Boolean, System" name="acceptCertificate">Specifies if a certificate will be accepted (true) or not (false)</param>
        /// <returns type="ASC.ActiveDirectory.ComplexOperations.LdapOperationStatus, ASC.ActiveDirectory">Operation status</returns>
        /// <path>api/2.0/settings/ldap</path>
        /// <httpMethod>POST</httpMethod>
        [Create("ldap")]
        public LdapOperationStatus SaveLdapSettings(string settings, bool acceptCertificate)
        {
            CheckLdapPermissions();

            var operations = LDAPTasks.GetTasks()
                .Where(t => t.GetProperty<int>(LdapOperation.OWNER) == TenantProvider.CurrentTenantID).ToList();

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                return GetStartProcessError();
            }

            var ldapSettings = JsonConvert.DeserializeObject<LdapSettings>(settings);

            ldapSettings.AcceptCertificate = acceptCertificate;

            if (!ldapSettings.EnableLdapAuthentication)
            {
                SetLdapCronSettings(null);
            }

            //ToDo
            ldapSettings.AccessRights.Clear();

            if (!ldapSettings.LdapMapping.ContainsKey(LdapSettings.MappingFields.MailAttribute) || string.IsNullOrEmpty(ldapSettings.LdapMapping[LdapSettings.MappingFields.MailAttribute]))
            {
                ldapSettings.SendWelcomeEmail = false;
            }

            var ldapLocalization = new LdapLocalization(Resource.ResourceManager, ASC.Web.Studio.Core.Notify.WebstudioNotifyPatternResource.ResourceManager);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            Cache.Insert("REWRITE_URL" + tenant.TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));

            var op = new LdapSaveSyncOperation(ldapSettings, tenant, LdapOperationType.Save, ldapLocalization, CurrentUser.ToString());

            return QueueTask(op);
        }

        /// <summary>
        /// Starts the process of saving LDAP settings and collecting preliminary changes on the portal according to them.
        /// </summary>
        /// <short>
        /// Test the LDAP saving process
        /// </short>
        /// <category>LDAP</category>
        /// <param type="System.String, System" name="settings">LDAP settings in the serialized string format</param>
        /// <param type="System.Boolean, System" name="acceptCertificate">Specifies if a certificate will be accepted (true) or not (false)</param>
        /// <path>api/2.0/settings/ldap/save/test</path>
        /// <httpMethod>POST</httpMethod>
        /// <returns type="ASC.ActiveDirectory.ComplexOperations.LdapOperationStatus, ASC.ActiveDirectory">Operation status</returns>
        [Create("ldap/save/test")]
        public LdapOperationStatus TestLdapSave(string settings, bool acceptCertificate)
        {
            CheckLdapPermissions();

            var operations = LDAPTasks.GetTasks()
                .Where(t => t.GetProperty<int>(LdapOperation.OWNER) == TenantProvider.CurrentTenantID)
                .ToList();

            var hasStarted = operations.Any(o =>
            {
                var opType = o.GetProperty<LdapOperationType>(LdapOperation.OPERATION_TYPE);

                return o.Status <= DistributedTaskStatus.Running &&
                       (opType == LdapOperationType.SyncTest || opType == LdapOperationType.SaveTest);
            });

            if (hasStarted)
            {
                return GetLdapOperationStatus();
            }

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                return GetStartProcessError();
            }

            var ldapSettings = JsonConvert.DeserializeObject<LdapSettings>(settings);

            ldapSettings.AcceptCertificate = acceptCertificate;

            var ldapLocalization = new LdapLocalization(Resource.ResourceManager);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            Cache.Insert("REWRITE_URL" + tenant.TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));

            var op = new LdapSaveSyncOperation(ldapSettings, tenant, LdapOperationType.SaveTest, ldapLocalization, CurrentUser.ToString());

            return QueueTask(op);
        }

        /// <summary>
        /// Returns the LDAP synchronization process status.
        /// </summary>
        /// <short>
        /// Get the LDAP synchronization process status
        /// </short>
        /// <category>LDAP</category>
        /// <returns type="ASC.ActiveDirectory.ComplexOperations.LdapOperationStatus, ASC.ActiveDirectory">Operation status</returns>
        /// <path>api/2.0/settings/ldap/status</path>
        /// <httpMethod>GET</httpMethod>
        [Read("ldap/status")]
        public LdapOperationStatus GetLdapOperationStatus()
        {
            CheckLdapPermissions();

            return ToLdapOperationStatus();
        }

        /// <summary>
        /// Returns the LDAP default settings.
        /// </summary>
        /// <short>
        /// Get the LDAP default settings
        /// </short>
        /// <category>LDAP</category>
        /// <returns type="ASC.ActiveDirectory.Base.Settings.LdapSettings, ASC.ActiveDirectory">LDAP default settings</returns>
        /// <path>api/2.0/settings/ldap/default</path>
        /// <httpMethod>GET</httpMethod>
        [Read("ldap/default")]
        public LdapSettings GetDefaultLdapSettings()
        {
            CheckLdapPermissions();

            return new LdapSettings().GetDefault() as LdapSettings;
        }

        private static LdapOperationStatus ToLdapOperationStatus()
        {
            var operations = LDAPTasks.GetTasks().ToList();

            foreach (var o in operations)
            {
                if (!string.IsNullOrEmpty(o.InstanseId) &&
                    Process.GetProcesses().Any(p => p.Id == int.Parse(o.InstanseId)))
                    continue;

                o.SetProperty(LdapOperation.PROGRESS, 100);
                LDAPTasks.RemoveTask(o.Id);
            }

            var operation =
                operations
                    .FirstOrDefault(t => t.GetProperty<int>(LdapOperation.OWNER) == TenantProvider.CurrentTenantID);

            if (operation == null)
            {
                return null;
            }

            if (DistributedTaskStatus.Running < operation.Status)
            {
                operation.SetProperty(LdapOperation.PROGRESS, 100);
                LDAPTasks.RemoveTask(operation.Id);
            }

            var certificateConfirmRequest = operation.GetProperty<LdapCertificateConfirmRequest>(LdapOperation.CERT_REQUEST);

            var result = new LdapOperationStatus
            {
                Id = operation.Id,
                Completed = operation.GetProperty<bool>(LdapOperation.FINISHED),
                Percents = operation.GetProperty<int>(LdapOperation.PROGRESS),
                Status = operation.GetProperty<string>(LdapOperation.RESULT),
                Error = operation.GetProperty<string>(LdapOperation.ERROR),
                CertificateConfirmRequest = certificateConfirmRequest,
                Source = operation.GetProperty<string>(LdapOperation.SOURCE),
                OperationType = Enum.GetName(typeof(LdapOperationType),
                    (LdapOperationType)Convert.ToInt32(operation.GetProperty<string>(LdapOperation.OPERATION_TYPE))),
                Warning = operation.GetProperty<string>(LdapOperation.WARNING)
            };

            if (!(string.IsNullOrEmpty(result.Warning)))
            {
                operation.SetProperty(LdapOperation.WARNING, ""); // "mark" as read
            }

            return result;
        }

        private static void CheckLdapPermissions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!CoreContext.Configuration.Standalone
                && (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString())
                    || !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Ldap");
            }
        }

        private LdapOperationStatus QueueTask(LdapOperation op)
        {
            LDAPTasks.QueueTask(op.RunJob, op.GetDistributedTask());
            return ToLdapOperationStatus();
        }

        private LdapOperationStatus GetStartProcessError()
        {
            var result = new LdapOperationStatus
            {
                Id = null,
                Completed = true,
                Percents = 0,
                Status = "",
                Error = Resource.LdapSettingsTooManyOperations,
                CertificateConfirmRequest = null,
                Source = ""
            };

            return result;
        }
    }
}
