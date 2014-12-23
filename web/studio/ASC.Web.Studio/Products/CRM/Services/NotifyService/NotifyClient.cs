/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using ASC.Web.CRM.Classes;
using ASC.CRM.Core;
using System.Collections.Specialized;
using ASC.Web.CRM.Resources;
using log4net;

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

        public void SendAboutSetAccess(EntityType entityType, int entityID, params Guid[] userID)
        {
            if (userID.Length == 0) return;

            var baseData = ExtractBaseDataFrom(entityType, entityID);

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

        private NameValueCollection ExtractBaseDataFrom(EntityType entityType, int entityID)
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
                        var contact = Global.DaoFactory.GetContactDao().GetByID(entityID);
                        title = contact != null ? contact.GetTitle() : string.Empty;
                        relativeURL = "default.aspx?id=" + entityID;
                        break;
                    }
                case EntityType.Opportunity:
                    {
                        var deal = Global.DaoFactory.GetDealDao().GetByID(entityID);
                        title = deal != null ? deal.Title : string.Empty;
                        relativeURL = "deals.aspx?id=" + entityID;
                        break;
                    }
                case EntityType.Case:
                    {
                        var cases = Global.DaoFactory.GetCasesDao().GetByID(entityID);
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

        public void SendAboutAddRelationshipEventAdd(RelationshipEvent entity,
                                                    Hashtable fileListInfoHashtable, params Guid[] userID)
        {
            if (userID.Length == 0) return;

            NameValueCollection baseEntityData;

            if (entity.EntityID != 0)
            {
                baseEntityData = ExtractBaseDataFrom(entity.EntityType, entity.EntityID);
            }
            else
            {
                var contact = Global.DaoFactory.GetContactDao().GetByID(entity.ContactID);

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

        public void SendAboutExportCompleted(Guid recipientID, String filePath)
        {
            if (recipientID == Guid.Empty) return;

            var recipient = ToRecipient(recipientID);

            client.SendNoticeToAsync(NotifyConstants.Event_ExportCompleted,
               null,
               new[] { recipient },
               true,
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
                    throw new ArgumentException("entity type is unknown");
            }

            client.SendNoticeToAsync(
                NotifyConstants.Event_ImportCompleted,
                null,
                new[] { recipient },
                true,
                new TagValue(NotifyConstants.Tag_EntityListRelativeURL, entitiyListRelativeURL),
                new TagValue(NotifyConstants.Tag_EntityListTitle, entitiyListTitle));
        }

        public static void SendAutoReminderAboutTask(DateTime scheduleDate)
        {
            var execAlert = new List<int>();

            var defaultDao = new DaoFactory(Tenant.DEFAULT_TENANT, CRMConstants.StorageModule);
            
            foreach (var row in defaultDao.GetTaskDao()
                                  .GetInfoForReminder(scheduleDate))
            {

                var tenantId = Convert.ToInt32(row[0]);
                var taskId = Convert.ToInt32(row[1]);
                var deadline = Convert.ToDateTime(row[2]);
                var alertValue = Convert.ToInt32(row[3]);
                var responsibleID = !string.IsNullOrEmpty(Convert.ToString(row[4])) ? new Guid(Convert.ToString(row[4])) : Guid.Empty;
                
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

                    var task = new DaoFactory(tenantId, CRMConstants.StorageModule).GetTaskDao().GetByID(taskId);

                     if (task == null) continue;

                    NotifyClient.Instance.SendTaskReminder(task);

                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC.CRM.Tasks").Error("SendAutoReminderAboutTask, tenant: " + tenant.TenantDomain, ex);
                }
            }

            defaultDao.GetTaskDao().ExecAlert(execAlert);
        }

        public void SendTaskReminder(Task task)
        {
            var recipient = ToRecipient(task.ResponsibleID);

            if (recipient == null) return;

            var deadLineString = task.DeadLine.Hour == 0 && task.DeadLine.Minute == 0
                ? task.DeadLine.ToShortDateString()
                : task.DeadLine.ToString(CultureInfo.InvariantCulture);

            client.SendNoticeToAsync(
              NotifyConstants.Event_TaskReminder,
              null,
              new[] { recipient },
              true,
              new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
              new TagValue(NotifyConstants.Tag_AdditionalData,
                 new Hashtable { 
                      { "TaskDescription", HttpUtility.HtmlEncode(task.Description) },
                      { "DueDate", deadLineString }
                 })
            );
        }

        public void SendAboutResponsibleByTask(Task task, String taskCategoryTitle, String taskContactName, Hashtable fileListInfoHashtable)
        {
            var recipient = ToRecipient(task.ResponsibleID);

            if (recipient == null) return;

            task.DeadLine = TenantUtil.DateTimeFromUtc(task.DeadLine);
            var deadLineString = task.DeadLine.Hour == 0 && task.DeadLine.Minute == 0
                ? task.DeadLine.ToShortDateString()
                : task.DeadLine.ToString();

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
                      { "LinkWithContact", taskContactName },
                      { "DueDate", deadLineString }
                 }));
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
                 }));
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