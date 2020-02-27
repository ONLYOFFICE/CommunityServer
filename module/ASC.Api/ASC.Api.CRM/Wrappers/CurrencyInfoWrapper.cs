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

        [DataMember]
        public String Title { get; set; }

        [DataMember]
        public String Symbol { get; set; }

        [DataMember]
        public String Abbreviation { get; set; }

        [DataMember]
        public String CultureName { get; set; }

        [DataMember]
        public bool IsConvertable { get; set; }

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

        [DataMember]
        public Decimal Rate { get; set; }
    }
}