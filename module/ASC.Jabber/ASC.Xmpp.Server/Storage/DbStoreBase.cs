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

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Xmpp.Server.Configuration;
using ASC.Xmpp.Server.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;

namespace ASC.Xmpp.Server.Storage
{
    public abstract class DbStoreBase : IConfigurable, IDisposable
    {
        protected static readonly int MESSAGE_COLUMN_LEN = (int)Math.Pow(2, 24) - 1;

        private readonly object syncRoot = new object();
        private DbManager db;


        public virtual void Configure(IDictionary<string, string> properties)
        {
            if (properties.ContainsKey("connectionStringName"))
            {
                db = new DbManager(properties["connectionStringName"], false);

                if (!properties.ContainsKey("generateSchema") || Convert.ToBoolean(properties["generateSchema"]))
                {
                    var creates = GetCreateSchemaScript();
                    if (creates != null && 0 < creates.Length)
                    {
                        foreach (var c in creates)
                        {
                            db.ExecuteNonQuery(c);
                        }
                    }
                }
            }
            else
            {
                throw new ConfigurationErrorsException("Can not create database connection: no connectionString or connectionStringName properties.");
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
        }


        protected virtual SqlCreate[] GetCreateSchemaScript()
        {
            return new SqlCreate[0];
        }


        protected List<object[]> ExecuteList(ISqlInstruction sql)
        {
            lock (syncRoot)
            {
                try
                {
                    return db.ExecuteList(sql);
                }
                catch (DbException)
                {
                    db.Dispose();
                    db = new DbManager(db.DatabaseId, false);
                    return db.ExecuteList(sql);
                }
            }
        }

        protected T ExecuteScalar<T>(ISqlInstruction sql)
        {
            lock (syncRoot)
            {
                try
                {
                    return db.ExecuteScalar<T>(sql);
                }
                catch (DbException)
                {
                    db.Dispose();
                    db = new DbManager(db.DatabaseId, false);
                    return db.ExecuteScalar<T>(sql);
                }
            }
        }

        protected int ExecuteNonQuery(ISqlInstruction sql)
        {
            lock (syncRoot)
            {
                try
                {
                    return db.ExecuteNonQuery(sql);
                }
                catch (DbException)
                {
                    db.Dispose();
                    db = new DbManager(db.DatabaseId, false);
                    return db.ExecuteNonQuery(sql);
                }
            }
        }

        protected int ExecuteBatch(IEnumerable<ISqlInstruction> batch)
        {
            lock (syncRoot)
            {
                try
                {
                    return db.ExecuteBatch(batch);
                }
                catch (DbException)
                {
                    db.Dispose();
                    db = new DbManager(db.DatabaseId, false);
                    return db.ExecuteBatch(batch);
                }
            }
        }
    }
}