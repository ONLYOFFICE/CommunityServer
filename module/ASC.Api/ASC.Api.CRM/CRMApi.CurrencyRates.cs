/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using ASC.Api.Attributes;
using ASC.CRM.Core;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        //TABLE `crm_currency_rate` column `rate` DECIMAL(10,2) NOT NULL
        public const decimal MaxRateValue = (decimal) 99999999.99;

        /// <summary>
        ///    Get the list of currency rates
        /// </summary>
        /// <short>Get currency rates list</short> 
        /// <category>Common</category>
        /// <returns>
        ///    List of currency rates
        /// </returns>
        [Read(@"currency/rates")]
        public IEnumerable<CurrencyRateWrapper> GetCurrencyRates()
        {
            return DaoFactory.CurrencyRateDao.GetAll().ConvertAll(ToCurrencyRateWrapper);
        }

        /// <summary>
        ///   Get currency rate by id
        /// </summary>
        /// <short>Get currency rate</short> 
        /// <category>Common</category>
        /// <returns>
        ///    Currency rate
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        [Read(@"currency/rates/{id:[0-9]+}")]
        public CurrencyRateWrapper GetCurrencyRate(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var currencyRate = DaoFactory.CurrencyRateDao.GetByID(id);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        ///   Get currency rate by currencies
        /// </summary>
        /// <short>Get currency rate</short> 
        /// <category>Common</category>
        /// <returns>
        ///    Currency rate
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        [Read(@"currency/rates/{fromCurrency}/{toCurrency}")]
        public CurrencyRateWrapper GetCurrencyRate(string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
                throw new ArgumentException();

            var currencyRate = DaoFactory.CurrencyRateDao.GetByCurrencies(fromCurrency, toCurrency);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        ///    Create new currency rate object
        /// </summary>
        /// <short></short>
        /// <category>Common</category>
        /// <returns></returns>
        [Create(@"currency/rates")]
        public CurrencyRateWrapper CreateCurrencyRate(string fromCurrency, string toCurrency, decimal rate)
        {
            ValidateRate(rate);

            ValidateCurrencies(new[] {fromCurrency, toCurrency});

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
        ///    Update currency rate object
        /// </summary>
        /// <short></short>
        /// <category>Common</category>
        /// <returns></returns>
        [Update(@"currency/rates/{id:[0-9]+}")]
        public CurrencyRateWrapper UpdateCurrencyRate(int id, string fromCurrency, string toCurrency, decimal rate)
        {
            if (id <= 0)
                throw new ArgumentException();

            ValidateRate(rate);

            ValidateCurrencies(new[] {fromCurrency, toCurrency});

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
        ///    Set currency rates
        /// </summary>
        /// <short></short>
        /// <category>Common</category>
        /// <returns></returns>
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
        ///    Add currency rates
        /// </summary>
        /// <short></short>
        /// <category>Common</category>
        /// <returns></returns>
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
        ///    Delete currency rate object
        /// </summary>
        /// <short></short>
        /// <category>Common</category>
        /// <returns></returns>
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