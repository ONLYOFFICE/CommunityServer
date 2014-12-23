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

        internal MailAddressDto(int id, int tenant, string name, int domain_id,
            int mailbox_id, bool is_mail_group, bool is_alias, WebDomainDto domain_dto)
        {
            this.id = id;
            this.name = name;
            this.domain_id = domain_id;
            this.mailbox_id = mailbox_id;
            this.tenant = tenant;
            this.is_mail_group = is_mail_group;
            this.is_alias = is_alias;
            domain = domain_dto;
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

        internal WebDomainDto(int id, string name, int tenant, bool is_virified)
        {
            this.id = id;
            this.name = name;
            this.tenant = tenant;
            this.is_virified = is_virified;
        }
    }

    public class MailboxDto
    {
        public readonly int id;
        public readonly string user_id;
        public readonly int tenant_id;
        public readonly string address;

        internal MailboxDto(int id, string user_id, int tenant_id, string address)
        {
            this.id = id;
            this.user_id = user_id;
            this.tenant_id = tenant_id;
            this.address = address;
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

        internal TenantServerDto(int id, string connection_string,  string mx_record,
            int type, int smtp_settings_id, int imap_settings_id)
        {
            this.id = id;
            this.connection_string = connection_string;
            this.mx_record = mx_record;
            this.type = type;
            this.smtp_settings_id = smtp_settings_id;
            this.imap_settings_id = imap_settings_id;
        }
    }

    public class MailGroupDto
    {
        public readonly int id;
        public readonly int id_tenant;
        public readonly int id_address;

        public readonly MailAddressDto address;
        public readonly List<MailAddressDto> addresses;

        internal MailGroupDto(int id, int id_tenant, int id_address, MailAddressDto address, List<MailAddressDto> addresses)
        {
            this.id = id;
            this.id_tenant = id_tenant;
            this.id_address = id_address;
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

        internal MailboxWithAddressDto(MailboxDto mailbox, MailAddressDto mailbox_address)
        {
            this.mailbox = mailbox;
            this.mailbox_address = mailbox_address;
        }
    }

    public class CNameXDomainDto
    {
        public readonly int id_domain;
        public readonly string cname;
        public readonly bool verified;
        public readonly string reference_url;

        internal CNameXDomainDto(int id_domain, string cname, string reference_url, bool verified)
        {
            this.id_domain = id_domain;
            this.cname = cname;
            this.verified = verified;
            this.reference_url = reference_url;
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

        internal DnsDto(int id, int tenant, string user, int id_domain, string dkim_selector, 
            string dkim_private_key, string dkim_public_key, string domain_chek, string spf)
        {
            this.id = id;
            this.id_domain = id_domain;
            this.tenant = tenant;
            this.user = user;
            this.dkim_selector = dkim_selector;
            this.dkim_private_key = dkim_private_key;
            this.dkim_public_key = dkim_public_key;
            this.domain_chek = domain_chek;
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

        internal DnsCheckTaskDto(int domain_id, string domain_name, bool domain_is_verified, 
            DateTime domain_date_added, DateTime domain_date_checked,
            int tenant, string user, 
            string dkim_selector, string dkim_public_key, 
            string spf, string mx_record)
        {
            this.domain_id = domain_id;
            this.domain_name = domain_name;
            this.domain_is_verified = domain_is_verified;
            this.domain_date_added = domain_date_added;
            this.domain_date_checked = domain_date_checked;

            this.tenant = tenant;
            this.user = user;

            this.dkim_selector = dkim_selector;
            this.dkim_public_key = dkim_public_key;

            this.spf = spf;
            this.mx_record = mx_record;
        }
    }

}
