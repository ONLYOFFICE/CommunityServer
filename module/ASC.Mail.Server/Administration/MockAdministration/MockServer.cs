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
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;

namespace ASC.Mail.Server.MockAdministration
{
    public class MockServer : ServerModel
    {
        private readonly MockServerData _serverData;
        private readonly WebDomainDal _domainDal;
        private readonly MailboxDal _mailboxDal;
        private readonly MailGroupDal _mailgroupDal;
        private readonly MailAddressDal _mailaddressDal;

        public MockServer(ServerSetup setup)
            : base(setup)
        {
            _serverData = MockServerData.GetInstance();

            _serverData.Domains.Clear();
            _serverData.Mailboxes.Clear();
            _serverData.Groups.Clear();

            _domainDal = new WebDomainDal(setup.Tenant);
            _mailboxDal = new MailboxDal(setup.Tenant, SetupInfo.Limits.MailboxMaxCountPerUser);
            _mailgroupDal = new MailGroupDal(setup.Tenant);
            _mailaddressDal = new MailAddressDal(setup.Tenant);

            var domains = _domainDal.GetTenantDomains();

            foreach (var domain in domains)
            {
                _serverData.Domains.Add(new WebDomainBase(domain.name));
            }

            var mailboxesDto = _mailboxDal.GetMailboxes();
            foreach (var mailboxDto in mailboxesDto)
            {
                var aliasList =
                    _mailaddressDal.GetMailboxAliases(mailboxDto.mailbox.id)
                                   .Select(
                                       aliasDto =>
                                       new MailAddressBase(aliasDto.name, new WebDomainBase(aliasDto.domain.name)))
                                   .ToList();

                var resultMailbox = new MailboxBase(
                    new MailAccountBase(mailboxDto.mailbox.address),
                    //Its not login. It adress. Needed only for testing
                    new MailAddressBase(mailboxDto.mailbox_address.name,
                                        new WebDomainBase(mailboxDto.mailbox_address.domain.name)), aliasList);

                _serverData.Mailboxes.Add(resultMailbox);
            }

            var groupsDto = _mailgroupDal.GetMailGroups();
            foreach (var resultGroup in from groupDto in groupsDto
                                        let groupAddresses = groupDto.addresses.Select(address =>
                                                                                       new MailAddressBase(address.name,
                                                                                                           new WebDomainBase
                                                                                                               (
                                                                                                               address
                                                                                                                   .domain
                                                                                                                   .name)))
                                                                     .ToList()
                                        select new MailGroupBase(
                                            new MailAddressBase(groupDto.address.name,
                                                                new WebDomainBase(groupDto.address.domain.name)),
                                            groupAddresses))
            {
                _serverData.Groups.Add(resultGroup);
            }
        }

        #region .Domains

        protected override WebDomainBase _CreateWebDomain(string name)
        {
            if (_serverData.Domains.Any(d => d.Name == name))
            {
                throw new ArgumentException("Already added");
            }

            var resultDomain = new WebDomainBase (name);
            _serverData.Domains.Add(resultDomain);
            return resultDomain;
        }

        protected override void _DeleteWebDomain(WebDomainBase webDomain)
        {
            _serverData.Domains.Remove(webDomain);
        }

        protected override List<WebDomainBase> _GetWebDomains(ICollection<string> domainNames)
        {
            return _serverData.Domains.FindAll(d => domainNames.Contains(d.Name));
        }

        protected override WebDomainBase _GetWebDomain(string domainName)
        {
            return _serverData.Domains.FirstOrDefault(d => d.Name == domainName);
        }

        #endregion

        #region .Mailboxes

        protected override MailboxBase _CreateMailbox(string login, string password, string localpart, string domain)
        {
            var resultMailbox = new MailboxBase(new MailAccountBase(login),
                                   new MailAddressBase(localpart, new WebDomainBase(domain)),
                                   new List<MailAddressBase>());

            if (_serverData.Mailboxes.Any(r => r.Address.ToString().Equals(resultMailbox.Address.ToString())))
            {
                throw new DuplicateNameException("You want to create mailbox with already existing address");
            }

            _serverData.Mailboxes.Add(resultMailbox);
            return resultMailbox;
        }

        protected override List<MailboxBase> _GetMailboxes(ICollection<string> mailboxNames)
        {
            return _serverData.Mailboxes.FindAll(m => mailboxNames.Contains(m.Address.ToString()));
        }

        public override MailboxBase _GetMailbox(string mailboxAddress)
        {
            return _serverData.Mailboxes.FirstOrDefault(r => r.Address.ToString().Equals(mailboxAddress));
        }

        protected override void _UpdateMailbox(MailboxBase mailbox)
        {
            throw new NotSupportedException();
        }

        protected override void _DeleteMailbox(MailboxBase mailbox)
        {
            _serverData.Mailboxes.Remove(mailbox);
        }

        #endregion

        #region .MailGroups

        public override MailGroupBase _GetMailGroup(string mailgroupAddress)
        {
            return _serverData.Groups.FirstOrDefault(g => g.Address.ToString().Equals(mailgroupAddress));
        }

        protected override MailGroupBase _CreateMailGroup(MailAddressBase address, List<MailAddressBase> mailboxAddressList)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address", "MockServer::_CreateMailGroup");
            }

            if (_serverData.Groups.Any(r => r.Address.ToString().Equals(address.ToString())))
            {
                throw new ArgumentException();
            }

            var resultGroup =  new MailGroupBase(address, mailboxAddressList);
            _serverData.Groups.Add(resultGroup);
            return resultGroup;
        }

        protected override MailGroupBase _DeleteMailGroup(MailGroupBase mailGroup)
        {
            _serverData.Groups.Remove(mailGroup);
            return mailGroup;
        }

        protected override ICollection<MailGroupBase> _GetMailGroups(ICollection<string> mailgroupsAddresses)
        {
            return _serverData.Groups.FindAll(g => mailgroupsAddresses.Contains(g.Address.ToString()));
        }

        #endregion
    }
}