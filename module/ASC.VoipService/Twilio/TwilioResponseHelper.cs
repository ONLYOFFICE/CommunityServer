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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using Twilio.TwiML;
using Twilio.Types;

namespace ASC.VoipService.Twilio
{
    public class TwilioResponseHelper
    {
        private readonly VoipSettings settings;
        private readonly string baseUrl;

        public TwilioResponseHelper(VoipSettings settings, string baseUrl)
        {
            this.settings = settings;
            this.baseUrl = baseUrl.TrimEnd('/') + "/twilio/";
        }

        public VoiceResponse Inbound(Tuple<Agent,bool> agentTuple)
        {
            var agent = agentTuple != null ? agentTuple.Item1 : null;
            var anyOnline = agentTuple != null ? agentTuple.Item2 : false;
            var response = new VoiceResponse();
            
            if (settings.WorkingHours != null && settings.WorkingHours.Enabled)
            {
                var now = TenantUtil.DateTimeFromUtc(DateTime.UtcNow);
                if (!(settings.WorkingHours.From <= now.TimeOfDay && settings.WorkingHours.To >= now.TimeOfDay))
                {
                    return AddVoiceMail(response);
                }
            }

            if (anyOnline)
            {
                if (!string.IsNullOrEmpty(settings.GreetingAudio))
                {
                    response.Play(Uri.EscapeUriString(settings.GreetingAudio));
                }

                response.Enqueue(settings.Queue.Name, GetEcho("Enqueue", agent != null), "POST",
                    GetEcho("Wait", agent != null), "POST");
            }

            return AddVoiceMail(response);
        }

        public VoiceResponse Outbound()
        {
            return !settings.Caller.AllowOutgoingCalls
                       ? new VoiceResponse()
                       : AddToResponse(new VoiceResponse(), settings.Caller);
        }

        public VoiceResponse Dial()
        {
            return new VoiceResponse();
        }

        public VoiceResponse Queue()
        {
            return new VoiceResponse();
        }

        public VoiceResponse Enqueue(string queueResult)
        {
            return queueResult == "leave" ? AddVoiceMail(new VoiceResponse()) : new VoiceResponse();
        }

        public VoiceResponse Dequeue()
        {
            return AddToResponse(new VoiceResponse(), settings.Caller);
        }

        public VoiceResponse Leave()
        {
            return AddVoiceMail(new VoiceResponse());
        }

        public VoiceResponse Wait(string queueId, string queueTime, string queueSize)
        {
            var response = new VoiceResponse();
            var queue = settings.Queue;

            if (Convert.ToInt32(queueTime) > queue.WaitTime || Convert.ToInt32(queueSize) > queue.Size) return response.Leave();

            if (!string.IsNullOrEmpty(queue.WaitUrl))
            {
                var gather = new Gather(method: "POST", action: GetEcho("gatherQueue"));
                gather.Play(Uri.EscapeUriString(queue.WaitUrl));
                response.Gather(gather);
            }
            else
            {
                response.Pause(queue.WaitTime);
            }

            return response;
        }

        public VoiceResponse GatherQueue(string digits, string number, List<Agent> availableOperators)
        {
            var response = new VoiceResponse();

            if (digits == "#") return AddVoiceMail(response);

            var oper = settings.Operators.Find(r => r.PostFix == digits && availableOperators.Contains(r)) ??
                settings.Operators.FirstOrDefault(r => availableOperators.Contains(r));

            return oper != null ? AddToResponse(response, oper) : response;
        }

        public VoiceResponse Redirect(string to)
        {
            if (to == "hold")
            {
                return new VoiceResponse().Play(Uri.EscapeUriString(settings.HoldAudio), 0);
            }

            Guid newCallerId;

            if (Guid.TryParse(to, out newCallerId))
            {
                SecurityContext.AuthenticateMe(newCallerId);
            }

            return new VoiceResponse().Enqueue(settings.Queue.Name, GetEcho("enqueue"), "POST",
                GetEcho("wait") + "&RedirectTo=" + to, "POST");
        }

        public VoiceResponse VoiceMail()
        {
            return new VoiceResponse();
        }

        private VoiceResponse AddToResponse(VoiceResponse response, Agent agent)
        {
            var dial = new Dial(method: "POST", action: GetEcho("dial"), timeout: agent.TimeOut, record: agent.Record ? "record-from-answer" : "do-not-record");

            switch (agent.Answer)
            {
                case AnswerType.Number:
                    response.Dial(dial.Number(agent.PhoneNumber, method: "POST", url: GetEcho("client")));
                    break;
                case AnswerType.Client:
                    response.Dial(dial.Client(agent.ClientID, "POST", GetEcho("client")));
                    break;
                case AnswerType.Sip:
                    response.Dial(dial.Sip(agent.ClientID, method: "POST", url: GetEcho("client")));
                    break;
            }

            return response;
        }


        private VoiceResponse AddVoiceMail(VoiceResponse response)
        {
            return string.IsNullOrEmpty(settings.VoiceMail)
                       ? response.Say("")
                       : response.Play(Uri.EscapeUriString(settings.VoiceMail)).Record(method: "POST", action: GetEcho("voiceMail"), maxLength: 30);
        }

        public string GetEcho(string action, bool user = true)
        {
            var result = baseUrl.TrimEnd('/');

            if (!string.IsNullOrEmpty(action))
            {
                result += "/" + action.TrimStart('/');
            }
            if (user)
            {
                result += "?CallerId=" + SecurityContext.CurrentAccount.ID;
            }

            return result;
        }
    }
}
