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
        private const bool _isVerified = true;
        const string PeterLogin = "peter";
        const string BobLogin = "bob";
        const string GroupLogin = "peter";
        const string PeterPassword = "peter_pass";

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void Init()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, _isVerified, TestContext.ServerFactory);
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
            server.CreateWebDomain(_peterDomainName, _isVerified, TestContext.ServerFactory);
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
            server.CreateWebDomain(null, _isVerified, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDomainWithEmptyName()
        {
            server.CreateWebDomain("", _isVerified, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public virtual void CreateDomainWithEmptyFactory()
        {
            server.CreateWebDomain("test.com", _isVerified, null);
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
            var returned_domains_count = domains.Count;
            var unique_domains_count = domains.Select(d => d.Id).Distinct().Count();
            Assert.AreEqual(returned_domains_count, unique_domains_count);
        }

        [Test]
        public virtual void DeleteGroupsWithDomain()
        {
            var account = TestContext.GetMailAccount("login", _peterDomainName);
            var mailbox = server.CreateMailbox(PeterLogin, PeterPassword, peter_domain, account, TestContext.ServerFactory);
            var mailgroup = server.CreateMailGroup(GroupLogin, peter_domain, new List<int> { mailbox.Address.Id }, TestContext.ServerFactory);
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            Assert.IsNull(server.GetMailGroup(mailgroup.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailbox(mailbox.Id, TestContext.ServerFactory));
        }

        [Test]
        public virtual void GroupExistsAfterAnotherDomainDeleted()
        {
            var second_test_domain_name = "2" + _peterDomainName;
            var second_domain = server.CreateWebDomain(second_test_domain_name, _isVerified, TestContext.ServerFactory);

            var account = TestContext.GetMailAccount("login", _peterDomainName);
            var mailbox = server.CreateMailbox(PeterLogin, PeterPassword, peter_domain, account, TestContext.ServerFactory);
            var second_mailbox = server.CreateMailbox(BobLogin, PeterPassword, second_domain, account, TestContext.ServerFactory);

            var first_mail_group = server.CreateMailGroup(GroupLogin, peter_domain, new List<int> { mailbox.Address.Id }, TestContext.ServerFactory);
            var second_mail_group = server.CreateMailGroup(GroupLogin, second_domain, new List<int> { second_mailbox.Address.Id }, TestContext.ServerFactory);

            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);

            Assert.IsNotNull(server.GetMailGroup(second_mail_group.Id, TestContext.ServerFactory));
            Assert.IsNotNull(server.GetMailbox(second_mailbox.Id, TestContext.ServerFactory));

            Assert.IsNull(server.GetMailGroup(first_mail_group.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailbox(mailbox.Id, TestContext.ServerFactory));

            server.DeleteWebDomain(second_domain, TestContext.ServerFactory);

            Assert.IsNull(server.GetMailGroup(second_mail_group.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailbox(second_mailbox.Id, TestContext.ServerFactory));
        }

        [Test]
        public virtual void ValidateWebDomains()
        {
            var domains = server.GetWebDomains(TestContext.ServerFactory);
            Assert.IsNotNull(domains, "Null GetWebDomains() should return empty list");

            foreach (var web_domain in domains)
            {
                Assert.Greater(web_domain.Id, -1);
                Assert.IsNotNullOrEmpty(web_domain.Name);
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
