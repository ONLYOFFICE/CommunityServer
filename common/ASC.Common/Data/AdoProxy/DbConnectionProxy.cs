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
    class DbConnectionProxy : DbConnection
    {
        private readonly DbConnection connection;
        private readonly ProxyContext context;
        private bool disposed;

        
        public DbConnectionProxy(DbConnection connection, ProxyContext ctx)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (ctx == null) throw new ArgumentNullException("ctx");

            this.connection = connection;
            context = ctx;
        }

        protected override System.Data.Common.DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "BeginTransaction", dur)))
            {
                return new DbTransactionProxy(connection.BeginTransaction(), context);
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

        public override string  ConnectionString
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
            return new DbCommandProxy(connection.CreateCommand(), context);
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
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "Open", dur)))
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

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "Dispose", dur)))
                    {
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