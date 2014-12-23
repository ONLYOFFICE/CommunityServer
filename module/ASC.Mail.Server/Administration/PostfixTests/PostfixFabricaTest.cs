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

            var server_base = factory.CreateServer(setup);
            Assert.AreEqual(server_base.Tenant, tenant_id);
            Assert.AreEqual(server_base.User, user_id);
            Assert.AreEqual(server_base.Id, server_id);
            Assert.AreEqual(server_base.ConnectionString, server_connection_string);
            Assert.AreEqual(server_base.Logger.GetType(), typeof(NullLogger));
            Assert.AreEqual(server_base.SetupInfo.Limits, limits);
        }
    }
}
