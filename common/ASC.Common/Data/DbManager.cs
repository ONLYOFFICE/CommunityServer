/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Web;
using ASC.Common.Data.AdoProxy;
using ASC.Common.Data.Sql;
using ASC.Common.Logging;
using ASC.Common.Web;


namespace ASC.Common.Data
{
    public class DbManager : IDbManager
    {
        private readonly ILog logger = LogManager.GetLogger("ASC.SQL");
        private readonly ProxyContext proxyContext;
        private readonly bool shared;

        private DbCommand command;
        private ISqlDialect dialect;
        private volatile bool disposed;

        private readonly int? commandTimeout;

        private DbCommand Command
        {
            get
            {
                CheckDispose();
                if (command == null)
                {
                    command = OpenConnection().CreateCommand();
                }
                if (command.Connection.State == ConnectionState.Closed || command.Connection.State == ConnectionState.Broken)
                {
                    command = OpenConnection().CreateCommand();
                }

                if (commandTimeout.HasValue)
                {
                    command.CommandTimeout = commandTimeout.Value;
                }

                return command;
            }
        }

        public string DatabaseId { get; private set; }

        public bool InTransaction
        {
            get { return Command.Transaction != null; }
        }

        public DbConnection Connection
        {
            get { return Command.Connection; }
        }


        public DbManager(string databaseId, int? commandTimeout = null)
            : this(databaseId, true, commandTimeout)
        {
        }

        public DbManager(string databaseId, bool shared, int? commandTimeout = null)
        {
            if (databaseId == null) throw new ArgumentNullException("databaseId");
            DatabaseId = databaseId;
            this.shared = shared;

            if (logger.IsDebugEnabled)
            {
                proxyContext = new ProxyContext(AdoProxyExecutedEventHandler);
            }

            if (commandTimeout.HasValue)
            {
                this.commandTimeout = commandTimeout;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (this)
            {
                if (disposed) return;
                disposed = true;
                if (command != null)
                {
                    if (command.Connection != null) command.Connection.Dispose();
                    command.Dispose();
                    command = null;
                }
            }
        }

        #endregion

        public static IDbManager FromHttpContext(string databaseId)
        {
            if (HttpContext.Current != null)
            {
                var dbManager = DisposableHttpContext.Current[databaseId] as DbManager;
                if (dbManager == null || dbManager.disposed)
                {
                    var localDbManager = new DbManager(databaseId);
                    var dbManagerAdapter = new DbManagerProxy(localDbManager);
                    DisposableHttpContext.Current[databaseId] = localDbManager;
                    return dbManagerAdapter;
                }
                return new DbManagerProxy(dbManager);
            }
            return new DbManager(databaseId);
        }

        private DbConnection OpenConnection()
        {
            var connection = GetConnection();
            connection.Open();
            return connection;
        }

        private DbConnection GetConnection()
        {
            CheckDispose();
            DbConnection connection = null;
            string key = null;
            if (shared && HttpContext.Current != null)
            {
                key = string.Format("Connection {0}|{1}", GetDialect(), DbRegistry.GetConnectionString(DatabaseId));
                connection = DisposableHttpContext.Current[key] as DbConnection;
                if (connection != null)
                {
                    var state = ConnectionState.Closed;
                    var disposed = false;
                    try
                    {
                        state = connection.State;
                    }
                    catch (ObjectDisposedException)
                    {
                        disposed = true;
                    }
                    if (!disposed && (state == ConnectionState.Closed || state == ConnectionState.Broken))
                    {
                        if (string.IsNullOrEmpty(connection.ConnectionString))
                        {
                            connection.ConnectionString = DbRegistry.GetConnectionString(DatabaseId).ConnectionString;
                        }
                        return connection;
                    }
                }
            }
            connection = DbRegistry.CreateDbConnection(DatabaseId);
            if (proxyContext != null)
            {
                connection = new DbConnectionProxy(connection, proxyContext);
            }
            if (shared && HttpContext.Current != null) DisposableHttpContext.Current[key] = connection;
            return connection;
        }

        public IDbTransaction BeginTransaction()
        {
            if (InTransaction) throw new InvalidOperationException("Transaction already open.");

            Command.Transaction = Command.Connection.BeginTransaction();

            var tx = new DbTransaction(Command.Transaction);
            tx.Unavailable += TransactionUnavailable;
            return tx;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            if (InTransaction) throw new InvalidOperationException("Transaction already open.");

            il = GetDialect().GetSupportedIsolationLevel(il);
            Command.Transaction = Command.Connection.BeginTransaction(il);

            var tx = new DbTransaction(Command.Transaction);
            tx.Unavailable += TransactionUnavailable;
            return tx;
        }

        public IDbTransaction BeginTransaction(bool nestedIfAlreadyOpen)
        {
            return nestedIfAlreadyOpen && InTransaction ? new DbNestedTransaction(Command.Transaction) : BeginTransaction();
        }

        public List<object[]> ExecuteList(string sql, params object[] parameters)
        {
            return Command.ExecuteList(sql, parameters);
        }

        public Task<List<object[]>> ExecuteListAsync(string sql, params object[] parameters)
        {
            return Command.ExecuteListAsync(sql, parameters);
        }

        public List<object[]> ExecuteList(ISqlInstruction sql)
        {
            return Command.ExecuteList(sql, GetDialect());
        }

        public Task<List<object[]>> ExecuteListAsync(ISqlInstruction sql)
        {
            return Command.ExecuteListAsync(sql, GetDialect());
        }

        public List<T> ExecuteList<T>(ISqlInstruction sql, Converter<IDataRecord, T> converter)
        {
            return Command.ExecuteList(sql, GetDialect(), converter);
        }

        public T ExecuteScalar<T>(string sql, params object[] parameters)
        {
            return Command.ExecuteScalar<T>(sql, parameters);
        }

        public T ExecuteScalar<T>(ISqlInstruction sql)
        {
            return Command.ExecuteScalar<T>(sql, GetDialect());
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return Command.ExecuteNonQuery(sql, parameters);
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            return Command.ExecuteNonQueryAsync(sql, parameters);
        }

        public int ExecuteNonQuery(ISqlInstruction sql)
        {
            return Command.ExecuteNonQuery(sql, GetDialect());
        }

        public int ExecuteBatch(IEnumerable<ISqlInstruction> batch)
        {
            if (batch == null) throw new ArgumentNullException("batch");

            var affected = 0;
            using (var tx = BeginTransaction())
            {
                foreach (var sql in batch)
                {
                    affected += ExecuteNonQuery(sql);
                }
                tx.Commit();
            }
            return affected;
        }

        private void TransactionUnavailable(object sender, EventArgs e)
        {
            if (Command.Transaction != null)
            {
                Command.Transaction = null;
            }
        }

        private void CheckDispose()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        private ISqlDialect GetDialect()
        {
            return dialect ?? (dialect = DbRegistry.GetSqlDialect(DatabaseId));
        }

        private void AdoProxyExecutedEventHandler(ExecutedEventArgs a)
        {
            logger.DebugWithProps(a.SqlMethod,
                new KeyValuePair<string, object>("duration", a.Duration.TotalMilliseconds),
                new KeyValuePair<string, object>("sql", RemoveWhiteSpaces(a.Sql)),
                new KeyValuePair<string, object>("sqlParams", RemoveWhiteSpaces(a.SqlParameters))
                );
        }

        private string RemoveWhiteSpaces(string str)
        {
            return !string.IsNullOrEmpty(str) ?
                str.Replace(Environment.NewLine, " ").Replace("\n", "").Replace("\r", "").Replace("\t", " ") :
                string.Empty;
        }
    }

    public class DbManagerProxy : IDbManager
    {
        private DbManager dbManager { get; set; }

        public DbManagerProxy(DbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void Dispose()
        {
            if (HttpContext.Current == null)
            {
                dbManager.Dispose();
            }
        }

        public DbConnection Connection { get { return dbManager.Connection; } }
        public string DatabaseId { get { return dbManager.DatabaseId; } }
        public bool InTransaction { get { return dbManager.InTransaction; } }

        public IDbTransaction BeginTransaction()
        {
            return dbManager.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return dbManager.BeginTransaction(isolationLevel);
        }

        public IDbTransaction BeginTransaction(bool nestedIfAlreadyOpen)
        {
            return dbManager.BeginTransaction(nestedIfAlreadyOpen);
        }

        public List<object[]> ExecuteList(string sql, params object[] parameters)
        {
            return dbManager.ExecuteList(sql, parameters);
        }

        public List<object[]> ExecuteList(ISqlInstruction sql)
        {
            return dbManager.ExecuteList(sql);
        }

        public Task<List<object[]>> ExecuteListAsync(ISqlInstruction sql)
        {
            return dbManager.ExecuteListAsync(sql);
        }

        public List<T> ExecuteList<T>(ISqlInstruction sql, Converter<IDataRecord, T> converter)
        {
            return dbManager.ExecuteList<T>(sql, converter);
        }

        public T ExecuteScalar<T>(string sql, params object[] parameters)
        {
            return dbManager.ExecuteScalar<T>(sql, parameters);
        }

        public T ExecuteScalar<T>(ISqlInstruction sql)
        {
            return dbManager.ExecuteScalar<T>(sql);
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return dbManager.ExecuteNonQuery(sql, parameters);
        }

        public int ExecuteNonQuery(ISqlInstruction sql)
        {
            return dbManager.ExecuteNonQuery(sql);
        }

        public int ExecuteBatch(IEnumerable<ISqlInstruction> batch)
        {
            return dbManager.ExecuteBatch(batch);
        }
    }
}