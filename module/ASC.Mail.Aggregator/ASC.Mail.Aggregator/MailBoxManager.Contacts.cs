/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.Filter;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.FullTextIndex;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Web.Core;
using Task = System.Threading.Tasks.Task;

namespace ASC.Mail.Aggregator
{
    public enum ContactInfoType
    {
        Email = 1,
        Phone
    }

    public enum ContactType
    {
        FrequentlyContacted = 0,
        Personal
    }

    public partial class MailBoxManager
    {
        // ToDo: replace with setting value
        public const int FulltextsearchIdsCount = 200;

        #region crm defines

        private const int CRM_CONTACT_ENTITY_TYPE = 0;
        #endregion

        #region public methods

        public List<ContactCardDto> GetMailContacts(int tenant, string user, MailContactsFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            var result = new List<ContactCardDto>();
            const string mail_contacts = "mc";
            const string contact_info = "ci";
            var sortOrder = filter.SortOrder == "ascending";

            using (var db = GetDb())
            {
                var ids = GetFilteredContactIds(db, tenant, user, filter);

                if (!ids.Any())
                    return result;

                var queryContacts = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                    .InnerJoin(ContactInfoTable.Name.Alias(contact_info),
                               Exp.EqColumns(ContactsTable.Columns.Id.Prefix(mail_contacts),
                                             ContactInfoTable.Columns.ContactId.Prefix(contact_info)))
                    .Select(
                        ContactsTable.Columns.Id.Prefix(mail_contacts),
                        ContactsTable.Columns.User.Prefix(mail_contacts),
                        ContactsTable.Columns.Tenant.Prefix(mail_contacts),
                        ContactsTable.Columns.ContactName.Prefix(mail_contacts),
                        ContactsTable.Columns.Description.Prefix(mail_contacts),
                        ContactsTable.Columns.Type.Prefix(mail_contacts),
                        ContactsTable.Columns.HasPhoto.Prefix(mail_contacts),
                        ContactInfoTable.Columns.Id.Prefix(contact_info),
                        ContactInfoTable.Columns.Tenant.Prefix(contact_info),
                        ContactInfoTable.Columns.User.Prefix(contact_info),
                        ContactInfoTable.Columns.ContactId.Prefix(contact_info),
                        ContactInfoTable.Columns.Data.Prefix(contact_info),
                        ContactInfoTable.Columns.Type.Prefix(contact_info),
                        ContactInfoTable.Columns.IsPrimary.Prefix(contact_info))
                    .Where(ContactsTable.Columns.Tenant.Prefix(mail_contacts), tenant)
                    .Where(ContactsTable.Columns.User.Prefix(mail_contacts), user)
                    .Where(Exp.In(ContactsTable.Columns.Id.Prefix(mail_contacts), ids))
                    .OrderBy(ContactsTable.Columns.ContactName.Prefix(mail_contacts), sortOrder);

                result = db.ExecuteList(queryContacts).ToContactCardDto();
            }
            return result;
        }

        public List<ContactCardDto> GetContactsByContactInfo(int tenant, string user, ContactInfoType infoType, String data, bool? isPrimary)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            List<ContactCardDto> result;
            const string mail_contacts = "mc";
            const string contact_info = "ci";

            using (var db = GetDb())
            {
                var queryContacts = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                    .InnerJoin(ContactInfoTable.Name.Alias(contact_info),
                               Exp.EqColumns(ContactsTable.Columns.Id.Prefix(mail_contacts),
                                             ContactInfoTable.Columns.ContactId.Prefix(contact_info)))
                    .Select(
                        ContactsTable.Columns.Id.Prefix(mail_contacts),
                        ContactsTable.Columns.User.Prefix(mail_contacts),
                        ContactsTable.Columns.Tenant.Prefix(mail_contacts),
                        ContactsTable.Columns.ContactName.Prefix(mail_contacts),
                        ContactsTable.Columns.Description.Prefix(mail_contacts),
                        ContactsTable.Columns.Type.Prefix(mail_contacts),
                        ContactsTable.Columns.HasPhoto.Prefix(mail_contacts),
                        ContactInfoTable.Columns.Id.Prefix(contact_info),
                        ContactInfoTable.Columns.Tenant.Prefix(contact_info),
                        ContactInfoTable.Columns.User.Prefix(contact_info),
                        ContactInfoTable.Columns.ContactId.Prefix(contact_info),
                        ContactInfoTable.Columns.Data.Prefix(contact_info),
                        ContactInfoTable.Columns.Type.Prefix(contact_info),
                        ContactInfoTable.Columns.IsPrimary.Prefix(contact_info))
                    .Where(ContactInfoTable.Columns.Tenant.Prefix(contact_info), tenant)
                    .Where(ContactInfoTable.Columns.User.Prefix(contact_info), user)
                    .Where(ContactInfoTable.Columns.Data.Prefix(contact_info), data)
                    .Where(ContactInfoTable.Columns.Type.Prefix(contact_info), (int)infoType);

                if (isPrimary.HasValue)
                {
                    queryContacts = queryContacts.Where(ContactInfoTable.Columns.IsPrimary.Prefix(contact_info), isPrimary.Value);
                }

               result = db.ExecuteList(queryContacts).ToContactCardDto();
            }
            return result;
        }

        public int GetMailContactsCount(int tenant, string user, MailContactsFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            int result;

            using (var db = GetDb())
            {
                SqlQuery queryContacts;
                if (string.IsNullOrEmpty(filter.SearchFilter))
                {
                    queryContacts = new SqlQuery(ContactsTable.Name)
                        .SelectCount()
                        .Where(GetUserWhere(user, tenant));

                    if (filter.Type.HasValue)
                        queryContacts = queryContacts.Where(ContactsTable.Columns.Type, filter.Type);
                }
                else
                {
                    const string mail_contacts = "mc";
                    const string contact_info = "ci";

                    queryContacts = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                        .SelectCount("distinct " + ContactsTable.Columns.Id.Prefix(mail_contacts))
                        .InnerJoin(ContactInfoTable.Name.Alias(contact_info),
                                   Exp.EqColumns(ContactsTable.Columns.Id.Prefix(mail_contacts),
                                                 ContactInfoTable.Columns.ContactId.Prefix(contact_info)))
                        .Where(ContactsTable.Columns.Tenant.Prefix(mail_contacts), tenant)
                        .Where(ContactsTable.Columns.User.Prefix(mail_contacts), user);
                    
                    if (filter.Type.HasValue)
                        queryContacts = queryContacts.Where(ContactsTable.Columns.Type.Prefix(mail_contacts), filter.Type);

                    if (FullTextSearch.SupportModule(FullTextSearch.MailContactsModule))
                    {
                        var ids = FullTextSearch.Search(FullTextSearch.MailContactsModule.Match(filter.SearchFilter));
                        queryContacts = queryContacts.Where(Exp.In(ContactInfoTable.Columns.Id, ids));
                    }
                    else
                    {
                        queryContacts = queryContacts.Where(
                            Exp.Or(Exp.Like(mail_contacts + '.' + ContactsTable.Columns.Description, filter.SearchFilter,
                                         SqlLike.StartWith),
                                Exp.Or(Exp.Like(mail_contacts + '.' + ContactsTable.Columns.ContactName, filter.SearchFilter,
                                         SqlLike.StartWith),
                                Exp.Like(contact_info + '.' + ContactInfoTable.Columns.Data, filter.SearchFilter,
                                         SqlLike.StartWith))));
                    }
                }
                result = db.ExecuteList(queryContacts).ConvertAll(r => Convert.ToInt32(r[0])).FirstOrDefault();
            }

            return result;
        }

        public ContactCardDto SaveMailContact(int tenant, string user, string name, string description, 
            List<string> emails, List<string> phoneNumbers, ContactType type)
        {
            var data = new List<ContactInfoDto>();
            int contactId;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    var queryContact = new SqlInsert(ContactsTable.Name, true)
                                    .InColumnValue(ContactsTable.Columns.Id, 0)
                                    .InColumnValue(ContactsTable.Columns.User, user)
                                    .InColumnValue(ContactsTable.Columns.Tenant, tenant)
                                    .InColumnValue(ContactsTable.Columns.ContactName, name)
                                    .InColumnValue(ContactsTable.Columns.Address, emails[0])
                                    .InColumnValue(ContactsTable.Columns.Description, description)
                                    .InColumnValue(ContactsTable.Columns.Type, (int)type)
                                    .InColumnValue(ContactsTable.Columns.HasPhoto, false)
                                    .Identity(0, 0, true);

                    contactId = db.ExecuteScalar<int>(queryContact);

                    for(var i = 0; i < emails.Count; i++)
                    {
                        var isPrimary = i == 0;
                        var id = SaveContactInfo(db, tenant, user, contactId, emails[i], ContactInfoType.Email, isPrimary);
                        data.Add(new ContactInfoDto(id, tenant, user, contactId, emails[i], (int)ContactInfoType.Email, isPrimary));
                    }

                    for (var i = 0; i < phoneNumbers.Count; i++)
                    {
                        var isPrimary = i == 0;
                        var id = SaveContactInfo(db, tenant, user, contactId, phoneNumbers[i], ContactInfoType.Phone, isPrimary);
                        data.Add(new ContactInfoDto(id, tenant, user, contactId, phoneNumbers[i], (int)ContactInfoType.Phone, isPrimary));
                    }

                    tx.Commit();
                }
            }
            return new ContactCardDto(contactId, user, tenant, name, data, description, (int)type, false);
        }

        public ContactCardDto GetMailContact(int tenant, string user, int id)
        {
            const string mail_contacts = "mc";
            const string contact_info = "ci";

            using (var db = GetDb())
            {
                var queryContacts = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                    .InnerJoin(ContactInfoTable.Name.Alias(contact_info),
                               Exp.EqColumns(ContactsTable.Columns.Id.Prefix(mail_contacts),
                                             ContactInfoTable.Columns.ContactId.Prefix(contact_info)))
                    .Select(
                        ContactsTable.Columns.Id.Prefix(mail_contacts),
                        ContactsTable.Columns.User.Prefix(mail_contacts),
                        ContactsTable.Columns.Tenant.Prefix(mail_contacts),
                        ContactsTable.Columns.ContactName.Prefix(mail_contacts),
                        ContactsTable.Columns.Description.Prefix(mail_contacts),
                        ContactsTable.Columns.Type.Prefix(mail_contacts),
                        ContactsTable.Columns.HasPhoto.Prefix(mail_contacts),
                        ContactInfoTable.Columns.Id.Prefix(contact_info),
                        ContactInfoTable.Columns.Tenant.Prefix(contact_info),
                        ContactInfoTable.Columns.User.Prefix(contact_info),
                        ContactInfoTable.Columns.ContactId.Prefix(contact_info),
                        ContactInfoTable.Columns.Data.Prefix(contact_info),
                        ContactInfoTable.Columns.Type.Prefix(contact_info),
                        ContactInfoTable.Columns.IsPrimary.Prefix(contact_info))
                    .Where(ContactsTable.Columns.Tenant.Prefix(mail_contacts), tenant)
                    .Where(ContactsTable.Columns.User.Prefix(mail_contacts), user)
                    .Where(ContactsTable.Columns.Id.Prefix(mail_contacts), id);

                var result = db.ExecuteList(queryContacts).ToContactCardDto().FirstOrDefault();
                return result;
            }
        }

        public ContactCardDto UpdateMailContact(int tenant, string user, int id, string name, string description,
            List<string> emails, List<string> phoneNumbers, ContactType type)
        {
            var contactCard = GetMailContact(tenant, user, id);

            if (null == contactCard)
                throw new ArgumentException("Contact with specified id doesn't exist.");

            var data = new List<ContactInfoDto>();

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    if (contactCard.name != name || contactCard.description != description || contactCard.type != (int)type)
                    {
                        var updateContact = new SqlUpdate(ContactsTable.Name)
                            .Set(ContactsTable.Columns.ContactName, name)
                            .Set(ContactsTable.Columns.Description, description)
                            .Set(ContactsTable.Columns.Type, (int)type)
                            .Where(ContactsTable.Columns.Id, id)
                            .Where(ContactsTable.Columns.Tenant, tenant)
                            .Where(ContactsTable.Columns.User, user);

                        db.ExecuteNonQuery(updateContact);
                    }

                    var deleteContactInfo = new SqlDelete(ContactInfoTable.Name)
                        .Where(ContactInfoTable.Columns.ContactId, id)
                        .Where(ContactInfoTable.Columns.Tenant, tenant)
                        .Where(ContactInfoTable.Columns.User, user);

                    db.ExecuteNonQuery(deleteContactInfo);

                    for(var i = 0; i < emails.Count; i++)
                    {
                        var isPrimary = i == 0;
                        var idContactInfo = SaveContactInfo(db, tenant, user, id, emails[i], ContactInfoType.Email, isPrimary);
                        data.Add(new ContactInfoDto(idContactInfo, tenant, user, id, emails[i], (int)ContactInfoType.Email, isPrimary));
                    }

                    for(var i = 0; i < phoneNumbers.Count; i++)
                    {
                        var isPrimary = i == 0;
                        var idContactInfo = SaveContactInfo(db, tenant, user, id, phoneNumbers[i], ContactInfoType.Phone, isPrimary);
                        data.Add(new ContactInfoDto(idContactInfo, tenant, user, id, phoneNumbers[i], (int)ContactInfoType.Phone, isPrimary));
                    }

                    tx.Commit();
                }
            }

            return new ContactCardDto(id, user, tenant, name, data, description, (int)type, false);
        }

        public void DeleteMailContacts(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    var deleteContactInfo = new SqlDelete(ContactInfoTable.Name)
                           .Where(Exp.In(ContactInfoTable.Columns.ContactId, ids))
                           .Where(GetUserWhere(user, tenant));

                    db.ExecuteNonQuery(deleteContactInfo);

                    var deleteContact = new SqlDelete(ContactsTable.Name)
                            .Where(Exp.In(ContactsTable.Columns.Id, ids))
                            .Where(GetUserWhere(user, tenant));

                    db.ExecuteNonQuery(deleteContact);

                    tx.Commit();
                }
            }
        }

        public List<string> SearchMailContacts(int tenant, string user, string searchText, int maxCount)
        {
            var contacts = new List<string>();

            if (string.IsNullOrEmpty(searchText) || maxCount <= 0)
                return contacts;

            const string mail_contacts = "mc";
            const string contact_info = "ci";

            Func<SqlQuery> getBaseQuery = () =>
            {
                var query = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                    .InnerJoin(ContactInfoTable.Name.Alias(contact_info),
                        Exp.EqColumns(ContactsTable.Columns.Id.Prefix(mail_contacts),
                            ContactInfoTable.Columns.ContactId.Prefix(contact_info)))
                    .Select(
                        ContactsTable.Columns.ContactName.Prefix(mail_contacts),
                        ContactInfoTable.Columns.Data.Prefix(contact_info))
                    .Where(ContactInfoTable.Columns.User.Prefix(contact_info), user)
                    .Where(ContactInfoTable.Columns.Tenant.Prefix(contact_info), tenant)
                    .Where(ContactInfoTable.Columns.Type.Prefix(contact_info), (int) ContactInfoType.Email);

                return query;
            };

            var ids = new List<int>();

            if (FullTextSearch.SupportModule(FullTextSearch.MailContactsModule))
            {
                ids = FullTextSearch.Search(FullTextSearch.MailContactsModule.Match(searchText));

                if (!ids.Any())
                    return contacts;
            }

            const int count_per_query = 100;

            using (var db = GetDb())
            {
                var f = 0;
                do
                {
                    var query = getBaseQuery();

                    if (ids.Any())
                    {
                        var partIds = ids.Skip(f).Take(count_per_query).ToList();

                        if (!partIds.Any())
                            break;

                        query
                            .Where(Exp.In(ContactInfoTable.Columns.Id.Prefix(contact_info), partIds));
                    }
                    else
                    {
                        query
                            .Where(
                                Exp.Like(
                                    string.Format("concat({0}, ' <', {1},'>')",
                                        ContactsTable.Columns.ContactName.Prefix(mail_contacts),
                                        ContactInfoTable.Columns.Data.Prefix(contact_info)),
                                    searchText.Replace("\"", "\\\""),
                                    SqlLike.AnyWhere))
                            .SetMaxResults(maxCount);
                    }

                    var partContacts = db.ExecuteList(query)
                        .ConvertAll(r => string.IsNullOrEmpty(r[0] as string)
                                ? r[1] as string
                                : MailUtil.CreateFullEmail((string)r[0], r[1] as string));

                    foreach (var partContact in partContacts.TakeWhile(partContact => maxCount - contacts.Count != 0))
                    {
                        contacts.Add(partContact);
                    }

                    if (maxCount - contacts.Count == 0 || !partContacts.Any() || partContacts.Count < count_per_query)
                        break;

                    f += count_per_query;

                } while (true);
            }

            return contacts;
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

            var apiHelper = new ApiHelper(httpContextScheme);

            Task<List<string>>[] taskArray =
            {
                Task<List<string>>.Factory.StartNew(
                    () =>
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        SecurityContext.AuthenticateMe(userGuid);

                        return SearchMailContacts(tenant, userName, term, maxCountPerSystem).ToList();
                    }),

                Task<List<string>>.Factory.StartNew(() =>
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    SecurityContext.AuthenticateMe(userGuid);

                    return SearchAccountEmails(tenant, userName, term);
                }),

                Task<List<string>>.Factory.StartNew(
                    () =>
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        SecurityContext.AuthenticateMe(userGuid);

                        return WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(),
                            SecurityContext.CurrentAccount.ID)
                            ? apiHelper.SearchCrmEmails(term, maxCountPerSystem)
                            : new List<string>();
                    }),

                Task<List<string>>.Factory.StartNew(
                    () =>
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        SecurityContext.AuthenticateMe(userGuid);

                        return WebItemSecurity.IsAvailableForUser(WebItemManager.PeopleProductID.ToString(),
                            SecurityContext.CurrentAccount.ID)
                            ? apiHelper.SearchPeopleEmails(term, 0, maxCountPerSystem)
                            : new List<string>();
                    })
            };

            try
            {
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
                    errorText.AppendFormat("\n-------------------------------------------------\n{0}",
                        t);
                }

                _log.Error(errorText.ToString());
            }

            contacts =
                taskArray.Aggregate(contacts,
                    (current, task) => !task.IsFaulted
                                       && task.IsCompleted
                                       && !task.IsCanceled
                        ? current.Concat(task.Result).ToList()
                        : current)
                    .Distinct(equality)
                    .ToList();

            _log.Debug("SearchEmails (term = '{0}'): {1} sec / {2} items", term, watch.Elapsed.TotalSeconds, contacts.Count);

            return contacts;
        }

        public List<CrmContactEntity> GetLinkedCrmEntitiesId(int messageId, int tenant, string user)
        {
            using (var db = GetDb())
            {
                var chainInfo = GetMessageChainInfo(db, tenant, user, messageId);
                return GetLinkedCrmEntitiesId(db, chainInfo.id, chainInfo.mailbox, tenant);
            }
        }

        public List<CrmContactEntity> GetLinkedCrmEntitiesId(string chainId, int mailboxId, int tenant)
        {
            using (var db = GetDb())
            {
                return GetLinkedCrmEntitiesId(db, chainId, mailboxId, tenant);
            }
        }

        public List<int> GetCrmContactsId(int tenant, string user, string email)
        {
            var ids = new List<int>();
            return string.IsNullOrEmpty(email) ? ids : GetCrmContactsId(tenant, user, new List<string> { email });
        }

        public List<int> GetCrmContactsId(int tenant, string user, List<string> emails)
        {
            var ids = new List<int>();

            if (!emails.Any())
                return ids;

            try
            {
                #region Set up connection to CRM sequrity
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(new Guid(user)));
                #endregion

                using (var db = new DbManager("crm"))
                {
                    #region If CRM contact

                    const string cc_alias = "cc";
                    const string cci_alias = "cci";

                    var q = new SqlQuery(CrmContactTable.Name.Alias(cc_alias))
                        .Select(CrmContactTable.Columns.Id.Prefix(cc_alias),
                                CrmContactTable.Columns.IsCompany.Prefix(cc_alias))
                        .InnerJoin(CrmContactInfoTable.Name.Alias(cci_alias),
                                   Exp.EqColumns(CrmContactTable.Columns.Tenant.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.Tenant.Prefix(cci_alias)) &
                                   Exp.EqColumns(CrmContactTable.Columns.Id.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.ContactId.Prefix(cci_alias)))
                        .Where(CrmContactTable.Columns.Tenant.Prefix(cc_alias), tenant)
                        .Where(CrmContactInfoTable.Columns.Type.Prefix(cci_alias), (int) CRM.Core.ContactInfoType.Email)
                        .Where(Exp.In(CrmContactInfoTable.Columns.Data.Prefix(cci_alias), emails));

                    var result = db.ExecuteList(q)
                        .ConvertAll(r => new { Id = Convert.ToInt32(r[0]), Company = Convert.ToBoolean(r[1]) });

                    foreach (var r in result)
                    {
                        var contact = r.Company ? new Company() : (Contact)new Person();
                        contact.ID = r.Id;
                        if (CRMSecurity.CanAccessTo(contact))
                        {
                            ids.Add(r.Id);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                _log.Warn("GetCrmContactsId(tenandId='{0}', userId='{1}', emails='{2}') Exception:\r\n{3}\r\n",
                   tenant, user, string.Join(",", emails), e.ToString());
            }
            return ids;
        }

        #endregion

        #region private methods

        private static List<CrmContactEntity> GetLinkedCrmEntitiesId(DbManager db, string chainId, int mailboxId, int tenant)
        {
            return db.ExecuteList(GetLinkedContactsQuery(chainId, mailboxId, tenant))
                     .ConvertAll(b => new CrmContactEntity
                     {
                         Id = Convert.ToInt32(b[0]),
                         Type = (CrmContactEntity.EntityTypes)b[1]
                     });
        }

        private static SqlQuery GetLinkedContactsQuery(string chainId, int mailboxId, int tenant)
        {
            return new SqlQuery(ChainXCrmContactEntity.Name)
                                .Select(ChainXCrmContactEntity.Columns.EntityId)
                                .Select(ChainXCrmContactEntity.Columns.EntityType)
                                .Where(ChainXCrmContactEntity.Columns.MailboxId, mailboxId)
                                .Where(ChainXCrmContactEntity.Columns.Tenant, tenant)
                                .Where(ChainXCrmContactEntity.Columns.ChainId, chainId);
        }

        private List<int> GetFilteredContactIds(DbManager db, int tenant, string user, MailContactsFilter filter)
        {
            SqlQuery queryContacts;
            var sortOrder = filter.SortOrder == "ascending";
            const string mail_contacts = "mc";
            const string contact_info = "ci";

            if (string.IsNullOrEmpty(filter.SearchFilter))
            {
                queryContacts = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                    .Select(ContactsTable.Columns.Id.Prefix(mail_contacts));
            }
            else
            {
                queryContacts = new SqlQuery(ContactsTable.Name.Alias(mail_contacts))
                    .Select(ContactsTable.Columns.Id.Prefix(mail_contacts))
                    .InnerJoin(ContactInfoTable.Name.Alias(contact_info),
                               Exp.EqColumns(ContactsTable.Columns.Id.Prefix(mail_contacts),
                                             ContactInfoTable.Columns.ContactId.Prefix(contact_info)))
                    .Distinct();

                if (FullTextSearch.SupportModule(FullTextSearch.MailContactsModule))
                {
                    var ids = FullTextSearch.Search(FullTextSearch.MailContactsModule.Match(filter.SearchFilter));
                    queryContacts = queryContacts.Where(Exp.In(ContactInfoTable.Columns.Id.Prefix(contact_info), ids));
                }
                else
                {
                    queryContacts = queryContacts.Where(
                        Exp.Or(
                            Exp.Like(mail_contacts + '.' + ContactsTable.Columns.Description, filter.SearchFilter, SqlLike.StartWith),
                            Exp.Or(
                                Exp.Like(ContactsTable.Columns.ContactName.Prefix(mail_contacts), filter.SearchFilter, SqlLike.StartWith),
                                Exp.Like(ContactInfoTable.Columns.Data.Prefix(contact_info), filter.SearchFilter, SqlLike.StartWith))));
                }
            }

            if (filter.Type.HasValue)
            {
                queryContacts = queryContacts.Where(ContactsTable.Columns.Type.Prefix(mail_contacts), filter.Type);
            }

            queryContacts = queryContacts.Where(ContactsTable.Columns.Tenant.Prefix(mail_contacts), tenant)
                                          .Where(ContactsTable.Columns.User.Prefix(mail_contacts), user)
                                          .OrderBy(ContactsTable.Columns.ContactName.Prefix(mail_contacts), sortOrder)
                                          .SetFirstResult(filter.StartIndex)
                                          .SetMaxResults(filter.Count);

            return db.ExecuteList(queryContacts).ConvertAll(r => Convert.ToInt32(r[0]));
        }

        private int SaveContactInfo(DbManager db, int tenant, string user, int contactId, string data, ContactInfoType infoType, bool isPrimary)
        {
            var query = new SqlInsert(ContactInfoTable.Name)
                            .InColumnValue(ContactInfoTable.Columns.Tenant, tenant)
                            .InColumnValue(ContactInfoTable.Columns.User, user)
                            .InColumnValue(ContactInfoTable.Columns.ContactId, contactId)
                            .InColumnValue(ContactInfoTable.Columns.Data, data)
                            .InColumnValue(ContactInfoTable.Columns.Type, (int)infoType)
                            .InColumnValue(ContactInfoTable.Columns.IsPrimary, isPrimary);
            return db.ExecuteScalar<int>(query);
        }

        #endregion
    }
}
