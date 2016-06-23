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


using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Mail.Aggregator.Core.Clients;
using MimeKit;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    [TestFixture]
    class MessageDeserializationTest : MessageParserTestsBase
    {
        const string TEST_FILE_NAME = "test_serialization.eml";
        const string TEST_FILE_PATH = TestFolderPath + TEST_FILE_NAME;
        private const string TEST_TEXT = "test тест";

        [SetUp]
        public void PrepareMessageForSerialization()
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(TEST_TEXT, "test.mail@qip.ru"));
            message.To.AddRange(new List<InternetAddress>
            {
                new MailboxAddress("name1", "test.mail@qip.ru"),
                new MailboxAddress("name2", "test.mail@gmail.com")
            });
            message.Subject = TEST_TEXT;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = TEST_TEXT,
                HtmlBody = string.Format("<a href='www.teamlab.com'>{0}</a>", TEST_TEXT)
            };

            message.Body = bodyBuilder.ToMessageBody();

            message.WriteTo(TEST_FILE_PATH);
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
            var testMail = MailClient.ParseMimeMessage(TEST_FILE_PATH);
            var from = testMail.From.Mailboxes.FirstOrDefault();

            Assert.AreEqual("test.mail@qip.ru", from == null ? "" : from.Address);
            Assert.AreEqual(TEST_TEXT, from == null ? "" : from.Name);
            Assert.IsTrue(testMail.To.Mailboxes.Count(mail => (mail.Address == "test.mail@qip.ru") && (mail.Name == "name1")) != 0);
            Assert.IsTrue(testMail.To.Mailboxes.Count(mail => (mail.Address == "test.mail@gmail.com") && (mail.Name == "name2")) != 0);

            Assert.AreEqual(TEST_TEXT, testMail.Subject);
            Assert.AreEqual(0, testMail.Attachments.Count());

           // Assert.AreEqual(utf8Charset, testMail.BodyText.Charset);
            Assert.AreEqual(TEST_TEXT, testMail.TextBody);
            //Assert.AreEqual(TEST_TEXT + "\r\n", testMail.BodyText.TextStripped);
            //Assert.AreEqual(BodyFormat.Text, testMail.BodyText.Format);
            //Assert.AreEqual(ContentTransferEncoding.QuotedPrintable, testMail.BodyText.ContentTransferEncoding);

            //Assert.AreEqual(utf8Charset, testMail.BodyHtml.Charset);
            Assert.AreEqual(string.Format("<a href='www.teamlab.com'>{0}</a>", TEST_TEXT), testMail.HtmlBody);
            //Assert.AreEqual(TEST_TEXT + "\r\n", testMail.BodyHtml.TextStripped);
            //Assert.AreEqual(BodyFormat.Html, testMail.BodyHtml.Format);
            //Assert.AreEqual(ContentTransferEncoding.QuotedPrintable, testMail.BodyHtml.ContentTransferEncoding);
        }
    }
}
