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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.UI;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Data.Storage;
using ASC.Web.Studio.Utility;
using AjaxPro;
using log4net;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.MailService, Location)]
    [AjaxNamespace("MailService")]
    public partial class MailService : UserControl
    {
        private static readonly ICache cache = AscCache.Default;

        public const string Location = "~/UserControls/Management/MailService/MailService.ascx";

        protected const string ConnectionStringFormat = "Server={0};Database={1};User ID={2};Password={3};Pooling=True;Character Set=utf8";
        protected const string MailServiceDbId = "mailservice";
        protected const string DefaultDatabase = "onlyoffice_mailserver";
        protected const string DefaultUser = "mail_admin";
        protected const string DefaultPassword = "Isadmin123";
        protected const string DefaultProtocol = "http";
        protected const int DefaultPort = 8081;
        protected const string DefaultVersion = "v1";

        protected string ServerIp;
        protected string User;
        protected string Password;


        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/usercontrols/management/MailService/js/mailservice.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "mailservice_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("usercontrols/management/mailservice/css/mailservice.css") + "\">", false);

            var mailServerInfo = MailServiceHelper.GetMailServerInfo();

            if (mailServerInfo == null)
            {
                ServerIp = string.Empty;
                User = DefaultUser;
                Password = DefaultPassword;
            }
            else
            {
                ServerIp = mailServerInfo.Api.Server;

                var connectionParts = mailServerInfo.DbConnection.Split(';');

                foreach (var connectionPart in connectionParts)
                {
                    if (connectionPart.StartsWith("User ID="))
                        User = connectionPart.Replace("User ID=", "");

                    if(connectionPart.StartsWith("Password="))
                        Password = connectionPart.Replace("Password=", "");
                }
            }
        }


        [AjaxMethod]
        public object Connect(string ip, string user, string password)
        {
            try
            {
                IPAddress ipAddress;

                if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out ipAddress))
                    throw new ArgumentException("ip");

                if (!PingHost(ip, 3306))
                    throw new Exception(string.Format(Resources.Resource.MailServicePingErrorMsg, ip));

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                var connectionString = string.Format(ConnectionStringFormat, ip, DefaultDatabase, user, password);

                var data = MailServiceHelper.GetDataFromExternalDatabase(MailServiceDbId, connectionString, ip);

                if(data == null || data.Length < 2 || string.IsNullOrEmpty(data[0]) || string.IsNullOrEmpty(data[1]))
                    throw new Exception(Resources.Resource.MailServiceCouldNotGetErrorMsg);

                return new
                {
                    Status = "success",
                    Message = Resources.Resource.MailServiceConnectSuccessMsg,
                    Ip = ip,
                    User = user,
                    Password = password,
                    Token = data[0],
                    Host = data[1]
                };
            }
            catch (Exception exception)
            {
                LogManager.GetLogger("ASC.Web.MailService").Error(exception);
                
                return new
                {
                    Status = "error",
                    Message = exception.Message.HtmlEncode()
                };
            }
        }

        [AjaxMethod]
        public object Save(string ip, string user, string password, string token, string host)
        {
            try
            {
                IPAddress ipAddress;

                if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out ipAddress))
                    throw new ArgumentException("ip");

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                if (string.IsNullOrEmpty(token))
                    throw new ArgumentException("token");

                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("host");

                var connectionString = string.Format(ConnectionStringFormat, ip, DefaultDatabase, user, password);

                var mailServer = new MailServerInfo
                {
                    DbConnection = connectionString,
                    Api = new MailServerApiInfo
                    {
                        Port = DefaultPort,
                        Protocol = DefaultProtocol,
                        Server = ip,
                        Token = token,
                        Version = DefaultVersion
                    }
                };

                MailServiceHelper.UpdateDataFromInternalDatabase(host, mailServer);

                ClearCache();

                return new
                {
                    Status = "success",
                    Message = Resources.Resource.MailServiceSaveSuccessMsg
                };
            }
            catch (Exception exception)
            {
                LogManager.GetLogger("ASC.Web.MailService").Error(exception);
                
                return new
                {
                    Status = "error",
                    Message = exception.Message.HtmlEncode()
                };
            }
        }


        private static bool PingHost(string host, int port)
        {
            try
            {
                var client = new TcpClient(host, port);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetCacheKey()
        {
            return string.Format("{0}:{1}", TenantProvider.CurrentTenantID, "mailserverinfo");
        }

        private static void ClearCache()
        {
            cache.Remove(GetCacheKey());
        }

        public class MailServiceHelper
        {
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

            private static DbManager GetDb()
            {
                return new DbManager("webstudio");
            }

            public static MailServerInfo GetMailServerInfo()
            {
                var mailServerInfo = cache.Get<MailServerInfo>(GetCacheKey());

                if (mailServerInfo != null)
                    return mailServerInfo;

                using (var dbManager = GetDb())
                {
                    var query = new SqlQuery("mail_server_server")
                        .Select("connection_string")
                        .SetMaxResults(1);

                    var value = dbManager
                        .ExecuteList(query)
                        .Select(r => Convert.ToString(r[0]))
                        .FirstOrDefault();

                    mailServerInfo = string.IsNullOrEmpty(value)
                               ? null
                               : Newtonsoft.Json.JsonConvert.DeserializeObject<MailServerInfo>(value);
                }

                if (mailServerInfo != null)
                    cache.Insert(GetCacheKey(), mailServerInfo, DateTime.UtcNow.Add(TimeSpan.FromHours(1)));

                return mailServerInfo;
            }

            public static string[] GetDataFromExternalDatabase(string dbid, string connectionString, string ip)
            {
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

                    selectQuery = new SqlQuery("greylisting_whitelist")
                        .Select("Comment")
                        .Where(Exp.Eq("Source", "SenderIP:" + ip))
                        .SetMaxResults(1);

                    var hostname = dbManager
                        .ExecuteList(selectQuery)
                        .Select(r => Convert.ToString(r[0]))
                        .FirstOrDefault();

                    return new[] { token, hostname };
                }
            }

            public static void UpdateDataFromInternalDatabase(string hostname, MailServerInfo mailServer)
            {
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
}