/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    public class PostfixPasswordEncryptorTests
    {
        [Test]
        public void TestMd5HashContainsDollar()
        {
            const string pass = "12345";

            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass);
            Assert.IsTrue(result.Contains("$"));
        }

        [Test]
        public void TestMd5HashContains3Parts()
        {
            const string pass = "12345";

            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass);
            var passParts = result.Split(new[] {'$'}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, passParts.Length);
        }

        [Test]
        public void TestMd5HashFormat()
        {
            const string pass = "12345";

            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass);
            var passParts = result.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            var expectedFormat = String.Format("${0}${1}${2}", passParts[0], passParts[1], passParts[2]);
            Assert.AreEqual(expectedFormat, result);
        }

        [Test]
        public void TestMd5InvalidSaltFormatThrowsException()
        {
            try
            {
                const string pass = "12345";
                PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass, "salt");
                Assert.Fail("Exception wasn't throwed");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid salt format. Should be $1$********", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Invalid esception format");
            }
        }

        [Test]
        public void TestMd5CorrectCalculating()
        {
            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "12345", "$1$12345678");

            // php crypt output
            Assert.AreEqual("$1$12345678$M1o7XeamKvTv64m7bK9e30", result);
        }

        [Test]
        public void TestMd5CorrectCalculating2()
        {
            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "qwerty123", "$1$22222222");

            // php crypt output
            Assert.AreEqual("$1$22222222$RzvaPuLVbLbx2ZZpLHnzm/", result);
        }

        [Test]
        public void TestMd5CorrectWorksWithDollarInSalt()
        {
            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "qwerty123", "$1$$2222222");
            var passParts = result.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, passParts.Length);
        }

        [Test]
        public void TestMd5ReplaceDollarInSaltWithComma()
        {
            var dollarResult = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "qwerty123", "$1$$2222222");
            var commaResult = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "qwerty123", "$1$,2222222");

            Assert.AreEqual(commaResult, dollarResult);
        }
    }
}
