/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Collections.Generic;
using System.Net;
using ASC.Mail.Server.Administration.Interfaces;
using NUnit.Framework;

namespace ASC.Mail.Server.Administration.TestCases
{
    [TestFixture]
    public abstract class MailGroupTestBase
    {
        protected MailServerBase server;
        protected IMailGroup peter_mail_group;
        protected IMailAddress peter_mail_group_address;
        protected IMailbox peter_mailbox;
        protected IWebDomain peter_domain;
        const string PETER_PASSWORD = "peter_pass";
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool IS_VERIFIED = true;
        
        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void Init()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, IS_VERIFIED, TestContext.ServerFactory);
            peter_mail_group_address = TestContext.CreateRandomMailAddress(peter_domain);
            var peterMailboxAddress = TestContext.CreateRandomMailAddress(peter_domain);
            var peterAccount = TestContext.GetMailAccount(peterMailboxAddress.LocalPart, _peterDomainName);
            peter_mailbox = server.CreateMailbox(peterAccount.TeamlabAccount.Name, peterMailboxAddress.LocalPart, 
                                                 PETER_PASSWORD, peterMailboxAddress.Domain, peterAccount, TestContext.ServerFactory);
            peter_mail_group = server.CreateMailGroup(peter_mail_group_address.LocalPart, peter_mail_group_address.Domain, new List<int> { peter_mailbox.Address.Id }, TestContext.ServerFactory);
        }

        [TearDown]
        public void Dispose()
        {
            if (peter_domain == null) return;
            server.DeleteWebDomain(peter_domain, TestContext.ServerFactory);
            peter_domain = null;
        }

        [Test]
        public virtual void CreateMailGroup()
        {
            Assert.IsNotNull(peter_mail_group);
            var groups = server.GetMailGroups(TestContext.ServerFactory);
            Assert.IsTrue(groups.Contains(peter_mail_group));
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailGroupNullName()
        {
            server.CreateMailGroup(null, peter_mail_group_address.Domain, new List<int> { peter_mailbox.Address.Id }, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailGroupNullDomain()
        {
            server.CreateMailGroup(peter_mail_group_address.LocalPart, null, new List<int> { peter_mailbox.Address.Id }, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void CreateMailGroupWithEmptyAddresses()
        {
            server.CreateMailGroup(peter_mail_group_address.LocalPart, peter_mail_group_address.Domain, new List<int>(), TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void CreateMailGroupWithNullFactory()
        {
            server.CreateMailGroup(peter_mail_group_address.LocalPart, peter_mail_group_address.Domain, new List<int> { peter_mailbox.Address.Id }, null);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void CreateDuplicateMailGroup()
        {
            server.CreateMailGroup(peter_mail_group_address.LocalPart, peter_mail_group_address.Domain, new List<int> { peter_mailbox.Address.Id }, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void AddNotExistingAddressIdToMailGroup()
        {
            peter_mail_group.AddMember(0, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void AddNegativeAddressIdToMailGroup()
        {
            peter_mail_group.AddMember(-1, TestContext.ServerFactory);
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentNullException")]
        public virtual void AddAddressIdToMailGroupWithNullFactory()
        {
            peter_mail_group.AddMember(1, null);
        }

        [Test]
        public virtual void AddMemberToMailGroup()
        {
            var peterAddress = TestContext.CreateRandomMailAddress(peter_domain);
            var account = TestContext.GetMailAccount(peterAddress.LocalPart, _peterDomainName);
            peter_mailbox = server.CreateMailbox(account.TeamlabAccount.Name, peterAddress.LocalPart, 
                                                 PETER_PASSWORD, peterAddress.Domain, account, TestContext.ServerFactory);
            peter_mail_group.AddMember(peter_mailbox.Address.Id, TestContext.ServerFactory);
            var mailGroupMembers = peter_mail_group.GetMembers(TestContext.ServerFactory);
            Assert.Greater(mailGroupMembers.Count, 0);
            Assert.IsTrue(mailGroupMembers.Contains(peter_mailbox.Address));
        }

        [Test]
        public virtual void DoubleAddMemberToMailGroup()
        {
            var peterAddress = TestContext.CreateRandomMailAddress(peter_domain);
            var account = TestContext.GetMailAccount(peterAddress.LocalPart, _peterDomainName);
            peter_mailbox = server.CreateMailbox(account.TeamlabAccount.Name, peterAddress.LocalPart, 
                                                 PETER_PASSWORD, peterAddress.Domain, account, TestContext.ServerFactory);
            peter_mail_group.AddMember(peter_mailbox.Address.Id, TestContext.ServerFactory);
            var mailGroupMembers = peter_mail_group.GetMembers(TestContext.ServerFactory);
            Assert.Greater(mailGroupMembers.Count, 0);
            Assert.IsTrue(mailGroupMembers.Contains(peter_mailbox.Address));
            Assert.Throws<ArgumentException>(() => peter_mail_group.AddMember(peter_mailbox.Address.Id, TestContext.ServerFactory));
        }

        [Test]
        public virtual void RemoveMemberFromMailGroup()
        {
            peter_mail_group.RemoveMember(peter_mailbox.Address.Id);
            var members = peter_mail_group.GetMembers(TestContext.ServerFactory);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public virtual void DoubleRemoveMemberFromMailGroup()
        {
            peter_mail_group.RemoveMember(peter_mailbox.Address.Id);
            var members = peter_mail_group.GetMembers(TestContext.ServerFactory);
            Assert.AreEqual(0, members.Count, "No members should be in the mail group.");
            Assert.Throws<ArgumentException>(() => peter_mail_group.RemoveMember(peter_mailbox.Address.Id));
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void RemoveFromMailGroupWithNegativeAddressId()
        {
            peter_mail_group.RemoveMember(-1);
        }

        [Test]
        public virtual void DoubleDeletingMailGroup()
        {
            server.DeleteMailGroup(peter_mail_group.Id, TestContext.ServerFactory);
            Assert.Throws<ArgumentException>(() => server.DeleteMailGroup(peter_mail_group.Id, TestContext.ServerFactory));
            Assert.IsNull(server.GetMailGroup(peter_mail_group.Id, TestContext.ServerFactory));
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.ArgumentException")]
        public virtual void DeletingMailGroupByNegativeId()
        {
            server.DeleteMailGroup(-1, TestContext.ServerFactory);
        }

        [Test]
        public virtual void GetMailgroupReturnsValidMailgroup()
        {
            var peterGroupFromServer = server.GetMailGroup(peter_mail_group.Id, TestContext.ServerFactory);
            Assert.AreEqual(peter_mail_group, peterGroupFromServer);
        }
    }
}
