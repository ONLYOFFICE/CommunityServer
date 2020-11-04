/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using ASC.Common.Module;

namespace ASC.Core.Common.Notify.Push
{
    public class PushServiceClient : BaseWcfClient<IPushService>, IPushService
    {
        public string RegisterDevice(int tenantID, string userID, string token, MobileAppType type)
        {
            return Channel.RegisterDevice(tenantID, userID, token, type);
        }

        public void DeregisterDevice(int tenantID, string userID, string token)
        {
            Channel.DeregisterDevice(tenantID, userID, token);
        }

        public void EnqueueNotification(int tenantID, string userID, PushNotification notification, List<string> targetDevices)
        {
            Channel.EnqueueNotification(tenantID, userID, notification, targetDevices);
        }

        public List<PushNotification> GetFeed(int tenantID, string userID, string deviceToken, DateTime from, DateTime to)
        {
            return Channel.GetFeed(tenantID, userID, deviceToken, from, to);
        }
    }
}
