/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using ASC.VoipService.Application.Core;
using ASC.VoipService.Twilio;

using Twilio.TwiML;
using log4net;

namespace ASC.VoipService.Application.Controllers
{
    public class TwilioController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");

        [HttpGet]
        [ActionName("Connect")]
        public HttpResponseMessage Get()
        {
            try
            {
                LogQueryString("Connect");

                return GetQueryParam("Direction") == "inbound" ? Inbound() : Outbound();
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private HttpResponseMessage Inbound()
        {
            ApiHelper.SaveCall(GetQueryParam("CallSid"), GetQueryParam("From"), GetQueryParam("To"), VoipCallStatus.Incoming);
            return GetHttpResponse(TwilioResponseHelper.Inbound());
        }

        private HttpResponseMessage Outbound()
        {
            SignalRHelper.ChangeAgentStatus((int)AgentStatus.Paused);
            ApiHelper.SaveOrUpdateCall(GetQueryParam("CallSid"), GetQueryParam("From"), GetQueryParam("To"), GetQueryParam("CallerId"), status: VoipCallStatus.Outcoming, contactId: GetQueryParam("ContactId"));
            return GetHttpResponse(TwilioResponseHelper.Outbound());
        }

        [HttpGet]
        [ActionName("Client")]
        public HttpResponseMessage Client()
        {
            try
            {
                LogQueryString("Client");

                ApiHelper.SaveOrUpdateCallHistory(GetQueryParam("CallSid"), GetQueryParam("ParentCallSid"), answerDate: DateTime.UtcNow, answeredBy: GetQueryParam("CallerId"));
                SignalRHelper.Start();

                return GetHttpResponse(new TwilioResponse());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("Dial")]
        public HttpResponseMessage Dial()
        {
            try
            {
                LogQueryString("Dial");

                ApiHelper.UploadRecord(GetQueryParam("DialCallSid"), GetQueryParam("CallSid"), GetQueryParam("RecordingUrl"));
                ApiHelper.SaveOrUpdateCallHistory(GetQueryParam("DialCallSid"), GetQueryParam("CallSid"), endDialDate: DateTime.UtcNow, recordUrl: GetQueryParam("RecordingUrl"), recordDuration: GetQueryParam("RecordingDuration"));

                if (!Settings.Pause)
                    SignalRHelper.ChangeAgentStatus((int) AgentStatus.Online);

                SignalRHelper.End();

                Task.Run(() => new ApiHelper(GetQueryParam("Tenant"), GetQueryParam("CallerId")).SavePrice(GetQueryParam("DialCallSid"), GetQueryParam("CallSid")));

                return GetHttpResponse(TwilioResponseHelper.Dial());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("Enqueue")]
        public HttpResponseMessage Enqueue()
        {
            try
            {
                LogQueryString("Enqueue");

                if (GetQueryParam("QueueResult") != "bridged" && GetQueryParam("QueueResult") != "redirected")
                {
                    ApiHelper.SaveOrUpdateCall(GetQueryParam("CallSid"), status: VoipCallStatus.Missed);
                    SignalRHelper.MissCall(GetQueryParam("CallSid"));
                }

                return GetHttpResponse(TwilioResponseHelper.Enqueue(GetQueryParam("QueueResult")));
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
        [HttpGet]
        [ActionName("Queue")]
        public HttpResponseMessage Queue()
        {
            try
            {
                LogQueryString("Queue");

                return GetHttpResponse(TwilioResponseHelper.Queue());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("Dequeue")]
        public HttpResponseMessage Dequeue()
        {
            try
            {
                LogQueryString("Dequeue");

                if (Convert.ToBoolean(GetQueryParam("Reject")))
                {
                    ApiHelper.SaveOrUpdateCall(GetQueryParam("CallSid"), status: VoipCallStatus.Missed);
                    SignalRHelper.MissCall(GetQueryParam("CallSid"));
                    return GetHttpResponse(TwilioResponseHelper.Leave());
                }

                ApiHelper.AnswerCall(GetQueryParam("CallSid"), GetQueryParam("From"), GetQueryParam("To"));

                return GetHttpResponse(TwilioResponseHelper.Dequeue());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("Wait")]
        public HttpResponseMessage Wait()
        {
            try
            {
                LogQueryString("Wait");

                if (Convert.ToInt32(GetQueryParam("QueueTime")) == 0)
                {
                    ApiHelper.SaveOrUpdateCallHistory(GetQueryParam("CallSid"), GetQueryParam("CallSid"), queueDate: DateTime.UtcNow);
                    var to = GetQueryParam("RedirectTo");
                    if (string.IsNullOrEmpty(to))
                    {
                        SignalRHelper.Enqueue(GetQueryParam("CallSid"));
                    }
                    else
                    {
                        SignalRHelper.Incoming(GetQueryParam("CallSid"), to);
                    }
                }

                return GetHttpResponse(TwilioResponseHelper.Wait(GetQueryParam("QueueSid"), GetQueryParam("QueueTime"), GetQueryParam("CurrentQueueSize")));
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("GatherQueue")]
        public HttpResponseMessage GatherQueue()
        {
            try
            {
                LogQueryString("GatherQueue");
                return GetHttpResponse(TwilioResponseHelper.GatherQueue(GetQueryParam("Digits"), GetQueryParam("To").Substring(1)));
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("Redirect")]
        public HttpResponseMessage Redirect()
        {
            try
            {
                LogQueryString("Redirect");

                return GetHttpResponse(TwilioResponseHelper.Redirect(GetQueryParam("RedirectTo")));
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpGet]
        [ActionName("VoiceMail")]
        public HttpResponseMessage VoiceMail()
        {
            try
            {
                LogQueryString("VoiceMail");

                ApiHelper.SaveOrUpdateCall(GetQueryParam("CallSid"), status: VoipCallStatus.Missed);

                SignalRHelper.VoiceMail(GetQueryParam("CallSid"));

                return GetHttpResponse(TwilioResponseHelper.VoiceMail());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        #region private

        private void LogQueryString(string action)
        {
            Log.Info("");
            Log.Info(action);

            var queryString = Request.RequestUri.ParseQueryString();

            foreach (var query in queryString)
            {
                Log.InfoFormat("{0}:{1}", query, queryString[query.ToString()]);
            }
        }

        private VoipSettings settings;
        private VoipSettings Settings
        {
            get
            {
                return settings ?? (settings = ApiHelper.GetSettings(GetQueryParam("Direction") == "inbound" ? GetQueryParam("To") : GetQueryParam("From")));
                
            }
        }

        private TwilioResponseHelper twilioResponseHelper;
        private TwilioResponseHelper TwilioResponseHelper
        {
            get
            {
                return twilioResponseHelper ?? (twilioResponseHelper = new TwilioResponseHelper(Settings, GetQueryParam("Tenant")));
            }
        }

        private ApiHelper apiHelper;
        private ApiHelper ApiHelper
        {
            get { return apiHelper ?? (apiHelper = new ApiHelper(GetQueryParam("Tenant"), GetQueryParam("CallerId"))); }
        }

        private SignalRHelper signalRHelper;
        private SignalRHelper SignalRHelper
        {
            get { return signalRHelper ?? (signalRHelper = new SignalRHelper(GetQueryParam("Tenant"), GetQueryParam("CallerId"), GetQueryParam("Direction") == "inbound" ? GetQueryParam("To") : GetQueryParam("From"))); }
        }

        private static HttpResponseMessage GetHttpResponse(TwilioResponse response)
        {
            Log.Info(response);
            return new HttpResponseMessage { Content = new StringContent(response.ToString(), Encoding.UTF8, "application/xml") };
        }
       

        private string GetQueryParam(string key)
        {
            var querystring = Request.RequestUri.ParseQueryString();

            if (key == "To" || key == "From")
            {
                var result = querystring[key];
                if (result.StartsWith("+"))
                    return result.Substring(1);
                if (result.EndsWith(","))
                    return result.Substring(0, result.Length - 1);
            }

            return querystring[key];
        }

        #endregion
    }
}