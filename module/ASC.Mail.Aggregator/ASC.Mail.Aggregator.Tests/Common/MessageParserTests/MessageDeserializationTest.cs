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


using System.IO;
using System.Linq;
using NUnit.Framework;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    [TestFixture]
    class MessageDeserializationTest : MessageParserTestsBase
    {
        const string TEST_FILE_NAME = "test_serialization.eml";
        const string TEST_FILE_PATH = TestFolderPath + TEST_FILE_NAME;


        [SetUp]
        public void PrepareMessageForSerialization()
        {
            var test_mail = new Message
            {
                From = { Email = "test.mail@qip.ru", Name = Codec.RFC2047Encode("test") },
                To = { new Address { Email = "test.mail@qip.ru", Name = Codec.RFC2047Encode("name1") },
                       new Address { Email = "test.mail@gmail.com", Name = Codec.RFC2047Encode("name2") } },
                Subject = Codec.RFC2047Encode("test"),
                BodyText = { Charset = utf8_charset,
                            ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable,
                            Text = "test" },
                BodyHtml = { Charset = utf8_charset,
                             ContentTransferEncoding = ContentTransferEncoding.QuotedPrintable,
                             Text = "<a href='www.teamlab.com'>test</a>" }
            };

            test_mail.StoreToFile(TEST_FILE_PATH);
        }


        [TearDown]
        public void CleanUpSerializedFile()
        {
            File.Delete(TEST_FILE_PATH);
        }


        //This test fails if data directory invalid or corrupted
        [Test]
        public void TestDirectoryPath()
        {
            using (new FileStream(TEST_FILE_PATH, FileMode.Open)) { }
        }


        [Test]
        public void DeserializationOfSerializedMessageTest()
        {
            var test_mail = Parser.ParseMessageFromFile(TEST_FILE_PATH);

            Assert.AreEqual("test.mail@qip.ru", test_mail.From.Email);
            Assert.AreEqual("test", test_mail.From.Name);
            Assert.IsTrue(test_mail.To.Count(mail => (mail.Email == "test.mail@qip.ru") && (mail.Name == "name1")) != 0);
            Assert.IsTrue(test_mail.To.Count(mail => (mail.Email == "test.mail@gmail.com") && (mail.Name == "name2")) != 0);

            Assert.AreEqual("test", test_mail.Subject);
            Assert.AreEqual(0, test_mail.Attachments.Count);

            Assert.AreEqual(utf8_charset, test_mail.BodyText.Charset);
            Assert.AreEqual("test\r\n", test_mail.BodyText.Text);
            Assert.AreEqual("test\r\n", test_mail.BodyText.TextStripped);
            Assert.AreEqual(BodyFormat.Text, test_mail.BodyText.Format);
            Assert.AreEqual(ContentTransferEncoding.QuotedPrintable, test_mail.BodyText.ContentTransferEncoding);

            Assert.AreEqual(utf8_charset, test_mail.BodyHtml.Charset);
            Assert.AreEqual("<a href='www.teamlab.com'>test</a>\r\n", test_mail.BodyHtml.Text);
            Assert.AreEqual("test\r\n", test_mail.BodyHtml.TextStripped);
            Assert.AreEqual(BodyFormat.Html, test_mail.BodyHtml.Format);
            Assert.AreEqual(ContentTransferEncoding.QuotedPrintable, test_mail.BodyHtml.ContentTransferEncoding);
        }
    }
}
