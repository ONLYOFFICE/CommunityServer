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


using ASC.Common.Module;
using ASC.Core.Common.Notify.Jabber;
using System;
using System.Collections.Generic;

namespace ASC.Core.Notify.Jabber
{
    public class JabberServiceClientWcf : BaseWcfClient<IJabberService>, IJabberService, IDisposable
    {
        public JabberServiceClientWcf()
        {
        }

        public string GetVersion()
        {
            return Channel.GetVersion();
        }
        public byte AddXmppConnection(string connectionId, string userName, byte state, int tenantId)
        {
            return Channel.AddXmppConnection(connectionId, userName, state, tenantId);
        }

        public byte RemoveXmppConnection(string connectionId, string userName, int tenantId)
        {
            return Channel.RemoveXmppConnection(connectionId, userName, tenantId);
        }

        public int GetNewMessagesCount(int tenantId, string userName)
        {
            return Channel.GetNewMessagesCount(tenantId, userName);
        }

        public string GetUserToken(int tenantId, string userName)
        {
            return Channel.GetUserToken(tenantId, userName);
        }

        public void SendCommand(int tenantId, string from, string to, string command, bool fromTenant)
        {
            Channel.SendCommand(tenantId, from, to, command, fromTenant);
        }

        public void SendMessage(int tenantId, string from, string to, string text, string subject)
        {
            Channel.SendMessage(tenantId, from, to, text, subject);
        }

        public byte SendState(int tenantId, string userName, byte state)
        {
            return Channel.SendState(tenantId, userName, state);
        }

        public MessageClass[] GetRecentMessages(int tenantId, string from, string to, int id)
        {
            return Channel.GetRecentMessages(tenantId, from, to, id);
        }

        public Dictionary<string, byte> GetAllStates(int tenantId, string userName)
        {
            return Channel.GetAllStates(tenantId, userName);
        }

        public byte GetState(int tenantId, string userName)
        {
            return Channel.GetState(tenantId, userName);
        }

        public void Ping(string userId, int tenantId, string userName, byte state)
        {
            Channel.Ping(userId, tenantId, userName, state);
        }

        public string HealthCheck(string userName, int tenantId)
        {
            return Channel.HealthCheck(userName, tenantId);
        }
    }
}
