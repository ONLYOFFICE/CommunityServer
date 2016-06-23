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


using System.IO;
using System.Linq;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Core.Clients;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    [TestFixture]
    class MessageParseTests
    {
        const string TEST_FOLDER_PATH = @"..\..\Data\";
        const string EML_FILE_NAME = @"nct_attachment_not_parsed.eml";
        const string FILE_PATH = TEST_FOLDER_PATH + EML_FILE_NAME;

        [Test]
        public void ParseMessageTest()
        {
            Assert.IsTrue(File.Exists(FILE_PATH));

            var emlMessage = MailClient.ParseMimeMessage(FILE_PATH);

            Assert.IsNotNull(emlMessage);
        }

        [Test]
        public void ConvertMessageToInternalFormatTest()
        {
            var message = MailClient.ParseMimeMessage(FILE_PATH);

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);
        }

        [Test]
        public void BodyReplaceEmbeddedImages()
        {
            var message = MailClient.ParseMimeMessage(FILE_PATH);

            var mailMessageItem = new MailMessage(message);

            mailMessageItem.ReplaceEmbeddedImages();

            Assert.IsNotEmpty(mailMessageItem.HtmlBody);
        }

        [Test]
        public void BodyWithSubmessages()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "message_with_submessages.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);
        }

        [Test]
        public void BodyWithAttachments()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "with_attachments.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);

            Assert.IsTrue(message.Attachments.Any());
        }

        [Test]
        public void CorruptedMessage1()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "bad_content_type.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);

            Assert.AreEqual(true, mailMessageItem.IsBodyCorrupted);

            Assert.AreEqual(1, mailMessageItem.Attachments.Count);
        }

        [Test]
        public void InvalidAddressesMessage()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "dostavka_ru_bad_content_type.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);

            Assert.AreEqual("", mailMessageItem.From);

            Assert.AreEqual("", mailMessageItem.To);
        }

        [Test]
        public void InvalidAttachmentsFilenames()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "invalid_attachments_filenames.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);

            Assert.IsTrue(message.Attachments.Any());

            Assert.AreEqual(12, mailMessageItem.Attachments.Count);

            var filenames = new string[]
            {
                "_.txt", "_.bin", "_.bin", ".txt", "☼.bin", "♫ .txt", "копия _.bin", "noname.bin", "копия ☼.bin",
                "копия _  (копия).txt", "(копия).txt", "копия ☼  (копия).txt"
            };

            int i, len;
            for (i = 0, len = mailMessageItem.Attachments.Count; i < len; i++)
            {
                var afn = mailMessageItem.Attachments[i].fileName;
                Assert.AreEqual(filenames[i], afn);
            }
        }

        [Test]
        public void MailRuSpam1()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "Message14612877700000000705.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);

            Assert.IsTrue(mailMessageItem.Attachments.Any());

            Assert.AreEqual(1, mailMessageItem.Attachments.Count);
        }

        [Test]
        public void MailRuSpam2()
        {
            var message = MailClient.ParseMimeMessage(TEST_FOLDER_PATH + "Message14634459670000000606.eml");

            var mailMessageItem = new MailMessage(message);

            Assert.IsNotNull(mailMessageItem);

            Assert.IsFalse(message.Attachments.Any());

            Assert.IsTrue(mailMessageItem.IsBodyCorrupted);
        }
    }
}
