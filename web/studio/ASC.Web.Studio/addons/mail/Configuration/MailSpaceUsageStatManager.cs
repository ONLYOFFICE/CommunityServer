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


using System;
using System.Collections.Generic;
using System.Configuration;
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
