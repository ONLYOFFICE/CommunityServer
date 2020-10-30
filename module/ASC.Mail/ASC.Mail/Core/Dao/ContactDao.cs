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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class ContactDao : BaseDao, IContactDao
    {
        protected static ITable table = new MailTableFactory().Create<ContactsTable>();

        protected string CurrentUserId { get; private set; }

        public ContactDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public int SaveContact(Contact contact)
        {
            var queryContact = new SqlInsert(ContactsTable.TABLE_NAME, true)
                .InColumnValue(ContactsTable.Columns.Id, contact.Id)
                .InColumnValue(ContactsTable.Columns.User, contact.User)
                .InColumnValue(ContactsTable.Columns.Tenant, contact.Tenant)
                .InColumnValue(ContactsTable.Columns.ContactName, contact.ContactName)
                .InColumnValue(ContactsTable.Columns.Address, contact.Address)
                .InColumnValue(ContactsTable.Columns.Description, contact.Description)
                .InColumnValue(ContactsTable.Columns.Type, contact.Type)
                .InColumnValue(ContactsTable.Columns.HasPhoto, contact.HasPhoto)
                .Identity(0, 0, true);

            return Db.ExecuteScalar<int>(queryContact);
        }

        public int RemoveContacts(List<int> ids)
        {
            var query = new SqlDelete(ContactInfoTable.TABLE_NAME)
                .Where(Exp.In(ContactInfoTable.Columns.ContactId, ids))
                .Where(ContactInfoTable.Columns.Tenant, Tenant)
                .Where(ContactInfoTable.Columns.User, CurrentUserId);

            return Db.ExecuteNonQuery(query);
        }
    }
}