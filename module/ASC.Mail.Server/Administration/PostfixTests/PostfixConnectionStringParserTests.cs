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
using ASC.Mail.Server.PostfixAdministration;
using NUnit.Framework;

namespace PostfixTests
{
    [TestFixture]
    public class PostfixConnectionStringParserTests
    {
        [Test]
        public void ParseEasyString()
        {
            const string connection_str = "{\"DbConnection\":\"connection\"}";
            var parser = new PostfixConnectionStringParser(connection_str);
            Assert.AreEqual("connection", parser.PostfixAdminDbConnectionString);
        }

        [Test]
        public void ParseEasyStringWithUselessValues()
        {
            const string connection_str = "{\"DbConnection\": \"connection\",\"AdminName\":\"test\"}";
            var parser = new PostfixConnectionStringParser(connection_str);
            Assert.AreEqual("connection", parser.PostfixAdminDbConnectionString);
        }

        [Test]
        public void ParseStringWithRealDAte()
        {
            const string connection_str = "{\"DbConnection\": \"Server=teamlab;Database=Test;User ID=qqq;Password=qqq;Pooling=True;Character Set=utf8\",\"AdminName\":\"test@test.test\"}";
            var parser = new PostfixConnectionStringParser(connection_str);
            Assert.AreEqual("Server=teamlab;Database=Test;User ID=qqq;Password=qqq;Pooling=True;Character Set=utf8", parser.PostfixAdminDbConnectionString);
        }

        [Test]
        public void ParseConfigWithoutDbConnectionThrowsException()
        {
            const string connection_str = "{\"AdminName\":\"test@test.test\"}";
            try
            {
// ReSharper disable ObjectCreationAsStatement
                new PostfixConnectionStringParser(connection_str);
// ReSharper restore ObjectCreationAsStatement
                Assert.Fail("Exception wasn't throwed");
            }
            catch (InvalidPostfixConnectionStringException ex)
            {
                Assert.AreEqual("Invalid connection string. Some keys wasn't founded", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Invalid exception type. Must be: InvalidPostfixConnectionStringException");
            }
        }

        [Test]
        public void ParseEmptyStringThrowsException()
        {
            const string connection_str = "";
            try
            {
// ReSharper disable ObjectCreationAsStatement
                new PostfixConnectionStringParser(connection_str);
// ReSharper restore ObjectCreationAsStatement
                Assert.Fail("Exception wasn't throwed");
            }
            catch (InvalidPostfixConnectionStringException ex)
            {
                Assert.AreEqual("Invalid connection string", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Invalid exception type. Must be: InvalidPostfixConnectionStringException");
            }
        }

        [Test]
        public void ParseEmptyConfigThrowsException()
        {
            const string connection_str = "{}";
            try
            {
// ReSharper disable ObjectCreationAsStatement
                new PostfixConnectionStringParser(connection_str);
// ReSharper restore ObjectCreationAsStatement
                Assert.Fail("Exception wasn't throwed");
            }
            catch (InvalidPostfixConnectionStringException ex)
            {
                Assert.AreEqual("Invalid connection string. Some keys wasn't founded", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Invalid exception type. Must be: InvalidPostfixConnectionStringException");
            }
        }

        [Test]
        public void ParseNonJsonConfigThrowsException()
        {
            const string connection_str = "<config><dbConnection>connection</dbConnection><AdminName>qqqq</AdminName></config>";
            try
            {
// ReSharper disable ObjectCreationAsStatement
                new PostfixConnectionStringParser(connection_str);
// ReSharper restore ObjectCreationAsStatement
                Assert.Fail("Exception wasn't throwed");
            }
            catch (InvalidPostfixConnectionStringException ex)
            {
                Assert.AreEqual("Invalid connection string", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Invalid exception type. Must be: InvalidPostfixConnectionStringException");
            }
        }
    }
}
