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
using System.Collections.ObjectModel;
using System.Linq;
using ASC.Core;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public abstract class ServerModel : MailServerBase
    {
        private WebDomainDal _domainDal;
        private WebDomainDal TeamlabDomainDal
        {
            get { return _domainDal ?? (_domainDal = new WebDomainDal(Tenant)); }
        }

        private MailboxDal _mailboxDal;
        private MailboxDal TeamlabMailboxDal
        {
            get { return _mailboxDal ?? (_mailboxDal = new MailboxDal(Tenant, SetupInfo.Limits.MailboxMaxCountPerUser)); }
        }

        private MailGroupDal _mailgroupDal;
        private MailGroupDal TeamlabMailGroupDal
        {
            get { return _mailgroupDal ?? (_mailgroupDal = new MailGroupDal(Tenant)); }
        }

        private MailAddressDal _mailaddressDal;
        private MailAddressDal TeamlabMailAddressDal
        {
            get { return _mailaddressDal ?? (_mailaddressDal = new MailAddressDal(Tenant)); }
        }

        private DnsDal _dkimDal;
        private DnsDal TeamlabDnsDal
        {
            get { return _dkimDal ?? (_dkimDal = new DnsDal(Tenant, User)); }
        }

        protected ServerModel(ServerSetup setup)
            : base(setup)
        {
        }

        #region .Domains

        public override IWebDomain CreateWebDomain(string name, bool is_verified, IMailServerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            WebDomainDto domain_dto;

            using (var db_context_with_tran = TeamlabDomainDal.CreateMailDbContext(true))
            {
                domain_dto = TeamlabDomainDal.AddWebDomain(name, is_verified, db_context_with_tran.DbManager);
                _CreateWebDomain(name);
                db_context_with_tran.CommitTransaction();
            }

            var webdomain = factory.CreateWebDomain(domain_dto.id, domain_dto.tenant, domain_dto.name, domain_dto.is_virified, this);

            return webdomain;
        }

        protected abstract WebDomainBase _CreateWebDomain(string name);

        public override IWebDomain GetWebDomain(int domain_id, IMailServerFactory factory)
        {
            if(domain_id < 0)
                throw new ArgumentException("domain_id has negative value", "domain_id");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var domain_dto = TeamlabDomainDal.GetDomain(domain_id);

            if (domain_dto == null)
                return null;

            var domain_base = _GetWebDomain(domain_dto.name);

            if(domain_base == null)
                throw new Exception("Server domain is missing");

            var webdomain = factory.CreateWebDomain(domain_dto.id, domain_dto.tenant, domain_dto.name, domain_dto.is_virified, this);

            return webdomain;
        }

        protected abstract WebDomainBase _GetWebDomain(string domain_name);

        public override ICollection<IWebDomain> GetWebDomains(IMailServerFactory factory)
        {
            if(factory == null)
                throw new ArgumentNullException("factory");

            var domains_dto_list = TeamlabDomainDal.GetTenantDomains();

            if(!domains_dto_list.Any())
                return new Collection<IWebDomain>();

            var domains_base_list = _GetWebDomains(domains_dto_list.ConvertAll(d => d.name));

            var web_domains = domains_base_list.Select(domain_base =>
                {
                    var domain_dto = domains_dto_list.Find(dal_domain => dal_domain.name == domain_base.Name);

                    var webdomain = factory.CreateWebDomain(domain_dto.id, domain_dto.tenant, domain_dto.name, domain_dto.is_virified, this);

                    return webdomain;

                }).ToList();

            return web_domains;
        }

        protected abstract List<WebDomainBase> _GetWebDomains(ICollection<string> domain_names);

        public override void DeleteWebDomain(IWebDomain web_domain, IMailServerFactory factory)
        {
            if (web_domain == null)
                throw new ArgumentNullException("web_domain", "ServerModel::DeleteWebDomain");

            if (factory == null)
                throw new ArgumentNullException("factory", "ServerModel::DeleteWebDomain");

            using (var db_context_with_tran = TeamlabDomainDal.CreateMailDbContext(true))
            {
                TeamlabDomainDal.DeleteDomain(web_domain.Id, db_context_with_tran.DbManager);
                _DeleteWebDomain(new WebDomainBase(web_domain));
                TeamlabDnsDal.RemoveUsedDns(web_domain.Id, db_context_with_tran.DbManager);
                db_context_with_tran.CommitTransaction();
            }
        }

        protected abstract void _DeleteWebDomain(WebDomainBase web_domain);

        public override bool IsDomainExists(string name)
        {
            var domain = TeamlabDomainDal.GetDomain(name);
            return domain != null;
        }

        #endregion

        #region .Mailboxes

        public override IMailbox CreateMailbox(string localpart, string password, IWebDomain domain, IMailAccount account, IMailServerFactory factory)
        {
            if (string.IsNullOrEmpty(localpart))
                throw new ArgumentNullException("localpart");

            if(domain == null)
                throw new ArgumentNullException("domain");

            if (account == null)
                throw new ArgumentNullException("account");

            if (localpart.Length + domain.Name.Length > 318) // 318 because of @ sign
                throw new ArgumentException("Address of mailbox exceed limitation of 319 characters.", "localpart");

            var mailbox_base = new MailboxBase(new MailAccountBase(account.Login),
                                   new MailAddressBase(localpart, new WebDomainBase(domain)),
                                   new List<MailAddressBase>())
                {
                    DateCreated = DateTime.UtcNow
                };

            MailboxWithAddressDto mailbox_with_address_dto;

            using (var db_context_with_tran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                mailbox_with_address_dto = TeamlabMailboxDal.CreateMailbox(account.TeamlabAccount,
                                                                           mailbox_base.Address.ToString(), password,
                                                                           mailbox_base.Address.LocalPart,
                                                                           mailbox_base.Address.DateCreated,
                                                                           domain.Id, domain.Name, domain.IsVerified, db_context_with_tran.DbManager);
                _CreateMailbox(account.Login, password, localpart, domain.Name);

                db_context_with_tran.CommitTransaction();
            }

            var mailbox_address = factory.CreateMailAddress(mailbox_with_address_dto.mailbox_address.id, mailbox_with_address_dto.mailbox_address.tenant, mailbox_with_address_dto.mailbox_address.name, domain) ;

            var mailbox = factory.CreateMailbox(mailbox_with_address_dto.mailbox.id, mailbox_with_address_dto.mailbox.tenant_id,
                mailbox_address, account, new List<IMailAddress>(), this);

            return mailbox;

        }

        protected abstract MailboxBase _CreateMailbox(string login, string password, string localpart, string domain);

        public override ICollection<IMailbox> GetMailboxes(IMailServerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var mailbox_list = new List<IMailbox>();

            using (var db_context = TeamlabMailboxDal.CreateMailDbContext())
            {
                var mailbox_dto_list = TeamlabMailboxDal.GetMailboxes(db_context.DbManager);

                var mailbox_base_list = _GetMailboxes(mailbox_dto_list.ConvertAll(d => d.mailbox.address));

                mailbox_list.AddRange(from server_mailbox in mailbox_base_list
                                      let mailbox_dto = mailbox_dto_list.Find(d => d.mailbox.address == server_mailbox.Address.ToString())
                                      let domain = factory.CreateWebDomain(mailbox_dto.mailbox_address.domain.id, mailbox_dto.mailbox_address.domain.tenant, mailbox_dto.mailbox_address.domain.name, mailbox_dto.mailbox_address.domain.is_virified, this)
                                      let mailbox_address = factory.CreateMailAddress(mailbox_dto.mailbox_address.id, mailbox_dto.mailbox_address.tenant, mailbox_dto.mailbox_address.name, domain)
                                      let aliases_dto_list = TeamlabMailAddressDal.GetMailboxAliases(mailbox_dto.mailbox.id, db_context.DbManager)
                                      let aliases = aliases_dto_list.Select(a => factory.CreateMailAddress(a.id, a.tenant, a.name, domain)).ToList()
                                      let teamlab_account = CoreContext.Authentication.GetAccountByID(new Guid(mailbox_dto.mailbox.user_id))
                                      let account = factory.CreateMailAccount(teamlab_account, server_mailbox.Account.Login)
                                      select factory.CreateMailbox(mailbox_dto.mailbox.id, mailbox_dto.mailbox.tenant_id, mailbox_address, account, aliases.ToList(), this));
            }

            return mailbox_list;
        }

        protected abstract List<MailboxBase> _GetMailboxes(ICollection<string> mailbox_names);

        public override IMailbox GetMailbox(int mailbox_id, IMailServerFactory factory)
        {
            if (mailbox_id < 0)
                throw new ArgumentException("mailbox_id has negative value");

            if(factory == null)
                throw new ArgumentNullException("factory");

            MailboxWithAddressDto mailbox_dto;
            List<MailAddressDto> aliases_dto_list;

            using (var db_context = TeamlabMailboxDal.CreateMailDbContext())
            {
                mailbox_dto = TeamlabMailboxDal.GetMailbox(mailbox_id, db_context.DbManager);
                aliases_dto_list = TeamlabMailAddressDal.GetMailboxAliases(mailbox_id, db_context.DbManager);
            }

            if (mailbox_dto == null)
                return null;

            var mailbox_base = _GetMailbox(mailbox_dto.mailbox_address.ToString());

            if (mailbox_base == null)
                throw new Exception("Mailbox is missing on server");

            var mailbox_domain = factory.CreateWebDomain(mailbox_dto.mailbox_address.domain.id, mailbox_dto.mailbox_address.domain.tenant, mailbox_dto.mailbox_address.domain.name, mailbox_dto.mailbox_address.domain.is_virified, this);

            var mailbox_address = factory.CreateMailAddress(mailbox_dto.mailbox_address.id, mailbox_dto.mailbox_address.tenant, mailbox_dto.mailbox_address.name, mailbox_domain);

            var account = CoreContext.Authentication.GetAccountByID(new Guid(mailbox_dto.mailbox.user_id));

            var mailbox_account = factory.CreateMailAccount(account, mailbox_base.Account.Login);

            var mailbox_aliases =
                aliases_dto_list
                    .Select(alias =>
                            factory.CreateMailAddress(alias.id, alias.tenant, alias.name, mailbox_domain))
                    .ToList();

            var mailbox = factory.CreateMailbox(mailbox_dto.mailbox.id, mailbox_dto.mailbox.tenant_id, 
                mailbox_address, mailbox_account, mailbox_aliases.ToList(), this);

            return mailbox;

        }

        public abstract MailboxBase _GetMailbox(string mailbox_address);

        public override void UpdateMailbox(IMailbox mailbox)
        {
            _UpdateMailbox(new MailboxBase(mailbox));
        }

        protected abstract void _UpdateMailbox(MailboxBase mailbox);

        public override void DeleteMailbox(IMailbox mailbox)
        {
            if (mailbox == null)
                throw new ArgumentNullException("mailbox", "ServerModel::DeleteMailbox");

            using (var db_context_with_tran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                TeamlabMailboxDal.DeleteMailbox(mailbox.Id, db_context_with_tran.DbManager);
                _DeleteMailbox(new MailboxBase(mailbox));
                db_context_with_tran.CommitTransaction();
            }

        }

        protected abstract void _DeleteMailbox(MailboxBase mailbox);

        #endregion

        #region .Groups

        public override IMailGroup CreateMailGroup(string group_name, IWebDomain domain, List<int> address_ids, IMailServerFactory factory)
        {
            if (string.IsNullOrEmpty(group_name))
                throw new ArgumentNullException("group_name", "ServerModel::CreateMailGroup");

            if (domain == null)
                throw new ArgumentNullException("domain");

            if (address_ids == null)
                throw new ArgumentNullException("address_ids");

            if(!address_ids.Any())
                throw new ArgumentException("Empty address_ids list");

            if (factory == null)
                throw new ArgumentNullException("factory");

            MailGroupDto mailgroup_dto;

            using (var db_context_with_tran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                var address_dto_list = TeamlabMailAddressDal.GetMailAddresses(address_ids, db_context_with_tran.DbManager);

                var address_base_list =
                    address_dto_list
                        .Select(dto =>
                                new MailAddressBase(dto.name,
                                                    new WebDomainBase(dto.domain.name)))
                        .ToList();

                var mailgroup_base = new MailGroupBase(new MailAddressBase(group_name, new WebDomainBase(domain)),
                                                       address_base_list);
                mailgroup_dto = TeamlabMailGroupDal.SaveMailGroup(mailgroup_base.Address.LocalPart,
                                                                  mailgroup_base.Address.DateCreated,
                                                                  domain.Id,
                                                                  domain.Name,
                                                                  domain.IsVerified,
                                                                  address_dto_list, db_context_with_tran.DbManager);
                _CreateMailGroup(mailgroup_base.Address, address_base_list);

                db_context_with_tran.CommitTransaction();
            }

            var mailgroup_address = factory.CreateMailAddress(mailgroup_dto.address.id, mailgroup_dto.address.tenant, mailgroup_dto.address.name, domain);

            var in_addresses =
                mailgroup_dto.addresses.Select(
                    address_dto => factory.CreateMailAddress(address_dto.id, address_dto.tenant, address_dto.name, domain))
                               .ToList();

            var mailgroup = factory.CreateMailGroup(mailgroup_dto.id, mailgroup_dto.id_tenant, 
                mailgroup_address, in_addresses.ToList(), this);

            return mailgroup;
        }

        protected abstract MailGroupBase _CreateMailGroup(MailAddressBase address, List<MailAddressBase> mailbox_address_list);

        public override ICollection<IMailGroup> GetMailGroups(IMailServerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var tl_groups = TeamlabMailGroupDal.GetMailGroups();

            var server_groups = _GetMailGroups(tl_groups.Select(g => g.address.ToString()).ToList());

            var list_mail_groups = new List<IMailGroup>();

            foreach (var server_group in server_groups)
            {
                var tl_group = tl_groups.First(g => g.address.ToString() == server_group.Address.ToString());

                var domain =
                    factory.CreateWebDomain(tl_group.address.domain.id, tl_group.address.domain.tenant, tl_group.address.domain.name, tl_group.address.domain.is_virified, this);

                var address =
                    factory.CreateMailAddress(tl_group.address.id, tl_group.address.tenant, tl_group.address.name, domain);

                var group_in_addresses =
                    tl_group.addresses
                            .Select(a => factory.CreateMailAddress(a.id, a.tenant, a.name, domain))
                            .ToList();

                var mailgroup = factory.CreateMailGroup(tl_group.id, tl_group.id_tenant, 
                    address, group_in_addresses.ToList(), this);

                list_mail_groups.Add(mailgroup);

            }

            return list_mail_groups;
        }

        protected abstract ICollection<MailGroupBase> _GetMailGroups(ICollection<string> mailgroups_addresses);

        public override IMailGroup GetMailGroup(int mailgroup_id, IMailServerFactory factory)
        {
            if (mailgroup_id < 0)
                throw new ArgumentException("mailgroup_id has negative value");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var mailgroup_dto = TeamlabMailGroupDal.GetMailGroup(mailgroup_id);

            if (mailgroup_dto == null)
                return null;

            var mailgroup_base = _GetMailGroup(mailgroup_dto.address.ToString());

            if (mailgroup_base == null)
                throw new Exception("Mailgroup is missing on server");

            var mailgroup_domain = factory.CreateWebDomain(mailgroup_dto.address.domain.id, mailgroup_dto.address.domain.tenant, mailgroup_dto.address.domain.name, mailgroup_dto.address.domain.is_virified, this);

            var mailgroup_address = factory.CreateMailAddress(mailgroup_dto.address.id, mailgroup_dto.address.tenant, mailgroup_dto.address.name, mailgroup_domain);

            var mailgroup_addresses =
                mailgroup_dto.addresses
                             .Select(alias =>
                                     factory.CreateMailAddress(alias.id, alias.tenant, alias.name, mailgroup_domain))
                             .ToList();

            var mailgroup = factory.CreateMailGroup(mailgroup_dto.id, mailgroup_dto.id_tenant, 
                mailgroup_address, mailgroup_addresses.ToList(), this);

            return mailgroup;
        }

        public abstract MailGroupBase _GetMailGroup(string mailgroup_address);

        public override void DeleteMailGroup(int mailgroup_id, IMailServerFactory factory)
        {
            if (mailgroup_id < 0)
                throw new ArgumentException("mailgroup_id has negative value");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var mailgroup = GetMailGroup(mailgroup_id, factory);

            if (mailgroup == null)
                throw new ArgumentException("Mailgroup is missing");

            using (var db_context_with_tran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                TeamlabMailGroupDal.DeleteMailGroup(mailgroup.Id, db_context_with_tran.DbManager);
                _DeleteMailGroup(new MailGroupBase(mailgroup));
                db_context_with_tran.CommitTransaction();
            }
        }

        protected abstract MailGroupBase _DeleteMailGroup(MailGroupBase mail_group);

        #endregion

        #region .DNS

        public override IDnsSettings GetFreeDnsRecords(IMailServerFactory factory)
        {
            var dns_dto = TeamlabDnsDal.GetFreeDnsRecords() ??
                TeamlabDnsDal.CreateFreeDnsRecords(SetupInfo.DnsPresets.DkimSelector, SetupInfo.DnsPresets.DomainCheckPrefix, SetupInfo.DnsPresets.SpfValue);

            var txt_name = SetupInfo.DnsPresets.CurrentOrigin;
            var dns = factory.CreateDnsSettings(dns_dto.id, dns_dto.tenant, dns_dto.user, "", dns_dto.dkim_selector,
                                                dns_dto.dkim_private_key, dns_dto.dkim_public_key, txt_name,
                                                dns_dto.domain_chek, txt_name, dns_dto.spf, 
                                                SetupInfo.DnsPresets.MxHost, SetupInfo.DnsPresets.MxPriority);

           return dns;
        }

        #endregion
    }
}
