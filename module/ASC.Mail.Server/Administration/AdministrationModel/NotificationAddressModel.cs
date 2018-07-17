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
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public class NotificationAddressModel : MailAddressBase, INotificationAddress
    {
        public new IWebDomain Domain { get; private set; }
        public string Email {
            get { return string.Format("{0}@{1}", LocalPart, Domain.Name); }
        }
        public string SmtpServer { get; private set; }
        public int SmtpPort { get; private set; }
        public string SmtpAccount { get; private set; }
        public bool SmtpAuth { get; private set; }
        public string SmptEncryptionType { get; set; }
        public string SmtpAuthenticationType { get; set; }

        public NotificationAddressModel(string localpart, IWebDomain domain, string smtpServer, int smtpPort,
                                        string smtpAccount, bool smtpAuth, EncryptionType smptEncryptionType,
                                        AuthenticationType smtpAuthenticationType)
            : base(localpart, new WebDomainBase(domain))
        {
            if (domain == null)
                throw new ArgumentException("Invalid domain", "domain");

            if (String.IsNullOrEmpty(smtpServer))
                throw new ArgumentNullException("smtpServer");

            if (smtpPort < 0)
                throw new ArgumentException("Invalid smtp port", "smtpPort");

            if (String.IsNullOrEmpty(smtpAccount))
                throw new ArgumentNullException("smtpAccount");

            Domain = domain;
            SmtpAccount = smtpAccount;
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            SmtpAuth = smtpAuth;
            SmptEncryptionType = smptEncryptionType.GetEnumDescription();
            SmtpAuthenticationType = smtpAuthenticationType.GetEnumDescription();

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is INotificationAddress)) return false;

            var other = (INotificationAddress)obj;

            return Domain.Equals(other.Domain) && string.Equals(LocalPart, other.LocalPart);
        }

        public override string ToString()
        {
            return String.Format("{0}@{1}", LocalPart, Domain.Name);
        }

        public override int GetHashCode()
        {
            return ((Domain != null ? Domain.GetHashCode() : 0)) ^ (LocalPart != null ? LocalPart.GetHashCode() : 0);
        }
    }
}
