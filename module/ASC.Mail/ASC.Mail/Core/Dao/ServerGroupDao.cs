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
using ASC.Mail.Server.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class ServerGroupDao : BaseDao, IServerGroupDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerMailGroupTable>();

        public ServerGroupDao(IDbManager dbManager, int tenant) 
            : base(table, dbManager, tenant)
        {
        }

        public ServerGroup Get(int id)
        {
            var query = Query()
                .Where(ServerMailGroupTable.Columns.Tenant, Tenant)
                .Where(ServerMailGroupTable.Columns.Id, id);

            var group = Db.ExecuteList(query)
                .ConvertAll(ToServerGroup)
                .SingleOrDefault();

            return group;
        }

        public List<ServerGroup> GetList()
        {
            var query = Query()
                .Where(ServerMailGroupTable.Columns.Tenant, Tenant);

            var groups = Db.ExecuteList(query)
                .ConvertAll(ToServerGroup);

            return groups;
        }

        public List<ServerGroup> GetList(int domainId)
        {
            const string group_alias = "msg";
            const string address_alias = "msa";

            var query = Query(group_alias)
                .InnerJoin(ServerAddressTable.TABLE_NAME.Alias(address_alias),
                    Exp.EqColumns(
                        ServerMailGroupTable.Columns.AddressId.Prefix(group_alias),
                        ServerAddressTable.Columns.Id.Prefix(address_alias)
                        )
                )
                .Where(ServerMailGroupTable.Columns.Tenant.Prefix(group_alias), Tenant)
                .Where(ServerAddressTable.Columns.DomainId.Prefix(address_alias), domainId)
                .Where(ServerAddressTable.Columns.IsMailGroup.Prefix(address_alias), true);

            var groups = Db.ExecuteList(query)
                .ConvertAll(ToServerGroup);

            return groups;
        }

        public int Save(ServerGroup @group)
        {
            var query = new SqlInsert(ServerMailGroupTable.TABLE_NAME, true)
                .InColumnValue(ServerMailGroupTable.Columns.Id, group.Id)
                .InColumnValue(ServerMailGroupTable.Columns.Tenant, group.Tenant)
                .InColumnValue(ServerMailGroupTable.Columns.Address, group.Address)
                .InColumnValue(ServerMailGroupTable.Columns.AddressId, group.AddressId)
                .InColumnValue(ServerMailGroupTable.Columns.DateCreated, group.DateCreated)
                .Identity(0, 0, true);

            return Db.ExecuteScalar<int>(query);
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(ServerMailGroupTable.TABLE_NAME)
                .Where(ServerMailGroupTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected ServerGroup ToServerGroup(object[] r)
        {
            var group = new ServerGroup
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                AddressId = Convert.ToInt32(r[2]),
                Address = Convert.ToString(r[3]),
                DateCreated = Convert.ToDateTime(r[4])
            };

            return group;
        }
    }
}