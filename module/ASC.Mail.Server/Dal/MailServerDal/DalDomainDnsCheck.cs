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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public class DalDomainDnsCheck
    {
        const string DOMAIN_ALIAS = "d";
        const string DNS_ALIAS = "dns";
        const string TENANT_X_SERVER_ALIAS = "txs";
        const string SERVER_ALIAS = "s";

        protected DbManager GetDb()
        {
            return new DbManager("mail");
        }

        public void SetDomainDisabled(int domainId, int disabledDays)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            if(disabledDays < 1)
                disabledDays = 1;

            var updateDomain = new SqlUpdate(DomainTable.name)
                .Set(string.Format("{0}=DATE_ADD(UTC_TIMESTAMP(), INTERVAL {1} DAY)", DomainTable.Columns.date_checked, disabledDays))
                .Where(DomainTable.Columns.id, domainId);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(updateDomain);
            }
        }

        public void SetDomainChecked(int domainId)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            var updateDomain = new SqlUpdate(DomainTable.name)
                .Set(string.Format("{0}=UTC_TIMESTAMP()", DomainTable.Columns.date_checked))
                .Where(DomainTable.Columns.id, domainId);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(updateDomain);
            }
        }

        public void SetDomainVerifiedAndChecked(int domainId, bool isVerified)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            var updateDomain = new SqlUpdate(DomainTable.name)
                .Set(string.Format("{0}=UTC_TIMESTAMP()", DomainTable.Columns.date_checked))
                .Set(DomainTable.Columns.is_verified, isVerified)
                .Where(DomainTable.Columns.id, domainId);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(updateDomain);
            }
        }

        public List<DnsCheckTaskDto> GetOldUnverifiedTasks(int getTaskOlderMinutes, int tasksLimit)
        {
            if (getTaskOlderMinutes < 0)
                throw new ArgumentException("Argument get_task_older_minutes less then zero.", "getTaskOlderMinutes");

            if (tasksLimit < 1)
                throw new ArgumentException("Argument tasks_limit less then one.", "tasksLimit");

            List<DnsCheckTaskDto> list;

            var query = GetDomainCheckTaskFieldsQuery(tasksLimit)
                .Where(DomainTable.Columns.is_verified.Prefix(DOMAIN_ALIAS), 0)
                .Where(string.Format("TIMESTAMPDIFF(MINUTE, {0}, UTC_TIMESTAMP()) > {1}",
                                     DomainTable.Columns.date_checked.Prefix(DOMAIN_ALIAS), getTaskOlderMinutes));

            using (var db = GetDb())
            {
                list = db.ExecuteList(query)
                         .ConvertAll(r => r.ToDnsCheckTaskDto());
            }

            return list;
        }

        public List<DnsCheckTaskDto> GetOldVerifiedTasks(int getTaskOlderMinutes, int tasksLimit)
        {
            if (getTaskOlderMinutes < 0)
                throw new ArgumentException("Argument get_task_older_minutes less then zero.", "getTaskOlderMinutes");

            if (tasksLimit < 1)
                throw new ArgumentException("Argument tasks_limit less then one.", "tasksLimit");

            List<DnsCheckTaskDto> list;

            var query = GetDomainCheckTaskFieldsQuery(tasksLimit)
                .Where(DomainTable.Columns.is_verified.Prefix(DOMAIN_ALIAS), 1)
                .Where(string.Format("TIMESTAMPDIFF(MINUTE, {0}, UTC_TIMESTAMP()) > {1}",
                                     DomainTable.Columns.date_checked.Prefix(DOMAIN_ALIAS), getTaskOlderMinutes));

            using (var db = GetDb())
            {
                list = db.ExecuteList(query)
                         .ConvertAll(r => r.ToDnsCheckTaskDto());
            }

            return list;
        }

        private SqlQuery GetDomainCheckTaskFieldsQuery(int limit)
        {
            return new SqlQuery(DomainTable.name.Alias(DOMAIN_ALIAS))
                .InnerJoin(DnsTable.name.Alias(DNS_ALIAS),
                           Exp.EqColumns(DomainTable.Columns.id.Prefix(DOMAIN_ALIAS),
                                         DnsTable.Columns.id_domain.Prefix(DNS_ALIAS)))
                .InnerJoin(TenantXServerTable.name.Alias(TENANT_X_SERVER_ALIAS),
                           Exp.EqColumns(DomainTable.Columns.tenant.Prefix(DOMAIN_ALIAS),
                                         TenantXServerTable.Columns.id_tenant.Prefix(TENANT_X_SERVER_ALIAS)))
                .InnerJoin(ServerTable.name.Alias(SERVER_ALIAS),
                           Exp.EqColumns(TenantXServerTable.Columns.id_server.Prefix(TENANT_X_SERVER_ALIAS),
                                         ServerTable.Columns.id.Prefix(SERVER_ALIAS)))
                .Select(DomainTable.Columns.id.Prefix(DOMAIN_ALIAS))
                .Select(DomainTable.Columns.name.Prefix(DOMAIN_ALIAS))
                .Select(DomainTable.Columns.is_verified.Prefix(DOMAIN_ALIAS))
                .Select(DomainTable.Columns.date_added.Prefix(DOMAIN_ALIAS))
                .Select(DomainTable.Columns.date_checked.Prefix(DOMAIN_ALIAS))
                .Select(DnsTable.Columns.tenant.Prefix(DNS_ALIAS))
                .Select(DnsTable.Columns.user.Prefix(DNS_ALIAS))
                .Select(DnsTable.Columns.dkim_selector.Prefix(DNS_ALIAS))
                .Select(DnsTable.Columns.dkim_public_key.Prefix(DNS_ALIAS))
                .Select(DnsTable.Columns.spf.Prefix(DNS_ALIAS))
                .Select(ServerTable.Columns.mx_record.Prefix(SERVER_ALIAS))
                .SetMaxResults(limit);

        }

    }
}
