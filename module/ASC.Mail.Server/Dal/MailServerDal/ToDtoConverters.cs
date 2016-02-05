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
using ASC.Mail.Server.Administration.Interfaces;
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

        public const int SERVER_COLUMNS_COUNT = 6;

        public static TenantServerDto ToTenantServerDto(this object[] dbRecord)
        {
            if (dbRecord.Length != SERVER_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to TenantServerDto. Invalid columns count.");

            return new TenantServerDto(
                (int) dbRecord[(int) ServerTableColumnsOrder.Id],
                dbRecord[(int)ServerTableColumnsOrder.ConnectionString].ToString(),
                dbRecord[(int)ServerTableColumnsOrder.MxRecord].ToString(),
                (int) dbRecord[(int)ServerTableColumnsOrder.ServerType],
                (int)dbRecord[(int)ServerTableColumnsOrder.SmtpServerId],
                (int)dbRecord[(int)ServerTableColumnsOrder.ImapServerId]
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
// ReSharper disable UnusedMember.Local
            DateCreated = 7
// ReSharper restore UnusedMember.Local
        }

        public const int MAIL_ADDRESS_COLUMNS_COUNT = 8;

        public static MailAddressDto ToMailAddressDto(this object[] dbRecord)
        {
            if (dbRecord.Length != MAIL_ADDRESS_COLUMNS_COUNT + DOMAIN_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to MailAddressDto. Invalid columns count.");

            var domainInfo =
                dbRecord.SubArray(MAIL_ADDRESS_COLUMNS_COUNT, dbRecord.Length - MAIL_ADDRESS_COLUMNS_COUNT)
                         .ToWebDomainDto();

            return new MailAddressDto(
                (int)dbRecord[(int)AddressTableColumnsOrder.Id],
                (int)dbRecord[(int)AddressTableColumnsOrder.Tenant],
                dbRecord[(int)AddressTableColumnsOrder.Name].ToString(),
                (int)dbRecord[(int)AddressTableColumnsOrder.DomainId],
                (int)dbRecord[(int)AddressTableColumnsOrder.MailboxId],
                Convert.ToBoolean(dbRecord[(int)AddressTableColumnsOrder.IsMailGroup]),
                Convert.ToBoolean(dbRecord[(int)AddressTableColumnsOrder.IsAlias]),
                domainInfo
           );
        }

        private enum WebDomainColumnsOrder
        {
            Id = 0,
            Name = 1,
            Tenant = 2,
// ReSharper disable UnusedMember.Local
            DateAdded = 3,
// ReSharper restore UnusedMember.Local
            IsVerified = 4
        }

        public const int DOMAIN_COLUMNS_COUNT = 5;

        public static WebDomainDto ToWebDomainDto(this object[] dbRecord)
        {
            if (dbRecord.Length != DOMAIN_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to WebDomainDto. Invalid columns count.");

            return new WebDomainDto(
                (int)dbRecord[(int)WebDomainColumnsOrder.Id],
                dbRecord[(int)WebDomainColumnsOrder.Name].ToString(),
                (int)dbRecord[(int)WebDomainColumnsOrder.Tenant],
                Convert.ToBoolean(dbRecord[(int)WebDomainColumnsOrder.IsVerified])
           );
        }

        private enum MailboxColumnsOrder
        {
            Id = 0,
            UserId = 1,
            Tenant = 2,
            Address = 3,
// ReSharper disable UnusedMember.Local
            DateCreated = 4
// ReSharper restore UnusedMember.Local
        }

        public const int MAILBOX_COLUMNS_COUNT = 5;

        public static MailboxDto ToMailbox(this object[] dbRecord)
        {
            if (dbRecord.Length != MAILBOX_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to MailboxDto. Invalid columns count.");

            return new MailboxDto(
                Convert.ToInt32(dbRecord[(int)MailboxColumnsOrder.Id]),
                dbRecord[(int)MailboxColumnsOrder.UserId].ToString(),
                (int)dbRecord[(int)MailboxColumnsOrder.Tenant],
                dbRecord[(int)MailboxColumnsOrder.Address].ToString()
            );
        }

        public static MailboxWithAddressDto ToMailboxWithAddressDto(this object[] dbRecord)
        {
            if (dbRecord.Length != MAILBOX_COLUMNS_COUNT + MAIL_ADDRESS_COLUMNS_COUNT + DOMAIN_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to MailboxDto. Invalid columns count.");

            var mailboxDto = dbRecord.SubArray(0, MAILBOX_COLUMNS_COUNT).ToMailbox();

            var mailaddressDto = dbRecord.SubArray(MAILBOX_COLUMNS_COUNT,
                                                     dbRecord.Length - MAILBOX_COLUMNS_COUNT).ToMailAddressDto();

            return new MailboxWithAddressDto(mailboxDto, mailaddressDto);
        }

        private enum MailgroupColumnsOrder
        {
            Id = 0,
            AddressId = 1,
            Tenant = 2,
// ReSharper disable UnusedMember.Local
            DateCreated = 3
// ReSharper restore UnusedMember.Local
        }

        public const int MAILGROUP_COLUMNS_COUNT = 4;

        public static MailGroupDto ToMailGroupDto(this object[] dbRecord, List<MailAddressDto> addressDtos)
        {
            if (dbRecord.Length != MAILGROUP_COLUMNS_COUNT + MAIL_ADDRESS_COLUMNS_COUNT + DOMAIN_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to MailGroupDto. Invalid columns count.");

            var addressDto = dbRecord.SubArray(MAILGROUP_COLUMNS_COUNT, dbRecord.Length - MAILGROUP_COLUMNS_COUNT).ToMailAddressDto();

            return new MailGroupDto(
                Convert.ToInt32(dbRecord[(int) MailgroupColumnsOrder.Id]),
                Convert.ToInt32(dbRecord[(int) MailgroupColumnsOrder.Tenant]),
                Convert.ToInt32(dbRecord[(int) MailgroupColumnsOrder.AddressId]),
                addressDto,
                addressDtos);
        }

        private enum CNameXDomainColumnsOrder
        {
            DomainId = 0,
            Cname = 1,
            ReferenceUrl = 2,
            Verified = 3
        }

        public const int CNAME_X_DOMAIN_COLUMNS_COUNT = 4;

        public static CNameXDomainDto ToCNameXDomainDto(this object[] dbRecord)
        {
            if (dbRecord.Length != CNAME_X_DOMAIN_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to CNameXDomainDto. Invalid columns count.");

            return new CNameXDomainDto(
                (int)dbRecord[(int)CNameXDomainColumnsOrder.DomainId],
                dbRecord[(int)CNameXDomainColumnsOrder.Cname].ToString(),
                dbRecord[(int)CNameXDomainColumnsOrder.ReferenceUrl].ToString(),
                Convert.ToBoolean(dbRecord[(int)CNameXDomainColumnsOrder.Verified])
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

        public const int DNS_COLUMNS_COUNT = 7;

        public static DnsDto ToDnsDto(this object[] dbRecord, int tenant, string user)
        {
            if (dbRecord.Length != DNS_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to DnsDto. Invalid columns count.");

            return new DnsDto(Convert.ToInt32(dbRecord[(int) DnsColumnsOrder.Id]),
                              tenant,
                              user,
                              Convert.ToInt32(dbRecord[(int) DnsColumnsOrder.DomainId]),
                              dbRecord[(int) DnsColumnsOrder.DkimSelector].ToString(),
                              dbRecord[(int) DnsColumnsOrder.DkimPrivateKey].ToString(),
                              dbRecord[(int) DnsColumnsOrder.DkimPublicKey].ToString(),
                              dbRecord[(int) DnsColumnsOrder.DnsCheckRecord].ToString(),
                              dbRecord[(int) DnsColumnsOrder.SpfRecord].ToString());
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

        public const int DOMAIN_CHECK_TASK_COLUMNS_COUNT = 11;

        public static DnsCheckTaskDto ToDnsCheckTaskDto(this object[] dbRecord)
        {
            if (dbRecord.Length != DOMAIN_CHECK_TASK_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to DnsCheckTaskDto. Invalid columns count.");

            return new DnsCheckTaskDto(
                Convert.ToInt32(dbRecord[(int)DomainCheckTaskColumnsOrder.DomainId]),
                dbRecord[(int)DomainCheckTaskColumnsOrder.DomainName].ToString(),
                Convert.ToBoolean(dbRecord[(int)DomainCheckTaskColumnsOrder.DomainIsVerified]),
                Convert.ToDateTime(dbRecord[(int)DomainCheckTaskColumnsOrder.DomainDateAdded]),
                Convert.ToDateTime(dbRecord[(int)DomainCheckTaskColumnsOrder.DomainDateChecked]),
                Convert.ToInt32(dbRecord[(int)DomainCheckTaskColumnsOrder.Tenant]),
                dbRecord[(int)DomainCheckTaskColumnsOrder.User].ToString(),
                dbRecord[(int)DomainCheckTaskColumnsOrder.DkimSelector].ToString(),
                dbRecord[(int)DomainCheckTaskColumnsOrder.DkimPublicKey].ToString(),
                dbRecord[(int)DomainCheckTaskColumnsOrder.SpfRecord].ToString(),
                dbRecord[(int)DomainCheckTaskColumnsOrder.MxRecord].ToString());
        }


        private enum TenantServerSettingColumnsOrder
        {
            SettingId = 0,
            Type,
            Hostname,
            Port,
            SocketType,
            Username,
            AuthenticationType
        }

        public const int MAILBOX_SETTINGS_COLUMNS_COUNT = 7;

        public static TenantServerSettingsDto ToTenantServerSettingsDto(this object[] dbRecord)
        {
            if (dbRecord.Length != MAILBOX_SETTINGS_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to TenantServerSettingsDto. Invalid columns count.");

            Administration.Interfaces.ServerType type;

            switch (dbRecord[(int) TenantServerSettingColumnsOrder.Type].ToString())
            {
                case "pop3":
                    type = Administration.Interfaces.ServerType.Pop3;
                    break;
                case "imap":
                    type = Administration.Interfaces.ServerType.Imap;
                    break;
                case "smtp":
                    type = Administration.Interfaces.ServerType.Smtp;
                    break;
                default:
                    throw new ArgumentException("Unknown mail server setting type");
            }

            Administration.Interfaces.EncryptionType socketType;

            switch (dbRecord[(int) TenantServerSettingColumnsOrder.SocketType].ToString())
            {
                case "plain":
                    socketType = Administration.Interfaces.EncryptionType.None;
                    break;
                case "SSL":
                    socketType = Administration.Interfaces.EncryptionType.SSL;
                    break;
                case "STARTTLS":
                    socketType = Administration.Interfaces.EncryptionType.StartTLS;
                    break;
                default:
                    throw new ArgumentException("Unknown mail server socket type");
            }

            Administration.Interfaces.AuthenticationType authenticationType;

            switch (dbRecord[(int) TenantServerSettingColumnsOrder.AuthenticationType].ToString())
            {
                case "":
                case "oauth2":
                case "password-cleartext":
                    authenticationType =  Administration.Interfaces.AuthenticationType.Login;
                    break;
                case "none":
                    authenticationType =  Administration.Interfaces.AuthenticationType.None;
                    break;
                case "password-encrypted":
                    authenticationType =  Administration.Interfaces.AuthenticationType.CramMd5;
                    break;
                default:
                    throw new ArgumentException("Unknown mail server authentication type");
            }

            return
                new TenantServerSettingsDto(Convert.ToInt32(dbRecord[(int) TenantServerSettingColumnsOrder.SettingId]),
                                            type, dbRecord[(int) TenantServerSettingColumnsOrder.Hostname].ToString(),
                                            Convert.ToInt32(dbRecord[(int) TenantServerSettingColumnsOrder.Port]),
                                            authenticationType, socketType,
                                            dbRecord[(int) TenantServerSettingColumnsOrder.Username].ToString());
        }

    }
}
