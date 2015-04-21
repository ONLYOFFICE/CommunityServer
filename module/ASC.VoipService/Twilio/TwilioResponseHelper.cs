/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Configuration;
using System.Linq;
using ASC.Core;
using Twilio.TwiML;

namespace ASC.VoipService.Twilio
{
    public class TwilioResponseHelper
    {
        private readonly VoipSettings settings;
        private readonly string tenant;
        private readonly string echoUrl;

        public TwilioResponseHelper(VoipSettings settings, string tenant)
        {
            this.settings = settings;
            this.tenant = tenant;
            echoUrl = ConfigurationManager.AppSettings["echoUrl"] ?? "http://voip.teamlab.info/";
            if (!echoUrl.EndsWith("/"))
                echoUrl += "/";
            echoUrl += "api/twilio/{0}?Tenant={1}";
        }

        public TwilioResponse Inbound()
        {
            var response = new TwilioResponse();

            if (settings.WorkingHours != null && settings.WorkingHours.Enabled)
            {
                if (!(settings.WorkingHours.From <= DateTime.UtcNow.TimeOfDay && settings.WorkingHours.To >= DateTime.UtcNow.TimeOfDay))
                {
                    return AddVoiceMail(response);
                }
            }

            if (settings.AvailableOperators.Any())
            {
                if (!string.IsNullOrEmpty(settings.GreetingAudio))
                {
                    response.Play(settings.GreetingAudio);
                }

                response.Enqueue(settings.Queue.Name, new { method = "GET", action = GetEcho("enqueue"), waitUrl = GetEcho("wait"), waitUrlMethod = "GET" });
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
                response.BeginGather(new { method = "GET", action = GetEcho("gatherQueue") })
                        .Play(queue.WaitUrl)
                        .EndGather();
            }

            return response;
        }

        public TwilioResponse GatherQueue(string digits, string number)
        {
            var response = new TwilioResponse();

            if (digits == "#") return AddVoiceMail(response);

            var oper = settings.AvailableOperators.Find(r => r.PostFix == digits) ?? settings.AvailableOperators.FirstOrDefault();

            return oper != null ? AddToResponse(response, oper) : response;
        }

        public TwilioResponse Redirect(string to)
        {
            return to == "hold"
                       ? new TwilioResponse().Play(settings.HoldAudio, new {loop = 0})
                       : new TwilioResponse().Enqueue(settings.Queue.Name, new { method = "GET", action = GetEcho("enqueue"), waitUrl = GetEcho("wait") + "&RedirectTo=" + to, waitUrlMethod = "GET" });
        }

        public TwilioResponse VoiceMail()
        {
            return new TwilioResponse();
        }

        private TwilioResponse AddToResponse(TwilioResponse response, Agent agent)
        {
            var dialAttributes = new { method = "GET", action = GetEcho("dial"), timeout = agent.TimeOut, record = agent.Record ? "record-from-answer" : "do-not-record" };

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
            element.SetAttributeValue("method", "GET");
        }

        private TwilioResponse AddVoiceMail(TwilioResponse response)
        {
            return settings.VoiceMail == null || !settings.VoiceMail.Enabled
                       ? response
                       : response.Play(settings.VoiceMail.Url).Record(new { method = "GET", action = GetEcho("voiceMail"), maxLength = settings.VoiceMail.TimeOut });
        }

        public string GetEcho(string action, bool user = true)
        {
            var result = string.Format(echoUrl, action, tenant);

            if (user)
            {
                result += "&CallerId=" + SecurityContext.CurrentAccount.ID;
            }

            return result;
        }
    }
}
