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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using Twilio.TwiML;

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

        public TwilioResponse Inbound(Tuple<Agent,bool> agentTuple)
        {
            var agent = agentTuple != null ? agentTuple.Item1 : null;
            var anyOnline = agentTuple != null ? agentTuple.Item2 : false;
            var response = new TwilioResponse();
            
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
                    response.Play(EncodePlay(settings.GreetingAudio));
                }

                response.Enqueue(settings.Queue.Name,
                    new
                    {
                        method = "POST",
                        action = GetEcho("Enqueue", agent != null),
                        waitUrl = GetEcho("Wait", agent != null),
                        waitUrlMethod = "POST"
                    });
            }

            return AddVoiceMail(response);
        }

        public TwilioResponse Outbound()
        {
            return !settings.Caller.AllowOutgoingCalls
                       ? new TwilioResponse()
                       : AddToResponse(new TwilioResponse(), settings.Caller);
        }

        public TwilioResponse Dial()
        {
            return new TwilioResponse();
        }

        public TwilioResponse Queue()
        {
            return new TwilioResponse();
        }

        public TwilioResponse Enqueue(string queueResult)
        {
            return queueResult == "leave" ? AddVoiceMail(new TwilioResponse()) : new TwilioResponse();
        }

        public TwilioResponse Dequeue()
        {
            return AddToResponse(new TwilioResponse(), settings.Caller);
        }

        public TwilioResponse Leave()
        {
            return AddVoiceMail(new TwilioResponse());
        }

        public TwilioResponse Wait(string queueId, string queueTime, string queueSize)
        {
            var response = new TwilioResponse();
            response.AllowedChildren.Add("Leave");

            var queue = settings.Queue;

            if (Convert.ToInt32(queueTime) > queue.WaitTime || Convert.ToInt32(queueSize) > queue.Size) return response.Leave();

            if (!string.IsNullOrEmpty(queue.WaitUrl))
            {
                response.BeginGather(new { method = "POST", action = GetEcho("gatherQueue") })
                        .Play(EncodePlay(queue.WaitUrl))
                        .EndGather();
            }

            return response;
        }

        public TwilioResponse GatherQueue(string digits, string number, List<Agent> availableOperators)
        {
            var response = new TwilioResponse();

            if (digits == "#") return AddVoiceMail(response);

            var oper = settings.Operators.Find(r => r.PostFix == digits && availableOperators.Contains(r)) ??
                settings.Operators.FirstOrDefault(r => availableOperators.Contains(r));

            return oper != null ? AddToResponse(response, oper) : response;
        }

        public TwilioResponse Redirect(string to)
        {
            if (to == "hold")
            {
                return new TwilioResponse().Play(EncodePlay(settings.HoldAudio), new {loop = 0});
            }

            Guid newCallerId;

            if (Guid.TryParse(to, out newCallerId))
            {
                SecurityContext.AuthenticateMe(newCallerId);
            }

            return new TwilioResponse().Enqueue(settings.Queue.Name,
                new
                {
                    method = "POST",
                    action = GetEcho("enqueue"),
                    waitUrl = GetEcho("wait") + "&RedirectTo=" + to,
                    waitUrlMethod = "POST"
                });
        }

        public TwilioResponse VoiceMail()
        {
            return new TwilioResponse();
        }

        private TwilioResponse AddToResponse(TwilioResponse response, Agent agent)
        {
            var dialAttributes = new { method = "POST", action = GetEcho("dial"), timeout = agent.TimeOut, record = agent.Record ? "record-from-answer" : "do-not-record" };

            switch (agent.Answer)
            {
                case AnswerType.Number:
                    var number = new Number(agent.RedirectToNumber);
                    AddUrlAttr(number, GetEcho("client"));
                    response.Dial(number, dialAttributes);
                    break;
                case AnswerType.Client:
                    var client = new Client(agent.ClientID);
                    AddUrlAttr(client, GetEcho("client"));
                    response.Dial(client, dialAttributes);
                    break;
                case AnswerType.Sip:
                    response.Dial(new Sip(agent.ClientID), dialAttributes);
                    break;
            }

            return response;
        }


        private void AddUrlAttr(ElementBase element, string url)
        {
            if (element.AllowedAttributes == null)
                element.AllowedAttributes = new List<string>();

            element.AllowedAttributes.Add("url");
            element.SetAttributeValue("url", url);

            element.AllowedAttributes.Add("method");
            element.SetAttributeValue("method", "POST");
        }

        private TwilioResponse AddVoiceMail(TwilioResponse response)
        {
            return string.IsNullOrEmpty(settings.VoiceMail)
                       ? response.Say("")
                       : response.Play(EncodePlay(settings.VoiceMail)).Record(new { method = "POST", action = GetEcho("voiceMail"), maxLength = 30 });
        }

        public string GetEcho(string action, bool user = true)
        {
            var result = baseUrl + action;

            if (user)
            {
                result += "?CallerId=" + SecurityContext.CurrentAccount.ID;
            }

            return result;
        }

        private string EncodePlay(string path)
        {
            var lastIndex = path.LastIndexOf('/');
            var start = path.Substring(0, lastIndex);
            var end = path.Substring(lastIndex + 1);
            return  start + '/' + HttpUtility.UrlEncode(end);
        }
    }
}
