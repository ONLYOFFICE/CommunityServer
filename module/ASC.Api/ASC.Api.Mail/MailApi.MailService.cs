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
        public object ConnectMailServer(string ip, string sqlip, string user, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(ip))
                    throw new ArgumentException("ip");

                if (string.IsNullOrEmpty(sqlip))
                    sqlip = ip;

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                if (!PingHost(ip, 8081))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, ip, 8081));

                if (!PingHost(sqlip, 3306))
                    throw new Exception(string.Format(Resource.MailServicePingingErrorMsg, sqlip, 3306));

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, sqlip, MailServiceHelper.DefaultDatabase, user, password);

                var data = GetAuthData(connectionString, ip);

                return new
                {
                    status = "success",
                    message = Resource.MailServiceConnectSuccessMsg,
                    ip,
                    sqlip,
                    user,
                    password,
                    token = data[0],
                    host = data[1]
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
        public object SaveMailServerInfo(string ip, string sqlip, string user, string password, string token, string host)
        {
            try
            {
                if (string.IsNullOrEmpty(ip))
                    throw new ArgumentException("ip");

                if (string.IsNullOrEmpty(sqlip))
                    sqlip = ip;

                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("user");

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password");

                if (string.IsNullOrEmpty(token))
                    throw new ArgumentException("token");

                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("host");

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, sqlip, MailServiceHelper.DefaultDatabase, user, password);

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
        public object ConnectAndSaveMailServerInfo(string host, string user, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("host");

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

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, ip, MailServiceHelper.DefaultDatabase, user, password);

                var data = GetAuthData(connectionString, ip);

                Save(connectionString, ip, data[0], data[1]);

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
        public object ConnectAndSavePartitionalMailServerInfo(string mailHost, string mysqlHost, string mysqlUser, string mysqlPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(mailHost))
                    throw new ArgumentException("mailHost");

                if (string.IsNullOrEmpty(mysqlHost))
                    throw new ArgumentException("mysqlHost");

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

                var connectionString = string.Format(MailServiceHelper.ConnectionStringFormat, mysqlIp, MailServiceHelper.DefaultDatabase, mysqlUser, mysqlPassword);

                var data = GetAuthData(connectionString, mailIp);

                Save(connectionString, mailIp, data[0], data[1]);

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

        private static string[] GetAuthData(string connectionString, string ip)
        {
            var data = MailServiceHelper.GetDataFromExternalDatabase(MailServiceHelper.MailServiceDbId, connectionString, ip);

            if (data == null || data.Length < 2 || string.IsNullOrEmpty(data[0]) || string.IsNullOrEmpty(data[1]))
                throw new Exception(Resource.MailServiceCouldNotGetErrorMsg);

            return data;
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

            MailServiceHelper.UpdateDataFromInternalDatabase(host, mailServer);

            MessageService.Send(HttpContext.Current.Request, MessageAction.MailServiceSettingsUpdated);
        }
    }
}
