/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Data.AdoProxy;
using ASC.Common.Data.Sql;
using ASC.Common.Logging;

using LogManager = ASC.Common.Logging.BaseLogManager;

namespace ASC.Common.Data
{
    public class DbSleepConnectionsCounter
    {
        public static volatile int SleepConnectionsCount;
    }

    public class DbManager : IDbManager
    {
        private readonly ILog logger = LogManager.GetLogger("ASC.SQL");
        private readonly ProxyContext proxyContext;

        private DbCommand command;
        private ISqlDialect dialect;
        private volatile bool disposed;

        private readonly int? commandTimeout;

        public DbCommand Command
        {
            get
            {
                CheckDispose();
                if (command == null)
                {
                    command = OpenConnection().CreateCommand();

                    if (logger.IsTraceEnabled)
                    {
                        CheckSleepConnections("new command");
                    }
                }

                if (command.Connection.State == ConnectionState.Broken)
                {
                    command.Connection.Close();
                    command.Connection.Open();
                }

                if (command.Connection.State == ConnectionState.Closed)
                {
                    command = OpenConnection().CreateCommand();

                    if (logger.IsTraceEnabled)
                    {
                        CheckSleepConnections("closed command");
                    }
                }

                if (commandTimeout.HasValue)
                {
                    command.CommandTimeout = commandTimeout.Value;
                }

                return command;
            }
        }

        private int CountSleepConnections()
        {
            return command.ExecuteScalar<int>("SELECT COUNT(*) FROM information_schema.processlist WHERE command = 'Sleep'");
        }

        private void CheckSleepConnections(string flag)
        {
            try
            {
                var previousCount = DbSleepConnectionsCounter.SleepConnectionsCount;
                var currentCount = CountSleepConnections();
                int diff = currentCount - previousCount;

                if (diff > 0)
                {
                    logger.Error(string.Format(
                            "{0}. {1} connection(s) have been leaked! Previous count: {2}, Current count: {3}",
                            flag,
                            diff,
                            previousCount,
                            currentCount
                        ));
                }

                DbSleepConnectionsCounter.SleepConnectionsCount = currentCount;
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        public string DatabaseId { get; private set; }

        public bool InTransaction
        {
            get { return Command.Transaction != null; }
        }

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public DbConnection Connection
        {
            get { return Command.Connection; }
        }


        public DbManager(string databaseId, int? commandTimeout = null)
        {
            if (databaseId == null)
            {
                throw new ArgumentNullException("databaseId");
            }

            DatabaseId = databaseId;

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
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (command != null)
            {
                if (command.Connection != null)
                {
                    command.Connection.Close();
                    command.Connection.Dispose();
                }

                command.Dispose();
                command = null;
            }
        }

        #endregion


        private DbConnection OpenConnection()
        {
            var connection = GetConnection();
            connection.Open();
            return connection;
        }

        private DbConnection GetConnection()
        {
            CheckDispose();
            var connection = DbRegistry.CreateDbConnection(DatabaseId);
            if (proxyContext != null)
            {
                connection = new DbConnectionProxy(connection, proxyContext);
            }
            return connection;
        }

        public IDbTransaction BeginTransaction()
        {
            if (InTransaction)
            {
                throw new InvalidOperationException("Transaction already open.");
            }

            Command.Transaction = Command.Connection.BeginTransaction();

            var tx = new DbTransaction(Command.Transaction);
            tx.Unavailable += TransactionUnavailable;
            return tx;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            if (InTransaction)
            {
                throw new InvalidOperationException("Transaction already open.");
            }

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
            return this.ExecuteList(sql, GetDialect());
        }

        public Task<List<object[]>> ExecuteListAsync(ISqlInstruction sql)
        {
            return this.ExecuteListAsync(sql, GetDialect());
        }

        public List<T> ExecuteList<T>(ISqlInstruction sql, Converter<IDataRecord, T> converter)
        {
            return this.ExecuteList(sql, GetDialect(), converter);
        }

        public T ExecuteScalar<T>(string sql, params object[] parameters)
        {
            return Command.ExecuteScalar<T>(sql, parameters);
        }

        public T ExecuteScalar<T>(ISqlInstruction sql)
        {
            return this.ExecuteScalar<T>(sql, GetDialect());
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
            return this.ExecuteNonQuery(sql, GetDialect());
        }

        public int ExecuteBatch(IEnumerable<ISqlInstruction> batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException("batch");
            }

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
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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
                new KeyValuePair<string, object>("sqlParams", RemoveWhiteSpaces(a.SqlParameters)),
                new KeyValuePair<string, object>("sqlThread", a.SqlThread)
                );
        }

        private string RemoveWhiteSpaces(string str)
        {
            return !string.IsNullOrEmpty(str) ?
                str.Replace(Environment.NewLine, " ").Replace("\n", "").Replace("\r", "").Replace("\t", " ") :
                string.Empty;
        }
    }
}