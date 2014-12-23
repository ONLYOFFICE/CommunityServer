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

using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;
using NUnit.Framework;

namespace ASC.Mail.Server.Administration.TestCases
{
    [TestFixture]
    public abstract class FabricaTestBase
    {
        protected IMailServerFactory factory;
        protected MailServerBase server;

        public abstract TestContextBase TestContext { get; }

        [SetUp]
        public void SetUp()
        {
            factory = TestContext.ServerFactory;
            server = TestContext.CreateServer();
        }

        [Test]
        public virtual void TestCreatedServer()
        {
            var limits = new ServerLimits.Builder()
                        .SetMailboxMaxCountPerUser(2)
                        .Build();

            var setup = new ServerSetup
                .Builder(1, 3, "some_user_id")
                .SetConnectionString("server_connection")
                .SetServerLimits(limits)
                .Build();

            var server = factory.CreateServer(setup);
            Assert.AreEqual(1, server.Id);
            Assert.AreEqual(3, server.Tenant);
            Assert.AreEqual("some_user_id", server.User);
            Assert.AreEqual(server.Logger.GetType(), typeof(NullLogger));
            Assert.AreEqual(server.SetupInfo.Limits, limits);
        }

        [Test]
        public virtual void TestCreatedMailAddress()
        {
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            var test_address = factory.CreateMailAddress(1, 0, "test", domain);

            Assert.IsNotNull(test_address);
            Assert.AreEqual(1, test_address.Id);
            Assert.AreEqual(0, test_address.Tenant);
            Assert.AreEqual("test", test_address.LocalPart);

            Assert.IsNotNull(test_address.Domain);
            Assert.AreEqual(1, test_address.Domain.Id);
            Assert.AreEqual(0, test_address.Domain.Tenant);
            Assert.AreEqual("test.ru", test_address.Domain.Name);
            Assert.AreEqual(true, test_address.Domain.IsVerified);
        }

        [Test]
        public virtual void TestCreatedWebDomain()
        {
            var test_domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            Assert.AreEqual(1, test_domain.Id);
            Assert.AreEqual(0, test_domain.Tenant);
            Assert.AreEqual("test.ru", test_domain.Name);
            Assert.AreEqual(true, test_domain.IsVerified);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException", UserMessage = "Null domain's name")]
        public virtual void TestCreatedWebDomainWithNullName()
        {
            factory.CreateWebDomain(1, 0, null, true, server);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Negative domain teamlab id")]
        public virtual void TestCreatedWebDomainWithNegativeId()
        {
            factory.CreateWebDomain(-1, 0, "test.ru", true, server);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Negative domain teamlab tenant")]
        public virtual void TestCreatedWebDomainWithNegativeTenant()
        {
            factory.CreateWebDomain(1, -1, "test.ru", true, server);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Domain string contains incorrect characters")]
        public virtual void TestCreatedWebDomainWithInvalidName()
        {
            factory.CreateWebDomain(1, 0, "te@st.ru", true, server);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Domain name exceed limitation of 255 characters")]
        public virtual void TestCreatedWebDomainWithExceedLimitOfNameLen()
        {
            const string domain_string_more_255_chars = "theverylonglongverylonglongverylonglongverylonglongverylonglongvery" +
                                                        "longlongverylonglongverylonglongverylonglongverylonglongverylonglong" +
                                                        "verylonglongverylonglongverylonglongverylonglongverylonglongverylonglong" +
                                                        "verylonglongverylonglongverylonglong" +
                                                        "domainname.com";
            factory.CreateWebDomain(1, 0, domain_string_more_255_chars, true, server);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException", UserMessage = "Null address's local part")]
        public virtual void TestCreatedMailAddressWithNullName()
        {
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            factory.CreateMailAddress(1, 0, null, domain);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Negative address teamlab id")]
        public virtual void TestCreatedMailAddressWithNegativeId()
        {
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            factory.CreateMailAddress(-1, 0, "test", domain);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Negative address teamlab tenant")]
        public virtual void TestCreatedMailAddressWithNegativeTenant()
        {
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            factory.CreateMailAddress(1, -1, "test", domain);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Email's local part contains incorrect characters")]
        public virtual void TestCreatedMailAddressWithInvalidLocalPart()
        {
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            factory.CreateMailAddress(1, 0, "te@st", domain);
        }

        [Test]
        [ExpectedException("System.ArgumentException", UserMessage = "Email's local part exceed limitation of 64 characters")]
        public virtual void TestCreatedMailAddressWithExceedLimitOfLocalPartLen()
        {
            const string email_local_part_more_64_chars = "theverylonglongverylonglongverylonglongrealverylonglongaddressname";
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            factory.CreateMailAddress(1, 0, email_local_part_more_64_chars, domain);
        }

        //TODO: Add factory tests for Mailbox, MailGroup, MailAccount
    }
}
