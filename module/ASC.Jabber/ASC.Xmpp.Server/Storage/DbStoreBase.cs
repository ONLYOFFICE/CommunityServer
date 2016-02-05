/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Xmpp.Server.Configuration;
using ASC.Xmpp.Server.Utils;
using log4net;
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
        private readonly static ILog _log = LogManager.GetLogger(typeof(DbStoreBase));

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
                catch (DbException ex)
                {
                    _log.ErrorFormat("DbException: {0} {1} {2}",
                        ex, ex.InnerException != null ? ex.InnerException.Message : String.Empty, sql.ToString());
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
                catch (DbException ex)
                {
                    _log.ErrorFormat("DbException: {0} {1}",
                        ex, ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                    foreach (var sql in batch)
                    {
                        _log.ErrorFormat("sql = {0}", sql.ToString());
                    }
                    db.Dispose();
                    db = new DbManager(db.DatabaseId, false);
                    return db.ExecuteBatch(batch);
                }
            }
        }
    }
}