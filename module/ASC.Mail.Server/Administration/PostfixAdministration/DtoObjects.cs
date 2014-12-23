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
using ASC.Mail.Server.Administration.ServerModel.Base;

namespace ASC.Mail.Server.PostfixAdministration
{
    public class PostfixMailAddressDto
    {
        public readonly string name;
        public readonly string redirect;
        public readonly string domain_name;
        public readonly bool active;

        private PostfixWebDomainDto _domainInfo;
        public PostfixWebDomainDto Domain
        {
            get { return _domainInfo; }
            internal set
            {
                if (value.name == domain_name)
                {
                    _domainInfo = value;
                }
                else
                {
                    var m = String.Format("Invalid Domain with name = {0}. Correct domain has name= {1}", value.name, domain_name);
                    throw new ArgumentException(m);
                }
            }
        }

        internal PostfixMailAddressDto(string name, string redirect, string domain_name,
            bool active)
        {
            this.name = name;
            this.redirect = redirect;
            this.domain_name = domain_name;
            this.active = active;
            _domainInfo = null;
        }

        internal MailAddressBase ToPostfixAddress()
        {
            return new MailAddressBase(name.Split('@')[0],
                                      new WebDomainBase(Domain.name));
        }
    }

    public class PostfixWebDomainDto
    {
        public readonly string name;
        public readonly string description;
        public readonly int aliases;
        public readonly int mailboxes;
        public readonly long maxquota;
        public readonly long quota;
        public readonly string transport;
        public readonly bool backupmx;
        public readonly bool active;


        internal PostfixWebDomainDto(string name, string description, int aliases, int mailboxes, long maxquota, long quota,
            string transport, bool backupmx, bool active)
        {
            this.name = name;
            this.description = description;
            this.aliases = aliases;
            this.mailboxes = mailboxes;
            this.maxquota = maxquota;
            this.quota = quota;
            this.backupmx = backupmx;
            this.transport = transport;
            this.active = active;
        }
    }

    public class PostfixMailboxDto
    {
        public readonly string username;
        public readonly string name;
        public readonly string maldir;
        public readonly long quota;
        public readonly string local_part;
        public readonly string domain;
        public readonly bool active;

        internal PostfixMailboxDto(string username, string name, string maldir, long quota,
            string local_part, string domain, bool active)
        {
            this.username = username;
            this.name = name;
            this.maldir = maldir;
            this.quota = quota;
            this.local_part = local_part;
            this.domain = domain;
            this.active = active;
        }
    }

    public class PostfixMailgroupDto
    {
        public readonly PostfixMailAddressDto address;

        internal PostfixMailgroupDto(PostfixMailAddressDto address)
        {
            this.address = address;
        }
    }

    public class PostfixDkimDto
    {
        public readonly int id;
        public readonly string domain_name;
        public readonly string selector;
        public readonly string private_key;
        public readonly string public_key;

        internal PostfixDkimDto(int id, string domain_name, string selector, string private_key, string public_key)
        {
            this.id = id;
            this.domain_name = domain_name;
            this.selector = selector;
            this.private_key = private_key;
            this.public_key = public_key;
        }
    }
}
