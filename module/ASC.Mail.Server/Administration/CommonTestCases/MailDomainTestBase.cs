/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Collections.Generic;
using System.Linq;
using System.Net;
using ASC.Mail.Server.Administration.Interfaces;
using NUnit.Framework;

namespace ASC.Mail.Server.Administration.TestCases
{
    [TestFixture]
    public abstract class MailDomainTestBase
    {
        protected MailServerBase server;
        protected IWebDomain peter_domain;
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool IS_VERIFIED = true;
        const string PETER_LOGIN = "peter";
        const string BOB_LOGIN = "bob";
        const string GROUP_LOGIN = "peter";
        const string PETER_PASSWORD = "peter_pass";

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void Init()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, IS_VERIFIED, TestContext.ServerFactory);
        }

        [TearDown]
        public void Dispose()
        {
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            server = null;
        }

        [Test]
        public virtual void CreateAndDeleteDomain()
        {
            Assert.IsNotNull(peter_domain, "Newly created peter_domain object shouldn't be null.");
            Assert.AreEqual(peter_domain.Name, _peterDomainName);
            Assert.Greater(peter_domain.Id, -1, "Domain id must be");
            Assert.IsTrue(server.GetWebDomain(peter_domain.Id, TestContext.ServerFactory) != null, "Server web domains list should contains created one.");
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            Assert.IsFalse(server.GetWebDomain(peter_domain.Id, TestContext.ServerFactory) != null, "Server web domains list shouldn't contain deleted peter_domain.");
        }

        [Test]
        [ExpectedException("System.Data.DuplicateNameException")]
        public virtual void CreateDuplicateDomain()
        {
            server.CreateWebDomain(_peterDomainName, IS_VERIFIED, TestContext.ServerFactory);
        }

        [Test]
        public virtual void DoubleDomainDeleting()
        {
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            var domains = server.GetWebDomains(TestContext.ServerFactory);
            Assert.IsFalse(domains.Contains(peter_domain));
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDomainWithNullName()
        {
            server.CreateWebDomain(null, IS_VERIFIED, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDomainWithEmptyName()
        {
            server.CreateWebDomain("", IS_VERIFIED, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDomainWithEmptyFactory()
        {
            server.CreateWebDomain("test.com", IS_VERIFIED, null);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void RemoveDomainWithNullParameter()
        {
            server.DeleteWebDomain(null, TestContext.ServerFactory);
        }

        [Test]
        public virtual void GetWebDomainsWorks()
        {
            var domains = server.GetWebDomains(TestContext.ServerFactory);
            Assert.IsNotNull(domains, "Empty GetWebDomains() should return empty list");
            Assert.Greater(domains.Count, 0);
        }

        [Test]
        public virtual void GetWebDomainsReturnsUniqueDomainsOnly()
        {
            var domains = server.GetWebDomains(TestContext.ServerFactory);
            var returnedDomainsCount = domains.Count;
            var uniqueDomainsCount = domains.Select(d => d.Id).Distinct().Count();
            Assert.AreEqual(returnedDomainsCount, uniqueDomainsCount);
        }

        [Test]
        public virtual void DeleteGroupsWithDomain()
        {
            var account = TestContext.GetMailAccount("login", _peterDomainName);
            var mailbox = server.CreateMailbox(PETER_LOGIN, PETER_PASSWORD, peter_domain, account, TestContext.ServerFactory);
            var mailgroup = server.CreateMailGroup(GROUP_LOGIN, peter_domain, new List<int> { mailbox.Address.Id }, TestContext.ServerFactory);
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            Assert.IsNull(server.GetMailGroup(mailgroup.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailbox(mailbox.Id, TestContext.ServerFactory));
        }

        [Test]
        public virtual void GroupExistsAfterAnotherDomainDeleted()
        {
            var secondTestDomainName = "2" + _peterDomainName;
            var secondDomain = server.CreateWebDomain(secondTestDomainName, IS_VERIFIED, TestContext.ServerFactory);

            var account = TestContext.GetMailAccount("login", _peterDomainName);
            var mailbox = server.CreateMailbox(PETER_LOGIN, PETER_PASSWORD, peter_domain, account, TestContext.ServerFactory);
            var secondMailbox = server.CreateMailbox(BOB_LOGIN, PETER_PASSWORD, secondDomain, account, TestContext.ServerFactory);

            var firstMailGroup = server.CreateMailGroup(GROUP_LOGIN, peter_domain, new List<int> { mailbox.Address.Id }, TestContext.ServerFactory);
            var secondMailGroup = server.CreateMailGroup(GROUP_LOGIN, secondDomain, new List<int> { secondMailbox.Address.Id }, TestContext.ServerFactory);

            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);

            Assert.IsNotNull(server.GetMailGroup(secondMailGroup.Id, TestContext.ServerFactory));
            Assert.IsNotNull(server.GetMailbox(secondMailbox.Id, TestContext.ServerFactory));

            Assert.IsNull(server.GetMailGroup(firstMailGroup.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailbox(mailbox.Id, TestContext.ServerFactory));

            server.DeleteWebDomain(secondDomain, TestContext.ServerFactory);

            Assert.IsNull(server.GetMailGroup(secondMailGroup.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailbox(secondMailbox.Id, TestContext.ServerFactory));
        }

        [Test]
        public virtual void ValidateWebDomains()
        {
            var domains = server.GetWebDomains(TestContext.ServerFactory);
            Assert.IsNotNull(domains, "Null GetWebDomains() should return empty list");

            foreach (var webDomain in domains)
            {
                Assert.Greater(webDomain.Id, -1);
                Assert.IsNotNullOrEmpty(webDomain.Name);
            }
        }

        //TODO: Fix tests of DKIM
        /*[Test]
        public virtual void CreateDkim()
        {
            const string selector = "dkim";
            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);
            var added_dkim = peter_domain.AddDkim(selector, private_key, public_key, TestContext.ServerFactory);
            var dkim = peter_domain.GetDkim(TestContext.ServerFactory);
            Assert.AreEqual(added_dkim, dkim);
        }

        [Test]
        public virtual void CreateDoubleDkim()
        {
            const string selector = "dkim";
            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);
            peter_domain.AddDkim(selector, private_key, public_key, TestContext.ServerFactory);
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);
            var added_dkim = peter_domain.AddDkim(selector, private_key, public_key, TestContext.ServerFactory);
            var dkim = peter_domain.GetDkim(TestContext.ServerFactory);
            Assert.AreEqual(added_dkim, dkim);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDkimWithEmptySelector()
        {
            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);
            peter_domain.AddDkim("", private_key, public_key, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDkimWithEmptyPrivateKey()
        {
            const string selector = "dkim";
            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);
            peter_domain.AddDkim(selector, "", public_key, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDkimWithEmptyPublicKey()
        {
            const string selector = "dkim";
            string private_key, public_key;
            DnsChecker.DnsChecker.GenerateKeys(out private_key, out public_key);
            peter_domain.AddDkim(selector, private_key, "", TestContext.ServerFactory);
        }*/
    }
}
