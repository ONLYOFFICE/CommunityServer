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

using System;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.SocialMedia.Facebook
{
    class FacebookAccountInfoDao : BaseDao
    {
        public FacebookAccountInfoDao(int tenantID, String storageKey) : base(tenantID, storageKey) { }

        public void CreateNewAccountInfo(FacebookAccountInfo accountInfo)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DeleteAccountInfo(accountInfo.AssociatedID);

                SqlInsert cmdInsert = Insert("sm_facebookaccounts")
                           .ReplaceExists(true)
                           .InColumnValue("access_token", accountInfo.AccessToken)                           
                           .InColumnValue("user_id", accountInfo.UserID)
                           .InColumnValue("associated_id", accountInfo.AssociatedID)
                           .InColumnValue("user_name", accountInfo.UserName);

                DbManager.ExecuteNonQuery(cmdInsert);

                tx.Commit();
            }
        }

        public void DeleteAccountInfo(Guid associatedID)
        {
            DbManager.ExecuteNonQuery(Delete("sm_facebookaccounts").Where(Exp.Eq("associated_id", associatedID)));
        }

        public FacebookAccountInfo GetAccountInfo(Guid associatedID)
        {
            var accounts = DbManager.ExecuteList(GetAccountQuery(Exp.Eq("associated_id", associatedID)));
            return accounts.Count > 0 ? ToFacebookAccountInfo(accounts[0]) : null;
        }

        private SqlQuery GetAccountQuery(Exp where)
        {
            SqlQuery sqlQuery = Query("sm_facebookaccounts")
                .Select(
                    "access_token",
                    "user_id",
                    "associated_id",
                    "user_name"
                );

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        private static FacebookAccountInfo ToFacebookAccountInfo(object[] row)
        {
            FacebookAccountInfo accountInfo = new FacebookAccountInfo
            {
                AccessToken = Convert.ToString(row[0]),
                UserID = Convert.ToString(row[1]),
                AssociatedID = ToGuid(row[2]),
                UserName = Convert.ToString(row[3])
            };

            return accountInfo;
        }
    }
}
