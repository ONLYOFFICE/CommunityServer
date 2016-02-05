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


using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Aggregator.Common.Extension;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.Imap
{
    [TestFixture]
    class ImapParseMailboxListTests
    {
        [Test]
        public void TestAvsmediaNetInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" INBOX\r\n140827105114648 OK LIST done";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("INBOX", folderList[0].name);
        }

        [Test]
        public void TestGmailInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"INBOX\"\r\n140827105114648 OK LIST done";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("INBOX", folderList[0].name);
        }

        [Test]
        public void TestMailRuInbox()
        {
            const string list_string = "* LIST (\\Inbox) \"/\" \"INBOX\"\r\n140827105114648 OK LIST done";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("INBOX", folderList[0].name);
        }

        [Test]
        public void TestYandexRuInbox()
        {
            const string list_string = "* LIST (\\Marked \\NoInferiors) \"|\" INBOX\r\n140827121936507 OK LIST completed";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("INBOX", folderList[0].name);
        }

        [Test]
        public void TestRamblerRuInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren \\UnMarked) \"/\" INBOX\r\n140827121936507 OK LIST completed";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("INBOX", folderList[0].name);
        }

        [Test]
        public void TestYahooComInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"Inbox\"\r\n140827121936507 OK LIST completed";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("Inbox", folderList[0].name);
        }

        [Test]
        public void TestGmxComInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" INBOX\r\n140827121936507 OK LIST completed";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("INBOX", folderList[0].name);
        }

        [Test]
        public void TestGmailWithQuotaFormat()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"Pelikan/M2/20071010 PTI \"ID\" model\"\r\n140827105114648 OK LIST done";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("Pelikan/M2/20071010 PTI \"ID\" model", folderList[0].name);
        }

        [Test]
        public void TestGmailWhenLastCharDuim()
        {
            const string list_string = "* LIST (\\HasChildren) \"/\" \"Cisco/EFA 11.6\\\"\"\r\n140827105114648 OK LIST done";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("Cisco/EFA 11.6\"", folderList[0].name);
        }

        [Test]
        public void TestWhenFolderNameIsOneChar()
        {
            const string list_string = "* LIST (\\HasChildren) \"/\" \"P\"\r\n140827105114648 OK LIST done";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("P", folderList[0].name);
        }


        [Test]
        public void TestYandexRuWithNewLineMoveFormat()
        {
            const string list_string = "* LIST (\\Unmarked \\HasNoChildren) \"|\" {28}\r\nCisco|test&AKA-\"id\"&AKA-test\r\n140827121936507 OK LIST completed";
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(1, folderList.Count);
            Assert.AreEqual(1, folderList[0].folder_id);
            Assert.AreEqual("Cisco|test&AKA-\"id\"&AKA-test", folderList[0].name);

        }

        [Test]
        public void TestFullParse()
        {
            const string list_response = "* LIST () \"/\" \"INBOX\"\r\n" +
                                         "* LIST () \"/\" \"Borradores\"\r\n" +
                                         "* LIST () \"/\" \"Deleted Items\"\r\n" +
                                         "* LIST () \"/\" \"Drafts\"\r\n" +
                                         "* LIST () \"/\" \"Enviados\"\r\n" +
                                         "* LIST () \"/\" \"Papelera\"\r\n" +
                                         "* LIST () \"/\" \"Sent Items\"\r\n" +
                                         "* LIST () \"/\" \"Sent\"\r\n" +
                                         "* LIST () \"/\" \"Trash\"\r\n" +
                                         "* LIST () \"/\" \"Public Folders/Bayesian Learning\"\r\n" +
                                         "* LIST () \"/\" \"Public Folders/Bayesian Learning/Non-Spam\"\r\n" +
                                         "* LIST () \"/\" \"Public Folders/Bayesian Learning/Spam\"\r\n" + 
                                         "140827103505391 OK LIST completed";

            ImapExtensions.ParseImapMailboxes(list_response, "", MailQueueItemSettings.DefaultFolders).ToList();
        }

        [Test]
        public void TestParseWithoutFolderFlags()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"Trash\"\r\n" +
                                       "* LIST (\\HasNoChildren) \"/\" \"Drafts\"\r\n" +
                                       "* LIST (\\HasNoChildren) \"/\" \"Sent\"\r\n" +
                                       "* LIST (\\HasNoChildren) \"/\" \"Junk\"\r\n" +
                                       "* LIST (\\HasNoChildren) \"/\" \"INBOX\"\r\n" +
                                       "151103112052465 OK List completed.\r\n";

            var respnse = new Dictionary<string, int> { {"INBOX", 1}, {"Sent", 2}, {"Drafts", 3}, {"Junk", 5}, {"Trash", 4} };
            var folderList = ImapExtensions.ParseImapMailboxes(list_string, "", MailQueueItemSettings.DefaultFolders,
                                                                MailQueueItemSettings.SpecialDomainFolders,
                                                                MailQueueItemSettings.SkipImapFlags, 
                                                                MailQueueItemSettings.ImapFlags).ToList();
            Assert.IsTrue(folderList.Any());
            Assert.AreEqual(5, folderList.Count);

            foreach (var folder in folderList)
            {
                Assert.IsTrue(respnse.ContainsKey(folder.name));
                Assert.AreEqual(respnse[folder.name], folder.folder_id);
            }
        }
    }
}
