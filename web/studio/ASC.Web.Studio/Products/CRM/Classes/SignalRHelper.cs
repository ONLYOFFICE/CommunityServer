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
using ASC.Core.Notify.Signalr;
using ASC.VoipService;

namespace ASC.Web.CRM.Classes
{
    public class SignalRHelper
    {
        private readonly string numberId;
        private readonly SignalrServiceClient signalrServiceClient;

        public SignalRHelper(string numberId)
        {
            signalrServiceClient = new SignalrServiceClient("voip");
            this.numberId = numberId.TrimStart('+');
        }

        public void Enqueue(string call, string agent)
        {
            signalrServiceClient.EnqueueCall(numberId, call, agent);
        }

        public void Incoming(string call, string agent)
        {
            signalrServiceClient.IncomingCall(call, agent);
        }

        public void MissCall(string call, string agent)
        {
            signalrServiceClient.MissCall(numberId, call, agent);
        }

        public void Reload(string agentId = null)
        {
            signalrServiceClient.Reload(numberId, agentId);
        }

        public Tuple<Agent, bool> GetAgent(List<Guid> contactsResponsibles)
        {
            return signalrServiceClient.GetAgent<Tuple<Agent, bool>>(numberId, contactsResponsibles);
        }
    }
}