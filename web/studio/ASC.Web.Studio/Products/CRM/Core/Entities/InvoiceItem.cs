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
using ASC.Common.Security;


namespace ASC.CRM.Core.Entities
{
    [DataContract]
    public class InvoiceItem : DomainObject, ISecurityObjectId
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "stockKeepingUnit")]
        public string StockKeepingUnit { get; set; }

        [DataMember(Name = "price")]
        public decimal Price { get; set; }

        [DataMember(Name = "stockQuantity")]
        public decimal StockQuantity { get; set; }

        [DataMember(Name = "trackInventory")]
        public bool TrackInventory { get; set; }

        [DataMember(Name = "invoiceTax1ID")]
        public int InvoiceTax1ID { get; set; }

        [DataMember(Name = "invoiceTax2ID")]
        public int InvoiceTax2ID { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }


        [DataMember(Name = "createOn")]
        public DateTime CreateOn { get; set; }

        [DataMember(Name = "createBy")]
        public Guid CreateBy { get; set; }

        [DataMember(Name = "lastModifedOn")]
        public DateTime? LastModifedOn { get; set; }

        [DataMember(Name = "lastModifedBy")]
        public Guid? LastModifedBy { get; set; }



        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }
    }
}
