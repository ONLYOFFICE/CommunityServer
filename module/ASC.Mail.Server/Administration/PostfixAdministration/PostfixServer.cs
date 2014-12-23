/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
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
                var domain_description = String.Format("Domain created in UtcTime: {0}, for tenant: {1}",
                                                       domain.DateCreated, Tenant);
                var insert_domain_query = new SqlInsert(DomainTable.name)
                    .InColumnValue(DomainTable.Columns.domain, domain.Name)
                    .InColumnValue(DomainTable.Columns.description, domain_description)
                    .InColumnValue(DomainTable.Columns.transport, "virtual")
                    .InColumnValue(DomainTable.Columns.created, domain.DateCreated)
                    .InColumnValue(DomainTable.Columns.modified, domain.DateCreated)
                    .InColumnValue(DomainTable.Columns.active, true);

                using (var db = _dbManager.GetAdminDb())
                {
                    db.ExecuteNonQuery(insert_domain_query);
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

        protected override List<WebDomainBase> _GetWebDomains(ICollection<string> domain_names)
        {
            if (null == domain_names)
                throw new ArgumentNullException("domain_names", "in _GetWebDomains method");

            object[] names_arg = domain_names.ToArray();

            var postfix_domains_query = new SqlQuery(DomainTable.name)
                .Select(DomainTable.Columns.domain, DomainTable.Columns.created)
                .Where(DomainTable.Columns.active, 1)
                .Where(Exp.In(DomainTable.Columns.domain, names_arg))
                .OrderBy(DomainTable.Columns.created, true);

            List<object[]> rows;
            using (var db = _dbManager.GetAdminDb())
                rows = db.ExecuteList(postfix_domains_query);

            var domains = rows
                .Select(d => new WebDomainBase(d[0].ToString())
                    {
                        DateCreated = Convert.ToDateTime(d[1])
                    })
                .ToList();

            return domains;
        }

        protected override WebDomainBase _GetWebDomain(string domain_name)
        {
            if (string.IsNullOrEmpty(domain_name))
                throw new ArgumentNullException(domain_name);

            var postfix_domains_query = new SqlQuery(DomainTable.name)
                .Select(DomainTable.Columns.domain, DomainTable.Columns.created)
                .Where(DomainTable.Columns.active, 1)
                .Where(DomainTable.Columns.domain, domain_name);

            List<object[]> rows;
            using (var db = _dbManager.GetAdminDb())
                rows = db.ExecuteList(postfix_domains_query);

            var domain = rows
                .Select(d => new WebDomainBase(d[0].ToString())
                    {
                        DateCreated = Convert.ToDateTime(d[1])
                    })
                .FirstOrDefault();

            return domain;
        }


        protected override void _DeleteWebDomain(WebDomainBase web_domain)
        {
            var delete_mailbox_query = new SqlDelete(MailboxTable.name).Where(MailboxTable.Columns.domain, web_domain.Name);
            var delete_mailbox_aliases = new SqlDelete(AliasTable.name).Where(AliasTable.Columns.domain, web_domain.Name);
            var delete_domain_query = new SqlDelete(DomainTable.name).Where(DomainTable.Columns.domain, web_domain.Name);
            using (var db = _dbManager.GetAdminDb())
            {
                using (var t = db.BeginTransaction())
                {
                    ClearDomainStorageSpace(web_domain.Name);

                    db.ExecuteNonQuery(delete_mailbox_aliases);
                    db.ExecuteNonQuery(delete_mailbox_query);
                    db.ExecuteNonQuery(delete_domain_query);

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

        protected override MailboxBase _CreateMailbox(string login, string password, string localpart, string domain)
        {
            var creation_date = DateTime.UtcNow;

            var maildir = GenerateMaildirPath(domain, localpart, creation_date);

            var insert_mailbox_query = new SqlInsert(MailboxTable.name)
                                        .InColumnValue(MailboxTable.Columns.username, login)
                                        .InColumnValue(MailboxTable.Columns.name, localpart)
                                        .InColumnValue(MailboxTable.Columns.password, PostfixPasswordEncryptor.EncryptString(HashType.Md5, password))
                                        .InColumnValue(MailboxTable.Columns.maildir, maildir)
                                        .InColumnValue(MailboxTable.Columns.local_part, localpart)
                                        .InColumnValue(MailboxTable.Columns.domain, domain)
                                        .InColumnValue(MailboxTable.Columns.created, creation_date)
                                        .InColumnValue(MailboxTable.Columns.modified, creation_date);

            var insert_mailbox_alias = new SqlInsert(AliasTable.name)
                                        .InColumnValue(AliasTable.Columns.address, login)
                                        .InColumnValue(AliasTable.Columns.redirect, login)
                                        .InColumnValue(AliasTable.Columns.domain, domain)
                                        .InColumnValue(AliasTable.Columns.created, creation_date)
                                        .InColumnValue(AliasTable.Columns.modified, creation_date)
                                        .InColumnValue(AliasTable.Columns.active, 1);
            try
            {
                using (var db = _dbManager.GetAdminDb())
                {
                    using (var t = db.BeginTransaction())
                    {
                        db.ExecuteNonQuery(insert_mailbox_query);
                        db.ExecuteNonQuery(insert_mailbox_alias);
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

        public override MailGroupBase _GetMailGroup(string mailgroup_address)
        {
            return _GetMailGroups(new List<string> { mailgroup_address }).FirstOrDefault();
        }

        protected override ICollection<MailGroupBase> _GetMailGroups(ICollection<string> mail_groups_addresses)
        {
            const string address_alias = "maa";
            var mail_group_query = PostfixCommonQueries.GetAddressJoinedWithDomainQuery(address_alias, "mda")
                                        .Where(AliasTable.Columns.active.Prefix(address_alias), true)
                                        .Where(AliasTable.Columns.is_group.Prefix(address_alias), true)
// ReSharper disable CoVariantArrayConversion
                                        .Where(Exp.In(AliasTable.Columns.address.Prefix(address_alias), mail_groups_addresses.ToArray()));
// ReSharper restore CoVariantArrayConversion
            List<PostfixMailgroupDto> mailgroup_dto_list;
            using (var db = _dbManager.GetAdminDb())
            {
                mailgroup_dto_list = db.ExecuteList(mail_group_query).ConvertAll(r => r.ToMailgroupDto()).ToList();
            }

            return mailgroup_dto_list
                .Select(mailgroup_dto =>
                        new MailGroupBase(mailgroup_dto.address.ToPostfixAddress(), new List<MailAddressBase>()))
                .ToList();
        }

        protected override List<MailboxBase> _GetMailboxes(ICollection<string> mailbox_names)
        {
            var names_arg = mailbox_names.ToArray();

            const string mailbox_ns = "msm";
            const string domain_ns = "msd";
            const string address_ns = "msa";

            var mailbox_query = new SqlQuery(MailboxTable.name.Alias(mailbox_ns))
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
                .Select(MailboxTable.Columns.local_part.Prefix(mailbox_ns))
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
                .Where(Exp.In(MailboxTable.Columns.username.Prefix(mailbox_ns), names_arg))
                .Where(Exp.EqColumns(AliasTable.Columns.redirect.Prefix(address_ns),
                                     MailboxTable.Columns.username.Prefix(mailbox_ns)));

            var mailbox_list = new List<MailboxBase>();

            List<object[]> db_result;

            using (var db = _dbManager.GetAdminDb())
            {
                db_result = db.ExecuteList(mailbox_query).ToList();
            }

            if (!db_result.Any())
                return mailbox_list;

            var grouped_result = db_result.GroupBy(r => r[0]).ToDictionary(g => g.Key, g => g.ToList());

            const int domain_start_index = ToDtoConverters.mailbox_columns_count + ToDtoConverters.mail_address_columns_count;

            foreach (var group in grouped_result)
            {
                var mailbox_dto = group.Value[0].SubArray(0, ToDtoConverters.mailbox_columns_count).ToMailboxDto();

                var alias_list = new List<MailAddressBase>();

                foreach (var group_val in group.Value)
                {
                    var address_dto = group_val.SubArray(ToDtoConverters.mailbox_columns_count, ToDtoConverters.mail_address_columns_count).ToAddressDto();
                    var domain_dto = group_val.SubArray(domain_start_index, group_val.Length - domain_start_index).ToWebDomainDto();
                    address_dto.Domain = domain_dto;

                    var addr = address_dto.ToPostfixAddress();

                    if (addr.ToString() != mailbox_dto.username)
                        alias_list.Add(addr);
                }

                var mailbox = new MailboxBase(
                    new MailAccountBase(mailbox_dto.username),
                    new MailAddressBase(mailbox_dto.local_part,
                        new WebDomainBase(mailbox_dto.domain)), alias_list);

                mailbox_list.Add(mailbox);
            }

            return mailbox_list;
        }

        public ICollection<PostfixMailAddressDto> GetMailboxAddress(string mailbox_address)
        {
            const string domain_alias = "msd";
            const string address_alias = "msa";
            var address_query = PostfixCommonQueries.GetAddressJoinedWithDomainQuery(address_alias, domain_alias)
                                .Where(AliasTable.Columns.redirect.Prefix(address_alias), mailbox_address)
                                .Where(AliasTable.Columns.active.Prefix(address_alias), true)
                                .Where(AliasTable.Columns.is_group.Prefix(address_alias), false);

            using (var db = _dbManager.GetAdminDb())
            {
                var result = db.ExecuteList(address_query);
                return result.Select(r =>
                {
                    const int address_length = ToDtoConverters.mail_address_columns_count;

                    var address = r.SubArray(0, address_length).ToAddressDto();
                    var domain_info = r.SubArray(address_length, r.Length - address_length).ToWebDomainDto();
                    address.Domain = domain_info;

                    return address;
                }
                       ).ToList();
            }
        }

        public override MailboxBase _GetMailbox(string mailbox_address)
        {
            return _GetMailboxes(new List<string>() {mailbox_address}).FirstOrDefault();
        }

        protected override MailGroupBase _CreateMailGroup(MailAddressBase address, List<MailAddressBase> in_addresses)
        {
            var creation_date = DateTime.UtcNow;

            var members_string = string.Join(",", in_addresses.Select(addr => addr.ToString()));

            var insert_addresses = new SqlInsert(AliasTable.name)
                .InColumns(AliasTable.Columns.address, AliasTable.Columns.redirect, AliasTable.Columns.domain,
                           AliasTable.Columns.modified,
                           AliasTable.Columns.created, AliasTable.Columns.active, AliasTable.Columns.is_group)
                .Values(address.ToString(), members_string, address.Domain.Name, creation_date, creation_date, true, true);

            try
            {
                using (var db = _dbManager.GetAdminDb())
                {
                    db.ExecuteNonQuery(insert_addresses);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Message.StartsWith("Duplicate entry"))
                    throw new ArgumentException("You want to create already existing group");

                throw;
            }

            return new MailGroupBase(address, in_addresses);
        }

        protected override MailGroupBase _DeleteMailGroup(MailGroupBase mail_group)
        {
            var mailgroup_delete_query = new SqlDelete(AliasTable.name)
                .Where(AliasTable.Columns.address, mail_group.Address.ToString());
            var rows_deleted = 0;
            using (var db = _dbManager.GetAdminDb())
            {
                rows_deleted = db.ExecuteNonQuery(mailgroup_delete_query);
                if (rows_deleted > 1)
                {
                    var m = String.Format("Invalid addresses count was deleted: {0}. Address: {1}", rows_deleted, mail_group.Address);
                    throw new InvalidOperationException(m);
                }
            }
            return rows_deleted == 0 ? null: mail_group;
        }

        protected override void _UpdateMailbox(MailboxBase mailbox)
        {
            throw new NotSupportedException();
        }

        protected override void _DeleteMailbox(MailboxBase mailbox)
        {
            //Todo: think about free space in maildir
            var delete_mailbox_query = new SqlDelete(MailboxTable.name).Where(MailboxTable.Columns.username, mailbox.Address.ToString());
            var delete_mailbox_aliases = new SqlDelete(AliasTable.name).Where(AliasTable.Columns.redirect, mailbox.Address.ToString());
            using (var db = _dbManager.GetAdminDb())
            {
                using (var t = db.BeginTransaction())
                {
                    ClearMailboxStorageSpace(mailbox.Address.LocalPart, mailbox.Address.Domain.Name);
                    
                    db.ExecuteNonQuery(delete_mailbox_query);
                    db.ExecuteNonQuery(delete_mailbox_aliases);

                    t.Commit();
                }
            }
        }

        private void ClearMailboxStorageSpace(string mailbox_localpart, string domain_name)
        {
            if (_serverApi == null) return; // Skip if api not presented

            var client = GetApiClient();
            var request = GetApiRequest("domains/{domain_name}/mailboxes/{mailbox_localpart}", Method.DELETE);
            request.AddUrlSegment("domain_name", domain_name);
            request.AddUrlSegment("mailbox_localpart", mailbox_localpart);
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

        private RestRequest GetApiRequest(string api_url, Method method)
        {
            return _serverApi == null ? null : new RestRequest(string.Format("/api/{0}/{1}?auth_token={2}", _serverApi.version, api_url, _serverApi.token), method);
        }

        #endregion
    }
}
