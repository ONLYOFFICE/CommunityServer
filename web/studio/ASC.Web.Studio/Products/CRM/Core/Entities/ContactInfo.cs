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
using ASC.Web.CRM.Classes;

namespace ASC.CRM.Core
{
    [DataContract]
    public class ContactInfo : DomainObject
    {
        [DataMember(Name = "contactID")]
        public int ContactID { get; set; }

        [DataMember(Name = "infoType")]
        public ContactInfoType InfoType { get; set; }

        [DataMember(Name = "category")]
        public int Category { get; set; }

        [DataMember(Name = "data")]
        public String Data { get; set; }

        [DataMember(Name = "isPrimary")]
        public bool IsPrimary { get; set; }


        public static int GetDefaultCategory(ContactInfoType infoTypeEnum)
        {
            switch (infoTypeEnum)
            {
                case ContactInfoType.Phone:
                    return (int)PhoneCategory.Work;
                case ContactInfoType.Address:
                    return (int)AddressCategory.Work;
                default:
                    return (int)ContactInfoBaseCategory.Work;
            }
        }

        public String CategoryToString()
        {
            switch (InfoType)
            {
                case ContactInfoType.Phone:
                    return ((PhoneCategory)Category).ToLocalizedString();
                case ContactInfoType.Address:
                    return ((AddressCategory)Category).ToLocalizedString();
                default:
                    return ((ContactInfoBaseCategory)Category).ToLocalizedString();
            }
        }

        public static Type GetCategory(ContactInfoType infoType)
        {
            switch (infoType)
            {
                case ContactInfoType.Phone:
                    return typeof(PhoneCategory);
                case ContactInfoType.Address:
                    return typeof(AddressCategory);
                default:
                    return typeof(ContactInfoBaseCategory);
            }
        }
    }
}