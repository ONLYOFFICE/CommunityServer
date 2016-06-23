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


using System.Net;
using ASC.Mail.Server.Administration.Interfaces;
using NUnit.Framework;

namespace ASC.Mail.Server.Administration.TestCases
{
    [TestFixture]
    public abstract class AddressComparerTests
    {
        public abstract TestContextBase TestContext { get; }

        MailServerBase _server;
        IMailAddress _peterAddress;
        IMailAddress _paulAddress;
        IWebDomain _paulsDomain;
        IWebDomain _petersDomain;
        readonly string _peterDomainName = Dns.GetHostName() + "1.com";
        readonly string _paulsDomainName = Dns.GetHostName() + "2.com";
        private const bool IS_VERIFIED = true;

        [SetUp]
        public void Init()
        {
            _server = TestContext.CreateServer();
            _petersDomain = _server.CreateWebDomain(_peterDomainName, IS_VERIFIED, TestContext.ServerFactory);
            _peterAddress = TestContext.CreateRandomMailAddress(_petersDomain);
        }

        [Test]
        public void CompareSimilarMailboxesWorksCorrect()
        {
            var peterAddressClone = _peterAddress;
            Assert.IsTrue(_peterAddress.Equals(peterAddressClone));
        }

        [Test]
        public void CompareDifferentMailboxesWorksCorrect()
        {
            _paulAddress = TestContext.CreateRandomMailAddress(_petersDomain);
            Assert.IsFalse(_peterAddress.Equals(_paulAddress));
        }

        [Test]
        public void CompareMailboxesWithDifferentDomainsWorks()
        {
            _paulsDomain = _server.CreateWebDomain(_paulsDomainName, IS_VERIFIED, TestContext.ServerFactory);
            _paulAddress = TestContext.CreateRandomMailAddress(_paulsDomain);
            Assert.IsFalse(_peterAddress.Equals(_paulAddress));
        }

        [Test]
        public void CompareMailboxesWithDifferentTenantsWorks()
        {
            _paulAddress = TestContext.CreateRandomMailAddress(_petersDomain);
            Assert.IsFalse(_peterAddress.Equals(_paulAddress));
        }

        [Test]
        public void CompareMailboxesWithDifferentIdsWorks()
        {
            _paulAddress = TestContext.CreateRandomMailAddress(_petersDomain);
            Assert.IsFalse(_peterAddress.Equals(_paulAddress));
        }

        [TearDown]
        public void Clean()
        {
            if(_petersDomain != null)
                _server.DeleteWebDomain(_petersDomain, TestContext.ServerFactory);

            if (_paulsDomain != null)
                _server.DeleteWebDomain(_paulsDomain, TestContext.ServerFactory);
        }
    }
}
