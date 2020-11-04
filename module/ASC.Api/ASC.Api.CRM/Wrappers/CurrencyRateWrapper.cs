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
using System.Runtime.Serialization;
using ASC.Api.CRM.Wrappers;
using ASC.CRM.Core;

namespace ASC.Api.CRM
{
    /// <summary>
    ///  Currency rate
    /// </summary>
    [DataContract(Name = "currencyRate", Namespace = "")]
    public class CurrencyRateWrapper : ObjectWrapperBase
    {
        public CurrencyRateWrapper(int id) : base(id)
        {
        }

        public CurrencyRateWrapper(CurrencyRate currencyRate)
            : base(currencyRate.ID)
        {
            FromCurrency = currencyRate.FromCurrency;
            ToCurrency = currencyRate.ToCurrency;
            Rate = currencyRate.Rate;
        }

        [DataMember]
        public String FromCurrency { get; set; }

        [DataMember]
        public String ToCurrency { get; set; }

        [DataMember]
        public decimal Rate { get; set; }

        public static CurrencyRateWrapper GetSample()
        {
            return new CurrencyRateWrapper(1)
            {
                FromCurrency = "EUR",
                ToCurrency = "USD",
                Rate = (decimal)1.1
            };
        }
    }
}