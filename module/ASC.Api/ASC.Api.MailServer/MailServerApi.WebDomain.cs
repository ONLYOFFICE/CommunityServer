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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Server.Utils;
using System.Security;
using ASC.Common.Utils;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Returns list of the web domains associated with tenant
        /// </summary>
        /// <returns>List of WebDomainData for current tenant</returns>
        /// <short>Get tenant web domain list</short> 
        /// <category>Domains</category>
        [Read(@"domains/get")]
        public List<WebDomainData> GetDomains()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            var listDomains = MailServer.GetWebDomains(MailServerFactory);

            var listDomainData = listDomains.Select(domain =>
                {
                    var dns = domain.GetDns(MailServerFactory);
                    var isVerified = dns.CheckDnsStatus();

                    if (domain.IsVerified != isVerified)
                        domain.SetVerified(isVerified);

                    return domain.ToWebDomainData(dns.ToDnsData());
                }).ToList();

            return listDomainData;
        }

        /// <summary>
        ///    Returns the common web domain
        /// </summary>
        /// <returns>WebDomainData for common web domain</returns>
        /// <short>Get common web domain</short> 
        /// <category>Domains</category>
        [Read(@"domains/common")]
        public WebDomainData GetCommonDomain()
        {
            var listDomains = MailServer.GetWebDomains(MailServerFactory).Where(x=> x.Tenant == Defines.SHARED_TENANT_ID);

            var listDomainData = listDomains.Select(domain =>
            {
                var dns = domain.GetDns(MailServerFactory);
                var isVerified = dns.CheckDnsStatus();

                if (domain.IsVerified != isVerified)
                    domain.SetVerified(isVerified);

                return domain.ToWebDomainData(dns.ToDnsData());
            }).ToList();

            return listDomainData.FirstOrDefault();
        }

        /// <summary>
        ///    Associate a web domain with tenant
        /// </summary>
        /// <param name="name">web domain name</param>
        /// <param name="id_dns"></param>
        /// <returns>WebDomainData associated with tenant</returns>
        /// <short>Add domain to mail server</short> 
        /// <category>Domains</category>
        [Create(@"domains/add")]
        public WebDomainData AddDomain(string name, int id_dns)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException(@"Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException(@"Incorrect domain name.", "name");

            var domainName = name.ToLowerInvariant();

            var freeDns = MailServer.GetFreeDnsRecords(MailServerFactory);

            if (freeDns.Id != id_dns)
                throw new InvalidDataException("This dkim public key is already in use. Please reopen wizard again.");

            var dnsLookup = new DnsLookup();

            if (!dnsLookup.IsDomainTxtRecordExists(domainName, freeDns.DomainCheckRecord))
                throw new InvalidOperationException("txt record is not correct.");

            var isVerified = freeDns.CheckDnsStatus(domainName);

            var webDomain = MailServer.CreateWebDomain(domainName, isVerified, MailServerFactory);

            webDomain.AddDns(id_dns, MailServerFactory);

            return webDomain.ToWebDomainData(freeDns.ToDnsData());
        }

        /// <summary>
        ///    Deletes the selected web domain
        /// </summary>
        /// <param name="id">id of web domain</param>
        /// <returns>id of web domain</returns>
        /// <short>Remove domain from mail server</short> 
        /// <category>Domains</category>
        [Delete(@"domains/remove/{id}")]
        public int RemoveDomain(int id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (id < 0)
                throw new ArgumentException(@"Invalid domain id.", "id");

            var domain = MailServer.GetWebDomain(id, MailServerFactory);
            if (domain.Tenant == Defines.SHARED_TENANT_ID)
                throw new SecurityException("Can not remove shared domain.");

            MailServer.DeleteWebDomain(domain, MailServerFactory);

            return id;
        }

        /// <summary>
        ///    Returns dns records associated with domain
        /// </summary>
        /// <param name="id">id of domain</param>
        /// <returns>Dns records associated with domain</returns>
        /// <short>Returns dns records</short>
        /// <category>DnsRecords</category>
        [Read(@"domains/dns/get")]
        public DnsData GetDnsRecords(int id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (id < 0)
                throw new ArgumentException(@"Invalid domain id.", "id");

            var domain = MailServer.GetWebDomain(id, MailServerFactory);

            var dns = domain.GetDns(MailServerFactory);

            if (dns == null)
                return new DnsData();

            var isVerified = dns.CheckDnsStatus();

            if (domain.IsVerified != isVerified)
                domain.SetVerified(isVerified);

            return dns.ToDnsData();
        }

        /// <summary>
        ///    Check web domain name existance
        /// </summary>
        /// <param name="name">web domain name</param>
        /// <returns>True if domain name already exists.</returns>
        /// <short>Is domain name exists.</short> 
        /// <category>Domains</category>
        [Read(@"domains/exists")]
        public bool IsDomainExists(string name)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException(@"Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException(@"Incorrect domain name.", "name");

            var domainName = name.ToLowerInvariant();

            var isExists = MailServer.IsDomainExists(domainName);

            return isExists;
        }

        /// <summary>
        ///    Check web domain name ownership over txt record in dns
        /// </summary>
        /// <param name="name">web domain name</param>
        /// <returns>True if user is owner of this domain.</returns>
        /// <short>Check domain ownership.</short> 
        /// <category>Domains</category>
        [Read(@"domains/ownership/check")]
        public bool CheckDomainOwnership(string name)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException(@"Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException(@"Incorrect domain name.", "name");

            var domainName = name.ToLowerInvariant();

            var dns = GetUnusedDnsRecords();

            var dnsLookup = new DnsLookup();

            return dnsLookup.IsDomainTxtRecordExists(domainName, dns.DomainCheckRecord.Value);
        }
    }
}
