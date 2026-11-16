/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using DnsClient;

namespace ASC.Common.Utils
{
    public class DnsLookup
    {
        // like ARSoft: ask the system DNS servers in order and take the first answer,
        // don't ask the other servers on NXDOMAIN or empty response
        private static readonly LookupClient _lookupClient = new LookupClient(new LookupClientOptions
        {
            UseCache = false,
            UseRandomNameServer = false,
            ContinueOnDnsError = false,
            ContinueOnEmptyResponse = false
        });

        /// <summary>
        /// Get domain MX records
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of MxRecord</returns>
        public List<DnsMxRecord> GetDomainMxRecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var mxRecords = GetDnsMessage(domainName, QueryType.MX)
                .Answers
                .MxRecords()
                .Select(r => new DnsMxRecord(r.Exchange.Value, r.Preference, r.TimeToLive))
                .ToList();

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

            var mxDomain = ParseDomainName(mxRecord);

            var records = GetDomainMxRecords(domainName);

            return records.Any(
                    mx => DomainNameEquals(mx.ExchangeDomainName, mxDomain));
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

            var dnsMessage = GetDnsMessage(domainName, QueryType.A);

            return dnsMessage.Answers.Any();
        }

        /// <summary>
        /// Get domain A records
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of ARecord</returns>
        public List<DnsARecord> GetDomainARecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var aRecords = GetDnsMessage(domainName, QueryType.A)
                .Answers
                .ARecords()
                .Select(r => new DnsARecord(r.Address, r.TimeToLive))
                .ToList();

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

            var domain = ParseDomainName(domainName);

            try
            {
                var hostEntry = _lookupClient.GetHostEntry(domain);

                return hostEntry != null ? hostEntry.AddressList.ToList() : new List<IPAddress>();
            }
            catch (DnsResponseException)
            {
                return new List<IPAddress>();
            }
        }

        /// <summary>
        /// Get domain TXT records
        /// </summary>
        /// <param name="domainName">domain name</param>
        /// <exception cref="ArgumentNullException">if domainName is empty</exception>
        /// <exception cref="ArgumentException">if domainName is invalid</exception>
        /// <returns>list of TxtRecord</returns>
        public List<DnsTxtRecord> GetDomainTxtRecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            // long TXT values (e.g. DKIM keys) are split into several strings, join them back
            var txtRecords = GetDnsMessage(domainName, QueryType.TXT)
                .Answers
                .TxtRecords()
                .Select(r => new DnsTxtRecord(string.Concat(r.Text), r.TimeToLive))
                .ToList();

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

            var domain = ParseDomainName(domainName);

            var dnsMessage = Query(() => _lookupClient.QueryReverse(ipAddress));

            return dnsMessage.Answers.PtrRecords().Any(ptr => DomainNameEquals(ptr.PtrDomainName.Value, domain));
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

        private IDnsQueryResponse GetDnsMessage(string domainName, QueryType type)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var domain = ParseDomainName(domainName);

            return Query(() => _lookupClient.Query(domain, type));
        }

        private static IDnsQueryResponse Query(Func<IDnsQueryResponse> query)
        {
            IDnsQueryResponse dnsMessage;

            try
            {
                dnsMessage = query();
            }
            catch (DnsResponseException)
            {
                throw new SystemException(); // DNS request failed
            }

            if ((dnsMessage == null) ||
                (dnsMessage.HasError && dnsMessage.Header.ResponseCode != DnsHeaderResponseCode.NotExistentDomain))
            {
                throw new SystemException(); // DNS request failed
            }

            return dnsMessage;
        }

        private static string ParseDomainName(string domainName)
        {
            var labels = domainName.TrimEnd('.').Split('.');

            if (labels.Any(label => label.Length == 0 || label.Length > 63 ||
                                    label.Any(c => c < 128 && !char.IsLetterOrDigit(c) && c != '-' && c != '_')))
            {
                throw new ArgumentException("Invalid domain name: " + domainName, "domainName");
            }

            return DnsString.Parse(domainName).Value;
        }

        private static bool DomainNameEquals(string first, string second)
        {
            return string.Equals(first.TrimEnd('.'), second.TrimEnd('.'), StringComparison.OrdinalIgnoreCase);
        }
    }

    public class DnsMxRecord
    {
        public DnsMxRecord(string exchangeDomainName, int preference, int timeToLive)
        {
            ExchangeDomainName = exchangeDomainName;
            Preference = preference;
            TimeToLive = timeToLive;
        }

        /// <summary>
        /// Mail exchanger host name with the trailing dot (e.g. "mx1.example.com.")
        /// </summary>
        public string ExchangeDomainName { get; private set; }

        public int Preference { get; private set; }

        public int TimeToLive { get; private set; }

        public override string ToString()
        {
            return ExchangeDomainName;
        }
    }

    public class DnsARecord
    {
        public DnsARecord(IPAddress address, int timeToLive)
        {
            Address = address;
            TimeToLive = timeToLive;
        }

        public IPAddress Address { get; private set; }

        public int TimeToLive { get; private set; }

        public override string ToString()
        {
            return Address.ToString();
        }
    }

    public class DnsTxtRecord
    {
        public DnsTxtRecord(string textData, int timeToLive)
        {
            TextData = textData;
            TimeToLive = timeToLive;
        }

        public string TextData { get; private set; }

        public int TimeToLive { get; private set; }

        public override string ToString()
        {
            return TextData;
        }
    }
}
