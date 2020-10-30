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
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class ServerDao : BaseDao, IServerDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerTable>();

        public ServerDao(IDbManager dbManager)
            : base(table, dbManager, -1)
        {
        }

        private const string SERVER_ALIAS = "ms";
        private const string SERVER_X_TENANT_ALIAS = "st";

        public Entities.Server Get(int tenant)
        {
            var query = Query(SERVER_ALIAS);

            query.InnerJoin(TenantXServerTable.TABLE_NAME.Alias(SERVER_X_TENANT_ALIAS),
                Exp.EqColumns(
                    ServerTable.Columns.Id.Prefix(SERVER_ALIAS),
                    TenantXServerTable.Columns.ServerId.Prefix(SERVER_X_TENANT_ALIAS)))
                .Where(TenantXServerTable.Columns.Tenant, tenant);

            return Db.ExecuteList(query)
                .ConvertAll(ToServer)
                .SingleOrDefault();
        }

        public List<Entities.Server> GetList()
        {
            var query = Query();

            return Db.ExecuteList(query)
                .ConvertAll(ToServer);
        }

        public int Link(Entities.Server server, int tenant)
        {
            var query = new SqlInsert(TenantXServerTable.TABLE_NAME, true)
                 .InColumnValue(TenantXServerTable.Columns.ServerId, server.Id)
                 .InColumnValue(TenantXServerTable.Columns.Tenant, tenant);

            return Db.ExecuteNonQuery(query);
        }

        public int UnLink(Entities.Server server, int tenant)
        {
            var query = new SqlDelete(TenantXServerTable.TABLE_NAME)
                .Where(TenantXServerTable.Columns.ServerId, server.Id)
                .Where(TenantXServerTable.Columns.Tenant, tenant);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Save(Entities.Server server)
        {
            var query = new SqlInsert(ServerTable.TABLE_NAME, true)
                .InColumnValue(ServerTable.Columns.Id, server.Id)
                .InColumnValue(ServerTable.Columns.MxRecord, server.MxRecord)
                .InColumnValue(ServerTable.Columns.ConnectionString, server.ConnectionString)
                .InColumnValue(ServerTable.Columns.ServerType, server.Type)
                .InColumnValue(ServerTable.Columns.SmtpSettingsId, server.SmtpSettingsId)
                .InColumnValue(ServerTable.Columns.ImapSettingsId, server.ImapSettingsId)
                .Identity(0, 0, true);

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(TenantXServerTable.TABLE_NAME)
                .Where(TenantXServerTable.Columns.ServerId, id);

            Db.ExecuteNonQuery(query);

            query = new SqlDelete(ServerTable.TABLE_NAME)
                .Where(ServerTable.Columns.Id, id);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        protected Entities.Server ToServer(object[] r)
        {
            var s = new Entities.Server
            {
                Id = Convert.ToInt32(r[0]),
                MxRecord = Convert.ToString(r[1]),
                ConnectionString = Convert.ToString(r[2]),
                Type = Convert.ToInt32(r[3]),
                SmtpSettingsId = Convert.ToInt32(r[4]),
                ImapSettingsId = Convert.ToInt32(r[5])
            };

            return s;
        }
    }
}