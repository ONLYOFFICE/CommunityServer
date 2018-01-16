/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Configuration;
using System.Data;
using System.Linq;

using ASC.Core.Notify.Signalr;
using ASC.Common.Data.Sql;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Server.Storage.Interface;

namespace ASC.Xmpp.Server.Storage
{
    public class DbOfflineStore : DbStoreBase, IOfflineStore
    {
        private Dictionary<string, int> countCache = new Dictionary<string, int>();
        private static readonly SignalrServiceClient signalrServiceClient = new SignalrServiceClient("counters");
        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_offmessage", true)
                .AddColumn(new SqlCreate.Column("id", DbType.Int32).NotNull(true).Autoincrement(true).PrimaryKey(true))
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("message", DbType.String, MESSAGE_COLUMN_LEN)
                .AddIndex("jabber_offmessage_jid", "jid");

            var t2 = new SqlCreate.Table("jabber_offpresence", true)
                .AddColumn(new SqlCreate.Column("id", DbType.Int32).NotNull(true).Autoincrement(true).PrimaryKey(true))
                .AddColumn("jid_to", DbType.String, 255, true)
                .AddColumn("jid_from", DbType.String, 255, false)
                .AddColumn(new SqlCreate.Column("type", DbType.Int32).NotNull(true).Default(0))
                .AddIndex("jabber_offpresence_to", "jid_to");

            var t3 = new SqlCreate.Table("jabber_offactivity", true)
                .AddColumn(new SqlCreate.Column("jid", DbType.String, 255).NotNull(true).PrimaryKey(true))
                .AddColumn("logout", DbType.DateTime)
                .AddColumn("status", DbType.String, 255);

            return new[] { t1, t2, t3 };
        }

        #region Offline Messages

        public List<Message> GetOfflineMessages(Jid jid)
        {
            return ExecuteList(new SqlQuery("jabber_offmessage").Select("message").Where("jid", GetBareJid(jid)).OrderBy("id", true))
                .ConvertAll(r => ElementSerializer.DeSerializeElement<Message>((string)r[0]));
        }

        public int GetOfflineMessagesCount(Jid jid)
        {
            if (jid == null) return 0;
            var key = GetBareJid(jid);

            lock (countCache)
            {
                if (countCache.ContainsKey(key)) return countCache[key];
            }
            var count = ExecuteScalar<int>(new SqlQuery("jabber_offmessage").SelectCount().Where("jid", GetBareJid(jid)));
            lock (countCache)
            {
                countCache[key] = count;
            }
            return count;
        }

        public void SaveOfflineMessages(params Message[] messages)
        {
            if (messages == null) throw new ArgumentNullException("messages");
            if (messages.Length == 0) return;

            var batch = new List<ISqlInstruction>(messages.Length);
            var jids = new List<Jid>();
            foreach (var m in messages)
            {
                if (m == null || !m.HasTo || string.IsNullOrEmpty(m.To.Bare))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(m.Body) &&
                    string.IsNullOrEmpty(m.Subject) &&
                    string.IsNullOrEmpty(m.Thread) &&
                    m.Html == null)
                {
                    continue;
                }

                if (!jids.Contains(m.To))
                {
                    jids.Add(m.To);
                }

                if (m.XDelay == null) m.XDelay = new Delay();
                if (m.XDelay.Stamp == default(DateTime)) m.XDelay.Stamp = DateTime.UtcNow;

                batch.Add(
                    new SqlInsert("jabber_offmessage")
                    .InColumnValue("jid", GetBareJid(m.To))
                    .InColumnValue("message", ElementSerializer.SerializeElement(m)));

                lock (countCache) countCache.Remove(GetBareJid(m.To));
            }
            ExecuteBatch(batch);

            if (SignalrServiceClient.EnableSignalr && jids.Count > 0)
            {
                Dictionary<string, int> unreadCounts = new Dictionary<string, int>();
                string domain = null;
                foreach (var jid in jids)
                {
                    int count = GetOfflineMessagesCount(jid);
                    if (count > 0)
                    {
                        domain = jid.Server;
                        unreadCounts.Add(jid.User, count);
                    }
                }
                if (unreadCounts.Count > 0)
                {
                    signalrServiceClient.SendUnreadCounts(unreadCounts, domain);
                }
            }
        }

        public void RemoveAllOfflineMessages(Jid jid)
        {
            ExecuteNonQuery(new SqlDelete("jabber_offmessage").Where("jid", GetBareJid(jid)));
            lock (countCache) countCache.Remove(GetBareJid(jid));
        }

        #endregion


        #region Offline Presences

        public List<Presence> GetOfflinePresences(Jid jid)
        {
            return ExecuteList(new SqlQuery("jabber_offpresence").Select("jid_from", "type").Where("jid_to", GetBareJid(jid)))
                .ConvertAll(r =>
                {
                    return new Presence()
                    {
                        To = jid.BareJid,
                        From = !string.IsNullOrEmpty((string)r[0]) ? new Jid((string)r[0]) : null,
                        Type = (PresenceType)Convert.ToInt32(r[1]),
                    };
                });
        }

        public void SaveOfflinePresence(Presence presence)
        {
            if (presence == null) throw new ArgumentNullException("pr");
            if (!presence.HasTo) return;

            ExecuteNonQuery(
                new SqlInsert("jabber_offpresence")
                .InColumnValue("jid_to", GetBareJid(presence.To))
                .InColumnValue("jid_from", presence.HasFrom ? GetBareJid(presence.From) : null)
                .InColumnValue("type", (Int32)presence.Type));
        }

        public void RemoveAllOfflinePresences(Jid jid)
        {
            ExecuteNonQuery(new SqlDelete("jabber_offpresence").Where("jid_to", GetBareJid(jid)));
        }

        #endregion


        #region Last Activity

        public void SaveLastActivity(Jid jid, LastActivity lastActivity)
        {
            if (lastActivity == null) throw new ArgumentNullException("lastActivity");

            ExecuteNonQuery(new SqlInsert("jabber_offactivity", true)
                .InColumnValue("jid", GetBareJid(jid))
                .InColumnValue("logout", lastActivity.LogoutDateTime)
                .InColumnValue("status", lastActivity.Status));
        }

        public LastActivity GetLastActivity(Jid jid)
        {
            return ExecuteList(new SqlQuery("jabber_offactivity").Select("logout", "status").Where("jid", GetBareJid(jid)))
                .ConvertAll(r => new LastActivity((DateTime)r[0], r[1] as string))
                .SingleOrDefault();
        }

        #endregion


        private string GetBareJid(Jid jid)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            return jid.Bare.ToLowerInvariant();
        }
    }
}