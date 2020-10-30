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
using System.Linq;
using System.Net.Sockets;
using System.Web;
using ASC.Api.Attributes;
using ASC.Common.Utils;
using ASC.MessagingSystem;
using ASC.Web.Core.Mail;
using Resources;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <visible>false</visible>
        [Read("mailservice/get")]
        public object GetMailServerInfo()
        {
            return MailServiceHelper.GetMailServerInfo();
        }

        /// <visible>false</visible>
        [Create("mailservice/connect")]
        public object ConnectMailServer(string ip, string sqlip, string database, string user, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(ip))
                    throw new ArgumentException("ip");

                if (string.IsNullOrEmpty(sqlip))
                    sqlip = ip;

                if (string.IsNullOrEmpty(database))
                    throw new ArgumentException("database");

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                if (!PingHost(ip, 8081))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, ip, 8081));

                if (!PingHost(sqlip, 3306))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, sqlip, 3306));

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, sqlip, database, user, password);

                var token = GetToken(connectionString);

                var hostname = GetHostname(connectionString, ip);

                return new
                {
                    status = "success",
                    message = Resource.MailServiceConnectSuccessMsg,
                    ip,
                    sqlip,
                    database,
                    user,
                    password,
                    token,
                    host = hostname
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);

                return new
                {
                    status = "error",
                    message = exception.Message.HtmlEncode()
                };
            }
        }

        /// <visible>false</visible>
        [Create("mailservice/save")]
        public object SaveMailServerInfo(string ip, string sqlip, string database, string user, string password, string token, string host)
        {
            try
            {
                if (string.IsNullOrEmpty(ip))
                    throw new ArgumentException("ip");

                if (string.IsNullOrEmpty(sqlip))
                    sqlip = ip;

                if (string.IsNullOrEmpty(database))
                    throw new ArgumentException("database");

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                if (string.IsNullOrEmpty(token))
                    throw new ArgumentException("token");

                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("host");

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, sqlip, database, user, password);

                Save(connectionString, ip, token, host);

                return new
                {
                    status = "success",
                    message = Resource.MailServiceSaveSuccessMsg
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);

                return new
                {
                    status = "error",
                    message = exception.Message.HtmlEncode()
                };
            }
        }

        /// <visible>false</visible>
        [Create("mailservice/connectandsave")]
        public object ConnectAndSaveMailServerInfo(string host, string database, string user, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("host");

                if (string.IsNullOrEmpty(database))
                    database = MailServiceHelper.DefaultDatabase;

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                var ip = GetHostIp(host);

                if (string.IsNullOrEmpty(ip))
                    throw new Exception("could not get host ip");

                if (!PingHost(ip, 8081))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, ip, 8081));

                if (!PingHost(ip, 3306))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, ip, 3306));

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, ip, database, user, password);

                var token = GetToken(connectionString);

                var hostname = GetHostname(connectionString, ip);

                Save(connectionString, ip, token, hostname);

                return new
                {
                    status = "success",
                    message = Resource.MailServiceSaveSuccessMsg
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);

                return new
                {
                    status = "error",
                    message = exception.Message.HtmlEncode()
                };
            }
        }

        /// <visible>false</visible>
        [Create("mailservice/connectandsavepartitional")]
        public object ConnectAndSavePartitionalMailServerInfo(string mailHost, string mysqlHost, string mysqlDatabase, string mysqlUser, string mysqlPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(mailHost))
                    throw new ArgumentException("mailHost");

                if (string.IsNullOrEmpty(mysqlHost))
                    throw new ArgumentException("mysqlHost");

                if (string.IsNullOrEmpty(mysqlDatabase))
                    mysqlDatabase = MailServiceHelper.DefaultDatabase;

                if (string.IsNullOrEmpty(mysqlUser))
                    throw new ArgumentException("mysqlUser");

                if (string.IsNullOrEmpty(mysqlPassword))
                    throw new ArgumentException("mysqlPassword");

                var mailIp = GetHostIp(mailHost);

                if (string.IsNullOrEmpty(mailIp))
                    throw new Exception("could not get mailHost ip");

                var mysqlIp = GetHostIp(mysqlHost);

                if (string.IsNullOrEmpty(mysqlIp))
                    throw new Exception("could not get mysqlHost ip");

                if (!PingHost(mailIp, 8081))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, mailIp, 8081));

                if (!PingHost(mysqlIp, 3306))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, mysqlIp, 3306));

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, mysqlIp, mysqlDatabase, mysqlUser, mysqlPassword);

                var token = GetToken(connectionString);

                var hostname = GetHostname(connectionString, mailIp);

                Save(connectionString, mailIp, token, hostname);

                return new
                {
                    status = "success",
                    message = Resource.MailServiceSaveSuccessMsg
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);

                return new
                {
                    status = "error",
                    message = exception.Message.HtmlEncode()
                };
            }
        }

        private static bool PingHost(string host, int port)
        {
            try
            {
                using (var client = new TcpClient(host, port))
                {
                    return client.Connected;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetHostIp(string host)
        {
            try
            {
                var ip = new DnsLookup().GetDomainIPs(host).FirstOrDefault();
                return ip == null ? null : ip.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetToken(string connectionString)
        {
            var token = MailServiceHelper.GetTokenFromExternalDatabase(MailServiceHelper.MailServiceDbId, connectionString);

            if (string.IsNullOrEmpty(token))
                throw new Exception(Resource.MailServiceCouldNotGetTokenErrorMsg);

            return token;
        }

        private static string GetHostname(string connectionString, string ip)
        {
            var hostname = MailServiceHelper.GetHostnameFromExternalDatabase(MailServiceHelper.MailServiceDbId, connectionString, ip);

            if (string.IsNullOrEmpty(hostname))
                throw new Exception(Resource.MailServiceCouldNotGetHostnameErrorMsg);

            return hostname;
        }

        private static void Save(string connectionString, string ip, string token, string host)
        {
            var mailServer = new MailServerInfo
            {
                DbConnection = connectionString,
                Api = new MailServerApiInfo
                {
                    Port = MailServiceHelper.DefaultPort,
                    Protocol = MailServiceHelper.DefaultProtocol,
                    Server = ip,
                    Token = token,
                    Version = MailServiceHelper.DefaultVersion
                }
            };

            MailServiceHelper.UpdateInternalDatabase(host, mailServer);

            MessageService.Send(HttpContext.Current.Request, MessageAction.MailServiceSettingsUpdated);
        }
    }
}
