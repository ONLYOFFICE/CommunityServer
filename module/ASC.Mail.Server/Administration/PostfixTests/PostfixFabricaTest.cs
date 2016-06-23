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


using System;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.TestCases;
using ASC.Mail.Server.PostfixAdministration;
using NUnit.Framework;

namespace PostfixTests
{
    [TestFixture]
    public class PostfixFabricaTest : FabricaTestBase
    {
        public override TestContextBase TestContext
        {
            get { return new TestContext(); }
        }

        [Test]
        public override void TestCreatedServer()
        {
            try
            {
                base.TestCreatedServer();
                Assert.Fail("Exception wasn't throwed.");
            }
            catch (InvalidPostfixConnectionStringException ex)
            {
                Assert.AreEqual(ex.Message, "Invalid connection string");
            }
            catch (Exception)
            {
                Assert.Fail("Invalid Exception type.");
            }
        }

        [Test]
        public void TestCreatedServerWithJsonConnectionString()
        {
            const string server_connection_string = "{\"DbConnection\" : \"Server=54.228.253.62;Database=vmail;User ID=vmailadmin;Password=ESbWgt5Xym1SRjGfHL0rsi4VVZbldk;Pooling=True;Character Set=utf8\", " +
                                                    "\"Api\":{\"Protocol\":\"http\", \"Server\":\"54.228.253.62\", \"Port\":\"8081\", \"Version\":\"v1\",\"Token\":\"6ffcb0f58a3d950ee49104177f7f1d96\"}}";
            const int server_id = 3;
            const int tenant_id = 1;
            const string user_id = "some_user_id";

            var limits = new ServerLimits.Builder()
                        .SetMailboxMaxCountPerUser(2)
                        .Build();

            var setup = new ServerSetup
                .Builder(server_id, tenant_id, user_id)
                .SetConnectionString(server_connection_string)
                .SetServerLimits(limits)
                .Build();

            var serverBase = factory.CreateServer(setup);
            Assert.AreEqual(serverBase.Tenant, tenant_id);
            Assert.AreEqual(serverBase.User, user_id);
            Assert.AreEqual(serverBase.Id, server_id);
            Assert.AreEqual(serverBase.ConnectionString, server_connection_string);
            Assert.AreEqual(serverBase.Logger.GetType(), typeof(NullLogger));
            Assert.AreEqual(serverBase.SetupInfo.Limits, limits);
        }
    }
}
