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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Contact")]
    public class MailContactData
    {
        [CollectionDataContract(Namespace = "", ItemName = "email")]
        public class EmailsList<TItem> : List<TItem>
        {
            public EmailsList()
            {
            }

            public EmailsList(IEnumerable<TItem> items)
                : base(items)
            {
            }
        }

        [CollectionDataContract(Namespace = "", ItemName = "phone")]
        public class PhoneNumgersList<TItem> : List<TItem>
        {
            public PhoneNumgersList()
            {
            }

            public PhoneNumgersList(IEnumerable<TItem> items)
                : base(items)
            {
            }
        }

        ///<example name="id" type="int">1234</example>
        [DataMember(IsRequired = false, Name = "id")]
        public int ContactId { get; set; }

        ///<example name="name">name</example>
        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        ///<example name="description">description</example>
        [DataMember(IsRequired = true, Name = "description")]
        public string Description { get; set; }

        ///<type name="emails">ASC.Mail.Data.Contracts.ContactInfo, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = true, Name = "emails")]
        public EmailsList<ContactInfo> Emails { get; set; }

        ///<type name="phones">ASC.Mail.Data.Contracts.ContactInfo, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = true, Name = "phones")]
        public PhoneNumgersList<ContactInfo> PhoneNumbers { get; set; }

        ///<example name="type" type="int">1234</example>
        [DataMember(IsRequired = false, Name = "type")]
        public int Type { get; set; }

        ///<example name="smallFotoUrl">smallFotoUrl</example>
        [DataMember(IsRequired = true, Name = "smallFotoUrl")]
        public string SmallFotoUrl { get; set; }

        ///<example name="mediumFotoUrl">mediumFotoUrl</example>
        [DataMember(IsRequired = true, Name = "mediumFotoUrl")]
        public string MediumFotoUrl { get; set; }
    }
}
