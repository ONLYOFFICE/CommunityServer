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
using System.Net.Mail;
using ASC.Mail.Aggregator.Common;
using ActiveUp.Net.Mail;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests
{
    [TestFixture]
    class MailboxManagerTests
    {
        private const int TIME_LIMIT = 45000;
        private MailBoxManager _mailBoxManager;

        [SetUp]
        public void Setup()
        {
            _mailBoxManager = new MailBoxManager(30);
        }

        [Test]
        public void PerformaceTestForSimpleAddingSettingsSearch()
        {
            const int repeat_times = 5;
            var summ = 0.0;
            for (var i = 0; i < repeat_times; ++i)
            {
                var start_time = DateTime.Now;
                try
                {
                    _mailBoxManager.SearchMailboxSettings("eva.mendes.4test@mail.ru", "Isadmin123", "", 1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
                }
                var diff = (DateTime.Now - start_time).TotalMilliseconds;
                Console.WriteLine(diff);
                summ += diff;
            }
            double average_time = summ / repeat_times;
            Console.Write(average_time);
            Assert.LessOrEqual(average_time, TIME_LIMIT);
        }

        [Test]
        public void PerformaceTestForBadMailbox()
        {
            const int repeat_times = 10;
            var summ = 0.0;
            for (var i = 0; i < repeat_times; ++i)
            {
                var start_time = DateTime.Now;
                try
                {
                    _mailBoxManager.SearchMailboxSettings("eva.mendes.4test@qqqqqmail.ru", "Isadmin123", "", 1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
                }
                var diff = (DateTime.Now - start_time).TotalMilliseconds;
                summ += diff;
            }
            var average_time = summ / repeat_times;
            Console.Write(average_time);
            Assert.LessOrEqual(average_time, TIME_LIMIT);
        }

        [Test]
        public void SuccessTest()
        {
            var start_time = DateTime.Now;
            var mbox = _mailBoxManager.SearchMailboxSettings("eva.mendes.4test@mail.ru", "Isadmin123", "", 1);
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNotNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void WrongPasswordTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("eva.mendes.4test@mail.ru", "Isadmin1234", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void WrongAccountTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("eva.mendesasdasdasd@mail.ru", "Isadmin123", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void BadSiteTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("qqqqqq@dropbox.com", "test_pass", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void YandexTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("asc.test.mail@yandex.ru", "Isadmin123", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNotNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        [ExpectedException("ASC.Mail.Aggregator.Common.SmtpConnectionException")]
        public void TestForExceptions()
        {
            _mailBoxManager.SearchMailboxSettings("eva.mendes.4test@mail.ru", "Isadmin1234", "", 1);
        }

        [Test]
        public void HotmailTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("profi.troll@hotmail.com", "Isadmin123", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNotNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void OutlookTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("profi.troll@outlook.com", "Isadmin123", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNotNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void GmailTest()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("asc4test@gmail.com", "Isadmin123", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNotNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void GmailTest2()
        {
            var start_time = DateTime.Now;
            MailBox mbox = null;
            try
            {
                mbox = _mailBoxManager.SearchMailboxSettings("profi.troll.4test@gmail.com", "Isadmin123", "", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchMailboxSettings ExceptionMessage: '" + ex.Message + "'");
            }
            var diff = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine(diff);
            Assert.IsNotNull(mbox);
            Assert.LessOrEqual(diff, TIME_LIMIT);
        }

        [Test]
        public void AolStartTlsTest()
        {
            var profi_trol = new MailBox
                {
                    Name = "",
                    EMail = new MailAddress("profi.troll@aol.com"),
                    
                    Account = "profi.troll",
                    Password = "Isadmin123",
                    AuthenticationTypeIn = SaslMechanism.Login,
                    IncomingEncryptionType = EncryptionType.StartTLS,
                    Imap = true,
                    Port = 143,
                    Server = "imap.aol.com",

                    SmtpAccount = "profi.troll",
                    SmtpPassword = "Isadmin123",
                    AuthenticationTypeSmtp = SaslMechanism.Login,
                    OutcomingEncryptionType = EncryptionType.StartTLS,
                    SmtpPort = 587,
                    SmtpServer = "smtp.aol.com"
                };
            Assert.IsTrue(MailServerHelper.Test(profi_trol));
        }
    }
}
