/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Data;
using System.Linq;
using System.Net;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Utils;
using ASC.Mail.Server.PostfixAdministration.DbSchema;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ASC.Mail.Server.PostfixAdministration
{
    public class PostfixServer : ServerModel
    {
        private readonly PostfixAdminDbManager _dbManager;

        private readonly ServerApi _serverApi;

        class ServerApi
        {
            public string server_ip;
            public string protocol;
            public int port;
            public string version;
            public string token;
        }

        public PostfixServer(ServerSetup setup)
            : base(setup)
        {
            _dbManager = new PostfixAdminDbManager(setup.ServerId, setup.ConnectionString);

            var json = JObject.Parse(setup.ConnectionString);

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

        #region .Domains

        protected override WebDomainBase _CreateWebDomain(string name)
        {
            var domain = new WebDomainBase(name) {DateCreated = DateTime.UtcNow};
            
            try
            {
                var domainDescription = String.Format("Domain created in UtcTime: {0}, for tenant: {1}",
                                                       domain.DateCreated, Tenant);
                var insertDomainQuery = new SqlInsert(DomainTable.name)
                    .InColumnValue(DomainTable.Columns.domain, domain.Name)
                    .InColumnValue(DomainTable.Columns.description, domainDescription)
                    .InColumnValue(DomainTable.Columns.created, domain.DateCreated)
                    .InColumnValue(DomainTable.Columns.modified, domain.DateCreated)
                    .InColumnValue(DomainTable.Columns.active, true);

                using (var db = _dbManager.GetAdminDb())
                {
                    db.ExecuteNonQuery(insertDomainQuery);
                }

                return domain;
            }
            catch (MySqlException ex)
            {
                if (ex.Message.StartsWith("Duplicate entry"))
                    throw new ArgumentException("Already added");

                throw;
            }
        }

        protected override List<WebDomainBase> _GetWebDomains(ICollection<string> domainNames)
        {
            if (null == domainNames)
                throw new ArgumentNullException("domainNames", "in _GetWebDomains method");

            var namesArg = domainNames.Cast<object>().ToArray();

            var postfixDomainsQuery = new SqlQuery(DomainTable.name)
                .Select(DomainTable.Columns.domain, DomainTable.Columns.created)
                .Where(DomainTable.Columns.active, 1)
                .Where(Exp.In(DomainTable.Columns.domain, namesArg))
                .OrderBy(DomainTable.Columns.created, true);

            List<object[]> rows;
            using (var db = _dbManager.GetAdminDb())
                rows = db.ExecuteList(postfixDomainsQuery);

            var domains = rows
                .Select(d => new WebDomainBase(d[0].ToString())
                    {
                        DateCreated = Convert.ToDateTime(d[1])
                    })
                .ToList();

            return domains;
        }

        protected override WebDomainBase _GetWebDomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException(domainName);

            var postfixDomainsQuery = new SqlQuery(DomainTable.name)
                .Select(DomainTable.Columns.domain, DomainTable.Columns.created)
                .Where(DomainTable.Columns.active, 1)
                .Where(DomainTable.Columns.domain, domainName);

            List<object[]> rows;
            using (var db = _dbManager.GetAdminDb())
                rows = db.ExecuteList(postfixDomainsQuery);

            var domain = rows
                .Select(d => new WebDomainBase(d[0].ToString())
                    {
                        DateCreated = Convert.ToDateTime(d[1])
                    })
                .FirstOrDefault();

            return domain;
        }


        protected override void _DeleteWebDomain(WebDomainBase webDomain)
        {
            var deleteMailboxQuery = new SqlDelete(MailboxTable.name).Where(MailboxTable.Columns.domain, webDomain.Name);
            var deleteMailboxAliases = new SqlDelete(AliasTable.name).Where(AliasTable.Columns.domain, webDomain.Name);
            var deleteDomainQuery = new SqlDelete(DomainTable.name).Where(DomainTable.Columns.domain, webDomain.Name);
            using (var db = _dbManager.GetAdminDb())
            {
                using (var t = db.BeginTransaction())
                {
                    ClearDomainStorageSpace(webDomain.Name);

                    db.ExecuteNonQuery(deleteMailboxAliases);
                    db.ExecuteNonQuery(deleteMailboxQuery);
                    db.ExecuteNonQuery(deleteDomainQuery);

                    t.Commit();
                }
            }
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
                throw new Exception("Can't delete all stored domain mailbox folders on server. Response code = " + response.StatusCode, response.ErrorException);
        }

        private string GenerateMaildirPath(string domain, string localpart, DateTime creationdate)
        {
            var maildir = domain + "/";

            if (localpart.Length >= 3)
            {
                maildir += string.Format("{0}/{1}/{2}/", localpart[0], localpart[1], localpart[2]);
            }
            else if (localpart.Length == 2)
            {
                maildir += string.Format("{0}/{1}/{2}/", localpart[0], localpart[1], localpart[1]);
            }
            else
            {
                maildir += string.Format("{0}/{1}/{2}/", localpart[0], localpart[0], localpart[0]);
            }

            maildir += string.Format("{0}-{1}/", localpart, creationdate.ToString("yyyy.MM.dd.HH.mm.ss"));

            return maildir.ToLower();
        }

        #endregion

        #region .Mailboxes

        protected override MailboxBase _CreateMailbox(string login, string password, string localpart, string domain, bool enableImap = true, bool enablePop = true)
        {
            var creationDate = DateTime.UtcNow;

            var maildir = GenerateMaildirPath(domain, localpart, creationDate);

            var insertMailboxQuery = new SqlInsert(MailboxTable.name)
                                        .InColumnValue(MailboxTable.Columns.username, login)
                                        .InColumnValue(MailboxTable.Columns.name, localpart)
                                        .InColumnValue(MailboxTable.Columns.password, PostfixPasswordEncryptor.EncryptString(HashType.Md5, password))
                                        .InColumnValue(MailboxTable.Columns.maildir, maildir)
                                        .InColumnValue(MailboxTable.Columns.localPart, localpart)
                                        .InColumnValue(MailboxTable.Columns.domain, domain)
                                        .InColumnValue(MailboxTable.Columns.created, creationDate)
                                        .InColumnValue(MailboxTable.Columns.modified, creationDate)
                                        .InColumnValue(MailboxTable.Columns.enableImap, enableImap)
                                        .InColumnValue(MailboxTable.Columns.enableImapSecured, enableImap)
                                        .InColumnValue(MailboxTable.Columns.enablePop, enablePop)
                                        .InColumnValue(MailboxTable.Columns.enablePopSecured, enablePop)
                                        .InColumnValue(MailboxTable.Columns.enableDeliver, enablePop || enableImap)
                                        .InColumnValue(MailboxTable.Columns.enableLda, enablePop || enableImap);

            var insertMailboxAlias = new SqlInsert(AliasTable.name)
                                        .InColumnValue(AliasTable.Columns.address, login)
                                        .InColumnValue(AliasTable.Columns.redirect, login)
                                        .InColumnValue(AliasTable.Columns.domain, domain)
                                        .InColumnValue(AliasTable.Columns.created, creationDate)
                                        .InColumnValue(AliasTable.Columns.modified, creationDate)
                                        .InColumnValue(AliasTable.Columns.active, 1);
            try
            {
                using (var db = _dbManager.GetAdminDb())
                {
                    using (var t = db.BeginTransaction())
                    {
                        db.ExecuteNonQuery(insertMailboxQuery);
                        db.ExecuteNonQuery(insertMailboxAlias);
                        t.Commit();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Message.StartsWith("Duplicate entry"))
                    throw new DuplicateNameException("You want to create mailbox with already existing address");

                throw;
            }

            return new MailboxBase(new MailAccountBase(login),
                                   new MailAddressBase(localpart, new WebDomainBase(domain)),
                                   new List<MailAddressBase>());
        }

        #endregion

        public override MailGroupBase _GetMailGroup(string mailgroupAddress)
        {
            return _GetMailGroups(new List<string> { mailgroupAddress }).FirstOrDefault();
        }

        protected override ICollection<MailGroupBase> _GetMailGroups(ICollection<string> mailgroupsAddresses)
        {
            const string address_alias = "maa";
            var mailGroupQuery = PostfixCommonQueries.GetAddressJoinedWithDomainQuery(address_alias, "mda")
                                        .Where(AliasTable.Columns.active.Prefix(address_alias), true)
                                        .Where(AliasTable.Columns.is_group.Prefix(address_alias), true)
// ReSharper disable CoVariantArrayConversion
                                        .Where(Exp.In(AliasTable.Columns.address.Prefix(address_alias), mailgroupsAddresses.ToArray()));
// ReSharper restore CoVariantArrayConversion
            List<PostfixMailgroupDto> mailgroupDtoList;
            using (var db = _dbManager.GetAdminDb())
            {
                mailgroupDtoList = db.ExecuteList(mailGroupQuery).ConvertAll(r => r.ToMailgroupDto()).ToList();
            }

            return mailgroupDtoList
                .Select(mailgroupDto =>
                        new MailGroupBase(mailgroupDto.address.ToPostfixAddress(), new List<MailAddressBase>()))
                .ToList();
        }

        protected override List<MailboxBase> _GetMailboxes(ICollection<string> mailboxNames)
        {
            var namesArg = mailboxNames.Cast<object>().ToArray();

            const string mailbox_ns = "msm";
            const string domain_ns = "msd";
            const string address_ns = "msa";

            var mailboxQuery = new SqlQuery(MailboxTable.name.Alias(mailbox_ns))
                .InnerJoin(AliasTable.name.Alias(address_ns),
                           Exp.EqColumns(MailboxTable.Columns.domain.Prefix(mailbox_ns),
                                         AliasTable.Columns.domain.Prefix(address_ns)))
                .InnerJoin(DomainTable.name.Alias(domain_ns),
                           Exp.EqColumns(MailboxTable.Columns.domain.Prefix(mailbox_ns),
                                         DomainTable.Columns.domain.Prefix(domain_ns)))
                .Select(MailboxTable.Columns.username.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.password.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.name.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.maildir.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.quota.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.localPart.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.domain.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.created.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.modified.Prefix(mailbox_ns))
                .Select(MailboxTable.Columns.active.Prefix(mailbox_ns))
                .Select(AliasTable.Columns.address.Prefix(address_ns))
                .Select(AliasTable.Columns.redirect.Prefix(address_ns))
                .Select(AliasTable.Columns.domain.Prefix(address_ns))
                .Select(AliasTable.Columns.created.Prefix(address_ns))
                .Select(AliasTable.Columns.modified.Prefix(address_ns))
                .Select(AliasTable.Columns.active.Prefix(address_ns))
                .Select(DomainTable.Columns.domain.Prefix(domain_ns))
                .Select(DomainTable.Columns.description.Prefix(domain_ns))
                .Select(DomainTable.Columns.aliases.Prefix(domain_ns))
                .Select(DomainTable.Columns.mailboxes.Prefix(domain_ns))
                .Select(DomainTable.Columns.maxquota.Prefix(domain_ns))
                .Select(DomainTable.Columns.quota.Prefix(domain_ns))
                .Select(DomainTable.Columns.transport.Prefix(domain_ns))
                .Select(DomainTable.Columns.backupmx.Prefix(domain_ns))
                .Select(DomainTable.Columns.created.Prefix(domain_ns))
                .Select(DomainTable.Columns.modified.Prefix(domain_ns))
                .Select(DomainTable.Columns.active.Prefix(domain_ns))
                .Where(MailboxTable.Columns.active.Prefix(mailbox_ns), 1)
                .Where(AliasTable.Columns.active.Prefix(address_ns), true)
                .Where(AliasTable.Columns.is_group.Prefix(address_ns), false)
                .Where(Exp.In(MailboxTable.Columns.username.Prefix(mailbox_ns), namesArg))
                .Where(Exp.EqColumns(AliasTable.Columns.redirect.Prefix(address_ns),
                                     MailboxTable.Columns.username.Prefix(mailbox_ns)));

            var mailboxList = new List<MailboxBase>();

            List<object[]> dbResult;

            using (var db = _dbManager.GetAdminDb())
            {
                dbResult = db.ExecuteList(mailboxQuery).ToList();
            }

            if (!dbResult.Any())
                return mailboxList;

            var groupedResult = dbResult.GroupBy(r => r[0]).ToDictionary(g => g.Key, g => g.ToList());

            const int domain_start_index = ToDtoConverters.MAILBOX_COLUMNS_COUNT + ToDtoConverters.MAIL_ADDRESS_COLUMNS_COUNT;

            foreach (var group in groupedResult)
            {
                var mailboxDto = group.Value[0].SubArray(0, ToDtoConverters.MAILBOX_COLUMNS_COUNT).ToMailboxDto();

                var aliasList = new List<MailAddressBase>();

                foreach (var groupVal in group.Value)
                {
                    var addressDto = groupVal.SubArray(ToDtoConverters.MAILBOX_COLUMNS_COUNT, ToDtoConverters.MAIL_ADDRESS_COLUMNS_COUNT).ToAddressDto();
                    var domainDto = groupVal.SubArray(domain_start_index, groupVal.Length - domain_start_index).ToWebDomainDto();
                    addressDto.Domain = domainDto;

                    var addr = addressDto.ToPostfixAddress();

                    if (addr.ToString() != mailboxDto.username)
                        aliasList.Add(addr);
                }

                var mailbox = new MailboxBase(
                    new MailAccountBase(mailboxDto.username),
                    new MailAddressBase(mailboxDto.local_part,
                        new WebDomainBase(mailboxDto.domain)), aliasList);

                mailboxList.Add(mailbox);
            }

            return mailboxList;
        }

        public ICollection<PostfixMailAddressDto> GetMailboxAddress(string mailboxAddress)
        {
            const string domain_alias = "msd";
            const string address_alias = "msa";
            var addressQuery = PostfixCommonQueries.GetAddressJoinedWithDomainQuery(address_alias, domain_alias)
                                .Where(AliasTable.Columns.redirect.Prefix(address_alias), mailboxAddress)
                                .Where(AliasTable.Columns.active.Prefix(address_alias), true)
                                .Where(AliasTable.Columns.is_group.Prefix(address_alias), false);

            using (var db = _dbManager.GetAdminDb())
            {
                var result = db.ExecuteList(addressQuery);
                return result.Select(r =>
                {
                    const int address_length = ToDtoConverters.MAIL_ADDRESS_COLUMNS_COUNT;

                    var address = r.SubArray(0, address_length).ToAddressDto();
                    var domainInfo = r.SubArray(address_length, r.Length - address_length).ToWebDomainDto();
                    address.Domain = domainInfo;

                    return address;
                }
                       ).ToList();
            }
        }

        public override MailboxBase _GetMailbox(string mailboxAddress)
        {
            return _GetMailboxes(new List<string> {mailboxAddress}).FirstOrDefault();
        }

        protected override MailGroupBase _CreateMailGroup(MailAddressBase address, List<MailAddressBase> mailboxAddressList)
        {
            var creationDate = DateTime.UtcNow;

            var membersString = string.Join(",", mailboxAddressList.Select(addr => addr.ToString()));

            var insertAddresses = new SqlInsert(AliasTable.name)
                .InColumns(AliasTable.Columns.address, AliasTable.Columns.redirect, AliasTable.Columns.domain,
                           AliasTable.Columns.modified,
                           AliasTable.Columns.created, AliasTable.Columns.active, AliasTable.Columns.is_group)
                .Values(address.ToString(), membersString, address.Domain.Name, creationDate, creationDate, true, true);

            try
            {
                using (var db = _dbManager.GetAdminDb())
                {
                    db.ExecuteNonQuery(insertAddresses);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Message.StartsWith("Duplicate entry"))
                    throw new ArgumentException("You want to create already existing group");

                throw;
            }

            return new MailGroupBase(address, mailboxAddressList);
        }

        protected override MailGroupBase _DeleteMailGroup(MailGroupBase mailGroup)
        {
            var mailgroupDeleteQuery = new SqlDelete(AliasTable.name)
                .Where(AliasTable.Columns.address, mailGroup.Address.ToString());
            int rowsDeleted;
            using (var db = _dbManager.GetAdminDb())
            {
                rowsDeleted = db.ExecuteNonQuery(mailgroupDeleteQuery);
                if (rowsDeleted > 1)
                {
                    var m = String.Format("Invalid addresses count was deleted: {0}. Address: {1}", rowsDeleted, mailGroup.Address);
                    throw new InvalidOperationException(m);
                }
            }
            return rowsDeleted == 0 ? null: mailGroup;
        }

        protected override void _UpdateMailbox(MailboxBase mailbox)
        {
            throw new NotSupportedException();
        }

        protected override void _DeleteMailbox(MailboxBase mailbox)
        {
            //Todo: think about free space in maildir
            var deleteMailboxQuery = new SqlDelete(MailboxTable.name).Where(MailboxTable.Columns.username, mailbox.Address.ToString());
            var deleteMailboxAliases = new SqlDelete(AliasTable.name).Where(AliasTable.Columns.redirect, mailbox.Address.ToString());
            using (var db = _dbManager.GetAdminDb())
            {
                using (var t = db.BeginTransaction())
                {
                    ClearMailboxStorageSpace(mailbox.Address.LocalPart, mailbox.Address.Domain.Name);
                    
                    db.ExecuteNonQuery(deleteMailboxQuery);
                    db.ExecuteNonQuery(deleteMailboxAliases);

                    t.Commit();
                }
            }
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
                throw new Exception("Can't delete stored mailbox's folder on server. Response code = " + response.StatusCode, response.ErrorException);

        }

        #region .Api

        private RestClient GetApiClient()
        {
            return _serverApi == null ? null : new RestClient(string.Format("{0}://{1}:{2}/", _serverApi.protocol, _serverApi.server_ip, _serverApi.port));
        }

        private RestRequest GetApiRequest(string apiUrl, Method method)
        {
            return _serverApi == null ? null : new RestRequest(string.Format("/api/{0}/{1}?auth_token={2}", _serverApi.version, apiUrl, _serverApi.token), method);
        }

        #endregion

        #region .Notification

        protected override MailboxBase _CreateNotificationAddress(string login, string password, string localpart, string domain)
        {
            return _CreateMailbox(login, password, localpart, domain, false, false);
        }

        protected override void _DeleteNotificationAddress(string address)
        {
            var mailbox = _GetMailbox(address);
            if(mailbox != null)
                _DeleteMailbox(mailbox);
        }

        #endregion
    }
}
