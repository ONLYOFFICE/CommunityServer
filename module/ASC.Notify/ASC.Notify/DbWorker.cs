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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Notify.Config;
using ASC.Notify.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace ASC.Notify
{
    class DbWorker
    {
        private readonly string dbid = NotifyServiceCfg.ConnectionStringName;
        private readonly object syncRoot = new object();


        public int SaveMessage(NotifyMessage m)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var i = new SqlInsert("notify_queue")
                    .InColumns("notify_id", "tenant_id", "sender", "reciever", "subject", "content_type", "content", "sender_type", "creation_date", "reply_to", "attachments", "auto_submitted")
                    .Values(0, m.Tenant, m.From, m.To, m.Subject, m.ContentType, m.Content, m.Sender, m.CreationDate, m.ReplyTo, JsonConvert.SerializeObject(m.EmbeddedAttachments), m.AutoSubmitted)
                    .Identity(0, 0, true);

                var id = db.ExecuteScalar<int>(i);

                i = new SqlInsert("notify_info")
                    .InColumns("notify_id", "state", "attempts", "modify_date", "priority")
                    .Values(id, 0, 0, DateTime.UtcNow, m.Priority);
                var affected = db.ExecuteNonQuery(i);

                tx.Commit();
                return affected;
            }
        }

        public IDictionary<int, NotifyMessage> GetMessages(int count)
        {
            lock (syncRoot)
            {
                using (var db = GetDb())
                using (var tx = db.BeginTransaction())
                {
                    var q = new SqlQuery("notify_queue q")
                        .InnerJoin("notify_info i", Exp.EqColumns("q.notify_id", "i.notify_id"))
                        .Select("q.notify_id", "q.tenant_id", "q.sender", "q.reciever", "q.subject", "q.content_type", "q.content", "q.sender_type", "q.creation_date", "q.reply_to", "q.attachments", "q.auto_submitted")
                        .Where(Exp.Eq("i.state", MailSendingState.NotSended) | (Exp.Eq("i.state", MailSendingState.Error) & Exp.Lt("i.modify_date", DateTime.UtcNow - NotifyServiceCfg.AttemptsInterval)))
                        .OrderBy("i.priority", true)
                        .OrderBy("i.notify_id", true)
                        .SetMaxResults(count);

                    var messages = db
                        .ExecuteList(q)
                        .ToDictionary(
                            r => Convert.ToInt32(r[0]),
                            r => new NotifyMessage
                            {
                                Tenant = Convert.ToInt32(r[1]),
                                From = (string)r[2],
                                To = (string)r[3],
                                Subject = (string)r[4],
                                ContentType = (string)r[5],
                                Content = (string)r[6],
                                Sender = (string)r[7],
                                CreationDate = Convert.ToDateTime(r[8]),
                                ReplyTo = (string)r[9],
                                EmbeddedAttachments = JsonConvert.DeserializeObject<NotifyMessageAttachment[]>((string)r[10]),
                                AutoSubmitted = (string)r[11]
                            });

                    var u = new SqlUpdate("notify_info").Set("state", MailSendingState.Sending).Where(Exp.In("notify_id", messages.Keys));
                    db.ExecuteNonQuery(u);
                    tx.Commit();

                    return messages;
                }
            }
        }


        public void ResetStates()
        {
            using (var db = GetDb())
            {
                var u = new SqlUpdate("notify_info").Set("state", 0).Where("state", 1);
                db.ExecuteNonQuery(u);
            }
        }

        public void SetState(int id, MailSendingState result)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (result == MailSendingState.Sended)
                {
                    var d = new SqlDelete("notify_info").Where("notify_id", id);
                    db.ExecuteNonQuery(d);
                }
                else
                {
                    if (result == MailSendingState.Error)
                    {
                        var q = new SqlQuery("notify_info").Select("attempts").Where("notify_id", id);
                        var attempts = db.ExecuteScalar<int>(q);
                        if (NotifyServiceCfg.MaxAttempts <= attempts + 1)
                        {
                            result = MailSendingState.FatalError;
                        }
                    }
                    var u = new SqlUpdate("notify_info")
                        .Set("state", (int)result)
                        .Set("attempts = attempts + 1")
                        .Set("modify_date", DateTime.UtcNow)
                        .Where("notify_id", id);
                    db.ExecuteNonQuery(u);
                }
                tx.Commit();
            }
        }


        private DbManager GetDb()
        {
            return new DbManager(dbid);
        }
    }
}
