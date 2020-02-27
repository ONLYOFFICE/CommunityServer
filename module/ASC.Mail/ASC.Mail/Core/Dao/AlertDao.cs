/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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