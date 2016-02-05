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
using System.Collections.Generic;
using System.Linq;
using System.Security;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Projects.Engine
{
    public class ReportEngine
    {
        private readonly IReportDao reportDao;
        private readonly EngineFactory factory;

        public ReportEngine(IDaoFactory daoFactory, EngineFactory factory)
        {
            reportDao = daoFactory.GetReportDao();
            this.factory = factory;
        }


        public List<ReportTemplate> GetTemplates(Guid userId)
        {
            if (ProjectSecurity.IsVisitor(SecurityContext.CurrentAccount.ID)) throw new SecurityException("Access denied.");

            return reportDao.GetTemplates(userId);
        }

        public List<ReportTemplate> GetAutoTemplates()
        {
            if (ProjectSecurity.IsVisitor(SecurityContext.CurrentAccount.ID)) throw new SecurityException("Access denied.");

            return reportDao.GetAutoTemplates();
        }

        public ReportTemplate GetTemplate(int id)
        {
            if (ProjectSecurity.IsVisitor(SecurityContext.CurrentAccount.ID)) throw new SecurityException("Access denied.");

            return reportDao.GetTemplate(id);
        }

        public ReportTemplate SaveTemplate(ReportTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");

            if (ProjectSecurity.IsVisitor(SecurityContext.CurrentAccount.ID)) throw new SecurityException("Access denied.");

            if (template.CreateOn == default(DateTime)) template.CreateOn = TenantUtil.DateTimeNow();
            if (template.CreateBy.Equals(Guid.Empty)) template.CreateBy = SecurityContext.CurrentAccount.ID;
            return reportDao.SaveTemplate(template);
        }

        public void DeleteTemplate(int id)
        {
            if (ProjectSecurity.IsVisitor(SecurityContext.CurrentAccount.ID)) throw new SecurityException("Access denied.");

            reportDao.DeleteTemplate(id);
        }


        public IList<object[]> BuildUsersWithoutActiveTasks(TaskFilter filter)
        {
            var result = new List<object[]>();

            var users = new List<Guid>();
            if (filter.UserId != Guid.Empty) users.Add(filter.UserId);
            else if (filter.DepartmentId != Guid.Empty)
            {
                users.AddRange(CoreContext.UserManager.GetUsersByGroup(filter.DepartmentId).Select(u => u.ID));
            }
            else if (filter.HasProjectIds)
            {
                users.AddRange(factory.ProjectEngine.GetTeam(filter.ProjectIds).Select(r => r.ID));
            }
            else if (!filter.HasProjectIds)
            {
                users.AddRange(CoreContext.UserManager.GetUsers().Select(u => u.ID));
            }

            foreach (var row in reportDao.BuildUsersStatisticsReport(filter))
            {
                users.Remove((Guid)row[0]);
                if ((long)row[1] == 0 && (long)row[2] == 0)
                {
                    result.Add(row);
                }
            }
            result.AddRange(users.Select(u => new object[] { u, 0, 0, 0 }));

            return result;
        }

        public IList<object[]> BuildUsersWorkload(TaskFilter filter)
        {
            return reportDao.BuildUsersStatisticsReport(filter);
        }

        public IList<object[]> BuildUsersActivityReport(TaskFilter filter)
        {
            return reportDao.BuildUsersActivityReport(filter);
        }
    }
}
