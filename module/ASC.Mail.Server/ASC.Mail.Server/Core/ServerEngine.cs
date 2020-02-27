/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Data;
using System.Net;
using System.Net.Mail;
using ASC.Common.Data;
using ASC.Mail.Server.Core.Dao;
using ASC.Mail.Server.Core.Entities;
using ASC.Mail.Server.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ASC.Mail.Server.Core
{
    public class ServerEngine
    {
        private readonly string _csName;
        private readonly ServerApi _serverApi;

        public ServerEngine(int serverId, string connectionString)
        {
            var serverDbConnection = string.Format("postfixserver{0}", serverId);

            var connectionStringParser = new PostfixConnectionStringParser(connectionString);

            var cs = new ConnectionStringSettings(serverDbConnection,
                                                  connectionStringParser.PostfixAdminDbConnectionString, "MySql.Data.MySqlClient");

            if (!DbRegistry.IsDatabaseRegistered(cs.Name))
            {
                DbRegistry.RegisterDatabase(cs.Name, cs);
            }

            _csName = cs.Name;

            var json = JObject.Parse(connectionString);

            if (json["Api"] != null)
            {
                _serverApi = new ServerApi
                {
                    server_ip = json["Api"]["Server"].ToString(),
                    port = Convert.ToInt32(json["Api"]["Port"].ToString()),
                    protocol = json["Api"]["Protocol"].ToString(),
                    version = json["Api"]["Version"].ToString(),
                    token = json["Api"]["Token"].ToString()
                };
            }
        }

        public int SaveDomain(Domain domain)
        {
            using (var db = new DbManager(_csName))
            {
                var domainDao = new DomainDao(db);

                return domainDao.Save(domain);
            }
        }

        public int SaveDkim(Dkim dkim)
        {
            using (var db = new DbManager(_csName))
            {
                var dkimDao = new DkimDao(db);

                return dkimDao.Save(dkim);
            }
        }

        public int SaveAlias(Alias alias)
        {
            using (var db = new DbManager(_csName))
            {
                var aliasDao = new AliasDao(db);

                return aliasDao.Save(alias);
            }
        }

        public int RemoveAlias(string alias)
        {
            using (var db = new DbManager(_csName))
            {
                var aliasDao = new AliasDao(db);

                return aliasDao.Remove(alias);
            }
        }

        public void ChangePassword(string username, string newPassword)
        {
            using (var db = new DbManager(_csName))
            {
                var mailboxDao = new MailboxDao(db);

                var res = mailboxDao.ChangePassword(username, newPassword);

                if (res < 1)
                    throw new Exception(string.Format("Server mailbox \"{0}\" not found", username));
            }
        }

        public void SaveMailbox(Mailbox mailbox, Alias address, bool deliver = true)
        {
            using (var db = new DbManager(_csName))
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var mailboxDao = new MailboxDao(db);

                    mailboxDao.Save(mailbox, deliver);

                    var aliasDao = new AliasDao(db);

                    aliasDao.Save(address);

                    tx.Commit();
                }
            }
        }

        public void RemoveMailbox(string address)
        {
            var mailAddress = new MailAddress(address);

            ClearMailboxStorageSpace(mailAddress.User, mailAddress.Host);

            using (var db = new DbManager(_csName))
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var mailboxDao = new MailboxDao(db);

                    mailboxDao.Remove(address);

                    var aliasDao = new AliasDao(db);

                    aliasDao.Remove(address);

                    tx.Commit();
                }
            }
        }

        public void RemoveDomain(string domain)
        {
            ClearDomainStorageSpace(domain);

            using (var db = new DbManager(_csName))
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var aliasDao = new AliasDao(db);

                    aliasDao.RemoveByDomain(domain);

                    var mailboxDao = new MailboxDao(db);

                    mailboxDao.RemoveByDomain(domain);

                    var domainDao = new DomainDao(db);

                    domainDao.Remove(domain);

                    var dkimDao = new DkimDao(db);

                    dkimDao.Remove(domain);

                    tx.Commit();
                }
            }
        }

        public string GetVersion()
        {
            if (_serverApi == null) 
                return null;

            var client = GetApiClient();
            var request = GetApiRequest("version", Method.GET);

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                throw new Exception("MailServer->GetVersion() Response code = " + response.StatusCode, response.ErrorException);

            var json = JObject.Parse(response.Content);

            if (json == null) return null;

            var globalVars = json["global_vars"];

            if (globalVars == null) return null;

            var version = globalVars["value"];

            return version == null ? null : version.ToString();
        }

        private void ClearDomainStorageSpace(string domain)
        {
            if (_serverApi == null) return;

            var client = GetApiClient();
            var request = GetApiRequest("domains/{domain_name}", Method.DELETE);
            request.AddUrlSegment("domain_name", domain);
            // execute the request
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                throw new Exception("MailServer->ClearDomainStorageSpace(). Response code = " + response.StatusCode, response.ErrorException);
        }

        private void ClearMailboxStorageSpace(string mailboxLocalpart, string domainName)
        {
            if (_serverApi == null) return; // Skip if api not presented

            var client = GetApiClient();
            var request = GetApiRequest("domains/{domain_name}/mailboxes/{mailbox_localpart}", Method.DELETE);
            request.AddUrlSegment("domain_name", domainName);
            request.AddUrlSegment("mailbox_localpart", mailboxLocalpart);
            // execute the request
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                throw new Exception("MailServer->ClearMailboxStorageSpace(). Response code = " + response.StatusCode, response.ErrorException);

        }

        private RestClient GetApiClient()
        {
            return _serverApi == null ? null : new RestClient(string.Format("{0}://{1}:{2}/", _serverApi.protocol, _serverApi.server_ip, _serverApi.port));
        }

        private RestRequest GetApiRequest(string apiUrl, Method method)
        {
            return _serverApi == null ? null : new RestRequest(string.Format("/api/{0}/{1}?auth_token={2}", _serverApi.version, apiUrl, _serverApi.token), method);
        }
    }
}
