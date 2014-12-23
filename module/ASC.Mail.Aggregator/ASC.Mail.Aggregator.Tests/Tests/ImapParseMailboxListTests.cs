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

using System.Linq;
using ASC.Mail.Aggregator.Common.Extension;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests
{
    [TestFixture]
    class ImapParseMailboxListTests
    {
        [Test]
        public void TestAvsmediaNetInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" INBOX\r\n140827105114648 OK LIST done";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("INBOX", folder_list[0].name);
        }

        [Test]
        public void TestGmailInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"INBOX\"\r\n140827105114648 OK LIST done";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("INBOX", folder_list[0].name);
        }

        [Test]
        public void TestMailRuInbox()
        {
            const string list_string = "* LIST (\\Inbox) \"/\" \"INBOX\"\r\n140827105114648 OK LIST done";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("INBOX", folder_list[0].name);
        }

        [Test]
        public void TestYandexRuInbox()
        {
            const string list_string = "* LIST (\\Marked \\NoInferiors) \"|\" INBOX\r\n140827121936507 OK LIST completed";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("INBOX", folder_list[0].name);
        }

        [Test]
        public void TestRamblerRuInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren \\UnMarked) \"/\" INBOX\r\n140827121936507 OK LIST completed";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("INBOX", folder_list[0].name);
        }

        [Test]
        public void TestYahooComInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"Inbox\"\r\n140827121936507 OK LIST completed";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("Inbox", folder_list[0].name);
        }

        [Test]
        public void TestGmxComInbox()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" INBOX\r\n140827121936507 OK LIST completed";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("INBOX", folder_list[0].name);
        }

        [Test]
        public void TestGmailWithQuotaFormat()
        {
            const string list_string = "* LIST (\\HasNoChildren) \"/\" \"Pelikan/M2/20071010 PTI \"ID\" model\"\r\n140827105114648 OK LIST done";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("Pelikan/M2/20071010 PTI \"ID\" model", folder_list[0].name);
        }

        [Test]
        public void TestGmailWhenLastCharDuim()
        {
            const string list_string = "* LIST (\\HasChildren) \"/\" \"Cisco/EFA 11.6\\\"\"\r\n140827105114648 OK LIST done";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("Cisco/EFA 11.6\"", folder_list[0].name);
        }

        [Test]
        public void TestWhenFolderNameIsOneChar()
        {
            const string list_string = "* LIST (\\HasChildren) \"/\" \"P\"\r\n140827105114648 OK LIST done";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("P", folder_list[0].name);
        }


        [Test]
        public void TestYandexRuWithNewLineMoveFormat()
        {
            const string list_string = "* LIST (\\Unmarked \\HasNoChildren) \"|\" {28}\r\nCisco|test&AKA-\"id\"&AKA-test\r\n140827121936507 OK LIST completed";
            var folder_list = ImapExtensions.ParseImapMailboxes(list_string).ToList();
            Assert.IsTrue(folder_list.Any());
            Assert.AreEqual(1, folder_list.Count);
            Assert.AreEqual(1, folder_list[0].folder_id);
            Assert.AreEqual("Cisco|test&AKA-\"id\"&AKA-test", folder_list[0].name);

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

            ImapExtensions.ParseImapMailboxes(list_response).ToList();
        }
    }
}
