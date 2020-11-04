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
    class DbTransactionProxy : System.Data.Common.DbTransaction
    {
        private bool disposed;
        private readonly ProxyContext context;
        public readonly System.Data.Common.DbTransaction Transaction;

        public DbTransactionProxy(System.Data.Common.DbTransaction transaction, ProxyContext ctx)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (ctx == null) throw new ArgumentNullException("ctx");

            Transaction = transaction;
            context = ctx;
        }


        public override void Commit()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "Commit", dur)))
            {
                Transaction.Commit();
            }
        }

        protected override DbConnection DbConnection
        {
            get { return (DbConnection)Transaction.Connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return Transaction.IsolationLevel; }
        }

        public override void Rollback()
        {
            using (ExecuteHelper.Begin(dur => context.FireExecuteEvent(this, "Rollback", dur)))
            {
                Transaction.Rollback();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Transaction.Dispose();
                }
                disposed = true;
            }
        }

        ~DbTransactionProxy()
        {
            Dispose(false);
        }
    }
}