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
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.FullTextIndex;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Mail.Aggregator.Filter;

namespace ASC.Mail.Aggregator.Extension
{
    internal static class SqlQueryExtensions
    {
        public static SqlQuery ApplyFilter(this SqlQuery query, MailFilter filter, string alias)
        {
            return ApplyFilter(query, filter, alias, false);
        }

        private static SqlQuery ApplyFilter(this SqlQuery query, MailFilter filter, string alias, bool skipFolder)
        {
            var conditions = GetMailFilterConditions(filter, skipFolder, alias);

            if (conditions == null) // skip query
                return null;

            if (conditions != Exp.Empty)
                query.Where(conditions);

            return query;
        }

        public static Exp GetMailFilterConditions(MailFilter filter, bool skipFolder, string alias = "")
        {
            var conditions = Exp.Empty;

            if (!skipFolder)
                conditions = Exp.Eq(MailTable.Columns.Folder.Prefix(alias), filter.PrimaryFolder);

            if (filter.Unread.HasValue)
            {
                conditions &= Exp.Eq(MailTable.Columns.Unread.Prefix(alias), filter.Unread);
            }

            if (filter.Attachments.HasValue)
                conditions &= Exp.Gt(MailTable.Columns.AttachCount.Prefix(alias), 0);

            if (filter.PeriodFrom.HasValue && filter.PeriodTo.HasValue)
            {
                var from = new DateTime(1970, 1, 1) + new TimeSpan(filter.PeriodFrom.Value*10000);

                var to = new DateTime(1970, 1, 1) + new TimeSpan(filter.PeriodTo.Value*10000) +
                         new TimeSpan(1, 0, 0, 0, 0); // 1 day was added to make the "To" date limit inclusive

                conditions &= Exp.Between(MailTable.Columns.DateSent.Prefix(alias), from, to);
            }

            if (filter.Important.HasValue)
            {
                conditions &= Exp.Eq(MailTable.Columns.Importance.Prefix(alias), true);
            }

            if (filter.WithCalendar.HasValue)
            {
                conditions &= !Exp.Eq(MailTable.Columns.CalendarUid.Prefix(alias), null);
            }

            if (!string.IsNullOrEmpty(filter.FindAddress) && !FullTextSearch.SupportModule(FullTextSearch.MailModule))
            {
                if (filter.PrimaryFolder == MailFolder.Ids.sent || filter.PrimaryFolder == MailFolder.Ids.drafts)
                    conditions &= Exp.Like(MailTable.Columns.To.Prefix(alias), filter.FindAddress, SqlLike.AnyWhere);
                else
                    conditions &= Exp.Like(MailTable.Columns.From.Prefix(alias), filter.FindAddress,
                        SqlLike.AnyWhere);
            }

            if (filter.MailboxId.HasValue)
            {
                conditions &= Exp.Eq(MailTable.Columns.MailboxId.Prefix(alias), filter.MailboxId.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchText) && !FullTextSearch.SupportModule(FullTextSearch.MailModule))
            {
                conditions &=
                    Exp.Or(Exp.Like(MailTable.Columns.From.Prefix(alias), filter.SearchText, SqlLike.AnyWhere),
                        Exp.Or(
                            Exp.Like(MailTable.Columns.To.Prefix(alias), filter.SearchText, SqlLike.AnyWhere),
                            Exp.Or(
                                Exp.Like(MailTable.Columns.Cc.Prefix(alias), filter.SearchText,
                                    SqlLike.AnyWhere),
                                Exp.Or(
                                    Exp.Like(MailTable.Columns.Bcc.Prefix(alias), filter.SearchText,
                                        SqlLike.AnyWhere),
                                    Exp.Like(MailTable.Columns.Subject.Prefix(alias), filter.SearchText,
                                        SqlLike.AnyWhere)))));
            }

            return conditions;
        }

        public static SqlQuery ApplySorting(this SqlQuery query, MailFilter filter)
        {
            var sortField = MailTable.Columns.DateSent;

            switch (filter.Sort)
            {
                case "subject":
                    sortField = MailTable.Columns.Subject;
                    break;
                case "sender":
                    sortField = MailTable.Columns.From;
                    break;
            }

            var sortOrder = filter.SortOrder == "ascending";

            query.OrderBy(sortField, sortOrder);

            return query;
        }
    }
}
