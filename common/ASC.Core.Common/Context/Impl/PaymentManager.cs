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


using ASC.Core.Billing;
using ASC.Core.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;


namespace ASC.Core
{
    public class PaymentManager
    {
        private readonly CoreConfiguration config;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly string partnerUrl;
        private readonly string partnerKey;


        public PaymentManager(CoreConfiguration config, IQuotaService quotaService, ITariffService tariffService)
        {
            this.config = config;
            this.quotaService = quotaService;
            this.tariffService = tariffService;
            partnerUrl = (ConfigurationManager.AppSettings["core.payment-partners"] ?? "https://partners.onlyoffice.com/api").TrimEnd('/');
            partnerKey = (ConfigurationManager.AppSettings["core.machinekey"] ?? "C5C1F4E85A3A43F5B3202C24D97351DF");
        }


        public Tariff GetTariff(int tenantId)
        {
            return tariffService.GetTariff(tenantId);
        }

        public void SetTariff(int tenantId, Tariff tariff)
        {
            tariffService.SetTariff(tenantId, tariff);
        }

        public void DeleteDefaultTariff()
        {
            tariffService.DeleteDefaultBillingInfo();
        }

        public IEnumerable<PaymentInfo> GetTariffPayments(int tenant)
        {
            return GetTariffPayments(tenant, DateTime.MinValue, DateTime.MaxValue);
        }

        public IEnumerable<PaymentInfo> GetTariffPayments(int tenant, DateTime from, DateTime to)
        {
            return tariffService.GetPayments(tenant, from, to);
        }

        public Invoice GetPaymentInvoice(string paymentId)
        {
            return tariffService.GetInvoice(paymentId);
        }

        public Uri GetShoppingUri(int tenant, int quotaId)
        {
            return tariffService.GetShoppingUri(tenant, quotaId, null);
        }

        public Uri GetShoppingUri(int quotaId, bool forCurrentTenant = true, string affiliateId = null)
        {
            return tariffService.GetShoppingUri(forCurrentTenant ? CoreContext.TenantManager.GetCurrentTenant().TenantId : (int?)null, quotaId, affiliateId);
        }

        public Uri GetShoppingUri(int quotaId, string affiliateId)
        {
            return tariffService.GetShoppingUri(null, quotaId, affiliateId);
        }

        public void SendTrialRequest(int tenant, UserInfo user)
        {
            var trial = quotaService.GetTenantQuotas().FirstOrDefault(q => q.Trial);
            if (trial != null)
            {
                var uri = ConfigurationManager.AppSettings["core.payment-request"] ?? "http://billing.onlyoffice.com/avangate/requestatrialversion.aspx";
                uri += uri.Contains('?') ? "&" : "?";
                uri += "FIRSTNAME=" + HttpUtility.UrlEncode(user.FirstName) +
                    "&LASTNAME=" + HttpUtility.UrlEncode(user.FirstName) +
                    "&CUSTOMEREMAIL=" + HttpUtility.UrlEncode(user.Email) +
                    "&PORTALID=" + HttpUtility.UrlEncode(config.GetKey(tenant)) +
                    "&PRODUCTID=" + HttpUtility.UrlEncode(trial.AvangateId);

                using (var webClient = new WebClient())
                {
                    var result = webClient.DownloadString(uri);
                    var element = XElement.Parse(result);
                    if (element.Value != null &&
                        (element.Value.StartsWith("error:", StringComparison.InvariantCultureIgnoreCase) ||
                        element.Value.StartsWith("warning:", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        throw new BillingException(element.Value, new { Tenant = tenant, User = user.ID });
                    }
                    var tariff = new Tariff
                    {
                        QuotaId = trial.Id,
                        State = TariffState.Trial,
                        DueDate = DateTime.UtcNow.Date.AddMonths(1),
                    };
                    tariffService.SetTariff(tenant, tariff);
                    tariffService.GetTariff(tenant);
                }
            }
        }



        public void ActivateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            var now = DateTime.UtcNow;
            var actionUrl = "/partnerapi/ActivateKey?code=" + HttpUtility.UrlEncode(key) + "&portal=" + HttpUtility.UrlEncode(CoreContext.TenantManager.GetCurrentTenant().TenantAlias);
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", GetPartnerAuthHeader(actionUrl));
                try
                {
                    webClient.DownloadData(partnerUrl + actionUrl);
                }
                catch (WebException we)
                {
                    var error = GetException(we);
                    if (error != null)
                    {
                        throw error;
                    }
                    throw;
                }
                tariffService.ClearCache(CoreContext.TenantManager.GetCurrentTenant().TenantId);

                var timeout = DateTime.UtcNow - now - TimeSpan.FromSeconds(5);
                if (TimeSpan.Zero < timeout)
                {
                    // clear tenant cache
                    Thread.Sleep(timeout);
                }
                CoreContext.TenantManager.GetTenant(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
        }

        private string GetPartnerAuthHeader(string url)
        {
            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(partnerKey)))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var data = string.Join("\n", now, "/api/" + url.TrimStart('/')); //data: UTC DateTime (yyyy:MM:dd HH:mm:ss) + \n + url
                var hash = HttpServerUtility.UrlTokenEncode(hasher.ComputeHash(Encoding.UTF8.GetBytes(data)));
                return string.Format("ASC :{0}:{1}", now, hash);
            }
        }

        private static Exception GetException(WebException we)
        {
            var response = (HttpWebResponse)we.Response;
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var result = reader.ReadToEnd();
                    var excInfo = JsonConvert.DeserializeObject<ExceptionJson>(result);
                    return (Exception)Activator.CreateInstance(Type.GetType(excInfo.exceptionType, true), excInfo.exceptionMessage);
                }
            }
            return null;
        }


        private class ExceptionJson
        {
            public string message = null;
            public string exceptionMessage = null;
            public string exceptionType = null;
            public string stackTrace = null;
        }
    }
}
