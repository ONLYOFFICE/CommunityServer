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