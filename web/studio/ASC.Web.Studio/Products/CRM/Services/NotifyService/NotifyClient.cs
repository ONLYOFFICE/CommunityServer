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


#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Core.Tenants;
using ASC.CRM.Core;
using System.Collections.Specialized;
using ASC.Common.Logging;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Resources;
using Autofac;

#endregion

namespace ASC.Web.CRM.Services.NotifyService
{
    public class NotifyClient
    {

        private static NotifyClient instance;
        private readonly INotifyClient client;
        private readonly INotifySource source;

        public static NotifyClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(NotifyClient))
                    {
                        if (instance == null) instance = new NotifyClient(WorkContext.NotifyContext.NotifyService.RegisterClient(NotifySource.Instance), NotifySource.Instance);
                    }
                }
                return instance;
            }
        }

        public void SendAboutCreateNewContact(List<Guid> recipientID, int contactID, String contactTitle, NameValueCollection fields)
        {
            if ((recipientID.Count == 0) || String.IsNullOrEmpty(contactTitle)) return;

            client.SendNoticeToAsync(
                NotifyConstants.Event_CreateNewContact,
                null,
                recipientID.ConvertAll(item => ToRecipient(item)).ToArray(),
                true,
                new TagValue(NotifyConstants.Tag_AdditionalData, fields),
                new TagValue(NotifyConstants.Tag_EntityTitle, contactTitle),
                new TagValue(NotifyConstants.Tag_EntityID, contactID)
             );

        }

        public void SendAboutSetAccess(EntityType entityType, int entityID, DaoFactory daoFactory, params Guid[] userID)
        {
            if (userID.Length == 0) return;

            var baseData = ExtractBaseDataFrom(entityType, entityID, daoFactory);

            client.SendNoticeToAsync(
                   NotifyConstants.Event_SetAccess,
                   null,
                   userID.Select(item => ToRecipient(item)).ToArray(),
                   true,
                   new TagValue(NotifyConstants.Tag_EntityID, baseData["id"]),
                   new TagValue(NotifyConstants.Tag_EntityTitle, baseData["title"]),
                   new TagValue(NotifyConstants.Tag_EntityRelativeURL, baseData["entityRelativeURL"])
                );
        }

        private NameValueCollection ExtractBaseDataFrom(EntityType entityType, int entityID, DaoFactory daoFactory)
        {
            var result = new NameValueCollection();

            String title;
            String relativeURL;

            switch (entityType)
            {
                case EntityType.Person:
                case EntityType.Company:
                case EntityType.Contact:
                {
                    var contact = daoFactory.ContactDao.GetByID(entityID);
                    title = contact != null ? contact.GetTitle() : string.Empty;
                    relativeURL = "default.aspx?id=" + entityID;
                    break;
                }
                case EntityType.Opportunity:
                {
                    var deal = daoFactory.DealDao.GetByID(entityID);
                    title = deal != null ? deal.Title : string.Empty;
                    relativeURL = "deals.aspx?id=" + entityID;
                    break;
                }
                case EntityType.Case:
                {
                    var cases = daoFactory.CasesDao.GetByID(entityID);
                    title = cases != null ? cases.Title : string.Empty;
                    relativeURL = "cases.aspx?id=" + entityID;
                    break;
                }

                default:
                    throw new ArgumentException();
            }

            result.Add("title", title);
            result.Add("id", entityID.ToString());
            result.Add("entityRelativeURL", String.Concat(PathProvider.BaseAbsolutePath, relativeURL));

            return result;
        }

        public void SendAboutAddRelationshipEventAdd(RelationshipEvent entity, Hashtable fileListInfoHashtable, DaoFactory daoFactory, params Guid[] userID)
        {
            if (userID.Length == 0) return;

            NameValueCollection baseEntityData;

            if (entity.EntityID != 0)
            {
                baseEntityData = ExtractBaseDataFrom(entity.EntityType, entity.EntityID, daoFactory);
            }
            else
            {
                var contact = daoFactory.ContactDao.GetByID(entity.ContactID);

                baseEntityData = new NameValueCollection();
                baseEntityData["title"] = contact.GetTitle();
                baseEntityData["id"] = contact.ID.ToString();
                baseEntityData["entityRelativeURL"] = "default.aspx?id=" + contact.ID;

                if (contact is Person)
                    baseEntityData["entityRelativeURL"] += "&type=people";

                baseEntityData["entityRelativeURL"] = String.Concat(PathProvider.BaseAbsolutePath,
                                                                    baseEntityData["entityRelativeURL"]);
            }

            client.BeginSingleRecipientEvent("send about add relationship event add");

            var interceptor = new InitiatorInterceptor(new DirectRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString(), ""));

            client.AddInterceptor(interceptor);

            try
            {

                client.SendNoticeToAsync(
                      NotifyConstants.Event_AddRelationshipEvent,
                      null,
                      userID.Select(item => ToRecipient(item)).ToArray(),
                      true,
                      new TagValue(NotifyConstants.Tag_EntityTitle, baseEntityData["title"]),
                      new TagValue(NotifyConstants.Tag_EntityID, baseEntityData["id"]),
                      new TagValue(NotifyConstants.Tag_EntityRelativeURL, baseEntityData["entityRelativeURL"]),
                      new TagValue(NotifyConstants.Tag_AdditionalData,
                      new Hashtable { 
                      { "Files", fileListInfoHashtable },
                      {"EventContent", entity.Content}}));

            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
                client.EndSingleRecipientEvent("send about add relationship event add");
            }


        }

        public void SendAboutExportCompleted(Guid recipientID, String fileName, String filePath)
        {
            if (recipientID == Guid.Empty) return;

            var recipient = ToRecipient(recipientID);

            client.SendNoticeToAsync(CoreContext.Configuration.CustomMode ? NotifyConstants.Event_ExportCompletedCustomMode : NotifyConstants.Event_ExportCompleted,
               null,
               new[] { recipient },
               true,
               new TagValue(NotifyConstants.Tag_EntityTitle, fileName),
               new TagValue(NotifyConstants.Tag_EntityRelativeURL, filePath));

        }

        public void SendAboutImportCompleted(Guid recipientID, EntityType entityType)
        {
            if (recipientID == Guid.Empty) return;

            var recipient = ToRecipient(recipientID);

            var entitiyListTitle = "";
            var entitiyListRelativeURL = "";
            switch (entityType)
            {
                case EntityType.Contact:
                    entitiyListTitle = CRMContactResource.Contacts;
                    entitiyListRelativeURL = "products/crm/";
                    break;
                case EntityType.Opportunity:
                    entitiyListTitle = CRMCommonResource.DealModuleName;
                    entitiyListRelativeURL = "products/crm/deals.aspx";
                    break;
                case EntityType.Case:
                    entitiyListTitle = CRMCommonResource.CasesModuleName;
                    entitiyListRelativeURL = "products/crm/cases.aspx";
                    break;
                case EntityType.Task:
                    entitiyListTitle = CRMCommonResource.TaskModuleName;
                    entitiyListRelativeURL = "products/crm/tasks.aspx";
                    break;
                default:
                    throw new ArgumentException(CRMErrorsResource.EntityTypeUnknown);
            }

            client.SendNoticeToAsync(
                CoreContext.Configuration.CustomMode ? NotifyConstants.Event_ImportCompletedCustomMode : NotifyConstants.Event_ImportCompleted,
                null,
                new[] { recipient },
                true,
                new TagValue(NotifyConstants.Tag_EntityListRelativeURL, entitiyListRelativeURL),
                new TagValue(NotifyConstants.Tag_EntityListTitle, entitiyListTitle));
        }

        public static void SendAutoReminderAboutTask(DateTime scheduleDate)
        {
            using (var scope = DIHelper.Resolve(-1))
            {
                var defaultDao = scope.Resolve<DaoFactory>();

                var execAlert = new List<int>();

                foreach (var row in defaultDao.TaskDao
                    .GetInfoForReminder(scheduleDate))
                {

                    var tenantId = Convert.ToInt32(row[0]);
                    var taskId = Convert.ToInt32(row[1]);
                    var deadline = Convert.ToDateTime(row[2]);
                    var alertValue = Convert.ToInt32(row[3]);
                    var responsibleID = !string.IsNullOrEmpty(Convert.ToString(row[4]))
                        ? new Guid(Convert.ToString(row[4]))
                        : Guid.Empty;

                    var deadlineReminderDate = deadline.AddMinutes(-alertValue);

                    if (deadlineReminderDate.Subtract(scheduleDate).Minutes > 1) continue;

                    execAlert.Add(taskId);

                    var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                    if (tenant == null ||
                        tenant.Status != TenantStatus.Active ||
                        TariffState.NotPaid <= CoreContext.PaymentManager.GetTariff(tenant.TenantId).State)
                    {
                        continue;
                    }

                    try
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                        var user = CoreContext.UserManager.GetUsers(responsibleID);

                        if (!(!Constants.LostUser.Equals(user) && user.Status == EmployeeStatus.Active)) continue;

                        SecurityContext.AuthenticateMe(user.ID);

                        Thread.CurrentThread.CurrentCulture = user.GetCulture();
                        Thread.CurrentThread.CurrentUICulture = user.GetCulture();

                        using (var innerScope = DIHelper.Resolve(tenantId))
                        {
                            var dao = innerScope.Resolve<DaoFactory>();
                            var task = dao.TaskDao.GetByID(taskId);

                            if (task == null) continue;

                            ASC.CRM.Core.Entities.Contact taskContact = null;
                            ASC.CRM.Core.Entities.Cases taskCase = null;
                            ASC.CRM.Core.Entities.Deal taskDeal = null;

                            if (task.ContactID > 0)
                            {
                                taskContact = dao.ContactDao.GetByID(task.ContactID);
                            }

                            if (task.EntityID > 0)
                            {
                                switch (task.EntityType)
                                {
                                    case EntityType.Case:
                                        taskCase = dao.CasesDao.GetByID(task.EntityID);
                                        break;
                                    case EntityType.Opportunity:
                                        taskDeal = dao.DealDao.GetByID(task.EntityID);
                                        break;
                                }
                            }

                            var listItem = dao.ListItemDao.GetByID(task.CategoryID);

                            NotifyClient.Instance.SendTaskReminder(task,
                                listItem != null ? listItem.Title : string.Empty,
                                taskContact, taskCase, taskDeal);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("ASC.CRM.Tasks")
                            .Error("SendAutoReminderAboutTask, tenant: " + tenant.TenantDomain, ex);
                    }
                }

                defaultDao.TaskDao.ExecAlert(execAlert);
            }
        }

        public void SendTaskReminder(Task task, String taskCategoryTitle, Contact taskContact, ASC.CRM.Core.Entities.Cases taskCase, ASC.CRM.Core.Entities.Deal taskDeal)
        {
            var recipient = ToRecipient(task.ResponsibleID);

            if (recipient == null) return;

            var deadLineString = task.DeadLine.Hour == 0 && task.DeadLine.Minute == 0
                ? task.DeadLine.ToShortDateString()
                : task.DeadLine.ToString(CultureInfo.InvariantCulture);

            string taskContactRelativeUrl = null;
            string taskContactTitle = null;

            string taskCaseRelativeUrl = null;
            string taskCaseTitle = null;

            string taskDealRelativeUrl = null;
            string taskDealTitle = null;

            if (taskContact != null)
            {
                taskContactRelativeUrl = String.Format("products/crm/default.aspx?id={0}{1}", taskContact.ID, taskContact is Person ? "&type=people" : "");
                taskContactTitle = taskContact.GetTitle();
            }

            if (taskCase != null)
            {
                taskCaseRelativeUrl = String.Format("products/crm/cases.aspx?id={0}", taskCase.ID);
                taskCaseTitle = taskCase.Title.HtmlEncode();
            }

            if (taskDeal != null)
            {
                taskDealRelativeUrl = String.Format("products/crm/deals.aspx?id={0}", taskDeal.ID);
                taskDealTitle = taskDeal.Title.HtmlEncode();
            }


            client.SendNoticeToAsync(
              NotifyConstants.Event_TaskReminder,
              null,
              new[] { recipient },
              true,
              new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
              new TagValue(NotifyConstants.Tag_AdditionalData,
                 new Hashtable { 
                      { "TaskDescription", HttpUtility.HtmlEncode(task.Description) },
                      { "TaskCategory", taskCategoryTitle },

                      { "ContactRelativeUrl", taskContactRelativeUrl },
                      { "ContactTitle", taskContactTitle },

                      { "CaseRelativeUrl", taskCaseRelativeUrl },
                      { "CaseTitle", taskCaseTitle },

                      { "DealRelativeUrl", taskDealRelativeUrl },
                      { "DealTitle", taskDealTitle },

                      { "DueDate", deadLineString }
                 })
            );
        }

        public void SendAboutResponsibleByTask(Task task, String taskCategoryTitle, Contact taskContact, ASC.CRM.Core.Entities.Cases taskCase, ASC.CRM.Core.Entities.Deal taskDeal, Hashtable fileListInfoHashtable)
        {
            var recipient = ToRecipient(task.ResponsibleID);

            if (recipient == null) return;

            task.DeadLine = TenantUtil.DateTimeFromUtc(task.DeadLine);
            var deadLineString = task.DeadLine.Hour == 0 && task.DeadLine.Minute == 0
                ? task.DeadLine.ToShortDateString()
                : task.DeadLine.ToString();


            string taskContactRelativeUrl = null;
            string taskContactTitle = null;

            string taskCaseRelativeUrl = null;
            string taskCaseTitle = null;

            string taskDealRelativeUrl = null;
            string taskDealTitle = null;

            if (taskContact != null)
            {
                taskContactRelativeUrl = String.Format("products/crm/default.aspx?id={0}{1}", taskContact.ID, taskContact is Person ? "&type=people" : "");
                taskContactTitle = taskContact.GetTitle();
            }

            if (taskCase != null)
            {
                taskCaseRelativeUrl = String.Format("products/crm/cases.aspx?id={0}", taskCase.ID);
                taskCaseTitle = taskCase.Title.HtmlEncode();
            }

            if (taskDeal != null)
            {
                taskDealRelativeUrl = String.Format("products/crm/deals.aspx?id={0}", taskDeal.ID);
                taskDealTitle = taskDeal.Title.HtmlEncode();
            }
           
            client.SendNoticeToAsync(
               NotifyConstants.Event_ResponsibleForTask,
               null,
               new[] { recipient },
               true,
               new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
               new TagValue(NotifyConstants.Tag_AdditionalData,
                 new Hashtable { 
                      { "TaskDescription", HttpUtility.HtmlEncode(task.Description) },
                      { "Files", fileListInfoHashtable },
                      { "TaskCategory", taskCategoryTitle },

                      { "ContactRelativeUrl", taskContactRelativeUrl },
                      { "ContactTitle", taskContactTitle },

                      { "CaseRelativeUrl", taskCaseRelativeUrl },
                      { "CaseTitle", taskCaseTitle },

                      { "DealRelativeUrl", taskDealRelativeUrl },
                      { "DealTitle", taskDealTitle },

                      { "DueDate", deadLineString }
                 })
               );
        }

        public void SendAboutResponsibleForOpportunity(Deal deal)
        {
            var recipient = ToRecipient(deal.ResponsibleID);

            if (recipient == null) return;

            client.SendNoticeToAsync(
            NotifyConstants.Event_ResponsibleForOpportunity,
            null,
            new[] { recipient },
            true,
            new TagValue(NotifyConstants.Tag_EntityTitle, deal.Title),
            new TagValue(NotifyConstants.Tag_EntityID, deal.ID),
            new TagValue(NotifyConstants.Tag_AdditionalData,
            new Hashtable { 
                      { "OpportunityDescription", HttpUtility.HtmlEncode(deal.Description) }
                 })
            );
        }

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }

        public INotifyClient Client
        {
            get { return client; }
        }

        private NotifyClient(INotifyClient client, INotifySource source)
        {
            this.client = client;
            this.source = source;
        }

    }
}