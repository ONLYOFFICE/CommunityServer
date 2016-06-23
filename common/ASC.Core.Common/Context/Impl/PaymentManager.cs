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


using ASC.Common.Caching;
using ASC.Core.Billing;
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
    public class PaymentManager
    {
        private readonly CoreConfiguration config;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly string partnerUrl;
        private readonly string partnerKey;
        private readonly ICache cache = AscCache.Memory;
        private readonly TimeSpan cacheTimeout = TimeSpan.FromMinutes(2);


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
                LogManager.GetLogger("ASC.Core.Billing").Error(ex);
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
                            HttpRuntime.Cache.Remove(partnerUrl + actionUrl);
                            HttpRuntime.Cache.Insert(partnerUrl + actionUrl, partner = JsonConvert.DeserializeObject<Partner>(data), 
                                new CacheDependency(null, new[] { "PartnerCache" }), DateTime.Now.Add(cacheTimeout), Cache.NoSlidingExpiration);
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
                LogManager.GetLogger("ASC.Core.Billing").Error(ex);
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
                var tariffs = cache.Get<TenantQuota[]>(partnerUrl + actionUrl);
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
                LogManager.GetLogger("ASC.Core.Billing").Error(ex);
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
                LogManager.GetLogger("ASC.Core.Billing").Error(ex);
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
            var left = currentTariff.DueDate.Date.Subtract(DateTime.Today).TotalDays;
            var totalOldDays = currentTariff.DueDate.Date.Subtract(start.Date).TotalDays;

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
