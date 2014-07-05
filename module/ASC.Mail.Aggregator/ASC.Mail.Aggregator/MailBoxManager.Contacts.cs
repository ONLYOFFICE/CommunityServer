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
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.FullTextIndex;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Data;
using ASC.Core;
using System.Configuration;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Mail.Aggregator.Dal.DbSchema;

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

        public List<string> SearchMailContacts(int tenant_id, string user_id, string search_text)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(search_text))
            {
                SqlQuery q;
                if (FullTextSearch.SupportModule(FullTextSearch.UserEmailsModule))
                {
                    var ids = FullTextSearch.Search(search_text + "*", FullTextSearch.MailContactsModule)
                                                      .GetIdentifiers();

                    q = new SqlQuery(ContactsTable.name)
                            .Select(ContactsTable.Columns.name, ContactsTable.Columns.address)
                            .Where(Exp.In(ContactsTable.Columns.id, ids.Take(FULLTEXTSEARCH_IDS_COUNT).ToList()))
                            .Where(ContactsTable.Columns.id_user, user_id);
                }
                else
                {
                    search_text = search_text.Replace("\"", "\\\"");
                    q = new SqlQuery(ContactsTable.name)
                                .Select(ContactsTable.Columns.name, ContactsTable.Columns.address)
                                .Where(ContactsTable.Columns.id_user, user_id)
                                .Where(Exp.Like("concat(" + ContactsTable.Columns.name + ", ' ', " + ContactsTable.Columns.address + ")", search_text));
                }

                using (var db = GetDb())
                {
                    var result = db.ExecuteList(q)
                        .ConvertAll(r => new
                        {
                            Name = Convert.ToString(r[0]),
                            Email = Convert.ToString(r[1])
                        });

                    foreach (var r in result)
                    {
                        string contact = "";
                        if (!string.IsNullOrEmpty(r.Name)) contact += "\"" + r.Name + "\" ";
                        contact += "<" + r.Email + ">";
                        contacts.Add(contact);
                    }
                }
            }

            return contacts;
        }

        public List<string> SearchCRMContacts(int tenant_id, string user_id, string search_text)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(search_text))
            {
                #region Set up connection to CRM sequrity
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant_id);
                    SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(new Guid(user_id)));

                    if (!DbRegistry.IsDatabaseRegistered("crm"))
                    {
                        DbRegistry.RegisterDatabase("crm", ConfigurationManager.ConnectionStrings["crm"]);
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Error connecting to CRM database", e);
                    return contacts;
                }
                #endregion

                using (var db = new DbManager("crm"))
                {
                    SqlQuery q;
                    if (FullTextSearch.SupportModule(FullTextSearch.CRMEmailsModule))
                    {
                        var ids = FullTextSearch.Search(search_text + "*", FullTextSearch.CRMEmailsModule)
                                                          .GetIdentifiers()
                                                          .Select(id => int.Parse(id));

                        q = new SqlQuery("crm_contact c")
                             .Select("c.id", "c.is_company", "trim(concat(c.first_name, ' ', c.last_name, ' ', c.company_name))", "i.data")
                             .InnerJoin("crm_contact_info i", Exp.EqColumns("c.tenant_id", "i.tenant_id") & Exp.EqColumns("c.id", "i.contact_id"))
                             .Where(Exp.In("i.id", ids.Take(FULLTEXTSEARCH_IDS_COUNT).ToList()))
                             .Where("i.type", (int)ContactInfoType.Email);

                    }
                    else
                    {
                        search_text = search_text.Replace("\"", "\\\"");
                        q = new SqlQuery("crm_contact c")
                            .Select("c.id", "c.is_company", "trim(concat(c.first_name, ' ', c.last_name, ' ', c.company_name))", "i.data")
                            .InnerJoin("crm_contact_info i", Exp.EqColumns("c.tenant_id", "i.tenant_id") & Exp.EqColumns("c.id", "i.contact_id"))
                            .Where("c.tenant_id", tenant_id)
                            .Where("i.type", (int)ContactInfoType.Email)
                            .Where("(concat(c.display_name, ' ', i.data) like '%" + search_text + "%')");
                    }

                    var result = db.ExecuteList(q)
                                    .ConvertAll(r => new
                                    {
                                        Id = Convert.ToInt32(r[0]),
                                        Company = Convert.ToBoolean(r[1]),
                                        DisplayName = Convert.ToString(r[2]),
                                        Email = Convert.ToString(r[3])
                                    });

                    foreach (var r in result)
                    {
                        var contact = r.Company ? (Contact)new Company() : new Person();
                        contact.ID = r.Id;
                        if (CRMSecurity.CanAccessTo(contact))
                        {
                            contacts.Add("\"" + r.DisplayName + "\" <" + r.Email + ">");
                        }
                    }
                }
            }
            return contacts;
        }

        public List<string> SearchTeamLabContacts(int tenant_id, string search_text)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(search_text))
            {
                if (FullTextSearch.SupportModule(FullTextSearch.UserEmailsModule))
                {
                    var ids = FullTextSearch.Search(search_text + "*", FullTextSearch.UserEmailsModule)
                                                      .GetIdentifiers();

                    using (var db = new DbManager("core"))
                    {
                        var q = new SqlQuery("core_user c")
                            .Select("concat('\\\"', c.firstname, ' ', c.lastname, '\\\" <', c.email, '>')")
                            .Where(Exp.In("id", ids.Take(FULLTEXTSEARCH_IDS_COUNT).ToList()));

                        var result = db.ExecuteList(q)
                            .ConvertAll(r => r[0].ToString());

                        contacts = result;
                    }
                }
                else
                {
                    using (var db = new DbManager("core"))
                    {
                        search_text = search_text.Replace("\"", "\\\"");

                        var q = new SqlQuery("core_user c")
                            .Select("concat('\\\"', c.firstname, ' ', c.lastname, '\\\" <', c.email, '>')")
                            .Where("c.tenant", tenant_id)
                            .Where(Exp.Like("concat(c.firstname, ' ', c.lastname, ' ', c.email)", search_text));

                        var result = db.ExecuteList(q)
                            .ConvertAll(r => r[0].ToString());

                        contacts = result;
                    }
                }
            }

            return contacts;
        }


        public List<CrmContactEntity> GetLinkedCrmEntitiesId(int id_message, int id_tenant, string id_user)
        {
            using (var db = GetDb())
            {
                var chain_info = GetMessageChainInfo(db, id_tenant, id_user, id_message);
                return GetLinkedCrmEntitiesId(db, chain_info.id, chain_info.mailbox, id_tenant);
            }
        }


        public List<CrmContactEntity> GetLinkedCrmEntitiesId(string id_chain, int id_mailbox, int id_tenant)
        {
            using (var db = GetDb())
            {
                return GetLinkedCrmEntitiesId(db, id_chain, id_mailbox, id_tenant);
            }
        }

        private static List<CrmContactEntity> GetLinkedCrmEntitiesId(DbManager db, string id_chain, int id_mailbox, int id_tenant)
        {
            return db.ExecuteList(GetLinkedContactsQuery(id_chain, id_mailbox, id_tenant))
                     .ConvertAll(b => new CrmContactEntity
                         {
                             Id = Convert.ToInt32(b[0]),
                             Type = (ChainXCrmContactEntity.EntityTypes) b[1]
                         });
        }

        private static SqlQuery GetLinkedContactsQuery(string id_chain, int id_mailbox, int id_tenant)
        {
            return new SqlQuery(ChainXCrmContactEntity.name)
                                .Select(ChainXCrmContactEntity.Columns.entity_id)
                                .Select(ChainXCrmContactEntity.Columns.entity_type)
                                .Where(ChainXCrmContactEntity.Columns.id_mailbox, id_mailbox)
                                .Where(ChainXCrmContactEntity.Columns.id_tenant, id_tenant)
                                .Where(ChainXCrmContactEntity.Columns.id_chain, id_chain);
        }


        public List<int> GetCrmContactsId(int tenant_id, string user_id, string email)
        {
            return GetCrmContactsId(tenant_id, user_id, new string[] {email});
        }

        public List<int> GetCrmContactsId(int tenant_id, string user_id, string[] emails)
        {
            var ids = new List<int>();
            try
            {
                #region Set up connection to CRM sequrity
                CoreContext.TenantManager.SetCurrentTenant(tenant_id);
                SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(new Guid(user_id)));
                if (!DbRegistry.IsDatabaseRegistered("crm"))
                {
                    DbRegistry.RegisterDatabase("crm", ConfigurationManager.ConnectionStrings["crm"]);
                }
                #endregion

                using (var db = new DbManager("crm"))
                {
                    #region If CRM contact
                    var q = new SqlQuery("crm_contact c")
                        .Select("c.id", "c.is_company")
                        .InnerJoin("crm_contact_info i", Exp.EqColumns("c.tenant_id", "i.tenant_id") & Exp.EqColumns("c.id", "i.contact_id"))
                        .Where("c.tenant_id", tenant_id)
                        .Where("i.type", (int)ContactInfoType.Email)
                        .Where(Exp.In("i.data", emails));

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
                   tenant_id, user_id, string.Join(",", emails), e.ToString());
            }
            return ids;
        }

        #endregion
    }
}
