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
using System.Linq;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public abstract class MailGroupModel : MailGroupBase, IMailGroup
    {
        private MailGroupDal _mailgroupDal;
        private MailAddressDal _addressDal;

        public int Id { get; private set; }
        public int Tenant { get; private set; }

        public MailGroupDal TeamlabMailGroupDal
        {
            get { return _mailgroupDal ?? (_mailgroupDal = new MailGroupDal(Tenant)); }
        }

        public MailAddressDal TeamlabAddressDal
        {
            get { return _addressDal ?? (_addressDal = new MailAddressDal(Tenant)); }
        }

        public new IMailAddress Address { get; private set; }

        public new ICollection<IMailAddress> InAddresses { get; private set; }

        public MailServerBase Server { get; private set; }

        protected MailGroupModel(int id, int tenant, IMailAddress address, List<IMailAddress> in_addresses, MailServerBase server)
            : base(new MailAddressBase(address), (in_addresses.Select(a => new MailAddressBase(a)).ToList()))
        {
            if (id < 0)
                throw new ArgumentException("Invalid domain id", "id");

            if (tenant < 0)
                throw new ArgumentException("Invalid tenant id", "tenant");

            if (address == null)
                throw new ArgumentException("Invalid address", "address");

            if (in_addresses == null)
                throw new ArgumentException("Invalid aliases", "in_addresses");

            if (server == null)
                throw new ArgumentException("Invalid server", "server");

            Id = id;
            Tenant = tenant;
            Address = address;
            InAddresses = in_addresses;
            Server = server;
        }

        public void AddMember(int mailbox_address_id, IMailServerFactory factory)
        {
            if (mailbox_address_id < 0)
                throw new ArgumentException("Negative parameter value", "mailbox_address_id");

            if (factory == null)
                throw new ArgumentNullException("factory");

            MailAddressDto address_dto;
            using (var db_context_with_tran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                address_dto = TeamlabAddressDal.GetMailAddress(mailbox_address_id, db_context_with_tran.DbManager);

                if (address_dto == null)
                    throw new ArgumentException("Address not exists");

                if (InAddresses.Any(addr => addr.Id == mailbox_address_id))
                    throw new ArgumentException("Address already exists");

                var mailbox_address = new MailAddressBase(address_dto.name, new WebDomainBase(address_dto.domain.name));


                TeamlabMailGroupDal.AddAddressToMailGroup(Id, mailbox_address_id, db_context_with_tran.DbManager);
                _AddMember(mailbox_address);
                db_context_with_tran.CommitTransaction();
            }


            InAddresses.Add(factory.CreateMailAddress(address_dto.id, address_dto.tenant, address_dto.name,
                                                      factory.CreateWebDomain(address_dto.domain.id,
                                                                              address_dto.domain.tenant,
                                                                              address_dto.domain.name,
                                                                              address_dto.domain.is_virified,
                                                                              Server)));
        }

        protected abstract void _AddMember(MailAddressBase address);

        public ICollection<IMailAddress> GetMembers(IMailServerFactory factory)
        {
            if(factory == null)
                throw new ArgumentNullException("factory");
            
            var server_addresses = _GetMembers();
            var tl_addresses = TeamlabMailGroupDal.GetGroupAddresses(Id);

            return (from tl_address in tl_addresses
                    let address_for_update = server_addresses.FirstOrDefault(a => a.ToString() == tl_address.ToString())
                    where address_for_update != null
                    let domain =
                        factory.CreateWebDomain(tl_address.domain.id, tl_address.domain.tenant, tl_address.domain.name, tl_address.domain.is_virified, Server)
                    select factory.CreateMailAddress(tl_address.id, tl_address.tenant, tl_address.name, domain))
                .ToList();
        }

        protected abstract ICollection<MailAddressBase> _GetMembers();

        public void RemoveMember(int mailbox_address_id)
        {
            if (mailbox_address_id < 0)
                throw new ArgumentException("Negative parameter value", "mailbox_address_id");

            using (var db_context_with_tran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                var address_dto = TeamlabAddressDal.GetMailAddress(mailbox_address_id, db_context_with_tran.DbManager);

                if (address_dto == null)
                    throw new ArgumentException("Address not exists");

                var mailbox_address = new MailAddressBase(address_dto.name, new WebDomainBase(address_dto.domain.name));

                TeamlabMailGroupDal.DeleteAddressFromMailGroup(Id, mailbox_address_id, db_context_with_tran.DbManager);
                _RemoveMember(mailbox_address);

                db_context_with_tran.CommitTransaction();
            }
        }

        protected abstract void _RemoveMember(MailAddressBase address);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other_group = (MailGroupModel)obj;

            return Id == other_group.Id && Tenant == other_group.Tenant && Address.Equals(other_group.Address);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode() ^ Id ^ Tenant;
        }
    }
}
