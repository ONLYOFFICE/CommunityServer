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


using System.Linq;
using ASC.Api.MailServer.DataContracts;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Server.Administration.Interfaces;

namespace ASC.Api.MailServer.Extensions
{
    public static class DataContractsExtensions
    {
        public static WebDomainData ToWebDomainData(this IWebDomain domain, DnsData dnsData = null)
        {
            return new WebDomainData
                {
                    Id = domain.Id,
                    IsSharedDomain = domain.Tenant == Defines.SHARED_TENANT_ID,
                    Name = domain.Name,
                    Dns = dnsData
                };
        }

        public static MailboxData ToMailboxData(this IMailbox mailbox)
        {
            return new MailboxData
                {
                    Id = mailbox.Id,
                    UserId = mailbox.Account.TeamlabAccount.ID.ToString(),
                    Address = mailbox.Address.ToAddressData(),
                    Aliases = mailbox.Aliases.Select(addr => addr.ToAddressData()).ToList()
                };
        }

        public static AddressData ToAddressData(this IMailAddress mailaddress)
        {
            return new AddressData
                {
                    Id = mailaddress.Id,
                    DomainId = mailaddress.Domain.Id,
                    Email = mailaddress.ToString()
                };
        }

        public static MailGroupData ToMailGroupData(this IMailGroup mailgroup)
        {
            return new MailGroupData
                {
                    Id = mailgroup.Id,
                    Address = mailgroup.Address.ToAddressData(),
                    Addresses = mailgroup.InAddresses.Select(addr => addr.ToAddressData()).ToList()
                };
        }

        public static DnsData ToDnsData(this IDnsSettings dns)
        {
            return new DnsData
                {
                    Id = dns.Id,
                    MxRecord = new MXRecordData
                        {
                            Host = dns.MxHost,
                            Priority = dns.MxPriority,
                            IsVerified = dns.MxVerified

                        },
                    SpfRecord = new DNSRecordData
                        {
                            Name = dns.SpfRecordName,
                            Value = dns.SpfRecord,
                            IsVerified = dns.SpfVerified
                        },
                    DkimRecord = new DKIMRecordData
                        {
                            Selector = dns.DkimSelector,
                            PublicKey = dns.DkimPublicKey,
                            IsVerified = dns.DkimVerified
                        },
                    DomainCheckRecord = new DNSRecordData
                        {
                            Name = dns.DomainCheckRecordName,
                            Value = dns.DomainCheckRecord
                        }
                };
        }
    }
}
