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

        internal PostfixMailAddressDto(string name, string redirect, string domainName,
            bool active)
        {
            this.name = name;
            this.redirect = redirect;
            domain_name = domainName;
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
        public readonly bool backupmx;
        public readonly bool active;


        internal PostfixWebDomainDto(string name, string description, int aliases, int mailboxes, long maxquota, long quota,
            bool backupmx, bool active)
        {
            this.name = name;
            this.description = description;
            this.aliases = aliases;
            this.mailboxes = mailboxes;
            this.maxquota = maxquota;
            this.quota = quota;
            this.backupmx = backupmx;
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
            string localPart, string domain, bool active)
        {
            this.username = username;
            this.name = name;
            this.maldir = maldir;
            this.quota = quota;
            local_part = localPart;
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

        internal PostfixDkimDto(int id, string domainName, string selector, string privateKey, string publicKey)
        {
            this.id = id;
            domain_name = domainName;
            this.selector = selector;
            private_key = privateKey;
            public_key = publicKey;
        }
    }
}
