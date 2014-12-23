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

        private const string loginEventsTable = "login_events";
        private const string auditEventsTable = "audit_events";

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
                var i = new SqlInsert(loginEventsTable)
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
                var i = new SqlInsert(auditEventsTable)
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