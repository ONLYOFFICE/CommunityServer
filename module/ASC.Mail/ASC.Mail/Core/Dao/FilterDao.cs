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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class FilterDao : BaseDao, IFilterDao
    {
        protected static ITable table = new MailTableFactory().Create<FilterTable>();

        protected string CurrentUserId { get; private set; }

        public FilterDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<Filter> GetList()
        {
            var query = Query()
                .Where(FilterTable.Columns.Tenant, Tenant)
                .Where(FilterTable.Columns.User, CurrentUserId);

            return Db.ExecuteList(query)
                .ConvertAll(ToFilter);
        }

        public Filter Get(int id)
        {
            var query = Query()
                .Where(FilterTable.Columns.Tenant, Tenant)
                .Where(FilterTable.Columns.User, CurrentUserId)
                .Where(FilterTable.Columns.Id, id);

            return Db.ExecuteList(query)
                .ConvertAll(ToFilter)
                .SingleOrDefault();
        }

        public int Save(Filter filter)
        {
            var now = DateTime.UtcNow;

            var query = new SqlInsert(FilterTable.TABLE_NAME, true)
                .InColumnValue(FilterTable.Columns.Id, filter.Id)
                .InColumnValue(FilterTable.Columns.Tenant, filter.Tenant)
                .InColumnValue(FilterTable.Columns.User, filter.User)
                .InColumnValue(FilterTable.Columns.Enabled, filter.Enabled)
                .InColumnValue(FilterTable.Columns.Filter, filter.FilterData)
                .InColumnValue(FilterTable.Columns.Position, filter.Position)
                .InColumnValue(FilterTable.Columns.MofifiedOn, now)
                .Identity(0, 0, true);

            if (filter.Id == 0)
            {
                query.InColumnValue(FilterTable.Columns.CreatedOn, now);
            }

            return Db.ExecuteScalar<int>(query);
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(FilterTable.TABLE_NAME)
                .Where(FilterTable.Columns.Tenant, Tenant)
                .Where(FilterTable.Columns.User, CurrentUserId)
                .Where(FilterTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected Filter ToFilter(object[] r)
        {
            var f = new Filter
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                Enabled = Convert.ToBoolean(r[3]),
                FilterData = Convert.ToString(r[4]),
                Position = Convert.ToInt32(r[5])
            };

            return f;
        }
    }
}