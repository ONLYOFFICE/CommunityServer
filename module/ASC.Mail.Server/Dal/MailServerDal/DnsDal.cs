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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public class DnsDal : DalBase
    {
        private readonly int _tenant;
        private readonly string _user;
        private const int DefaultDomainId = -1;

        public DnsDal(int tenant_id, string user_id)
            : this("mailserver", tenant_id, user_id)
        {
        }

        public DnsDal(string db_connection_string_name, int tenant_id, string user_id)
            : base(db_connection_string_name, tenant_id)
        {
            _user = user_id;
            _tenant = tenant_id;
        }

        public DnsDto CreateFreeDnsRecords(string dkim_selector, string domain_checker_prefix, 
            string spf, DbManager db = null)
        {
            if (string.IsNullOrEmpty(dkim_selector))
                throw new ArgumentNullException("dkim_selector");

            if (string.IsNullOrEmpty(domain_checker_prefix))
                throw new ArgumentNullException("domain_checker_prefix");

            if (string.IsNullOrEmpty(spf))
                throw new ArgumentNullException("spf");

            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);

            var rand = new Random();
            var domain_check_value = Membership.GeneratePassword(16, 0);
            domain_check_value = Regex.Replace(domain_check_value, @"[^a-zA-Z0-9]", m => rand.Next(10).ToString());
            var domain_check = domain_checker_prefix + ": " + domain_check_value;


            var insert_values_query = new SqlInsert(DnsTable.name)
                .InColumnValue(AddressTable.Columns.id, 0)
                .InColumnValue(DnsTable.Columns.user, _user)
                .InColumnValue(DnsTable.Columns.tenant, _tenant)
                .InColumnValue(DnsTable.Columns.dkim_selector, dkim_selector)
                .InColumnValue(DnsTable.Columns.dkim_private_key, private_key)
                .InColumnValue(DnsTable.Columns.dkim_public_key, public_key)
                .InColumnValue(DnsTable.Columns.domain_check, domain_check)
                .InColumnValue(DnsTable.Columns.spf, spf)
                .Identity(0, 0, true);

            var dns_record_id = NullSafeExecuteScalar<int>(db, insert_values_query);
            return new DnsDto(dns_record_id, _tenant, _user, DefaultDomainId, dkim_selector, 
                private_key, public_key, domain_check, spf);
        }

        public DnsDto GetFreeDnsRecords(DbManager db = null)
        {
            return GetDomainDnsRecordsForCurrentUser(db);
        }

        public DnsDto LinkDnsToDomain(int dns_id, int domain_id, DbManager db)
        {
            if (dns_id < 0)
                throw new ArgumentException("Argument dns_id less then zero.", "dns_id");

            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            if (db == null)
                throw new ArgumentNullException("db");

            var dns_dto = GetDns(dns_id, db);

            if (dns_dto == null)
                throw new InvalidOperationException(String.Format("Record with dns id: {0} not found in db.", dns_id));

            if (dns_dto.id_domain != domain_id)
            {
                var update_query = new SqlUpdate(DnsTable.name)
                    .Set(DnsTable.Columns.id_domain, domain_id)
                    .Where(DnsTable.Columns.id, dns_id);

                var rows_affected = db.ExecuteNonQuery(update_query);
                if (rows_affected == 0)
                    throw new InvalidOperationException(String.Format("Record with dns id: {0} not found in db.", dns_id));

                dns_dto.id_domain = domain_id;
                return dns_dto;
            }

            return dns_dto;
        }

        public DnsDto GetDomainDnsRecords(int domain_id, DbManager db = null)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            var get_dns_query = GetDnsQuery()
                .Where(DnsTable.Columns.tenant, _tenant)
                .Where(DnsTable.Columns.id_domain, domain_id);

            return NullSafeExecuteList(db, get_dns_query).Select(r => r.ToDnsDto(_tenant, _user)).FirstOrDefault();
        }

        public DnsDto GetDomainDnsRecordsForCurrentUser(DbManager db = null)
        {
            var get_dns_query = GetDnsQuery()
                .Where(DnsTable.Columns.tenant, _tenant)
                .Where(DnsTable.Columns.user, _user)
                .Where(DnsTable.Columns.id_domain, DefaultDomainId);

            return NullSafeExecuteList(db, get_dns_query).Select(r => r.ToDnsDto(_tenant, _user)).FirstOrDefault();
        }

        public DnsDto GetDns(int dns_id, DbManager db = null)
        {
            if (dns_id < 0)
                throw new ArgumentException("Argument dns_id less then zero.", "dns_id");

            var get_dns_query = GetDnsQuery()
                .Where(DnsTable.Columns.id, dns_id)
                .Where(DnsTable.Columns.tenant, _tenant)
                .Where(DnsTable.Columns.user, _user);

            return NullSafeExecuteList(db, get_dns_query).Select(r => r.ToDnsDto(_tenant, _user)).FirstOrDefault();
        }

        public void RemoveUsedDns(int domain_id, DbManager db = null)
        {
            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            var remove_dns_query = new SqlDelete(DnsTable.name)
                .Where(DnsTable.Columns.id_domain, domain_id);

            NullSafeExecuteNonQuery(db, remove_dns_query);
        }

        private SqlQuery GetDnsQuery()
        {
            return new SqlQuery(DnsTable.name)
                .Select(DnsTable.Columns.id)
                .Select(DnsTable.Columns.id_domain)
                .Select(DnsTable.Columns.dkim_selector)
                .Select(DnsTable.Columns.dkim_private_key)
                .Select(DnsTable.Columns.dkim_public_key)
                .Select(DnsTable.Columns.domain_check)
                .Select(DnsTable.Columns.spf);
        }
    }
}
