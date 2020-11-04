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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

using ASC.Common.Logging;
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
using ASC.Web.Core.Files;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Projects.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

using Autofac;

namespace ASC.Web.Projects.Classes
{
    public class NotifyHelper
    {
        private static ILog logger;

        static NotifyHelper()
        {
            logger = LogManager.GetLogger("ASC.Web.Projects.Reports");
        }

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

                                    NotifyClient.Instance.SendReminderAboutTaskDeadline(new List<Guid> { responsible }, t);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("SendAutoReminderAboutTask", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("SendAutoReminderAboutTask", ex);
            }
        }

        private static bool TimeToSendReminderAboutTask(DateTime currentTime)
        {
            var hourToSend = 7;
            if (!string.IsNullOrEmpty(ConfigurationManagerExtension.AppSettings["remindertime"]))
            {
                int hour;
                if (int.TryParse(ConfigurationManagerExtension.AppSettings["remindertime"], out hour))
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

                foreach (var tGrouped in templates.GroupBy(r => r.Tenant))
                {
                    try
                    {
                        SendByTenant(now, tGrouped);
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("ASC").Error("SendByTenant", e);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("SendAutoReports", ex);
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
                        logger.Error("SendMsgMilestoneDeadline, tenant: " + tenant.TenantDomain, ex);
                    }
                }
            }
        }

        private static void SendByTenant(DateTime now, IGrouping<int, ReportTemplate> tGrouped)
        {
            var tenant = CoreContext.TenantManager.GetTenant(tGrouped.Key);
            if (tenant == null || tenant.Status != TenantStatus.Active || CoreContext.PaymentManager.GetTariff(tenant.TenantId).State >= TariffState.NotPaid) return;

            CoreContext.TenantManager.SetCurrentTenant(tenant);
            DocbuilderReportsUtility.Terminate(ReportOrigin.ProjectsAuto);

            var timeZone = CoreContext.TenantManager.GetCurrentTenant().TimeZone;

            foreach (var t in tGrouped)
            {
                try
                {
                    SendReport(now, timeZone, t);
                }
                catch (System.Security.SecurityException se)
                {
                    logger.Error("SendAutoReports", se);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("TemplateId: {0}, Temaplate: {1}\r\n{2}", t.Id, t.Filter.ToXml(), ex);
                }
            }
        }

        private static void SendReport(DateTime now, TimeZoneInfo timeZone, ReportTemplate t)
        {
            var cron = new CronExpression(t.Cron) { TimeZone = timeZone };
            var date = cron.GetTimeAfter(now.AddTicks(-1));

            LogManager.GetLogger("ASC.Web.Projects.Reports").DebugFormat("Find auto report: {0} - {1}, now: {2}, date: {3}", t.Name, t.Cron, now, date);
            if (date != now) return;

            var user = CoreContext.UserManager.GetUsers(t.CreateBy);
            if (user.ID == Constants.LostUser.ID || user.Status != EmployeeStatus.Active) return;

            SecurityContext.AuthenticateMe(user.ID);

            Thread.CurrentThread.CurrentCulture = user.GetCulture();
            Thread.CurrentThread.CurrentUICulture = user.GetCulture();

            var message = new NoticeMessage(user, HttpUtility.HtmlDecode(t.Name), "", "html");
            message.AddArgument(new TagValue(CommonTags.SendFrom, CoreContext.TenantManager.GetCurrentTenant().Name));
            message.AddArgument(new TagValue(CommonTags.Priority, 1));

            ReportState state;
            var template = t;
            var result = Report.TryCreateReportFromTemplate(t, (s, u) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(s.Exception))
                    {
                        template.SaveDocbuilderReport(s, u);
                    }

                    SendWhenGenerated(s, user);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }, message, out state, true);

            if (!result)
            {
                SendWhenGenerated(state, user);
            }
        }

        private static void SendWhenGenerated(ReportState state, UserInfo user)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = user.GetCulture();
                Thread.CurrentThread.CurrentUICulture = user.GetCulture();

                var message = (NoticeMessage)state.Obj;

                message.Body = !string.IsNullOrEmpty(state.Exception) ? state.Exception : string.Format(Resources.ReportResource.AutoGeneratedReportMail, message.Subject, CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(state.FileId)));

                WorkContext.NotifyContext.DispatchEngine.Dispatch(message, "email.sender");

                logger.DebugFormat("Send auto report: {0} to {1}, tenant: {2}", message.Subject, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID);
            }
            catch (Exception e)
            {
                logger.Error("SendWhenGenerated", e);
            }
        }
    }
}
