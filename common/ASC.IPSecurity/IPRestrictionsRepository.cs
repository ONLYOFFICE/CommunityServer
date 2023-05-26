/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.IPSecurity
{
    internal class IPRestrictionsRepository
    {
        private const string dbId = "default";


        public static List<IPRestriction> Get(int tenant)
        {
            using (var db = new DbManager(dbId))
            {
                return db
                    .ExecuteList(new SqlQuery("tenants_iprestrictions").Select("id", "ip", "for_admin").Where("tenant", tenant))
                    .ConvertAll(r => new IPRestriction
                    {
                        Id = Convert.ToInt32(r[0]),
                        Ip = Convert.ToString(r[1]),
                        ForAdmin = Convert.ToBoolean(r[2]),
                        TenantId = tenant,
                    });
            }
        }

        public static List<IPRestrictionBase> Save(IEnumerable<IPRestrictionBase> ips, int tenant)
        {
            using (var db = new DbManager(dbId))
            using (var tx = db.BeginTransaction())
            {
                var d = new SqlDelete("tenants_iprestrictions").Where("tenant", tenant);
                db.ExecuteNonQuery(d);

                var ipsList = ips.ToList();
                foreach (var ip in ipsList)
                {
                    var i = new SqlInsert("tenants_iprestrictions")
                        .InColumnValue("tenant", tenant)
                        .InColumnValue("ip", ip.Ip)
                        .InColumnValue("for_admin", ip.ForAdmin);

                    db.ExecuteNonQuery(i);
                }

                tx.Commit();
                return ipsList;
            }
        }
    }
}