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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Users;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Core.Mail
{
    public static class MailServiceHelper
    {
        public const string ConnectionStringFormat = "Server={0};Database={1};User ID={2};Password={3};Pooling=True;Character Set=utf8";
        public const string MailServiceDbId = "mailservice";
        public const string DefaultDatabase = "onlyoffice_mailserver";
        public const string DefaultUser = "mail_admin";
        public const string DefaultPassword = "Isadmin123";
        public const string DefaultProtocol = "http";
        public const int DefaultPort = 8081;
        public const string DefaultVersion = "v1";

        private static readonly ICache Cache = AscCache.Default;
        private const string CacheKey = "mailserverinfo";


        private static DbManager GetDb()
        {
            return new DbManager("webstudio");
        }
        
        private static DbManager GetDb(string dbid, string connectionString)
        {
            var connectionSettings = new ConnectionStringSettings(dbid, connectionString, "MySql.Data.MySqlClient");

            if (DbRegistry.IsDatabaseRegistered(connectionSettings.Name))
            {
                DbRegistry.UnRegisterDatabase(connectionSettings.Name);
            }

            DbRegistry.RegisterDatabase(connectionSettings.Name, connectionSettings);

            return new DbManager(connectionSettings.Name);
        }

        private static void DemandPermission()
        {
            if (!CoreContext.Configuration.Standalone)
                throw new NotSupportedException("Method for server edition only.");

            if (!CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID))
                throw new SecurityException();
        }


        public static bool IsMailServerAvailable()
        {
            return _GetMailServerInfo() != null;
        }


        public static MailServerInfo GetMailServerInfo()
        {
            DemandPermission();

            return _GetMailServerInfo();
        }

        private static MailServerInfo _GetMailServerInfo()
        {
            var cachedData = Cache.Get<Tuple<MailServerInfo>>(CacheKey);

            if (cachedData != null)
                return cachedData.Item1;

            using (var dbManager = GetDb())
            {
                var query = new SqlQuery("mail_server_server")
                    .Select("connection_string")
                    .SetMaxResults(1);

                var value = dbManager
                    .ExecuteList(query)
                    .Select(r => Convert.ToString(r[0]))
                    .FirstOrDefault();

                cachedData =
                    new Tuple<MailServerInfo>(string.IsNullOrEmpty(value)
                                                  ? null
                                                  : Newtonsoft.Json.JsonConvert.DeserializeObject<MailServerInfo>(value));
            }

            Cache.Insert(CacheKey, cachedData, DateTime.UtcNow.Add(TimeSpan.FromDays(1)));

            return cachedData.Item1;
        }


        public static string[] GetDataFromExternalDatabase(string dbid, string connectionString, string ip)
        {
            DemandPermission();

            using (var dbManager = GetDb(dbid, connectionString))
            {
                var selectQuery = new SqlQuery("api_keys")
                    .Select("access_token")
                    .Where(Exp.Eq("id", 1))
                    .SetMaxResults(1);

                var token = dbManager
                    .ExecuteList(selectQuery)
                    .Select(r => Convert.ToString(r[0]))
                    .FirstOrDefault();

                IPAddress ipAddress;
                string hostname;

                if (IPAddress.TryParse(ip, out ipAddress))
                {
                    selectQuery = new SqlQuery("greylisting_whitelist")
                        .Select("Comment")
                        .Where(Exp.Eq("Source", "SenderIP:" + ip))
                        .SetMaxResults(1);

                    hostname = dbManager
                        .ExecuteList(selectQuery)
                        .Select(r => Convert.ToString(r[0]))
                        .FirstOrDefault();
                }
                else
                {
                    hostname = ip;
                }

                return new[] { token, hostname };
            }
        }


        public static void UpdateDataFromInternalDatabase(string hostname, MailServerInfo mailServer)
        {
            DemandPermission();

            using (var dbManager = GetDb())
            using (var transaction = dbManager.BeginTransaction())
            {
                var insertQuery = new SqlInsert("mail_mailbox_provider")
                    .InColumnValue("id", 0)
                    .InColumnValue("name", hostname)
                    .Identity(0, 0, true);

                var providerId = dbManager.ExecuteScalar<int>(insertQuery);

                insertQuery = new SqlInsert("mail_mailbox_server")
                    .InColumnValue("id", 0)
                    .InColumnValue("id_provider", providerId)
                    .InColumnValue("type", "smtp")
                    .InColumnValue("hostname", hostname)
                    .InColumnValue("port", 587)
                    .InColumnValue("socket_type", "STARTTLS")
                    .InColumnValue("username", "%EMAILADDRESS%")
                    .InColumnValue("authentication", "")
                    .InColumnValue("is_user_data", false)
                    .Identity(0, 0, true);

                var smtpServerId = dbManager.ExecuteScalar<int>(insertQuery);

                insertQuery = new SqlInsert("mail_mailbox_server")
                    .InColumnValue("id", 0)
                    .InColumnValue("id_provider", providerId)
                    .InColumnValue("type", "imap")
                    .InColumnValue("hostname", hostname)
                    .InColumnValue("port", 143)
                    .InColumnValue("socket_type", "STARTTLS")
                    .InColumnValue("username", "%EMAILADDRESS%")
                    .InColumnValue("authentication", "")
                    .InColumnValue("is_user_data", false)
                    .Identity(0, 0, true);

                var imapServerId = dbManager.ExecuteScalar<int>(insertQuery);


                var selectQuery = new SqlQuery("mail_server_server")
                    .Select("id, smtp_settings_id, imap_settings_id")
                    .SetMaxResults(1);

                var mailServerData = dbManager
                    .ExecuteList(selectQuery)
                    .Select(r => new[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]), Convert.ToInt32(r[2]) })
                    .FirstOrDefault();

                var connectionString = Newtonsoft.Json.JsonConvert.SerializeObject(mailServer);

                insertQuery = new SqlInsert("mail_server_server")
                    .InColumnValue("id", 0)
                    .InColumnValue("mx_record", hostname)
                    .InColumnValue("connection_string", connectionString)
                    .InColumnValue("server_type", 2)
                    .InColumnValue("smtp_settings_id", smtpServerId)
                    .InColumnValue("imap_settings_id", imapServerId);

                dbManager.ExecuteNonQuery(insertQuery);

                if (mailServerData != null)
                {
                    var deleteQuery = new SqlDelete("mail_server_server")
                        .Where(Exp.Eq("id", mailServerData[0]));

                    dbManager.ExecuteNonQuery(deleteQuery);

                    selectQuery = new SqlQuery("mail_mailbox_server")
                        .Select("id_provider")
                        .Where(Exp.Eq("id", mailServerData[1]))
                        .SetMaxResults(1);

                    providerId = dbManager
                        .ExecuteList(selectQuery)
                        .Select(r => Convert.ToInt32(r[0]))
                        .FirstOrDefault();

                    deleteQuery = new SqlDelete("mail_mailbox_provider")
                        .Where(Exp.Eq("id", providerId));

                    dbManager.ExecuteNonQuery(deleteQuery);

                    deleteQuery = new SqlDelete("mail_mailbox_server")
                        .Where(Exp.In("id", new[] { mailServerData[1], mailServerData[2] }));

                    dbManager.ExecuteNonQuery(deleteQuery);

                    selectQuery = new SqlQuery("mail_mailbox")
                        .Select("id")
                        .Where(Exp.Eq("id_smtp_server", mailServerData[1]))
                        .Where(Exp.Eq("id_in_server", mailServerData[2]));

                    var mailboxId = dbManager
                        .ExecuteList(selectQuery)
                        .Select(r => Convert.ToInt32(r[0]))
                        .ToArray();

                    var updateQuery = new SqlUpdate("mail_mailbox")
                        .Set("id_smtp_server", smtpServerId)
                        .Set("id_in_server", imapServerId)
                        .Where(Exp.In("id", mailboxId));

                    dbManager.ExecuteNonQuery(updateQuery);
                }

                transaction.Commit();

                Cache.Remove(CacheKey);
            }
        }
    }

    public class MailServerInfo
    {
        public string DbConnection { get; set; }
        public MailServerApiInfo Api { get; set; }
    }

    public class MailServerApiInfo
    {
        public string Protocol { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Version { get; set; }
        public string Token { get; set; }
    }
}