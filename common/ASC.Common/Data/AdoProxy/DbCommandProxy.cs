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
    class DbCommandProxy : IDbCommand
    {
        private readonly IDbCommand command;
        private readonly ProxyContext context;
        private bool disposed;


        public DbCommandProxy(IDbCommand command, ProxyContext ctx)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (ctx == null) throw new ArgumentNullException("ctx");

            this.command = command;
            context = ctx;
        }


        public void Cancel()
        {
            command.Cancel();
        }

        public string CommandText
        {
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public int CommandTimeout
        {
            get { return command.CommandTimeout; }
            set { command.CommandTimeout = value; }
        }

        public CommandType CommandType
        {
            get { return command.CommandType; }
            set { command.CommandType = value; }
        }

        public IDbConnection Connection
        {
            get { return new DbConnectionProxy(command.Connection, context); }
            set { command.Connection = value is DbConnectionProxy ? value : new DbConnectionProxy(value, context); }
        }

        public IDbDataParameter CreateParameter()
        {
            return command.CreateParameter();
        }

        public int ExecuteNonQuery()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "ExecuteNonQuery", dur)))
            {
                return command.ExecuteNonQuery();
            }
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, string.Format("ExecuteReader({0})", behavior), dur)))
            {
                return command.ExecuteReader(behavior);
            }
        }

        public IDataReader ExecuteReader()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "ExecuteReader", dur)))
            {
                return command.ExecuteReader();
            }
        }

        public object ExecuteScalar()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "ExecuteScalar", dur)))
            {
                return command.ExecuteScalar();
            }
        }

        public IDataParameterCollection Parameters
        {
            get { return command.Parameters; }
        }

        public void Prepare()
        {
            command.Prepare();
        }

        public IDbTransaction Transaction
        {
            get { return command.Transaction; }
            set { command.Transaction = value is DbTransactionProxy ? ((DbTransactionProxy)value).transaction : value; }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get { return command.UpdatedRowSource; }
            set { command.UpdatedRowSource = value; }
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