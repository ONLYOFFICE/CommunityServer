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


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.DbSchema;

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

            var domainAddTime = DateTime.UtcNow;

            var addWebDomainQuery = new SqlInsert(DomainTable.Name)
                .InColumnValue(DomainTable.Columns.Id, 0)
                .InColumnValue(DomainTable.Columns.DomainName, name)
                .InColumnValue(DomainTable.Columns.Tenant, tenant)
                .InColumnValue(DomainTable.Columns.IsVerified, isVerified)
                .InColumnValue(DomainTable.Columns.DateAdded, domainAddTime)
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
            var groupQuery = new SqlQuery(MailGroupTable.Name.Alias(group_alias))
                                .InnerJoin(AddressTable.Name.Alias(address_alias),
                                           Exp.EqColumns(MailGroupTable.Columns.AddressId.Prefix(group_alias),
                                                         AddressTable.Columns.Id.Prefix(address_alias)
                                                        )
                                          )
                                .Select(MailGroupTable.Columns.Id.Prefix(group_alias))
                                .Where(AddressTable.Columns.Tenant.Prefix(address_alias), tenant)
                                .Where(AddressTable.Columns.DomainId.Prefix(address_alias), domainId)
                                .Where(AddressTable.Columns.IsMailGroup.Prefix(address_alias), true);

            var mailboxQuery = new SqlQuery(AddressTable.Name)
                                .Select(AddressTable.Columns.MailboxId)
                                .Where(AddressTable.Columns.Tenant, tenant)
                                .Where(AddressTable.Columns.DomainId, domainId)
                                .Where(AddressTable.Columns.IsMailGroup, false)
                                .Where(AddressTable.Columns.IsAlias, false);

            var deleteWebDomainQuery = new SqlDelete(DomainTable.Name)
                .Where(DomainTable.Columns.Tenant, tenant)
                .Where(DomainTable.Columns.Id, domainId);


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
                .Where(Exp.In(DomainTable.Columns.Tenant, new List<int> { tenant, Defines.SHARED_TENANT_ID }));

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
                .Where(DomainTable.Columns.DomainName, name);

            var result = NullSafeExecuteList(db, getDomainForNameQuery);

            return result.Select(r => r.ToWebDomainDto()).FirstOrDefault();
        }

        public WebDomainDto GetDomain(int domainId)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            var getDomainForIdQuery = GetDomainFieldsQuery()
                .Where(Exp.In(DomainTable.Columns.Tenant, new List<int> { tenant, Defines.SHARED_TENANT_ID }))
                .Where(DomainTable.Columns.Id, domainId);

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

            var checkDate = DateTime.UtcNow;

            using (var db = GetDb())
            {
                var updateDomain = new SqlUpdate(DomainTable.Name)
                    .Set(DomainTable.Columns.IsVerified, isVerified)
                    .Set(DomainTable.Columns.DateChecked, checkDate)
                    .Where(DomainTable.Columns.Id, domainId);

                db.ExecuteNonQuery(updateDomain);
            }
        }

        private SqlQuery GetDomainFieldsQuery()
        {
            return new SqlQuery(DomainTable.Name)
                .Select(DomainTable.Columns.Id)
                .Select(DomainTable.Columns.DomainName)
                .Select(DomainTable.Columns.Tenant)
                .Select(DomainTable.Columns.DateAdded)
                .Select(DomainTable.Columns.IsVerified);
        }
    }
}
