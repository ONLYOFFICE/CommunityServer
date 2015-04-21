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
using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.IPSecurity
{
    internal class IPRestrictionsRepository
    {
        private const string dbId = "core";
        private const string ipRestrictionsTable = "tenants_iprestrictions";

        private static readonly List<string> ipRestrictionColumns = new List<string> {"id", "ip"};

        public static IEnumerable<IPRestriction> Get(int tenant)
        {
            var q = new SqlQuery(ipRestrictionsTable + " r")
                .Select(ipRestrictionColumns.Select(x => "r." + x).ToArray())
                .Where("r.tenant", tenant);

            using (var db = new DbManager(dbId))
            {
                return db.ExecuteList(q).ConvertAll(ToAuditEvent);
            }
        }

        public static IEnumerable<string> Save(IEnumerable<string> ips, int tenant)
        {
            using (var db = new DbManager(dbId))
            using (var tx = db.BeginTransaction())
            {
                var d = new SqlDelete(ipRestrictionsTable)
                    .Where("tenant", tenant);

                db.ExecuteNonQuery(d);

                var ipsList = ips.ToList();
                foreach (var ip in ipsList)
                {
                    var i = new SqlInsert(ipRestrictionsTable)
                        .InColumnValue("tenant", tenant)
                        .InColumnValue("ip", ip);

                    db.ExecuteNonQuery(i);
                }

                tx.Commit();
                return ipsList;
            }
        }


        private static IPRestriction ToAuditEvent(object[] row)
        {
            var r = new IPRestriction
                {
                    Id = Convert.ToInt32(row[0]),
                    Ip = Convert.ToString(row[1])
                };

            return r;
        }
    }
}