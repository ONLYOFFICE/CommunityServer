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
using System.IO;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Mail.Server.Utils;

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
            var list_domains = MailServer.GetWebDomains(MailServerFactory);

            var list_domain_data = list_domains.Select(domain =>
                {
                    var dns = domain.GetDns(MailServerFactory);
                    var is_verified = dns.CheckDnsStatus();

                    if (domain.IsVerified != is_verified)
                        domain.SetVerified(is_verified);

                    return domain.ToWebDomainData(dns.ToDnsData());
                }).ToList();

            return list_domain_data;
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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException("Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException("Incorrect domain name.", "name");

            var domain_name = name.ToLowerInvariant();

            var free_dns = MailServer.GetFreeDnsRecords(MailServerFactory);

            if (free_dns.Id != id_dns)
                throw new InvalidDataException("This dkim public key is already in use. Please reopen wizard again.");

            var is_verified = free_dns.CheckDnsStatus(domain_name);

            var web_domain = MailServer.CreateWebDomain(domain_name, is_verified, MailServerFactory);

            web_domain.AddDns(id_dns, MailServerFactory);

            return web_domain.ToWebDomainData(free_dns.ToDnsData());
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
            if (id < 0)
                throw new ArgumentException("Invalid domain id.", "id");

            var domain = MailServer.GetWebDomain(id, MailServerFactory);
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
            if (id < 0)
                throw new ArgumentException("Invalid domain id.", "id");

            var domain = MailServer.GetWebDomain(id, MailServerFactory);

            var dns = domain.GetDns(MailServerFactory);

            if (dns == null)
                return new DnsData();

            dns.CheckDnsStatus();

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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException("Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException("Incorrect domain name.", "name");

            var domain_name = name.ToLowerInvariant();

            var is_exists = MailServer.IsDomainExists(domain_name);

            return is_exists;
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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException("Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException("Incorrect domain name.", "name");

            var domain_name = name.ToLowerInvariant();

            var dns = GetUnusedDnsRecords();

            return DnsChecker.DnsChecker.IsTxtRecordCorrect(domain_name, dns.DomainCheckRecord.Value, Logger);
        }
    }
}
