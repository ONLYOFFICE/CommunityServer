/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Xmpp.Server.Handler;

namespace ASC.Xmpp.Server.Storage
{
   
    public class UserPushInfo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string endpoint { get; set; }
        public string browser { get; set; }


        public UserPushInfo(int id, string username, string endpoint, string browser)
        {
            this.id = id;
            this.username = username;
            this.endpoint = endpoint;
            this.browser = browser;
        }

    }

    public class DbPushStore : DbStoreBase
    {
        private static readonly ILog log = LogManager.GetLogger("ASC");

        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_push", true)
                .AddColumn(new SqlCreate.Column("id", DbType.Int32).NotNull(true).Autoincrement(true).PrimaryKey(true))
                .AddColumn("username", DbType.String, 255, true)
                .AddColumn("browser", DbType.String, 255)
                .AddColumn("endpoint", DbType.String, 255);

            return new[] { t1 };
        }

        public void SaveUserEndpoint(string username, string endpoint, string browser)
        {
            if (username == null) throw new ArgumentNullException("push");

            List<UserPushInfo> userPushList = new List<UserPushInfo>();
            userPushList = GetUserEndpoint(username,browser);

            if (userPushList.Count > 0)
            {
                //rewrite record
                foreach (UserPushInfo user in userPushList)
                {
                    ExecuteNonQuery(new SqlUpdate("jabber_push")
                        .Set("endpoint", endpoint)
                        .Where("id",user.id));
                }
            }
            else
            {
                //create a new record
                ExecuteNonQuery(new SqlInsert("jabber_push", true)
                    .InColumnValue("username", username)
                    .InColumnValue("browser", browser)
                    .InColumnValue("endpoint", endpoint));
            }
        }

        public List<UserPushInfo> GetUserEndpoint(string username)
        {
            return ExecuteList(new SqlQuery("jabber_push").Select("id", "username", "endpoint", "browser").Where(Exp.Like("username", username)))
                        .ConvertAll(r => new UserPushInfo((int)r[0], (string)r[1], (string)r[2], (string)r[3]));
        }

        public List<UserPushInfo> GetUserEndpoint(string username, string browser)
        {
            return ExecuteList(new SqlQuery("jabber_push").Select("id", "username", "endpoint", "browser")
                        .Where(Exp.Like("username", username))
                        .Where(Exp.Like("browser", browser)))
                        .ConvertAll(r => new UserPushInfo((int)r[0], (string)r[1], (string)r[2], (string)r[3]));
        }
    }
}