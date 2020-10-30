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
        [ExpectedException(typeof(ArgumentNullException), "domainName")]
        public void DomainNameEmptyExists()
        {
            const string domain = "";

            var dnsLoopup = new DnsLookup();

            dnsLoopup.IsDomainExists(domain);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Domain name could not be parsed")]
        public void DomainNameInvalidExists()
        {
            const string domain = "/.";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainExists(domain);

            Assert.IsTrue(exists);
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

        [TestMethod]
        public void MxExists()
        {
            const string domain = "onlyoffice.com";
            const string mx_record = "mx1.onlyoffice.com";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainMxRecordExists(domain, mx_record);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DkimExists()
        {
            const string domain = "onlyoffice.com";
            const string dkim_record = "v=DKIM1; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDiblqlVejxACSfc3Y0OzRzyFFtnUHgkw65k+QGjG4WvjmJJQNcfdJNaaLo9xKPIfw9vTRVigZa78KgeYFymGlqXtR0z323EwiHaNh82Qo1oBICOZT2AVjWpPjBUGwD6qTorulmLnY9+YKn1bV8B7mt964ewpPHDDsqaHddhV7hqQIDAQAB";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainDkimRecordExists(domain, "dkim", dkim_record);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void TxtSpfExists()
        {
            const string domain = "onlyoffice.com";
            const string txt_record = "v=spf1 a mx mx:avsmedia.net a:smtp1.uservoice.com a:qamail.teamlab.info include:amazonses.com -all";

            var dnsLoopup = new DnsLookup();

            var exists = dnsLoopup.IsDomainTxtRecordExists(domain, txt_record);

            Assert.IsTrue(exists);
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

            Assert.IsTrue(!mxRecords.Any());
        }
    }
}
#endif