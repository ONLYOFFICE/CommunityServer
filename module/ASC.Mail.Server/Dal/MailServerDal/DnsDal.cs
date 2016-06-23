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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Mail.Server.Utils;
using ASC.Common.Data.Sql.Expressions;
using System.Collections.Generic;

namespace ASC.Mail.Server.Dal
{
    public class DnsDal : DalBase
    {
        private readonly int _tenant;
        private readonly string _user;

        public DnsDal(int tenant, string user)
            : this("mailserver", tenant, user)
        {
        }

        public DnsDal(string dbConnectionStringName, int tenant, string user)
            : base(dbConnectionStringName, tenant)
        {
            _user = user;
            _tenant = tenant;
        }

        public DnsDto CreateFreeDnsRecords(string dkimSelector, string domainCheckerPrefix, 
            string spf, DbManager db = null)
        {
            if (string.IsNullOrEmpty(dkimSelector))
                throw new ArgumentNullException("dkimSelector");

            if (string.IsNullOrEmpty(domainCheckerPrefix))
                throw new ArgumentNullException("domainCheckerPrefix");

            if (string.IsNullOrEmpty(spf))
                throw new ArgumentNullException("spf");

            string privateKey, publicKey;
            DnsChecker.DnsChecker.GenerateKeys(out privateKey, out publicKey);

            var domainCheckValue = PasswordGenerator.GenerateNewPassword(16);
            var domainCheck = domainCheckerPrefix + ": " + domainCheckValue;

            var insertValuesQuery = new SqlInsert(DnsTable.Name)
                .InColumnValue(DnsTable.Columns.Id, 0)
                .InColumnValue(DnsTable.Columns.User, _user)
                .InColumnValue(DnsTable.Columns.Tenant, _tenant)
                .InColumnValue(DnsTable.Columns.DkimSelector, dkimSelector)
                .InColumnValue(DnsTable.Columns.DkimPrivateKey, privateKey)
                .InColumnValue(DnsTable.Columns.DkimPublicKey, publicKey)
                .InColumnValue(DnsTable.Columns.DomainCheck, domainCheck)
                .InColumnValue(DnsTable.Columns.Spf, spf)
                .Identity(0, 0, true);

            var dnsRecordId = NullSafeExecuteScalar<int>(db, insertValuesQuery);
            return new DnsDto(dnsRecordId, _tenant, _user, Defines.UNUSED_DNS_SETTING_DOMAIN_ID, dkimSelector, 
                privateKey, publicKey, domainCheck, spf);
        }

        public DnsDto GetFreeDnsRecords(DbManager db = null)
        {
            return GetDomainDnsRecordsForCurrentUser(db);
        }

        public DnsDto LinkDnsToDomain(int dnsId, int domainId, DbManager db)
        {
            if (dnsId < 0)
                throw new ArgumentException("Argument dns_id less then zero.", "dnsId");

            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            if (db == null)
                throw new ArgumentNullException("db");

            var dnsDto = GetDns(dnsId, db);

            if (dnsDto == null)
                throw new InvalidOperationException(String.Format("Record with dns id: {0} not found in db.", dnsId));

            if (dnsDto.id_domain != domainId)
            {
                var updateQuery = new SqlUpdate(DnsTable.Name)
                    .Set(DnsTable.Columns.DomainId, domainId)
                    .Where(DnsTable.Columns.Id, dnsId);

                var rowsAffected = db.ExecuteNonQuery(updateQuery);
                if (rowsAffected == 0)
                    throw new InvalidOperationException(String.Format("Record with dns id: {0} not found in db.", dnsId));

                dnsDto.id_domain = domainId;
                return dnsDto;
            }

            return dnsDto;
        }

        public DnsDto GetDomainDnsRecords(int domainId, DbManager db = null)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            var getDnsQuery = GetDnsQuery()
                .Where(Exp.In(DnsTable.Columns.Tenant, new List<int> { _tenant, Defines.SHARED_TENANT_ID }))
                .Where(DnsTable.Columns.DomainId, domainId);

            return NullSafeExecuteList(db, getDnsQuery).Select(r => r.ToDnsDto(_tenant, _user)).FirstOrDefault();
        }

        public DnsDto GetDomainDnsRecordsForCurrentUser(DbManager db = null)
        {
            var getDnsQuery = GetDnsQuery()
                .Where(DnsTable.Columns.Tenant, _tenant)
                .Where(DnsTable.Columns.User, _user)
                .Where(DnsTable.Columns.DomainId, Defines.UNUSED_DNS_SETTING_DOMAIN_ID);

            return NullSafeExecuteList(db, getDnsQuery).Select(r => r.ToDnsDto(_tenant, _user)).FirstOrDefault();
        }

        public DnsDto GetDns(int dnsId, DbManager db = null)
        {
            if (dnsId < 0)
                throw new ArgumentException("Argument dns_id less then zero.", "dnsId");

            var getDnsQuery = GetDnsQuery()
                .Where(DnsTable.Columns.Id, dnsId)
                .Where(DnsTable.Columns.Tenant, _tenant)
                .Where(DnsTable.Columns.User, _user);

            return NullSafeExecuteList(db, getDnsQuery).Select(r => r.ToDnsDto(_tenant, _user)).FirstOrDefault();
        }

        public void RemoveUsedDns(int domainId, DbManager db = null)
        {
            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            var removeDnsQuery = new SqlDelete(DnsTable.Name)
                .Where(DnsTable.Columns.DomainId, domainId);

            NullSafeExecuteNonQuery(db, removeDnsQuery);
        }

        private static SqlQuery GetDnsQuery()
        {
            return new SqlQuery(DnsTable.Name)
                .Select(DnsTable.Columns.Id)
                .Select(DnsTable.Columns.DomainId)
                .Select(DnsTable.Columns.DkimSelector)
                .Select(DnsTable.Columns.DkimPrivateKey)
                .Select(DnsTable.Columns.DkimPublicKey)
                .Select(DnsTable.Columns.DomainCheck)
                .Select(DnsTable.Columns.Spf);
        }
    }
}
