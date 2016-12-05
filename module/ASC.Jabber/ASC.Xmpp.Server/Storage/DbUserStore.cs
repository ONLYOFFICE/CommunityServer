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


using ASC.Common.Data.Sql;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Server.Storage.Interface;
using ASC.Xmpp.Server.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ASC.Xmpp.Server.Storage
{
    public class DbUserStore : DbStoreBase, IUserStore
    {
        private readonly object syncRoot = new object();

        private IDictionary<string, User> users;

        private IDictionary<string, User> Users
        {
            get
            {
                if (users == null)
                {
                    lock (syncRoot)
                    {
                        if (users == null) users = LoadFromDb();
                    }
                }
                return users;
            }
        }


        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_user", true)
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("pwd", DbType.String, 255)
                .AddColumn(new SqlCreate.Column("admin", DbType.Int32).NotNull(true).Default(0))
                .PrimaryKey("jid");
            return new[] { t1 };
        }


        #region IUserStore Members

        public ICollection<User> GetUsers(string domain)
        {
            lock (syncRoot)
            {
                return Users.Values.Where(u => string.Compare(u.Jid.Server, domain, true) == 0).ToList();
            }
        }

        public User GetUser(Jid jid)
        {
            var bareJid = GetBareJid(jid);
            lock (syncRoot)
            {
                return Users.ContainsKey(bareJid) ? Users[bareJid] : null;
            }
        }

        public void SaveUser(User user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var bareJid = GetBareJid(user.Jid);
            ExecuteNonQuery(new SqlInsert("jabber_user", true)
                .InColumnValue("jid", bareJid)
                .InColumnValue("pwd", user.Password)
                .InColumnValue("admin", user.IsAdmin));
            lock (syncRoot)
            {
                Users[bareJid] = user;
            }
        }

        public void RemoveUser(Jid jid)
        {
            var bareJid = GetBareJid(jid);
            ExecuteNonQuery(new SqlDelete("jabber_user").Where("jid", bareJid));
            lock (syncRoot)
            {
                Users.Remove(bareJid);
            }
        }

        #endregion

        private IDictionary<string, User> LoadFromDb()
        {
            return ExecuteList(new SqlQuery("jabber_user").Select("jid", "pwd", "admin"))
                .ConvertAll(r => new User(new Jid((string)r[0]), (string)r[1], Convert.ToBoolean(r[2])))
                .ToDictionary(u => u.Jid.ToString());
        }

        private string GetBareJid(Jid jid)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            return jid.Bare.ToLowerInvariant();
        }
    }
}