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
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Administration.ServerModel.Base
{
    public class MailAddressBase
    {
        public WebDomainBase Domain { get; private set; }

        public string LocalPart { get; private set; }

        public DateTime DateCreated { get; set; }

        public MailAddressBase(string localpart, WebDomainBase domain)
        {
            if (string.IsNullOrEmpty(localpart))
                throw new ArgumentNullException("localpart");

            if (!Parser.IsEmailLocalPartValid(localpart))
                throw new ArgumentException("Email's local part contains incorrect characters.", "localpart");

            if (localpart.Length > 64)
                throw new ArgumentException("Email's local part exceed limitation of 64 characters.", "localpart");

            LocalPart = localpart;
            Domain = domain;
        }

        public MailAddressBase(IMailAddress mailAddress)
            : this(mailAddress.LocalPart, new WebDomainBase(mailAddress.Domain))
        {
            
        }

        public override string ToString()
        {
            return String.Format("{0}@{1}", LocalPart, Domain.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            var other = (MailAddressBase)obj;

            return Domain.Equals(other.Domain) && string.Equals(LocalPart, other.LocalPart);
        }

        public override int GetHashCode()
        {
            return ((Domain != null ? Domain.GetHashCode() : 0)) ^ (LocalPart != null ? LocalPart.GetHashCode() : 0);
        }
    }
}
