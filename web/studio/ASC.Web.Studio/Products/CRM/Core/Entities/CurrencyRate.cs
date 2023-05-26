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

namespace ASC.CRM.Core
{
    ///<inherited>ASC.CRM.Core.DomainObject,ASC.WEB.CRM</inherited>
    [DataContract]
    public class CurrencyRate : DomainObject
    {
        ///<example name="fromCurrency">fromCurrency</example>
        [DataMember(Name = "fromCurrency")]
        public string FromCurrency { get; set; }

        ///<example name="toCurrency">toCurrency</example>
        [DataMember(Name = "toCurrency")]
        public string ToCurrency { get; set; }

        ///<example name="rate" type="double">1.1</example>
        [DataMember(Name = "rate")]
        public decimal Rate { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid? LastModifedBy { get; set; }

        public DateTime? LastModifedOn { get; set; }
    }
}