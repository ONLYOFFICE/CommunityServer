/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Web.Core;

namespace ASC.Feed.Aggregator.Modules
{
    internal abstract class FeedModule : IFeedModule
    {
        public abstract string Name { get; }
        public abstract string Product { get; }
        public abstract Guid ProductID { get; }

        protected abstract string Table { get; }
        protected abstract string LastUpdatedColumn { get; }
        protected abstract string TenantColumn { get; }
        protected abstract string DbId { get; }

        protected int Tenant
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        protected string GetGroupId(string item, Guid author, string rootId = null, int action = -1)
        {
            const int interval = 2;

            var now = DateTime.UtcNow;
            var hours = now.Hour;
            var groupIdHours = hours - (hours % interval);

            if (rootId == null)
            {
                // groupId = {item}_{author}_{date}
                return string.Format("{0}_{1}_{2}",
                                     item,
                                     author,
                                     now.ToString("yyyy.MM.dd.") + groupIdHours);
            }
            if (action == -1)
            {
                // groupId = {item}_{author}_{date}_{rootId}_{action}
                return string.Format("{0}_{1}_{2}_{3}",
                                     item,
                                     author,
                                     now.ToString("yyyy.MM.dd.") + groupIdHours,
                                     rootId);
            }

            // groupId = {item}_{author}_{date}_{rootId}_{action}
            return string.Format("{0}_{1}_{2}_{3}_{4}",
                                 item,
                                 author,
                                 now.ToString("yyyy.MM.dd.") + groupIdHours,
                                 rootId,
                                 action);
        }


        public virtual IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q = new SqlQuery(Table)
                .Select(TenantColumn)
                .Where(Exp.Gt(LastUpdatedColumn, fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            using (var db = new DbManager(DbId))
            {
                return db
                    .ExecuteList(q)
                    .ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public abstract IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter);

        public virtual bool VisibleFor(Feed feed, object data, Guid userId)
        {
            return WebItemSecurity.IsAvailableForUser(ProductID.ToString(), userId);
        }
    }
}