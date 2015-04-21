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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Specific;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ASC.Mail.Aggregator.Common
{
    public static class ApiHelper
    {
        static ApiHelper()
        {
            Log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api");
        }

        private static Cookie _cookie;

        private static readonly ILogger Log;

        private static string _scheme;

        public static void SetupScheme(string scheme)
        {
            Log.Debug("ApiHelper->SetupScheme('{0}')", scheme);
            _scheme = scheme;
        }

        private static string Scheme
        {
            get
            {
                return !string.IsNullOrEmpty(_scheme)
                           ? _scheme
                           : HttpContext.Current != null ? HttpContext.Current.Request.GetUrlRewriter().Scheme : Uri.UriSchemeHttp;
            }
        }

        public static UriBuilder BaseUrl { get; set; }

        private static void Setup()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            var user = SecurityContext.CurrentAccount;

            Log.Debug("ApiHelper->Setup: Tenant={0} User='{1}' IsAuthenticated={2} Scheme='{3}' HttpContext is {4}",
                      tenant.TenantId, user.ID, user.IsAuthenticated, Scheme,
                      HttpContext.Current != null
                          ? string.Format("not null and UrlRewriter = {0}, RequestUrl = {1}", HttpContext.Current.Request.GetUrlRewriter(), HttpContext.Current.Request.Url)
                          : "null");

            if (!user.IsAuthenticated)
                throw new AuthenticationException("User not authenticated");

            var hs = new HostedSolution(ConfigurationManager.ConnectionStrings["default"]);
            var authenticationCookie = hs.CreateAuthenticationCookie(tenant.TenantId, user.ID);

            var tempUrl = WebConfigurationManager.AppSettings["api.url"].Trim('~', '/');

            var ubBase = new UriBuilder(Scheme, tenant.TenantAlias);

            if (tenant.TenantAlias == "localhost")
            {
                var virtualDir = WebConfigurationManager.AppSettings["core.virtual-dir"];
                if (!string.IsNullOrEmpty(virtualDir))
                    tempUrl = virtualDir.Trim('/') + "/" + tempUrl;

                var host = WebConfigurationManager.AppSettings["core.host"];
                if (!string.IsNullOrEmpty(host))
                    ubBase.Host = host;

                var port = WebConfigurationManager.AppSettings["core.port"];
                if (!string.IsNullOrEmpty(port))
                    ubBase.Port = int.Parse(port);
            }
            else
                ubBase.Host += "." + WebConfigurationManager.AppSettings["core.base-domain"];

            ubBase.Path = tempUrl;

            BaseUrl = ubBase;

            _cookie = new Cookie("asc_auth_key", authenticationCookie, "/", BaseUrl.Host);
        }

        public static T Execute<T>(RestRequest request) where T : new()
        {
            Setup();

            Log.Debug("ApiHelper->Execute<{0}>: request url: {1}/{2}", typeof(T), BaseUrl.Uri.ToString(), request.Resource);

            var client = new RestClient {BaseUrl = BaseUrl.Uri.ToString()};

            request.AddCookie(_cookie.Name, _cookie.Value);

            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var ex = new ApplicationException(message, response.ErrorException);
                throw ex;
            }
            return response.Data;
        }

        public static IRestResponse Execute(RestRequest request)
        {
            Setup();

            Log.Debug("ApiHelper->Execute: request url: {0}/{1}", BaseUrl.Uri.ToString(), request.Resource);

            var client = new RestClient { BaseUrl = BaseUrl.Uri.ToString() };

            request.AddCookie(_cookie.Name, _cookie.Value);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _scheme = Scheme == Uri.UriSchemeHttp ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

                Log.Debug("ApiHelper->Execute.Response == HttpStatusCode.NotFound. Request scheme was changed to '{0}'", _scheme);

                Setup();

                Log.Debug("ApiHelper->Execute: request url: {0}/{1}", BaseUrl.Uri.ToString(), request.Resource);

                response = client.Execute(request);
            }

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var ex = new ApplicationException(message, response.ErrorException);
                throw ex;
            }

            return response;
        }

        public static Defines.TariffType GetTenantTariff(int tenantOverdueDays)
        {
            var request = new RestRequest("portal/tariff.json", Method.GET);

            request.AddUrlSegment("Payment-Info", "false");

            var tenantInfo = CoreContext.TenantManager.GetCurrentTenant();

            if (tenantInfo.Status == TenantStatus.RemovePending)
                return Defines.TariffType.LongDead;

            SecurityContext.AuthenticateMe(tenantInfo.OwnerId);

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

            var state = Int32.Parse(json["response"]["state"].ToString());

            Defines.TariffType result;

            if (state == 0 || state == 1)
                result = Defines.TariffType.Active;
            else
            {
                var dueDate = DateTime.Parse(json["response"]["dueDate"].ToString());

                result = dueDate.AddDays(tenantOverdueDays) <= DateTime.UtcNow
                             ? Defines.TariffType.LongDead
                             : Defines.TariffType.Overdue;
            }

            return result;
        }

        public static void RemoveTeamlabMailbox(int mailboxId)
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

        private const int MAIL_CRM_HISTORY_CATEGORY = -3;

        public static void AddToCrmHistory(MailMessageItem item, CrmContactEntity entity, string contentString, IEnumerable<int> fileIds)
        {
            var request = new RestRequest("crm/history.json", Method.POST);

            request.AddParameter("content", contentString)
                   .AddParameter("categoryId", MAIL_CRM_HISTORY_CATEGORY)
                   .AddParameter("created", new ApiDateTime(item.Date));

            var crmEntityType = entity.Type.StringName();

            if (crmEntityType == ChainXCrmContactEntity.CrmEntityTypeNames.contact)
            {
                request.AddParameter("contactId", entity.Id)
                       .AddParameter("entityId", 0);
            }
            else
            {
                if (crmEntityType != ChainXCrmContactEntity.CrmEntityTypeNames.Case
                    && crmEntityType != ChainXCrmContactEntity.CrmEntityTypeNames.opportunity)
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
                throw new ApiHelperException("Add message to crm history failed.", response.StatusCode, response.Content);
            }
        }

        public static int UploadToCrm(Stream fileStream, string filename, string contentType,
                                      CrmContactEntity entity)
        {
            var request = new RestRequest(string.Format("crm/{0}/{1}/files/upload.json", entity.Type.StringName(), entity.Id), Method.POST);

            request.AddParameter("storeOriginalFileFlag", false);

            var bytes = fileStream.GetCorrectBuffer();

            request.AddFile(filename, bytes, filename, contentType);

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

        public static int UploadToDocuments(Stream fileStream, string filename, string contentType, string folderId, bool createNewIfExist)
        {
            var request = new RestRequest(string.Format("files/{0}/upload.json", folderId), Method.POST);

            request.AddParameter("createNewIfExist", createNewIfExist)
                   .AddParameter("storeOriginalFileFlag", true);

            var bytes = fileStream.GetCorrectBuffer();

            request.AddFile(filename, bytes, filename, contentType);

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
        public static void SendEmlToSpamTrainer(string serverIp, string serverProtocol, int serverPort,
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

    }

}
