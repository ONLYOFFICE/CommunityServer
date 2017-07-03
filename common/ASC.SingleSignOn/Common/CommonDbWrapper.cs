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
using ASC.Common.Data.Sql.Expressions;
using log4net;
using System;

namespace ASC.SingleSignOn.Common
{
    public class CommonDbWrapper
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CommonDbWrapper));

        public void SaveJti(string tokenType, int tenant, string jti, DateTime expirationDate)
        {
            if (tokenType == null) throw new ArgumentNullException("tokenType");
            if (jti == null) throw new ArgumentNullException("jti");

            log.DebugFormat("Saving jti: tokenType={0}, tenant={1}, jti={2}, expirationDate={3}", tokenType, tenant, jti, expirationDate);

            using (var db = new DbManager("webstudio"))
            {
                db.ExecuteNonQuery(new SqlInsert("sso_tokens")
                    .InColumnValue("tokenType", tokenType)
                    .InColumnValue("tenant", tenant)
                    .InColumnValue("tokenId", jti)
                    .InColumnValue("expirationDate", expirationDate));
            }
        }

        public void RemoveOldJtis()
        {
            var utcNow = DateTime.UtcNow;
            log.DebugFormat("Removing old jtis now={0}", utcNow);
            using (var db = new DbManager("webstudio"))
            {
                db.ExecuteNonQuery(new SqlDelete("sso_tokens")
                    .Where(Exp.Lt("expirationDate", utcNow)));
            }
        }

        public bool JtiIsExists(string jti)
        {
            if (jti == null) throw new ArgumentNullException("jti");

            log.DebugFormat("Check existing jti={0}", jti);

            using (var db = new DbManager("webstudio"))
            {
                var list = db.ExecuteList(new SqlQuery("sso_tokens")
                    .Select("tokenId")
                    .Where("tokenId", jti));
                if (list.Count == 0)
                {
                    return false;
                }
                log.ErrorFormat("jti={0} aready exists", jti);
                return true;
            }
        }

        public void SaveExtUserId(string userId, string extUserId, string profile)
        {
            if (extUserId == null) throw new ArgumentNullException("extUserId");
            if (userId == null) throw new ArgumentNullException("userId");
            if (profile == null) throw new ArgumentNullException("profile");

            log.DebugFormat("Saving extrnalIserId={0}, userId={1}, profile={2}", extUserId, userId, profile);

            using (var db = new DbManager("webstudio"))
            {
                db.ExecuteNonQuery(new SqlInsert("sso_links")
                    .InColumnValue("id", userId)
                    .InColumnValue("uid", extUserId)
                    .InColumnValue("profile", profile));
            }
        }

        public string GetUserId(string extUserId, string profile)
        {
            if (extUserId == null) throw new ArgumentNullException("extUserId");
            if (profile == null) throw new ArgumentNullException("profile");

            log.DebugFormat("Geting userId: extrnalIserId={0}, profile={1}", extUserId, profile);

            using (var db = new DbManager("webstudio"))
            {
                var list = db.ExecuteList(new SqlQuery("sso_links")
                    .Select("id")
                    .Where("uid", extUserId)
                    .Where("profile", profile));
                if (list.Count == 0)
                {
                    return null;
                }
                return list[0][0].ToString();
            }
        }
    }
}
