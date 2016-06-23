/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public abstract class MailboxModel : MailboxBase, IMailbox
    {
        private MailboxDal _mailboxDal;
        private MailAddressDal _addressDal;

        public int Id { get; private set; }
        public int Tenant { get; private set; }

        public MailServerBase Server { get; private set; }


        public MailboxDal TeamlabMailboxDal
        {
            get { return _mailboxDal ?? ( _mailboxDal = new MailboxDal(Tenant, Server.SetupInfo.Limits.MailboxMaxCountPerUser)); }
        }

        public MailAddressDal TeamlabAddressDal
        {
            get { return _addressDal ?? (_addressDal = new MailAddressDal(Tenant)); }
        }

        public new IMailAccount Account { get; private set; }

        public new IMailAddress Address { get; private set; }

        public new ICollection<IMailAddress> Aliases { get; private set; }

        protected MailboxModel(int id, int tenant, IMailAddress address, string name, IMailAccount account,
                            ICollection<IMailAddress> aliases, MailServerBase server)
            : base(new MailAccountBase(account), new MailAddressBase(address), name, (aliases.Select(a => new MailAddressBase(a)).ToList()))
        {
            if (id < 0)
                throw new ArgumentException("Invalid domain id", "id");

            if (tenant < 0)
                throw new ArgumentException("Invalid tenant id", "tenant");

            if (account == null)
                throw new ArgumentException("Invalid account", "account");

            if (address == null)
                throw new ArgumentException("Invalid address", "address");

            if (aliases == null)
                throw new ArgumentException("Invalid aliases", "aliases");

            if (server == null)
                throw new ArgumentException("Invalid server", "server");

            Id = id;
            Tenant = tenant;
            Account = account;
            Address = address;
            Aliases = aliases;
            Server = server;
        }

        public IMailAddress AddAlias(string aliasName, IWebDomain domain, IMailServerFactory factory)
        {
            if (Aliases.Any(a => a.LocalPart == aliasName && a.Domain.Id == domain.Id))
                throw new DuplicateNameException("You want to add already existed alias");

            var addressBase = new MailAddressBase(aliasName, new WebDomainBase(domain))
                {
                    DateCreated = DateTime.UtcNow
                };

            MailAddressDto aliasDto;
            using (var dbContextWithTran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                if (TeamlabAddressDal.IsAddressAlreadyRegistered(aliasName, domain.Name, dbContextWithTran.DbManager))
                    throw new DuplicateNameException("You want to add already existed alias");

                aliasDto = TeamlabAddressDal.AddMailboxAlias(Id, aliasName, addressBase.DateCreated,
                                                              domain.Id, domain.Name, domain.IsVerified, dbContextWithTran.DbManager);
                _AddAlias(addressBase);

                dbContextWithTran.CommitTransaction();
            }

            var alias = factory.CreateMailAddress(aliasDto.id, aliasDto.tenant, addressBase.LocalPart, domain);

            Aliases.Add(alias);

            return alias;
        }

        protected abstract void _AddAlias(MailAddressBase aliasToAdd);

        public void RemoveAlias(int aliasId)
        {
            if (Aliases.Count <= 0) 
                return;

            var aliasToRemove = Aliases.FirstOrDefault(a => a.Id == aliasId);

            if (aliasToRemove == null) 
                return;

            using (var dbContextWithTran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                TeamlabAddressDal.RemoveMailboxAlias(aliasToRemove.Id, dbContextWithTran.DbManager);
                _RemoveAlias(new MailAddressBase(aliasToRemove));
                dbContextWithTran.CommitTransaction();
            }

            Aliases.Remove(aliasToRemove);
        }

        protected abstract void _RemoveAlias(MailAddressBase aliasToRemove);

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IMailbox))
            {
                return false;
            }

            var other = (IMailbox)obj;

            if (!Account.Equals(other.Account) ||
                !Address.Equals(other.Address) ||
                !Id.Equals(other.Id) ||
                !Tenant.Equals(other.Tenant) ||
                !Server.Equals(other.Server) ||
                !Name.Equals(other.Name) ||
                Aliases.Count != other.Aliases.Count)
                return false;

            for (var i = 0; i < Aliases.Count; i++)
            {
                if (!other.Aliases.Contains(Aliases.ElementAt(i)))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Account.GetHashCode() ^ Address.GetHashCode() ^ Aliases.GetHashCode() ^ Id ^ Tenant ^ Server.GetHashCode() ^ Name.GetHashCode();
        }
    }
}
