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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.VoipService;
using ASC.VoipService.Twilio;
using ASC.Web.CRM.Core;
using ASC.Web.Studio.Utility;
using Autofac;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace ASC.Web.CRM.Classes
{
    [ValidateRequest]
    public class TwilioController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");
        private static readonly object LockObj = new object();

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Index(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                lock (LockObj)
                {
                    using (var scope = DIHelper.Resolve())
                    {
                        var daoFactory = scope.Resolve<DaoFactory>();
                        request.AddAdditionalFields(callerId, contactId);
                        var response = request.IsInbound ? Inbound(request, daoFactory) : Outbound(request, daoFactory);
                        return GetHttpResponse(response);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Client(TwilioVoiceRequest request, [FromUri]Guid callerId)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    request.AddAdditionalFields(callerId);

                    new VoipEngine(scope.Resolve<DaoFactory>()).SaveOrUpdateCall(CallFromTwilioRequest(request));

                    return GetHttpResponse(new VoiceResponse());
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Dial(TwilioVoiceRequest request, [FromUri]Guid callerId, [FromUri]int contactId = 0, [FromUri]string reject = null)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var voipEngine = new VoipEngine(daoFactory);

                    request.AddAdditionalFields(callerId, contactId, reject);

                    var call = CallFromTwilioRequest(request);
                    call = voipEngine.SaveOrUpdateCall(call);

                    var parentCall = daoFactory.VoipDao.GetCall(call.ParentID);

                    if (!string.IsNullOrEmpty(request.RecordingSid))
                    {
                        if (parentCall.VoipRecord == null || string.IsNullOrEmpty(parentCall.VoipRecord.Id))
                        {
                            parentCall.VoipRecord = new VoipRecord {Id = request.RecordingSid};
                        }

                        daoFactory.VoipDao.SaveOrUpdateCall(parentCall);
                    }

                    voipEngine.SaveAdditionalInfo(parentCall.Id);

                    return GetHttpResponse(request.Dial());
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Enqueue(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var voipEngine = new VoipEngine(daoFactory);

                    request.AddAdditionalFields(callerId, contactId);
                    if (request.QueueResult != "bridged" && request.QueueResult != "redirected")
                    {
                        MissCall(request, voipEngine);
                    }

                    return GetHttpResponse(request.Enqueue(request.QueueResult));
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Queue(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId);
                return GetHttpResponse(request.Queue());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Dequeue(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0, [FromUri]string reject = "")
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var voipEngine = new VoipEngine(scope.Resolve<DaoFactory>());
                    request.AddAdditionalFields(callerId, contactId, reject);

                    if (Convert.ToBoolean(request.Reject))
                    {
                        MissCall(request, voipEngine);
                        return GetHttpResponse(request.Leave());
                    }


                    voipEngine.AnswerCall(CallFromTwilioRequest(request));

                    return GetHttpResponse(request.Dequeue());
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Wait(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0, [FromUri]string redirectTo = null)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var voipEngine = new VoipEngine(daoFactory);

                    request.AddAdditionalFields(callerId, contactId, redirectTo: redirectTo);
                    if (Convert.ToInt32(request.QueueTime) == 0)
                    {
                        var history = CallFromTwilioRequest(request);
                        history.ParentID = history.Id;
                        voipEngine.SaveOrUpdateCall(history);

                        var to = request.RedirectTo;
                        if (string.IsNullOrEmpty(to))
                        {
                            request.GetSignalRHelper()
                                .Enqueue(request.CallSid, callerId.HasValue ? callerId.Value.ToString() : "");
                        }
                        else
                        {
                            request.GetSignalRHelper().Incoming(request.CallSid, to);
                        }
                    }

                    return GetHttpResponse(request.Wait());
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage GatherQueue(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId);
                return GetHttpResponse(request.GatherQueue());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Redirect(TwilioVoiceRequest request, [FromUri]string redirectTo, [FromUri]Guid? callerId = null)
        {
            try
            {
                request.AddAdditionalFields(callerId, redirectTo: redirectTo);
                return GetHttpResponse(request.Redirect());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage VoiceMail(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var voipEngine = new VoipEngine(daoFactory);
                    request.AddAdditionalFields(callerId, contactId);

                    MissCall(request, voipEngine);

                    return GetHttpResponse(request.VoiceMail());
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private VoiceResponse Inbound(TwilioVoiceRequest request, DaoFactory daoFactory)
        {
            SecurityContext.AuthenticateMe(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
            var call = SaveCall(request, VoipCallStatus.Incoming, daoFactory);

            return request.Inbound(call, daoFactory);
        }

        private VoiceResponse Outbound(TwilioVoiceRequest request, DaoFactory daoFactory)
        {
            SaveCall(request, VoipCallStatus.Outcoming, daoFactory);

            var history = CallFromTwilioRequest(request);
            history.ParentID = history.Id;
            new VoipEngine(daoFactory).SaveOrUpdateCall(history);

            return request.Outbound();
        }

        private VoipCall SaveCall(TwilioVoiceRequest request, VoipCallStatus status, DaoFactory daoFactory)
        {
            var call = CallFromTwilioRequest(request);
            call.Status = status;
            return daoFactory.VoipDao.SaveOrUpdateCall(call);
        }

        private void MissCall(TwilioVoiceRequest request, VoipEngine voipEngine)
        {
            var voipCall = CallFromTwilioRequest(request);
            voipCall.Status = VoipCallStatus.Missed;

            if (!string.IsNullOrEmpty(request.RecordingSid))
            {
                if (voipCall.VoipRecord == null || string.IsNullOrEmpty(voipCall.VoipRecord.Id))
                {
                    voipCall.VoipRecord = new VoipRecord { Id = request.RecordingSid };
                }
            }

            voipCall = voipEngine.SaveOrUpdateCall(voipCall);
            request.GetSignalRHelper().MissCall(request.CallSid, voipCall.AnsweredBy.ToString());
            voipEngine.SaveAdditionalInfo(voipCall.Id);
        }

        private VoipCall CallFromTwilioRequest(TwilioVoiceRequest request)
        {
            if (!string.IsNullOrEmpty(request.DialCallSid))
            {
                return new VoipCall
                {
                    Id = request.DialCallSid,
                    ParentID = request.CallSid,
                    From = request.From,
                    To = request.To,
                    AnsweredBy = request.CallerId,
                    EndDialDate = DateTime.UtcNow
                };
            }

            return new VoipCall
            {
                Id = request.CallSid,
                ParentID = request.ParentCallSid,
                From = request.From,
                To = request.To,
                AnsweredBy = request.CallerId,
                Date = DateTime.UtcNow,
                ContactId = request.ContactId
            };
        }


        private static HttpResponseMessage GetHttpResponse(VoiceResponse response)
        {
            Log.Info(response);
            return new HttpResponseMessage { Content = new StringContent(response.ToString(), Encoding.UTF8, "application/xml") };
        }
    }

    public class TwilioVoiceRequest : VoiceRequest
    {
        public Guid CallerId { get; set; }
        public int ContactId { get; set; }
        public string ParentCallSid { get; set; }
        public string QueueResult { get; set; }
        public string QueueTime { get; set; }
        public string QueueSid { get; set; }
        public bool Reject { get; set; }
        public string RedirectTo { get; set; }
        public string CurrentQueueSize { get; set; }

        public bool Pause { get { return GetSettings().Pause; } }

        private TwilioResponseHelper twilioResponseHelper;
        private TwilioResponseHelper GetTwilioResponseHelper()
        {
            return twilioResponseHelper ?? (twilioResponseHelper = new TwilioResponseHelper(GetSettings(), CommonLinkUtility.GetFullAbsolutePath("")));
        }

        private VoipSettings settings;
        private VoipSettings GetSettings()
        {
            using (var scope = DIHelper.Resolve())
            {
                return settings ?? (settings = scope.Resolve<DaoFactory>().VoipDao.GetNumber(IsInbound ? To : From).Settings);
            }
        }

        private SignalRHelper signalRHelper;
        public SignalRHelper GetSignalRHelper()
        {
            return signalRHelper ?? (signalRHelper = new SignalRHelper(IsInbound ? To : From));
        }

        public bool IsInbound
        {
            get { return Direction == "inbound"; }
        }

        public void AddAdditionalFields(Guid? callerId, int contactId = 0, string reject = null, string redirectTo = null)
        {
            if (callerId.HasValue && !callerId.Value.Equals(ASC.Core.Configuration.Constants.Guest.ID))
            {
                CallerId = callerId.Value;
                SecurityContext.AuthenticateMe(CallerId);
            }
            if (contactId != 0)
            {
                ContactId = contactId;
            }

            if (!string.IsNullOrEmpty(reject))
            {
                Reject = Convert.ToBoolean(reject);
            }

            if (!string.IsNullOrEmpty(redirectTo))
            {
                RedirectTo = redirectTo;
            }
        }

        internal VoiceResponse Inbound(VoipCall call, DaoFactory daoFactory)
        {
            var contactPhone = call.Status == VoipCallStatus.Incoming || call.Status == VoipCallStatus.Answered
                ? call.From
                : call.To;

            Contact contact;
            var contacts = new VoipEngine(daoFactory).GetContacts(contactPhone, daoFactory);
            var managers = contacts.SelectMany(CRMSecurity.GetAccessSubjectGuidsTo).ToList();
            var agent = GetSignalRHelper().GetAgent(managers);

            if (agent != null && agent.Item1 != null)
            {
                var agentId = agent.Item1.Id;
                SecurityContext.AuthenticateMe(agentId);
                call.AnsweredBy = agentId;

                contact = contacts.FirstOrDefault(CRMSecurity.CanAccessTo);

                daoFactory.VoipDao.SaveOrUpdateCall(call);
            }
            else
            {
                contact = contacts.FirstOrDefault();
            }

            if (contact == null)
            {
                contact = new VoipEngine(daoFactory).CreateContact(call.From.TrimStart('+'));
                call.ContactId = contact.ID;
                daoFactory.VoipDao.SaveOrUpdateCall(call);
            }

            return GetTwilioResponseHelper().Inbound(agent);
        }
        internal VoiceResponse Outbound() { return GetTwilioResponseHelper().Outbound(); }
        internal VoiceResponse Dial() { return GetTwilioResponseHelper().Dial(); }
        internal VoiceResponse Enqueue(string queueResult) { return GetTwilioResponseHelper().Enqueue(queueResult); }
        internal VoiceResponse Queue() { return GetTwilioResponseHelper().Queue(); }
        internal VoiceResponse Leave() { return GetTwilioResponseHelper().Leave(); }
        internal VoiceResponse Dequeue() { return GetTwilioResponseHelper().Dequeue(); }
        internal VoiceResponse Wait() { return GetTwilioResponseHelper().Wait(QueueSid, QueueTime, QueueTime); }
        internal VoiceResponse GatherQueue() { return GetTwilioResponseHelper().GatherQueue(Digits, To.Substring(1), new List<Agent>()); }
        internal VoiceResponse Redirect() { return GetTwilioResponseHelper().Redirect(RedirectTo); }
        internal VoiceResponse VoiceMail() { return GetTwilioResponseHelper().VoiceMail(); }
    }

    public class ValidateRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!new RequestValidationHelper().IsValidRequest(filterContext.HttpContext, ConsumerFactory.Get<Web.Core.Sms.TwilioProvider>()["twilioAuthToken"], HttpContext.Current.Request.GetUrlRewriter().AbsoluteUri))
                filterContext.Result = new Twilio.AspNet.Mvc.HttpStatusCodeResult(HttpStatusCode.Forbidden);
            base.OnActionExecuting(filterContext);
        }
    }
}