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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Mail.Configuration
{
    public class MailSpaceUsageStatManager : SpaceUsageStatManager, IUserSpaceUsage
    {
        private const string MailDatabaseId = "mail";

        public override List<UsageSpaceStatItem> GetStatData()
        {
            using (var mail_db = new DbManager(MailDatabaseId))
            {
                var query = new SqlQuery("mail_attachment a")
                    .InnerJoin("mail_mail m", Exp.EqColumns("a.id_mail", "m.id"))
                    .Select("m.id_user")
                    .Select("sum(a.size) as size")
                    .Where("a.tenant", TenantProvider.CurrentTenantID)
                    .Where("a.need_remove", 0)
                    .GroupBy(1)
                    .OrderBy(2, false);

                return mail_db.ExecuteList(query)
                    .Select(r =>
                        {
                            var user_id = new Guid(Convert.ToString(r[0]));
                            var user = CoreContext.UserManager.GetUsers(user_id);
                            var item = new UsageSpaceStatItem
                                {
                                    Name = DisplayUserSettings.GetFullUserName(user, false),
                                    ImgUrl = UserPhotoManager.GetSmallPhotoURL(user.ID),
                                    Url = CommonLinkUtility.GetUserProfile(user.ID),
                                    SpaceUsage = Convert.ToInt64(r[1]),
                                    Disabled = user.Status == EmployeeStatus.Terminated
                                };
                            return item;
                        })
                    .ToList();
            }
        }

        public long GetUserSpaceUsage(Guid userId)
        {
            using (var mail_db = new DbManager(MailDatabaseId))
            {
                var query = new SqlQuery("mail_attachment a")
                    .InnerJoin("mail_mail m", Exp.EqColumns("a.id_mail", "m.id"))
                    .Select("sum(a.size) as size")
                    .Where("a.tenant", TenantProvider.CurrentTenantID)
                    .Where("m.id_user", userId)
                    .Where("a.need_remove", 0);

                return mail_db.ExecuteScalar<long>(query);
            }
        }
    }
}
