/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Text;
using System.Threading.Tasks;

using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Core.Common.Notify.Telegram
{
    public class TelegramDao
    {
        private readonly string _databaseID;

        public TelegramDao(string dbid)
        {
            if (dbid == null)
            {
                throw new ArgumentNullException("dbid");
            }
            _databaseID = dbid;
        }

        public void RegisterUser(Guid userId, int tenantId, int telegramId)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteScalar<int>(new SqlInsert("telegram_users").ReplaceExists(true)
                    .InColumnValue("portal_user_id", userId.ToString())
                    .InColumnValue("tenant_id", tenantId)
                    .InColumnValue("telegram_user_id", telegramId));
            }
        }

        public TelegramUser GetUser(Guid userId, int tenantId)
        {
            using (var db = GetDbManager())
            {
                var q = new SqlQuery("telegram_users u").Select("u.telegram_user_id").Where("portal_user_id", userId.ToString()).Where("tenant_id", tenantId);

                var id = db.ExecuteScalar<int>(q);
                if (id <= 0) return null;
                return new TelegramUser() { PortalUserId = userId, TenantId = tenantId, TelegramId = id };
            }
        }

        public List<TelegramUser> GetUser(int telegramId)
        {
            using (var db = GetDbManager())
            {
                var q = new SqlQuery("telegram_users").SelectAll().Where("telegram_user_id", telegramId);

                return db.ExecuteList(q, x => new TelegramUser()
                {
                    PortalUserId = Guid.Parse(x.Get<string>("portal_user_id")),
                    TenantId = x.Get<int>("tenant_id"),
                    TelegramId = x.Get<int>("telegram_user_id")
                });
            }
        }

        public void Delete(Guid userId, int tenantId)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(new SqlDelete("telegram_users").Where("portal_user_id", userId.ToString()).Where("tenant_id", tenantId));
            }
        }

        public void Delete(int telegramId)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(new SqlDelete("telegram_users").Where("telegram_user_id", telegramId));
            }
        }

        private DbManager GetDbManager()
        {
            return new DbManager(_databaseID);
        }
    }
}
