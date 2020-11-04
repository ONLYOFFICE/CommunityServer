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
using System.Web;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core.Common.Notify.Push;

namespace ASC.Web.Core.Mobile
{
    public class MobileAppInstallRegistrator : IMobileAppInstallRegistrator
    {
        public void RegisterInstall(string userEmail, MobileAppType appType)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(
                    "INSERT INTO `mobile_app_install` (`user_email`, `app_type`, `registered_on`, `last_sign`)" +
                    " VALUES (@user_email, @app_type, @registered_on, @last_sign)" +
                    " ON DUPLICATE KEY UPDATE `last_sign`=@last_sign",
                    new
                        {
                            user_email = userEmail,
                            app_type = (int) appType,
                            registered_on = DateTime.UtcNow,
                            last_sign = DateTime.UtcNow
                        });
            }
        }

        public bool IsInstallRegistered(string userEmail, MobileAppType? appType)
        {
            var query = new SqlQuery("mobile_app_install")
                .SelectCount()
                .Where("user_email", userEmail);

            if (appType.HasValue)
                query.Where("app_type", (int) appType.Value);


            using (var db = GetDbManager())
            {
                return db.ExecuteScalar<int>(query) > 0;
            }
        }

        private IDbManager GetDbManager()
        {
            return DbManager.FromHttpContext("default");
        }
    }
}