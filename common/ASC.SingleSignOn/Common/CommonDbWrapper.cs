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

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using log4net;
using System;

namespace ASC.SingleSignOn.Common
{
    public class CommonDbWrapper
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(CommonDbWrapper));

        public void SaveJti(string tokenType, int tenant, string jti, DateTime expirationDate)
        {
            if (tokenType == null) throw new ArgumentNullException("tokenType");
            if (jti == null) throw new ArgumentNullException("jti");

            _log.DebugFormat("Saving jti: tokenType={0},tenant={1},jti={2},expirationDate={3}", tokenType, tenant, jti, expirationDate);

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
            _log.DebugFormat("Removing old jtis now={0}", utcNow);
            using (var db = new DbManager("webstudio"))
            {
                db.ExecuteNonQuery(new SqlDelete("sso_tokens")
                    .Where(Exp.Lt("expirationDate", utcNow)));
            }
        }

        public bool JtiIsExists(string jti)
        {
            if (jti == null) throw new ArgumentNullException("jti");

            _log.DebugFormat("Check existing jti={0}", jti);

            using (var db = new DbManager("webstudio"))
            {
                var list = db.ExecuteList(new SqlQuery("sso_tokens")
                    .Select("tokenId")
                    .Where("tokenId", jti));
                if (list.Count == 0)
                {
                    return false;
                }
                _log.ErrorFormat("jti={0} aready exists", jti);
                return true;
            }
        }

        public void SaveExtUserId(string userId, string extUserId, string profile)
        {
            if (extUserId == null) throw new ArgumentNullException("extUserId");
            if (userId == null) throw new ArgumentNullException("userId");
            if (profile == null) throw new ArgumentNullException("profile");

            _log.DebugFormat("Saving extrnalIserId={0}, userId={1}, profile={2}", extUserId, userId, profile);

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

            _log.DebugFormat("Geting userId: extrnalIserId={0}, profile={1}", extUserId, profile);

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
