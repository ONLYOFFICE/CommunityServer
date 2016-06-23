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
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public class DkimDal : DalBase
    {
        private readonly int _tenant;
        private readonly string _user;
        private const int DefaultDomainId = -1;

        public DkimDal(int tenant_id, string user_id)
            : this("mailserver", tenant_id, user_id)
        {
        }

        public DkimDal(string db_connection_string_name, int tenant_id, string user_id)
            : base(db_connection_string_name, tenant_id)
        {
            _user = user_id;
            _tenant = tenant_id;
        }

        public DkimDto CreateFreeDkim(string selector, DbManager db = null)
        {
            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);

            var insert_values_query = new SqlInsert(DkimTable.name)
                .InColumnValue(AddressTable.Columns.id, 0)
                .InColumnValue(DkimTable.Columns.user, _user)
                .InColumnValue(DkimTable.Columns.tenant, _tenant)
                .InColumnValue(DkimTable.Columns.selector, selector)
                .InColumnValue(DkimTable.Columns.private_key, private_key)
                .InColumnValue(DkimTable.Columns.public_key, public_key)
                .Identity(0, 0, true);

            var dkim_id = NullSafeExecuteScalar<int>(db, insert_values_query);
            return new DkimDto(dkim_id, _tenant, _user, DefaultDomainId, selector, private_key, public_key);
        }

        public DkimDto GetFreeDkim(DbManager db = null)
        {
            return GetDomainDkim(DefaultDomainId, db);
        }

        public DkimDto LinkDkimToDomain(int dkim_id, int domain_id, DbManager db)
        {
            var dkim_dto = GetDkim(dkim_id, db);

            if(dkim_dto == null)
                throw new InvalidOperationException(String.Format("Record with dkim id: {0} not found in db.", dkim_id));

            if (dkim_dto.id_domain != domain_id)
            {
                var update_query = new SqlUpdate(DkimTable.name)
                    .Set(DkimTable.Columns.id_domain, domain_id)
                    .Where(DkimTable.Columns.id, dkim_id);

                var rows_affected = db.ExecuteNonQuery(update_query);
                if (rows_affected == 0)
                    throw new InvalidOperationException(String.Format("Record with dkim id: {0} not found in db.", dkim_id));

                dkim_dto.id_domain = domain_id;
                return dkim_dto;
            }

            return dkim_dto;
        }

        public DkimDto GetDomainDkim(int domain_id, DbManager db = null)
        {
            var get_dkim_query = GetDkimQuery()
                .Where(DkimTable.Columns.id_domain, domain_id);

            return NullSafeExecuteList(db, get_dkim_query).Select(r => r.ToDkimDto(_tenant, _user)).FirstOrDefault();
        }

        public DkimDto GetDkim(int dkim_id, DbManager db = null)
        {
            var get_dkim_query = GetDkimQuery()
                .Where(DkimTable.Columns.id, dkim_id);

            return NullSafeExecuteList(db, get_dkim_query).Select(r => r.ToDkimDto(_tenant, _user)).FirstOrDefault();
        }

        public void RemoveUsedDkim(int domain_id, DbManager db = null)
        {
            var remove_dkim_query = new SqlDelete(DkimTable.name)
                .Where(DkimTable.Columns.id_domain, domain_id);

            if (db == null)
            {
                using (db = GetDb())
                    db.ExecuteNonQuery(remove_dkim_query);
            }
            else
                db.ExecuteNonQuery(remove_dkim_query);
        }

        private SqlQuery GetDkimQuery()
        {
            return new SqlQuery(DkimTable.name)
                .Select(DkimTable.Columns.id)
                .Select(DkimTable.Columns.id_domain)
                .Select(DkimTable.Columns.selector)
                .Select(DkimTable.Columns.private_key)
                .Select(DkimTable.Columns.public_key)
                .Where(DkimTable.Columns.tenant, _tenant)
                .Where(DkimTable.Columns.user, _user);
        }
    }
}
