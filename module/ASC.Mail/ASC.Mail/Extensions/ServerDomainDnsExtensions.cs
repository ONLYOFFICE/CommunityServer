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
