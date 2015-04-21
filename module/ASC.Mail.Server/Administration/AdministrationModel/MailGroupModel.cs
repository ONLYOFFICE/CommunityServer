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

        protected MailGroupModel(int id, int tenant, IMailAddress address, List<IMailAddress> inAddresses, MailServerBase server)
            : base(new MailAddressBase(address), (inAddresses.Select(a => new MailAddressBase(a)).ToList()))
        {
            if (id < 0)
                throw new ArgumentException("Invalid domain id", "id");

            if (tenant < 0)
                throw new ArgumentException("Invalid tenant id", "tenant");

            if (address == null)
                throw new ArgumentException("Invalid address", "address");

            if (inAddresses == null)
                throw new ArgumentException("Invalid aliases", "inAddresses");

            if (server == null)
                throw new ArgumentException("Invalid server", "server");

            Id = id;
            Tenant = tenant;
            Address = address;
            InAddresses = inAddresses;
            Server = server;
        }

        public void AddMember(int mailboxAddressId, IMailServerFactory factory)
        {
            if (mailboxAddressId < 0)
                throw new ArgumentException("Negative parameter value", "mailboxAddressId");

            if (factory == null)
                throw new ArgumentNullException("factory");

            MailAddressDto addressDto;
            using (var dbContextWithTran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                addressDto = TeamlabAddressDal.GetMailAddress(mailboxAddressId, dbContextWithTran.DbManager);

                if (addressDto == null)
                    throw new ArgumentException("Address not exists");

                if(Address.Domain.Tenant != addressDto.domain.tenant)
                    throw new ArgumentException("Address not belongs to this domain");

                if (InAddresses.Any(addr => addr.Id == mailboxAddressId))
                    throw new ArgumentException("Address already exists");

                var mailboxAddress = new MailAddressBase(addressDto.name, new WebDomainBase(addressDto.domain.name));

                TeamlabMailGroupDal.AddAddressToMailGroup(Id, mailboxAddressId, dbContextWithTran.DbManager);
                _AddMember(mailboxAddress);
                dbContextWithTran.CommitTransaction();
            }

            InAddresses.Add(factory.CreateMailAddress(addressDto.id, addressDto.tenant, addressDto.name,
                                                      factory.CreateWebDomain(addressDto.domain.id,
                                                                              addressDto.domain.tenant,
                                                                              addressDto.domain.name,
                                                                              addressDto.domain.is_virified,
                                                                              Server)));
        }

        protected abstract void _AddMember(MailAddressBase address);

        public ICollection<IMailAddress> GetMembers(IMailServerFactory factory)
        {
            if(factory == null)
                throw new ArgumentNullException("factory");
            
            var serverAddresses = _GetMembers();
            var tlAddresses = TeamlabMailGroupDal.GetGroupAddresses(Id);

            return (from tlAddress in tlAddresses
                    let addressForUpdate = serverAddresses.FirstOrDefault(a => a.ToString() == tlAddress.ToString())
                    where addressForUpdate != null
                    let domain =
                        factory.CreateWebDomain(tlAddress.domain.id, tlAddress.domain.tenant, tlAddress.domain.name, tlAddress.domain.is_virified, Server)
                    select factory.CreateMailAddress(tlAddress.id, tlAddress.tenant, tlAddress.name, domain))
                .ToList();
        }

        protected abstract ICollection<MailAddressBase> _GetMembers();

        public void RemoveMember(int mailboxAddressId)
        {
            if (mailboxAddressId < 0)
                throw new ArgumentException("Negative parameter value", "mailboxAddressId");

            using (var dbContextWithTran = TeamlabMailGroupDal.CreateMailDbContext(true))
            {
                var addressDto = TeamlabAddressDal.GetMailAddress(mailboxAddressId, dbContextWithTran.DbManager);

                if (addressDto == null)
                    throw new ArgumentException("Address not exists");

                var mailboxAddress = new MailAddressBase(addressDto.name, new WebDomainBase(addressDto.domain.name));

                TeamlabMailGroupDal.DeleteAddressFromMailGroup(Id, mailboxAddressId, dbContextWithTran.DbManager);
                _RemoveMember(mailboxAddress);

                dbContextWithTran.CommitTransaction();
            }
        }

        protected abstract void _RemoveMember(MailAddressBase address);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var otherGroup = (MailGroupModel)obj;

            return Id == otherGroup.Id && Tenant == otherGroup.Tenant && Address.Equals(otherGroup.Address);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode() ^ Id ^ Tenant;
        }
    }
}
