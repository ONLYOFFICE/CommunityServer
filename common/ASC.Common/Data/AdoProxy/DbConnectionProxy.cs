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
using System.Data;
using System.Data.Common;

using MySql.Data.MySqlClient;

namespace ASC.Common.Data.AdoProxy
{
    class DbConnectionProxy : DbConnection
    {
        private readonly DbConnection connection;
        private DbCommandProxy dbCommandProxy;
        private readonly ProxyContext context;
        private bool disposed;
        private int threadId;


        public DbConnectionProxy(DbConnection connection, ProxyContext ctx)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (ctx == null) throw new ArgumentNullException("ctx");

            this.connection = connection;
            context = ctx;
            threadId = GetThreadId();
        }

        protected override System.Data.Common.DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent("BeginTransaction", dur, GetThreadId())))
            {
                return new DbTransactionProxy(connection.BeginTransaction(), context, GetThreadId());
            }
        }

        public override void ChangeDatabase(string databaseName)
        {
            connection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            connection.Close();
        }

        public override string ConnectionString
        {
            get { return connection.ConnectionString; }
            set { connection.ConnectionString = value; }
        }

        public override int ConnectionTimeout
        {
            get { return connection.ConnectionTimeout; }
        }

        protected override DbCommand CreateDbCommand()
        {
            if (dbCommandProxy != null)
            {
                dbCommandProxy.Parameters.Clear();
                return dbCommandProxy;
            }
            dbCommandProxy = new DbCommandProxy(connection.CreateCommand(), context, this);
            return dbCommandProxy;
        }

        public override string Database
        {
            get { return connection.Database; }
        }

        public override string DataSource
        {
            get { return connection.DataSource; }
        }

        public override void Open()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent("Open", dur, GetThreadId())))
            {
                connection.Open();
            }
        }

        public override string ServerVersion
        {
            get { return connection.ServerVersion; }
        }

        public override ConnectionState State
        {
            get { return connection.State; }
        }

        internal int GetThreadId()
        {
            if (threadId != 0) return threadId;

            if (connection is MySqlConnection mySqlConnection &&
                connection.State > ConnectionState.Closed)
            {
                threadId = mySqlConnection.ServerThread;
            }

            return threadId;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    var threadId = GetThreadId();
                    using (ExecuteHelper.Begin(dur => context.FireExecuteEvent("Dispose", dur, threadId)))
                    {
                        if (dbCommandProxy != null)
                        {
                            dbCommandProxy.Dispose();
                        }
                        connection.Dispose();
                    }
                }
                disposed = true;
            }
        }

        ~DbConnectionProxy()
        {
            Dispose(false);
        }
    }
}