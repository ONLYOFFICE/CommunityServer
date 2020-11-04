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

namespace ASC.Common.Data
{
    class DbNestedTransaction : IDbTransaction
    {
        private readonly IDbTransaction transaction;


        public DbNestedTransaction(IDbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            this.transaction = transaction;
        }

        public IDbConnection Connection
        {
            get { return transaction.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return transaction.IsolationLevel; }
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        public void Dispose()
        {
        }
    }
}