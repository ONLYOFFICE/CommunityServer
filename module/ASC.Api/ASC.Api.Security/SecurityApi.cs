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
using System.Collections.Generic;
using System.IO;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.AuditTrail;
using ASC.AuditTrail.Data;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Utils;
using System.Linq;
using ASC.Web.Studio.Core;

namespace ASC.Api.Security
{
    public class SecurityApi : IApiEntryPoint
    {
        private readonly ApiContext context;

        private static int CurrentTenant
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        public SecurityApi(ApiContext context)
        {
            this.context = context;
        }

        public string Name
        {
            get { return "security"; }
        }

        [Read("/audit/login/last")]
        public IEnumerable<LoginEventWrapper> GetLastLoginEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            return LoginEventsRepository.GetLast(CurrentTenant, 20).Select(x => new LoginEventWrapper(x));
        }

        [Read("/audit/events/last")]
        public IEnumerable<AuditEventWrapper> GetLastAuditEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            return AuditEventsRepository.GetLast(CurrentTenant, 20).Select(x => new AuditEventWrapper(x));
        }

        [Create("/audit/login/report")]
        public string CreateLoginHistoryReport()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            string fileUrl;

            var to = DateTime.UtcNow;
            var from = to.AddMonths(-6);

            var events = LoginEventsRepository.Get(CurrentTenant, from, to);
            var reportPath = AuditReportCreator.CreateXlsxReport(events);
            
            try
            {
                var reportName = string.Format(AuditReportResource.LoginHistoryReportName + ".xlsx", from.ToString("MM.dd.yyyy"), to.ToString("MM.dd.yyyy"));
                using (var stream = new FileStream(reportPath, FileMode.Open))
                {
                    var file = FileUploader.Exec(Global.FolderMy.ToString(), reportName, stream.Length, stream, true);
                    fileUrl = FilesLinkUtility.GetFileWebEditorUrl((int)file.ID);
                }
            }
            finally
            {
                AuditReportCreator.DeleteReport(reportPath);
            }

            MessageService.Send(context, MessageAction.LoginHistoryReportDownloaded);

            return fileUrl;
        }

        [Create("/audit/events/report")]
        public string CreateAuditTrailReport()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            string fileUrl;

            var to = DateTime.UtcNow;
            var from = to.AddMonths(-6);

            var events = AuditEventsRepository.Get(CurrentTenant, from, to);
            var reportPath = AuditReportCreator.CreateXlsxReport(events);
            
            if (reportPath == null) throw new ApplicationException();

            try
            {
                var reportName = string.Format(AuditReportResource.AuditTrailReportName + ".xlsx", from.ToString("MM.dd.yyyy"), to.ToString("MM.dd.yyyy"));
                using (var stream = new FileStream(reportPath, FileMode.Open))
                {
                    var file = FileUploader.Exec(Global.FolderMy.ToString(), reportName, stream.Length, stream, true);
                    fileUrl = FilesLinkUtility.GetFileWebEditorUrl((int)file.ID);
                }
            }
            finally
            {
                AuditReportCreator.DeleteReport(reportPath);
            }

            MessageService.Send(context, MessageAction.AuditTrailReportDownloaded);

            return fileUrl;
        }
    }
}