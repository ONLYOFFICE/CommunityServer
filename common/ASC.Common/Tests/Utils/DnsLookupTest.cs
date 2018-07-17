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