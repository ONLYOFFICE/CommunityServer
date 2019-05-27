/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
