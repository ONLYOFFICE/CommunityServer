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
using System.Runtime.Serialization;

using ASC.CRM.Core;

namespace ASC.Api.CRM
{
    /// <summary>
    ///  Currency information
    /// </summary>
    [DataContract(Name = "currencyInfo", Namespace = "")]
    public class CurrencyInfoWrapper
    {
        public CurrencyInfoWrapper()
        {
        }

        public CurrencyInfoWrapper(CurrencyInfo currencyInfo)
        {
            Abbreviation = currencyInfo.Abbreviation;
            CultureName = currencyInfo.CultureName;
            Symbol = currencyInfo.Symbol;
            Title = currencyInfo.Title;
            IsConvertable = currencyInfo.IsConvertable;
            IsBasic = currencyInfo.IsBasic;
        }

        ///<example>Chinese Yuan</example>
        [DataMember]
        public String Title { get; set; }

        ///<example>¥</example>
        [DataMember]
        public String Symbol { get; set; }

        ///<example>CNY</example>
        [DataMember]
        public String Abbreviation { get; set; }

        ///<example>CN</example>
        [DataMember]
        public String CultureName { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool IsConvertable { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool IsBasic { get; set; }

        public static CurrencyInfoWrapper GetSample()
        {
            return new CurrencyInfoWrapper
            {
                Title = "Chinese Yuan",
                Abbreviation = "CNY",
                Symbol = "¥",
                CultureName = "CN",
                IsConvertable = true,
                IsBasic = false
            };
        }
    }


    /// <summary>
    ///  Currency rate information
    /// </summary>
    /// <inherited>ASC.Api.CRM.CurrencyInfoWrapper, ASC.Api.CRM</inherited>
    [DataContract(Name = "currencyRateInfo", Namespace = "")]
    public class CurrencyRateInfoWrapper : CurrencyInfoWrapper
    {
        public CurrencyRateInfoWrapper()
        {
        }

        public CurrencyRateInfoWrapper(CurrencyInfo currencyInfo, Decimal rate) : base(currencyInfo)
        {
            Rate = rate;
        }

        ///<example type="double">1,1</example>
        [DataMember]
        public Decimal Rate { get; set; }
    }
}