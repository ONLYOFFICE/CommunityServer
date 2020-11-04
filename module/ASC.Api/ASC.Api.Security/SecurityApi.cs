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


using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.AuditTrail;
using ASC.AuditTrail.Data;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Security
{
    public class SecurityApi : IApiEntryPoint
    {
        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        public string Name
        {
            get { return "security"; }
        }

        [Read("/audit/login/last")]
        public IEnumerable<LoginEventWrapper> GetLastLoginEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.LoginHistory.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Audit)
                throw new BillingException(Resource.ErrorNotAllowedOption, "Audit");

            return LoginEventsRepository.GetLast(TenantProvider.CurrentTenantID, 20).Select(x => new LoginEventWrapper(x));
        }

        [Read("/audit/events/last")]
        public IEnumerable<AuditEventWrapper> GetLastAuditEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.AuditTrail.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Audit)
                throw new BillingException(Resource.ErrorNotAllowedOption, "Audit");

            return AuditEventsRepository.GetLast(TenantProvider.CurrentTenantID, 20).Select(x => new AuditEventWrapper(x));
        }

        [Create("/audit/login/report")]
        public string CreateLoginHistoryReport()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var tenantId = TenantProvider.CurrentTenantID;
            
            if (!SetupInfo.IsVisibleSettings(ManagementType.LoginHistory.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(tenantId).Audit)
                throw new BillingException(Resource.ErrorNotAllowedOption, "Audit");

            var settings = TenantAuditSettings.LoadForTenant(tenantId);

            var to = DateTime.UtcNow;
            var from = to.Subtract(TimeSpan.FromDays(settings.LoginHistoryLifeTime));

            var reportName = string.Format(AuditReportResource.LoginHistoryReportName + ".csv", from.ToString("MM.dd.yyyy"), to.ToString("MM.dd.yyyy"));
            var events = LoginEventsRepository.Get(tenantId, from, to);
            var result = AuditReportCreator.CreateCsvReport(events, reportName);

            MessageService.Send(Request, MessageAction.LoginHistoryReportDownloaded);
            return result;
        }

        [Create("/audit/events/report")]
        public string CreateAuditTrailReport()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var tenantId = TenantProvider.CurrentTenantID;

            if (!SetupInfo.IsVisibleSettings(ManagementType.AuditTrail.ToString()) ||
                CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(tenantId).Audit)
                throw new BillingException(Resource.ErrorNotAllowedOption, "Audit");

            var settings = TenantAuditSettings.LoadForTenant(tenantId);

            var to = DateTime.UtcNow;
            var from = to.Subtract(TimeSpan.FromDays(settings.AuditTrailLifeTime));

            var reportName = string.Format(AuditReportResource.AuditTrailReportName + ".csv", from.ToString("MM.dd.yyyy"), to.ToString("MM.dd.yyyy"));

            var events = AuditEventsRepository.Get(tenantId, from, to);
            var result = AuditReportCreator.CreateCsvReport(events, reportName);

            MessageService.Send(Request, MessageAction.AuditTrailReportDownloaded);
            return result;
        }

        [Read("/audit/settings/lifetime")]
        public TenantAuditSettings GetAuditSettings()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            return TenantAuditSettings.LoadForTenant(TenantProvider.CurrentTenantID);
        }

        [Create("/audit/settings/lifetime")]
        public TenantAuditSettings SetAuditSettings(TenantAuditSettings settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (settings.LoginHistoryLifeTime <= 0 || settings.LoginHistoryLifeTime > TenantAuditSettings.MaxLifeTime)
                throw new ArgumentException("LoginHistoryLifeTime");

            if (settings.AuditTrailLifeTime <= 0 || settings.AuditTrailLifeTime > TenantAuditSettings.MaxLifeTime)
                throw new ArgumentException("AuditTrailLifeTime");

            settings.SaveForTenant(TenantProvider.CurrentTenantID);

            MessageService.Send(Request, MessageAction.AuditSettingsUpdated);

            return settings;
        }
    }
}