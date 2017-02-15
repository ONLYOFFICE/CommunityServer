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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Specific;
using ASC.Web.Core;
using DotNetOpenAuth.Messaging;
using Newtonsoft.Json.Linq;
using RestSharp;
using Newtonsoft.Json;

namespace ASC.Mail.Aggregator.Common
{
    public class ApiHelper
    {
        private const int MAIL_CRM_HISTORY_CATEGORY = -3;
        private const string ERR_MESSAGE = "Error retrieving response. Check inner details for more info.";
        private Cookie _cookie;
        private readonly ILogger _log;

        public string Scheme { get; private set; }

        public UriBuilder BaseUrl { get; private set; }

        /// <summary>
        /// Constructor of class ApiHelper
        /// </summary>
        /// <param name="scheme">Uri.UriSchemeHttps or Uri.UriSchemeHttp</param>
        /// <exception cref="ApiHelperException">Exception happens when scheme is invalid.</exception>>
        public ApiHelper(string scheme)
        {
            if (!scheme.Equals(Uri.UriSchemeHttps) && !scheme.Equals(Uri.UriSchemeHttp))
                throw new ApiHelperException("ApiHelper: url scheme not setup", HttpStatusCode.InternalServerError, "");

            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api");
            Scheme = scheme;

            if (scheme.Equals(Uri.UriSchemeHttps) && ConfigurationManager.AppSettings["mail.certificate-permit"] != null)
            {
                var sslCertificateErrorsPermit =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["mail.certificate-permit"]);

                if (sslCertificateErrorsPermit)
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                }
            }
        }

        private void Setup()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            _log.Debug("ApiHelper->Setup: Tenant={0} User='{1}' IsAuthenticated={2} Scheme='{3}' HttpContext is {4}",
                      tenant.TenantId, user.ID, user.IsAuthenticated, Scheme,
                      HttpContext.Current != null
                          ? string.Format("not null and UrlRewriter = {0}, RequestUrl = {1}", HttpContext.Current.Request.GetUrlRewriter(), HttpContext.Current.Request.Url)
                          : "null");

            if (!user.IsAuthenticated)
                throw new AuthenticationException("User not authenticated");

            var hs = new HostedSolution(ConfigurationManager.ConnectionStrings["default"]);
            var authenticationCookie = hs.CreateAuthenticationCookie(tenant.TenantId, user.ID);

            var tempUrl = (WebConfigurationManager.AppSettings["api.url"] ?? "").Trim('~', '/');

            var ubBase = new UriBuilder
            {
                Scheme = Scheme,
                Host = tenant.GetTenantDomain(false)
            };

            var virtualDir = WebConfigurationManager.AppSettings["api.virtual-dir"];
            if (!string.IsNullOrEmpty(virtualDir))
                tempUrl = string.Format("{0}/{1}", virtualDir.Trim('/'), tempUrl);

            var host = WebConfigurationManager.AppSettings["api.host"];
            if (!string.IsNullOrEmpty(host))
                ubBase.Host = host;

            var port = WebConfigurationManager.AppSettings["api.port"];
            if (!string.IsNullOrEmpty(port))
                ubBase.Port = int.Parse(port);

            ubBase.Path = tempUrl;

            BaseUrl = ubBase;

            _cookie = new Cookie("asc_auth_key", authenticationCookie, "/", BaseUrl.Host);
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            Setup();

            _log.Debug("ApiHelper->Execute<{0}>: request url: {1}/{2}", typeof(T), BaseUrl.Uri.ToString(), request.Resource);

            var client = new RestClient {BaseUrl = BaseUrl.Uri.ToString()};

            request.AddCookie(_cookie.Name, _cookie.Value);

            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                var ex = new ApplicationException(ERR_MESSAGE, response.ErrorException);
                throw ex;
            }
            return response.Data;
        }

        public IRestResponse Execute(RestRequest request)
        {
            Setup();

            _log.Debug("ApiHelper->Execute: request url: {0}/{1}", BaseUrl.Uri.ToString(), request.Resource);

            var client = new RestClient { BaseUrl = BaseUrl.Uri.ToString() };

            request.AddCookie(_cookie.Name, _cookie.Value);

            var response = client.ExecuteSafe(request);

            if (response.ErrorException is ApiHelperException)
            {
                return response;
            }

            if (response.ErrorException != null)
            {
                throw new ApplicationException(ERR_MESSAGE, response.ErrorException);
            }

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

        public void SendMessage(MailMessage message, bool isAutoreply = false)
        {
            var request = new RestRequest("mail/messages/send.json", Method.PUT);
            var jObject = new JObject();
            jObject.Add("id", message.Id);
            if (!String.IsNullOrEmpty(message.From))
            {
                jObject.Add("from", message.From);
            }
            jObject.Add("to", message.To);
            if (!String.IsNullOrEmpty(message.Cc))
            {
                jObject.Add("cc", message.Cc);
            }
            if (!String.IsNullOrEmpty(message.Bcc))
            {
                jObject.Add("bcc", message.Bcc);
            }
            jObject.Add("subject", message.Subject);
            jObject.Add("body", message.HtmlBody);
            jObject.Add("mimeReplyToId", message.MimeReplyToId);
            jObject.Add("importance", message.Important);
            if (message.TagIds != null && message.TagIds.Count != 0)
            {
                jObject.Add("tags", JsonConvert.SerializeObject(message.TagIds));
            }
            if (message.Attachments != null && message.Attachments.Count != 0)
            {
                jObject.Add("attachments", JsonConvert.SerializeObject(message.Attachments));
            }
            if (!string.IsNullOrEmpty(message.CalendarEventIcs))
            {
                jObject.Add("calendarIcs", message.CalendarEventIcs);
            }
            jObject.Add("isAutoreply", isAutoreply);

            request.AddParameter("application/json; charset=utf-8", jObject, ParameterType.RequestBody);
            var response = Execute(request);
            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                if (response.ErrorException is ApiHelperException)
                {
                    throw response.ErrorException;
                }

                throw new ApiHelperException("Send message to api failed.", response.StatusCode, response.Content);
            }
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

                if(commonData == null)
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
                    emails.AddRange(contactData.Where(d => d["type"].ToString() == "mail").Select(d => (string) d["value"]).ToList());
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

        public void AddToCrmHistory(MailMessage message, CrmContactEntity entity, IEnumerable<int> fileIds)
        {
            var request = new RestRequest("crm/history.json", Method.POST);

            var contentJson = string.Format("{{ message_id : {0} }}", message.Id);

            request.AddParameter("content", contentJson)
                   .AddParameter("categoryId", MAIL_CRM_HISTORY_CATEGORY)
                   .AddParameter("created", new ApiDateTime(message.Date));

            var crmEntityType = entity.EntityTypeName;

            if (crmEntityType == CrmContactEntity.CrmEntityTypeNames.contact)
            {
                request.AddParameter("contactId", entity.Id)
                       .AddParameter("entityId", 0);
            }
            else
            {
                if (crmEntityType != CrmContactEntity.CrmEntityTypeNames.Case
                    && crmEntityType != CrmContactEntity.CrmEntityTypeNames.opportunity)
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

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                if (response.ErrorException is ApiHelperException)
                {
                    throw response.ErrorException;
                }

                throw new ApiHelperException("Add message to crm history failed.", response.StatusCode, response.Content);
            }
        }

        public int UploadToCrm(Stream fileStream, string filename, string contentType,
                                      CrmContactEntity entity)
        {
            var request = new RestRequest("crm/{entityType}/{entityId}/files/upload.json", Method.POST);

            request.AddUrlSegment("entityType", entity.EntityTypeName)
                .AddUrlSegment("entityId", entity.Id.ToString())
                .AddParameter("storeOriginalFileFlag", false);

            request.AddFile(filename, fileStream.CopyTo, filename, contentType);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Upload file to crm failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            var id = Int32.Parse(json["response"]["id"].ToString());

            return id;
        }

        public int UploadToDocuments(Stream fileStream, string filename, string contentType, string folderId, bool createNewIfExist)
        {
            var request = new RestRequest("files/{folderId}/upload.json", Method.POST);

            request.AddUrlSegment("folderId", folderId)
                   .AddParameter("createNewIfExist", createNewIfExist)
                   .AddParameter("storeOriginalFileFlag", true);

            request.AddFile(filename, fileStream.CopyTo, filename, contentType);

            var response = Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed ||
                (response.StatusCode != HttpStatusCode.Created &&
                 response.StatusCode != HttpStatusCode.OK))
            {
                throw new ApiHelperException("Upload file to documents failed.", response.StatusCode, response.Content);
            }

            var json = JObject.Parse(response.Content);

            var id = Int32.Parse(json["response"]["id"].ToString());

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

            request.AddFile(filename, fileStream.CopyTo, filename, contentType);

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
                _log.Warn("Upload ics-file to calendar failed. No count number.", BaseUrl.ToString(), response.StatusCode, response.Content);
            }
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
