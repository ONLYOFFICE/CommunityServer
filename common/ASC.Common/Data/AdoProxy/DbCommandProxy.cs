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