/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Web.CRM.Resources;

namespace ASC.CRM.Core
{
    [DataContract]
    public class CurrencyInfo
    {
        private String _resourceKey;

        [DataMember(Name = "title")]
        public String Title
        {
            get
            {

                if (String.IsNullOrEmpty(_resourceKey))
                    return String.Empty;

                return CRMCommonResource.ResourceManager.GetString(_resourceKey);

            }
        }

        [DataMember(Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Name = "abbreviation")]
        public string Abbreviation { get; set; }

        [DataMember(Name = "cultureName")]
        public string CultureName { get; set; }

        [DataMember(Name = "isConvertable")]
        public bool IsConvertable { get; set; }

        [DataMember(Name = "isBasic")]
        public bool IsBasic { get; set; }


        public CurrencyInfo(string resourceKey, string abbreviation, string symbol, string cultureName, bool isConvertable, bool isBasic)
        {
            _resourceKey = resourceKey;
            Symbol = symbol;
            Abbreviation = abbreviation;
            CultureName = cultureName;
            IsConvertable = isConvertable;
            IsBasic = isBasic;
        }

        public override bool Equals(object obj)
        {
            var ci = obj as CurrencyInfo;
            return ci != null && string.Compare(Title, ci.Title, true) == 0;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(Abbreviation, "-", Title);
        }

    }
}