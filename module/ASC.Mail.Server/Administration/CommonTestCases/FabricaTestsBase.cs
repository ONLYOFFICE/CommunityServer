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

            var serverBase = factory.CreateServer(setup);
            Assert.AreEqual(1, serverBase.Id);
            Assert.AreEqual(3, serverBase.Tenant);
            Assert.AreEqual("some_user_id", serverBase.User);
            Assert.AreEqual(serverBase.Logger.GetType(), typeof(NullLogger));
            Assert.AreEqual(serverBase.SetupInfo.Limits, limits);
        }

        [Test]
        public virtual void TestCreatedMailAddress()
        {
            var domain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            var testAddress = factory.CreateMailAddress(1, 0, "test", domain);

            Assert.IsNotNull(testAddress);
            Assert.AreEqual(1, testAddress.Id);
            Assert.AreEqual(0, testAddress.Tenant);
            Assert.AreEqual("test", testAddress.LocalPart);

            Assert.IsNotNull(testAddress.Domain);
            Assert.AreEqual(1, testAddress.Domain.Id);
            Assert.AreEqual(0, testAddress.Domain.Tenant);
            Assert.AreEqual("test.ru", testAddress.Domain.Name);
            Assert.AreEqual(true, testAddress.Domain.IsVerified);
        }

        [Test]
        public virtual void TestCreatedWebDomain()
        {
            var testDomain = factory.CreateWebDomain(1, 0, "test.ru", true, server);
            Assert.AreEqual(1, testDomain.Id);
            Assert.AreEqual(0, testDomain.Tenant);
            Assert.AreEqual("test.ru", testDomain.Name);
            Assert.AreEqual(true, testDomain.IsVerified);
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
