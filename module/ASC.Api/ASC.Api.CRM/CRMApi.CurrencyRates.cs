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
using System.Linq;

using ASC.Api.Attributes;
using ASC.CRM.Core;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;

namespace ASC.Api.CRM
{
    ///<name>crm</name>
    public partial class CRMApi
    {
        //TABLE `crm_currency_rate` column `rate` DECIMAL(10,2) NOT NULL
        public const decimal MaxRateValue = (decimal)99999999.99;

        /// <summary>
        /// Returns a list of all the currency rates.
        /// </summary>
        /// <short>Get currency rates</short> 
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">
        /// List of currency rates
        /// </returns>
        /// <path>api/2.0/crm/currency/rates</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"currency/rates")]
        public IEnumerable<CurrencyRateWrapper> GetCurrencyRates()
        {
            return DaoFactory.CurrencyRateDao.GetAll().ConvertAll(ToCurrencyRateWrapper);
        }

        /// <summary>
        /// Returns a currency rate by ID.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Currency rate ID</param>
        /// <short>Get a currency rate by ID</short> 
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">
        /// Currency rate
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/crm/currency/rates/{id}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"currency/rates/{id:[0-9]+}")]
        public CurrencyRateWrapper GetCurrencyRate(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var currencyRate = DaoFactory.CurrencyRateDao.GetByID(id);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        /// Returns a currency rate by currencies.
        /// </summary>
        /// <param type="System.String, System" method="url" name="fromCurrency">Currency to convert</param>
        /// <param type="System.String, System" method="url" name="toCurrency">Currency into which the original currency will be converted</param>
        /// <short>Get a currency rate by currencies</short> 
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">
        /// Currency rate
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/crm/currency/rates/{fromCurrency}/{toCurrency}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"currency/rates/{fromCurrency}/{toCurrency}")]
        public CurrencyRateWrapper GetCurrencyRate(string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
                throw new ArgumentException();

            var currencyRate = DaoFactory.CurrencyRateDao.GetByCurrencies(fromCurrency, toCurrency);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        /// Creates a new currency rate with the parameters specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="fromCurrency">Currency to convert</param>
        /// <param type="System.String, System" name="toCurrency">Currency into which the original currency will be converted</param>
        /// <param type="System.Decimal, System" name="rate">Exchange rate</param>
        /// <short>Create a currency rate</short>
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">Currency rate</returns>
        /// <path>api/2.0/crm/currency/rates</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"currency/rates")]
        public CurrencyRateWrapper CreateCurrencyRate(string fromCurrency, string toCurrency, decimal rate)
        {
            ValidateRate(rate);

            ValidateCurrencies(new[] { fromCurrency, toCurrency });

            var currencyRate = new CurrencyRate
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rate
            };

            currencyRate.ID = DaoFactory.CurrencyRateDao.SaveOrUpdate(currencyRate);
            MessageService.Send(Request, MessageAction.CurrencyRateUpdated, fromCurrency, toCurrency);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        /// Updates a currency rate with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Currency ID</param>
        /// <param type="System.String, System" method="url" name="fromCurrency">New currency to convert</param>
        /// <param type="System.String, System" method="url" name="toCurrency">New currency into which the original currency will be converted</param>
        /// <param type="System.Decimal, System" name="rate">New currency rate</param>
        /// <short>Update a currency rate</short>
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">Updated currency rate</returns>
        /// <path>api/2.0/crm/currency/rates/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"currency/rates/{id:[0-9]+}")]
        public CurrencyRateWrapper UpdateCurrencyRate(int id, string fromCurrency, string toCurrency, decimal rate)
        {
            if (id <= 0)
                throw new ArgumentException();

            ValidateRate(rate);

            ValidateCurrencies(new[] { fromCurrency, toCurrency });

            var currencyRate = DaoFactory.CurrencyRateDao.GetByID(id);

            if (currencyRate == null)
                throw new ArgumentException();

            currencyRate.FromCurrency = fromCurrency;
            currencyRate.ToCurrency = toCurrency;
            currencyRate.Rate = rate;

            currencyRate.ID = DaoFactory.CurrencyRateDao.SaveOrUpdate(currencyRate);
            MessageService.Send(Request, MessageAction.CurrencyRateUpdated, fromCurrency, toCurrency);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        /// Sets currency rates to the currency specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by API">Currency (abbreviation)</param>
        /// <param type="System.Collections.Generic.List{ASC.CRM.Core.CurrencyRate}, System.Collections.Generic" name="rates">List of currency rates</param>
        /// <short>Set currency rates</short>
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">Currency information</returns>
        /// <path>api/2.0/crm/currency/setrates</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        [Create(@"currency/setrates")]
        public List<CurrencyRateWrapper> SetCurrencyRates(String currency, List<CurrencyRate> rates)
        {
            if (!CRMSecurity.IsAdmin)
                throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(currency))
                throw new ArgumentException();

            ValidateCurrencyRates(rates);

            currency = currency.ToUpper();

            if (Global.TenantSettings.DefaultCurrency.Abbreviation != currency)
            {
                var cur = CurrencyProvider.Get(currency);

                if (cur == null)
                    throw new ArgumentException();

                Global.SaveDefaultCurrencySettings(cur);

                MessageService.Send(Request, MessageAction.CrmDefaultCurrencyUpdated);
            }

            rates = DaoFactory.CurrencyRateDao.SetCurrencyRates(rates);

            foreach (var rate in rates)
            {
                MessageService.Send(Request, MessageAction.CurrencyRateUpdated, rate.FromCurrency, rate.ToCurrency);
            }

            return rates.Select(ToCurrencyRateWrapper).ToList();
        }

        /// <summary>
        /// Adds currency rates specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{ASC.CRM.Core.CurrencyRate}, System.Collections.Generic" name="rates">List of currency rates</param>
        /// <short>Add currency rates</short>
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">Currency information</returns>
        /// <path>api/2.0/crm/currency/addrates</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        [Create(@"currency/addrates")]
        public List<CurrencyRateWrapper> AddCurrencyRates(List<CurrencyRate> rates)
        {
            if (!CRMSecurity.IsAdmin)
                throw CRMSecurity.CreateSecurityException();

            ValidateCurrencyRates(rates);

            var existingRates = DaoFactory.CurrencyRateDao.GetAll();

            foreach (var rate in rates)
            {
                var exist = false;

                foreach (var existingRate in existingRates)
                {
                    if (rate.FromCurrency != existingRate.FromCurrency || rate.ToCurrency != existingRate.ToCurrency)
                        continue;

                    existingRate.Rate = rate.Rate;
                    DaoFactory.CurrencyRateDao.SaveOrUpdate(existingRate);
                    MessageService.Send(Request, MessageAction.CurrencyRateUpdated, rate.FromCurrency, rate.ToCurrency);
                    exist = true;
                    break;
                }

                if (exist) continue;

                rate.ID = DaoFactory.CurrencyRateDao.SaveOrUpdate(rate);
                MessageService.Send(Request, MessageAction.CurrencyRateUpdated, rate.FromCurrency, rate.ToCurrency);
                existingRates.Add(rate);
            }

            return existingRates.Select(ToCurrencyRateWrapper).ToList();
        }
        /// <summary>
        /// Deletes a currency rate with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Currency rate ID</param>
        /// <short>Delete a currency rate</short>
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyRateWrapper, ASC.Api.CRM">Currency rate</returns>
        /// <path>api/2.0/crm/currency/rates/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"currency/rates/{id:[0-9]+}")]
        public CurrencyRateWrapper DeleteCurrencyRate(int id)
        {
            if (id <= 0)
                throw new ArgumentException();

            var currencyRate = DaoFactory.CurrencyRateDao.GetByID(id);

            if (currencyRate == null)
                throw new ArgumentException();

            DaoFactory.CurrencyRateDao.Delete(id);

            return ToCurrencyRateWrapper(currencyRate);
        }

        private static void ValidateCurrencyRates(IEnumerable<CurrencyRate> rates)
        {
            var currencies = new List<string>();

            foreach (var rate in rates)
            {
                ValidateRate(rate.Rate);
                currencies.Add(rate.FromCurrency);
                currencies.Add(rate.ToCurrency);
            }

            ValidateCurrencies(currencies.ToArray());
        }

        private static void ValidateCurrencies(string[] currencies)
        {
            if (currencies.Any(string.IsNullOrEmpty))
                throw new ArgumentException();

            var available = CurrencyProvider.GetAll().Select(x => x.Abbreviation);

            var unknown = currencies.Where(x => !available.Contains(x)).ToArray();

            if (!unknown.Any()) return;

            throw new ArgumentException(string.Format(CRMErrorsResource.UnknownCurrency, string.Join(",", unknown)));
        }

        private static void ValidateRate(decimal rate)
        {
            if (rate < 0 || rate > MaxRateValue)
                throw new ArgumentException(string.Format(CRMErrorsResource.InvalidCurrencyRate, rate));
        }

        private static CurrencyRateWrapper ToCurrencyRateWrapper(CurrencyRate currencyRate)
        {
            return new CurrencyRateWrapper(currencyRate);
        }
    }
}