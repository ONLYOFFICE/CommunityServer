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
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Threading.Workers;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.VoipService;
using ASC.VoipService.Dao;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;
using Autofac;

namespace ASC.Web.CRM.Classes
{
    public class VoipEngine
    {
        private static readonly WorkerQueue<QueueItem> Queue = new WorkerQueue<QueueItem>(1, TimeSpan.FromMinutes(30));
        private static readonly object Locker = new object();
        private readonly DaoFactory daoFactory;

        public VoipEngine(DaoFactory daoFactory)
        {
            this.daoFactory = daoFactory;
        }
        public VoipCall SaveOrUpdateCall(VoipCall callHistory)
        {
            var dao = daoFactory.VoipDao;
            var call = dao.GetCall(callHistory.Id) ?? callHistory;

            if (string.IsNullOrEmpty(call.ParentID))
            {
                GetContact(call);
            }

            if (!callHistory.AnsweredBy.Equals(Guid.Empty))
            {
                call.AnsweredBy = callHistory.AnsweredBy;
            }

            if (!callHistory.Date.Equals(default(DateTime)))
            {
                call.Date = callHistory.Date;
            }

            if (!callHistory.EndDialDate.Equals(default(DateTime)))
            {
                call.EndDialDate = callHistory.EndDialDate;
            }

            if (call.Price == 0 && callHistory.Price != default(decimal))
            {
                call.Price = callHistory.Price;
            }

            if (call.DialDuration == 0)
            {
                call.DialDuration = callHistory.DialDuration;
            }

            if (call.VoipRecord == null)
            {
                call.VoipRecord = new VoipRecord();
            }

            if (string.IsNullOrEmpty(call.VoipRecord.Id))
            {
                call.VoipRecord.Id = callHistory.VoipRecord.Id;
            }

            if (call.VoipRecord.Price == default(decimal))
            {
                call.VoipRecord.Price = callHistory.VoipRecord.Price;
            }

            if (string.IsNullOrEmpty(call.VoipRecord.Uri))
            {
                call.VoipRecord.Uri = callHistory.VoipRecord.Uri;
            }

            if (call.VoipRecord.Duration == 0)
            {
                call.VoipRecord.Duration = callHistory.VoipRecord.Duration;
            }

            if (callHistory.Status.HasValue)
            {
                call.Status = callHistory.Status;
            }

            return dao.SaveOrUpdateCall(call);
        }

        public static void AddHistoryToCallContact(VoipCall call, DaoFactory daoFactory)
        {
            var listItemDao = daoFactory.ListItemDao;

            if (call == null || call.ContactId == 0) return;

            var category = listItemDao.GetByTitle(ListType.HistoryCategory, CRMCommonResource.HistoryCategory_Call);
            if (category == null)
            {
                category = new ListItem(CRMCommonResource.HistoryCategory_Call, "event_category_call.png");
                category.ID = listItemDao.CreateItem(ListType.HistoryCategory, category);
            }
            var contact = daoFactory.ContactDao.GetByID(call.ContactId);
            if (contact != null && CRMSecurity.CanAccessTo(contact))
            {
                var note = call.Status == VoipCallStatus.Incoming || call.Status == VoipCallStatus.Answered
                    ? CRMContactResource.HistoryVoipIncomingNote
                    : CRMContactResource.HistoryVoipOutcomingNote;
                var content = string.Format(note, call.DialDuration);

                var relationshipEvent = new RelationshipEvent
                {
                    CategoryID = category.ID,
                    EntityType = EntityType.Any,
                    EntityID = 0,
                    Content = content,
                    ContactID = contact.ID,
                    CreateOn = TenantUtil.DateTimeFromUtc(DateTime.UtcNow),
                    CreateBy = SecurityContext.CurrentAccount.ID
                };

                daoFactory.RelationshipEventDao.CreateItem(relationshipEvent);
            }
        }

        public Contact GetContact(VoipCall call)
        {
            if (call.ContactId != 0)
            {
                return null;
            }

            var contactPhone = call.Status == VoipCallStatus.Incoming || call.Status == VoipCallStatus.Answered ? call.From : call.To;

            var newContactIds = daoFactory.ContactDao.GetContactIDsByContactInfo(ContactInfoType.Phone, contactPhone.TrimStart('+'), null, true);

            foreach (var newContactId in newContactIds)
            {
                if (newContactId != 0)
                {
                    var existContact = daoFactory.ContactDao.GetByID(newContactId);
                    if (CRMSecurity.CanAccessTo(existContact))
                    {
                        call.ContactId = newContactId;
                        return existContact;
                    }
                }
            }

            return null;
        }

        public List<Contact> GetContacts(string contactPhone, DaoFactory daoFactory)
        {
            var dao = daoFactory.ContactDao;
            var ids = dao.GetContactIDsByContactInfo(ContactInfoType.Phone, contactPhone.TrimStart('+'), null, true);
            return ids.Select(r => dao.GetByID(r)).ToList();
        }

        public void SaveAdditionalInfo(string callId)
        {
            lock (Locker)
            {
                if (!Queue.IsStarted)
                {
                    Queue.Start(SaveAdditionalInfoAction);
                }

                Queue.Add(new QueueItem {CallID = callId, TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId});
            }
        }

        private static void SaveAdditionalInfoAction(QueueItem queueItem)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(queueItem.TenantID);
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var voipEngine = new VoipEngine(daoFactory);
                    var dao = daoFactory.VoipDao;

                    var call = dao.GetCall(queueItem.CallID);

                    GetPriceAndDuration(call);

                    if (call.ChildCalls.Any())
                    {
                        call.ChildCalls.ForEach(r =>
                        {
                            GetPriceAndDuration(r);
                            voipEngine.SaveOrUpdateCall(r);
                        });
                    }

                    call = voipEngine.SaveOrUpdateCall(call);

                    if (!string.IsNullOrEmpty(call.VoipRecord.Id))
                    {
                        call.VoipRecord = VoipDao.GetProvider().GetRecord(call.Id, call.VoipRecord.Id);
                        voipEngine.SaveOrUpdateCall(call);
                    }

                    SecurityContext.AuthenticateMe(call.AnsweredBy);
                    AddHistoryToCallContact(call, daoFactory);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").ErrorFormat("SaveAdditionalInfo {0}, {1}", ex, ex.StackTrace);
            }
        }

        private static void GetPriceAndDuration(VoipCall call)
        {
            var provider = VoipDao.GetProvider();
            var twilioCall = provider.GetCall(call.Id);
            call.Price = twilioCall.Price;
            call.DialDuration = twilioCall.DialDuration;
        }

        public void AnswerCall(VoipCall call)
        {
            call.AnsweredBy = SecurityContext.CurrentAccount.ID;
            call.Status = VoipCallStatus.Answered;
            daoFactory.VoipDao.SaveOrUpdateCall(call);
        }

        public Contact CreateContact(string contactPhone)
        {
            var contact = new Person
            {
                FirstName = contactPhone,
                LastName = TenantUtil.DateTimeFromUtc(DateTime.UtcNow).ToString("yyyy-MM-dd hh:mm"),
                ShareType = ShareType.None,
                CreateBy = SecurityContext.CurrentAccount.ID,
                CreateOn = DateTime.UtcNow
            };

            contact.ID = daoFactory.ContactDao.SaveContact(contact);

            daoFactory.ContactInfoDao
                .Save(new ContactInfo
                {
                    ContactID = contact.ID,
                    IsPrimary = true,
                    InfoType = ContactInfoType.Phone,
                    Data = contactPhone
                });

            CRMSecurity.SetAccessTo(contact, new List<Guid> {SecurityContext.CurrentAccount.ID});

            return contact;
        }

        private class QueueItem
        {
            public int TenantID { get; set; }
            public string CallID { get; set; }
        }
    }
}