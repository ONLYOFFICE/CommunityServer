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
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Dal
{
    public static class ToDtoConverters
    {
        private enum ServerTableColumnsOrder
        {
            Id = 0,
            ConnectionString,
            MxRecord,
            ServerType,
            SmtpServerId,
            ImapServerId
        }

        public const int server_columns_count = 6;

        public static TenantServerDto ToTenantServerDto(this object[] db_record)
        {
            if (db_record.Length != server_columns_count)
                throw new InvalidCastException("Can't convert to TenantServerDto. Invalid columns count.");

            return new TenantServerDto(
                (int) db_record[(int) ServerTableColumnsOrder.Id],
                db_record[(int)ServerTableColumnsOrder.ConnectionString].ToString(),
                db_record[(int)ServerTableColumnsOrder.MxRecord].ToString(),
                (int) db_record[(int)ServerTableColumnsOrder.ServerType],
                (int)db_record[(int)ServerTableColumnsOrder.SmtpServerId],
                (int)db_record[(int)ServerTableColumnsOrder.ImapServerId]
           );
        }


        private enum AddressTableColumnsOrder
        {
            Id = 0,
            Tenant = 1,
            Name = 2,
            DomainId = 3,
            MailboxId = 4,
            IsMailGroup = 5,
            IsAlias = 6,
            DateCreated = 7
        }

        public const int mail_address_columns_count = 8;

        public static MailAddressDto ToMailAddressDto(this object[] db_record)
        {
            if (db_record.Length != mail_address_columns_count + domain_columns_count)
                throw new InvalidCastException("Can't convert to MailAddressDto. Invalid columns count.");

            var domain_info =
                db_record.SubArray(mail_address_columns_count, db_record.Length - mail_address_columns_count)
                         .ToWebDomainDto();

            return new MailAddressDto(
                (int)db_record[(int)AddressTableColumnsOrder.Id],
                (int)db_record[(int)AddressTableColumnsOrder.Tenant],
                db_record[(int)AddressTableColumnsOrder.Name].ToString(),
                (int)db_record[(int)AddressTableColumnsOrder.DomainId],
                (int)db_record[(int)AddressTableColumnsOrder.MailboxId],
                Convert.ToBoolean(db_record[(int)AddressTableColumnsOrder.IsMailGroup]),
                Convert.ToBoolean(db_record[(int)AddressTableColumnsOrder.IsAlias]),
                domain_info
           );
        }

        private enum WebDomainColumnsOrder
        {
            Id = 0,
            Name = 1,
            Tenant = 2,
            DateAdded = 3,
            IsVerified = 4
        }

        public const int domain_columns_count = 5;

        public static WebDomainDto ToWebDomainDto(this object[] db_record)
        {
            if (db_record.Length != domain_columns_count)
                throw new InvalidCastException("Can't convert to WebDomainDto. Invalid columns count.");

            return new WebDomainDto(
                (int)db_record[(int)WebDomainColumnsOrder.Id],
                db_record[(int)WebDomainColumnsOrder.Name].ToString(),
                (int)db_record[(int)WebDomainColumnsOrder.Tenant],
                Convert.ToBoolean(db_record[(int)WebDomainColumnsOrder.IsVerified])
           );
        }

        private enum MailboxColumnsOrder
        {
            Id = 0,
            UserId = 1,
            Tenant = 2,
            Address = 3,
            DateCreated = 4
        }

        public const int mailbox_columns_count = 5;

        public static MailboxDto ToMailbox(this object[] db_record)
        {
            if (db_record.Length != mailbox_columns_count)
                throw new InvalidCastException("Can't convert to MailboxDto. Invalid columns count.");

            return new MailboxDto(
                Convert.ToInt32(db_record[(int)MailboxColumnsOrder.Id]),
                db_record[(int)MailboxColumnsOrder.UserId].ToString(),
                (int)db_record[(int)MailboxColumnsOrder.Tenant],
                db_record[(int)MailboxColumnsOrder.Address].ToString()
            );
        }

        public static MailboxWithAddressDto ToMailboxWithAddressDto(this object[] db_record)
        {
            if (db_record.Length != mailbox_columns_count + mail_address_columns_count + domain_columns_count)
                throw new InvalidCastException("Can't convert to MailboxDto. Invalid columns count.");

            var mailbox_dto = db_record.SubArray(0, mailbox_columns_count).ToMailbox();

            var mailaddress_dto = db_record.SubArray(mailbox_columns_count,
                                                     db_record.Length - mailbox_columns_count).ToMailAddressDto();

            return new MailboxWithAddressDto(mailbox_dto, mailaddress_dto);
        }

        private enum MailgroupColumnsOrder
        {
            Id = 0,
            AddressId = 1,
            Tenant = 2,
            DateCreated = 3
        }

        public const int mailgroup_columns_count = 4;

        public static MailGroupDto ToMailGroupDto(this object[] db_record, List<MailAddressDto> address_dtos)
        {
            if (db_record.Length != mailgroup_columns_count + mail_address_columns_count + domain_columns_count)
                throw new InvalidCastException("Can't convert to MailGroupDto. Invalid columns count.");

            var address_dto = db_record.SubArray(mailgroup_columns_count, db_record.Length - mailgroup_columns_count).ToMailAddressDto();

            return new MailGroupDto(
                Convert.ToInt32(db_record[(int) MailgroupColumnsOrder.Id]),
                Convert.ToInt32(db_record[(int) MailgroupColumnsOrder.Tenant]),
                Convert.ToInt32(db_record[(int) MailgroupColumnsOrder.AddressId]),
                address_dto,
                address_dtos);
        }

        private enum CNameXDomainColumnsOrder
        {
            DomainId = 0,
            Cname = 1,
            ReferenceUrl = 2,
            Verified = 3
        }

        public const int cname_x_domain_columns_count = 4;

        public static CNameXDomainDto ToCNameXDomainDto(this object[] db_record)
        {
            if (db_record.Length != cname_x_domain_columns_count)
                throw new InvalidCastException("Can't convert to CNameXDomainDto. Invalid columns count.");

            return new CNameXDomainDto(
                (int)db_record[(int)CNameXDomainColumnsOrder.DomainId],
                db_record[(int)CNameXDomainColumnsOrder.Cname].ToString(),
                db_record[(int)CNameXDomainColumnsOrder.ReferenceUrl].ToString(),
                Convert.ToBoolean(db_record[(int)CNameXDomainColumnsOrder.Verified])
            );
        }

        private enum DnsColumnsOrder
        {
            Id = 0,
            DomainId = 1,
            DkimSelector = 2,
            DkimPrivateKey = 3,
            DkimPublicKey = 4,
            DnsCheckRecord = 5,
            SpfRecord = 6
        }

        public const int dns_columns_count = 7;

        public static DnsDto ToDnsDto(this object[] db_record, int tenant, string user)
        {
            if (db_record.Length != dns_columns_count)
                throw new InvalidCastException("Can't convert to DnsDto. Invalid columns count.");

            return new DnsDto(Convert.ToInt32(db_record[(int) DnsColumnsOrder.Id]),
                              tenant,
                              user,
                              Convert.ToInt32(db_record[(int) DnsColumnsOrder.DomainId]),
                              db_record[(int) DnsColumnsOrder.DkimSelector].ToString(),
                              db_record[(int) DnsColumnsOrder.DkimPrivateKey].ToString(),
                              db_record[(int) DnsColumnsOrder.DkimPublicKey].ToString(),
                              db_record[(int) DnsColumnsOrder.DnsCheckRecord].ToString(),
                              db_record[(int) DnsColumnsOrder.SpfRecord].ToString());
        }


        private enum DomainCheckTaskColumnsOrder
        {
            DomainId = 0,
            DomainName,
            DomainIsVerified,
            DomainDateAdded,
            DomainDateChecked,
            Tenant,
            User,
            DkimSelector,
            DkimPublicKey,
            SpfRecord,
            MxRecord
        }

        public const int domain_check_task_columns_count = 11;

        public static DnsCheckTaskDto ToDnsCheckTaskDto(this object[] db_record)
        {
            if (db_record.Length != domain_check_task_columns_count)
                throw new InvalidCastException("Can't convert to DnsCheckTaskDto. Invalid columns count.");

            return new DnsCheckTaskDto(
                Convert.ToInt32(db_record[(int)DomainCheckTaskColumnsOrder.DomainId]),
                db_record[(int)DomainCheckTaskColumnsOrder.DomainName].ToString(),
                Convert.ToBoolean(db_record[(int)DomainCheckTaskColumnsOrder.DomainIsVerified]),
                Convert.ToDateTime(db_record[(int)DomainCheckTaskColumnsOrder.DomainDateAdded]),
                Convert.ToDateTime(db_record[(int)DomainCheckTaskColumnsOrder.DomainDateChecked]),
                Convert.ToInt32(db_record[(int)DomainCheckTaskColumnsOrder.Tenant]),
                db_record[(int)DomainCheckTaskColumnsOrder.User].ToString(),
                db_record[(int)DomainCheckTaskColumnsOrder.DkimSelector].ToString(),
                db_record[(int)DomainCheckTaskColumnsOrder.DkimPublicKey].ToString(),
                db_record[(int)DomainCheckTaskColumnsOrder.SpfRecord].ToString(),
                db_record[(int)DomainCheckTaskColumnsOrder.MxRecord].ToString());
        }
    }
}
