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