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
    public class ContactInfoDao : BaseDao, IContactInfoDao
    {
        protected static ITable table = new MailTableFactory().Create<ContactInfoTable>();

        protected string CurrentUserId { get; private set; }

        public ContactInfoDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public int SaveContactInfo(ContactInfo contactInfo)
        {
            var query = new SqlInsert(ContactInfoTable.TABLE_NAME, true)
                .InColumnValue(ContactInfoTable.Columns.Id, contactInfo.Id)
                .InColumnValue(ContactInfoTable.Columns.Tenant, contactInfo.Tenant)
                .InColumnValue(ContactInfoTable.Columns.User, contactInfo.User)
                .InColumnValue(ContactInfoTable.Columns.ContactId, contactInfo.ContactId)
                .InColumnValue(ContactInfoTable.Columns.Data, contactInfo.Data)
                .InColumnValue(ContactInfoTable.Columns.Type, contactInfo.Type)
                .InColumnValue(ContactInfoTable.Columns.IsPrimary, contactInfo.IsPrimary);

            return Db.ExecuteScalar<int>(query);
        }

        public int RemoveContactInfo(int id)
        {
            var query = new SqlDelete(ContactInfoTable.TABLE_NAME)
                .Where(ContactInfoTable.Columns.Id, id)
                .Where(ContactInfoTable.Columns.Tenant, Tenant)
                .Where(ContactInfoTable.Columns.User, CurrentUserId);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int RemoveByContactIds(List<int> contactIds)
        {
            var deleteContact = new SqlDelete(ContactsTable.TABLE_NAME)
                .Where(Exp.In(ContactsTable.Columns.Id, contactIds))
                .Where(ContactInfoTable.Columns.Tenant, Tenant)
                .Where(ContactInfoTable.Columns.User, CurrentUserId);

            return Db.ExecuteNonQuery(deleteContact);
        }
    }
}