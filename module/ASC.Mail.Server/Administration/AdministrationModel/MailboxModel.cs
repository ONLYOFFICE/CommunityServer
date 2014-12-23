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
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.Utils;

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

        protected MailboxModel(int id, int tenant, IMailAddress address, IMailAccount account,
                            ICollection<IMailAddress> aliases, MailServerBase server)
            : base(new MailAccountBase(account), new MailAddressBase(address), (aliases.Select(a => new MailAddressBase(a)).ToList()))
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

        public IMailAddress AddAlias(string alias_name, IWebDomain domain, IMailServerFactory factory)
        {
            if (Aliases.Any(a => a.LocalPart == alias_name && a.Domain.Id == domain.Id))
                throw new DuplicateNameException("You want to add already existed alias");

            var address_base = new MailAddressBase(alias_name, new WebDomainBase(domain))
                {
                    DateCreated = DateTime.UtcNow.ToDbStyle()
                };

            MailAddressDto alias_dto;
            using (var db_context_with_tran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                if (TeamlabAddressDal.IsAddressAlreadyRegistered(alias_name, domain.Name, db_context_with_tran.DbManager))
                    throw new DuplicateNameException("You want to add already existed alias");

                alias_dto = TeamlabAddressDal.AddMailboxAlias(Id, alias_name, address_base.DateCreated,
                                                              domain.Id, domain.Name, domain.IsVerified, db_context_with_tran.DbManager);
                _AddAlias(address_base);

                db_context_with_tran.CommitTransaction();
            }

            var alias = factory.CreateMailAddress(alias_dto.id, alias_dto.tenant, address_base.LocalPart, domain);

            Aliases.Add(alias);

            return alias;
        }

        protected abstract void _AddAlias(MailAddressBase alias_to_add);

        public void RemoveAlias(int alias_id)
        {
            if (Aliases.Count <= 0) 
                return;

            var alias_to_remove = Aliases.FirstOrDefault(a => a.Id == alias_id);

            if (alias_to_remove == null) 
                return;

            using (var db_context_with_tran = TeamlabMailboxDal.CreateMailDbContext(true))
            {
                TeamlabAddressDal.RemoveMailboxAlias(alias_to_remove.Id, db_context_with_tran.DbManager);
                _RemoveAlias(new MailAddressBase(alias_to_remove));
                db_context_with_tran.CommitTransaction();
            }

            Aliases.Remove(alias_to_remove);
        }

        protected abstract void _RemoveAlias(MailAddressBase alias_to_remove);

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
            return Account.GetHashCode() ^ Address.GetHashCode() ^ Aliases.GetHashCode() ^ Id ^ Tenant ^ Server.GetHashCode();
        }
    }
}
