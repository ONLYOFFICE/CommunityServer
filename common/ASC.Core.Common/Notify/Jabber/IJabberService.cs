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


using ASC.Core.Common.Notify.Jabber;
using System.Collections.Generic;
using System.ServiceModel;

namespace ASC.Core.Notify.Jabber
{
    [ServiceContract]
    public interface IJabberService
    {
        [OperationContract]
        string GetVersion();

        [OperationContract]
        byte AddXmppConnection(string connectionId, string userName, byte state, int tenantId);

        [OperationContract]
        byte RemoveXmppConnection(string connectionId, string userName, int tenantId);

        [OperationContract]
        int GetNewMessagesCount(int tenantId, string userName);

        [OperationContract]
        string GetUserToken(int tenantId, string userName);

        [OperationContract(IsOneWay = true)]
        void SendCommand(int tenantId, string from, string to, string command, bool fromTenant);

        [OperationContract(IsOneWay = true)]
        void SendMessage(int tenantId, string from, string to, string text, string subject);

        [OperationContract]
        byte SendState(int tenantId, string userName, byte state);

        [OperationContract]
        MessageClass[] GetRecentMessages(int tenantId, string from, string to, int id);

        [OperationContract(IsOneWay = true)]
        void Ping(string userId, int tenantId, string userName, byte state);

        [OperationContract]
        Dictionary<string, byte> GetAllStates(int tenantId, string userName);

        [OperationContract]
        byte GetState(int tenantId, string userName);

        [OperationContract]
        string HealthCheck(string userName, int tenantId);
    }
}