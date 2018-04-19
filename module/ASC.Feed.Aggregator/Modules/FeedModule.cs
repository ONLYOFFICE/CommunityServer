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


        protected static Guid ToGuid(object guid)
        {
            try
            {
                var str = guid as string;
                return !string.IsNullOrEmpty(str) ? new Guid(str) : Guid.Empty;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }

        }
    }
}