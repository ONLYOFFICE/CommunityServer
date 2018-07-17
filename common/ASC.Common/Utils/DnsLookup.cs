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
using System.Linq;
using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace ASC.Common.Utils
{
    public class DnsLookup
    {
        private readonly IDnsResolver _sDnsResolver;
        private readonly DnsClient _dnsClient;

        public DnsLookup()
        {
            _dnsClient = DnsClient.Default;
            _sDnsResolver = new DnsStubResolver(_dnsClient);
        }

        /// <summary>
        /// Get domain MX records
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of MxRecord</returns>
        public List<MxRecord> GetDomainMxRecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var mxRecords = DnsResolve<MxRecord>(domainName, RecordType.Mx);

            return mxRecords;
        }

        /// <summary>
        /// Check existance of MX record in domain DNS
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <param name="mxRecord">MX record value</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>true if exists and vice versa</returns>
        public bool IsDomainMxRecordExists(string domainName, string mxRecord)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            if (string.IsNullOrEmpty(mxRecord))
                throw new ArgumentNullException("mxRecord");

            var mxDomain = DomainName.Parse(mxRecord);

            var records = GetDomainMxRecords(domainName);

            return records.Any(
                    mx => mx.ExchangeDomainName.Equals(mxDomain));
        }

        /// <summary>
        /// Check domain existance
        /// </summary>
        /// <param name="domainName"></param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <exception cref="SystemException">if DNS request failed</exception>
        /// <returns>true if any DNS record exists and vice versa</returns>
        public bool IsDomainExists(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var dnsMessage = GetDnsMessage(domainName);

            return dnsMessage.AnswerRecords.Any();
        }

        /// <summary>
        /// Get domain A records
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of ARecord</returns>
        public List<ARecord> GetDomainARecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var aRecords = DnsResolve<ARecord>(domainName, RecordType.A);

            return aRecords;
        }

        /// <summary>
        /// Get domain IP addresses list
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of IPAddress</returns>
        public List<IPAddress> GetDomainIPs(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var addresses = _sDnsResolver.ResolveHost(domainName);

            return addresses;
        }

        /// <summary>
        /// Get domain TXT records
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of TxtRecord</returns>
        public List<TxtRecord> GetDomainTxtRecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var txtRecords = DnsResolve<TxtRecord>(domainName, RecordType.Txt);

            return txtRecords;
        }

        /// <summary>
        /// Check existance of TXT record in domain DNS
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <param name="recordValue">TXT record value</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>true if exists and vice versa</returns>
        public bool IsDomainTxtRecordExists(string domainName, string recordValue)
        {
            var txtRecords = GetDomainTxtRecords(domainName);

            return
                txtRecords.Any(
                    txtRecord =>
                        txtRecord.TextData.Trim('\"').Equals(recordValue, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Check existance of DKIM record in domain DNS
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <param name="dkimSelector">DKIM selector (example is "dkim")</param>
        /// <param name="dkimValue">DKIM record value</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>true if exists and vice versa</returns>
        public bool IsDomainDkimRecordExists(string domainName, string dkimSelector, string dkimValue)
        {
            var dkimRecordName = dkimSelector + "._domainkey." + domainName;

            var txtRecords = GetDomainTxtRecords(dkimRecordName);

            return txtRecords.Any(txtRecord => txtRecord.TextData.Trim('\"').Equals(dkimValue));
        }

        /// <summary>
        /// Check existance Domain in PTR record
        /// </summary>
        /// <param name="ipAddress">IP address for PTR check</param>
        /// <param name="domainName">PTR domain name</param>
        /// <exception cref="ArgumentNullException">if domainName or ipAddress is empty/null</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>true if exists and vice versa</returns>
        public bool IsDomainPtrRecordExists(IPAddress ipAddress, string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            if (ipAddress == null)
                throw new ArgumentNullException("ipAddress");

            var domain = DomainName.Parse(domainName);

            var ptrDomain = _sDnsResolver.ResolvePtr(ipAddress);

            return ptrDomain.Equals(domain);
        }

        /// <summary>
        /// Check existance Domain in PTR record
        /// </summary>
        /// <param name="ipAddress">IP address for PTR check</param>
        /// <param name="domainName">PTR domain name</param>
        /// <exception cref="ArgumentNullException">if domainName or ipAddress is empty/null</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <exception cref="FormatException">if ipAddress is invalid</exception>
        /// <returns>true if exists and vice versa</returns>
        public bool IsDomainPtrRecordExists(string ipAddress, string domainName)
        {
            return IsDomainPtrRecordExists(IPAddress.Parse(ipAddress), domainName);
        }

        private DnsMessage GetDnsMessage(string domainName, RecordType? type = null)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var domain = DomainName.Parse(domainName);

            var dnsMessage = type.HasValue ? _dnsClient.Resolve(domain, type.Value) : _dnsClient.Resolve(domain);

            if ((dnsMessage == null) ||
                ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
            {
                throw new SystemException(); // DNS request failed
            }

            return dnsMessage;
        }

        private List<T> DnsResolve<T>(string domainName, RecordType type)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var dnsMessage = GetDnsMessage(domainName, type);

            return dnsMessage.AnswerRecords.Where(r => r.RecordType == type).Cast<T>().ToList();
        }
    }
}
