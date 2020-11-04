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