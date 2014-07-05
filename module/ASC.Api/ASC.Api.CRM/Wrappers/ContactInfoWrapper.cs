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
using System.Runtime.Serialization;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using Newtonsoft.Json.Linq;

namespace ASC.Api.CRM.Wrappers
{
    /// <summary>
    ///   Address
    /// </summary>
    [DataContract(Name = "address", Namespace = "")]
    public class Address
    {
        public Address()
        {
        }

        public Address(ContactInfo contactInfo)
        {
            if (contactInfo.InfoType != ContactInfoType.Address) throw new ArgumentException();

            City = JObject.Parse(contactInfo.Data)["city"].Value<String>();
            Country = JObject.Parse(contactInfo.Data)["country"].Value<String>();
            State = JObject.Parse(contactInfo.Data)["state"].Value<String>();
            Street = JObject.Parse(contactInfo.Data)["street"].Value<String>();
            Zip = JObject.Parse(contactInfo.Data)["zip"].Value<String>();
            Category = contactInfo.Category;
            CategoryName = contactInfo.CategoryToString();
            IsPrimary = contactInfo.IsPrimary;
        }

        [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public String Street { get; set; }

        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public String City { get; set; }

        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public String State { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public String Zip { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public String Country { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public int Category { get; set; }

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public String CategoryName { get; set; }

        [DataMember(Order = 8, IsRequired = false, EmitDefaultValue = false)]
        public Boolean IsPrimary { get; set; }


        public static Address GetSample()
        {
            return new Address
                {
                    Country = "Latvia",
                    Zip = "LV-1021",
                    Street = "Lubanas st. 125a-25",
                    State = "",
                    City = "Riga",
                    IsPrimary = true,
                    Category = (int)ContactInfoBaseCategory.Work,
                    CategoryName = ((AddressCategory)ContactInfoBaseCategory.Work).ToLocalizedString()
                };
        }
    }

    /// <summary>
    ///   Contact information
    /// </summary>
    [DataContract(Name = "commonDataItem", Namespace = "")]
    public class ContactInfoWrapper : ObjectWrapperBase
    {
        public ContactInfoWrapper()
            : base(0)
        {
        }

        public ContactInfoWrapper(int id)
            : base(id)
        {
        }

        public ContactInfoWrapper(ContactInfo contactInfo)
            : base(contactInfo.ID)
        {
            InfoType = contactInfo.InfoType;
            Category = contactInfo.Category;
            CategoryName = contactInfo.CategoryToString();
            Data = contactInfo.Data;
            IsPrimary = contactInfo.IsPrimary;
            ID = contactInfo.ID;
        }


        [DataMember(Order = 1)]
        public ContactInfoType InfoType { get; set; }

        [DataMember(Order = 2)]
        public int Category { get; set; }

        [DataMember(Order = 3)]
        public String Data { get; set; }

        [DataMember(Order = 4)]
        public String CategoryName { get; set; }

        [DataMember(Order = 5)]
        public bool IsPrimary { get; set; }

        public static ContactInfoWrapper GetSample()
        {
            return new ContactInfoWrapper(0)
                {
                    IsPrimary = true,
                    Category = (int)ContactInfoBaseCategory.Home,
                    CategoryName = ContactInfoBaseCategory.Home.ToLocalizedString(),
                    Data = "support@onlyoffice.com",
                    InfoType = ContactInfoType.Email
                };
        }
    }
}