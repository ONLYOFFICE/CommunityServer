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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Remoting.Messaging;
using System.Web;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x;
using ASC.Xmpp.Core.utils;
using ASC.ElasticSearch;
using ASC.Web.Talk;

namespace ASC.Xmpp.Server.Storage
{
    public class DbMessageArchive : DbStoreBase
    {
        private IDictionary<string, bool> loggingCache = new ConcurrentDictionary<string, bool>();

        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_archive", true)
                .AddColumn(new SqlCreate.Column("id", DbType.Int32).NotNull(true).PrimaryKey(true).Autoincrement(true))
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("stamp", DbType.DateTime, true)
                .AddColumn("message", DbType.String, MESSAGE_COLUMN_LEN)
                .AddIndex("jabber_archive_jid", "jid");

            var t2 = new SqlCreate.Table("jabber_archive_switch", true)
                .AddColumn(new SqlCreate.Column("id", DbType.String, 255).NotNull(true).PrimaryKey(true));

            return new[] { t1, t2 };
        }


        public void SetMessageLogging(Jid from, Jid to, bool logging)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            var key = GetKey(from, to);
            if (logging)
            {
                ExecuteNonQuery(new SqlDelete("jabber_archive_switch").Where("id", key));
                loggingCache.Remove(key);
            }
            else
            {
                ExecuteNonQuery(new SqlInsert("jabber_archive_switch", true).InColumnValue("id", key));
                loggingCache[key] = false;
            }
        }

        public bool GetMessageLogging(Jid from, Jid to)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            return !loggingCache.ContainsKey(GetKey(from, to));
        }


        public void SaveMessages(params Message[] messages)
        {
            if (messages == null) throw new ArgumentNullException("message");
            if (messages.Length == 0) return;

            foreach (var m in messages)
            {
                if (string.IsNullOrEmpty(m.Body) && string.IsNullOrEmpty(m.Subject) && string.IsNullOrEmpty(m.Thread) && m.Html == null)
                {
                    continue;
                }

                if (m.XDelay == null) m.XDelay = new Delay();
                if (m.XDelay.Stamp == default(DateTime)) m.XDelay.Stamp = DateTime.UtcNow;

                var message = ElementSerializer.SerializeElement(m);
                var jid = GetKey(m.From, m.To);
                var stamp = DateTime.UtcNow;

                var id = ExecuteScalar<int>(
                    new SqlInsert("jabber_archive")
                    .InColumnValue("id", 0)
                    .InColumnValue("jid", jid)
                    .InColumnValue("stamp", stamp)
                    .InColumnValue("message", message)
                    .Identity(0, 0, true));

                FactoryIndexer<JabberWrapper>.IndexAsync(new JabberWrapper
                {
                    Id = id,
                    Jid = jid,
                    LastModifiedOn = stamp,
                    Message = message
                });
            }
        }

        public void ClearUnreadMessages(Jid from, Jid to, IServiceProvider serviceProvider)
        {
            var offlineStore = ((StorageManager)serviceProvider.GetService(typeof(StorageManager))).OfflineStorage;
            offlineStore.RemoveAllOfflineMessages(from, to);
        }

        public Message[] GetMessages(Jid from, Jid to, DateTime start, DateTime end, string text, int count, int startindex = 0)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            var q = new SqlQuery("jabber_archive")
                .Select("message")
                .Where("jid", GetKey(from, to))
                .OrderBy("id", false);

            if (start != DateTime.MinValue)
            {
                q.Where(Exp.Ge("stamp", start));
            }
            if (end != DateTime.MaxValue)
            {
                q.Where(Exp.Le("stamp", end));
            }
            if (startindex < int.MaxValue)
            {
                q.SetFirstResult(startindex);
            }
            if (0 < count && count < int.MaxValue)
            {
                q.SetMaxResults(count);
            }

            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    List<int> ids;
                    CallContext.SetData(TenantManager.CURRENT_TENANT, CoreContext.TenantManager.GetTenant(from.Server));
                    if (FactoryIndexer<Web.Talk.JabberWrapper>.TrySelectIds(r => r.MatchAll(HttpUtility.HtmlDecode(text)), out ids))
                    {
                        q.Where(Exp.In("id", ids));
                    }

                }
                finally
                {
                    CallContext.SetData(TenantManager.CURRENT_TENANT, null);
                }
            }

            var messages = ExecuteList(q).ConvertAll(r => ElementSerializer.DeSerializeElement<Message>((string)r[0]));
            messages.Reverse();
            return messages.ToArray();
        }

        public Message[] GetMessages(Jid from, Jid to, int id, int count)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            var q = new SqlQuery("jabber_archive")
                .UseIndex("jabber_archive_jid")
                .Select("id", "stamp", "message")
                .Where("jid", GetKey(from, to))
                .Where(Exp.Lt("id", id))
                .OrderBy("id", false);
            if (0 < count && count < int.MaxValue) q.SetMaxResults(count);

            var messages = ExecuteList(q).ConvertAll(r =>
            {
                Message m;
                try
                {
                    var internalId = Convert.ToInt32(r[0]);
                    var dbStamp = Convert.ToDateTime(r[1]);
                    m = ElementSerializer.DeSerializeElement<Message>((string)r[2]);
                    m.InternalId = internalId;
                    m.DbStamp = dbStamp;
                }
                catch
                {
                    throw new Exception(string.Format("Wrong message: {0} {1} {2}", r[0], r[1], r[2]));
                }
                return m;
            });
            messages.Reverse();
            return messages.ToArray();
        }

        public void RemoveMessages(Jid from, Jid to)
        {
            ExecuteNonQuery(new SqlDelete("jabber_archive").Where("jid", GetKey(from, to)));
        }

        private string GetKey(Jid from, Jid to)
        {
            return string.Compare(from.Bare, to.Bare) < 0 ? string.Format("{0}|{1}", from.Bare, to.Bare) : string.Format("{1}|{0}", from.Bare, to.Bare);
        }
    }
}