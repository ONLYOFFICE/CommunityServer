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

        [DataMember(IsRequired = false, Name = "id")]
        public int ContactId { get; set; }

        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Name = "description")]
        public string Description { get; set; }

        [DataMember(IsRequired = true, Name = "emails")]
        public EmailsList<ContactInfo> Emails { get; set; }

        [DataMember(IsRequired = true, Name = "phones")]
        public PhoneNumgersList<ContactInfo> PhoneNumbers { get; set; }

        [DataMember(IsRequired = false, Name = "type")]
        public int Type { get; set; }

        [DataMember(IsRequired = true, Name = "smallFotoUrl")]
        public string SmallFotoUrl { get; set; }

        [DataMember(IsRequired = true, Name = "mediumFotoUrl")]
        public string MediumFotoUrl { get; set; }
    }
}
