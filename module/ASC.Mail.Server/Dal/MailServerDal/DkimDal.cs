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
