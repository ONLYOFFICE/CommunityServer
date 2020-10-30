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
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Dao
{
    public class AlertDao : BaseDao, IAlertDao
    {
        protected static ITable table = new MailTableFactory().Create<AlertsTable>();

        protected string CurrentUserId { get; private set; }

        public AlertDao(IDbManager dbManager, int tenant, string user = null) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public Alert GetAlert(long id)
        {
            var query = Query()
                .Where(AlertsTable.Columns.Tenant, Tenant)
                .Where(AlertsTable.Columns.User, CurrentUserId)
                .Where(AlertsTable.Columns.Id, id);

            return Db.ExecuteList(query)
                .ConvertAll(ToAlert)
                .SingleOrDefault();
        }

        public List<Alert> GetAlerts(int mailboxId = -1, MailAlertTypes type = MailAlertTypes.Empty)
        {
            var query = Query()
                .Where(AlertsTable.Columns.Tenant, Tenant)
                .Where(AlertsTable.Columns.User, CurrentUserId);

            if (mailboxId > 0)
                query.Where(AlertsTable.Columns.MailboxId, mailboxId);

            if (type != MailAlertTypes.Empty)
                query.Where(AlertsTable.Columns.Type, (int)type);

            return Db.ExecuteList(query)
                .ConvertAll(ToAlert);
        }

        public int SaveAlert(Alert alert, bool unique = false)
        {
            if (unique)
            {
                var alerts = GetAlerts(alert.MailboxId, alert.Type);

                if (alerts.Any())
                {
                    var result = DeleteAlerts(alerts.Select(a => a.Id).ToList());

                    if(result <= 0)
                        throw new Exception("Delete old alerts failed");
                }
            }

            var query = new SqlInsert(AlertsTable.TABLE_NAME, true)
                .InColumnValue(AlertsTable.Columns.Id, alert.Id)
                .InColumnValue(AlertsTable.Columns.Tenant, alert.Tenant)
                .InColumnValue(AlertsTable.Columns.User, alert.User)
                .InColumnValue(AlertsTable.Columns.MailboxId, alert.MailboxId)
                .InColumnValue(AlertsTable.Columns.Type, (int)alert.Type)
                .InColumnValue(AlertsTable.Columns.Data, alert.Data);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteAlert(long id)
        {
            var query = new SqlDelete(AlertsTable.TABLE_NAME)
                .Where(AlertsTable.Columns.Tenant, Tenant)
                .Where(AlertsTable.Columns.User, CurrentUserId)
                .Where(AlertsTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteAlerts(int mailboxId)
        {
            var query = new SqlDelete(AlertsTable.TABLE_NAME)
               .Where(AlertsTable.Columns.Tenant, Tenant)
               .Where(AlertsTable.Columns.User, CurrentUserId)
               .Where(AlertsTable.Columns.MailboxId, mailboxId);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteAlerts(List<int> ids)
        {
            var query = new SqlDelete(AlertsTable.TABLE_NAME)
                .Where(AlertsTable.Columns.Tenant, Tenant)
                .Where(AlertsTable.Columns.User, CurrentUserId)
                .Where(Exp.In(AlertsTable.Columns.Id, ids));

            return Db.ExecuteNonQuery(query);
        }

        protected Alert ToAlert(object[] r)
        {
            var f = new Alert
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                MailboxId = Convert.ToInt32(r[3]),
                Type = (MailAlertTypes)Convert.ToInt32(r[4]),
                Data = Convert.ToString(r[5])
            };

            return f;
        }
    }
}