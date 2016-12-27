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


using ASC.ActiveDirectory;
using ASC.ActiveDirectory.Novell;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Import;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Linq;

namespace ASC.Api.Settings
{
    public partial class SettingsApi : IApiEntryPoint
    {
        /// <summary>
        /// Get ldap settings
        /// </summary>
        /// <returns>LDAPSupportSettings</returns>
        [Read("ldap")]
        public LDAPSupportSettings GetLdapSettings()
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Ldap");
            }
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);

            settings.Password = null;
            settings.PasswordBytes = null;
            settings.IsDefault = settings.Equals(new LDAPSupportSettings().GetDefault() as LDAPSupportSettings);

            return settings;
        }

        /// <summary>
        /// save ldap settings
        /// </summary>
        /// <returns></returns>
        [Create("ldap")]
        public void SaveLdapSettings(string settings, bool acceptCertificate)
        {
            ILog log = LogManager.GetLogger(typeof(SettingsApi));
            try
            {
                if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                    CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap)
                {
                    throw new BillingException(Resource.ErrorNotAllowedOption, "Ldap");
                }
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                lock (LdapTasks.SynchRoot)
                {
                    var task = LdapTasks.GetItems().OfType<SaveLdapSettingTask>().
                        FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
                    if (task != null)
                    {
                        bool isCompleted = (bool?)AscCache.Default.Get<object>("SaveLdapSettingTaskIsCompleted") ?? true;
                        if (isCompleted)
                        {
                            LdapTasks.Remove(task);
                            task = null;
                        }
                    }
                    if (task == null)
                    {
                        string started = AscCache.Default.Get<string>("SaveLdapSettingTaskStarted");
                        if (started == null)
                        {
                            AscCache.Default.Insert("SaveLdapSettingTaskStarted", "started", TimeSpan.FromMinutes(15));

                            AscCache.Default.Remove("SaveLdapSettingTaskId");
                            AscCache.Default.Remove("SaveLdapSettingTaskIsCompleted");
                            AscCache.Default.Remove("SaveLdapSettingTaskPercentage");
                            AscCache.Default.Remove("SaveLdapSettingTaskStatus");
                            AscCache.Default.Remove("SaveLdapSettingTaskError");
                            AscCache.Default.Remove("NovellLdapCertificateConfirmRequest");

                            task = new SaveLdapSettingTask(settings, TenantProvider.CurrentTenantID, string.Empty, acceptCertificate);
                            LdapTasks.Add(task);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Save LDAP settings, error: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// read ldap settings status
        /// </summary>
        /// <returns>LDAPSupportSettingsResult</returns>
        [Read("ldap/status")]
        public LDAPSupportSettingsResult GetLdapSettingsStatus()
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Ldap");
            }
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            lock (LdapTasks.SynchRoot)
            {
                return ToLdapSettingsResult();
            }
        }

        /// <summary>
        /// read default ldap settings
        /// </summary>
        /// <returns>LDAPSupportSettings</returns>
        [Read("ldap/default")]
        public LDAPSupportSettings GetDefaultLdapSettings()
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Ldap");
            }
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            lock (LdapTasks.SynchRoot)
            {
                return new LDAPSupportSettings().GetDefault() as LDAPSupportSettings;
            }
        }

        private LDAPSupportSettingsResult ToLdapSettingsResult()
        {
            var id = AscCache.Default.Get<string>("SaveLdapSettingTaskId");
            bool isCompleted = Convert.ToBoolean(AscCache.Default.Get<string>("SaveLdapSettingTaskIsCompleted") ?? "true");
            int percentage = Convert.ToInt32(AscCache.Default.Get<string>("SaveLdapSettingTaskPercentage") ?? "0");
            string status = AscCache.Default.Get<string>("SaveLdapSettingTaskStatus");
            string error = AscCache.Default.Get<string>("SaveLdapSettingTaskError");
            NovellLdapCertificateConfirmRequest certificateConfirmRequest = 
                AscCache.Default.Get<NovellLdapCertificateConfirmRequest>("NovellLdapCertificateConfirmRequest");
            return new LDAPSupportSettingsResult
            {
                Id = id,
                Completed = isCompleted,
                Percents = percentage,
                Status = status,
                Error = error,
                CertificateConfirmRequest = certificateConfirmRequest
            };
        }
    }
}
