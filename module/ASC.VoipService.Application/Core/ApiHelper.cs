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
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ASC.Api.Client;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using Newtonsoft.Json.Linq;
using Twilio;
using log4net;

namespace ASC.VoipService.Application
{
    public class ApiHelper
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");

        public Tenant Tenant { get; set; }
        public ApiClient ApiClient { get; set; }
        public string Cookie { get; set; }
        public Guid CurrentAccountId { get; set; }

        public ApiHelper(string domain, string currentAccount)
        {
            try
            {
                var multiRegionHostedSolution = new MultiRegionHostedSolution("teamlabsite");
                Tenant = multiRegionHostedSolution.GetTenant(domain);
                CoreContext.TenantManager.SetCurrentTenant(Tenant);
                CurrentAccountId = !string.IsNullOrEmpty(currentAccount) ? new Guid(currentAccount) : Tenant.OwnerId;
                ApiClient = new ApiClient(Tenant.GetTenantDomain());
                Cookie = multiRegionHostedSolution.CreateAuthenticationCookie(Tenant.HostedRegion, Tenant.TenantId, CurrentAccountId);
                SecurityContext.AuthenticateMe(Cookie);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("ApiHelper: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        public VoipSettings GetSettings(string number)
        {
            try
            {
                var request = new ApiRequest(string.Format("crm/voip/numbers/{0}", number), Cookie)
                    {
                        Method = HttpMethod.Get,
                        ResponseType = ResponseType.Json
                    };

                return Newtonsoft.Json.JsonConvert.DeserializeObject<VoipPhone>(ApiClient.GetResponse(request).Response).Settings;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("GetSettings: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
                throw;
            }
        }

        public JObject SaveCall(string callId, string from, string to, VoipCallStatus status)
        {
            try
            {
                return SaveOrUpdateCall(callId, from, to, status: status);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("SaveCall: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
                return new JObject();
            }
        }

        public void UploadRecord(string callId, string parentCallId, string recordUrl)
        {
            Task.Run(() => UploadRecordToS3(callId, parentCallId, recordUrl));
        }

        public JObject SaveOrUpdateCallHistory(string callId, string parentCallId, string answeredBy = null, DateTime? queueDate = null,
            DateTime? answerDate = null, DateTime? endDialDate = null, string recordUrl = null, string recordDuration = null, decimal? price = null)
        {
            try
            {
                var request = new ApiRequest(string.Format("crm/voip/callhistory/{0}", callId), Cookie)
                {
                    Method = HttpMethod.Post,
                    ResponseType = ResponseType.Json
                };

                request.Parameters.Add(new RequestParameter { Name = "parentCallId", Value = parentCallId });

                if (!string.IsNullOrEmpty(answeredBy))
                    request.Parameters.Add(new RequestParameter { Name = "answeredBy", Value = answeredBy });

                if (queueDate.HasValue)
                    request.Parameters.Add(new RequestParameter { Name = "queueDate", Value = queueDate.Value });

                if (answerDate.HasValue)
                    request.Parameters.Add(new RequestParameter { Name = "answerDate", Value = answerDate.Value });

                if (endDialDate.HasValue)
                    request.Parameters.Add(new RequestParameter { Name = "endDialDate", Value = endDialDate.Value });

                if (!string.IsNullOrEmpty(recordUrl))
                    request.Parameters.Add(new RequestParameter { Name = "recordUrl", Value = recordUrl });

                if (!string.IsNullOrEmpty(recordDuration))
                    request.Parameters.Add(new RequestParameter { Name = "recordDuration", Value = recordDuration });

                if (price.HasValue)
                    request.Parameters.Add(new RequestParameter { Name = "price", Value = price.Value.ToString(CultureInfo.InvariantCulture) });

                return JObject.Parse(ApiClient.GetResponse(request).Response);
            }
            catch (ApiErrorException e)
            {
                Log.ErrorFormat("SaveOrUpdateCallHistory: StackTrace:{0}, Message: {1}", e.ErrorStackTrace, e.ErrorMessage);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("SaveOrUpdateCallHistory: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
            }

            return new JObject();
        }

        private void UploadRecordToS3(string callId, string parentCallId, string recordUrl)
        {
            try
            {
                Thread.Sleep(10000);
                CoreContext.TenantManager.SetCurrentTenant(Tenant);
                var stream = new WebClient().OpenRead(recordUrl);
                var storage = StorageFactory.GetStorage(Tenant.TenantId.ToString(CultureInfo.InvariantCulture), "crm");
                var newRecordUri = storage.Save(recordUrl.Split('/').Last() + ".wav", stream.GetBuffered());
                if (stream != null)
                    stream.Close();

                SaveOrUpdateCallHistory(callId, parentCallId, recordUrl: newRecordUri.AbsoluteUri);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("UploadRecord: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        public void SavePrice(string callId, string parentCallId)
        {
            try
            {
                Thread.Sleep(15000);
                var client = new TwilioRestClient(ConfigurationManager.AppSettings["twilioAccountSid"],ConfigurationManager.AppSettings["twilioAuthToken"]);

                SaveOrUpdateCall(parentCallId, price: GetPrice(client, parentCallId));
                SaveOrUpdateCallHistory(callId, parentCallId, price: GetPrice(client, callId));
            }
            catch (ApiErrorException e)
            {
                Log.ErrorFormat("SavePrice: StackTrace:{0}, Message: {1}", e.ErrorStackTrace, e.ErrorMessage);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("SavePrice: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        private static decimal? GetPrice(TwilioRestClient client, string callId)
        {
            try
            {
                var count = 3;
                var price = 0m;

                while (count > 0)
                {
                    var call = client.GetCall(callId);
                    if (call.Price.HasValue)
                        price = (-1) * call.Price.Value;

                    if (price > 0)
                        break;

                    count--;
                    Thread.Sleep(5000);
                }

                Log.InfoFormat("Price: {0}, callId: {1}", price, callId);

                return price;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("GetPrice: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
            }

            return null;
        }

        public void AnswerCall(string callId, string from, string to)
        {
            try
            {
                SaveOrUpdateCall(callId, answeredBy: CurrentAccountId.ToString(), status: VoipCallStatus.Answered);
            }
            catch (ApiErrorException e)
            {
                Log.ErrorFormat("AnswerCall: StackTrace:{0}, Message: {1}", e.ErrorStackTrace, e.ErrorMessage);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("AnswerCall: StackTrace:{0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        public JObject SaveOrUpdateCall(string callId, string from = null, string to = null, string answeredBy = null,
                                        string dialDuration = null, string recordUrl = null,
                                        string recordDuration = null, VoipCallStatus? status = null,
                                        string contactId = null, decimal? price = null)
        {
            try
            {
                var request = new ApiRequest(string.Format("crm/voip/call/{0}", callId), Cookie)
                    {
                        Method = HttpMethod.Post,
                        ResponseType = ResponseType.Json
                    };

                if (!string.IsNullOrEmpty(from))
                    request.Parameters.Add(new RequestParameter {Name = "from", Value = from});

                if (!string.IsNullOrEmpty(to))
                    request.Parameters.Add(new RequestParameter {Name = "to", Value = to});

                if (status != null)
                    request.Parameters.Add(new RequestParameter {Name = "status", Value = status.Value});

                if (!string.IsNullOrEmpty(dialDuration))
                    request.Parameters.Add(new RequestParameter {Name = "dialDuration", Value = dialDuration});

                if (!string.IsNullOrEmpty(recordUrl))
                    request.Parameters.Add(new RequestParameter {Name = "recordUrl", Value = recordUrl});

                if (!string.IsNullOrEmpty(recordDuration))
                    request.Parameters.Add(new RequestParameter {Name = "recordDuration", Value = recordDuration});

                if (!string.IsNullOrEmpty(answeredBy))
                    request.Parameters.Add(new RequestParameter {Name = "answeredBy", Value = answeredBy});

                if (!string.IsNullOrEmpty(contactId))
                    request.Parameters.Add(new RequestParameter {Name = "contactId", Value = contactId});

                if (price.HasValue)
                    request.Parameters.Add(new RequestParameter { Name = "price", Value = price.Value.ToString(CultureInfo.InvariantCulture) });

                return JObject.Parse(ApiClient.GetResponse(request).Response);
            }
            catch (ApiErrorException e)
            {
                Log.ErrorFormat("SaveOrUpdateCall: StackTrace:{0}, Message: {1}", e.ErrorStackTrace, e.ErrorMessage);
                throw;
            }
        }
    }
}