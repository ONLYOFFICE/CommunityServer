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

            var mailboxes_dto = _mailboxDal.GetMailboxes();
            foreach (var mailbox_dto in mailboxes_dto)
            {
                var alias_list =
                    _mailaddressDal.GetMailboxAliases(mailbox_dto.mailbox.id)
                                   .Select(
                                       alias_dto =>
                                       new MailAddressBase(alias_dto.name, new WebDomainBase(alias_dto.domain.name)))
                                   .ToList();

                var result_mailbox = new MailboxBase(
                    new MailAccountBase(mailbox_dto.mailbox.address),
                    //Its not login. It adress. Needed only for testing
                    new MailAddressBase(mailbox_dto.mailbox_address.name,
                                        new WebDomainBase(mailbox_dto.mailbox_address.domain.name)), alias_list);

                _serverData.Mailboxes.Add(result_mailbox);
            }

            var groups_dto = _mailgroupDal.GetMailGroups();
            foreach (var group_dto in groups_dto)
            {
                var group_addresses = group_dto.addresses.Select(address =>
                                                                 new MailAddressBase(address.name,
                                                                                     new WebDomainBase(
                                                                                         address.domain.name))).ToList();

                var result_group = new MailGroupBase(
                    new MailAddressBase(group_dto.address.name,
                                        new WebDomainBase(group_dto.address.domain.name)), group_addresses);

                _serverData.Groups.Add(result_group);
            }
        }

        #region .Domains

        protected override WebDomainBase _CreateWebDomain(string name)
        {
            if (_serverData.Domains.Any(d => d.Name == name))
            {
                throw new ArgumentException("Already added");
            }

            var result_domain = new WebDomainBase (name);
            _serverData.Domains.Add(result_domain);
            return result_domain;
        }

        protected override void _DeleteWebDomain(WebDomainBase web_domain)
        {
            _serverData.Domains.Remove(web_domain);
        }

        protected override List<WebDomainBase> _GetWebDomains(ICollection<string> domain_names)
        {
            return _serverData.Domains.FindAll(d => domain_names.Contains(d.Name));
        }

        protected override WebDomainBase _GetWebDomain(string domain_name)
        {
            return _serverData.Domains.FirstOrDefault(d => d.Name == domain_name);
        }

        #endregion

        #region .Mailboxes

        protected override MailboxBase _CreateMailbox(string login, string password, string localpart, string domain)
        {
            var result_mailbox = new MailboxBase(new MailAccountBase(login),
                                   new MailAddressBase(localpart, new WebDomainBase(domain)),
                                   new List<MailAddressBase>());

            if (_serverData.Mailboxes.Any(r => r.Address.ToString().Equals(result_mailbox.Address.ToString())))
            {
                throw new DuplicateNameException("You want to create mailbox with already existing address");
            }

            _serverData.Mailboxes.Add(result_mailbox);
            return result_mailbox;
        }

        protected override List<MailboxBase> _GetMailboxes(ICollection<string> mailbox_names)
        {
            return _serverData.Mailboxes.FindAll(m => mailbox_names.Contains(m.Address.ToString()));
        }

        public override MailboxBase _GetMailbox(string mailbox_address)
        {
            return _serverData.Mailboxes.FirstOrDefault(r => r.Address.ToString().Equals(mailbox_address));
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

        public override MailGroupBase _GetMailGroup(string mailgroup_address)
        {
            return _serverData.Groups.FirstOrDefault(g => g.Address.ToString().Equals(mailgroup_address));
        }

        protected override MailGroupBase _CreateMailGroup(MailAddressBase address, List<MailAddressBase> in_addresses)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address", "MockServer::_CreateMailGroup");
            }

            if (_serverData.Groups.Any(r => r.Address.ToString().Equals(address.ToString())))
            {
                throw new ArgumentException();
            }

            var result_group =  new MailGroupBase(address, in_addresses);
            _serverData.Groups.Add(result_group);
            return result_group;
        }

        protected override MailGroupBase _DeleteMailGroup(MailGroupBase mail_group)
        {
            _serverData.Groups.Remove(mail_group);
            return mail_group;
        }

        protected override ICollection<MailGroupBase> _GetMailGroups(ICollection<string> mail_group_addresses)
        {
            return _serverData.Groups.FindAll(g => mail_group_addresses.Contains(g.Address.ToString()));
        }

        #endregion
    }
}