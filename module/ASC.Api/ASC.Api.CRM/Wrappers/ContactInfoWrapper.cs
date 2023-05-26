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

using ASC.Common.Logging;
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
            try
            {
                var jResult = JObject.Parse(contactInfo.Data);
                City = jResult["city"].Value<String>();
                Country = jResult["country"].Value<String>();
                State = jResult["state"].Value<String>();
                Street = jResult["street"].Value<String>();
                Zip = jResult["zip"].Value<String>();
                Category = contactInfo.Category;
                CategoryName = contactInfo.CategoryToString();
                IsPrimary = contactInfo.IsPrimary;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Api").Error(ex);
            }
        }

        public static bool TryParse(ContactInfo contactInfo, out Address res)
        {
            if (contactInfo.InfoType != ContactInfoType.Address)
            {
                res = null;
                return false;
            }

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<Address>(contactInfo.Data);
                res.Category = contactInfo.Category;
                res.CategoryName = contactInfo.CategoryToString();
                res.IsPrimary = contactInfo.IsPrimary;
                return true;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Api").Error(ex);
                res = null;
                return false;
            }
        }

        ///<example>Lubanas st. 125a-25</example>
        ///<order>1</order>
        [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public String Street { get; set; }

        ///<example></example>
        ///<order>2</order>
        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public String City { get; set; }

        ///<example></example>
        ///<order>3</order>
        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public String State { get; set; }

        ///<example>LV-1021</example>
        ///<order>4</order>
        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public String Zip { get; set; }

        ///<example>Latvia</example>
        ///<order>5</order>
        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public String Country { get; set; }

        ///<example type="int">0</example>
        ///<order>6</order>
        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public int Category { get; set; }

        ///<example>Home</example>
        ///<order>7</order>
        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public String CategoryName { get; set; }

        ///<example>true</example>
        ///<order>8</order>
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
    ///<inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
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

        ///<example type="int">1</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public ContactInfoType InfoType { get; set; }

        ///<example type="int">0</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public int Category { get; set; }

        ///<example>support@onlyoffice.com</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public String Data { get; set; }

        ///<example>Home</example>
        ///<order>4</order>
        [DataMember(Order = 4)]
        public String CategoryName { get; set; }

        ///<example>true</example>
        ///<order>5</order>
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