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

namespace ASC.Core
{
    [DataContract]
    public class Partner
    {
        [DataMember(Name = "Id")]
        public string Id { get; set; }

        [DataMember(Name = "Email")]
        public string Email { get; set; }

        [DataMember(Name = "FirstName")]
        public string FirstName { get; set; }

        [DataMember(Name = "LastName")]
        public string LastName { get; set; }

        [DataMember(Name = "Url")]
        public string Url { get; set; }

        [DataMember(Name = "Phone")]
        public string Phone { get; set; }

        [DataMember(Name = "Language")]
        public string Language { get; set; }

        [DataMember(Name = "CompanyName")]
        public string CompanyName { get; set; }

        [DataMember(Name = "Country")]
        public string Country { get; set; }

        [DataMember(Name = "CountryCode")]
        public string CountryCode { get; set; }

        [DataMember(Name = "CountryHasVat")]
        public bool CountryHasVat { get; set; }

        [DataMember(Name = "Address")]
        public string Address { get; set; }

        [DataMember(Name = "VatId")]
        public string VatId { get; set; }

        [DataMember(Name = "CreationDate")]
        public DateTime CreationDate { get; set; }

        [DataMember(Name = "Status")]
        public PartnerStatus Status { get; set; }

        [DataMember(Name = "Comment")]
        public string Comment { get; set; }

        [DataMember(Name = "Portal")]
        public string Portal { get; set; }

        [DataMember(Name = "PortalConfirmed")]
        public bool PortalConfirmed { get; set; }

        [DataMember(Name = "IsAdmin")]
        public bool IsAdmin { get { return PartnerType == PartnerType.Administrator; } }

        [DataMember(Name = "Limit")]
        public decimal Limit { get; set; }

        [DataMember(Name = "Discount")]
        public int Discount { get; set; }

        [DataMember(Name = "PayPalAccount")]
        public string PayPalAccount { get; set; }

        [DataMember(Name = "Deposit")]
        public decimal Deposit { get; set; }

        [DataMember(Name = "Removed")]
        public bool Removed { get; set; }

        [DataMember(Name = "Currency")]
        public string Currency { get; set; }

        [DataMember(Name = "LogoUrl")]
        public string LogoUrl { get; set; }

        [DataMember(Name = "DisplayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "DisplayType")]
        public PartnerDisplayType DisplayType { get; set; }

        [DataMember(Name = "SupportPhone")]
        public string SupportPhone { get; set; }

        [DataMember(Name = "SupportEmail")]
        public string SupportEmail { get; set; }

        [DataMember(Name = "SalesEmail")]
        public string SalesEmail { get; set; }

        [DataMember(Name = "TermsUrl")]
        public string TermsUrl { get; set; }

        [DataMember(Name = "Theme")]
        public string Theme { get; set; }

        [DataMember(Name = "RuAccount")]
        public string RuAccount { get; set; }

        [DataMember(Name = "RuBank")]
        public string RuBank { get; set; }

        [DataMember(Name = "RuKs")]
        public string RuKs { get; set; }

        [DataMember(Name = "RuKpp")]
        public string RuKpp { get; set; }

        [DataMember(Name = "RuBik")]
        public string RuBik { get; set; }

        [DataMember(Name = "RuInn")]
        public string RuInn { get; set; }

        [DataMember(Name = "PartnerType")]
        public PartnerType PartnerType { get; set; }

        [DataMember(Name = "PaymentMethod")]
        public PartnerPaymentMethod PaymentMethod { get; set; }

        [DataMember(Name = "PaymentUrl")]
        public string PaymentUrl { get; set; }

        [DataMember(Name = "AvailableCredit")]
        public decimal AvailableCredit { get; set; }

        [DataMember(Name = "CustomEmailSignature")]
        public bool CustomEmailSignature { get; set; }

        [DataMember(Name = "AuthorizedKey")]
        public string AuthorizedKey { get; set; }

        public override bool Equals(object obj)
        {
            var p = obj as Partner;
            return p != null && p.Id == Id;
        }

        public override int GetHashCode()
        {
            return (Id ?? string.Empty).GetHashCode();
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}