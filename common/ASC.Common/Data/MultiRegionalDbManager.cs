/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ASC.Common.Data.Sql;

namespace ASC.Common.Data
{
    public class MultiRegionalDbManager : IDbManager
    {
        private readonly List<IDbManager> databases;

        private readonly IDbManager localDb;

        private volatile bool disposed;


        public string DatabaseId { get; private set; }
        public bool InTransaction { get { return localDb.InTransaction; } }

        public DbConnection Connection
        {
            get { return localDb.Connection; }
        }


        public MultiRegionalDbManager(string dbId)
        {
            const StringComparison cmp = StringComparison.InvariantCultureIgnoreCase;
            DatabaseId = dbId;
            databases = ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>()
                                            .Where(c => c.Name.Equals(dbId, cmp) || c.Name.StartsWith(dbId + ".", cmp))
                                            .Select(
                                                c =>
                                                HttpContext.Current != null
                                                    ? DbManager.FromHttpContext(c.Name)
                                                    : new DbManager(c.Name))
                                            .ToList();
            localDb = databases.SingleOrDefault(db => db.DatabaseId.Equals(dbId, cmp));
        }

        public MultiRegionalDbManager(IEnumerable<IDbManager> databases)
        {
            this.databases = databases.ToList();
            localDb = databases.FirstOrDefault();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (disposed) return;
                disposed = true;
                databases.ForEach(db => db.Dispose());
            }
        }

        public static MultiRegionalDbManager FromHttpContext(string databaseId)
        {
            return new MultiRegionalDbManager(databaseId);
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return localDb.BeginTransaction(isolationLevel);
        }

        public IDbTransaction BeginTransaction(bool nestedIfAlreadyOpen)
        {
            return localDb.BeginTransaction(nestedIfAlreadyOpen);
        }

        public List<object[]> ExecuteList(string sql, params object[] parameters)
        {
            return databases.SelectMany(db => db.ExecuteList(sql, parameters)).ToList();
        }

        public List<object[]> ExecuteList(ISqlInstruction sql)
        {
            return databases.SelectMany(db => db.ExecuteList(sql)).ToList();
        }

        public Task<List<object[]>> ExecuteListAsync(ISqlInstruction sql)
        {
            throw new NotImplementedException(); //TODO: implement

        }

        public List<object[]> ExecuteListWithRegion(ISqlInstruction sql)
        {
            return databases.SelectMany(db => db.ExecuteList(sql)
                                                .Select(oldArray =>
                                                    {
                                                        var newArray = new object[oldArray.Count() + 1];
                                                        oldArray.CopyTo(newArray, 0);
                                                        newArray[oldArray.Count()] =
                                                            db.DatabaseId.IndexOf('.') > -1
                                                                ? db.DatabaseId.Substring(db.DatabaseId.IndexOf('.') + 1)
                                                                : "";

                                                        return newArray;
                                                    })).ToList();
        }

        public List<T> ExecuteList<T>(ISqlInstruction sql, Converter<IDataRecord, T> converter)
        {
            return databases.SelectMany(db => db.ExecuteList(sql, converter)).ToList();
        }

        public T ExecuteScalar<T>(string sql, params object[] parameters)
        {
            return localDb.ExecuteScalar<T>(sql, parameters);
        }

        public T ExecuteScalar<T>(ISqlInstruction sql)
        {
            return localDb.ExecuteScalar<T>(sql);
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return localDb.ExecuteNonQuery(sql, parameters);
        }

        public int ExecuteNonQuery(ISqlInstruction sql)
        {
            return localDb.ExecuteNonQuery(sql);
        }

        public int ExecuteNonQuery(string region, ISqlInstruction sql)
        {
            var db = string.IsNullOrEmpty(region)
                         ? localDb
                         : databases.FirstOrDefault(x => x.DatabaseId.EndsWith(region));
            if (db != null) return db.ExecuteNonQuery(sql);
            return -1;
        }

        public int ExecuteBatch(IEnumerable<ISqlInstruction> batch)
        {
            return localDb.ExecuteBatch(batch);
        }

        public IDbTransaction BeginTransaction()
        {
            return localDb.BeginTransaction();
        }
    }
}