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
using System.Configuration;
using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core.Common.Notify.Push;

namespace ASC.PushService
{
    internal class DeviceDao
    {
        private readonly string _databaseID;

        public DeviceDao()
            : this(ConfigurationManager.ConnectionStrings["core"])
        { 
        }

        public DeviceDao(ConnectionStringSettings connectionString)
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

        public void Save(Device device)
        {
            using (var db = GetDbManager())
            {
                device.ID = db.ExecuteScalar<int>(
                    new SqlInsert("push_device")
                        .ReplaceExists(true)
                        .Identity(0, 0, returnIdentity: true)
                        .InColumnValue("id", device.ID)
                        .InColumnValue("tenant_id", device.TenantID)
                        .InColumnValue("user_id", device.UserID)
                        .InColumnValue("token", device.Token)
                        .InColumnValue("type", device.Type)
                        .InColumnValue("badge", device.Badge));
            }
        }

        public List<Device> GetAll(int tenantID, string userID)
        {
            using (var db = GetDbManager())
            {
                var query = new SqlQuery("push_device")
                    .Select("id", "tenant_id", "user_id", "token", "type", "badge")
                    .Where("tenant_id", tenantID)
                    .Where("user_id", userID);

                return db.ExecuteList(
                    query,
                    r => new Device
                        {
                            ID = r.Get<int>("id"),
                            TenantID = r.Get<int>("tenant_id"),
                            UserID = r.Get<string>("user_id"),
                            Token = r.Get<string>("token"),
                            Type = (DeviceType)r.Get<int>("type"),
                            Badge = r.Get<int>("badge")
                        });
            }
        }

        public void Delete(int deviceID)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(new SqlDelete("push_device").Where("id", deviceID));
            }
        }

        public void Delete(string deviceToken)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(new SqlDelete("push_device").Where("token", deviceToken));
            }
        }

        public void UpdateToken(string oldDeviceToken, string newDeviceToken)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate("push_device")
                        .Set("token", newDeviceToken)
                        .Where("token", oldDeviceToken));
            }
        }

        private DbManager GetDbManager()
        {
            return new DbManager(_databaseID);
        }
    }
}
