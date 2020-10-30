/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using ASC.Common.Utils;
using ASC.Mail.Core;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Extensions
{
    public static class ServerDomainDnsExtensions
    {
        public static bool CheckDnsStatus(this ServerDomainDnsData dnsData, string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return false;

            var dnsLookup = new DnsLookup();

            var mxVerified = dnsLookup.IsDomainMxRecordExists(domain, dnsData.MxRecord.Host);

            dnsData.MxRecord.IsVerified = mxVerified;

            var spfVerified = dnsLookup.IsDomainTxtRecordExists(domain, dnsData.SpfRecord.Value);

            dnsData.SpfRecord.IsVerified = spfVerified;

            var dkimVerified = dnsLookup.IsDomainDkimRecordExists(domain, dnsData.DkimRecord.Selector,
                dnsData.DkimRecord.PublicKey);

            dnsData.DkimRecord.IsVerified = dkimVerified;

            return mxVerified && spfVerified && dkimVerified;
        }

        public static bool UpdateMx(this ServerDns dns, string domain)
        {
            var utcNow = DateTime.UtcNow;
            var hasChanges = false;

            var dnsLookup = new DnsLookup();

            if (dns.MxDateChecked.HasValue && dns.MxDateChecked.Value.AddSeconds(dns.MxTtl) >= utcNow)
                return hasChanges;

            var mxRecord =
                dnsLookup
                    .GetDomainMxRecords(domain)
                    .FirstOrDefault(mx => mx.ExchangeDomainName.ToString().TrimEnd('.').Equals(dns.Mx));

            dns.MxVerified = mxRecord != null;
            dns.MxTtl = mxRecord != null ? mxRecord.TimeToLive : Defines.ServerDnsDefaultTtl;
            dns.MxDateChecked = utcNow;

            hasChanges = true;

            return hasChanges;
        }

        public static bool UpdateSpf(this ServerDns dns, string domain)
        {
            var utcNow = DateTime.UtcNow;

            var dnsLookup = new DnsLookup();

            if (dns.SpfDateChecked.HasValue && dns.SpfDateChecked.Value.AddSeconds(dns.SpfTtl) >= utcNow)
                return false;

            var txtRecords = dnsLookup
                .GetDomainTxtRecords(domain);

            var spfRecord = txtRecords.FirstOrDefault(
                txt => txt.TextData.Trim('\"')
                    .Equals(dns.Spf, StringComparison.InvariantCultureIgnoreCase));

            dns.SpfVerified = spfRecord != null;
            dns.SpfTtl = spfRecord != null ? spfRecord.TimeToLive : Defines.ServerDnsDefaultTtl;
            dns.SpfDateChecked = utcNow;

            return true;
        }

        public static bool UpdateDkim(this ServerDns dns, string domain)
        {
            var utcNow = DateTime.UtcNow;

            var dnsLookup = new DnsLookup();

            if (dns.DkimDateChecked.HasValue && dns.DkimDateChecked.Value.AddSeconds(dns.DkimTtl) >= utcNow)
                return false;

            var dkimRecordName = string.Format("{0}._domainkey.{1}", dns.DkimSelector, domain);

            var dkimRecord = dnsLookup
                .GetDomainTxtRecords(dkimRecordName).FirstOrDefault(
                    txt => txt.TextData.Trim('\"')
                        .Equals(dns.DkimPublicKey, StringComparison.InvariantCultureIgnoreCase));

            dns.DkimVerified = dkimRecord != null;
            dns.DkimTtl = dkimRecord != null ? dkimRecord.TimeToLive : Defines.ServerDnsDefaultTtl;
            dns.DkimDateChecked = utcNow;

            return true;
        }

        public static bool UpdateRecords(this ServerDns dns, string domain, bool force = false)
        {
            if (string.IsNullOrEmpty(domain) || dns == null)
                return false;

            var utcNow = DateTime.UtcNow;

            var hasChanges = false;

            if (force)
            {
                if (dns.UpdateMx(domain))
                {
                    hasChanges = true;
                }

                if (dns.UpdateSpf(domain))
                {
                    hasChanges = true;
                }

                if (dns.UpdateDkim(domain))
                {
                    hasChanges = true;
                }
            }
            else
            {
                if (dns.MxDateChecked.HasValue && dns.MxDateChecked.Value.AddSeconds(dns.MxTtl) >= utcNow &&
                    dns.SpfDateChecked.HasValue && dns.SpfDateChecked.Value.AddSeconds(dns.SpfTtl) >= utcNow &&
                    dns.DkimDateChecked.HasValue && dns.DkimDateChecked.Value.AddSeconds(dns.DkimTtl) >= utcNow)
                    return hasChanges;

                var engineFactory = new EngineFactory(dns.Tenant, dns.User);
                engineFactory.OperationEngine.CheckDomainDns(domain, dns);
            }

            return hasChanges;
        }
    }
}
