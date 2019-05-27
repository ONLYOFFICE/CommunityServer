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
