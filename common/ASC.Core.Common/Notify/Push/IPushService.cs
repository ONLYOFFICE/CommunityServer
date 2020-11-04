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
using System.ServiceModel;

namespace ASC.Core.Common.Notify.Push
{
    [ServiceContract]
    public interface IPushService
    {
        [OperationContract]
        string RegisterDevice(int tenantID, string userID, string token, MobileAppType type);

        [OperationContract]
        void DeregisterDevice(int tenantID, string userID, string token);

        [OperationContract]
        void EnqueueNotification(int tenantID, string userID, PushNotification notification, List<string> targetDevices);

        [OperationContract]
        List<PushNotification> GetFeed(int tenantID, string userID, string deviceToken, DateTime from, DateTime to);
    }
}
