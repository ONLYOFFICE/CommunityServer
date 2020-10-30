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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Core.Dao.Expressions.Contact;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Search;
using ASC.Mail.Enums;
using ASC.Mail.Utils;
using ASC.Web.Core;

namespace ASC.Mail.Core.Engine
{
    public class ContactEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public ContactEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ContactEngine");
        }

        public List<ContactCard> GetContactCards(IContactsExp exp)
        {
            if (exp == null)
                throw new ArgumentNullException("exp");

            using (var daoFactory = new DaoFactory())
            {
                var daoContacts = daoFactory.CreateContactCardDao(Tenant, User);

                var list = daoContacts.GetContactCards(exp);

                return list;
            }
        }

        public int GetContactCardsCount(IContactsExp exp)
        {
            if (exp == null)
                throw new ArgumentNullException("exp");

            using (var daoFactory = new DaoFactory())
            {
                var daoContacts = daoFactory.CreateContactCardDao(Tenant, User);

                var count = daoContacts.GetContactCardsCount(exp);

                return count;
            }
        }

        public ContactCard GetContactCard(int id)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoContacts = daoFactory.CreateContactCardDao(Tenant, User);

                var contactCard = daoContacts.GetContactCard(id);

                return contactCard;
            }
        }

        public ContactCard SaveContactCard(ContactCard contactCard)
        {
            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoContact = daoFactory.CreateContactDao(Tenant, User);
                    var daoContactInfo = daoFactory.CreateContactInfoDao(Tenant, User);

                    var contactId = daoContact.SaveContact(contactCard.ContactInfo);

                    contactCard.ContactInfo.Id = contactId;

                    foreach (var contactItem in contactCard.ContactItems)
                    {
                        contactItem.ContactId = contactId;

                        var contactItemId = daoContactInfo.SaveContactInfo(contactItem);

                        contactItem.Id = contactItemId;
                    }

                    tx.Commit();
                }
            }

            var factory = new EngineFactory(Tenant, User, Log);

            Log.Debug("IndexEngine->SaveContactCard()");

            factory.IndexEngine.Add(contactCard.ToMailContactWrapper());

            return contactCard;
        }

        public ContactCard UpdateContactCard(ContactCard newContactCard)
        {
            var contactId = newContactCard.ContactInfo.Id;

            if(contactId < 0)
                throw new ArgumentException("Invalid contact id");

            var contactCard = GetContactCard(contactId);

            if (null == contactCard)
                throw new ArgumentException("Contact not found");

            var contactChanged = !contactCard.ContactInfo.Equals(newContactCard.ContactInfo);

            var newContactItems = newContactCard.ContactItems.Where(c => !contactCard.ContactItems.Contains(c)).ToList();

            var removedContactItems = contactCard.ContactItems.Where(c => !newContactCard.ContactItems.Contains(c)).ToList();

            if (!contactChanged && !newContactItems.Any() && !removedContactItems.Any())
                return contactCard;

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    if (contactChanged)
                    {
                        var daoContact = daoFactory.CreateContactDao(Tenant, User);

                        daoContact.SaveContact(newContactCard.ContactInfo);

                        contactCard.ContactInfo = newContactCard.ContactInfo;
                    }

                    var daoContactInfo = daoFactory.CreateContactInfoDao(Tenant, User);

                    if (newContactItems.Any())
                    {
                        foreach (var contactItem in newContactItems)
                        {
                            contactItem.ContactId = contactId;

                            var contactItemId = daoContactInfo.SaveContactInfo(contactItem);

                            contactItem.Id = contactItemId;

                            contactCard.ContactItems.Add(contactItem);
                        }
                    }

                    if (removedContactItems.Any())
                    {
                        foreach (var contactItem in removedContactItems)
                        {
                            daoContactInfo.RemoveContactInfo(contactItem.Id);

                            contactCard.ContactItems.Remove(contactItem);
                        }
                    }

                    tx.Commit();
                }
            }

            var factory = new EngineFactory(Tenant, User, Log);

            Log.Debug("IndexEngine->UpdateContactCard()");

            factory.IndexEngine.Update(new List<MailContactWrapper> {contactCard.ToMailContactWrapper()});

            return contactCard;
        }

        public void RemoveContacts(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var daoContact = daoFactory.CreateContactDao(Tenant, User);

                    daoContact.RemoveContacts(ids);

                    var daoContactInfo = daoFactory.CreateContactInfoDao(Tenant, User);

                    daoContactInfo.RemoveByContactIds(ids);

                    tx.Commit();
                }
            }

            var factory = new EngineFactory(Tenant, User, Log);

            Log.Debug("IndexEngine->RemoveContacts()");

            factory.IndexEngine.RemoveContacts(ids, Tenant, new Guid(User));
        }

        /// <summary>
        /// Search emails in Accounts, Mail, CRM, Peaople Contact System
        /// </summary>
        /// <param name="tenant">Tenant id</param>
        /// <param name="userName">User id</param>
        /// <param name="term">Search word</param>
        /// <param name="maxCountPerSystem">limit result per Contact System</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="httpContextScheme"></param>
        /// <returns></returns>
        public List<string> SearchEmails(int tenant, string userName, string term, int maxCountPerSystem, string httpContextScheme, int timeout = -1)
        {
            var equality = new ContactEqualityComparer();
            var contacts = new List<string>();
            var userGuid = new Guid(userName);

            var watch = new Stopwatch();

            watch.Start();

            var apiHelper = new ApiHelper(httpContextScheme, Log);

            var taskList = new List<Task<List<string>>>()
            {
                Task.Run(() =>
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    SecurityContext.AuthenticateMe(userGuid);

                    var engine = new EngineFactory(tenant, userName);

                    var exp = new FullFilterContactsExp(tenant, userName, term, infoType: ContactInfoType.Email, orderAsc: true, limit: maxCountPerSystem);

                    var contactCards = engine.ContactEngine.GetContactCards(exp);

                    return (from contactCard in contactCards
                        from contactItem in contactCard.ContactItems
                        select
                            string.IsNullOrEmpty(contactCard.ContactInfo.ContactName)
                                ? contactItem.Data
                                : MailUtil.CreateFullEmail(contactCard.ContactInfo.ContactName, contactItem.Data))
                        .ToList();
                }),

                Task.Run(() =>
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    SecurityContext.AuthenticateMe(userGuid);

                    var engine = new EngineFactory(tenant, userGuid.ToString());
                    return engine.AccountEngine.SearchAccountEmails(term);
                }),

                Task.Run(() =>
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    SecurityContext.AuthenticateMe(userGuid);

                    return WebItemSecurity.IsAvailableForMe(WebItemManager.CRMProductID)
                        ? apiHelper.SearchCrmEmails(term, maxCountPerSystem)
                        : new List<string>();
                }),

                Task.Run(() =>
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    SecurityContext.AuthenticateMe(userGuid);

                    return WebItemSecurity.IsAvailableForMe(WebItemManager.PeopleProductID)
                        ? apiHelper.SearchPeopleEmails(term, 0, maxCountPerSystem)
                        : new List<string>();
                })
            };

            try
            {
                var taskArray = taskList.ToArray<Task>();

                Task.WaitAll(taskArray, timeout);

                watch.Stop();
            }
            catch (AggregateException e)
            {
                watch.Stop();

                var errorText =
                    new StringBuilder("SearchEmails: \nThe following exceptions have been thrown by WaitAll():");

                foreach (var t in e.InnerExceptions)
                {
                    errorText
                        .AppendFormat("\n-------------------------------------------------\n{0}", t);
                }

                Log.Error(errorText.ToString());
            }

            contacts =
                taskList.Aggregate(contacts,
                    (current, task) => !task.IsFaulted
                                       && task.IsCompleted
                                       && !task.IsCanceled
                        ? current.Concat(task.Result).ToList()
                        : current)
                    .Distinct(equality)
                    .ToList();

            Log.DebugFormat("SearchEmails (term = '{0}'): {1} sec / {2} items", term, watch.Elapsed.TotalSeconds, contacts.Count);

            return contacts;
        }

        public class ContactEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string contact1, string contact2)
            {
                if (contact1 == null && contact2 == null)
                    return true;

                if (contact1 == null || contact2 == null)
                    return false;

                var contact1Parts = contact1.Split('<');
                var contact2Parts = contact2.Split('<');

                return contact1Parts.Last().Replace(">", "") == contact2Parts.Last().Replace(">", "");
            }

            public int GetHashCode(string str)
            {
                var strParts = str.Split('<');
                return strParts.Last().Replace(">", "").GetHashCode();
            }
        }
    }
}
