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
        const string PeterPassword = "peter_pass";
        readonly string _peterDomainName = Dns.GetHostName() + ".com";
        private const bool _isVerified = true;

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void Init()
        {
            server = TestContext.CreateServer();
            peter_domain = server.CreateWebDomain(_peterDomainName, _isVerified, TestContext.ServerFactory);
            peter_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_second_address = TestContext.CreateRandomMailAddress(peter_domain);
            peter_alias = TestContext.CreateRandomMailAddress(peter_domain);
            peter_account = TestContext.GetMailAccount(peter_address.LocalPart, _peterDomainName);
            peter_second_account = TestContext.GetMailAccount(peter_second_address.LocalPart, _peterDomainName);
            peter_mailbox = server.CreateMailbox(peter_address.LocalPart, PeterPassword, peter_domain, peter_account, TestContext.ServerFactory);
            peter_second_mailbox = server.CreateMailbox(peter_second_address.LocalPart, PeterPassword, peter_domain, peter_second_account, TestContext.ServerFactory);
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