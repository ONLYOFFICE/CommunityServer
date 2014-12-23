/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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