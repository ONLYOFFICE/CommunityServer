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