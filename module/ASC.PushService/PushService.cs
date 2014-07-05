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
using System.ServiceModel;
using ASC.Core.Common.Notify.Push;
using log4net;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using IPushService = ASC.Core.Common.Notify.Push.IPushService;

namespace ASC.PushService
{
    public class PushService : IPushService
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PushService));

        public string RegisterDevice(int tenantID, string userID, string token, DeviceType type)
        {
            if (string.IsNullOrEmpty(token))
                throw new FaultException("empty device token");

            if (string.IsNullOrEmpty(userID))
                throw new FaultException("empty user id");

            var device = GetDeviceDao().GetAll(tenantID, userID).FirstOrDefault(x => x.Token == token);

            if (device == null)
            {
                _log.DebugFormat("register device ({0}, {1}, {2}, {3})", tenantID, userID, token, type);
                device = new Device
                    {
                        TenantID = tenantID,
                        UserID = userID,
                        Token = token,
                        Type = type
                    };

                GetDeviceDao().Save(device);
            }

            return device.RegistrationID;
        }

        public void DeregisterDevice(int tenantID, string userID, string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new FaultException("empty device token");

            var device = GetDeviceDao().GetAll(tenantID, userID).FirstOrDefault(x => x.Token == token);

            if (device == null)
                throw new FaultException(string.Format("can't find device ({0}) registered by user ({1})", token, userID));

            _log.DebugFormat("deregister device ({0}, {1}, {2})", tenantID, userID, token);

            GetDeviceDao().Delete(device.ID);
        }

        public void EnqueueNotification(int tenantID, string userID, PushNotification notification, List<string> targetDevices)
        {
            List<Device> sendForDevices = GetDeviceDao().GetAll(tenantID, userID);

            if (targetDevices != null && targetDevices.Any())
            {
                var restrictedDevices = targetDevices.Where(token => sendForDevices.All(device => device.Token != token)).ToList();

                if (restrictedDevices.Any())
                    throw new FaultException(string.Format("can't send for devices ({0})", string.Join("; ", restrictedDevices)));

                sendForDevices = sendForDevices.Where(device => targetDevices.Contains(device.Token)).ToList();
            }

            if (!sendForDevices.Any())
            {
                _log.DebugFormat("no registered devices for user {0}", userID);
                return;
            }

            foreach (var device in sendForDevices)
            {
                if (string.IsNullOrEmpty(notification.Message) && notification.Badge == device.Badge)
                    continue;

                device.Badge = notification.Badge ?? device.Badge + 1;

                try
                {
                    switch (device.Type)
                    {
                        case DeviceType.Ios:
                            EnqueueApnsNotification(device, notification);
                            break;

                        case DeviceType.Android:
                            EnqueueAndroidNotification(device, notification);
                            break;
                    }
                }
                catch (Exception error)
                {
                    _log.WarnFormat("enqueue notification {0} for device {1} error: {2}", notification.Message, device.ID, error);
                }
                
                GetDeviceDao().Save(device);

                if (notification.Module != PushModule.Unknown)
                    GetNotificationDao().Save(device.ID, notification);
            }
        }

        private void EnqueueApnsNotification(Device device, PushNotification notification)
        {
            var config = PushServiceConfiguration.GetSection();
            
            string message = notification.Message;

            if (message != null && message.Length > config.Apns.MaxMessageLength)
                message = notification.ShortMessage ?? notification.Message;

            if (message != null && message.Length > config.Apns.MaxMessageLength)
            {
                _log.WarnFormat("message is larger than maximum allowed length of {0}", config.Apns.MaxMessageLength);
                return;
            }

            var apnsNotification = new AppleNotification()
                .ForDeviceToken(device.Token)
                .WithAlert(message)
                .WithBadge(device.Badge)
                .WithCustomItem("regid", device.RegistrationID);

            if (notification.Module != PushModule.Unknown && notification.Item != null)
            {
                var itemType = notification.Item.Type;
                var itemId = notification.Item.ID;

                if (notification.Item.Type == PushItemType.Subtask && notification.ParentItem != null)
                {
                    itemType = notification.ParentItem.Type;
                    itemId = notification.ParentItem.ID;
                }

                apnsNotification.WithCustomItem("itemid", itemId);
                apnsNotification.WithCustomItem("itemtype", itemType.ToString().ToLower());
            }

            if (config.IsDebug || !config.Apns.ElementInformation.IsPresent)
            {
                _log.DebugFormat("notification ({0}) prevented from sending to device {1}", apnsNotification, device.ID);
                return;
            }

            PushBrokerProvider.Instance.QueueNotification(apnsNotification);
        }

        private void EnqueueAndroidNotification(Device device, PushNotification notification)
        {
            var config = PushServiceConfiguration.GetSection();

            string json = string.Format(@"{{""message"":""{0}"",""msgcnt"":""{1}"",""regid"":""{2}""", notification.Message, device.Badge, device.RegistrationID);
            if (notification.Module != PushModule.Unknown && notification.Item != null)
            {
                var itemType = notification.Item.Type;
                var itemId = notification.Item.ID;

                if (notification.Item.Type == PushItemType.Subtask && notification.ParentItem != null)
                {
                    itemType = notification.ParentItem.Type;
                    itemId = notification.ParentItem.ID;
                }

                json += string.Format(@",""itemid"":""{0}"",""itemtype"":""{1}""", itemId, itemType.ToString().ToLower());
            }

            json += "}";

            var gcmNotification = new GcmNotification()
                .ForDeviceRegistrationId(device.Token)
                .WithJson(json);

            if (config.IsDebug || !config.Gcm.ElementInformation.IsPresent)
            {
                _log.DebugFormat("notification ({0}) prevented from sending to device {1}", gcmNotification, device.ID);
                return;
            }

            PushBrokerProvider.Instance.QueueNotification(gcmNotification);
        }

        public List<PushNotification> GetFeed(int tenantID, string userID, string deviceToken, DateTime from, DateTime to)
        {
            return GetNotificationDao().GetNotifications(tenantID, userID, deviceToken, from, to);
        }

        private static DeviceDao GetDeviceDao()
        {
            return new DeviceDao();
        }

        private static NotificationDao GetNotificationDao()
        {
            return new NotificationDao();
        }
    }
}
