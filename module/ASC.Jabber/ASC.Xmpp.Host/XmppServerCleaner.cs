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
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Core;
using ASC.Xmpp.Core.utils;

namespace ASC.Xmpp.Host
{
    public class XmppServerCleaner : IServiceController
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Xmpp.Host");
        private readonly ManualResetEvent stop = new ManualResetEvent(false);
        private Thread worker;


        public void Start()
        {
            worker = new Thread(Clear)
            {
                Name = "Xmpp Cleaner",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            worker.Start();
        }

        public void Stop()
        {
            stop.Set();
            worker.Join(TimeSpan.FromSeconds(5));
        }


        private void Clear()
        {
            while (true)
            {
                try
                {
                    log.InfoFormat("Start cleaner interation.");

                    using (var db = new DbManager("default"))
                    {
                        var t = new SqlCreate.Table("jabber_clear", true)
                            .AddColumn("lastdate", DbType.DateTime);
                        db.ExecuteNonQuery(t);

                        var mindate = db.ExecuteScalar<DateTime>("select lastdate from jabber_clear limit 1");

                        var tenants = new List<Tuple<int, string>>();
                        var maxdate = DateTime.UtcNow.AddDays(-365);

                        var sql = @"select
t.id, t.alias
from tenants_tenants t, webstudio_uservisit v
where t.id = v.tenantid and t.creationdatetime < ?
group by 1,2
having max(v.visitdate) between ? and ?";

                        using (var cmd = CreateCommand(db, sql, maxdate, mindate, maxdate))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tenants.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1)));
                            }
                        }
                        log.DebugFormat("Find {0} tenants for clear jabber messages", tenants.Count);

                        foreach (var tid in tenants)
                        {
                            // remove all service messages in inactive (no visits at last year) portals

                            var domain = CoreContext.Configuration.BaseDomain;
                            var replace = ConfigurationManager.AppSettings["jabber.replace-domain"];
                            if (!string.IsNullOrEmpty(replace))
                            {
                                var arr = replace.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                                var from = arr[0];
                                var to = arr[1];
                                domain = domain.Replace(to, from); // revert replace
                            }
                            domain = (tid.Item2.EndsWith("_deleted") ? tid.Item2.Substring(0, tid.Item2.Length - 8) : tid.Item2) +
                                "." + domain;                            

                            if (stop.WaitOne(TimeSpan.Zero))
                            {
                                return;
                            }
                            RemoveFromArchive(db, domain, maxdate);

                            var users = new List<string>();
                            using (var cmd = CreateCommand(db, "select username from core_user where tenant = ? and username <= ?", tid.Item1, tid.Item2))
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    users.Add(reader.GetString(0));
                                }
                            }

                            foreach (var user in users)
                            {
                                var username = user.ToLowerInvariant().Trim();

                                if (stop.WaitOne(TimeSpan.Zero))
                                {
                                    return;
                                }
                                var jid = string.Format("{0}@{1}|{1}", username, domain);
                                RemoveFromArchive(db, jid, maxdate);
                            }
                        }
                        db.ExecuteNonQuery("delete from jabber_clear;");
                        db.ExecuteNonQuery("insert into jabber_clear values (?)", maxdate);

                        // remove offline messages
                        var id = 0;
                        using (var cmd = CreateCommand(db, "select id, message from jabber_offmessage order by 1"))
                        using (var reader = cmd.ExecuteReader())
                        {
                            var less = false;
                            while (reader.Read())
                            {
                                var message = reader.GetString(1);
                                var m = Regex.Match(message, "<x xmlns=\"jabber:x:delay\" stamp=\"(.+)\"");
                                if (m.Success)
                                {
                                    var date = Time.Date(m.Groups[1].Value);
                                    if (date != DateTime.MinValue && date <= maxdate)
                                    {
                                        less = true;
                                    }
                                    else
                                    {
                                        if (less)
                                        {
                                            id = reader.GetInt32(0);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        if (0 < id)
                        {
                            using (var cmd = CreateCommand(db, "delete from jabber_offmessage where id < ?", id))
                            {
                                var affected = cmd.ExecuteNonQuery();
                                log.DebugFormat("Remove {0} messages from jabber_offmessage", affected);
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    // ignore
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
                if (stop.WaitOne(TimeSpan.FromDays(1)))
                {
                    break;
                }
            }
        }


        private IDbCommand CreateCommand(DbManager db, string sql, params object[] parameters)
        {
            var cmd = db.Connection.CreateCommand(sql, parameters);
            cmd.CommandTimeout = 60 * 60;
            return cmd;
        }

        private void RemoveFromArchive(DbManager db, string jid, DateTime lastdate)
        {
            jid = jid.Trim().TrimEnd('.').Replace("_", "\\_").Replace("%", "\\%");
            if (!jid.Contains("|"))
            {
                jid += "|%";
            }
            using (var del = CreateCommand(db, "delete from jabber_archive where jid like ? and stamp < ?", jid, lastdate))
            {
                var affected = del.ExecuteNonQuery();
                if (0 < affected)
                {
                    log.DebugFormat("Remove from jabber_archive {0} rows with jid {1}", affected, jid);
                }
            }
        }
    }
}
