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


using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Contact
{
    public class SimpleFilterContactsExp : IContactsExp
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public bool? OrderAsc { get; private set; }
        public int? StartIndex { get; private set; }
        public int? Limit { get; private set; }

        private const string MAIL_CONTACTS = "mc";

        public SimpleFilterContactsExp(int tenant, string user, bool? orderAsc = null,
            int? startIndex = null, int? limit = null)
        {
            Tenant = tenant;
            User = user;

            OrderAsc = orderAsc;
            StartIndex = startIndex;
            Limit = limit;
        }

        public virtual Exp GetExpression()
        {
            return Exp.Eq(ContactsTable.Columns.Tenant.Prefix(MAIL_CONTACTS), Tenant)
                   & Exp.Eq(ContactsTable.Columns.User.Prefix(MAIL_CONTACTS), User);
        }
    }
}