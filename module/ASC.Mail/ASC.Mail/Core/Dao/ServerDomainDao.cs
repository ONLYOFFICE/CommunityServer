/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class ServerDomainDao : BaseDao, IServerDomainDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerDomainTable>();

        public ServerDomainDao(IDbManager dbManager, int tenant)
            : base(table, dbManager, tenant)
        {
        }

        public int Save(ServerDomain domain)
        {
            var query = new SqlInsert(ServerDomainTable.TABLE_NAME)
                .InColumnValue(ServerDomainTable.Columns.Id, domain.Id)
                .InColumnValue(ServerDomainTable.Columns.DomainName, domain.Name)
                .InColumnValue(ServerDomainTable.Columns.Tenant, Tenant)
                .InColumnValue(ServerDomainTable.Columns.IsVerified, domain.IsVerified)
                .Identity(0, 0, true);

            if (domain.Id <= 0)
            {
                query.InColumnValue(ServerDomainTable.Columns.DateAdded, DateTime.UtcNow);
            }

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(ServerDomainTable.TABLE_NAME)
                .Where(ServerDomainTable.Columns.Tenant, Tenant)
                .Where(ServerDomainTable.Columns.Id, id);

            var result = Db.ExecuteNonQuery(query);

            query = new SqlDelete(ServerDnsTable.TABLE_NAME)
                .Where(ServerDnsTable.Columns.Tenant, Tenant)
                .Where(ServerDnsTable.Columns.DomainId, id);

            Db.ExecuteNonQuery(query);

            return result;
        }

        public List<ServerDomain> GetDomains()
        {
            var query = Query()
                .Where(Exp.In(ServerDomainTable.Columns.Tenant, new List<int> {Tenant, Defines.SHARED_TENANT_ID}));

            var list = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain);

            return list;
        }

        public List<ServerDomain> GetAllDomains()
        {
            var query = Query();

            var list = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain);

            return list;
        }

        public ServerDomain GetDomain(int id)
        {
            var query = Query()
                .Where(Exp.In(ServerDomainTable.Columns.Tenant, new List<int> { Tenant, Defines.SHARED_TENANT_ID }))
                .Where(ServerDomainTable.Columns.Id, id);

            var domain = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain)
                .SingleOrDefault();

            return domain;
        }

        public bool IsDomainExists(string name)
        {
            var query = Query()
                .Where(ServerDomainTable.Columns.DomainName, name);

            var domain = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain)
                .SingleOrDefault();

            return domain != null;
        }

        public int SetVerified(int id, bool isVerified)
        {
            var query = new SqlUpdate(ServerDomainTable.TABLE_NAME)
                .Set(ServerDomainTable.Columns.IsVerified, isVerified)
                .Set(ServerDomainTable.Columns.DateChecked, DateTime.UtcNow)
                .Where(ServerDomainTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected ServerDomain ToServerDomain(object[] r)
        {
            var d = new ServerDomain
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                Name = Convert.ToString(r[2]),
                IsVerified = Convert.ToBoolean(r[3]),
                DateAdded = Convert.ToDateTime(r[4]),
                DateChecked = Convert.ToDateTime(r[5])
            };

            return d;
        }
    }
}