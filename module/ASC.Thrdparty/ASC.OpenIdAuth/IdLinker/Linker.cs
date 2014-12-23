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
using System.Collections.Generic;
using System.Configuration;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using OpenIdAuth.Profile;

namespace OpenIdAuth.IdLinker
{
    public class Linker
    {
        private const string DbId = "openidlinker";
        private const string LinkTable = "openid_links";

        public Linker(ConnectionStringSettings connectionString)
        {
            if (!DbRegistry.IsDatabaseRegistered(DbId))
            {
                DbRegistry.RegisterDatabase(DbId, connectionString);
            }
        }

        public IEnumerable<string> GetLinkedObjects(UniversalProfile profile)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(DbId))
            {
                var query = new SqlQuery(LinkTable)
                    .Select("id").Where("uid", profile.HashId);
                return db.ExecuteList(query, (x)=>(string)x[0]);
            }
        }

        public IEnumerable<UniversalProfile> GetLinkedProfiles(string obj)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(DbId))
            {
                var query = new SqlQuery(LinkTable)
                    .Select("profile").Where("id", obj);
                return db.ExecuteList(query, (x) => UniversalProfile.CreateFromSerializedString((string)x[0]));
            }
        }

        public void AddLink(string obj, UniversalProfile profile)
        {
            using (var db = new DbManager(DbId))
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteScalar<int>(
                        new SqlInsert(LinkTable)
                            .InColumnValue("id", obj)
                            .InColumnValue("uid", profile.HashId)
                            .InColumnValue("provider", profile.Provider)
                            .InColumnValue("profile", profile.ToSerializedString())
                            .InColumnValue("linked", DateTime.Now)
                        );
                    tx.Commit();
                }
            }

        }

        public void RemoveLink(string obj, UniversalProfile profile)
        {
            using (var db = new DbManager(DbId))
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
    }
}