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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.ElasticSearch;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Message
{
    public class FilterChainMessagesExp : FilterMessagesExp
    {
        public FilterChainMessagesExp(MailSearchFilterData filter, int tenant, string user, List<int> ids = null)
            : base(ids ?? new List<int>() , tenant, user, filter)
        {
        }

        private const string MM_ALIAS = "mm";

        public override Exp GetExpression()
        {
            var exp = base.GetExpression();

            if (Filter.FromDate.HasValue)
            {
                exp &= Filter.PrevFlag.GetValueOrDefault(false)
                    ? Exp.Ge(MailTable.Columns.ChainDate.Prefix(MM_ALIAS), Filter.FromDate.Value)
                    : Exp.Le(MailTable.Columns.ChainDate.Prefix(MM_ALIAS), Filter.FromDate.Value);
            }

            return exp;
        }

        public static bool TryGetFullTextSearchChains(MailSearchFilterData filter, string user,
            out List<MailWrapper> mailWrappers)
        {
            mailWrappers = new List<MailWrapper>();

            if (!FactoryIndexer<MailWrapper>.Support)
            {
                return false;
            }

            var userId = new Guid(user);

            Selector<MailWrapper> selector = null;

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                selector = new Selector<MailWrapper>().MatchAll(filter.SearchText);
            }

            if (!string.IsNullOrEmpty(filter.FromAddress))
            {
                Selector<MailWrapper> tempSelector;

                if (filter.PrimaryFolder == FolderType.Sent || filter.PrimaryFolder == FolderType.Draft)
                {
                    tempSelector = new Selector<MailWrapper>().Match(s => s.ToText, filter.FromAddress);
                }
                else
                {
                    tempSelector = new Selector<MailWrapper>().Match(s => s.FromText, filter.FromAddress);
                }

                if (selector != null)
                    selector &= tempSelector;
                else
                    selector = tempSelector;
            }

            if (!string.IsNullOrEmpty(filter.ToAddress))
            {
                Selector<MailWrapper> tempSelector;

                if (filter.PrimaryFolder == FolderType.Sent || filter.PrimaryFolder == FolderType.Draft)
                {
                    tempSelector = new Selector<MailWrapper>().Match(s => s.FromText, filter.ToAddress);
                }
                else
                {
                    tempSelector = new Selector<MailWrapper>().Match(s => s.ToText, filter.ToAddress);
                }

                if (selector != null)
                    selector &= tempSelector;
                else
                    selector = tempSelector;
            }

            if (selector == null)
                selector = new Selector<MailWrapper>();

            selector.Where(r => r.Folder, (int) filter.PrimaryFolder);

            if (filter.MailboxId.HasValue)
            {
                selector.Where(r => r.MailboxId, filter.MailboxId.Value);
            }

            if (filter.Unread.HasValue)
            {
                selector.Where(s => s.Unread, filter.Unread.Value);
            }

            if (filter.Important.HasValue)
            {
                selector.Where(s => s.Importance, filter.Important.Value);
            }

            if (filter.Attachments.HasValue)
            {
                selector.Where(s => s.HasAttachments, filter.Attachments.Value);
            }

            if (filter.PrimaryFolder == FolderType.UserFolder && filter.UserFolderId.HasValue)
            {
                selector.InAll(s => s.UserFolders.Select(f => f.Id), new[] {filter.UserFolderId.Value});
            }

            if (filter.WithCalendar.HasValue)
            {
                selector.Where(m => m.WithCalendar, filter.WithCalendar.Value);
            }

            if (filter.CustomLabels != null && filter.CustomLabels.Any())
            {
                selector.InAll(r => r.Tags.Select(t => t.Id), filter.CustomLabels.ToArray());
            }

            if (filter.FromDate.HasValue)
            {
                if (filter.PrevFlag.GetValueOrDefault(false))
                {
                    selector.Ge(r => r.ChainDate, filter.FromDate.Value);
                }
                else
                {
                    selector.Le(r => r.ChainDate, filter.FromDate.Value);
                }
            }

            if (filter.PeriodFrom.HasValue && filter.PeriodTo.HasValue)
            {
                var fromTs = TimeSpan.FromMilliseconds(filter.PeriodFrom.Value);
                var from = Defines.BaseJsDateTime.Add(fromTs);

                var toTs = TimeSpan.FromMilliseconds(filter.PeriodTo.Value);
                var to = Defines.BaseJsDateTime.Add(toTs);

                selector.Ge(s => s.DateSent, from);
                selector.Le(s => s.DateSent, to);
            }

            if (filter.Page.HasValue)
            {
                selector.Limit(filter.Page.Value, filter.PageSize.GetValueOrDefault(25));
            }
            else if (filter.PageSize.HasValue)
            {
                selector.Limit(0, filter.PageSize.Value);
            }

            selector.Where(r => r.UserId, userId)
                .Sort(r => r.ChainDate, filter.SortOrder == Defines.ASCENDING);

            IReadOnlyCollection<MailWrapper> result;

            if (!FactoryIndexer<MailWrapper>.TrySelect(s => selector, out result)) 
                return false;

            mailWrappers = result.ToList();

            return true;
        }
    }
}