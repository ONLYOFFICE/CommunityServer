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
using System.Configuration;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Common.Notify.Push;

namespace ASC.PushService
{
    internal class NotificationDao
    {
        private readonly string _databaseID;

        public NotificationDao()
            : this(ConfigurationManager.ConnectionStrings["core"])
        {
        }

        public NotificationDao(ConnectionStringSettings connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (!DbRegistry.IsDatabaseRegistered(connectionString.Name))
            {
                DbRegistry.RegisterDatabase(connectionString.Name, connectionString);
            }
            _databaseID = connectionString.Name;
        }

        public void Save(int deviceID, PushNotification notification)
        {
            var insertQuery = new SqlInsert("push_notification")
                .InColumnValue("device_id", deviceID)
                .InColumnValue("module", notification.Module)
                .InColumnValue("action", notification.Action)
                .InColumnValue("item_type", notification.Item.Type)
                .InColumnValue("item_id", notification.Item.ID)
                .InColumnValue("item_description", notification.Item.Description)
                .InColumnValue("queued_on", DateTime.UtcNow);

            if (notification.ParentItem != null)
                insertQuery
                    .InColumnValue("parent_type", notification.ParentItem.Type)
                    .InColumnValue("parent_id", notification.ParentItem.ID)
                    .InColumnValue("parent_description", notification.ParentItem.Description);

            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(insertQuery);
            }
        }

        public List<PushNotification> GetNotifications(int tenantID, string userID, string deviceToken, DateTime from, DateTime to)
        {
            var selectQuery = new SqlQuery("push_notification t1")
                .Select("t1.module", "t1.action", "t1.item_type", "t1.item_id", "t1.item_description", "t1.parent_type", "t1.parent_id", "t1.parent_description", "t1.queued_on", "t2.token")
                .InnerJoin("push_device t2", Exp.EqColumns("t1.device_id", "t2.id"))
                .Where("t2.tenant_id", tenantID)
                .Where("t2.user_id", userID)
                .Where("t2.token", deviceToken);

            if (from > DateTime.MinValue)
                selectQuery.Where(Exp.Gt("t1.queued_on", from));

            if (to < DateTime.MaxValue)
                selectQuery.Where(Exp.Lt("t1.queued_on", to));

            using (var db = GetDbManager())
            {
                return db.ExecuteList(
                    selectQuery,
                    r =>
                        {
                            var notification = new PushNotification
                                {
                                    Module = (PushModule)r.Get<int>("module"),
                                    Action = (PushAction)r.Get<int>("action"),
                                    Item = new PushItem
                                        {
                                            Type = (PushItemType)r.Get<int>("item_type"),
                                            ID = r.Get<string>("item_id"),
                                            Description = r.Get<string>("item_description")
                                        },
                                    QueuedOn = r.Get<DateTime>("queued_on")
                                };

                            if (!r.IsDBNull(r.GetOrdinal("parent_type")))
                                notification.ParentItem = new PushItem
                                    {
                                        Type = (PushItemType)r.Get<int>("parent_type"),
                                        ID = r.Get<string>("parent_id"),
                                        Description = r.Get<string>("parent_description")
                                    };

                            return notification;
                        });
            }
        }

        public void Delete(DateTime to)
        {
            var deleteQuery = new SqlDelete("push_notification")
                .Where(Exp.Le("queued_on", to));

            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(deleteQuery);
            }
        }

        private DbManager GetDbManager()
        {
            return new DbManager(_databaseID);
        }
    }
}
