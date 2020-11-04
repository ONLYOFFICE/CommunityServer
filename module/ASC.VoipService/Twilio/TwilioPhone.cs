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


using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Twilio;
using Twilio.Clients;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.Queue;
using Twilio.Types;

namespace ASC.VoipService.Twilio
{
    public class TwilioPhone : VoipPhone
    {
        private readonly TwilioRestClient twilio;

        public TwilioPhone(TwilioRestClient twilio) 
        {
            this.twilio = twilio;
            Settings = new TwilioVoipSettings();
        }

        #region Calls

        public override VoipCall Call(string to, string contactId = null)
        {
            var number = to.Split('#');

            var call = CallResource.Create(new CreateCallOptions(new PhoneNumber("+" + number[0].TrimStart('+')), new PhoneNumber("+" + Number.TrimStart('+')))
            {
                SendDigits = number.Length > 1 ? number[1] + "#" : string.Empty,
                Record = Settings.Caller.Record,
                Url = new System.Uri(Settings.Connect(contactId: contactId))
            }, twilio);

            return new VoipCall {Id = call.Sid, From = call.From, To = call.To};
        }

        public override VoipCall LocalCall(string to)
        {
            return Call(Number + "#" + to);
        }

        public override VoipCall RedirectCall(string callId, string to)
        {
            var call =  CallResource.Update(callId, url: new System.Uri(Settings.Redirect(to)), method: HttpMethod.Post, client: twilio);
            return new VoipCall {Id = call.Sid, To = to};
        }

        public override VoipCall HoldUp(string callId)
        {
            return RedirectCall(callId, "hold");
        }

        #endregion

        #region Queue

        public Queue CreateQueue(string name, int size, string waitUrl, int waitTime)
        {
            var queues = QueueResource.Read(new ReadQueueOptions(), twilio);
            var queue = queues.FirstOrDefault(r => r.FriendlyName == name);
            if (queue == null)
            {
                queue = QueueResource.Create(name, client: twilio);
            }
            return new Queue(queue.Sid, name, size, waitUrl, waitTime);
        }

        public string GetQueue(string name)
        {
            var queues = QueueResource.Read(new ReadQueueOptions(), twilio);
            return queues.First(r => r.FriendlyName == name).Sid;
        }

        public IEnumerable<string> QueueCalls(string id)
        {
            var calls = MemberResource.Read(id, client: twilio);
            return calls.Select(r => r.CallSid);
        }

        private void AnswerQueueCall(string queueId, string callId, bool reject = false)
        {
            var calls = QueueCalls(queueId);
            if (calls.Contains(callId))
            {
                MemberResource.Update(queueId, callId, new System.Uri(Settings.Dequeue(reject)), HttpMethod.Post,
                    client: twilio);
            }
        }

        public override void AnswerQueueCall(string callId)
        {
            AnswerQueueCall(Settings.Queue.Id, callId);
        }

        public override void RejectQueueCall(string callId)
        {
            AnswerQueueCall(Settings.Queue.Id, callId, true);
        }

        #endregion
    }
}
