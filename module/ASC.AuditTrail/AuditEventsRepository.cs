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
using ASC.AuditTrail.Mappers;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Core.Users;
using Newtonsoft.Json;
using log4net;

namespace ASC.AuditTrail
{
    public class AuditEventsRepository
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");

        private const string auditDbId = "core";

        private const string auditTable = "audit_events";
        private const string usersTable = "core_user";

        private static readonly List<string> auditColumns =
            new List<string> {"id", "ip", "initiator", "browser", "mobile", "platform", "date", "tenant_id", "user_id", "page", "action", "description"};

        public static IEnumerable<AuditEvent> GetLast(int tenant, int chunk)
        {
            var q = new SqlQuery(auditTable + " au")
                .Select(auditColumns.Select(x => "au." + x).ToArray())
                .LeftOuterJoin(usersTable + " u", Exp.EqColumns("au.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("au.tenant_id", tenant)
                .OrderBy("au.date", false)
                .SetMaxResults(chunk);

            using (var db = new DbManager(auditDbId))
            {
                return db.ExecuteList(q).ConvertAll(ToAuditEvent).Where(x => x != null);
            }
        }

        public static IEnumerable<AuditEvent> Get(int tenant, DateTime from, DateTime to)
        {
            var q = new SqlQuery(auditTable + " au")
                .Select(auditColumns.Select(x => "au." + x).ToArray())
                .LeftOuterJoin(usersTable + " u", Exp.EqColumns("au.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("au.tenant_id", tenant)
                .Where(Exp.Between("au.date", from, to))
                .OrderBy("au.date", false);

            using (var db = new DbManager(auditDbId))
            {
                return db.ExecuteList(q).Select(ToAuditEvent).Where(x => x != null);
            }
        }


        private static AuditEvent ToAuditEvent(object[] row)
        {
            try
            {
                var evt = new AuditEvent
                    {
                        Id = Convert.ToInt32(row[0]),
                        IP = Convert.ToString(row[1]),
                        Initiator = Convert.ToString(row[2]),
                        Browser = Convert.ToString(row[3]),
                        Mobile = Convert.ToBoolean(row[4]),
                        Platform = Convert.ToString(row[5]),
                        Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[6])),
                        TenantId = Convert.ToInt32(row[7]),
                        UserId = Guid.Parse(Convert.ToString(row[8])),
                        Page = Convert.ToString(row[9]),
                        Action = Convert.ToInt32(row[10])
                    };

                if (row[11] != null)
                {
                    evt.Description = JsonConvert.DeserializeObject<IList<string>>(Convert.ToString(row[11]), new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc
                        });
                }

                evt.UserName = (row[12] != null && row[13] != null)
                                   ? UserFormatter.GetUserName(Convert.ToString(row[12]), Convert.ToString(row[13]))
                                   : evt.UserId == Core.Configuration.Constants.CoreSystem.ID ? AuditReportResource.SystemAccount
                                         : evt.UserId == Core.Configuration.Constants.Guest.ID ? AuditReportResource.GuestAccount
                                               : evt.Initiator ?? AuditReportResource.UnknownAccount;

                evt.ActionText = AuditActionMapper.GetActionText(evt);
                evt.ActionTypeText = AuditActionMapper.GetActionTypeText(evt);
                evt.Product = AuditActionMapper.GetProductText(evt);
                evt.Module = AuditActionMapper.GetModuleText(evt);

                return evt;
            }
            catch(Exception)
            {
                //log.Error("Error while forming event from db: " + ex);
                return null;
            }
        }
    }
}