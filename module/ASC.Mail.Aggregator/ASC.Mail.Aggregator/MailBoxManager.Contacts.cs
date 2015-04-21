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
using ASC.Common.Data.Sql;
using ASC.FullTextIndex;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Data;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.FullTextIndex.Service;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ActiveUp.Net.Mail;
using ASC.Web.CRM.Core.Enums;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        // ToDo: replace with setting value
        public const int FULLTEXTSEARCH_IDS_COUNT = 200;

        #region crm defines

        private const int CRM_CONTACT_ENTITY_TYPE = 0;
        #endregion

        #region public methods

        public void SaveMailContacts(int tenant, string user, Message message)
        {
            try
            {
                var contacts = new AddressCollection();
                contacts.AddRange(message.To);
                contacts.AddRange(message.Cc);
                contacts.AddRange(message.Bcc);

                foreach (var contact in contacts)
                {
                    contact.Name = !String.IsNullOrEmpty(contact.Name) ? Codec.RFC2047Decode(contact.Name) : String.Empty;
                }

                var contactsList = contacts.Distinct().ToList();

                using (var db = GetDb())
                {
                    var validContacts = (from contact in contactsList
                                         where MailContactExists(db, tenant, user, contact.Name, contact.Email) < 1
                                         select contact).ToList();

                    if (!validContacts.Any()) return;

                    var lastModified = DateTime.UtcNow;

                    var insertQuery = new SqlInsert(ContactsTable.name)
                        .InColumns(ContactsTable.Columns.id_user,
                                   ContactsTable.Columns.id_tenant,
                                   ContactsTable.Columns.name,
                                   ContactsTable.Columns.address,
                                   ContactsTable.Columns.last_modified);

                    validContacts
                        .ForEach(contact =>
                                 insertQuery
                                     .Values(user, tenant, contact.Name, contact.Email, lastModified));

                    db.ExecuteNonQuery(insertQuery);
                }
            }
            catch (Exception e)
            {
                _log.Error("SaveMailContacts(tenant={0}, userId='{1}', mail_id={2}) Exception:\r\n{3}\r\n",
                          tenant, user, message.Id, e.ToString());
            }
        }

        public List<string> SearchMailContacts(int tenant, string user, string searchText)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(searchText))
            {
                var query =  new SqlQuery(ContactsTable.name)
                            .Select(ContactsTable.Columns.name, ContactsTable.Columns.address);
                if (FullTextSearch.SupportModule(FullTextSearch.MailContactsModule))
                {
                    var ids = FullTextSearch.Search(FullTextSearch.MailContactsModule.Match(searchText.TrimEnd('*') + "*"));

                    if (!ids.Any())
                        return contacts;

                    query.Where(Exp.In(ContactsTable.Columns.id, ids.Take(FULLTEXTSEARCH_IDS_COUNT).ToList()))
                         .Where(ContactsTable.Columns.id_user, user);
                }
                else
                {
                    searchText = searchText.Replace("\"", "\\\"");
                    query
                        .Where(ContactsTable.Columns.id_user, user)
                        .Where(
                            Exp.Like(
                                "concat(" + ContactsTable.Columns.name + ", ' ', " + ContactsTable.Columns.address + ")",
                                searchText));
                }

                using (var db = GetDb())
                {
                    var result = db.ExecuteList(query)
                        .ConvertAll(r => new
                        {
                            Name = Convert.ToString(r[0]),
                            Email = Convert.ToString(r[1])
                        });

                    foreach (var r in result)
                    {
                        var contact = "";
                        if (!string.IsNullOrEmpty(r.Name)) contact += "\"" + r.Name + "\" ";
                        contact += "<" + r.Email + ">";
                        contacts.Add(contact);
                    }
                }
            }

            return contacts;
        }

        public List<string> SearchCrmContacts(int tenant, string user, string searchText)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(searchText))
            {
                #region Set up connection to CRM sequrity
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(new Guid(user)));
                }
                catch (Exception e)
                {
                    _log.Error("Error connecting to CRM database", e);
                    return contacts;
                }
                #endregion

                //TODO: move to crm api

                const string cc_alias = "cc";
                const string cci_alias = "cci";

                using (var db = new DbManager("crm"))
                {
                    var query = new SqlQuery(CrmContactTable.name.Alias(cc_alias))
                        .Select(CrmContactTable.Columns.id.Prefix(cc_alias),
                                CrmContactTable.Columns.is_company.Prefix(cc_alias),
                                string.Format("trim(concat({0}, ' ', {1}, ' ', {2}))",
                                              CrmContactTable.Columns.first_name.Prefix(cc_alias),
                                              CrmContactTable.Columns.last_name.Prefix(cc_alias),
                                              CrmContactTable.Columns.company_name.Prefix(cc_alias)),
                                CrmContactTable.Columns.is_shared.Prefix(cc_alias),
                                CrmContactInfoTable.Columns.data.Prefix(cci_alias))
                        .InnerJoin(CrmContactInfoTable.name.Alias(cci_alias),
                                   Exp.EqColumns(CrmContactTable.Columns.tenant_id.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.tenant_id.Prefix(cci_alias)) &
                                   Exp.EqColumns(CrmContactTable.Columns.id.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.contact_id.Prefix(cci_alias)))
                        .Where(CrmContactInfoTable.Columns.type.Prefix(cci_alias), (int) ContactInfoType.Email);

                    if (FullTextSearch.SupportModule(FullTextSearch.CRMEmailsModule))
                    {
                        var ids = FullTextSearch.Search(FullTextSearch.CRMEmailsModule.Match(searchText.TrimEnd('*') + "*"));

                        if (!ids.Any())
                            return contacts;

                        query.Where(Exp.In(CrmContactInfoTable.Columns.id.Prefix(cci_alias),
                                           ids.Take(FULLTEXTSEARCH_IDS_COUNT).ToList()));

                    }
                    else
                    {
                        searchText = searchText.Replace("\"", "\\\"");
                        query
                            .Where(CrmContactTable.Columns.tenant_id.Prefix(cc_alias), tenant)
                            .Where(string.Format("(concat({0}, ' ', {1}) like '%{2}%')",
                                                 CrmContactTable.Columns.display_name.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.data.Prefix(cci_alias),
                                                 searchText));
                    }

                    var result = db.ExecuteList(query)
                                    .ConvertAll(r => new
                                    {
                                        Id = Convert.ToInt32(r[0]),
                                        Company = Convert.ToBoolean(r[1]),
                                        DisplayName = Convert.ToString(r[2]),
                                        IsShared = r[3],
                                        Email = Convert.ToString(r[4])
                                    });

                    foreach (var r in result)
                    {
                        Contact contact;
                        if(r.Company)
                        {
                            contact = new Company();
                        }
                        else
                        {
                            contact = new Person();
                        }
                        contact.ID = r.Id;
                        contact.ShareType = GetContactShareType(contact, r.IsShared);
                        if (CRMSecurity.CanAccessTo(contact))
                        {
                            contacts.Add("\"" + r.DisplayName + "\" <" + r.Email + ">");
                        }
                    }
                }
            }
            return contacts;
        }

        public List<string> SearchTeamLabContacts(int tenant, string searchText)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(searchText))
            {
                const string cu_alias = "cu";

                var query = new SqlQuery(CoreUserTable.name.Alias(cu_alias))
                    .Select(string.Format("concat('\\\"', {0}, ' ', {1}, '\\\" <', {2}, '>')",
                                          CoreUserTable.Columns.firstname.Prefix(cu_alias),
                                          CoreUserTable.Columns.lastname.Prefix(cu_alias),
                                          CoreUserTable.Columns.email.Prefix(cu_alias)))
                    .Where(CoreUserTable.Columns.removed, false);

                if (FullTextSearch.SupportModule(FullTextSearch.UserEmailsModule))
                {
                    var ids =
                        FullTextSearch.Search(FullTextSearch.UserEmailsModule.Match(searchText.TrimEnd('*') + "*"));

                    if (!ids.Any())
                        return contacts;

                    query.Where(Exp.In(CoreUserTable.Columns.id.Prefix(cu_alias), ids.Take(FULLTEXTSEARCH_IDS_COUNT).ToList()));
                }
                else
                {
                    searchText = searchText.Replace("\"", "\\\"");

                    query
                        .Where(CoreUserTable.Columns.tenant.Prefix(cu_alias), tenant)
                        .Where(
                            Exp.Like(
                                string.Format("concat({0}, ' ', {1}, ' ', {2})",
                                              CoreUserTable.Columns.firstname.Prefix(cu_alias),
                                              CoreUserTable.Columns.lastname.Prefix(cu_alias),
                                              CoreUserTable.Columns.email.Prefix(cu_alias)), searchText));

                }

                using (var db = new DbManager("core"))
                {
                    contacts = db.ExecuteList(query)
                                 .ConvertAll(r => r[0].ToString());
                }
            }

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

        private static List<CrmContactEntity> GetLinkedCrmEntitiesId(DbManager db, string chainId, int mailboxId, int tenant)
        {
            return db.ExecuteList(GetLinkedContactsQuery(chainId, mailboxId, tenant))
                     .ConvertAll(b => new CrmContactEntity
                         {
                             Id = Convert.ToInt32(b[0]),
                             Type = (ChainXCrmContactEntity.EntityTypes)b[1]
                         });
        }

        private static SqlQuery GetLinkedContactsQuery(string chainId, int mailboxId, int tenant)
        {
            return new SqlQuery(ChainXCrmContactEntity.name)
                                .Select(ChainXCrmContactEntity.Columns.entity_id)
                                .Select(ChainXCrmContactEntity.Columns.entity_type)
                                .Where(ChainXCrmContactEntity.Columns.id_mailbox, mailboxId)
                                .Where(ChainXCrmContactEntity.Columns.id_tenant, tenant)
                                .Where(ChainXCrmContactEntity.Columns.id_chain, chainId);
        }


        private int MailContactExists(IDbManager db, int tenant, string user, string name, string address)
        {
            return db.ExecuteScalar<int>(
                new SqlQuery(ContactsTable.name)
                    .Select(ContactsTable.Columns.id)
                    .Where(ContactsTable.Columns.id_user, user)
                    .Where(ContactsTable.Columns.name, name)
                    .Where(ContactsTable.Columns.address, address));
        }

        private ShareType GetContactShareType(Contact contact, object IsShared)
        {
            if (IsShared == null)
            {
                var accessSubjectToContact = CRMSecurity.GetAccessSubjectTo(contact);
                return !accessSubjectToContact.Any() ? ShareType.ReadWrite : ShareType.None;
            }
            else
            {
                return (ShareType)(Convert.ToInt32(IsShared));
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

                    var q = new SqlQuery(CrmContactTable.name.Alias(cc_alias))
                        .Select(CrmContactTable.Columns.id.Prefix(cc_alias),
                                CrmContactTable.Columns.is_company.Prefix(cc_alias))
                        .InnerJoin(CrmContactInfoTable.name.Alias(cci_alias),
                                   Exp.EqColumns(CrmContactTable.Columns.tenant_id.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.tenant_id.Prefix(cci_alias)) &
                                   Exp.EqColumns(CrmContactTable.Columns.id.Prefix(cc_alias),
                                                 CrmContactInfoTable.Columns.contact_id.Prefix(cci_alias)))
                        .Where(CrmContactTable.Columns.tenant_id.Prefix(cc_alias), tenant)
                        .Where(CrmContactInfoTable.Columns.type.Prefix(cci_alias), (int) ContactInfoType.Email)
                        .Where(Exp.In(CrmContactInfoTable.Columns.data.Prefix(cci_alias), emails));

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
    }
}
