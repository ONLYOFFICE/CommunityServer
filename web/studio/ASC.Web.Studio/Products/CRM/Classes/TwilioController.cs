using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.VoipService;
using ASC.VoipService.Twilio;
using ASC.Web.Studio.Utility;
using log4net;
using Twilio.Mvc;
using Twilio.TwiML;

namespace ASC.Web.CRM.Classes
{
    [ValidateRequest]
    public class TwilioController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");
        private static readonly object LockObj = new object();
        private readonly VoipEngine voipEngine = new VoipEngine();

        [HttpPost]
        public HttpResponseMessage Index(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                lock (LockObj)
                {
                    request.AddAdditionalFields(callerId, contactId);
                    var response = request.Direction == "inbound" ? Inbound(request) : Outbound(request);
                    return GetHttpResponse(response);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpPost]
        public HttpResponseMessage Client(TwilioVoiceRequest request, [FromUri]Guid callerId)
        {
            try
            {
                request.AddAdditionalFields(callerId);

                voipEngine.SaveOrUpdateCall(CallFromTwilioRequest(request));

                return GetHttpResponse(new TwilioResponse());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpPost]
        public HttpResponseMessage Dial(TwilioVoiceRequest request, [FromUri]Guid callerId, [FromUri]int contactId = 0, [FromUri]string reject = null)
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId, reject);
                
                var call = CallFromTwilioRequest(request);
                call = voipEngine.SaveOrUpdateCall(call);

                var parentCall = Global.DaoFactory.GetVoipDao().GetCall(call.ParentID);

                if (!string.IsNullOrEmpty(request.RecordingSid))
                {    
                    if (parentCall.VoipRecord == null || string.IsNullOrEmpty(parentCall.VoipRecord.Id))
                    {
                        parentCall.VoipRecord = new VoipRecord {Id = request.RecordingSid};
                    }

                    Global.DaoFactory.GetVoipDao().SaveOrUpdateCall(parentCall);
                }

                voipEngine.SaveAdditionalInfo(parentCall.Id);

                return GetHttpResponse(request.Dial());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpPost]
        public HttpResponseMessage Enqueue(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId);
                if (request.QueueResult != "bridged" && request.QueueResult != "redirected")
                {
                    MissCall(request);
                }

                return GetHttpResponse(request.Enqueue(request.QueueResult));
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpPost]
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

        [HttpPost]
        public HttpResponseMessage Dequeue(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0, [FromUri]string reject = "")
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId, reject);

                if (Convert.ToBoolean(request.Reject))
                {
                    MissCall(request);
                    return GetHttpResponse(request.Leave());
                }


                voipEngine.AnswerCall(CallFromTwilioRequest(request));

                return GetHttpResponse(request.Dequeue());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpPost]
        public HttpResponseMessage Wait(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0, [FromUri]string redirectTo = null)
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId, redirectTo: redirectTo);
                if (Convert.ToInt32(request.QueueTime) == 0)
                {
                    var history = CallFromTwilioRequest(request);
                    history.ParentID = history.Id;
                    voipEngine.SaveOrUpdateCall(history);

                    var to = request.RedirectTo;
                    if (string.IsNullOrEmpty(to))
                    {
                        request.GetSignalRHelper().Enqueue(request.CallSid, callerId.HasValue ? callerId.Value.ToString() : "");
                    }
                    else
                    {
                        request.GetSignalRHelper().Incoming(request.CallSid, to);
                    }
                }

                return GetHttpResponse(request.Wait());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        [HttpPost]
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

        [HttpPost]
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

        [HttpPost]
        public HttpResponseMessage VoiceMail(TwilioVoiceRequest request, [FromUri]Guid? callerId = null, [FromUri]int contactId = 0)
        {
            try
            {
                request.AddAdditionalFields(callerId, contactId);

                MissCall(request);

                return GetHttpResponse(request.VoiceMail());
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private TwilioResponse Inbound(TwilioVoiceRequest request)
        {
            SecurityContext.AuthenticateMe(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
            var call = SaveCall(request, VoipCallStatus.Incoming);

            return request.Inbound(call);
        }

        private TwilioResponse Outbound(TwilioVoiceRequest request)
        {
            SaveCall(request, VoipCallStatus.Outcoming);

            var history = CallFromTwilioRequest(request);
            history.ParentID = history.Id;
            voipEngine.SaveOrUpdateCall(history);

            return request.Outbound();
        }

        private VoipCall SaveCall(TwilioVoiceRequest request, VoipCallStatus status)
        {
            var call = CallFromTwilioRequest(request);
            call.Status = status;
            return Global.DaoFactory.GetVoipDao().SaveOrUpdateCall(call);
        }

        private void MissCall(TwilioVoiceRequest request)
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


        private static HttpResponseMessage GetHttpResponse(TwilioResponse response)
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
            return settings ?? (settings = Global.DaoFactory.GetVoipDao().GetNumber(IsInbound ? To : From).Settings);
        }

        private SignalRHelper signalRHelper;
        public SignalRHelper GetSignalRHelper()
        {
            return signalRHelper ?? (signalRHelper = new SignalRHelper(CoreContext.TenantManager.GetCurrentTenant(), CallerId, IsInbound ? To : From));
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

        internal TwilioResponse Inbound(VoipCall call)
        {
            var contactPhone = call.Status == VoipCallStatus.Incoming || call.Status == VoipCallStatus.Answered ? call.From : call.To;

            Contact contact = null;
            var contacts = new VoipEngine().GetContacts(contactPhone);
            var managers = contacts.SelectMany(CRMSecurity.GetAccessSubjectGuidsTo).ToList();
            var agent = GetSignalRHelper().GetAgent(managers);

            if (agent != null && agent.Item1 != null)
            {
                var agentId = agent.Item1.Id;
                SecurityContext.AuthenticateMe(agentId);
                call.AnsweredBy = agentId;

                contact = contacts.FirstOrDefault(CRMSecurity.CanAccessTo);

                Global.DaoFactory.GetVoipDao().SaveOrUpdateCall(call);
            }
            else
            {
                contact = contacts.FirstOrDefault();
            }

            if (contact == null)
            {
                contact = new VoipEngine().CreateContact(call.From.TrimStart('+'));
                call.ContactId = contact.ID;
                Global.DaoFactory.GetVoipDao().SaveOrUpdateCall(call);
            }

            return GetTwilioResponseHelper().Inbound(agent);
        }
        internal TwilioResponse Outbound() { return GetTwilioResponseHelper().Outbound(); }
        internal TwilioResponse Dial() { return GetTwilioResponseHelper().Dial(); }
        internal TwilioResponse Enqueue(string queueResult) { return GetTwilioResponseHelper().Enqueue(queueResult); }
        internal TwilioResponse Queue() { return GetTwilioResponseHelper().Queue(); }
        internal TwilioResponse Leave() { return GetTwilioResponseHelper().Leave(); }
        internal TwilioResponse Dequeue() { return GetTwilioResponseHelper().Dequeue(); }
        internal TwilioResponse Wait() { return GetTwilioResponseHelper().Wait(QueueSid, QueueTime, QueueTime); }
        internal TwilioResponse GatherQueue() { return GetTwilioResponseHelper().GatherQueue(Digits, To.Substring(1), new List<Agent>()); }
        internal TwilioResponse Redirect() { return GetTwilioResponseHelper().Redirect(RedirectTo); }
        internal TwilioResponse VoiceMail() { return GetTwilioResponseHelper().VoiceMail(); }
    }

    public class ValidateRequestAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!new RequestValidator().IsValidRequest(HttpContext.Current, TwilioLoginProvider.TwilioAuthToken))
                actionContext.Response = new HttpResponseMessage {StatusCode = HttpStatusCode.Forbidden};
            base.OnActionExecuting(actionContext);
        }
    }
}