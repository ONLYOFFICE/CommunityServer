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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ASC.MessagingSystem.DbSender
{
    internal class MessagesRepository
    {
        private const string messagesDbId = "core";


        public static void Add(EventMessage message)
        {
            // messages with action code < 2000 are related to login-history
            if ((int)message.Action < 2000)
            {
                AddLoginEvent(message);
            }
            else
            {
                AddAuditEvent(message);
            }
        }


        private static void AddLoginEvent(EventMessage message)
        {
            using (var db = new DbManager(messagesDbId))
            {
                var i = new SqlInsert("login_events")
                    .InColumnValue("ip", message.IP)
                    .InColumnValue("login", message.Initiator)
                    .InColumnValue("browser", message.Browser)
                    .InColumnValue("platform", message.Platform)
                    .InColumnValue("date", message.Date)
                    .InColumnValue("tenant_id", message.TenantId)
                    .InColumnValue("user_id", message.UserId)
                    .InColumnValue("page", message.Page)
                    .InColumnValue("action", message.Action);

                if (message.Description != null && message.Description.Any())
                {
                    i = i.InColumnValue("description", JsonConvert.SerializeObject(message.Description, new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc
                        }));
                }

                db.ExecuteNonQuery(i);
            }
        }

        private static void AddAuditEvent(EventMessage message)
        {
            using (var db = new DbManager(messagesDbId))
            {
                var i = new SqlInsert("audit_events")
                    .InColumnValue("ip", message.IP)
                    .InColumnValue("initiator", message.Initiator)
                    .InColumnValue("browser", message.Browser)
                    .InColumnValue("platform", message.Platform)
                    .InColumnValue("date", message.Date)
                    .InColumnValue("tenant_id", message.TenantId)
                    .InColumnValue("user_id", message.UserId)
                    .InColumnValue("page", message.Page)
                    .InColumnValue("action", message.Action);

                if (message.Description != null && message.Description.Any())
                {
                    i = i.InColumnValue("description", JsonConvert.SerializeObject(GetSafeDescription(message.Description), new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc
                        }));
                }

                db.ExecuteNonQuery(i);
            }
        }

        private static IList<string> GetSafeDescription(IEnumerable<string> description)
        {
            const int maxLength = 15000;
            
            var currentLength = 0;
            var safe = new List<string>();

            foreach (var d in description)
            {
                if (currentLength + d.Length <= maxLength)
                {
                    currentLength += d.Length;
                    safe.Add(d);
                }
                else
                {
                    safe.Add(d.Substring(0, maxLength - currentLength  - 3) + "...");
                    break;
                }
            }

            return safe;
        }
    }
}