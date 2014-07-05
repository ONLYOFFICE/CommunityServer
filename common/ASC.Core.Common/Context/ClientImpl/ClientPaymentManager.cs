/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.Core.Billing;
using ASC.Core.Caching;
using ASC.Core.Tenants;
using ASC.Core.Users;
using log4net;
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
using System.Web.Caching;
using System.Xml.Linq;


namespace ASC.Core
{
    class ClientPaymentManager : IPaymentManagerClient
    {
        private readonly IConfigurationClient config;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly string partnerUrl;
        private readonly string partnerKey;
        private readonly ICache cache = new AspCache();
        private readonly TimeSpan cacheTimeout = TimeSpan.FromMinutes(2);


        public ClientPaymentManager(IConfigurationClient config, IQuotaService quotaService, ITariffService tariffService)
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
            return tariffService.GetShoppingUri(tenant, quotaId);
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
                        throw new BillingException(element.Value);
                    }
                    var tariff = new Tariff
                    {
                        QuotaId = trial.Id,
                        State = TariffState.Trial,
                        DueDate = DateTime.UtcNow.Date.AddMonths(1),
                    };
                    tariffService.SetTariff(tenant, tariff);
                    var tt = tariffService.GetTariff(tenant);
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

        public void RequestClientPayment(string partnerId, int quotaId, bool requestKey)
        {
            var stringBuilder = new StringBuilder("/partnerapi/RequestClientPayment?");
            stringBuilder.AppendFormat("partnerId={0}", HttpUtility.UrlEncode(partnerId));
            stringBuilder.AppendFormat("&tariff={0}", HttpUtility.UrlEncode(quotaId.ToString(CultureInfo.InvariantCulture)));
            stringBuilder.AppendFormat("&portal={0}", HttpUtility.UrlEncode(CoreContext.TenantManager.GetCurrentTenant().TenantAlias));
            stringBuilder.AppendFormat("&userEmail={0}", HttpUtility.UrlEncode(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email));
            stringBuilder.AppendFormat("&requestType={0}", requestKey ? "Key" : "Payment");

            using (var webClient = new WebClient())
            {
                var actionUrl = stringBuilder.ToString();
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
            }
        }

        public void CreateClient(string partnerId, string email, string firstName, string lastName, string phone, string portal, string portalDomain)
        {
            try
            {
                var postData = string.Format("partnerId={0}&email={1}&firstName={2}&lastName={3}&phone={4}&portal={5}&portalDomain={6}", 
                    HttpUtility.UrlEncode(partnerId),
                    HttpUtility.UrlEncode(email),
                    HttpUtility.UrlEncode(firstName),
                    HttpUtility.UrlEncode(lastName),
                    HttpUtility.UrlEncode(phone),
                    HttpUtility.UrlEncode(portal),
                    HttpUtility.UrlEncode(portalDomain));

                var byte1 = new ASCIIEncoding().GetBytes(postData);

                var actionUrl = string.Format("/partnerapi/client?{0}", postData);
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Authorization", GetPartnerAuthHeader(actionUrl));
                    try
                    {
                        webClient.UploadData(partnerUrl + actionUrl, "POST", byte1);
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
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Error(ex);
            }
        }

        public Partner GetPartner(string key)
        {
            try
            {
                var actionUrl ="/partnerapi/partner/" + HttpUtility.UrlEncode(key);
                var partner = (Partner)HttpRuntime.Cache.Get(partnerUrl + actionUrl);
                if (partner == null)
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Authorization", GetPartnerAuthHeader(actionUrl));
                        try
                        {
                            var data = Encoding.UTF8.GetString(webClient.DownloadData(partnerUrl + actionUrl));
                            HttpRuntime.Cache.Insert(partnerUrl + actionUrl, partner = JsonConvert.DeserializeObject<Partner>(data), new CacheDependency(null, new[] { "PartnerCache" }), DateTime.UtcNow.Add(cacheTimeout), Cache.NoSlidingExpiration);
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
                    }
                }
                return partner;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Error(ex);
                return null;
            }
        }

        public Partner GetApprovedPartner()
        {
            var partnerId = CoreContext.TenantManager.GetCurrentTenant().PartnerId;
            if (!string.IsNullOrEmpty(partnerId))
            {
                var partner = GetPartner(partnerId);

                if (partner != null && partner.Status == PartnerStatus.Approved && !partner.Removed && partner.PartnerType != PartnerType.System)
                {
                    return partner;
                }
            }
            return null;
        }

        public IEnumerable<TenantQuota> GetPartnerTariffs(string partnerId)
        {
            try
            {
                var actionUrl = "/partnerapi/tariffs?partnerid=" + HttpUtility.UrlEncode(partnerId);
                var tariffs = (IEnumerable<TenantQuota>)cache.Get(partnerUrl + actionUrl);
                if (tariffs == null)
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Authorization", GetPartnerAuthHeader(actionUrl));
                        try
                        {
                            var data = Encoding.UTF8.GetString(webClient.DownloadData(partnerUrl + actionUrl));
                            cache.Insert(partnerUrl + actionUrl, tariffs = JsonConvert.DeserializeObject<TenantQuota[]>(data), DateTime.UtcNow.Add(cacheTimeout));
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
                    }
                }
                return tariffs;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Error(ex);
                return Enumerable.Empty<TenantQuota>();
            }
        }

        public string GetButton(string partnerId, int quotaId)
        {
            try
            {
                var buttonUrl = tariffService.GetButton(quotaId, partnerId);

                if (string.IsNullOrEmpty(buttonUrl))
                {
                    buttonUrl = CreateButton(quotaId, partnerId);
                }

                return AddCustomData(buttonUrl, quotaId, partnerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Error(ex);
                throw;
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

        private string CreateButton(int tariffID, string partnerID)
        {
            var postData = string.Format("partnerID={0}&tariffID={1}", HttpUtility.UrlEncode(partnerID), tariffID);
            var byte1 = new ASCIIEncoding().GetBytes(postData);

            var actionUrl = string.Format("/partnerapi/button?{0}", postData);
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", GetPartnerAuthHeader(actionUrl));
                var data = Encoding.UTF8.GetString(webClient.UploadData(partnerUrl + actionUrl, "POST", byte1));
                var buttonUrl = JsonConvert.DeserializeObject<string>(data);
                tariffService.SaveButton(tariffID, partnerID, buttonUrl);
                return buttonUrl;
            }
        }

        private string AddCustomData(string buttonUrl, int quotaId, string partnerId)
        {
            var amount = Amount(GetPartnerTariffs(partnerId).ToList(), quotaId);
            var amountDefault = Amount(quotaService.GetTenantQuotas().ToList(), quotaId);

            var data = string.Format("{0}|{1}|{2}|{3}|{4}", CoreContext.TenantManager.GetCurrentTenant().TenantAlias, quotaId, partnerId, ASC.Common.Utils.Signature.Create(amount, "partner"), ASC.Common.Utils.Signature.Create(amountDefault, "partner"));

            return string.Format("{0}&custom={1}&amount={2}&currency_code={3}", buttonUrl, HttpUtility.UrlEncode(data), HttpUtility.UrlEncode(amount.ToString(CultureInfo.InvariantCulture)), new RegionInfo(GetPartner(partnerId).Currency).ISOCurrencySymbol);
        }

        private decimal Amount(List<TenantQuota> quotas, int quotaId)
        {
            var currentTariff = GetTariff(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var currentQuota = quotas.FirstOrDefault(r => r.Id == currentTariff.QuotaId);
            var newQuota = quotas.First(r => r.Id == quotaId);

            //trial, prolong, month ->year, new buy
            if (currentQuota == null || currentQuota.Trial || newQuota.ActiveUsers == currentQuota.ActiveUsers || (!currentQuota.Year && newQuota.Year) || currentTariff.DueDate < DateTime.UtcNow)
                return newQuota.Price;

            //downgrade
            if (newQuota.ActiveUsers < currentQuota.ActiveUsers)
                return 0;

            //upgrade
            var start = currentQuota.Year ? currentTariff.DueDate.AddYears(-1) : currentTariff.DueDate.AddMonths(-1);
            var left = currentTariff.DueDate.Subtract(DateTime.UtcNow).TotalDays;
            var totalOldDays = currentTariff.DueDate.Subtract(start).TotalDays;

            var used = totalOldDays - left;
            var totalNewDays = (newQuota.Year ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1)).Subtract(DateTime.UtcNow).TotalDays;

            return Math.Round(newQuota.Price - currentQuota.Price * (decimal)(left / totalOldDays) - newQuota.Price * (decimal)(used / totalNewDays), 2);
        }

        private Exception GetException(WebException we)
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
