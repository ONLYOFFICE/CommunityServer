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
using System.Runtime.Serialization;
using ASC.Common.Logging;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Utils;

// ReSharper disable InconsistentNaming

namespace ASC.Mail.Core.Engine
{
    public class AlertEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public AlertEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.AlertEngine");
        }

        public List<MailAlertData> GetAlerts(int mailboxId = -1, MailAlertTypes type = MailAlertTypes.Empty)
        {
            using (var dao = new DaoFactory())
            {
                return dao.CreateAlertDao(Tenant, User)
                    .GetAlerts(mailboxId, type)
                    .ConvertAll(ToMailAlert);
            }
        }

        public bool DeleteAlert(long id)
        {
            using (var dao = new DaoFactory())
            {
                var result = dao.CreateAlertDao(Tenant, User)
                    .DeleteAlert(id);

                if (result <= 0)
                    return false;
            }

            return true;
        }

        public bool DeleteAlert(MailAlertTypes type)
        {
            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateAlertDao(Tenant, User);

                var quotaAlerts = dao.GetAlerts(-1, type);

                if (!quotaAlerts.Any()) 
                    return true;

                var result = dao.DeleteAlerts(quotaAlerts.Select(a => a.Id).ToList());

                if (result <= 0)
                    throw new Exception("Delete old alerts failed");
            }

            return true;
        }

        [DataContract]
        private struct UploadToDocumentsFailure
        {
            [DataMember]
            public int error_type;
        }

        public int CreateUploadToDocumentsFailureAlert(int tenant, string user, int mailboxId, UploadToDocumentsErrorType errorType)
        {
            var data = new UploadToDocumentsFailure
            {
                error_type = (int)errorType
            };

            var jsonData = MailUtil.GetJsonString(data);

            var alert = new Alert
            {
                Tenant = tenant,
                User = user,
                MailboxId = mailboxId,
                Type = MailAlertTypes.UploadFailure,
                Data = jsonData
            };

            using (var dao = new DaoFactory())
            {
                var result = dao.CreateAlertDao(tenant, user).SaveAlert(alert);

                if (result <= 0)
                    throw new Exception("Save alert failed");

                return result;
            }
        }

        public int CreateDisableAllMailboxesAlert(int tenant, List<string> users)
        {
            var result = 0;

            if (!users.Any()) return result;

            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateAlertDao(tenant);

                foreach (var user in users)
                {
                    var alert = new Alert
                    {
                        Tenant = tenant,
                        User = user,
                        MailboxId = -1,
                        Type = MailAlertTypes.DisableAllMailboxes,
                        Data = null
                    };

                    var r = dao.SaveAlert(alert);

                    if (r <= 0)
                        throw new Exception("Save alert failed");

                    result += r;
                }
            }

            return result;
        }

        public int CreateAuthErrorWarningAlert(int tenant, string user, int mailboxId)
        {
            var alert = new Alert
            {
                Tenant = tenant,
                User = user,
                MailboxId = mailboxId,
                Type = MailAlertTypes.AuthConnectFailure,
                Data = null
            };

            using (var dao = new DaoFactory())
            {
                var result = dao.CreateAlertDao(tenant, user).SaveAlert(alert, true);

                if (result <= 0)
                    throw new Exception("Save alert failed");

                return result;
            }
        }

        public int CreateAuthErrorDisableAlert(int tenant, string user, int mailboxId)
        {
            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateAlertDao(tenant, user);

                var alerts = dao.GetAlerts(mailboxId, MailAlertTypes.AuthConnectFailure);

                if (alerts.Any())
                {
                    var r = dao.DeleteAlerts(alerts.Select(a => a.Id).ToList());

                    if (r <= 0)
                        throw new Exception("Delete alerts failed");
                }

                var alert = new Alert
                {
                    Tenant = tenant,
                    User = user,
                    MailboxId = mailboxId,
                    Type = MailAlertTypes.TooManyAuthError,
                    Data = null
                };

                var result = dao.SaveAlert(alert, true);

                if (result <= 0)
                    throw new Exception("Save alert failed");

                return result;
            }
        }

        public int CreateQuotaErrorWarningAlert(int tenant, string user)
        {
            var alert = new Alert
            {
                Tenant = tenant,
                User = user,
                MailboxId = -1,
                Type = MailAlertTypes.QuotaError,
                Data = null
            };

            using (var dao = new DaoFactory())
            {
                var result = dao.CreateAlertDao(tenant, user).SaveAlert(alert, true);

                if (result <= 0)
                    throw new Exception("Save alert failed");

                return result;
            }
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

        public int CreateDeliveryFailureAlert(int tenant, string user, int mailboxId, string subject, string from,
            int messageId, int mailDaemonMessageid)
        {
            var data = new DeliveryFailure
            {
                @from = from,
                message_id = messageId,
                subject = subject,
                failure_id = mailDaemonMessageid
            };

            var jsonData = MailUtil.GetJsonString(data);

            var alert = new Alert
            {
                Tenant = tenant,
                User = user,
                MailboxId = mailboxId,
                Type = MailAlertTypes.DeliveryFailure,
                Data = jsonData
            };

            using (var dao = new DaoFactory())
            {
                var result = dao.CreateAlertDao(tenant, user).SaveAlert(alert);

                if (result <= 0)
                    throw new Exception("Save alert failed");

                return result;
            }
        }

        /*[DataContract]
        private struct CrmOperationFailure
        {
            [DataMember]
            public int message_id;
        }

        public int CreateCrmOperationFailureAlert(int tenant, string user, int messageId, MailAlertTypes type)
        {
            var data = new CrmOperationFailure
            {
                message_id = messageId
            };

            var jsonData = MailUtil.GetJsonString(data);

            var alert = new Alert
            {
                Tenant = tenant,
                User = user,
                MailboxId = -1,
                Type = type,
                Data = jsonData
            };

            using (var dao = new DaoFactory())
            {
                var result = dao.GetAlertDao().SaveAlert(alert);

                if (result <= 0)
                    throw new Exception("Save alert failed");

                return result;
            }
        }*/

        protected MailAlertData ToMailAlert(Alert a)
        {
            return new MailAlertData
            {
                id = a.Id,
                id_mailbox = a.MailboxId,
                type = a.Type,
                data = a.Data
            };
        }
    }
}
