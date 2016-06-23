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
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.SocialMedia.Twitter
{
    class TwitterAccountInfoDao : BaseDao
    {
        public TwitterAccountInfoDao(int tenantID, String storageKey) : base(tenantID, storageKey) { }

        public void CreateNewAccountInfo(TwitterAccountInfo accountInfo)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DeleteAccountInfo(accountInfo.AssociatedID);

                SqlInsert cmdInsert = Insert("sm_twitteraccounts")
                           .ReplaceExists(true)
                           .InColumnValue("access_token", accountInfo.AccessToken)
                           .InColumnValue("access_token_secret", accountInfo.AccessTokenSecret)
                           .InColumnValue("screen_name", accountInfo.ScreenName)
                           .InColumnValue("user_id", accountInfo.UserID)
                           .InColumnValue("associated_id", accountInfo.AssociatedID)
                           .InColumnValue("user_name", accountInfo.UserName);

                DbManager.ExecuteNonQuery(cmdInsert);

                tx.Commit();
            }
        }

        public void DeleteAccountInfo(Guid associatedID)
        {
            DbManager.ExecuteNonQuery(Delete("sm_twitteraccounts").Where(Exp.Eq("associated_id", associatedID)));
        }

        public TwitterAccountInfo GetAccountInfo(Guid associatedID)
        {
            var accounts = DbManager.ExecuteList(GetAccountQuery(Exp.Eq("associated_id", associatedID)));
            return accounts.Count > 0 ? ToTwitterAccountInfo(accounts[0]) : null;
        }

        private SqlQuery GetAccountQuery(Exp where)
        {
            SqlQuery sqlQuery = Query("sm_twitteraccounts")
                .Select(
                    "access_token",
                    "access_token_secret",
                    "screen_name",
                    "user_id",
                    "associated_id",
                    "user_name"
                );

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        private static TwitterAccountInfo ToTwitterAccountInfo(object[] row)
        {
            TwitterAccountInfo accountInfo = new TwitterAccountInfo
            {
                AccessToken = Convert.ToString(row[0]),
                AccessTokenSecret = Convert.ToString(row[1]),
                ScreenName = Convert.ToString(row[2]),
                UserID = Convert.ToDecimal(row[3]),
                AssociatedID = ToGuid(row[4]),
                UserName = (row[5] != null && row[5] != DBNull.Value) ? Convert.ToString(row[5]) : null
            };

            return accountInfo;
        }
    }
}
