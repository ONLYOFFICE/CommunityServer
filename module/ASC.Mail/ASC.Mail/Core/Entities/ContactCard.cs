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


using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Entities
{
    public class ContactCard
    {
        public ContactCard(Contact contact, List<ContactInfo> contactItems)
        {
            ContactInfo = contact;
            ContactItems = contactItems;
        }

        public ContactCard(int id, int tenant, string user, string name, string description, ContactType type,
            IReadOnlyList<string> emails, IReadOnlyList<string> phoneNumbers = null)
        {
            ContactInfo = new Contact
            {
                Id = id,
                User = user,
                Tenant = tenant,
                ContactName = name,
                Address = emails[0],
                Description = description,
                Type = type,
                HasPhoto = false
            };

            ContactItems = new List<ContactInfo>();

            if (emails != null && emails.Any())
            {
                for (var i = 0; i < emails.Count; i++)
                {
                    var isPrimary = i == 0;

                    ContactItems
                        .Add(new ContactInfo
                        {
                            Id = 0,
                            ContactId = id,
                            Tenant = tenant,
                            User = user,
                            Type = (int) ContactInfoType.Email,
                            Data = emails[i],
                            IsPrimary = isPrimary
                        });
                }
            }

            if (phoneNumbers != null && phoneNumbers.Any())
            {
                for (var i = 0; i < phoneNumbers.Count; i++)
                {
                    var isPrimary = i == 0;

                    ContactItems
                        .Add(new ContactInfo
                        {
                            Id = 0,
                            ContactId = id,
                            Tenant = tenant,
                            User = user,
                            Type = (int) ContactInfoType.Phone,
                            Data = phoneNumbers[i],
                            IsPrimary = isPrimary
                        });
                }
            }
        }

        public Contact ContactInfo { get; set; }
        public List<ContactInfo> ContactItems { get; set; }
    }
}
