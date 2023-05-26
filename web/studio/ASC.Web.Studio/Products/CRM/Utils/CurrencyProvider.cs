/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Core;

using Autofac;

using HtmlAgilityPack;

namespace ASC.Web.CRM.Classes
{

    public static class CurrencyProvider
    {

        #region Members

        private static readonly ILog _log = LogManager.GetLogger("ASC");
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<String, CurrencyInfo> _currencies;
        private static Dictionary<String, Decimal> _exchangeRates;
        private static DateTime _publisherDate;
        private const String _formatDate = "yyyy-MM-ddTHH:mm:ss.fffffffK";
        private static HttpClient httpClient;

        #endregion

        #region Constructor

        static CurrencyProvider()
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var currencies = daoFactory.CurrencyInfoDao.GetAll();

                if (currencies == null || currencies.Count == 0)
                {
                    currencies = new List<CurrencyInfo>
                    {
                        new CurrencyInfo("Currency_UnitedStatesDollar", "USD", "$", "US", true, true)
                    };
                }

                _currencies = currencies.ToDictionary(c => c.Abbreviation);
            }

            var httpHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseDefaultCredentials = true,
                MaxAutomaticRedirections = 2,

            };

            httpClient = HttpClientFactory.CreateClient(nameof(CurrencyProvider), httpHandler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; rv:8.0) Gecko/20100101 Firefox/8.0");
        }

        #endregion

        #region Property

        public static DateTime GetPublisherDate
        {
            get
            {
                TryToReadPublisherDate(GetExchangesTempPath());
                return _publisherDate;
            }
        }

        #endregion

        #region Public Methods

        public static CurrencyInfo Get(string currencyAbbreviation)
        {
            if (!_currencies.ContainsKey(currencyAbbreviation))
                return null;

            return _currencies[currencyAbbreviation];
        }

        public static List<CurrencyInfo> GetAll()
        {
            return _currencies.Values.OrderBy(v => v.Abbreviation).ToList();
        }

        public static List<CurrencyInfo> GetBasic()
        {
            return _currencies.Values.Where(c => c.IsBasic).OrderBy(v => v.Abbreviation).ToList();
        }

        public static List<CurrencyInfo> GetOther()
        {
            return _currencies.Values.Where(c => !c.IsBasic).OrderBy(v => v.Abbreviation).ToList();
        }

        public static Dictionary<CurrencyInfo, Decimal> MoneyConvert(CurrencyInfo baseCurrency)
        {
            if (baseCurrency == null) throw new ArgumentNullException("baseCurrency");
            if (!_currencies.ContainsKey(baseCurrency.Abbreviation)) throw new ArgumentOutOfRangeException("baseCurrency", "Not found.");

            var result = new Dictionary<CurrencyInfo, Decimal>();
            var rates = GetExchangeRates();
            foreach (var ci in GetAll())
            {

                if (baseCurrency.Title == ci.Title)
                {
                    result.Add(ci, 1);

                    continue;
                }

                var key = String.Format("{1}/{0}", baseCurrency.Abbreviation, ci.Abbreviation);

                if (!rates.ContainsKey(key))
                    continue;

                result.Add(ci, rates[key]);
            }
            return result;
        }


        public static bool IsConvertable(String abbreviation)
        {
            var findedItem = _currencies.Keys.ToList().Find(item => String.Compare(abbreviation, item) == 0);

            if (findedItem == null)
                throw new ArgumentException(abbreviation);

            return _currencies[findedItem].IsConvertable;
        }

        public static Decimal MoneyConvert(decimal amount, string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.Compare(from, to, true) == 0) return amount;

            var rates = GetExchangeRates();

            if (from.Contains('-')) from = new RegionInfo(from).ISOCurrencySymbol;
            if (to.Contains('-')) to = new RegionInfo(to).ISOCurrencySymbol;
            var key = string.Format("{0}/{1}", to, from);

            return Math.Round(rates[key] * amount, 4, MidpointRounding.AwayFromZero);
        }

        public static Decimal MoneyConvertToDefaultCurrency(decimal amount, string from)
        {
            return MoneyConvert(amount, from, Global.TenantSettings.DefaultCurrency.Abbreviation);
        }

        #endregion

        #region Private Methods

        private static bool ObsoleteData()
        {
            return _exchangeRates == null || (DateTime.UtcNow.Date.Subtract(_publisherDate.Date).Days > 0);
        }

        private static string GetExchangesTempPath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var companyAttribute = assembly?.GetCustomAttribute<AssemblyCompanyAttribute>();
            return Path.Combine(TempPath.GetTempPath(), companyAttribute?.Company ?? string.Empty, "exchanges");
        }

        private static Dictionary<String, Decimal> GetExchangeRates()
        {
            if (ObsoleteData())
            {
                lock (_syncRoot)
                {
                    if (ObsoleteData())
                    {
                        try
                        {
                            _exchangeRates = new Dictionary<string, decimal>();

                            if (ConfigurationManagerExtension.AppSettings["crm.update.currency.info.enable"] == "false")
                            {
                                return _exchangeRates;
                            }

                            var tmppath = GetExchangesTempPath();

                            TryToReadPublisherDate(tmppath);

                            var ratesUpdatedFlag = false;

                            foreach (var ci in _currencies.Values.Where(c => c.IsConvertable))
                            {
                                var filepath = Path.Combine(tmppath, ci.Abbreviation + ".html");

                                if (0 < (DateTime.UtcNow.Date - _publisherDate.Date).TotalDays || !File.Exists(filepath))
                                {
                                    var filepath_temp = Path.Combine(tmppath, ci.Abbreviation + "_temp.html");

                                    DownloadCurrencyPage(ci.Abbreviation, filepath_temp);

                                    if (File.Exists(filepath_temp))
                                    {
                                        if (TryGetRatesFromFile(filepath_temp, ci))
                                        {
                                            ratesUpdatedFlag = true;
                                            File.Copy(filepath_temp, filepath, true);
                                        }
                                        File.Delete(filepath_temp);
                                        continue;
                                    }
                                }

                                if (File.Exists(filepath) && TryGetRatesFromFile(filepath, ci))
                                {
                                    ratesUpdatedFlag = true;
                                }
                            }

                            if (ratesUpdatedFlag)
                            {
                                _publisherDate = DateTime.UtcNow;
                                WritePublisherDate(tmppath);
                            }
                            else
                            {
                                throw new Exception("Сurrency rates are not updated");
                            }
                        }
                        catch (Exception error)
                        {
                            LogManager.GetLogger("ASC.CRM").Error(error);
                            _publisherDate = DateTime.UtcNow;
                        }
                    }
                }
            }

            return _exchangeRates;
        }

        private static bool TryGetRatesFromFile(string filepath, CurrencyInfo curCI)
        {
            var success = false;

            var doc = new HtmlDocument();
            doc.Load(filepath);

            var targets = new[] { "major-currency-table", "minor-currency-table", "exotic-currency-table" };
            var tables = doc.DocumentNode.SelectNodes("//table");

            foreach (var table in tables)
            {
                var idAttr = table.Attributes["id"];
                if (idAttr == null || !targets.Contains(idAttr.Value))
                {
                    continue;
                }

                string abbreviation = null;
                decimal rate = 0;

                var tds = table.SelectNodes(".//td");
                foreach (var td in tds)
                {
                    var em = td.SelectSingleNode(".//em");
                    if (em != null)
                    {
                        var classAttr = em.Attributes["class"];
                        abbreviation = classAttr != null ? classAttr.Value : null;
                    }

                    var dataValueAttr = td.Attributes["data-value"];
                    if (dataValueAttr != null)
                    {
                        if (decimal.TryParse(dataValueAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out rate) && !string.IsNullOrEmpty(abbreviation))
                        {
                            _exchangeRates.Add(string.Format("{0}/{1}", abbreviation, curCI.Abbreviation), rate);
                            success = true;
                        }
                        abbreviation = null;
                    }
                }
            }

            return success;
        }


        private static void TryToReadPublisherDate(string tmppath)
        {
            if (_publisherDate == default(DateTime))
            {
                try
                {
                    var timefile = Path.Combine(tmppath, "last.time");
                    if (File.Exists(timefile))
                    {
                        var dateFromFile = File.ReadAllText(timefile);
                        _publisherDate = DateTime.ParseExact(dateFromFile, _formatDate, null);
                    }
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.CRM").Error(err);
                }
            }
        }

        private static void WritePublisherDate(string tmppath)
        {
            try
            {
                var timefile = Path.Combine(tmppath, "last.time");
                File.WriteAllText(timefile, _publisherDate.ToString(_formatDate));
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.CRM").Error(err);
            }
        }

        private static void DownloadCurrencyPage(string currency, string filepath)
        {

            try
            {
                var dir = Path.GetDirectoryName(filepath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var destinationURI = new Uri(string.Format("https://themoneyconverter.com/{0}/{0}", currency));

                Func<Task<string>> requestFunc = async () =>
                {
                    return await httpClient.GetStringAsync(destinationURI);
                };

                var data = Task.Run(() => ResiliencePolicyManager.GetStringWithPoliciesAsync("DownloadCurrencyPage", requestFunc)).Result;
                File.WriteAllText(filepath, data);

                System.Threading.Thread.Sleep(100);// limit 10 requests per second

            }
            catch (Exception error)
            {
                _log.Error("DownloadCurrencyPage failed for currency: " + currency, error);
            }
        }

        #endregion
    }
}
