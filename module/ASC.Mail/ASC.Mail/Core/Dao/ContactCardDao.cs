/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Expressions.Contact;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class ContactCardDao : IContactCardDao
    {
        public IDbManager Db { get; private set; }
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ContactCardDao(IDbManager dbManager, int tenant, string user)
        {
            Db = dbManager;
            Tenant = tenant;
            User = user;
        }

        private const string MAIL_CONTACTS = "mc";
        private const string CONTACT_INFO = "ci";

        public ContactCard GetContactCard(int id)
        {
            var query = new SqlQuery(ContactsTable.TABLE_NAME.Alias(MAIL_CONTACTS))
                .Select(ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.User.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Tenant.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.ContactName.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Address.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Description.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Type.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.HasPhoto.Prefix(MAIL_CONTACTS))
                .Where(ContactsTable.Columns.Tenant.Prefix(MAIL_CONTACTS), Tenant)
                .Where(ContactsTable.Columns.User.Prefix(MAIL_CONTACTS), User)
                .Where(ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS), id);

            var contacts = Db.ExecuteList(query)
                .ConvertAll(ToContact);

            query = new SqlQuery(ContactInfoTable.TABLE_NAME.Alias(CONTACT_INFO))
                .Select(ContactInfoTable.Columns.Id.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.Tenant.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.User.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.ContactId.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.Data.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.Type.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.IsPrimary.Prefix(CONTACT_INFO))
                .Where(ContactInfoTable.Columns.ContactId.Prefix(CONTACT_INFO), id);

            var contactInfos = Db.ExecuteList(query)
                .ConvertAll(ToContactInfo);

            return ToContactCardList(contacts, contactInfos).SingleOrDefault();
        }

        public List<ContactCard> GetContactCards(IContactsExp exp)
        {
            var query = new SqlQuery(ContactsTable.TABLE_NAME.Alias(MAIL_CONTACTS))
                .Select(ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.User.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Tenant.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.ContactName.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Address.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Description.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.Type.Prefix(MAIL_CONTACTS),
                    ContactsTable.Columns.HasPhoto.Prefix(MAIL_CONTACTS))
                .Where(exp.GetExpression());

            if (exp.OrderAsc.HasValue)
            {
                query.OrderBy(ContactsTable.Columns.ContactName.Prefix(MAIL_CONTACTS), exp.OrderAsc.HasValue);
            }

            if (exp.StartIndex.HasValue)
            {
                query.SetFirstResult(exp.StartIndex.Value);
            }

            if (exp.Limit.HasValue)
            {
                query.SetMaxResults(exp.Limit.Value);
            }

            var contacts = Db.ExecuteList(query)
                .ConvertAll(ToContact);

            var ids = contacts.Select(c => c.Id).ToList();

            query = new SqlQuery(ContactInfoTable.TABLE_NAME.Alias(CONTACT_INFO))
                .Select(ContactInfoTable.Columns.Id.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.Tenant.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.User.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.ContactId.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.Data.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.Type.Prefix(CONTACT_INFO),
                    ContactInfoTable.Columns.IsPrimary.Prefix(CONTACT_INFO))
                .Where(Exp.In(ContactInfoTable.Columns.ContactId.Prefix(CONTACT_INFO), ids));

            var contactInfos = Db.ExecuteList(query)
                .ConvertAll(ToContactInfo);

            return ToContactCardList(contacts, contactInfos);
        }

        public int GetContactCardsCount(IContactsExp exp)
        {
            var query = new SqlQuery(ContactsTable.TABLE_NAME.Alias(MAIL_CONTACTS))
                .SelectCount("distinct " + ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS))
                .LeftOuterJoin(ContactInfoTable.TABLE_NAME.Alias(CONTACT_INFO),
                    Exp.EqColumns(ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS),
                        ContactInfoTable.Columns.ContactId.Prefix(CONTACT_INFO)))
                .Where(exp.GetExpression());

            var result = Db.ExecuteScalar<int>(query);

            return result;
        }

        protected List<ContactCard> ToContactCardList(List<Contact> contacts, List<ContactInfo> contactInfos)
        {
            return
                contacts.Select(
                    contact => new ContactCard(contact, contactInfos.Where(ci => ci.ContactId == contact.Id).ToList()))
                    .ToList();
        }

        protected Contact ToContact(object[] r)
        {
            var c = new Contact
            {
                Id = Convert.ToInt32(r[0]),
                User = Convert.ToString(r[1]),
                Tenant = Convert.ToInt32(r[2]),
                ContactName = Convert.ToString(r[3]),
                Address = Convert.ToString(r[4]),
                Description = Convert.ToString(r[5]),
                Type = (ContactType) Convert.ToInt32(r[6]),
                HasPhoto = Convert.ToBoolean(r[7])
            };

            return c;
        }

        protected ContactInfo ToContactInfo(object[] r)
        {
            var c = new ContactInfo
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                ContactId = Convert.ToInt32(r[3]),
                Data = Convert.ToString(r[4]),
                Type = Convert.ToInt32(r[5]),
                IsPrimary = Convert.ToBoolean(r[6])
            };

            return c;
        }
    }
}