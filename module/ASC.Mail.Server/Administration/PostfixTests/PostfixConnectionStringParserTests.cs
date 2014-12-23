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
            var connection_str = "{\"DbConnection\":\"connection\"}";
            var parser = new PostfixConnectionStringParser(connection_str);
            Assert.AreEqual("connection", parser.PostfixAdminDbConnectionString);
        }

        [Test]
        public void ParseEasyStringWithUselessValues()
        {
            var connection_str = "{\"DbConnection\": \"connection\",\"AdminName\":\"test\"}";
            var parser = new PostfixConnectionStringParser(connection_str);
            Assert.AreEqual("connection", parser.PostfixAdminDbConnectionString);
        }

        [Test]
        public void ParseStringWithRealDAte()
        {
            var connection_str = "{\"DbConnection\": \"Server=teamlab;Database=Test;User ID=qqq;Password=qqq;Pooling=True;Character Set=utf8\",\"AdminName\":\"test@test.test\"}";
            var parser = new PostfixConnectionStringParser(connection_str);
            Assert.AreEqual("Server=teamlab;Database=Test;User ID=qqq;Password=qqq;Pooling=True;Character Set=utf8", parser.PostfixAdminDbConnectionString);
        }

        [Test]
        public void ParseConfigWithoutDbConnectionThrowsException()
        {
            var connection_str = "{\"AdminName\":\"test@test.test\"}";
            try
            {
                var parser = new PostfixConnectionStringParser(connection_str);
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
            var connection_str = "";
            try
            {
                var parser = new PostfixConnectionStringParser(connection_str);
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
            var connection_str = "{}";
            try
            {
                var parser = new PostfixConnectionStringParser(connection_str);
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
            var connection_str = "<config><dbConnection>connection</dbConnection><AdminName>qqqq</AdminName></config>";
            try
            {
                var parser = new PostfixConnectionStringParser(connection_str);
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
