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