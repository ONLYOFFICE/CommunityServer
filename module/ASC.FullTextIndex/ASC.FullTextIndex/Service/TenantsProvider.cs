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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.FullTextIndex.Service
{
    class TenantsProvider
    {
        private readonly string dbid;
        private readonly int userActivityDays;
        private DateTime last;


        public TenantsProvider(string dbid, int userActivityDays)
        {
            this.dbid = dbid;
            this.userActivityDays = userActivityDays;
        }

        public List<Tenant> GetTenants()
        {
            var result = new List<Tenant>();
            var tenants = CoreContext.TenantManager.GetTenants();

            if (last == DateTime.MinValue)
            {
                // first start
                if (userActivityDays == 0)
                {
                    // not use user_activity
                    result.AddRange(tenants);
                }
                else
                {
                    // use user_activity
                    using (var db = new DbManager(dbid))
                    {
                        var q = new SqlQuery("webstudio_uservisit")
                            .Select("tenantid")
                            .Where(Exp.Ge("visitdate", DateTime.UtcNow.Date.AddDays(-userActivityDays)))
                            .GroupBy(1);
                        var ids = db
                            .ExecuteList(q)
                            .ConvertAll(r => Convert.ToInt32(r[0]));
                        result.AddRange(tenants.FindAll(t => ids.Contains(t.TenantId)));
                    }
                }
            }
            else
            {
                using (var db = new DbManager(dbid))
                {
                    var q = new SqlQuery("webstudio_uservisit")
                        .Select("tenantid")
                        .Where(Exp.Eq("visitdate", DateTime.UtcNow.Date))
                        .Where(Exp.Ge("lastvisittime", last.AddHours(-1)))
                        .GroupBy(1);
                    var ids = db
                        .ExecuteList(q)
                        .ConvertAll(r => Convert.ToInt32(r[0]));
                    result.AddRange(tenants.FindAll(t => ids.Contains(t.TenantId)));
                }
            }
            last = DateTime.UtcNow;

            result.RemoveAll(t => t.Status != TenantStatus.Active);
            return result;
        }
    }
}
