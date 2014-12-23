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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Dal
{
    public class WebDomainDal : DalBase
    {
        public WebDomainDal(int tenant_id)
            : base("mailserver", tenant_id)
        {
        }

        public WebDomainDal(string db_connection_string_name, int tenant_id) : base(db_connection_string_name, tenant_id)
        {
        }

        public WebDomainDto AddWebDomain(string name, bool is_verified, DbManager db)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (db == null)
                throw new ArgumentNullException("db");

            var domain = GetDomain(name, db);
            if (domain != null)
                throw new DuplicateNameException(String.Format("Domain with name {0}. Already added to tenant {1}.",
                                                               domain.name, domain.tenant));

            var domain_add_time = DateTime.UtcNow.ToDbStyle();

            var add_web_domain_query = new SqlInsert(DomainTable.name)
                .InColumnValue(DomainTable.Columns.id, 0)
                .InColumnValue(DomainTable.Columns.name, name)
                .InColumnValue(DomainTable.Columns.tenant, tenant_id)
                .InColumnValue(DomainTable.Columns.is_verified, is_verified)
                .InColumnValue(DomainTable.Columns.date_added, domain_add_time)
                .Identity(0, 0, true);

            var added_domain_id = db.ExecuteScalar<int>(add_web_domain_query);
            return new WebDomainDto(added_domain_id, name, tenant_id, is_verified);
        }

        public void DeleteDomain(int domain_id, DbManager db)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");
            
            if(db == null)
                throw new ArgumentNullException("db");

            const string group_alias = "msg";
            const string address_alias = "msa";
            var group_query = new SqlQuery(MailGroupTable.name + " " + group_alias)
                                .InnerJoin(AddressTable.name + " " + address_alias,
                                           Exp.EqColumns(MailGroupTable.Columns.id_address.Prefix(group_alias),
                                                         AddressTable.Columns.id.Prefix(address_alias)
                                                        )
                                          )
                                .Select(MailGroupTable.Columns.id.Prefix(group_alias))
                                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant_id)
                                .Where(AddressTable.Columns.id_domain.Prefix(address_alias), domain_id)
                                .Where(AddressTable.Columns.is_mail_group.Prefix(address_alias), true);

            var mailbox_query = new SqlQuery(AddressTable.name)
                                .Select(AddressTable.Columns.id_mailbox)
                                .Where(AddressTable.Columns.tenant, tenant_id)
                                .Where(AddressTable.Columns.id_domain, domain_id)
                                .Where(AddressTable.Columns.is_mail_group, false)
                                .Where(AddressTable.Columns.is_alias, false);

            var delete_web_domain_query = new SqlDelete(DomainTable.name)
                .Where(DomainTable.Columns.tenant, tenant_id)
                .Where(DomainTable.Columns.id, domain_id);


            var result = db.ExecuteList(group_query);
            var group_ids = result.Select(r => (int)r[0]).ToList();

            var group_dal = new MailGroupDal(tenant_id);

            foreach (var group_id in group_ids)
            {
                group_dal.DeleteMailGroup(group_id, db);
            }

            result = db.ExecuteList(mailbox_query);
            var mailbox_ids = result.Select(r => (int)r[0]).ToList();

            var mailbox_dal = new MailboxDal(tenant_id);

            foreach (var mailbox_id in mailbox_ids)
            {
                mailbox_dal.DeleteMailbox(mailbox_id, db);
            }

            db.ExecuteNonQuery(delete_web_domain_query);
        }

        public List<WebDomainDto> GetTenantDomains()
        {
            var get_all_tenant_web_domains_query = GetDomainFieldsQuery()
                .Where(DomainTable.Columns.tenant, tenant_id);

            using (var db = GetDb())
            {
                var result = db.ExecuteList(get_all_tenant_web_domains_query);
                return result.Select(r => r.ToWebDomainDto()).ToList();
            }

        }

        public WebDomainDto GetDomain(string name, DbManager db = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var get_domain_for_name_query = GetDomainFieldsQuery()
                .Where(DomainTable.Columns.name, name);

            var result = NullSafeExecuteList(db, get_domain_for_name_query);

            return result.Select(r => r.ToWebDomainDto()).FirstOrDefault();
        }

        public WebDomainDto GetDomain(int domain_id)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            var get_domain_for_id_query = GetDomainFieldsQuery()
                .Where(DomainTable.Columns.tenant, tenant_id)
                .Where(DomainTable.Columns.id, domain_id);

            using (var db = GetDb())
            {
                var result = db.ExecuteList(get_domain_for_id_query);
                return result.Select(r => r.ToWebDomainDto()).FirstOrDefault();
            }
        }

        public void SetDomainVerified(int domain_id, bool is_verified)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");
            
            using (var db = GetDb())
            {
                var update_domain = new SqlUpdate(DomainTable.name)
                    .Set(DomainTable.Columns.is_verified, is_verified)
                    .Where(DomainTable.Columns.id, domain_id);

                db.ExecuteNonQuery(update_domain);
            }
        }

        private SqlQuery GetDomainFieldsQuery()
        {
            return new SqlQuery(DomainTable.name)
                .Select(DomainTable.Columns.id)
                .Select(DomainTable.Columns.name)
                .Select(DomainTable.Columns.tenant)
                .Select(DomainTable.Columns.date_added)
                .Select(DomainTable.Columns.is_verified);
        }
    }
}
