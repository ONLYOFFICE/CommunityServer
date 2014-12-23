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
        const string PeterUsername = "peter";
        const string PeterPassword = "peter_pass";
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool _isVerified = true;
        
        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void Init()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, _isVerified, TestContext.ServerFactory);
            peter_mail_group_address = TestContext.CreateRandomMailAddress(peter_domain);
            var peter_mailbox_address = TestContext.CreateRandomMailAddress(peter_domain);
            var peter_account = TestContext.GetMailAccount(peter_mailbox_address.LocalPart, _peterDomainName);
            peter_mailbox = server.CreateMailbox(peter_mailbox_address.LocalPart, PeterPassword, peter_mailbox_address.Domain, peter_account,
                                                 TestContext.ServerFactory);
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
            var peter_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, TestContext.GetMailAccount(peter_address.LocalPart, _peterDomainName), TestContext.ServerFactory);
            peter_mail_group.AddMember(peter_mailbox.Address.Id, TestContext.ServerFactory);
            var mail_group_members = peter_mail_group.GetMembers(TestContext.ServerFactory);
            Assert.Greater(mail_group_members.Count, 0);
            Assert.IsTrue(mail_group_members.Contains(peter_mailbox.Address));
        }

        [Test]
        public virtual void DoubleAddMemberToMailGroup()
        {
            var peter_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_address.Domain, TestContext.GetMailAccount(peter_address.LocalPart, _peterDomainName), TestContext.ServerFactory);
            peter_mail_group.AddMember(peter_mailbox.Address.Id, TestContext.ServerFactory);
            var mail_group_members = peter_mail_group.GetMembers(TestContext.ServerFactory);
            Assert.Greater(mail_group_members.Count, 0);
            Assert.IsTrue(mail_group_members.Contains(peter_mailbox.Address));
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
            var peter_group_from_server = server.GetMailGroup(peter_mail_group.Id, TestContext.ServerFactory);
            Assert.AreEqual(peter_mail_group, peter_group_from_server);
        }
    }
}
