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

        const string PETER_PASSWORD = "peter_pass";
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool IS_VERIFIED = true;

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void SetUp()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, IS_VERIFIED, TestContext.ServerFactory);
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
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);

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
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);
            server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailboxWithNullAccount()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name,  peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, null, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailboxWithNullAddressName()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, null, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailboxWithNullDomain()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, null, peter_account, TestContext.ServerFactory);
        }

        [Test]
        public virtual void GetMailboxListFromServerTest()
        {
            var mailboxes = server.GetMailboxes(TestContext.ServerFactory);

            Assert.GreaterOrEqual(mailboxes.Count, 0);
            if (mailboxes.Count <= 0) return;
            var serverInfo = mailboxes.First().Server;
            Assert.AreEqual(mailboxes.Count(m => m.Server.ConnectionString == serverInfo.ConnectionString), mailboxes.Count());
        }

        [Test]
        public virtual void DeleteMailboxFromServer()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);

            var mailboxesBeforeDeleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsTrue(mailboxesBeforeDeleting.Contains(peter_mailbox), "Mailbox wasn't created.");

            server.DeleteMailbox(peter_mailbox);

            var mailboxesAfterDeleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsFalse(mailboxesAfterDeleting.Contains(peter_mailbox));
        }

        [Test]
        public virtual void DoubleDeleteMailboxFromServerTest()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);

            var mailboxesBeforeDeleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsTrue(mailboxesBeforeDeleting.Contains(peter_mailbox), "Mailbox wasn't created.");
            
            server.DeleteMailbox(peter_mailbox);
            
            var mailboxesAfterDeleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsFalse(mailboxesAfterDeleting.Contains(peter_mailbox));
            
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
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);

            var mailboxGettedById = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);
            Assert.IsTrue(peter_mailbox.Equals(mailboxGettedById));
        }

        [Test]
        public virtual void GetMailboxByNonExistentId()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);
            server.DeleteMailbox(peter_mailbox);
            
            var mailboxesAfterDeleting = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsFalse(mailboxesAfterDeleting.Contains(peter_mailbox), "Mailbox wasn't deleted.");

            var mailboxGettedById = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);
            Assert.AreEqual(null, mailboxGettedById);
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
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);
            server.DeleteMailbox(peter_mailbox);
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);

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
        public virtual void UpdateMailbox()
        {
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart,
                                                 PETER_PASSWORD, peter_address.Domain, peter_account, TestContext.ServerFactory);
            const string new_name = "Peter_New";

            server.UpdateMailbox(peter_mailbox, new_name, TestContext.ServerFactory);

            var newMailbox = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);

            Assert.IsFalse(peter_mailbox.Equals(newMailbox), "Mailbox wasn't updated.");
            Assert.IsTrue(newMailbox.Name.Equals(new_name), "Mailbox wasn't updated.");
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentOutOfRangeException")]
        public virtual void CheckLimitationOfMailboxException()
        {
            var myMailboxLimit = server.SetupInfo.Limits.MailboxMaxCountPerUser;

            for (var i = 0; i <= myMailboxLimit; i++)
            {
                var address = TestContext.CreateRandomMailAddress(peter_domain);
                var account = TestContext.GetMailAccount(address.LocalPart, _peterDomainName);
                server.CreateMailbox(account.TeamlabAccount.Name, address.LocalPart, 
                                     PETER_PASSWORD, address.Domain, account, TestContext.ServerFactory);
            }
        }
    }
}