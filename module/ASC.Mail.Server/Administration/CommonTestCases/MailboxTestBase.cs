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

using System.Linq;
using System.Net;
using ASC.Mail.Server.Administration.Interfaces;
using NUnit.Framework;

namespace ASC.Mail.Server.Administration.TestCases
{
    [TestFixture]
    public abstract class MailboxTestBase
    {
        protected MailServerBase server;
        protected IMailbox peter_mailbox;
        protected IWebDomain peter_domain;
        protected IMailAddress peter_address;
        protected IMailAccount peter_account;

        const string PeterLogin = "peter";
        const string PeterPassword = "peter_pass";
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool _isVerified = true;

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void SetUp()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, _isVerified, TestContext.ServerFactory);
            peter_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_account = TestContext.GetMailAccount(peter_address.LocalPart, _peterDomainName);
        }

        [TearDown]
        public void Clean()
        {
            if (peter_mailbox != null)
            {
                server.DeleteMailbox(peter_mailbox);
                peter_mailbox = null;
            }

            if (peter_domain != null)
            {
                server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
                peter_domain = null;
            }
        }

        [Test]
        public virtual void CreateMailboxOnServer()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);

            Assert.Greater(peter_mailbox.Id, 0, "mailbox.Id must be > 0");
            Assert.GreaterOrEqual(peter_mailbox.Tenant, 0, "mailbox.Tenant must be >= 0");
            Assert.IsNotNull(peter_mailbox.Account, "mailbox.Account");
            Assert.IsNotNull(peter_mailbox.Address, "mailbox.Address");
            Assert.IsNotNull(peter_mailbox.Aliases, "mailbox.Aliases");

            Assert.IsNotEmpty(peter_mailbox.Account.Login);
            Assert.IsNotNull(peter_mailbox.Account.TeamlabAccount);

            Assert.Greater(peter_mailbox.Address.Id, 0);
            Assert.GreaterOrEqual(peter_mailbox.Address.Tenant, 0);
            Assert.IsNotEmpty(peter_mailbox.Address.LocalPart);

            Assert.IsNotNull(peter_mailbox.Address.Domain);
            Assert.Greater(peter_mailbox.Address.Domain.Id, 0);
            Assert.IsNotEmpty(peter_mailbox.Address.Domain.Name);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.Data.DuplicateNameException", UserMessage = "You want to create account with already existed username")]
        public virtual void DoubleCreateMailbox()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);
            server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailboxWithNullAccount()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, null, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailboxWithNullAddressName()
        {
            peter_mailbox = server.CreateMailbox(null, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailboxWithNullDomain()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, null, peter_account, TestContext.ServerFactory);
        }

        [Test]
        public virtual void GetMailboxListFromServerTest()
        {
            var mailboxes = server.GetMailboxes(TestContext.ServerFactory);

            Assert.GreaterOrEqual(mailboxes.Count, 0);
            if (mailboxes.Count > 0)
            {
                var server_info = mailboxes.First().Server;
                Assert.AreEqual(mailboxes.Count(m => m.Server.ConnectionString == server_info.ConnectionString), mailboxes.Count());
            }
        }

        [Test]
        public virtual void DeleteMailboxFromServer()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);

            var mailboxes_before_deleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsTrue(mailboxes_before_deleting.Contains(peter_mailbox), "Mailbox wasn't created.");

            server.DeleteMailbox(peter_mailbox);

            var mailboxes_after_deleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsFalse(mailboxes_after_deleting.Contains(peter_mailbox));
        }

        [Test]
        public virtual void DoubleDeleteMailboxFromServerTest()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);

            var mailboxes_before_deleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsTrue(mailboxes_before_deleting.Contains(peter_mailbox), "Mailbox wasn't created.");
            
            server.DeleteMailbox(peter_mailbox);
            
            var mailboxes_after_deleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsFalse(mailboxes_after_deleting.Contains(peter_mailbox));
            
            server.DeleteMailbox(peter_mailbox);
            peter_mailbox = null;
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void DeleteNullMailboxFromServerTest()
        {
            server.DeleteMailbox(null);
        }

        [Test]
        public virtual void GetMailboxById()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);

            var mailbox_getted_by_id = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);
            Assert.IsTrue(peter_mailbox.Equals(mailbox_getted_by_id));
        }

        [Test]
        public virtual void GetMailboxByNonExistentId()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);
            server.DeleteMailbox(peter_mailbox);
            
            var mailboxes_after_deleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsFalse(mailboxes_after_deleting.Contains(peter_mailbox), "Mailbox wasn't deleted.");

            var mailbox_getted_by_id = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);
            Assert.AreEqual(null, mailbox_getted_by_id);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void GetMailboxByNegativeId()
        {
            server.GetMailbox(-1, TestContext.ServerFactory);
        }

        [Test]
        public virtual void ValidateMailbox()
        {
            var mailboxes = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsNotNull(mailboxes, "Null GetMailboxes() should return empty list");

            foreach (var mailbox in mailboxes)
            {
                Assert.Greater(mailbox.Id, 0, "mailbox.Id must be > 0");
                Assert.GreaterOrEqual(mailbox.Tenant, 0, "mailbox.Tenant must be >= 0");
                Assert.IsNotNull(mailbox.Account, "mailbox.Account");
                Assert.IsNotNull(mailbox.Address, "mailbox.Address");
                Assert.IsNotNull(mailbox.Aliases, "mailbox.Aliases");

                Assert.IsNotEmpty(mailbox.Account.Login);
                Assert.IsNotNull(mailbox.Server);
                Assert.IsNotEmpty(mailbox.Server.ConnectionString);
                Assert.IsNotNull(mailbox.Account.TeamlabAccount);

                Assert.Greater(mailbox.Address.Id, 0);
                Assert.GreaterOrEqual(mailbox.Address.Tenant, 0);
                Assert.IsNotEmpty(mailbox.Address.LocalPart);

                Assert.IsNotNull(mailbox.Address.Domain);
                Assert.Greater(mailbox.Address.Domain.Id, 0);
                Assert.IsNotEmpty(mailbox.Address.Domain.Name);

                foreach (var alias in mailbox.Aliases)
                {
                    Assert.Greater(alias.Id, 0);
                    Assert.GreaterOrEqual(alias.Tenant, 0);
                    Assert.IsNotEmpty(alias.LocalPart);
                    Assert.IsNotNull(alias.Domain);
                    Assert.Greater(alias.Domain.Id, 0);
                    Assert.IsNotEmpty(alias.Domain.Name);
                }
            }
        }

        [Test]
        public virtual void ReCreationMailbox()
        {
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);
            server.DeleteMailbox(peter_mailbox);
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, peter_account, TestContext.ServerFactory);

            Assert.Greater(peter_mailbox.Id, 0, "mailbox.Id must be > 0");
            Assert.GreaterOrEqual(peter_mailbox.Tenant, 0, "mailbox.Tenant must be >= 0");
            Assert.IsNotNull(peter_mailbox.Account, "mailbox.Account");
            Assert.IsNotNull(peter_mailbox.Address, "mailbox.Address");
            Assert.IsNotNull(peter_mailbox.Aliases, "mailbox.Aliases");

            Assert.IsNotEmpty(peter_mailbox.Account.Login);
            Assert.IsNotNull(peter_mailbox.Server);
            Assert.IsNotEmpty(peter_mailbox.Server.ConnectionString);
            Assert.IsNotNull(peter_mailbox.Account.TeamlabAccount);

            Assert.Greater(peter_mailbox.Address.Id, 0);
            Assert.GreaterOrEqual(peter_mailbox.Address.Tenant, 0);
            Assert.IsNotEmpty(peter_mailbox.Address.LocalPart);

            Assert.IsNotNull(peter_mailbox.Address.Domain);
            Assert.Greater(peter_mailbox.Address.Domain.Id, 0);
            Assert.IsNotEmpty(peter_mailbox.Address.Domain.Name);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentOutOfRangeException")]
        public virtual void CheckLimitationOfMailboxException()
        {
            var my_mailbox_limit = server.SetupInfo.Limits.MailboxMaxCountPerUser;

            for (var i = 0; i <= my_mailbox_limit; i++)
            {
                var address = TestContext.CreateRandomMailAddress(peter_domain);
                var account = TestContext.GetMailAccount(address.LocalPart, _peterDomainName);
                server.CreateMailbox(address.LocalPart, PeterPassword, address.Domain, account, TestContext.ServerFactory);
            }
        }
    }
}