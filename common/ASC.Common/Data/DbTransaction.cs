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


#region usings

using System;
using System.Data;

#endregion

namespace ASC.Common.Data
{
    public class DbTransaction : IDbTransaction
    {
        public DbTransaction(IDbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            Transaction = transaction;
        }

        internal IDbTransaction Transaction { get; private set; }

        #region IDbTransaction Members

        public IDbConnection Connection
        {
            get { return Transaction.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return Transaction.IsolationLevel; }
        }

        public void Commit()
        {
            try
            {
                Transaction.Commit();
            }
            finally
            {
                OnUnavailable();
            }
        }

        public void Rollback()
        {
            try
            {
                Transaction.Rollback();
            }
            finally
            {
                OnUnavailable();
            }
        }

        public void Dispose()
        {
            try
            {
                Transaction.Dispose();
            }
            finally
            {
                OnUnavailable();
            }
        }

        #endregion

        public event EventHandler Unavailable;

        private void OnUnavailable()
        {
            try
            {
                if (Unavailable != null) Unavailable(this, EventArgs.Empty);
            }
            catch
            {
            }
        }
    }
}