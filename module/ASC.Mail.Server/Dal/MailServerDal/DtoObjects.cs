/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using Newtonsoft.Json.Linq;

namespace ASC.Mail.Server.Dal
{
    public class MailAddressDto
    {
        public readonly int id;
        public readonly int tenant;
        public readonly string name;
        public readonly int domain_id;
        public readonly int mailbox_id;
        public readonly bool is_mail_group;
        public readonly bool is_alias;
        public readonly WebDomainDto domain;

        internal MailAddressDto(int id, int tenant, string name, int domainId,
            int mailboxId, bool isMailGroup, bool isAlias, WebDomainDto domainDto)
        {
            this.id = id;
            this.name = name;
            domain_id = domainId;
            mailbox_id = mailboxId;
            this.tenant = tenant;
            is_mail_group = isMailGroup;
            is_alias = isAlias;
            domain = domainDto;
        }

        public override string ToString()
        {
            return name + '@' + domain.name;
        }
    }

    public class WebDomainDto
    {
        public readonly int id;
        public readonly int tenant;
        public readonly string name;
        public readonly bool is_virified;

        internal WebDomainDto(int id, string name, int tenant, bool isVirified)
        {
            this.id = id;
            this.name = name;
            this.tenant = tenant;
            is_virified = isVirified;
        }
    }

    public class MailboxDto
    {
        public readonly int id;
        public readonly string user;
        public readonly int tenant;
        public readonly string address;
        public readonly string name;

        internal MailboxDto(int id, string user, int tenant, string address, string name)
        {
            this.id = id;
            this.user = user;
            this.tenant = tenant;
            this.address = address;
            this.name = name;
        }
    }

    public class TenantServerDto
    {
        public readonly int id;
        public readonly string connection_string;
        public readonly string mx_record;
        public readonly int type;
        public readonly int smtp_settings_id;
        public readonly int imap_settings_id;

        public string ApiVersionUrl
        {
            get
            {
                try
                {
                    var apiObject = JObject.Parse(connection_string)["Api"];
                    return string.Format("{0}://{1}:{2}/api/{3}/version.json?auth_token={4}", apiObject["Protocol"], apiObject["Server"], apiObject["Port"], apiObject["Version"], apiObject["Token"]);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        internal TenantServerDto(int id, string connectionString,  string mxRecord,
            int type, int smtpSettingsId, int imapSettingsId)
        {
            this.id = id;
            connection_string = connectionString;
            mx_record = mxRecord;
            this.type = type;
            smtp_settings_id = smtpSettingsId;
            imap_settings_id = imapSettingsId;
        }
    }

    public class TenantServerSettingsDto
    {
        public readonly int id;
        public readonly Administration.Interfaces.ServerType type;
        public readonly string hostname;
        public readonly int port;
        public readonly Administration.Interfaces.AuthenticationType authentication_type;
        public readonly Administration.Interfaces.EncryptionType socket_type;
        public readonly string smtpLoginFormat;

        internal TenantServerSettingsDto(int id, Administration.Interfaces.ServerType type, string hostname,
            int port, Administration.Interfaces.AuthenticationType authentication_type, Administration.Interfaces.EncryptionType socket_type, string loginFormat)
        {
            this.id = id;
            this.type = type;
            this.hostname = hostname;
            this.port = port;
            this.authentication_type = authentication_type;
            this.socket_type = socket_type;
            this.smtpLoginFormat = loginFormat;
        }

    }

    public class MailGroupDto
    {
        public readonly int id;
        public readonly int id_tenant;
        public readonly int id_address;

        public readonly MailAddressDto address;
        public readonly List<MailAddressDto> addresses;

        internal MailGroupDto(int id, int tenant, int addressId, MailAddressDto address, List<MailAddressDto> addresses)
        {
            this.id = id;
            id_tenant = tenant;
            id_address = addressId;
            this.address = address;
            this.addresses = addresses;
        }
    }

    public enum ServerType
    {
        MockServer = -1,
        MailEnable = 1,
        Postfix = 2
    }

    public class MailboxWithAddressDto
    {
        public readonly MailboxDto mailbox;
        public readonly MailAddressDto mailbox_address;

        internal MailboxWithAddressDto(MailboxDto mailbox, MailAddressDto mailboxAddress)
        {
            this.mailbox = mailbox;
            mailbox_address = mailboxAddress;
        }
    }

    public class CNameXDomainDto
    {
        public readonly int id_domain;
        public readonly string cname;
        public readonly bool verified;
        public readonly string reference_url;

        internal CNameXDomainDto(int domainId, string cname, string referenceUrl, bool verified)
        {
            id_domain = domainId;
            this.cname = cname;
            this.verified = verified;
            reference_url = referenceUrl;
        }
    }

    public class DnsDto
    {
        public readonly int id;
        public readonly int tenant;
        public readonly string user;
        public int id_domain;
        public readonly string dkim_selector;
        public readonly string dkim_private_key;
        public readonly string dkim_public_key;
        public readonly string domain_chek;
        public readonly string spf;

        internal DnsDto(int id, int tenant, string user, int domainId, string dkimSelector, 
            string dkimPrivateKey, string dkimPublicKey, string domainChek, string spf)
        {
            this.id = id;
            id_domain = domainId;
            this.tenant = tenant;
            this.user = user;
            dkim_selector = dkimSelector;
            dkim_private_key = dkimPrivateKey;
            dkim_public_key = dkimPublicKey;
            domain_chek = domainChek;
            this.spf = spf;
        }
    }

    public class DnsCheckTaskDto
    {
        public readonly int domain_id;
        public readonly string domain_name;
        public readonly bool domain_is_verified;
        public readonly DateTime domain_date_added;
        public readonly DateTime domain_date_checked;

        public readonly int tenant;
        public readonly string user;

        public readonly string dkim_selector;
        public readonly string dkim_public_key;

        public readonly string spf;
        public readonly string mx_record;

        internal DnsCheckTaskDto(int domainId, string domainName, bool domainIsVerified, 
            DateTime domainDateAdded, DateTime domainDateChecked,
            int tenant, string user, 
            string dkimSelector, string dkimPublicKey, 
            string spf, string mxRecord)
        {
            domain_id = domainId;
            domain_name = domainName;
            domain_is_verified = domainIsVerified;
            domain_date_added = domainDateAdded;
            domain_date_checked = domainDateChecked;

            this.tenant = tenant;
            this.user = user;

            dkim_selector = dkimSelector;
            dkim_public_key = dkimPublicKey;

            this.spf = spf;
            mx_record = mxRecord;
        }
    }

}
