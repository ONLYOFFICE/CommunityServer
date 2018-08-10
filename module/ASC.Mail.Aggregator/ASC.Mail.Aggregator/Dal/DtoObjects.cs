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

namespace ASC.Mail.Aggregator.Dal
{
    public class ContactInfoDto
    {
        public readonly int id;
        public readonly int tenant;
        public readonly string userId;
        public readonly int contactCardId;
        public readonly string data;
        public readonly int type;
        public readonly bool isPrimary;

        internal ContactInfoDto(int id, int tenant, string userId, int contactCardId, string data, int type, bool isPrimary)
        {
            this.id = id;
            this.tenant = tenant;
            this.userId = userId;
            this.contactCardId = contactCardId;
            this.data = data;
            this.type = type;
            this.isPrimary = isPrimary;
        }
    }

    public class ContactCardDto
    {
        public readonly int id;
        public readonly string userId;
        public readonly int tenant;
        public readonly string name;
        public readonly List<ContactInfoDto> contacts;
        public readonly string description;
        public readonly int type;
        public readonly bool hasPhoto;

        internal ContactCardDto(int id, string userId, int tenant, string name,
            List<ContactInfoDto> contacts, string description, int type, bool hasPhoto)
        {
            this.id = id;
            this.userId = userId;
            this.tenant = tenant;
            this.name = name;
            this.contacts = contacts;
            this.description = description;
            this.type = type;
            this.hasPhoto = hasPhoto;
        }
    }
}
