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
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Utils;
using NUnit.Framework;

namespace ASC.Mail.GarbageEraser.Tests
{
    [TestFixture]
    public class UserMailEraseTests
    {
        public UserInfo TestUser1 { get; private set; }
        public UserInfo TestUser2 { get; private set; }

        public List<MailBox> TestUser1Mailboxes { get; set; }
        public List<MailBox> TestUser2Mailboxes { get; set; }

        public const int CURRENT_TENANT = 0;
        public const string PASSWORD = "123456";
        public const string DOMAIN = "gmail.com";

        private MailBoxManager _mailBoxManager;

        [SetUp]
        public void Init()
        {
            var startDate = DateTime.Now;

            var email = string.Format("1test_{0}_{1}_{2}_{3}@{4}",
                startDate.Day, startDate.Month, startDate.Year,
                startDate.Ticks, DOMAIN);

            var userInfo = new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = "1Test_UserName_" + startDate,
                FirstName = "Test_FirstName_" + startDate,
                LastName = "Test_LastName_" + startDate,
                Email = email,
                BirthDate = startDate,
                WorkFromDate = startDate,
                Status = EmployeeStatus.Active,
                ActivationStatus = EmployeeActivationStatus.Activated
            };

            var emailUser2 = string.Format("2test_{0}_{1}_{2}_{3}@{4}",
                startDate.Day, startDate.Month, startDate.Year,
                startDate.Ticks, DOMAIN);

            var userInfo2 = new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = "2Test_UserName_" + startDate,
                FirstName = "Test_FirstName_" + startDate,
                LastName = "Test_LastName_" + startDate,
                Email = emailUser2,
                BirthDate = startDate,
                WorkFromDate = startDate,
                Status = EmployeeStatus.Active,
                ActivationStatus = EmployeeActivationStatus.Activated
            };

            CoreContext.TenantManager.SetCurrentTenant(CURRENT_TENANT);

            SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);

            TestUser1 = CoreContext.UserManager.SaveUserInfo(userInfo);

            SecurityContext.SetUserPassword(TestUser1.ID, PASSWORD);

            TestUser2 = CoreContext.UserManager.SaveUserInfo(userInfo2);

            SecurityContext.SetUserPassword(TestUser2.ID, PASSWORD);

            _mailBoxManager = new MailBoxManager();

            var mailboxSettings = _mailBoxManager.GetMailBoxSettings(DOMAIN);

            var testMailboxes = mailboxSettings.ToMailboxList(email, PASSWORD, CURRENT_TENANT, TestUser1.ID.ToString());

            var mbox = testMailboxes.FirstOrDefault();

            if (!_mailBoxManager.SaveMailBox(mbox))
            {
                throw new Exception(string.Format("Can't create mailbox with email: {0}", email));
            }

            testMailboxes = mailboxSettings.ToMailboxList(emailUser2, PASSWORD, CURRENT_TENANT, TestUser2.ID.ToString());

            var mboxUser2 = testMailboxes.FirstOrDefault();

            if (!_mailBoxManager.SaveMailBox(mboxUser2))
            {
                throw new Exception(string.Format("Can't create mailbox with email: {0}", emailUser2));
            }

            TestUser2Mailboxes = new List<MailBox> {mboxUser2};

            var email2 = "test2@mail.ru";

            var test2Mailboxes = mailboxSettings.ToMailboxList(email2, PASSWORD, CURRENT_TENANT, TestUser1.ID.ToString());

            var mbox2 = test2Mailboxes.FirstOrDefault();

            if (!_mailBoxManager.SaveMailBox(mbox2))
            {
                throw new Exception(string.Format("Can't create mailbox with email: {0}", email2));
            }

            var email3 = "test3@mail.ru";

            var test3Mailboxes = mailboxSettings.ToMailboxList(email3, PASSWORD, CURRENT_TENANT, TestUser1.ID.ToString());

            var mbox3 = test3Mailboxes.FirstOrDefault();

            if (!_mailBoxManager.SaveMailBox(mbox3))
            {
                throw new Exception(string.Format("Can't create mailbox with email: {0}", email3));
            }

            TestUser1Mailboxes = new List<MailBox> {mbox, mbox2, mbox3};
        }

        [TearDown]
        public void Cleanup()
        {
            if (TestUser1 == null || TestUser1.ID == Guid.Empty) 
                return;

            CoreContext.TenantManager.SetCurrentTenant(CURRENT_TENANT);

            SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);

            CoreContext.UserManager.DeleteUser(TestUser1.ID);
            CoreContext.UserManager.DeleteUser(TestUser2.ID);

            // Clear TestUser1 mailboxes
            var eraser = new MailGarbageEraser();
            eraser.ClearUserMail(TestUser1.ID, CoreContext.TenantManager.GetCurrentTenant());

            // Clear TestUser2 mailboxes
            eraser = new MailGarbageEraser();
            eraser.ClearUserMail(TestUser2.ID, CoreContext.TenantManager.GetCurrentTenant());
        }

        [Test]
        public void ClearUserMailTest()
        {
            var eraser = new MailGarbageEraser();

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            eraser.ClearUserMail(TestUser1.ID, tenant);

            foreach (var mailbox in TestUser1Mailboxes)
            {
                var mb = _mailBoxManager.GetMailBox(mailbox.MailBoxId);
                Assert.Null(mb);
            }

            foreach (var mailbox in TestUser2Mailboxes)
            {
                var mb = _mailBoxManager.GetMailBox(mailbox.MailBoxId);
                Assert.NotNull(mb);
            }
        }

        [Test]
        public void ClearInvalidUserMailTest()
        {
            var eraser = new MailGarbageEraser();

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            eraser.ClearUserMail(Guid.NewGuid(), tenant);
        }
    }
}
