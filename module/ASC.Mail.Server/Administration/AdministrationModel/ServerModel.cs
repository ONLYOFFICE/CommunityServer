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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ASC.Core;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;
using ASC.Web.Core.Utility.Settings;
using ServerType = ASC.Mail.Server.Administration.Interfaces.ServerType;

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

        private ServerDal _serverDal;
        private ServerDal TeamlabServerDal
        {
            get
            {
                return _serverDal ?? (_serverDal = new ServerDal(Tenant));
            }
        }

        protected ServerModel(ServerSetup setup)
            : base(setup)
        {
        }

        #region .Domains

        public override IWebDomain CreateWebDomain(string name, bool isVerified, IMailServerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            WebDomainDto domainDto;

            using (var dbContextWithTran = TeamlabDomainDal.CreateMailDbContext(true))
            {
                domainDto = TeamlabDomainDal.AddWebDomain(name, isVerified, dbContextWithTran.DbManager);
                _CreateWebDomain(name);
                dbContextWithTran.CommitTransaction();
            }

            var webdomain = factory.CreateWebDomain(domainDto.id, domainDto.tenant, domainDto.name, domainDto.is_virified, this);

            return webdomain;
        }

        protected abstract WebDomainBase _CreateWebDomain(string name);

        public override IWebDomain GetWebDomain(int domainId, IMailServerFactory factory)
        {
            if(domainId < 0)
                throw new ArgumentException("domain_id has negative value", "domainId");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var domainDto = TeamlabDomainDal.GetDomain(domainId);

            if (domainDto == null)
                throw new Exception("Domain is missing");

            var domainBase = _GetWebDomain(domainDto.name);

            if(domainBase == null)
                throw new Exception("Server domain is missing");

            var webdomain = factory.CreateWebDomain(domainDto.id, domainDto.tenant, domainDto.name, domainDto.is_virified, this);

            return webdomain;
        }

        protected abstract WebDomainBase _GetWebDomain(string domainName);

        public override ICollection<IWebDomain> GetWebDomains(IMailServerFactory factory)
        {
            if(factory == null)
                throw new ArgumentNullException("factory");

            var domainsDtoList = TeamlabDomainDal.GetTenantDomains();

            if(!domainsDtoList.Any())
                return new Collection<IWebDomain>();

            var domainsBaseList = _GetWebDomains(domainsDtoList.ConvertAll(d => d.name));

            var webDomains = domainsBaseList.Select(domainBase =>
                {
                    var domainDto = domainsDtoList.Find(dalDomain => dalDomain.name == domainBase.Name);

                    var webdomain = factory.CreateWebDomain(domainDto.id, domainDto.tenant, domainDto.name, domainDto.is_virified, this);

                    return webdomain;

                }).OrderBy(obj=>obj.Tenant).ToList();

            return webDomains;
        }

        protected abstract List<WebDomainBase> _GetWebDomains(ICollection<string> domainNames);

        public override void DeleteWebDomain(IWebDomain webDomain, IMailServerFactory factory)
        {
            if (webDomain == null)
                throw new ArgumentNullException("webDomain", "ServerModel::DeleteWebDomain");

            if (factory == null)
                throw new ArgumentNullException("factory", "ServerModel::DeleteWebDomain");

            using (var dbContextWithTran = TeamlabDomainDal.CreateMailDbContext(true))
            {
                TeamlabDomainDal.DeleteDomain(webDomain.Id, dbContextWithTran.DbManager);
                _DeleteWebDomain(new WebDomainBase(webDomain));
                TeamlabDnsDal.RemoveUsedDns(webDomain.Id, dbContextWithTran.DbManager);
                dbContextWithTran.CommitTransaction();
            }
        }

        protected abstract void _DeleteWebDomain(WebDomainBase webDomain);

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

            var mailboxBase = new MailboxBase(new MailAccountBase(account.Login),
                                   new MailAddressBase(localpart, new WebDomainBase(domain)),
                                   new List<MailAddressBase>())
                {
                    DateCreated = DateTime.UtcNow
                };

            MailboxWithAddressDto mailboxWithAddressDto;

            using (var dbContextWithTran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                mailboxWithAddressDto = TeamlabMailboxDal.CreateMailbox(account.TeamlabAccount,
                                                                           mailboxBase.Address.ToString(), password,
                                                                           mailboxBase.Address.LocalPart,
                                                                           mailboxBase.Address.DateCreated,
                                                                           domain.Id, domain.Name, domain.IsVerified, dbContextWithTran.DbManager);
                _CreateMailbox(account.Login, password, localpart, domain.Name);

                dbContextWithTran.CommitTransaction();
            }

            var mailboxAddress = factory.CreateMailAddress(mailboxWithAddressDto.mailbox_address.id, mailboxWithAddressDto.mailbox_address.tenant, mailboxWithAddressDto.mailbox_address.name, domain) ;

            var mailbox = factory.CreateMailbox(mailboxWithAddressDto.mailbox.id, mailboxWithAddressDto.mailbox.tenant,
                mailboxAddress, account, new List<IMailAddress>(), this);

            return mailbox;

        }

        protected abstract MailboxBase _CreateMailbox(string login, string password, string localpart, string domain, bool enableImap = true, bool enablePop = true);

        public override ICollection<IMailbox> GetMailboxes(IMailServerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var mailboxList = new List<IMailbox>();

            using (var dbContext = TeamlabMailboxDal.CreateMailDbContext())
            {
                var mailboxDtoList = TeamlabMailboxDal.GetMailboxes(dbContext.DbManager);

                var mailboxBaseList = _GetMailboxes(mailboxDtoList.ConvertAll(d => d.mailbox.address));

                mailboxList.AddRange(from serverMailbox in mailboxBaseList
                                      let mailboxDto = mailboxDtoList.Find(d => d.mailbox.address == serverMailbox.Address.ToString())
                                      let domain = factory.CreateWebDomain(mailboxDto.mailbox_address.domain.id, mailboxDto.mailbox_address.domain.tenant, mailboxDto.mailbox_address.domain.name, mailboxDto.mailbox_address.domain.is_virified, this)
                                      let mailboxAddress = factory.CreateMailAddress(mailboxDto.mailbox_address.id, mailboxDto.mailbox_address.tenant, mailboxDto.mailbox_address.name, domain)
                                      let aliasesDtoList = TeamlabMailAddressDal.GetMailboxAliases(mailboxDto.mailbox.id, dbContext.DbManager)
                                      let aliases = aliasesDtoList.Select(a => factory.CreateMailAddress(a.id, a.tenant, a.name, domain)).ToList()
                                      let teamlabAccount = CoreContext.Authentication.GetAccountByID(new Guid(mailboxDto.mailbox.user))
                                      let account = factory.CreateMailAccount(teamlabAccount, serverMailbox.Account.Login)
                                      select factory.CreateMailbox(mailboxDto.mailbox.id, mailboxDto.mailbox.tenant, mailboxAddress, account, aliases.ToList(), this));
            }

            return mailboxList;
        }

        protected abstract List<MailboxBase> _GetMailboxes(ICollection<string> mailboxNames);

        public override IMailbox GetMailbox(int mailboxId, IMailServerFactory factory)
        {
            if (mailboxId < 0)
                throw new ArgumentException("mailbox_id has negative value");

            if(factory == null)
                throw new ArgumentNullException("factory");

            MailboxWithAddressDto mailboxDto;
            List<MailAddressDto> aliasesDtoList;

            using (var dbContext = TeamlabMailboxDal.CreateMailDbContext())
            {
                mailboxDto = TeamlabMailboxDal.GetMailbox(mailboxId, dbContext.DbManager);
                aliasesDtoList = TeamlabMailAddressDal.GetMailboxAliases(mailboxId, dbContext.DbManager);
            }

            if (mailboxDto == null)
                return null;

            var mailboxBase = _GetMailbox(mailboxDto.mailbox_address.ToString());

            if (mailboxBase == null)
                throw new Exception("Mailbox is missing on server");

            var mailboxDomain = factory.CreateWebDomain(mailboxDto.mailbox_address.domain.id, mailboxDto.mailbox_address.domain.tenant, mailboxDto.mailbox_address.domain.name, mailboxDto.mailbox_address.domain.is_virified, this);

            var mailboxAddress = factory.CreateMailAddress(mailboxDto.mailbox_address.id, mailboxDto.mailbox_address.tenant, mailboxDto.mailbox_address.name, mailboxDomain);

            var account = CoreContext.Authentication.GetAccountByID(new Guid(mailboxDto.mailbox.user));

            var mailboxAccount = factory.CreateMailAccount(account, mailboxBase.Account.Login);

            var mailboxAliases =
                aliasesDtoList
                    .Select(alias =>
                            factory.CreateMailAddress(alias.id, alias.tenant, alias.name, mailboxDomain))
                    .ToList();

            var mailbox = factory.CreateMailbox(mailboxDto.mailbox.id, mailboxDto.mailbox.tenant, 
                mailboxAddress, mailboxAccount, mailboxAliases.ToList(), this);

            return mailbox;

        }

        public abstract MailboxBase _GetMailbox(string mailboxAddress);

        public override void UpdateMailbox(IMailbox mailbox)
        {
            _UpdateMailbox(new MailboxBase(mailbox));
        }

        protected abstract void _UpdateMailbox(MailboxBase mailbox);

        public override void DeleteMailbox(IMailbox mailbox)
        {
            if (mailbox == null)
                throw new ArgumentNullException("mailbox", "ServerModel::DeleteMailbox");

            using (var dbContextWithTran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                TeamlabMailboxDal.DeleteMailbox(mailbox.Id, dbContextWithTran.DbManager);
                _DeleteMailbox(new MailboxBase(mailbox));
                dbContextWithTran.CommitTransaction();
            }

        }

        protected abstract void _DeleteMailbox(MailboxBase mailbox);

        #endregion

        #region .Groups

        public override IMailGroup CreateMailGroup(string groupName, IWebDomain domain, List<int> addressIds, IMailServerFactory factory)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentNullException("groupName", "ServerModel::CreateMailGroup");

            if (domain == null)
                throw new ArgumentNullException("domain");

            if (addressIds == null)
                throw new ArgumentNullException("addressIds");

            if(!addressIds.Any())
                throw new ArgumentException("Empty address_ids list");

            if (factory == null)
                throw new ArgumentNullException("factory");

            MailGroupDto mailgroupDto;

            using (var dbContextWithTran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                var addressDtoList = TeamlabMailAddressDal.GetMailAddresses(addressIds, dbContextWithTran.DbManager);

                var addressBaseList =
                    addressDtoList
                        .Select(dto =>
                                new MailAddressBase(dto.name,
                                                    new WebDomainBase(dto.domain.name)))
                        .ToList();

                var mailgroupBase = new MailGroupBase(new MailAddressBase(groupName, new WebDomainBase(domain)),
                                                       addressBaseList);
                mailgroupDto = TeamlabMailGroupDal.SaveMailGroup(mailgroupBase.Address.LocalPart,
                                                                  mailgroupBase.Address.DateCreated,
                                                                  domain.Id,
                                                                  domain.Name,
                                                                  domain.IsVerified,
                                                                  addressDtoList, dbContextWithTran.DbManager);
                _CreateMailGroup(mailgroupBase.Address, addressBaseList);

                dbContextWithTran.CommitTransaction();
            }

            var mailgroupAddress = factory.CreateMailAddress(mailgroupDto.address.id, mailgroupDto.address.tenant, mailgroupDto.address.name, domain);

            var inAddresses =
                mailgroupDto.addresses.Select(
                    addressDto => factory.CreateMailAddress(addressDto.id, addressDto.tenant, addressDto.name, domain))
                               .ToList();

            var mailgroup = factory.CreateMailGroup(mailgroupDto.id, mailgroupDto.id_tenant, 
                mailgroupAddress, inAddresses.ToList(), this);

            return mailgroup;
        }

        protected abstract MailGroupBase _CreateMailGroup(MailAddressBase address, List<MailAddressBase> mailboxAddressList);

        public override ICollection<IMailGroup> GetMailGroups(IMailServerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var tlGroups = TeamlabMailGroupDal.GetMailGroups();

            var serverGroups = _GetMailGroups(tlGroups.Select(g => g.address.ToString()).ToList());

            return (serverGroups.Select(
                serverGroup => tlGroups.First(g => g.address.ToString() == serverGroup.Address.ToString()))
                                .Select(tlGroup => new
                                    {
                                        tlGroup,
                                        domain =
                                                       factory.CreateWebDomain(tlGroup.address.domain.id,
                                                                               tlGroup.address.domain.tenant,
                                                                               tlGroup.address.domain.name,
                                                                               tlGroup.address.domain.is_virified, this)
                                    }).Select(@t => new
                                        {
                                            @t,
                                            address =
                                                        factory.CreateMailAddress(@t.tlGroup.address.id,
                                                                                  @t.tlGroup.address.tenant,
                                                                                  @t.tlGroup.address.name,
                                                                                  @t.domain)
                                        }).Select(@t => new
                                            {
                                                @t,
                                                groupInAddresses =
                                                            @t.@t.tlGroup.addresses.Select(
                                                                a =>
                                                                factory.CreateMailAddress(a.id, a.tenant, a.name,
                                                                                          @t.@t.domain))
                                                              .ToList()
                                            })
                                .Select(
                                    @t =>
                                    factory.CreateMailGroup(@t.@t.@t.tlGroup.id, @t.@t.@t.tlGroup.id_tenant,
                                                            @t.@t.address, @t.groupInAddresses.ToList(), this)))
                .ToList();
        }

        protected abstract ICollection<MailGroupBase> _GetMailGroups(ICollection<string> mailgroupsAddresses);

        public override IMailGroup GetMailGroup(int mailgroupId, IMailServerFactory factory)
        {
            if (mailgroupId < 0)
                throw new ArgumentException("mailgroup_id has negative value");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var mailgroupDto = TeamlabMailGroupDal.GetMailGroup(mailgroupId);

            if (mailgroupDto == null)
                return null;

            var mailgroupBase = _GetMailGroup(mailgroupDto.address.ToString());

            if (mailgroupBase == null)
                throw new Exception("Mailgroup is missing on server");

            var mailgroupDomain = factory.CreateWebDomain(mailgroupDto.address.domain.id, mailgroupDto.address.domain.tenant, mailgroupDto.address.domain.name, mailgroupDto.address.domain.is_virified, this);

            var mailgroupAddress = factory.CreateMailAddress(mailgroupDto.address.id, mailgroupDto.address.tenant, mailgroupDto.address.name, mailgroupDomain);

            var mailgroupAddresses =
                mailgroupDto.addresses
                             .Select(alias =>
                                     factory.CreateMailAddress(alias.id, alias.tenant, alias.name, mailgroupDomain))
                             .ToList();

            var mailgroup = factory.CreateMailGroup(mailgroupDto.id, mailgroupDto.id_tenant, 
                mailgroupAddress, mailgroupAddresses.ToList(), this);

            return mailgroup;
        }

        public abstract MailGroupBase _GetMailGroup(string mailgroupAddress);

        public override void DeleteMailGroup(int mailgroupId, IMailServerFactory factory)
        {
            if (mailgroupId < 0)
                throw new ArgumentException("mailgroup_id has negative value");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var mailgroup = GetMailGroup(mailgroupId, factory);

            if (mailgroup == null)
                throw new ArgumentException("Mailgroup is missing");

            using (var dbContextWithTran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                TeamlabMailGroupDal.DeleteMailGroup(mailgroup.Id, dbContextWithTran.DbManager);
                _DeleteMailGroup(new MailGroupBase(mailgroup));
                dbContextWithTran.CommitTransaction();
            }
        }

        protected abstract MailGroupBase _DeleteMailGroup(MailGroupBase mailGroup);

        #endregion

        #region .DNS

        public override IDnsSettings GetFreeDnsRecords(IMailServerFactory factory)
        {
            var dnsDto = TeamlabDnsDal.GetFreeDnsRecords() ??
                TeamlabDnsDal.CreateFreeDnsRecords(SetupInfo.DnsPresets.DkimSelector, SetupInfo.DnsPresets.DomainCheckPrefix, SetupInfo.DnsPresets.SpfValue);

            var txtName = SetupInfo.DnsPresets.CurrentOrigin;
            var dns = factory.CreateDnsSettings(dnsDto.id, dnsDto.tenant, dnsDto.user, "", dnsDto.dkim_selector,
                                                dnsDto.dkim_private_key, dnsDto.dkim_public_key, txtName,
                                                dnsDto.domain_chek, txtName, dnsDto.spf, 
                                                SetupInfo.DnsPresets.MxHost, SetupInfo.DnsPresets.MxPriority);

           return dns;
        }

        #endregion

        #region .Notification

        public override INotificationAddress CreateNotificationAddress(string localpart, string password, IWebDomain domain,
                                                               IMailServerFactory factory)
        {
            if (string.IsNullOrEmpty(localpart))
                throw new ArgumentNullException("localpart");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            if (domain == null)
                throw new ArgumentNullException("domain");

            if (localpart.Length + domain.Name.Length > 318) // 318 because of @ sign
                throw new ArgumentException("Address of mailbox exceed limitation of 319 characters.", "localpart");

            var notificationAddressBase = new MailAddressBase(localpart, new WebDomainBase(domain));

            var address = notificationAddressBase.ToString();

            var smtpSettings = TeamlabServerDal.GetTenantServerSettings().FirstOrDefault(s => s.type == ServerType.Smtp);

            if (smtpSettings == null)
                throw new Exception("No smtp settings.");

            var smtpLogin = smtpSettings.smtpLoginFormat.Replace("%EMAILADDRESS%", address)
                         .Replace("%EMAILLOCALPART%", localpart)
                         .Replace("%EMAILDOMAIN%", notificationAddressBase.Domain.Name.ToLowerInvariant())
                         .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(notificationAddressBase.Domain.Name.ToLowerInvariant()));

            var notificationAddressSettings =
                                SettingsManager.Instance.LoadSettings<NotificationAddressSettings>(Tenant);

            if (!string.IsNullOrEmpty(notificationAddressSettings.NotificationAddress))
                throw new Exception("Setting already exists. Remove first.");

            _CreateNotificationAddress(address, password, localpart, domain.Name);

            notificationAddressSettings = new NotificationAddressSettings {NotificationAddress = address};

            if (!SettingsManager.Instance.SaveSettings(notificationAddressSettings, Tenant))
            {
                _DeleteNotificationAddress(address);
                throw new Exception("Could not save notification address setting.");
            }

            var notificationAddress =
                                factory.CreateNotificationAddress(localpart, domain, smtpSettings.hostname, smtpSettings.port,
                                                                  smtpLogin,
                                                                  true, smtpSettings.socket_type, smtpSettings.authentication_type);

            return notificationAddress;
        }

        protected abstract MailboxBase _CreateNotificationAddress(string login, string password, string localpart, string domain);

        public override void DeleteNotificationAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address", "ServerModel::DeleteNotificationAddress");

            var deleteAddress = address.ToLowerInvariant();
            var notificationAddressSettings =
                                SettingsManager.Instance.LoadSettings<NotificationAddressSettings>(Tenant);

            if (notificationAddressSettings.NotificationAddress != deleteAddress)
                throw new ArgumentException("Mailbox not exists");

            _DeleteNotificationAddress(address);

            if (!SettingsManager.Instance.SaveSettings(notificationAddressSettings.GetDefault(), Tenant))
            {
                throw new Exception("Could not delete notification address setting.");
            }
        }

        protected abstract void _DeleteNotificationAddress(string address);

        #endregion
    }
}
