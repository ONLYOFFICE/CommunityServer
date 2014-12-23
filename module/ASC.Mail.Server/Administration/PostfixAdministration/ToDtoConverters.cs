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
using ASC.Mail.Server.Utils;
using ASC.Mail.Server.Administration.Interfaces;


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

        public const int mail_address_columns_count = 6;

        public static PostfixMailAddressDto ToAddressDto(this object[] db_record)
        {
            if (db_record.Length != mail_address_columns_count) throw new ArgumentException("Can't convert to PostfixMailAddress invalid column num");

            var new_address = new PostfixMailAddressDto(
                db_record[(int)AddressTableColumnsOrder.Address].ToString(),
                db_record[(int)AddressTableColumnsOrder.Redirect].ToString(),
                db_record[(int)AddressTableColumnsOrder.DomainName].ToString(),
                Convert.ToBoolean(db_record[(int)AddressTableColumnsOrder.Actove])
           );

            if (new_address.redirect.Contains(","))
                throw new InvalidCastException(String.Format("This value can't be converted to MailAddress: {0}", new_address.redirect));

            return new_address;
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

        public const int domain_columns_count = 11;

        public static PostfixWebDomainDto ToWebDomainDto(this object[] db_record)
        {
            if (db_record.Length != domain_columns_count) throw new ArgumentException("Can't convert to PostfixWebDomain invalid column num");

            return new PostfixWebDomainDto(
                db_record[(int)WebDomainColumnsOrder.Name].ToString(),
                db_record[(int)WebDomainColumnsOrder.Description].ToString(),
                Convert.ToInt32(db_record[(int)WebDomainColumnsOrder.Aliases]),
                Convert.ToInt32(db_record[(int)WebDomainColumnsOrder.Mailboxes]),
                Convert.ToInt64(db_record[(int)WebDomainColumnsOrder.Maxquta]),
                Convert.ToInt64(db_record[(int)WebDomainColumnsOrder.Quta]),
                db_record[(int)WebDomainColumnsOrder.Transport].ToString(),
                Convert.ToBoolean(db_record[(int)WebDomainColumnsOrder.Backupmx]),
                Convert.ToBoolean(db_record[(int)WebDomainColumnsOrder.Active])
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

        public const int mailbox_columns_count = 10;

        public static PostfixMailboxDto ToMailboxDto(this object[] db_record)
        {
            if (db_record.Length != mailbox_columns_count) throw new ArgumentException("Can't convert to PostfixMailbox invalid column num");

            return new PostfixMailboxDto(
                db_record[(int)MailboxColumnsOrder.Username].ToString(),
                db_record[(int)MailboxColumnsOrder.Name].ToString(),
                db_record[(int)MailboxColumnsOrder.Maildir].ToString(),
                Convert.ToInt64(db_record[(int)MailboxColumnsOrder.Quota]),
                db_record[(int)MailboxColumnsOrder.LocalPart].ToString(),
                db_record[(int)MailboxColumnsOrder.Domain].ToString(),
                Convert.ToBoolean(db_record[(int)MailboxColumnsOrder.Active])
            );
        }

        public static PostfixMailgroupDto ToMailgroupDto(this object[] db_record)
        {
            if (db_record.Length != mail_address_columns_count + domain_columns_count) throw new ArgumentException("Can't convert to PostfixMailbox invalid column num");

            var add = new PostfixMailAddressDto(
                db_record[(int)AddressTableColumnsOrder.Address].ToString(),
                db_record[(int)AddressTableColumnsOrder.Redirect].ToString(),
                db_record[(int)AddressTableColumnsOrder.DomainName].ToString(),
                Convert.ToBoolean(db_record[(int)AddressTableColumnsOrder.Actove])
            );
            add.Domain = db_record.SubArray(mail_address_columns_count, domain_columns_count).ToWebDomainDto();

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

        public const int dkim_columns_count = 5;

        public static PostfixDkimDto ToDkimDto(this object[] db_record)
        {
            if (db_record.Length != dkim_columns_count) 
                throw new ArgumentException("Can't convert to PostfixMailbox invalid column num");

            var dkim_dto = new PostfixDkimDto(
                Convert.ToInt32(db_record[(int)DkimTableColumnsOrder.Id]),
                db_record[(int)DkimTableColumnsOrder.DomainName].ToString(),
                db_record[(int)DkimTableColumnsOrder.Selector].ToString(),
                db_record[(int)DkimTableColumnsOrder.PrivateKey].ToString(),
                db_record[(int)DkimTableColumnsOrder.PublicKey].ToString()
            );

            return dkim_dto;
        }
    }
}
