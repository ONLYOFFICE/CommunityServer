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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using System.Runtime.Serialization.Json;
using System.IO;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public defines

        [DataContract]
        public struct MailAlert{
            [DataMember]
            public Int64 id;
            [DataMember]
            public int type;
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
            TooManyAuthError = 7
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

        #region db defines

        private const string MAIL_ALERTS = "mail_alerts";

        private struct MailAlerts
        {
            public const string id = "id";
            public const string id_tenant = "tenant";
            public const string id_user = "id_user";
            public const string id_mailbox = "id_mailbox";
            public const string type = "type";
            public const string data = "data";
        };

        #endregion

        #region public methods

        public List<MailAlert> GetMailAlerts(int tenant, string id_user)
        {
            using (var db = GetDb())
            {
                return FindAlerts(db, tenant, id_user);
            }
        }

        public int CreateDeliveryFailureAlert(int tenant, string id_user, string subject, string from, int message_id)
        {
            var data = new DeliveryFailure
                {
                    @from = @from,
                    message_id = message_id,
                    subject = subject
                };

            return CreateAlert(tenant, id_user, -1, AlertTypes.DeliveryFailure, data);
        }

        public int CreateCrmOperationFailureAlert(int tenant, string id_user, int message_id, AlertTypes type)
        {
            var data = new CrmOperationFailure
                {
                    message_id = message_id
                };

            return CreateAlert(tenant, id_user, -1, type, data);
        }

        public int CreateUploadToDocumentsFailureAlert(int tenant, string id_user, int mailbox_id, UploadToDocumentsErrorType error_type)
        {
            var data = new UploadToDocumentsFailure
                {
                    error_type = (int) error_type
                };

            return CreateAlert(tenant, id_user, mailbox_id, AlertTypes.UploadFailure, data);
        }

        public int CreateDisableAllMailboxesAlert(int tenant, IEnumerable<string> ids_user)
        {
            return CreateAlerts(tenant, ids_user, AlertTypes.DisableAllMailboxes);
        }

        public int CreateAuthErrorWarningAlert(int tenant, string id_user, int mailbox_id)
        {
            return CreateUniqueAlert(tenant, id_user, mailbox_id, AlertTypes.AuthConnectFailure);
        }

        public int CreateAuthErrorDisableAlert(int tenant, string id_user, int id_mailbox)
        {
            using (var db = GetDb())
            {
                var alerts = FindAlerts(db, tenant, id_user, id_mailbox, AlertTypes.AuthConnectFailure);
                if (alerts.Any())
                    DeleteAlerts(db, tenant, id_user, alerts.Select(alert => alert.id).ToList());

                return CreateUniqueAlert(db, tenant, id_user, id_mailbox, AlertTypes.TooManyAuthError);
            }
        }

        public void DeleteAlert(int tenant, string id_user, long id_alert)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlDelete(MAIL_ALERTS)
                        .Where(GetUserWhere(id_user, tenant))
                        .Where(MailAlerts.id, id_alert));
            }
        }

        public void DeleteAlerts(IDbManager db, int tenant, string id_user, List<long> alert_ids)
        {
            db.ExecuteNonQuery(
                new SqlDelete(MAIL_ALERTS)
                    .Where(GetUserWhere(id_user, tenant))
                    .Where(Exp.In(MailAlerts.id, alert_ids)));
        }

        #endregion

        #region private methods

        private string GetJsonString(object data)
        {
            string json_data = null;
            if (data != null)
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(data.GetType());
                    serializer.WriteObject(stream, data);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        json_data = reader.ReadToEnd();
                    }
                }
            }
            return json_data;
        }

        private List<MailAlert> FindAlerts(IDbManager db, int tenant, string id_user, int id_mailbox = -1, AlertTypes type = AlertTypes.Empty)
        {
            var search_query = new SqlQuery(MAIL_ALERTS)
                    .Select(MailAlerts.id, MailAlerts.type, MailAlerts.id_mailbox, MailAlerts.data)
                    .Where(GetUserWhere(id_user, tenant));

            if (id_mailbox > 0)
                search_query.Where(MailAlerts.id_mailbox, id_mailbox);

            if (type != AlertTypes.Empty)
                search_query.Where(MailAlerts.type, (int) type);

            var found_alerts = db.ExecuteList(search_query)
                .ConvertAll(x => new MailAlert
                {
                    id = Convert.ToInt32(x[0]),
                    type = Convert.ToInt32(x[1]),
                    id_mailbox = Convert.ToInt32(x[2]),
                    data = (string) x[3]
                });

            return found_alerts;
        }

        private int CreateUniqueAlert(IDbManager db, int tenant, string id_user, int id_mailbox, AlertTypes type, object data = null)
        {
            var alerts = FindAlerts(db, tenant, id_user, id_mailbox, type);

            if (alerts.Any())
                return -1;

            var json_data = GetJsonString(data);

            var insert_query = new SqlInsert(MAIL_ALERTS)
                .InColumnValue(MailAlerts.id_tenant, tenant)
                .InColumnValue(MailAlerts.id_user, id_user)
                .InColumnValue(MailAlerts.id_mailbox, id_mailbox)
                .InColumnValue(MailAlerts.type, (int)type)
                .InColumnValue(MailAlerts.data, json_data);

            return db.ExecuteNonQuery(insert_query);
        }

        private int CreateUniqueAlert(int tenant, string id_user, int id_mailbox, AlertTypes type, object data = null)
        {
            using (var db = GetDb())
            {
                return CreateUniqueAlert(db, tenant, id_user, id_mailbox, type, data);
            }
        }

        private int CreateAlert(int tenant, string id_user, int id_mailbox, AlertTypes type, object data = null)
        {
            using (var db = GetDb())
            {
                var json_data = GetJsonString(data);

                var insert_query = new SqlInsert(MAIL_ALERTS)
                    .InColumnValue(MailAlerts.id_tenant, tenant)
                    .InColumnValue(MailAlerts.id_user, id_user)
                    .InColumnValue(MailAlerts.id_mailbox, id_mailbox)
                    .InColumnValue(MailAlerts.type, (int) type)
                    .InColumnValue(MailAlerts.data, json_data);

                return db.ExecuteNonQuery(insert_query);
            }
        }

        private int CreateAlerts(int tenant, IEnumerable<string> ids_user, AlertTypes type, object data = null)
        {
            var result = 0;

            var users = ids_user as string[] ?? ids_user.ToArray();
            if (users.Length == 0) return result;

            using (var db = GetDb())
            {
                var json_data = GetJsonString(data);

                CreateInsertDelegate create_insert_query = ()
                => new SqlInsert(MAIL_ALERTS)
                        .IgnoreExists(true)
                        .InColumns(MailAlerts.id_tenant,
                                   MailAlerts.id_user,
                                   MailAlerts.id_mailbox,
                                   MailAlerts.type,
                                   MailAlerts.data);

                var insert_query = create_insert_query();

                int i, users_len;
                for (i = 0, users_len = users.Length; i < users_len; i++)
                {
                    var user_id = users[i];

                    insert_query
                        .Values(tenant, user_id, -1, (int) type, json_data);

                    if ((i%100 != 0 || i == 0) && i + 1 != users_len) continue;

                    result += db.ExecuteNonQuery(insert_query);
                    insert_query = create_insert_query();
                }
            }
            return result;
        }

        #endregion
    }
}
