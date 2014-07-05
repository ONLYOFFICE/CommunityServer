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

using System;
using System.Security;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///    Get the list of currency rates
        /// </summary>
        [Read(@"currency/rates")]
        public IEnumerable<CurrencyRateWrapper> GetCurrencyRates()
        {
            return DaoFactory.GetCurrencyRateDao().GetAll().ConvertAll(ToCurrencyRateWrapper);
        }
        
        /// <summary>
        ///   Get currency rate by id
        /// </summary>
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

            currencyRate.ID  = DaoFactory.GetCurrencyRateDao().SaveOrUpdate(currencyRate);

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        ///    Update currency rate object
        /// </summary>
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

            return ToCurrencyRateWrapper(currencyRate);
        }

        /// <summary>
        ///    Delete currency rate object
        /// </summary>
        [Delete(@"currency/rates/{id:[0-9]+}")]
        public CurrencyRateWrapper DeleteCurrencyRate(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var currencyRate = DaoFactory.GetCurrencyRateDao().GetByID(id);

            DaoFactory.GetCurrencyRateDao().Delete(id);

            return ToCurrencyRateWrapper(currencyRate);
        }

        private CurrencyRateWrapper ToCurrencyRateWrapper(CurrencyRate currencyRate)
        {
            return new CurrencyRateWrapper(currencyRate);
        }
    }
}