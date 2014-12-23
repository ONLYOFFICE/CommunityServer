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
        private bool _isVerified = true;

        [SetUp]
        public void Init()
        {
            _server = TestContext.CreateServer();
            _petersDomain = _server.CreateWebDomain(_peterDomainName, _isVerified, TestContext.ServerFactory);
            _peterAddress = TestContext.CreateRandomMailAddress(_petersDomain);
        }

        [Test]
        public void CompareSimilarMailboxesWorksCorrect()
        {
            var peter_address_clone = _peterAddress;
            Assert.IsTrue(_peterAddress.Equals(peter_address_clone));
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
            _paulsDomain = _server.CreateWebDomain(_paulsDomainName, _isVerified, TestContext.ServerFactory);
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
