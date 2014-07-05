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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Server.Storage.Interface;
using ASC.Xmpp.Server.Users;
using ASC.Collections;

namespace ASC.Xmpp.Server.Storage
{
    public class DbUserStore : DbStoreBase, IUserStore
    {
        private object syncRoot = new object();

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
            lock (syncRoot)
            {
                var bareJid = GetBareJid(user.Jid);
                ExecuteNonQuery(new SqlInsert("jabber_user", true)
                    .InColumnValue("jid", bareJid)
                    .InColumnValue("pwd", user.Password)
                    .InColumnValue("admin", user.IsAdmin));
                Users[bareJid] = user;
            }
        }

        public void RemoveUser(Jid jid)
        {
            var bareJid = GetBareJid(jid);
            lock (syncRoot)
            {
                ExecuteNonQuery(new SqlDelete("jabber_user").Where("jid", bareJid));
                Users.Remove(bareJid);
            }
        }

        #endregion

        private IDictionary<string, User> LoadFromDb()
        {
            var d = ExecuteList(new SqlQuery("jabber_user").Select("jid", "pwd", "admin"))
                .ConvertAll(r => new User(new Jid((string)r[0]), (string)r[1], Convert.ToBoolean(r[2])))
                .ToDictionary(u => u.Jid.ToString());
            return new SynchronizedDictionary<string, User>(d);
        }

        private string GetBareJid(Jid jid)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            return jid.Bare.ToLowerInvariant();
        }
    }
}