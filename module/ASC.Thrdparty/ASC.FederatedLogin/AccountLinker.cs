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

#region Import

using System;
using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.FederatedLogin.Profile;
using System.Linq;
using System.Linq.Expressions;
using System.Configuration;

#endregion


namespace ASC.FederatedLogin
{
    public class AccountLinker
    {
        private readonly string dbid;
        private const string LinkTable = "account_links";


        public AccountLinker(string dbid)
        {
            this.dbid = dbid;
        }

        public IEnumerable<string> GetLinkedObjects(string id, string provider)
        {
            return GetLinkedObjects(new LoginProfile() { Id = id, Provider = provider });
        }

        public IEnumerable<string> GetLinkedObjects(LoginProfile profile)
        {
            //Retrieve by uinque id
            return GetLinkedObjectsByHashId(profile.HashId);
        }

        public IEnumerable<string> GetLinkedObjectsByHashId(string hashid)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(dbid))
            {
                var query = new SqlQuery(LinkTable)
                    .Select("id").Where("uid", hashid).Where(!Exp.Eq("provider", string.Empty));
                return db.ExecuteList(query).ConvertAll(x => (string)x[0]);
            }
        }

        public IEnumerable<LoginProfile> GetLinkedProfiles(string obj)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(dbid))
            {
                var query = new SqlQuery(LinkTable)
                    .Select("profile").Where("id", obj);
                return db.ExecuteList(query).ConvertAll(x => LoginProfile.CreateFromSerializedString((string)x[0]));
            }
        }

        public void AddLink(string obj, LoginProfile profile)
        {
            using (var db = new DbManager(dbid))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteScalar<int>(
                        new SqlInsert(LinkTable, true)
                            .InColumnValue("id", obj)
                            .InColumnValue("uid", profile.HashId)
                            .InColumnValue("provider", profile.Provider)
                            .InColumnValue("profile", profile.ToSerializedString())
                            .InColumnValue("linked", DateTime.UtcNow)
                        );
                    tx.Commit();
                }
            }
        }

        public void AddLink(string obj, string id, string provider)
        {
            AddLink(obj, new LoginProfile() { Id = id, Provider = provider });
        }

        public void RemoveLink(string obj, string id, string provider)
        {
            RemoveLink(obj, new LoginProfile() { Id = id, Provider = provider });
        }

        public void RemoveLink(string obj, LoginProfile profile)
        {
            using (var db = new DbManager(dbid))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteScalar<int>(
                        new SqlDelete(LinkTable)
                            .Where("id", obj)
                            .Where("uid", profile.HashId)
                        );
                    tx.Commit();
                }
            }
        }

        public void Unlink(string obj)
        {
            using (var db = new DbManager(dbid))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteScalar<int>(
                        new SqlDelete(LinkTable)
                            .Where("id", obj)
                        );
                    tx.Commit();
                }
            }
        }

        public void RemoveProvider(string obj, string provider)
        {
            using (var db = new DbManager(dbid))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteScalar<int>(
                        new SqlDelete(LinkTable)
                            .Where("id", obj)
                            .Where("provider", provider)
                        );
                    tx.Commit();
                }
            }
        }
    }
}