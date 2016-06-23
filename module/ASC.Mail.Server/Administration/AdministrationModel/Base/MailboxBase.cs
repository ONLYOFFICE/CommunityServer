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
using System.Linq;
using ASC.Mail.Server.Administration.Interfaces;

namespace ASC.Mail.Server.Administration.ServerModel.Base
{
    public class MailboxBase
    {
        public MailAccountBase Account { get; private set; }

        public MailAddressBase Address { get; private set; }

        public List<MailAddressBase> Aliases { get; private set; }

        public string Name { get; private set; }

        public DateTime DateCreated { get; set; }

        public MailboxBase(MailAccountBase account, MailAddressBase address, string name, List<MailAddressBase> aliases)
        {
            if (account == null)
                throw new ArgumentException("Invalid account", "account");

            if (address == null)
                throw new ArgumentException("Invalid address", "address");

            if (aliases == null)
                throw new ArgumentException("Invalid aliases", "aliases");

            Account = account;
            Address = address;
            Aliases = aliases;
            Name = name;
        }

        public MailboxBase(IMailbox mailbox)
            : this(new MailAccountBase(mailbox.Account), new MailAddressBase(mailbox.Address), mailbox.Name, mailbox.Aliases.Select(a => new MailAddressBase(a)).ToList())
        {
            
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MailboxBase))
            {
                return false;
            }

            var other = (MailboxBase)obj;

            if (!Account.Equals(other.Account) ||
                !Address.Equals(other.Address) ||
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
            return Account.GetHashCode() ^ Address.GetHashCode() ^ Aliases.GetHashCode();
        }
    }
}
