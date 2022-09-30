using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Core.Common.Notify.FireBase.Dao
{
    public class FirebaseDao
    {
        public FireBaseUser RegisterUserDevice(Guid userId, int tenantId, string fbDeviceToken, bool isSubscribed, string application)
        {
            using (var db = GetDbManager())
            {
                var q = new SqlQuery("firebase_users")
                        .SelectCount()
                        .Where("user_id", userId)
                        .Where("tenant_id", tenantId)
                        .Where("application", application)
                        .Where("firebase_device_token", fbDeviceToken);

                if (db.ExecuteScalar<int>(q) == 0)
                {
                    db.ExecuteScalar<int>(new SqlInsert("firebase_users").ReplaceExists(true)
                    .InColumnValue("user_id", userId)
                    .InColumnValue("tenant_id", tenantId)
                    .InColumnValue("firebase_device_token", fbDeviceToken)
                    .InColumnValue("application", application)
                    .InColumnValue("is_subscribed", isSubscribed));
                }
                    
            }
            var list = GetUserDeviceTokens(userId, tenantId, application);
            foreach (var fb in list)
            {
                if (fb.FirebaseDeviceToken == fbDeviceToken)
                {
                    return fb;
                }
            }
            return null;

        }


        public List<FireBaseUser> GetUserDeviceTokens(Guid userId, int tenantId, string application)
        {
            using (var db = GetDbManager())
            {
                var q = new SqlQuery("firebase_users").SelectAll().Where("user_id", userId).Where("tenant_id", tenantId).Where("application", application);

                return db.ExecuteList(q, x => new FireBaseUser()
                {
                    UserId = Guid.Parse(x.Get<string>("user_id")),
                    TenantId = x.Get<int>("tenant_id"),
                    FirebaseDeviceToken = x.Get<string>("firebase_device_token"),
                    Application = x.Get<string>("application"),
                    IsSubscribed = Convert.ToBoolean(x.Get<int>("is_subscribed")),
                });
            }
        }

        public FireBaseUser UpdateUser(Guid userId, int tenantId, string fbDeviceToken, bool isSubscribed, string application)
        {
            using (var db = GetDbManager())
            {

                var q = new SqlUpdate("firebase_users")
                        .Set("is_subscribed", isSubscribed)
                        .Where("user_id", userId)
                        .Where("tenant_id", tenantId)
                        .Where("firebase_device_token", fbDeviceToken)
                        .Where("application", application);
                db.ExecuteNonQuery(q);

                return new FireBaseUser() { UserId = userId, TenantId = tenantId, FirebaseDeviceToken = fbDeviceToken, IsSubscribed = isSubscribed, Application = application };

            }
        }

        private IDbManager GetDbManager()
        {
            return DbManager.FromHttpContext("default");
        }

    }
}
