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
    public class PostfixPasswordEncryptorTests
    {
        [Test]
        public void TestMd5HashContainsDollar()
        {
            var pass = "12345";

            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass);
            Assert.IsTrue(result.Contains("$"));
        }

        [Test]
        public void TestMd5HashContains3Parts()
        {
            var pass = "12345";

            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass);
            var pass_parts = result.Split(new[] {'$'}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, pass_parts.Length);
        }

        [Test]
        public void TestMd5HashFormat()
        {
            var pass = "12345";

            var result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, pass);
            var pass_parts = result.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            var expected_format = String.Format("${0}${1}${2}", pass_parts[0], pass_parts[1], pass_parts[2]);
            Assert.AreEqual(expected_format, result);
        }

        [Test]
        public void TestMd5InvalidSaltFormatThrowsException()
        {
            try
            {
                var pass = "12345";
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
            var pass_parts = result.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, pass_parts.Length);
        }

        [Test]
        public void TestMd5ReplaceDollarInSaltWithComma()
        {
            var dollar_result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "qwerty123", "$1$$2222222");
            var comma_result = PostfixPasswordEncryptor.EncryptString(HashType.Md5, "qwerty123", "$1$,2222222");

            Assert.AreEqual(comma_result, dollar_result);
        }
    }
}
