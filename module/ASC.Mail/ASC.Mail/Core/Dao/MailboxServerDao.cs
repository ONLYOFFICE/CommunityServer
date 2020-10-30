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
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class MailboxServerDao : BaseDao, IMailboxServerDao
    {
        protected static ITable table = new MailTableFactory().Create<MailboxServerTable>();

        public MailboxServerDao(IDbManager dbManager) 
            : base(table, dbManager, -1)
        {
        }

        public MailboxServer GetServer(int id)
        {
            var query = Query()
                .Where(MailboxServerTable.Columns.Id, id);

            return Db.ExecuteList(query)
                .ConvertAll(ToMailboxServer)
               .SingleOrDefault();
        }

        public List<MailboxServer> GetServers(int providerId, bool isUserData = false)
        {
            var query = Query()
                .Where(MailboxServerTable.Columns.ProviderId, providerId)
                .Where(MailboxServerTable.Columns.IsUserData, isUserData);

            return Db.ExecuteList(query)
                .ConvertAll(ToMailboxServer);
        }

        public int SaveServer(MailboxServer mailboxServer)
        {
            var query = new SqlInsert(MailboxServerTable.TABLE_NAME, true)
                .InColumnValue(MailboxServerTable.Columns.Id, mailboxServer.Id)
                .InColumnValue(MailboxServerTable.Columns.ProviderId, mailboxServer.ProviderId)
                .InColumnValue(MailboxServerTable.Columns.Type, mailboxServer.Type)
                .InColumnValue(MailboxServerTable.Columns.Hostname,  mailboxServer.Hostname)
                .InColumnValue(MailboxServerTable.Columns.Port,  mailboxServer.Port)
                .InColumnValue(MailboxServerTable.Columns.SocketType,  mailboxServer.SocketType)
                .InColumnValue(MailboxServerTable.Columns.Username,  mailboxServer.Username)
                .InColumnValue(MailboxServerTable.Columns.Authentication, mailboxServer.Authentication)
                .InColumnValue(MailboxServerTable.Columns.IsUserData, mailboxServer.IsUserData)
                .Identity(0, 0, true);

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int DelteServer(int id)
        {
            var query = new SqlDelete(MailboxServerTable.TABLE_NAME)
                .Where(MailboxServerTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected MailboxServer ToMailboxServer(object[] r)
        {
            var s = new MailboxServer
            {
                Id = Convert.ToInt32(r[0]),
                ProviderId = Convert.ToInt32(r[1]),
                Type = Convert.ToString(r[2]),
                Hostname = Convert.ToString(r[3]),
                Port = Convert.ToInt32(r[4]),
                SocketType = Convert.ToString(r[5]),
                Username = Convert.ToString(r[6]),
                Authentication = Convert.ToString(r[7]),
                IsUserData = Convert.ToBoolean(r[8])
            };

            return s;
        }
    }
}