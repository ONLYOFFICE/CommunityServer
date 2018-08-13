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


using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify.Cron;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain.Reports;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Projects.Core;
using ASC.Web.Studio.Core.Notify;
using Autofac;
using log4net;

namespace ASC.Web.Projects.Classes
{
    public class NotifyHelper
    {
        public static void SendAutoReminderAboutTask(DateTime state)
        {
            try
            {
                var now = DateTime.UtcNow;
                List<object[]> tasks;

                using (var scope = DIHelper.Resolve(Tenant.DEFAULT_TENANT))
                {
                    var daoFactory = scope.Resolve<IDaoFactory>();
                    tasks = daoFactory.TaskDao.GetTasksForReminder(now);
                }

                foreach (var r in tasks)
                {
                    var tenant = CoreContext.TenantManager.GetTenant((int)r[0]);
                    if (tenant == null ||
                        tenant.Status != TenantStatus.Active ||
                        TariffState.NotPaid <= CoreContext.PaymentManager.GetTariff(tenant.TenantId).State)
                    {
                        continue;
                    }

                    var localTime = TenantUtil.DateTimeFromUtc(tenant.TimeZone, now);
                    if (!TimeToSendReminderAboutTask(localTime)) continue;

                    var deadline = (DateTime)r[2];
                    if (deadline.Date != localTime.Date) continue;

                    try
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                        using (var scope = DIHelper.Resolve())
                        {
                            var engineFactory = scope.Resolve<EngineFactory>();
                            var t = engineFactory.TaskEngine.GetByID((int)r[1]);
                            if (t == null) continue;

                            foreach (var responsible in t.Responsibles)
                            {
                                var user = CoreContext.UserManager.GetUsers(t.CreateBy);
                                if (!Constants.LostUser.Equals(user) && user.Status == EmployeeStatus.Active)
                                {
                                    SecurityContext.AuthenticateMe(user.ID);

                                    Thread.CurrentThread.CurrentCulture = user.GetCulture();
                                    Thread.CurrentThread.CurrentUICulture = user.GetCulture();

                                    NotifyClient.Instance.SendReminderAboutTaskDeadline(new List<Guid> {responsible}, t);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("ASC.Projects.Tasks").Error("SendAutoReminderAboutTask", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Projects.Tasks").Error("SendAutoReminderAboutTask", ex);
            }
        }

        private static bool TimeToSendReminderAboutTask(DateTime currentTime)
        {
            var hourToSend = 7;
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["remindertime"]))
            {
                int hour;
                if (int.TryParse(WebConfigurationManager.AppSettings["remindertime"], out hour))
                {
                    hourToSend = hour;
                }
            }
            return currentTime.Hour == hourToSend;
        }

        public static void SendAutoReports(DateTime datetime)
        {
            try
            {
                var now = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour);

                List<ReportTemplate> templates;

                using (var scope = DIHelper.Resolve(Tenant.DEFAULT_TENANT))
                {
                    var daoFactory = scope.Resolve<IDaoFactory>();
                    templates = daoFactory.ReportDao.GetAutoTemplates();
                }

                foreach (var t in templates)
                {
                    try
                    {
                        var tenant = CoreContext.TenantManager.GetTenant(t.Tenant);
                        if (tenant != null && tenant.Status == TenantStatus.Active && CoreContext.PaymentManager.GetTariff(tenant.TenantId).State < TariffState.NotPaid)
                        {
                            CoreContext.TenantManager.SetCurrentTenant(tenant);
                            var cron = new CronExpression(t.Cron) { TimeZone = CoreContext.TenantManager.GetCurrentTenant().TimeZone };
                            var date = cron.GetTimeAfter(now.AddTicks(-1));

                            LogManager.GetLogger("ASC.Web.Projects.Reports").DebugFormat("Find auto report: {0} - {1}, now: {2}, date: {3}", t.Name, t.Cron, now, date);
                            if (date == now)
                            {
                                var user = CoreContext.UserManager.GetUsers(t.CreateBy);
                                if (user.ID != Constants.LostUser.ID && user.Status == EmployeeStatus.Active)
                                {
                                    SecurityContext.AuthenticateMe(user.ID);

                                    Thread.CurrentThread.CurrentCulture = user.GetCulture();
                                    Thread.CurrentThread.CurrentUICulture = user.GetCulture();

                                    var result = Report.CreateNewReport(t.ReportType, t.Filter).BuildReport(ReportViewType.EMail, t.Id);
                                    var message = new NoticeMessage(user, HttpUtility.HtmlDecode(t.Name), result, "html");
                                    message.AddArgument(new TagValue(CommonTags.SendFrom, CoreContext.TenantManager.GetCurrentTenant().Name));
                                    message.AddArgument(new TagValue(CommonTags.Priority, 1));
                                    LogManager.GetLogger("ASC.Web.Projects.Reports").DebugFormat("Send auto report: {0} to {1}, tenant: {2}", t.Name, user, CoreContext.TenantManager.GetCurrentTenant());
                                    WorkContext.NotifyContext.DispatchEngine.Dispatch(message, "email.sender");
                                }
                            }
                        }
                    }
                    catch (System.Security.SecurityException se)
                    {
                        LogManager.GetLogger("ASC.Web.Projects.Reports").Info("SendAutoReports", se);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("ASC.Web.Projects.Reports").ErrorFormat("TemplateId: {0}, Temaplate: {1}\r\n{2}", t.Id, t.Filter.ToXml(), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web.Projects.Reports").Error("SendAutoReports", ex);
            }
        }

                
        public static void SendMsgMilestoneDeadline(DateTime scheduleDate)
        {
            var date = DateTime.UtcNow.AddDays(2);

            List<object[]> milestones;

            using (var scope = DIHelper.Resolve(Tenant.DEFAULT_TENANT))
            {
                var daoFactory = scope.Resolve<IDaoFactory>();
                milestones = daoFactory.MilestoneDao.GetInfoForReminder(date);
            }

            foreach (var r in milestones)
            {
                var tenant = CoreContext.TenantManager.GetTenant((int)r[0]);
                if (tenant == null ||
                    tenant.Status != TenantStatus.Active ||
                    TariffState.NotPaid <= CoreContext.PaymentManager.GetTariff(tenant.TenantId).State)
                {
                    continue;
                }

                var localTime = TenantUtil.DateTimeFromUtc(tenant.TimeZone, date);
                if (localTime.Date == ((DateTime)r[2]).Date)
                {
                    try
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                        using (var scope = DIHelper.Resolve())
                        {
                            var m = scope.Resolve<IDaoFactory>().MilestoneDao.GetById((int)r[1]);
                            if (m != null)
                            {
                                var sender = !m.Responsible.Equals(Guid.Empty) ? m.Responsible : m.Project.Responsible;
                                var user = CoreContext.UserManager.GetUsers(sender);
                                if (!Constants.LostUser.Equals(user) && user.Status == EmployeeStatus.Active)
                                {
                                    SecurityContext.AuthenticateMe(user.ID);

                                    Thread.CurrentThread.CurrentCulture = user.GetCulture();
                                    Thread.CurrentThread.CurrentUICulture = user.GetCulture();

                                    NotifyClient.Instance.SendMilestoneDeadline(sender, m);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("ASC.Projects.Tasks").Error("SendMsgMilestoneDeadline, tenant: " + tenant.TenantDomain, ex);
                    }
                }
            }
        }
    }
}
