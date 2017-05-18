/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using Twilio;

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

            var call = twilio.InitiateOutboundCall(new CallOptions
            {
                SendDigits = number.Length > 1 ? number[1] + "#" : string.Empty,
                To = number[0],
                From = Number,
                Record = Settings.Caller.Record,
                Url = Settings.Connect(contactId: contactId)
            });

            TwilioProvider.ThrowIfError(call);

            return new VoipCall {Id = call.Sid, From = call.From, To = call.To};
        }

        public override VoipCall LocalCall(string to)
        {
            return Call(Number + "#" + to);
        }

        public override VoipCall RedirectCall(string callId, string to)
        {
            var call = twilio.RedirectCall(callId, Settings.Redirect(to), "POST");
            TwilioProvider.ThrowIfError(call);
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
            var queues = twilio.ListQueues();
            var queue = queues.Queues.FirstOrDefault(r => r.FriendlyName == name);
            if (queue == null)
            {
                queue = twilio.CreateQueue(name);
                TwilioProvider.ThrowIfError(queue);
            }
            return new Queue(queue.Sid, name, size, waitUrl, waitTime);
        }

        public string GetQueue(string name)
        {
            var queues = twilio.ListQueues();
            TwilioProvider.ThrowIfError(queues);
            return queues.Queues.Find(r => r.FriendlyName == name).Sid;
        }

        public IEnumerable<string> QueueCalls(string id)
        {
            var calls = twilio.ListQueueMembers(id);
            TwilioProvider.ThrowIfError(calls);
            return calls.QueueMembers.Select(r => r.CallSid);
        }

        private void AnswerQueueCall(string queueId, string callId, bool reject = false)
        {
            var calls = QueueCalls(queueId);
            if (calls.Contains(callId))
                twilio.DequeueQueueMember(queueId, callId, Settings.Dequeue(reject), "POST");
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
