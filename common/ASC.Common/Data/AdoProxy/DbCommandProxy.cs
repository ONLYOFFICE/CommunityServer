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
using System.Data;
using System.Data.Common;

namespace ASC.Common.Data.AdoProxy
{
    class DbCommandProxy : DbCommand
    {
        private readonly DbCommand command;
        private readonly ProxyContext context;
        private bool disposed;


        public DbCommandProxy(DbCommand command, ProxyContext ctx)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (ctx == null) throw new ArgumentNullException("ctx");

            this.command = command;
            context = ctx;
        }


        public override void Cancel()
        {
            command.Cancel();
        }

        public override string CommandText
        {
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return command.CommandTimeout; }
            set { command.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return command.CommandType; }
            set { command.CommandType = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return new DbConnectionProxy(command.Connection, context); }
            set { command.Connection = value is DbConnectionProxy ? value : new DbConnectionProxy(value, context); }
        }

        protected override DbParameter CreateDbParameter()
        {
            return command.CreateParameter();
        }

        public override int ExecuteNonQuery()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "ExecuteNonQuery", dur)))
            {
                return command.ExecuteNonQuery();
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, string.Format("ExecuteReader({0})", behavior), dur)))
            {
                return command.ExecuteReader(behavior);
            }
        }

        public override object ExecuteScalar()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "ExecuteScalar", dur)))
            {
                return command.ExecuteScalar();
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return command.Parameters; }
        }

        public override void Prepare()
        {
            command.Prepare();
        }

        protected override System.Data.Common.DbTransaction DbTransaction
        {
            get { return command.Transaction == null ? null : new DbTransactionProxy(command.Transaction, context); }
            set { command.Transaction = value is DbTransactionProxy ? ((DbTransactionProxy)value).Transaction : value; }
        }

        public override bool DesignTimeVisible
        {
            get { return command.DesignTimeVisible; }
            set { command.DesignTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return command.UpdatedRowSource; }
            set { command.UpdatedRowSource = value; }
        }


        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    command.Dispose();
                }
                disposed = true;
            }
        }

        ~DbCommandProxy()
        {
            Dispose(false);
        }
    }
}