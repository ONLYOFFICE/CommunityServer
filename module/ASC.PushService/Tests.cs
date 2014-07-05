/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Common.Notify.Push;
using log4net.Config;
using NUnit.Framework;

namespace ASC.PushService
{
    [TestFixture]
    internal class TokenDaoTests
    {
        private const int TestTenantID = -1;
        private static readonly Guid TestUserID = new Guid("{EFC0B066-108F-4e68-803E-255062A1A0B6}");
        private readonly DeviceDao _deviceDao = new DeviceDao();

        [Test]
        public void TokenCrudTest()
        {
            var device1 = new Device {Token = "test1", TenantID = TestTenantID, UserID = TestUserID.ToString()};
            var device2 = new Device {Token = "test2", TenantID = TestTenantID, UserID = TestUserID.ToString()};

            _deviceDao.Save(device1);
            _deviceDao.Save(device2);

            var devices = _deviceDao.GetAll(TestTenantID, TestUserID.ToString());
            Assert.AreEqual(2, devices.Count);
            Assert.That(devices.Any(device => device.Token == "test1"));
            Assert.That(devices.Any(device => device.Token == "test2"));

            _deviceDao.UpdateToken("test1", "test1-renamed");

            device2.Badge = 5;
            _deviceDao.Save(device2);

            devices = _deviceDao.GetAll(TestTenantID, TestUserID.ToString());
            Assert.AreEqual(2, devices.Count);
            Assert.That(devices.All(device => device.Token != "test1"));
            Assert.That(devices.Any(device => device.Token == "test1-renamed"));
            Assert.AreEqual(5, devices.First(device => device.Token == "test2").Badge);

            _deviceDao.Delete("test1-renamed");
            _deviceDao.Delete("test2");

            devices = _deviceDao.GetAll(TestTenantID, TestUserID.ToString());
            Assert.AreEqual(0, devices.Count);
        }
    }

    [TestFixture]
    internal class NotificationDaoTests
    {
        private const int TestTenantID = -1;
        private const string TestUserID = "EFC0B066-108F-4e68-803E-255062A1A0B6";

        [TestFixtureSetUp]
        public void SetUp()
        {
            var deviceDao = new DeviceDao();
            for (int i = 0; i < 3; i++)
            {
                deviceDao.Save(new Device
                    {
                        TenantID = TestTenantID,
                        UserID = TestUserID,
                        Token = "device_" + (i + 1)
                    });
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            var deviceDao = new DeviceDao();
            foreach (var device in deviceDao.GetAll(TestTenantID, TestUserID))
            {
                deviceDao.Delete(device.Token);
            }
        }

        [Test]
        public void NotificationCrudTest()
        {
            var notificationDao = new NotificationDao();
            var deviceDao = new DeviceDao();
            foreach (var device in deviceDao.GetAll(TestTenantID, TestUserID))
            {
                notificationDao.Save(device.ID, new PushNotification
                    {
                        Module = PushModule.Projects,
                        Action = PushAction.Created,
                        Item = new PushItem(PushItemType.Task, "task" + device.ID, "")
                    });

                notificationDao.Save(device.ID, new PushNotification
                    {
                        Module = PushModule.Projects,
                        Action = PushAction.InvitedTo,
                        Item = new PushItem(PushItemType.Project, "project" + device.ID, "")
                    });

                notificationDao.Save(device.ID, new PushNotification
                    {
                        Module = PushModule.Projects,
                        Action = PushAction.Assigned,
                        Item = new PushItem(PushItemType.Milestone, "milestone" + device.ID, "")
                    });
            }

            List<PushNotification> notifications = notificationDao.GetNotifications(TestTenantID, TestUserID, "device_1", DateTime.MinValue, DateTime.MaxValue);
            Assert.AreEqual(3, notifications.Count);

            notificationDao.Delete(DateTime.MaxValue);

            notifications = notificationDao.GetNotifications(TestTenantID, TestUserID, "device_1", DateTime.MinValue, DateTime.MaxValue);
            Assert.AreEqual(0, notifications.Count);
        }
    }
}
