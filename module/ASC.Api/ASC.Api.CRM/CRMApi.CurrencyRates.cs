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
using ASC.Api.Attributes;
using ASC.CRM.Core;
using ASC.MessagingSystem;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
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
            return DaoFactory.GetCurrencyRateDao().GetAll().ConvertAll(ToCurrencyRateWrapper);
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

            var currencyRate = DaoFactory.GetCurrencyRateDao().GetByID(id);

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

            var currencyRate = DaoFactory.GetCurrencyRateDao().GetByCurrencies(fromCurrency, toCurrency);

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
            if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency) || rate < 0)
                throw new ArgumentException();

            var currencyRate = new CurrencyRate
                {
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    Rate = rate
                };

            currencyRate.ID = DaoFactory.GetCurrencyRateDao().SaveOrUpdate(currencyRate);
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
            if (id <= 0 || string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency) || rate < 0)
                throw new ArgumentException();

            var currencyRate = new CurrencyRate
                {
                    ID = id,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    Rate = rate
                };

            currencyRate.ID = DaoFactory.GetCurrencyRateDao().SaveOrUpdate(currencyRate);
            MessageService.Send(Request, MessageAction.CurrencyRateUpdated, fromCurrency, toCurrency);

            return ToCurrencyRateWrapper(currencyRate);
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
            if (id <= 0) throw new ArgumentException();

            var currencyRate = DaoFactory.GetCurrencyRateDao().GetByID(id);

            DaoFactory.GetCurrencyRateDao().Delete(id);

            return ToCurrencyRateWrapper(currencyRate);
        }

        private static CurrencyRateWrapper ToCurrencyRateWrapper(CurrencyRate currencyRate)
        {
            return new CurrencyRateWrapper(currencyRate);
        }
    }
}