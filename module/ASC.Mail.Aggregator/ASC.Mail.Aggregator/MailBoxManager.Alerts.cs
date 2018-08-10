/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Runtime.Serialization;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.DbSchema;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public defines

        [DataContract]
        public class MailAlert{
            [DataMember]
            public Int64 id;
            [DataMember]
            public AlertTypes type;
            [DataMember]
            public int id_mailbox;
            [DataMember]
            public string data;
        }

        #endregion

        #region private defines

        public enum AlertTypes
        {
            Empty = 0,
            DeliveryFailure  = 1,
            LinkFailure = 2,
            ExportFailure = 3,
            UploadFailure = 4,
            DisableAllMailboxes = 5,
            AuthConnectFailure = 6,
            TooManyAuthError = 7,
            QuotaError = 8
        }

        public enum UploadToDocumentsErrorType
        {
            FolderNotFound = 1,
            AccessDenied = 2
        }

        [DataContract]
        private struct DeliveryFailure
        {
            [DataMember]
            public string subject;
            [DataMember]
            public string from;
            [DataMember]
            public int message_id;
            [DataMember]
            public int failure_id;
        }

        [DataContract]
        private struct CrmOperationFailure
        {
            [DataMember]
            public int message_id;
        }

        [DataContract]
        private struct UploadToDocumentsFailure
        {
            [DataMember]
            public int error_type;
        }

        #endregion

        #region public methods

        public List<MailAlert> GetMailAlerts(int tenant, string user)
        {
            using (var db = GetDb())
            {
                return FindAlerts(db, tenant, user);
            }
        }

        public int CreateDeliveryFailureAlert(int tenant, string user, int mailboxId, string subject, string from, int messageId, int mailDaemonMessageid)
        {
            var data = new DeliveryFailure
                {
                    @from = @from,
                    message_id = messageId,
                    subject = subject,
                    failure_id = mailDaemonMessageid
                };

            return CreateAlert(tenant, user, mailboxId, AlertTypes.DeliveryFailure, data);
        }

        public int CreateCrmOperationFailureAlert(int tenant, string user, int messageId, AlertTypes type)
        {
            var data = new CrmOperationFailure
                {
                    message_id = messageId
                };

            return CreateAlert(tenant, user, -1, type, data);
        }

        public int CreateUploadToDocumentsFailureAlert(int tenant, string user, int mailboxId, UploadToDocumentsErrorType errorType)
        {
            var data = new UploadToDocumentsFailure
                {
                    error_type = (int) errorType
                };

            return CreateAlert(tenant, user, mailboxId, AlertTypes.UploadFailure, data);
        }

        public int CreateDisableAllMailboxesAlert(int tenant, List<string> users)
        {
            return CreateAlerts(tenant, users, AlertTypes.DisableAllMailboxes);
        }

        public int CreateAuthErrorWarningAlert(IDbManager db, int tenant, string user, int mailboxId)
        {
            return CreateUniqueAlert(db, tenant, user, mailboxId, AlertTypes.AuthConnectFailure);
        }

        public int CreateAuthErrorDisableAlert(IDbManager db, int tenant, string user, int mailboxId)
        {
            var alerts = FindAlerts(db, tenant, user, mailboxId, AlertTypes.AuthConnectFailure);
            if (alerts.Any())
                DeleteAlerts(db, tenant, user, alerts.Select(alert => alert.id).ToList());

            return CreateUniqueAlert(db, tenant, user, mailboxId, AlertTypes.TooManyAuthError);
        }

        public int CreateQuotaErrorWarningAlert(int tenant, string user, IDbManager db = null)
        {
            return db == null? CreateUniqueAlert(tenant, user, -1, AlertTypes.QuotaError): 
                CreateUniqueAlert(db, tenant, user, -1, AlertTypes.QuotaError);
        }

        public void DeleteAlert(int tenant, string user, long alertId)
        {
            using (var db = GetDb())
            {
                var alert = GetAlert(db, tenant, user, alertId);

                if (alert == null)
                    return;

                db.ExecuteNonQuery(
                    new SqlDelete(MailAlertsTable.Name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailAlertsTable.Columns.Id, alertId));

                if (alert.type == AlertTypes.QuotaError)
                {
                    db.ExecuteNonQuery(new SqlUpdate(MailboxTable.Name)
                                           .Where(MailboxTable.Columns.Tenant, tenant)
                                           .Where(MailboxTable.Columns.User, user)
                                           .Where(MailboxTable.Columns.IsRemoved, false)
                                           .Set(MailboxTable.Columns.QuotaError, false));
                }
            }
        }

        public void DeleteAlerts(IDbManager db, int tenant, string user, List<long> alertIds)
        {
            db.ExecuteNonQuery(
                new SqlDelete(MailAlertsTable.Name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(Exp.In(MailAlertsTable.Columns.Id, alertIds)));
        }

        public void DeleteAlerts(IDbManager db, int tenant, string user, int mailboxId)
        {
            db.ExecuteNonQuery(
                new SqlDelete(MailAlertsTable.Name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailAlertsTable.Columns.MailboxId, mailboxId));
        }

        public List<MailAlert> FindAlerts(IDbManager db, int tenant, string user, int mailboxId = -1, AlertTypes type = AlertTypes.Empty)
        {
            var searchQuery = new SqlQuery(MailAlertsTable.Name)
                    .Select(MailAlertsTable.Columns.Id, MailAlertsTable.Columns.Type, MailAlertsTable.Columns.MailboxId, MailAlertsTable.Columns.Data)
                    .Where(GetUserWhere(user, tenant));

            if (mailboxId > 0)
                searchQuery.Where(MailAlertsTable.Columns.MailboxId, mailboxId);

            if (type != AlertTypes.Empty)
                searchQuery.Where(MailAlertsTable.Columns.Type, (int)type);

            var foundAlerts = db.ExecuteList(searchQuery)
                .ConvertAll(x => new MailAlert
                {
                    id = Convert.ToInt32(x[0]),
                    type = (AlertTypes)x[1],
                    id_mailbox = Convert.ToInt32(x[2]),
                    data = (string)x[3]
                });

            return foundAlerts;
        }

        public MailAlert GetAlert(IDbManager db, int tenant, string user, long alertId)
        {
            var searchQuery = new SqlQuery(MailAlertsTable.Name)
                    .Select(MailAlertsTable.Columns.Id, MailAlertsTable.Columns.Type, MailAlertsTable.Columns.MailboxId, MailAlertsTable.Columns.Data)
                    .Where(MailAlertsTable.Columns.Id, alertId)
                    .Where(GetUserWhere(user, tenant));

            var foundAlert = db.ExecuteList(searchQuery)
                .ConvertAll(x => new MailAlert
                {
                    id = Convert.ToInt32(x[0]),
                    type = (AlertTypes)x[1],
                    id_mailbox = Convert.ToInt32(x[2]),
                    data = (string)x[3]
                }).FirstOrDefault();

            return foundAlert;
        }

        #endregion

        #region private methods

        private int CreateUniqueAlert(IDbManager db, int tenant, string user, int mailboxId, AlertTypes type, object data = null)
        {
            var alerts = FindAlerts(db, tenant, user, mailboxId, type);

            if (alerts.Any())
                return -1;

            var jsonData = MailUtil.GetJsonString(data);

            var insertQuery = new SqlInsert(MailAlertsTable.Name)
                .InColumnValue(MailAlertsTable.Columns.Tenant, tenant)
                .InColumnValue(MailAlertsTable.Columns.User, user)
                .InColumnValue(MailAlertsTable.Columns.MailboxId, mailboxId)
                .InColumnValue(MailAlertsTable.Columns.Type, (int)type)
                .InColumnValue(MailAlertsTable.Columns.Data, jsonData);

            return db.ExecuteNonQuery(insertQuery);
        }

        private int CreateUniqueAlert(int tenant, string user, int mailboxId, AlertTypes type, object data = null)
        {
            using (var db = GetDb())
            {
                return CreateUniqueAlert(db, tenant, user, mailboxId, type, data);
            }
        }

        private int CreateAlert(int tenant, string user, int mailboxId, AlertTypes type, object data = null)
        {
            using (var db = GetDb())
            {
                var jsonData = MailUtil.GetJsonString(data);

                var insertQuery = new SqlInsert(MailAlertsTable.Name)
                    .InColumnValue(MailAlertsTable.Columns.Tenant, tenant)
                    .InColumnValue(MailAlertsTable.Columns.User, user)
                    .InColumnValue(MailAlertsTable.Columns.MailboxId, mailboxId)
                    .InColumnValue(MailAlertsTable.Columns.Type, (int)type)
                    .InColumnValue(MailAlertsTable.Columns.Data, jsonData);

                return db.ExecuteNonQuery(insertQuery);
            }
        }

        private int CreateAlerts(int tenant, List<string> users, AlertTypes type, object data = null)
        {
            using (var db = GetDb())
            {
                return CreateAlerts(db, tenant, users, type, data);
            }
        }

        private int CreateAlerts(IDbManager db, int tenant, List<string> users, AlertTypes type, object data = null)
        {
            var result = 0;

            if (!users.Any()) return result;

            
                var jsonData = MailUtil.GetJsonString(data);

                CreateInsertDelegate createInsertQuery = ()
                => new SqlInsert(MailAlertsTable.Name)
                        .IgnoreExists(true)
                        .InColumns(MailAlertsTable.Columns.Tenant,
                                   MailAlertsTable.Columns.User,
                                   MailAlertsTable.Columns.MailboxId,
                                   MailAlertsTable.Columns.Type,
                                   MailAlertsTable.Columns.Data);

                var insertQuery = createInsertQuery();

                int i, usersLen;
                for (i = 0, usersLen = users.Count; i < usersLen; i++)
                {
                    var user = users[i];

                    insertQuery
                        .Values(tenant, user, -1, (int) type, jsonData);

                    if ((i%100 != 0 || i == 0) && i + 1 != usersLen) continue;

                    result += db.ExecuteNonQuery(insertQuery);
                    insertQuery = createInsertQuery();
                }
            
            return result;
        }

        #endregion
    }
}
