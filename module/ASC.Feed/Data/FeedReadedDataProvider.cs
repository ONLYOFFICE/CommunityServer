/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;

namespace ASC.Feed.Data
{
    public class FeedReadedDataProvider
    {
        private const string dbId = Constants.FeedDbId;


        public DateTime GetTimeReaded()
        {
            return GetTimeReaded(GetUser(), "all", GetTenant());
        }

        public DateTime GetTimeReaded(string module)
        {
            return GetTimeReaded(GetUser(), module, GetTenant());
        }

        public DateTime GetTimeReaded(Guid user, string module, int tenant)
        {
            var query = new SqlQuery("feed_readed")
                .SelectMax("timestamp")
                .Where("tenant_id", tenant)
                .Where("user_id", user.ToString())
                .Where("module", module);

            using (var db = GetDb())
            {
                return db.ExecuteScalar<DateTime>(query);
            }
        }

        public void SetTimeReaded()
        {
            SetTimeReaded(GetUser(), DateTime.UtcNow, "all", GetTenant());
        }

        public void SetTimeReaded(string module)
        {
            SetTimeReaded(GetUser(), DateTime.UtcNow, module, GetTenant());
        }

        public void SetTimeReaded(Guid user)
        {
            SetTimeReaded(user, DateTime.UtcNow, "all", GetTenant());
        }

        public void SetTimeReaded(Guid user, DateTime time, string module, int tenant)
        {
            if (string.IsNullOrEmpty(module)) return;

            var query = new SqlInsert("feed_readed", true)
                .InColumns("user_id", "timestamp", "module", "tenant_id")
                .Values(user.ToString(), time, module, tenant);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(query);
            }
        }

        public IEnumerable<string> GetReadedModules(DateTime fromTime)
        {
            return GetReadedModules(GetUser(), GetTenant(), fromTime);
        }

        public IEnumerable<string> GetReadedModules(Guid user, int tenant, DateTime fromTime)
        {
            var query = new SqlQuery("feed_readed")
                .Select("module")
                .Where("tenant_id", tenant)
                .Where("user_id", user)
                .Where(Exp.Gt("timestamp", fromTime));

            using (var db = GetDb())
            {
                return db.ExecuteList(query).ConvertAll(r => (string)r[0]);
            }
        }


        private static DbManager GetDb()
        {
            return new DbManager(dbId);
        }

        private static int GetTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        private static Guid GetUser()
        {
            return SecurityContext.CurrentAccount.ID;
        }
    }
}