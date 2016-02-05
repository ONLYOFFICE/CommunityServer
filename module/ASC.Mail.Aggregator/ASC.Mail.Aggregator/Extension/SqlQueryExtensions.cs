/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.FullTextIndex;
using ASC.FullTextIndex.Service;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Filter;

namespace ASC.Mail.Aggregator.Extension
{
    internal static class SqlQueryExtensions
    {
        public static SqlQuery ApplyFilter(this SqlQuery query, MailFilter filter, string alias)
        {
            return ApplyFilter(query, filter, alias, false);
        }

        public static SqlUpdate ApplyFilter(this SqlUpdate query, MailFilter filter, string alias)
        {
            return ApplyFilter(query, filter, alias, false);
        }

        public static SqlDelete ApplyFilter(this SqlDelete query, MailFilter filter, string alias)
        {
            return ApplyFilter(query, filter, alias, false);
        }

        private static SqlQuery ApplyFilter(this SqlQuery query, MailFilter filter, string alias, bool skipFolder)
        {
            var conditions = GetMailFilterConditions(filter, skipFolder, alias);

            if (conditions != null)
                query.Where(conditions);

            return query;
        }

        private static SqlUpdate ApplyFilter(this SqlUpdate query, MailFilter filter, string alias, bool skipFolder)
        {
            var conditions = GetMailFilterConditions(filter, skipFolder, alias);

            if (conditions != null)
                query.Where(conditions);

            return query;
        }

        private static SqlDelete ApplyFilter(this SqlDelete query, MailFilter filter, string alias, bool skipFolder)
        {
            var conditions = GetMailFilterConditions(filter, skipFolder, alias);

            if (conditions != null)
                query.Where(conditions);

            return query;
        }

        public static Exp GetMailFilterConditions(MailFilter filter, bool skipFolder, string alias)
        {
            Exp conditions = null;

            if (!string.IsNullOrEmpty(alias))
                alias += ".";

            if (!skipFolder)
                conditions = Exp.Eq(alias + MailTable.Columns.folder, filter.PrimaryFolder);

            if (filter.Unread.HasValue)
            {
                conditions &= Exp.Eq(alias + MailTable.Columns.unread, filter.Unread);
            }

            if (filter.Attachments)
                conditions &= Exp.Gt(alias + MailTable.Columns.attach_count, 0);

            if (filter.PeriodFrom > 0)
            {
                var from = new DateTime(1970, 1, 1) + new TimeSpan(filter.PeriodFrom * 10000);
                var to = new DateTime(1970, 1, 1) + new TimeSpan(filter.PeriodTo * 10000) +
                         new TimeSpan(1, 0, 0, 0, 0); // 1 day was added to make the "To" date limit inclusive
                conditions &= Exp.Between(alias + MailTable.Columns.date_sent, from, to);
            }

            if (filter.Important)
            {
                conditions &= Exp.Eq(alias + MailTable.Columns.importance, true);
            }

            if (!string.IsNullOrEmpty(filter.FindAddress))
            {
                if (FullTextSearch.SupportModule(FullTextSearch.MailModule))
                {
                    List<int> ids;
                    if (filter.PrimaryFolder == MailFolder.Ids.sent || filter.PrimaryFolder == MailFolder.Ids.drafts)
                        ids = FullTextSearch.Search(FullTextSearch.MailModule.Match(filter.FindAddress, MailTable.Columns.to));
                    else
                        ids = FullTextSearch.Search(FullTextSearch.MailModule.Match(filter.FindAddress, MailTable.Columns.from));
                    
                    conditions &= Exp.In(alias + MailTable.Columns.id, ids.Take(MailBoxManager.FULLTEXTSEARCH_IDS_COUNT).ToList());
                }
                else
                {
                    if (filter.PrimaryFolder == MailFolder.Ids.sent || filter.PrimaryFolder == MailFolder.Ids.drafts)
                        conditions &= Exp.Like(alias + MailTable.Columns.to, filter.FindAddress, SqlLike.AnyWhere);
                    else
                        conditions &= Exp.Like(alias + MailTable.Columns.from, filter.FindAddress, SqlLike.AnyWhere);
                }
            }

            if (filter.MailboxId.HasValue)
            {
                conditions &= Exp.Eq(alias + MailTable.Columns.id_mailbox, filter.MailboxId.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchFilter))
            {
                if (FullTextSearch.SupportModule(FullTextSearch.MailModule))
                {
                    var mailModule = FullTextSearch.MailModule.Match(filter.SearchFilter).OrderBy(MailTable.Columns.date_sent, filter.SortOrder == "ascending");

                    if (filter.PrimaryFolder != 1 && filter.PrimaryFolder != 2)
                    {
                        mailModule.AddAttribute("folder", filter.PrimaryFolder);
                    }
                    else
                    {
                        mailModule.AddAttribute("folder", new[] {1, 2});
                    }

                    var ids = FullTextSearch.Search(mailModule);

                    conditions &= Exp.In(alias + MailTable.Columns.id, ids.Take(MailBoxManager.FULLTEXTSEARCH_IDS_COUNT).ToList());
                }
                else
                {
                    conditions &= Exp.Or(Exp.Like(alias + MailTable.Columns.from, filter.SearchFilter, SqlLike.AnyWhere),
                                       Exp.Or(
                                           Exp.Like(alias + MailTable.Columns.to, filter.SearchFilter, SqlLike.AnyWhere),
                                           Exp.Or(
                                               Exp.Like(alias + MailTable.Columns.cc, filter.SearchFilter,
                                                        SqlLike.AnyWhere),
                                               Exp.Or(
                                                   Exp.Like(alias + MailTable.Columns.bcc, filter.SearchFilter,
                                                            SqlLike.AnyWhere),
                                                   Exp.Like(alias + MailTable.Columns.subject, filter.SearchFilter,
                                                            SqlLike.AnyWhere)))));
                }
            }

            return conditions;
        }


        public static SqlQuery ApplySorting(this SqlQuery query, MailFilter filter)
        {
            var sortField = MailTable.Columns.date_sent;

            switch (filter.Sort)
            {
                case "subject":
                    sortField = MailTable.Columns.subject;
                    break;
                case "sender":
                    sortField = MailTable.Columns.@from;
                    break;
            }

            var sortOrder = filter.SortOrder == "ascending";

            query.OrderBy(sortField, sortOrder);

            return query;
        }
    }
}
