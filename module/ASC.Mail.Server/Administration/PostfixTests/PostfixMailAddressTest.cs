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
using ASC.Mail.Server.Administration.TestCases;
using ASC.Mail.Server.PostfixAdministration;
using NUnit.Framework;

namespace PostfixTests
{
    [TestFixture]
    public class PostfixMailAddressTest : MailAddressTestBase
    {
        public override TestContextBase TestContext
        {
            get { return new TestContext(); }
        }

        [Test]
        public void ToAddressDtoConverterTest()
        {
            var recordFromDb = new object[]
                {
                    "test@test.ru", "test@test.com", "test.ru", DateTime.Parse("2014-01-21 08:53:02"),
                    DateTime.Parse("2014-01-21 08:53:03"), 1
                };
            var address = recordFromDb.ToAddressDto();
            Assert.AreEqual("test@test.ru", address.name);
            Assert.AreEqual("test@test.com", address.redirect);
            Assert.AreEqual(true, address.active);
        }

        [Test]
        public void ToAddressDtoThrowsExceptionOnMailGroupTest()
        {
            var recordFromDb = new object[]
                {
                    "test@test.ru", "test@test.com,tester@test.com", "test.ru", DateTime.Parse("2014-01-21 08:53:02"),
                    DateTime.Parse("2014-01-21 08:53:03"), 1
                };
            try
            {
                recordFromDb.ToAddressDto();
                Assert.Fail("Exception wasn't throwed");
            }
            catch (InvalidCastException ex)
            {
                Assert.AreEqual("This value can't be converted to MailAddress: test@test.com,tester@test.com", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Wrong exception type.");
            }
        }
    }
}
