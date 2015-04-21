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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public class WebDomainDal : DalBase
    {
        public WebDomainDal(int tenant)
            : base("mailserver", tenant)
        {
        }

        public WebDomainDal(string dbConnectionStringName, int tenant) : base(dbConnectionStringName, tenant)
        {
        }

        public WebDomainDto AddWebDomain(string name, bool isVerified, DbManager db)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (db == null)
                throw new ArgumentNullException("db");

            var domain = GetDomain(name, db);
            if (domain != null)
                throw new DuplicateNameException(String.Format("Domain with name {0}. Already added to tenant {1}.",
                                                               domain.name, domain.tenant));

            var domainAddTime = DateTime.UtcNow.ToDbStyle();

            var addWebDomainQuery = new SqlInsert(DomainTable.name)
                .InColumnValue(DomainTable.Columns.id, 0)
                .InColumnValue(DomainTable.Columns.name, name)
                .InColumnValue(DomainTable.Columns.tenant, tenant)
                .InColumnValue(DomainTable.Columns.is_verified, isVerified)
                .InColumnValue(DomainTable.Columns.date_added, domainAddTime)
                .Identity(0, 0, true);

            var addedDomainId = db.ExecuteScalar<int>(addWebDomainQuery);
            return new WebDomainDto(addedDomainId, name, tenant, isVerified);
        }

        public void DeleteDomain(int domainId, DbManager db)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");
            
            if(db == null)
                throw new ArgumentNullException("db");

            const string group_alias = "msg";
            const string address_alias = "msa";
            var groupQuery = new SqlQuery(MailGroupTable.name + " " + group_alias)
                                .InnerJoin(AddressTable.name + " " + address_alias,
                                           Exp.EqColumns(MailGroupTable.Columns.id_address.Prefix(group_alias),
                                                         AddressTable.Columns.id.Prefix(address_alias)
                                                        )
                                          )
                                .Select(MailGroupTable.Columns.id.Prefix(group_alias))
                                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant)
                                .Where(AddressTable.Columns.id_domain.Prefix(address_alias), domainId)
                                .Where(AddressTable.Columns.is_mail_group.Prefix(address_alias), true);

            var mailboxQuery = new SqlQuery(AddressTable.name)
                                .Select(AddressTable.Columns.id_mailbox)
                                .Where(AddressTable.Columns.tenant, tenant)
                                .Where(AddressTable.Columns.id_domain, domainId)
                                .Where(AddressTable.Columns.is_mail_group, false)
                                .Where(AddressTable.Columns.is_alias, false);

            var deleteWebDomainQuery = new SqlDelete(DomainTable.name)
                .Where(DomainTable.Columns.tenant, tenant)
                .Where(DomainTable.Columns.id, domainId);


            var result = db.ExecuteList(groupQuery);
            var groupIds = result.Select(r => (int)r[0]).ToList();

            var groupDal = new MailGroupDal(tenant);

            foreach (var groupId in groupIds)
            {
                groupDal.DeleteMailGroup(groupId, db);
            }

            result = db.ExecuteList(mailboxQuery);
            var mailboxIds = result.Select(r => (int)r[0]).ToList();

            var mailboxDal = new MailboxDal(tenant);

            foreach (var mailboxId in mailboxIds)
            {
                mailboxDal.DeleteMailbox(mailboxId, db);
            }

            db.ExecuteNonQuery(deleteWebDomainQuery);
        }

        public List<WebDomainDto> GetTenantDomains()
        {
            var getAllTenantWebDomainsQuery = GetDomainFieldsQuery()
                .Where(Exp.In(DomainTable.Columns.tenant, new List<int> { tenant, Defines.SHARED_TENANT_ID }));

            using (var db = GetDb())
            {
                var result = db.ExecuteList(getAllTenantWebDomainsQuery);
                return result.Select(r => r.ToWebDomainDto()).ToList();
            }

        }

        public WebDomainDto GetDomain(string name, DbManager db = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var getDomainForNameQuery = GetDomainFieldsQuery()
                .Where(DomainTable.Columns.name, name);

            var result = NullSafeExecuteList(db, getDomainForNameQuery);

            return result.Select(r => r.ToWebDomainDto()).FirstOrDefault();
        }

        public WebDomainDto GetDomain(int domainId)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            var getDomainForIdQuery = GetDomainFieldsQuery()
                .Where(Exp.In(DomainTable.Columns.tenant, new List<int> { tenant, Defines.SHARED_TENANT_ID }))
                .Where(DomainTable.Columns.id, domainId);

            using (var db = GetDb())
            {
                var result = db.ExecuteList(getDomainForIdQuery);
                return result.Select(r => r.ToWebDomainDto()).FirstOrDefault();
            }
        }

        public void SetDomainVerified(int domainId, bool isVerified)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");
            
            using (var db = GetDb())
            {
                var updateDomain = new SqlUpdate(DomainTable.name)
                    .Set(DomainTable.Columns.is_verified, isVerified)
                    .Where(DomainTable.Columns.id, domainId);

                db.ExecuteNonQuery(updateDomain);
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
