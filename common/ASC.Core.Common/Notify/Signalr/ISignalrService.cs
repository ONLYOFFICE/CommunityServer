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

namespace ASC.Core.Notify.Signalr
{
    [ServiceContract]
    public interface ISignalrService
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId, string domain);

        [OperationContract(IsOneWay = true)]
        void SendInvite(string chatRoomName, string calleeUserName, string domain);

        [OperationContract(IsOneWay = true)]
        void SendState(string from, byte state, int tenantId, string domain);

        [OperationContract(IsOneWay = true)]
        void SendOfflineMessages(string callerUserName, List<string> users, int tenantId);

        [OperationContract(IsOneWay = true)]
        void SendUnreadCounts(Dictionary<string, int> unreadCounts, string domain);

        [OperationContract(IsOneWay = true)]
        void SendUnreadUsers(Dictionary<int, HashSet<Guid>> unreadUsers);

        [OperationContract(IsOneWay = true)]
        void SendUnreadUser(int tenant, string userId);

        [OperationContract(IsOneWay = true)]
        void SendMailNotification(int tenant, string userId, int state);
    }
}
