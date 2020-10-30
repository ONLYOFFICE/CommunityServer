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
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.ElasticSearch;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Data.Search;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Contact
{
    public class FullFilterContactsExp : SimpleFilterContactsExp
    {
        public ContactInfoType? InfoType { get; private set; }
        public bool? IsPrimary { get; private set; }
        public string SearchTerm { get; private set; }
        public int? Type { get; set; }

        private const string MAIL_CONTACTS = "mc";
        private const string CONTACT_INFO = "ci";

        public FullFilterContactsExp(int tenant, string user, string searchTerm = null, int? type = null,
            ContactInfoType? infoType = null, bool? isPrimary = null, bool? orderAsc = true, int? startIndex = null,
            int? limit = null)
            : base(tenant, user, orderAsc, startIndex, limit)
        {
            InfoType = infoType;
            IsPrimary = isPrimary;
            SearchTerm = searchTerm;
            Type = type;
        }

        public override Exp GetExpression()
        {
            var exp = base.GetExpression();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var bySearch = Exp.Empty;

                if (FactoryIndexer<MailContactWrapper>.Support && FactoryIndexer.CheckState(false))
                {
                    var selector = new Selector<MailContactWrapper>()
                        .MatchAll(SearchTerm)
                        .Where(s => s.User, new Guid(User));

                    if (InfoType.HasValue) 
                    {
                        selector.InAll(s => s.InfoList.Select(i => i.InfoType), new[] { (int)InfoType.Value });
                    }

                    if (IsPrimary.HasValue)
                    {
                        selector.InAll(s => s.InfoList.Select(i => i.IsPrimary), new[] { IsPrimary.Value });
                    }

                    List<int> ids;
                    if (FactoryIndexer<MailContactWrapper>.TrySelectIds(s => selector, out ids))
                    {
                        bySearch = Exp.In(ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS), ids); // if ids.length == 0 then IN (1=0) - equals to no results
                    }
                }

                if (bySearch == Exp.Empty)
                {
                    var contactInfoQuery = new SqlQuery(ContactInfoTable.TABLE_NAME.Alias(CONTACT_INFO))
                        .Distinct()
                        .Select(ContactInfoTable.Columns.ContactId.Prefix(CONTACT_INFO))
                        .Where(ContactInfoTable.Columns.Tenant.Prefix(CONTACT_INFO), Tenant)
                        .Where(ContactInfoTable.Columns.User.Prefix(CONTACT_INFO), User)
                        .Where(Exp.Like(ContactInfoTable.Columns.Data.Prefix(CONTACT_INFO), SearchTerm, SqlLike.AnyWhere));

                    if (IsPrimary.HasValue)
                    {
                        contactInfoQuery.Where(Exp.Eq(ContactInfoTable.Columns.IsPrimary.Prefix(CONTACT_INFO), IsPrimary.Value));
                    }

                    if (InfoType.HasValue)
                    {
                        contactInfoQuery.Where(Exp.Eq(ContactInfoTable.Columns.Type.Prefix(CONTACT_INFO), (int)InfoType.Value));
                    }

                    bySearch =
                        Exp.Or(
                            Exp.Like(ContactsTable.Columns.Description.Prefix(MAIL_CONTACTS), SearchTerm,
                                SqlLike.AnyWhere),
                            Exp.Or(
                                Exp.Like(ContactsTable.Columns.ContactName.Prefix(MAIL_CONTACTS), SearchTerm,
                                    SqlLike.AnyWhere),
                                Exp.In(ContactsTable.Columns.Id.Prefix(MAIL_CONTACTS), contactInfoQuery)));
                }

                exp &= bySearch;
            }

            if (Type.HasValue)
            {
                exp &= Exp.Eq(ContactsTable.Columns.Type.Prefix(MAIL_CONTACTS), Type.Value);
            }

            return exp;
        }
    }
}