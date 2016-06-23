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
    public abstract class MailAddressTestBase
    {
        protected MailServerBase server;
        protected IWebDomain peter_domain;
        protected IMailAddress peter_address;
        protected IMailAddress peter_second_address;
        protected IMailAddress peter_alias;
        protected IMailbox peter_mailbox;
        protected IMailbox peter_second_mailbox;
        protected IMailAccount peter_account;
        protected IMailAccount peter_second_account;
        const string PETER_PASSWORD = "peter_pass";
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool IS_VERIFIED = true;

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void Init()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, IS_VERIFIED, TestContext.ServerFactory);
            peter_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_second_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_alias = TestContext.CreateRandomMailAddress(peter_domain);
            peter_account = TestContext.GetMailAccount(peter_address.LocalPart, _peterDomainName);
            peter_second_account = TestContext.GetMailAccount(peter_second_address.LocalPart, _peterDomainName);
            peter_mailbox = server.CreateMailbox(peter_account.TeamlabAccount.Name, peter_address.LocalPart, 
                                                 PETER_PASSWORD, peter_domain, peter_account, TestContext.ServerFactory);
            peter_second_mailbox = server.CreateMailbox(peter_second_account.TeamlabAccount.Name, peter_second_address.LocalPart, 
                                                 PETER_PASSWORD, peter_domain, peter_second_account, TestContext.ServerFactory);
        }

        [TearDown]
        public void Clean()
        {
            if (peter_domain != null)
                server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
        }

        [Test]
        public virtual void AddAliasAndRemoveAliasToMailbox()
        {
            peter_alias = peter_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
            Assert.IsTrue(peter_mailbox.Aliases.Contains(peter_alias));
            peter_mailbox.RemoveAlias(peter_alias.Id);
            Assert.IsFalse(peter_mailbox.Aliases.Contains(peter_alias));
        }

        [Test]
        public virtual void AliasAddAndRemoveSyncronizationWithServerTest()
        {
            peter_alias = peter_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
            peter_mailbox = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);
            Assert.IsTrue(peter_mailbox.Aliases.Contains(peter_alias));

            peter_mailbox.RemoveAlias(peter_alias.Id);
            peter_mailbox = server.GetMailbox(peter_mailbox.Id, TestContext.ServerFactory);
            Assert.IsFalse(peter_mailbox.Aliases.Contains(peter_alias));
            server.DeleteMailbox(peter_mailbox);
        }

        [ExpectedException("System.Data.DuplicateNameException", UserMessage = "You want to add already existed address")]
        [Test]
        public virtual void DoubleAddSameAddress()
        {
            peter_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
            peter_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException("System.Data.DuplicateNameException", UserMessage = "You want to add already existed address")]
        public virtual void AddingSameAliasForTwoDifferentMailboxesFails()
        {
            peter_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
            peter_second_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
        }

        [Test]
        public virtual void CorrectToStringWorks()
        {
            Assert.AreEqual(peter_address.LocalPart + '@' + peter_address.Domain.Name, peter_address.ToString());
        }

        [Test]
        public virtual void DoubleRemovingSameAddressSameAddress()
        {
            peter_mailbox.AddAlias(peter_alias.LocalPart, peter_alias.Domain, TestContext.ServerFactory);
            peter_mailbox.RemoveAlias(peter_alias.Id);
            peter_mailbox.RemoveAlias(peter_alias.Id);
            Assert.IsFalse(peter_mailbox.Aliases.Contains(peter_alias));
            var mailboxes = server.GetMailboxes(TestContext.ServerFactory);
            Assert.IsTrue(mailboxes.Contains(peter_mailbox));
        }
    }
}