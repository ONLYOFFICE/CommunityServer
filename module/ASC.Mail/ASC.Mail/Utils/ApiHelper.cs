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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Specific;
using ASC.Web.Core;
using DotNetOpenAuth.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ASC.Mail.Utils
{
    public class ApiHelper
    {
        private const int MAIL_CRM_HISTORY_CATEGORY = -3;
        private const string ERR_MESSAGE = "Error retrieving response. Check inner details for more info.";
        private readonly ILog _log;

        public string Scheme { get; private set; }

        public UriBuilder BaseUrl { get; private set; }

        public string Token { get; set; }

        /// <summary>
        /// Constructor of class ApiHelper
        /// </summary>
        /// <param name="scheme">Uri.UriSchemeHttps or Uri.UriSchemeHttp</param>
        /// <exception cref="ApiHelperException">Exception happens when scheme is invalid.</exception>>
        public ApiHelper(string scheme, ILog log = null)
        {
            if (!scheme.Equals(Uri.UriSchemeHttps) && !scheme.Equals(Uri.UriSchemeHttp))
                throw new ApiHelperException("ApiHelper: url scheme not setup", HttpStatusCode.InternalServerError, "");

            _log = log ?? LogManager.GetLogger("ASC.Mail.ApiHelper");

            Scheme = scheme;

            if (!scheme.Equals(Uri.UriSchemeHttps) || !Defines.SslCertificatesErrorPermit)
                return;

            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        private void Setup()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            var user = SecurityContext.CurrentAccount;

            _log.DebugFormat("Tenant={0} User='{1}' IsAuthenticated={2} Scheme='{3}' HttpContext is {4}",
                      tenant.TenantId, user.ID, user.IsAuthenticated, Scheme,
                      HttpContext.Current != null
                          ? string.Format("not null and UrlRewriter = {0}, RequestUrl = {1}", HttpContext.Current.Request.GetUrlRewriter(), HttpContext.Current.Request.Url)
                          : "null");

            if (!user.IsAuthenticated)
                throw new AuthenticationException("User not authenticated");

            var tempUrl = Defines.ApiPrefix;

            var ubBase = new UriBuilder
            {
                Scheme = Scheme,
                Host = tenant.GetTenantDomain(false)
            };

            if (!string.IsNullOrEmpty(Defines.ApiVirtualDirPrefix))
                tempUrl = string.Format("{0}/{1}", Defines.ApiVirtualDirPrefix, tempUrl);

            if (!string.IsNullOrEmpty(Defines.ApiHost))
                ubBase.Host = Defines.ApiHost;

            if (!string.IsNullOrEmpty(Defines.ApiPort))
                ubBase.Port = int.Parse(Defines.ApiPort);

            ubBase.Path = tempUrl;

            BaseUrl = ubBase;

            Token = SecurityContext.AuthenticateMe(user.ID);
        }

        public IRestResponse Execute(RestRequest request)
        {
            Setup();

            _log.DebugFormat("Execute request url: baseUrl='{0}' resourceUrl='{1}' token='{2}'", 
                BaseUrl.Uri.ToString(), 
                request.Resource,
                Token);

            var client = new RestClient { BaseUrl = BaseUrl.Uri };

            request.AddHeader("Authorization", Token);

            var response = client.ExecuteSafe(request);

            if (response.ErrorException is ApiHelperException)
                return response;

            if (response.ErrorException != null)
                throw new ApplicationException(ERR_MESSAGE, response.ErrorException);

            return response;
        }

        public Defines.TariffType GetTenantTariff(int tenantOverdueDays)
        {
            var request = new RestRequest("portal/tariff.json", Method.GET);

            request.AddHeader("Payment-Info", "false");

            var response = Execute(request);

            if (response.StatusCode == HttpStatusCode.PaymentRequired)
                return Defines.TariffType.LongDead;

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Get tenant tariff failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            TariffState state;

            Enum.TryParse(json["response"]["state"].ToString(), out state);

            Defines.TariffType result;

            if (state < TariffState.NotPaid)
            {
                result = Defines.TariffType.Active;
            }
            else
            {
                var dueDate = DateTime.Parse(json["response"]["dueDate"].ToString());

                var delayDateString = json["response"]["delayDueDate"].ToString();

                var delayDueDate = DateTime.Parse(delayDateString);

                var maxDateStr = DateTime.MaxValue.CutToSecond().ToString(CultureInfo.InvariantCulture);

                delayDateString = delayDueDate.CutToSecond().ToString(CultureInfo.InvariantCulture);

                result = (!delayDateString.Equals(maxDateStr) ? delayDueDate : dueDate)
                             .AddDays(tenantOverdueDays) <= DateTime.UtcNow
                             ? Defines.TariffType.LongDead
                             : Defines.TariffType.Overdue;
            }

            return result;
        }

        public void RemoveTeamlabMailbox(int mailboxId)
        {
            var request = new RestRequest("mailserver/mailboxes/remove/{id}", Method.DELETE);

            request.AddUrlSegment("id", mailboxId.ToString(CultureInfo.InvariantCulture));

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Delete teamlab mailbox failed.", response.StatusCode, response.Content);
            }
        }

        public void SendMessage(MailMessageData message, bool isAutoreply = false)
        {
            var request = new RestRequest("mail/messages/send.json", Method.PUT);

            var jObject = new JObject { { "id", message.Id } };

            if (!string.IsNullOrEmpty(message.From))
                jObject.Add("from", message.From);

            jObject.Add("to", message.To);

            if (!string.IsNullOrEmpty(message.Cc))
                jObject.Add("cc", message.Cc);

            if (!string.IsNullOrEmpty(message.Bcc))
                jObject.Add("bcc", message.Bcc);

            jObject.Add("subject", message.Subject);

            jObject.Add("body", message.HtmlBody);

            jObject.Add("mimeReplyToId", message.MimeReplyToId);

            jObject.Add("importance", message.Important);

            if (message.TagIds != null && message.TagIds.Count != 0)
                jObject.Add("tags", JsonConvert.SerializeObject(message.TagIds));

            if (message.Attachments != null && message.Attachments.Count != 0)
                jObject.Add("attachments", JsonConvert.SerializeObject(message.Attachments));

            if (!string.IsNullOrEmpty(message.CalendarEventIcs))
                jObject.Add("calendarIcs", message.CalendarEventIcs);

            jObject.Add("isAutoreply", isAutoreply);

            request.AddParameter("application/json; charset=utf-8", jObject, ParameterType.RequestBody);

            var response = Execute(request);

            if (response.ResponseStatus == ResponseStatus.Completed &&
                (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK))
                return;

            if (response.ErrorException is ApiHelperException)
                throw response.ErrorException;

            throw new ApiHelperException("Send message to api failed.", response.StatusCode, response.Content);
        }

        public List<string> SearchEmails(string term)
        {
            var request = new RestRequest("mail/emails/search.json", Method.GET);

            request.AddParameter("term", term);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                if (response.ErrorException is ApiHelperException)
                {
                    throw response.ErrorException;
                }

                throw new ApiHelperException("Search Emails failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            return json["response"].ToObject<List<string>>();
        }

        public List<string> SearchCrmEmails(string term, int maxCount)
        {
            var request = new RestRequest("crm/contact/simple/byEmail.json", Method.GET);

            request.AddParameter("term", term)
                .AddParameter("maxCount", maxCount.ToString());

            var response = Execute(request);

            var crmEmails = new List<string>();

            var json = JObject.Parse(response.Content);

            var contacts = json["response"] as JArray;

            if (contacts == null)
                return crmEmails;

            foreach (var contact in contacts)
            {
                var commonData = contact["contact"]["commonData"] as JArray;

                if (commonData == null)
                    continue;

                var emails = commonData.Where(d => int.Parse(d["infoType"].ToString()) == 1).Select(d => (string)d["data"]).ToList();

                if (!emails.Any())
                    continue;

                var displayName = contact["contact"]["displayName"].ToString();

                if (displayName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    crmEmails.AddRange(emails.Select(e => MailUtil.CreateFullEmail(displayName, e)));
                }
                else
                {
                    crmEmails.AddRange(emails
                        .Where(e => e.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                        .Select(e => MailUtil.CreateFullEmail(displayName, e)));
                }
            }

            return crmEmails;
        }

        public List<string> SearchPeopleEmails(string term, int startIndex, int count)
        {
            var request = new RestRequest("people/filter.json?filterValue={FilterValue}&StartIndex={StartIndex}&Count={Count}", Method.GET);

            request.AddParameter("FilterValue", term, ParameterType.UrlSegment)
                .AddParameter("StartIndex", startIndex.ToString(), ParameterType.UrlSegment)
                .AddParameter("Count", count.ToString(), ParameterType.UrlSegment);

            var response = Execute(request);

            var peopleEmails = new List<string>();

            var json = JObject.Parse(response.Content);

            var contacts = json["response"] as JArray;

            if (contacts == null)
                return peopleEmails;

            foreach (var contact in contacts)
            {
                var displayName = contact["displayName"].ToString();

                var emails = new List<string>();

                var email = contact["email"].ToString();

                if (!string.IsNullOrEmpty(email))
                    emails.Add(email);

                var contactData = contact["contacts"] as JArray;

                if (contactData != null)
                {
                    emails.AddRange(contactData.Where(d => d["type"].ToString() == "mail").Select(d => (string)d["value"]).ToList());
                }

                if (displayName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    peopleEmails.AddRange(emails.Select(e => MailUtil.CreateFullEmail(displayName, e)));
                }
                else
                {
                    peopleEmails.AddRange(emails
                        .Where(e => e.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                        .Select(e => MailUtil.CreateFullEmail(displayName, e)));
                }
            }

            return peopleEmails;
        }

        public void AddToCrmHistory(MailMessageData message, CrmContactData entity, IEnumerable<object> fileIds)
        {
            var request = new RestRequest("crm/history.json", Method.POST);

            var contentJson = string.Format("{{ message_id : {0} }}", message.Id);

            request.AddParameter("content", contentJson)
                   .AddParameter("categoryId", MAIL_CRM_HISTORY_CATEGORY)
                   .AddParameter("created", new ApiDateTime(message.Date));

            var crmEntityType = entity.EntityTypeName;

            if (crmEntityType == CrmContactData.CrmEntityTypeNames.CONTACT)
            {
                request.AddParameter("contactId", entity.Id)
                       .AddParameter("entityId", 0);
            }
            else
            {
                if (crmEntityType != CrmContactData.CrmEntityTypeNames.CASE
                    && crmEntityType != CrmContactData.CrmEntityTypeNames.OPPORTUNITY)
                    throw new ArgumentException(String.Format("Invalid crm entity type: {0}", crmEntityType));

                request.AddParameter("contactId", 0)
                       .AddParameter("entityId", entity.Id)
                       .AddParameter("entityType", crmEntityType);
            }

            if (fileIds != null)
            {
                fileIds.ToList().ForEach(
                    id => request.AddParameter("fileId[]", id));
            }

            var response = Execute(request);

            if (response.ResponseStatus == ResponseStatus.Completed &&
                (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK))
                return;

            if (response.ErrorException is ApiHelperException)
                throw response.ErrorException;

            throw new ApiHelperException("Add message to crm history failed.", response.StatusCode, response.Content);
        }

        public object UploadToCrm(Stream fileStream, string filename, string contentType,
                                      CrmContactData entity)
        {
            if(entity == null)
                throw new ArgumentNullException("entity");

            var request = new RestRequest("crm/{entityType}/{entityId}/files/upload.json", Method.POST);

            request.AddUrlSegment("entityType", entity.EntityTypeName)
                .AddUrlSegment("entityId", entity.Id.ToString())
                .AddParameter("storeOriginalFileFlag", false);

            request.AddFile(filename, fileStream.CopyTo, filename, fileStream.Length, contentType);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Upload file to crm failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            var id = json["response"]["id"];

            return id;
        }

        public object UploadToDocuments(Stream fileStream, string filename, string contentType, string folderId, bool createNewIfExist)
        {
            var request = new RestRequest("files/{folderId}/upload.json", Method.POST);

            request.AddUrlSegment("folderId", folderId)
                   .AddParameter("createNewIfExist", createNewIfExist);

            request.AddFile(filename, fileStream.CopyTo, filename, fileStream.Length, contentType);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Upload file to documents failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            var id = json["response"]["id"];

            return id;
        }

        //TODO: need refactoring to comman execute method
        public void SendEmlToSpamTrainer(string serverIp, string serverProtocol, int serverPort,
                                         string serverApiVersion, string serverApiToken, string urlEml,
                                         bool isSpam)
        {
            if (string.IsNullOrEmpty(urlEml))
                return;

            var saLearnApiClient =
                new RestClient(string.Format("{0}://{1}:{2}/", serverProtocol,
                                             serverIp, serverPort));

            var saLearnRequest =
                new RestRequest(
                    string.Format("/api/{0}/spam/training.json?auth_token={1}", serverApiVersion,
                                  serverApiToken), Method.POST);

            saLearnRequest.AddParameter("url", urlEml)
                          .AddParameter("is_spam", isSpam ? 1 : 0);

            var response = saLearnApiClient.Execute(saLearnRequest);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Send eml to spam trainer failed.", response.StatusCode, response.Content);
            }
        }

        public void UploadIcsToCalendar(int calendarId, Stream fileStream, string filename, string contentType)
        {
            var request = new RestRequest("calendar/import.json", Method.POST);

            request.AddParameter("calendarId", calendarId);

            request.AddFile(filename, fileStream.CopyTo, filename, fileStream.Length, contentType);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Upload ics-file to calendar failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            int count;

            if (!int.TryParse(json["response"].ToString(), out count))
            {
                _log.WarnFormat("Upload ics-file to calendar failed. No count number.", BaseUrl.ToString(), response.StatusCode, response.Content);
            }
        }

        public UserInfo CreateEmployee(bool isVisitor, string email, string firstname, string lastname, string password)
        {
            var request = new RestRequest("people.json", Method.POST);

            request.AddParameter("isVisitor", isVisitor)
                .AddParameter("email", email)
                .AddParameter("firstname", firstname)
                .AddParameter("lastname", lastname)
                .AddParameter("password", password);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("ApiHelper->CreateEmployee() failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            _log.Debug(json["response"].ToString());

            var userInfo = new UserInfo
            {
                ID = Guid.Parse(json["response"]["id"].ToString()),
                Email = json["response"]["email"].ToString(),
                FirstName = json["response"]["firstName"].ToString(),
                LastName = json["response"]["lastName"].ToString(),
                UserName = json["response"]["userName"].ToString(),
            };

            return userInfo;
        }

        public JObject GetPortalSettings()
        {
            var request = new RestRequest("settings/security.json", Method.GET);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("GetPortalSettings failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            return json;
        }

        public bool IsCalendarModuleAvailable()
        {
            var json = GetPortalSettings();

            var jWebItem = json["response"].Children<JObject>()
                .FirstOrDefault(
                    o =>
                        o["webItemId"] != null &&
                        o["webItemId"].ToString() == WebItemManager.CalendarProductID.ToString());

            var isAvailable = jWebItem != null && jWebItem["enabled"] != null && Convert.ToBoolean(jWebItem["enabled"]);

            return isAvailable;
        }

        public bool IsMailModuleAvailable()
        {
            var json = GetPortalSettings();

            var jWebItem = json["response"].Children<JObject>()
                .FirstOrDefault(
                    o =>
                        o["webItemId"] != null &&
                        o["webItemId"].ToString() == WebItemManager.MailProductID.ToString());

            var isAvailable = jWebItem != null && jWebItem["enabled"] != null && Convert.ToBoolean(jWebItem["enabled"]);

            return isAvailable;
        }

        public bool IsCrmModuleAvailable()
        {
            var json = GetPortalSettings();

            var crmId = WebItemManager.CRMProductID.ToString();

            var jWebItem = json["response"].Children<JObject>()
                .FirstOrDefault(
                    o =>
                        o["webItemId"] != null &&
                        o["webItemId"].ToString() == crmId);

            var isAvailable = jWebItem != null && jWebItem["enabled"] != null && Convert.ToBoolean(jWebItem["enabled"]);

            return isAvailable;
        }
    }
}
