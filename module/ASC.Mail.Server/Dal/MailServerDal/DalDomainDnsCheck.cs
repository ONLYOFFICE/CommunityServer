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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Dal
{
    public class DalDomainDnsCheck
    {
        const string DomainAlias = "d";
        const string DnsAlias = "dns";
        const string TenantXServerAlias = "txs";
        const string ServerAlias = "s";

        protected DbManager GetDb()
        {
            return new DbManager("mail");
        }

        public void SetDomainDisabled(int domain_id, int disabled_days)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            if(disabled_days < 1)
                disabled_days = 1;

            var update_domain = new SqlUpdate(DomainTable.name)
                .Set(string.Format("{0}=DATE_ADD(NOW(), INTERVAL {1} DAY)", DomainTable.Columns.date_checked, disabled_days))
                .Where(DomainTable.Columns.id, domain_id);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(update_domain);
            }
        }

        public void SetDomainChecked(int domain_id)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            var update_domain = new SqlUpdate(DomainTable.name)
                .Set(string.Format("{0}=NOW()", DomainTable.Columns.date_checked))
                .Where(DomainTable.Columns.id, domain_id);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(update_domain);
            }
        }

        public void SetDomainVerifiedAndChecked(int domain_id, bool is_verified)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            var update_domain = new SqlUpdate(DomainTable.name)
                .Set(string.Format("{0}=NOW()", DomainTable.Columns.date_checked))
                .Set(DomainTable.Columns.is_verified, is_verified)
                .Where(DomainTable.Columns.id, domain_id);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(update_domain);
            }
        }

        public List<DnsCheckTaskDto> GetOldUnverifiedTasks(int get_task_older_minutes, int tasks_limit)
        {
            if (get_task_older_minutes < 0)
                throw new ArgumentException("Argument get_task_older_minutes less then zero.", "get_task_older_minutes");

            if (tasks_limit < 1)
                throw new ArgumentException("Argument tasks_limit less then one.", "tasks_limit");

            List<DnsCheckTaskDto> list;

            var query = GetDomainCheckTaskFieldsQuery(tasks_limit)
                .Where(DomainTable.Columns.is_verified.Prefix(DomainAlias), 0)
                .Where(string.Format("TIMESTAMPDIFF(MINUTE, {0}, NOW()) > {1}",
                                     DomainTable.Columns.date_checked.Prefix(DomainAlias), get_task_older_minutes));

            using (var db = GetDb())
            {
                list = db.ExecuteList(query)
                         .ConvertAll(r => r.ToDnsCheckTaskDto());
            }

            return list;
        }

        public List<DnsCheckTaskDto> GetOldVerifiedTasks(int get_task_older_minutes, int tasks_limit)
        {
            if (get_task_older_minutes < 0)
                throw new ArgumentException("Argument get_task_older_minutes less then zero.", "get_task_older_minutes");

            if (tasks_limit < 1)
                throw new ArgumentException("Argument tasks_limit less then one.", "tasks_limit");

            List<DnsCheckTaskDto> list;

            var query = GetDomainCheckTaskFieldsQuery(tasks_limit)
                .Where(DomainTable.Columns.is_verified.Prefix(DomainAlias), 1)
                .Where(string.Format("TIMESTAMPDIFF(MINUTE, {0}, NOW()) > {1}",
                                     DomainTable.Columns.date_checked.Prefix(DomainAlias), get_task_older_minutes));

            using (var db = GetDb())
            {
                list = db.ExecuteList(query)
                         .ConvertAll(r => r.ToDnsCheckTaskDto());
            }

            return list;
        }

        private SqlQuery GetDomainCheckTaskFieldsQuery(int limit)
        {
            return new SqlQuery(DomainTable.name.Alias(DomainAlias))
                .InnerJoin(DnsTable.name.Alias(DnsAlias),
                           Exp.EqColumns(DomainTable.Columns.id.Prefix(DomainAlias),
                                         DnsTable.Columns.id_domain.Prefix(DnsAlias)))
                .InnerJoin(TenantXServerTable.name.Alias(TenantXServerAlias),
                           Exp.EqColumns(DomainTable.Columns.tenant.Prefix(DomainAlias),
                                         TenantXServerTable.Columns.id_tenant.Prefix(TenantXServerAlias)))
                .InnerJoin(ServerTable.name.Alias(ServerAlias),
                           Exp.EqColumns(TenantXServerTable.Columns.id_server.Prefix(TenantXServerAlias),
                                         ServerTable.Columns.id.Prefix(ServerAlias)))
                .Select(DomainTable.Columns.id.Prefix(DomainAlias))
                .Select(DomainTable.Columns.name.Prefix(DomainAlias))
                .Select(DomainTable.Columns.is_verified.Prefix(DomainAlias))
                .Select(DomainTable.Columns.date_added.Prefix(DomainAlias))
                .Select(DomainTable.Columns.date_checked.Prefix(DomainAlias))
                .Select(DnsTable.Columns.tenant.Prefix(DnsAlias))
                .Select(DnsTable.Columns.user.Prefix(DnsAlias))
                .Select(DnsTable.Columns.dkim_selector.Prefix(DnsAlias))
                .Select(DnsTable.Columns.dkim_public_key.Prefix(DnsAlias))
                .Select(DnsTable.Columns.spf.Prefix(DnsAlias))
                .Select(ServerTable.Columns.mx_record.Prefix(ServerAlias))
                .SetMaxResults(limit);

        }

    }
}
