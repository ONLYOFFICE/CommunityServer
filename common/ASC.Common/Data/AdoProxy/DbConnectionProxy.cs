/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Data;

namespace ASC.Common.Data.AdoProxy
{
    class DbConnectionProxy : IDbConnection
    {
        private readonly IDbConnection connection;
        private readonly ProxyContext context;
        private bool disposed;

        
        public DbConnectionProxy(IDbConnection connection, ProxyContext ctx)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (ctx == null) throw new ArgumentNullException("ctx");

            this.connection = connection;
            context = ctx;
        }


        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, String.Format("BeginTransaction({0})", il), dur)))
            {
                return new DbTransactionProxy(connection.BeginTransaction(il), context);
            }
        }

        public IDbTransaction BeginTransaction()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "BeginTransaction", dur)))
            {
                return new DbTransactionProxy(connection.BeginTransaction(), context);
            }
        }

        public void ChangeDatabase(string databaseName)
        {
            connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            connection.Close();
        }

        public string ConnectionString
        {
            get { return connection.ConnectionString; }
            set { connection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return connection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return new DbCommandProxy(connection.CreateCommand(), context);
        }

        public string Database
        {
            get { return connection.Database; }
        }

        public void Open()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "Open", dur)))
            {
                connection.Open();
            }
        }

        public ConnectionState State
        {
            get { return connection.State; }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
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