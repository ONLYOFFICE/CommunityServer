/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Aggregator.Common.Utils;

namespace ASC.Mail.Aggregator.Dal
{
    public static class DtoConverters
    {
        private enum ContactInfoTableColumnsOrder
        {
            Id = 0,
            Tenant,
            UserId,
            ContactId,
            Data,
            Type,
            IsPrimary,
            LastModified
        }

        public const int CONTACT_INFO_COLUMNS_COUNT = 7;

        public static ContactInfoDto ToMailContactInfoDto(this object[] dbRecord)
        {
            if (dbRecord.Length != CONTACT_INFO_COLUMNS_COUNT)
                throw new InvalidCastException("Can't convert to MailContactInfoDto. Invalid columns count.");

            return new ContactInfoDto(
                Convert.ToInt32(dbRecord[(int)ContactInfoTableColumnsOrder.Id]),
                Convert.ToInt32(dbRecord[(int)ContactInfoTableColumnsOrder.Tenant]),
                Convert.ToString(dbRecord[(int)ContactInfoTableColumnsOrder.UserId]),
                Convert.ToInt32(dbRecord[(int)ContactInfoTableColumnsOrder.ContactId]),
                Convert.ToString(dbRecord[(int)ContactInfoTableColumnsOrder.Data]),
                Convert.ToInt32(dbRecord[(int)ContactInfoTableColumnsOrder.Type]),
                Convert.ToBoolean(dbRecord[(int)ContactInfoTableColumnsOrder.IsPrimary])
           );
        }

        private enum ContactsTableColumnsOrder
        {
            Id = 0,
            UserId,
            Tenant,
            Name,
            Description,
            Type,
            HasPhoto,
            LastModified
        }

        public const int CONTACTS_COLUMNS_COUNT = 7;

        public static List<ContactCardDto> ToContactCardDto(this List<object[]> objectList)
        {
            var contactCards = new List<ContactCardDto>();

            foreach (var r in objectList)
            {
                if (r.Length != CONTACTS_COLUMNS_COUNT + CONTACT_INFO_COLUMNS_COUNT)
                    throw new InvalidCastException("Can't convert to ContactCardDto. Invalid columns count.");

                var id = Convert.ToInt32(r[(int)ContactsTableColumnsOrder.Id]);

                var card  = contactCards.FirstOrDefault(c => c.id == id);
                if (card != null)
                {
                    card.contacts.Add(
                        r.SubArray(CONTACTS_COLUMNS_COUNT, r.Length - CONTACTS_COLUMNS_COUNT).ToMailContactInfoDto());
                }
                else
                {
                    var tempCard = new ContactCardDto(
                        id,
                        Convert.ToString(r[(int)ContactsTableColumnsOrder.UserId]),
                        Convert.ToInt32(r[(int)ContactsTableColumnsOrder.Tenant]),
                        Convert.ToString(r[(int)ContactsTableColumnsOrder.Name]),
                        new List<ContactInfoDto>(),
                        Convert.ToString(r[(int)ContactsTableColumnsOrder.Description]),
                        Convert.ToInt32(r[(int)ContactsTableColumnsOrder.Type]),
                        Convert.ToBoolean(r[(int)ContactsTableColumnsOrder.HasPhoto])
                        );

                    tempCard.contacts.Add(r.SubArray(CONTACTS_COLUMNS_COUNT, r.Length - CONTACTS_COLUMNS_COUNT).ToMailContactInfoDto());

                    contactCards.Add(tempCard);
                }
            }

            return contactCards;
        }
    }
}
