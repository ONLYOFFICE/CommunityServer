/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using System.Xml;
using ASC.CRM.Core;
using ASC.Web.CRM.Resources;
using log4net;
using System.Security.Principal;

#endregion

namespace ASC.Web.CRM.Classes
{

    public static class CurrencyProvider
    {

        #region Members

        private static readonly ILog _log = LogManager.GetLogger(typeof(CurrencyProvider));
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<String, CurrencyInfo> _currencies;
        private static Dictionary<String, Decimal> _exchangeRates;
        private static DateTime _publisherDate;

        #endregion

        #region Constructor

        static CurrencyProvider()
        {
            var currencies = Global.DaoFactory.GetCurrencyInfoDao().GetAll();

            if (currencies == null || currencies.Count == 0)
            {
                currencies = new List<CurrencyInfo>
                    {
                        new CurrencyInfo("Currency_UnitedStatesDollar", "USD", "$", "US", true, true)
                    };
            }

            _currencies = currencies.ToDictionary(c => c.Abbreviation);
        }

        #endregion

        #region Property

        public static DateTime GetPublisherDate
        {
            get { return _publisherDate; }
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

                            var tmppath = Environment.GetEnvironmentVariable("TEMP");
                            if (string.IsNullOrEmpty(tmppath))
                            {
                                tmppath = Path.GetTempPath();
                            }
                            tmppath = Path.Combine(tmppath, WindowsIdentity.GetCurrent().Name + "\\Teamlab\\crm\\Exchange_Rates\\");

                            if (_publisherDate == default(DateTime))
                            {
                                try
                                {
                                    var timefile = Path.Combine(tmppath, "last.time");
                                    if (File.Exists(timefile))
                                    {
                                        _publisherDate = DateTime.ParseExact(File.ReadAllText(timefile), "o", null);
                                    }
                                }
                                catch (Exception err)
                                {
                                    LogManager.GetLogger("ASC.CRM").Error(err);
                                }
                            }

                            var regex = new Regex("= (?<Currency>([\\s\\.\\d]*))");
                            var updateEnable = WebConfigurationManager.AppSettings["crm.update.currency.info.enable"] != "false";
                            foreach (var ci in _currencies.Values.Where(c => c.IsConvertable))
                            {
                                var filepath = Path.Combine(tmppath, ci.Abbreviation + ".xml");

                                if (updateEnable && 0 < (DateTime.UtcNow.Date - _publisherDate.Date).TotalDays || !File.Exists(filepath))
                                {
                                    DownloadRSS(ci.Abbreviation, filepath);
                                }

                                if (!File.Exists(filepath))
                                {
                                    continue;
                                }

                                using (var reader = XmlReader.Create(filepath))
                                {
                                    var feed = SyndicationFeed.Load(reader);
                                    if (feed != null)
                                    {
                                        foreach (var item in feed.Items)
                                        {
                                            var currency = regex.Match(item.Summary.Text).Groups["Currency"].Value.Trim();
                                            _exchangeRates.Add(item.Title.Text, Convert.ToDecimal(currency, CultureInfo.InvariantCulture.NumberFormat));
                                        }
                                    }

                                    _publisherDate = feed.LastUpdatedTime.DateTime;
                                }
                            }

                            try
                            {
                                var timefile = Path.Combine(tmppath, "last.time");
                                File.WriteAllText(timefile, _publisherDate.ToString("o"));
                            }
                            catch (Exception err)
                            {
                                LogManager.GetLogger("ASC.CRM").Error(err);
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

        private static void DownloadRSS(string currency, string filepath)
        {

            try
            {
                var dir = Path.GetDirectoryName(filepath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var destinationURI = new Uri(String.Format("http://themoneyconverter.com/rss-feed/{0}/rss.xml", currency));

                var request = (HttpWebRequest)WebRequest.Create(destinationURI);
                request.Method = "GET";
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 2;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:8.0) Gecko/20100101 Firefox/8.0";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = new StreamReader(response.GetResponseStream()))
                {
                    var data = responseStream.ReadToEnd();

                    File.WriteAllText(filepath, data);

                }
            }
            catch (Exception error)
            {
                _log.Error(error);
            }
        }

        #endregion
    }
}
