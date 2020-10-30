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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ASC.Common.Logging;
using Newtonsoft.Json;

namespace ASC.Core.Common.Billing
{
    public class CouponManager
    {
        private static IEnumerable<AvangateProduct> Products { get; set; }
        private static IEnumerable<string> Groups { get; set; }
        private static readonly int Percent;
        private static readonly int Schedule;
        private static readonly string VendorCode;
        private static readonly byte[] Secret;
        private static readonly Uri BaseAddress;
        private static readonly string ApiVersion;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private static readonly ILog Log;

        static CouponManager()
        {
            SemaphoreSlim = new SemaphoreSlim(1, 1);
            Log = LogManager.GetLogger("ASC");

            try
            {
                var cfg = (AvangateCfgSectionHandler)ConfigurationManagerExtension.GetSection("avangate");
                Secret = Encoding.UTF8.GetBytes(cfg.Secret);
                VendorCode = cfg.Vendor;
                Percent = cfg.Percent;
                Schedule = cfg.Schedule;
                BaseAddress = new Uri(cfg.BaseAddress);
                ApiVersion = "/rest/" + cfg.ApiVersion.TrimStart('/');
                Groups = (cfg.Groups ?? "").Split(',', '|', ' ');
            }
            catch (Exception e)
            {
                Secret = Encoding.UTF8.GetBytes("");
                VendorCode = "";
                Percent = AvangateCfgSectionHandler.DefaultPercent;
                Schedule = AvangateCfgSectionHandler.DefaultShedule;
                BaseAddress = new Uri(AvangateCfgSectionHandler.DefaultAdress);
                ApiVersion = AvangateCfgSectionHandler.DefaultApiVersion;
                Groups = new List<string>();
                Log.Fatal(e);
            }
        }

        public static string CreateCoupon()
        {
            return CreatePromotionAsync().Result;
        }

        private static async Task<string> CreatePromotionAsync()
        {
            try
            {
                using (var httpClient = PrepaireClient())
                using (var content = new StringContent(await Promotion.GeneratePromotion(Percent, Schedule), Encoding.Default, "application/json"))
                using (var response = await httpClient.PostAsync(string.Format("{0}/promotions/", ApiVersion), content))
                {
                    if (!response.IsSuccessStatusCode)
                        throw new HttpException((int) response.StatusCode, response.ReasonPhrase);

                    var result = await response.Content.ReadAsStringAsync();
                    await Task.Delay(1000 - DateTime.UtcNow.Millisecond); // otherwise authorize exception
                    var createdPromotion = JsonConvert.DeserializeObject<Promotion>(result);
                    return createdPromotion.Coupon.Code;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        internal static async Task<IEnumerable<AvangateProduct>> GetProducts()
        {
            if (Products != null) return Products;
            
            await SemaphoreSlim.WaitAsync();

            if (Products != null)
            {
                SemaphoreSlim.Release();
                return Products;
            }

            try
            {
                using (var httpClient = PrepaireClient())
                using (var response = await httpClient.GetAsync(string.Format("{0}/products/?Limit=1000&Enabled=true", ApiVersion)))
                {
                    if (!response.IsSuccessStatusCode)
                        throw new HttpException((int) response.StatusCode, response.ReasonPhrase);

                    var result = await response.Content.ReadAsStringAsync();
                    Log.Debug(result);

                    var products = JsonConvert.DeserializeObject<List<AvangateProduct>>(result);
                    products = products.Where(r => r.ProductGroup != null && Groups.Contains(r.ProductGroup.Code)).ToList();
                    return Products = products;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private static HttpClient PrepaireClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            const string applicationJson = "application/json";
            var httpClient = new HttpClient {BaseAddress = BaseAddress, Timeout = TimeSpan.FromMinutes(3)};
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", applicationJson);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", applicationJson);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Avangate-Authentication", CreateAuthHeader());
            return httpClient;
        }

        private static string CreateAuthHeader()
        {
            using (var hmac = new HMACMD5(Secret))
            {
                var date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var hash = VendorCode.Length + VendorCode + date.Length + date;
                var data = hmac.ComputeHash(Encoding.UTF8.GetBytes(hash));

                var sBuilder = new StringBuilder();
                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("code='{0}' ", VendorCode);
                stringBuilder.AppendFormat("date='{0}' ", date);
                stringBuilder.AppendFormat("hash='{0}'", sBuilder);
                return stringBuilder.ToString();
            }
        }
    }

    class Promotion
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Enabled { get; set; }
        public int MaximumOrdersNumber { get; set; }
        public bool InstantDiscount { get; set; }
        public string ChannelType { get; set; }
        public string ApplyRecurring { get; set; }
        public Coupon Coupon { get; set; }
        public Discount Discount { get; set; }
        public IEnumerable<CouponProduct> Products { get; set; }
        public int PublishToAffiliatesNetwork {get; set;}
        public int AutoApply { get; set; }

        public static async Task<string> GeneratePromotion(int percent, int schedule)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var startDate = DateTime.UtcNow.Date;
                var endDate = startDate.AddDays(schedule);
                var code = tenant.TenantAlias;

                var promotion = new Promotion
                {
                    Type = "REGULAR",
                    Enabled = true,
                    MaximumOrdersNumber = 1,
                    InstantDiscount = false,
                    ChannelType = "ECOMMERCE",
                    ApplyRecurring = "NONE",
                    PublishToAffiliatesNetwork = 0,
                    AutoApply = 0,

                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd"),
                    Name = string.Format("{0} {1}% off", code, percent),
                    Coupon = new Coupon {Type = "SINGLE", Code = code},
                    Discount = new Discount {Type = "PERCENT", Value = percent},
                    Products = (await CouponManager.GetProducts()).Select(r => new CouponProduct { Code = r.ProductCode })

                };

                return JsonConvert.SerializeObject(promotion);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error(ex.Message, ex);
                throw;
            }
        }
    }

    class Coupon
    {
        public string Type { get; set; }
        public string Code { get; set; }
    }

    class Discount
    {
        public string Type { get; set; }
        public int Value { get; set; }
    }

    class AvangateProduct
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public AvangateProductGroup ProductGroup { get; set; }
    }

    class AvangateProductGroup
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    class CouponProduct
    {
        public string Code { get; set; }
    }

    class AvangateCfgSectionHandler : ConfigurationSection
    {
        public const string DefaultAdress = "https://api.avangate.com/";
        public const string DefaultApiVersion = "4.0";
        public const int DefaultPercent = 5;
        public const int DefaultShedule = 10;

        [ConfigurationProperty("secret")]
        public string Secret
        {
            get { return (string)this["secret"]; }
        }

        [ConfigurationProperty("vendor")]
        public string Vendor
        {
            get { return (string)this["vendor"]; }
            set { this["vendor"] = value; }
        }

        [ConfigurationProperty("percent", DefaultValue = DefaultPercent)]
        public int Percent
        {
            get { return Convert.ToInt32(this["percent"]); }
            set { this["percent"] = value; }
        }

        [ConfigurationProperty("schedule", DefaultValue = DefaultShedule)]
        public int Schedule
        {
            get { return Convert.ToInt32(this["schedule"]); }
            set { this["schedule"] = value; }
        }

        [ConfigurationProperty("groups")]
        public string Groups
        {
            get { return (string)this["groups"]; }
        }

        [ConfigurationProperty("address", DefaultValue = DefaultAdress)]
        public string BaseAddress
        {
            get { return (string)this["address"]; }
            set { this["address"] = value; }
        }

        [ConfigurationProperty("apiVersion", DefaultValue = DefaultApiVersion)]
        public string ApiVersion
        {
            get { return (string)this["apiVersion"]; }
            set { this["apiVersion"] = value; }
        }
    }
}