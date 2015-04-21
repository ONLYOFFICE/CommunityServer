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
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.PostfixAdministration
{
    public static class ToDtoConverters
    {
        private enum AddressTableColumnsOrder
        {
            Address = 0,
            Redirect = 1,
            DomainName = 2,
            CreatedDate = 3,
            ModifiedDate = 4,
            Actove = 5
        }

        public const int MAIL_ADDRESS_COLUMNS_COUNT = 6;

        public static PostfixMailAddressDto ToAddressDto(this object[] dbRecord)
        {
            if (dbRecord.Length != MAIL_ADDRESS_COLUMNS_COUNT) throw new ArgumentException("Can't convert to PostfixMailAddress invalid column num");

            var newAddress = new PostfixMailAddressDto(
                dbRecord[(int)AddressTableColumnsOrder.Address].ToString(),
                dbRecord[(int)AddressTableColumnsOrder.Redirect].ToString(),
                dbRecord[(int)AddressTableColumnsOrder.DomainName].ToString(),
                Convert.ToBoolean(dbRecord[(int)AddressTableColumnsOrder.Actove])
           );

            if (newAddress.redirect.Contains(","))
                throw new InvalidCastException(String.Format("This value can't be converted to MailAddress: {0}", newAddress.redirect));

            return newAddress;
        }


        private enum WebDomainColumnsOrder
        {
            Name = 0,
            Description = 1,
            Aliases = 2,
            Mailboxes = 3,
            Maxquta = 4,
            Quta = 5,
            Transport = 6,
            Backupmx = 7,
            CreatedDate = 8,
            ModifiedDate = 9,
            Active = 10
        }

        public const int DOMAIN_COLUMNS_COUNT = 11;

        public static PostfixWebDomainDto ToWebDomainDto(this object[] dbRecord)
        {
            if (dbRecord.Length != DOMAIN_COLUMNS_COUNT) throw new ArgumentException("Can't convert to PostfixWebDomain invalid column num");

            return new PostfixWebDomainDto(
                dbRecord[(int)WebDomainColumnsOrder.Name].ToString(),
                dbRecord[(int)WebDomainColumnsOrder.Description].ToString(),
                Convert.ToInt32(dbRecord[(int)WebDomainColumnsOrder.Aliases]),
                Convert.ToInt32(dbRecord[(int)WebDomainColumnsOrder.Mailboxes]),
                Convert.ToInt64(dbRecord[(int)WebDomainColumnsOrder.Maxquta]),
                Convert.ToInt64(dbRecord[(int)WebDomainColumnsOrder.Quta]),
                Convert.ToBoolean(dbRecord[(int)WebDomainColumnsOrder.Backupmx]),
                Convert.ToBoolean(dbRecord[(int)WebDomainColumnsOrder.Active])
           );
        }

        private enum MailboxColumnsOrder
        {
            Username = 0,
            Password = 1,
            Name = 2,
            Maildir = 3,
            Quota = 4,
            LocalPart = 5,
            Domain = 6,
            CreatedDate  = 7,
            ModifiedDate = 8,
            Active = 9
        }

        public const int MAILBOX_COLUMNS_COUNT = 10;

        public static PostfixMailboxDto ToMailboxDto(this object[] dbRecord)
        {
            if (dbRecord.Length != MAILBOX_COLUMNS_COUNT) throw new ArgumentException("Can't convert to PostfixMailbox invalid column num");

            return new PostfixMailboxDto(
                dbRecord[(int)MailboxColumnsOrder.Username].ToString(),
                dbRecord[(int)MailboxColumnsOrder.Name].ToString(),
                dbRecord[(int)MailboxColumnsOrder.Maildir].ToString(),
                Convert.ToInt64(dbRecord[(int)MailboxColumnsOrder.Quota]),
                dbRecord[(int)MailboxColumnsOrder.LocalPart].ToString(),
                dbRecord[(int)MailboxColumnsOrder.Domain].ToString(),
                Convert.ToBoolean(dbRecord[(int)MailboxColumnsOrder.Active])
            );
        }

        public static PostfixMailgroupDto ToMailgroupDto(this object[] dbRecord)
        {
            if (dbRecord.Length != MAIL_ADDRESS_COLUMNS_COUNT + DOMAIN_COLUMNS_COUNT) throw new ArgumentException("Can't convert to PostfixMailbox invalid column num");

            var add = new PostfixMailAddressDto(
                dbRecord[(int) AddressTableColumnsOrder.Address].ToString(),
                dbRecord[(int) AddressTableColumnsOrder.Redirect].ToString(),
                dbRecord[(int) AddressTableColumnsOrder.DomainName].ToString(),
                Convert.ToBoolean(dbRecord[(int) AddressTableColumnsOrder.Actove])
                ) {Domain = dbRecord.SubArray(MAIL_ADDRESS_COLUMNS_COUNT, DOMAIN_COLUMNS_COUNT).ToWebDomainDto()};

            return new PostfixMailgroupDto(add);
        }

        private enum DkimTableColumnsOrder
        {
            Id = 0,
            DomainName = 1,
            Selector = 2,
            PrivateKey = 3,
            PublicKey = 4
        }

        public const int DKIM_COLUMNS_COUNT = 5;

        public static PostfixDkimDto ToDkimDto(this object[] dbRecord)
        {
            if (dbRecord.Length != DKIM_COLUMNS_COUNT) 
                throw new ArgumentException("Can't convert to PostfixMailbox invalid column num");

            var dkimDto = new PostfixDkimDto(
                Convert.ToInt32(dbRecord[(int)DkimTableColumnsOrder.Id]),
                dbRecord[(int)DkimTableColumnsOrder.DomainName].ToString(),
                dbRecord[(int)DkimTableColumnsOrder.Selector].ToString(),
                dbRecord[(int)DkimTableColumnsOrder.PrivateKey].ToString(),
                dbRecord[(int)DkimTableColumnsOrder.PublicKey].ToString()
            );

            return dkimDto;
        }
    }
}
