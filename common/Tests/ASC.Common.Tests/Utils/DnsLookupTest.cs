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


#if DEBUG
namespace ASC.Common.Tests.Utils
{
    using System;
    using System.Linq;
    using ASC.Common.Utils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class DnsLookupTest
    {
        [TestMethod]
        public void DomainNameEmptyExists()
        {
            const string domain = "";

            var dnsLoopup = new DnsLookup();

            Assert.Throws<ArgumentNullException>(() => dnsLoopup.IsDomainExists(domain));
        }

        [TestMethod]
        public void DomainNameInvalidExists()
        {
            const string domain = "/.";

            var dnsLoopup = new DnsLookup();

            Assert.Throws<ArgumentException>(() => dnsLoopup.IsDomainExists(domain));
        }

        [TestMethod]
        public void DomainExists()
        {
            const string domain = "onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainExists(domain);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DomainNotExists()
        {
            const string domain = "sdkjskytt111hdhdhwooo.ttt";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainExists(domain);

            Assert.IsFalse(exists);
        }

        // record values are read from DNS first: the tests check the lookup logic, not the current DNS content of the domain

        [TestMethod]
        public void MxExists()
        {
            const string domain = "onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var mxRecord = dnsLoopup.GetDomainMxRecords(domain).Select(r => r.ExchangeDomainName.TrimEnd('.')).FirstOrDefault();

            Assert.IsNotNull(mxRecord, "no MX records for " + domain);
            Assert.IsTrue(dnsLoopup.IsDomainMxRecordExists(domain, mxRecord));
            Assert.IsFalse(dnsLoopup.IsDomainMxRecordExists(domain, "mx-not-exists." + domain));
        }

        [TestMethod]
        public void DkimExists()
        {
            const string domain = "onlyoffice.com";
            const string selector = "dkim";

            var dnsLoopup = new DnsLookup();

            var dkimRecord = dnsLoopup.GetDomainTxtRecords(selector + "._domainkey." + domain)
                .Select(r => r.TextData.Trim('\"'))
                .FirstOrDefault(r => r.StartsWith("v=DKIM1", StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(dkimRecord, "no DKIM record for selector " + selector + " of " + domain);
            Assert.IsTrue(dnsLoopup.IsDomainDkimRecordExists(domain, selector, dkimRecord));
            Assert.IsFalse(dnsLoopup.IsDomainDkimRecordExists(domain, selector, dkimRecord + "AAAA"));
        }

        [TestMethod]
        public void TxtSpfExists()
        {
            const string domain = "onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var spfRecord = dnsLoopup.GetDomainTxtRecords(domain)
                .Select(r => r.TextData.Trim('\"'))
                .FirstOrDefault(r => r.StartsWith("v=spf1", StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(spfRecord, "no SPF record for " + domain);
            Assert.IsTrue(dnsLoopup.IsDomainTxtRecordExists(domain, spfRecord));
            Assert.IsFalse(dnsLoopup.IsDomainTxtRecordExists(domain, "v=spf1 include:spf-not-exists." + domain + " -all"));
        }

        [TestMethod]
        public void GetMxRecords()
        {
            const string domain = "onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var mxRecords = dnsLoopup.GetDomainMxRecords(domain);

            Assert.IsTrue(mxRecords.Any());
        }

        [TestMethod]
        public void GetARecords()
        {
            const string domain = "onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var aRecords = dnsLoopup.GetDomainARecords(domain);

            Assert.IsTrue(aRecords.Any());
        }

        [TestMethod]
        public void GetIPs()
        {
            const string domain = "onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var ips = dnsLoopup.GetDomainIPs(domain);

            Assert.IsTrue(ips.Any());
        }

        [TestMethod]
        public void GetPtr()
        {
            const string domain = "mx1.onlyoffice.com";
            const string ip = "54.244.95.25";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainPtrRecordExists(ip, domain);

            Assert.IsTrue(exists);
        }


        [TestMethod]
        public void GetNoexistedDomainMx()
        {
            const string domain = "taramparam.tk";

            var dnsLoopup = new DnsLookup();

            var mxRecords = dnsLoopup.GetDomainMxRecords(domain);

            Assert.IsFalse(mxRecords.Any());
        }
    }
}
#endif