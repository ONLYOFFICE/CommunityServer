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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using ASC.Common.Data.Sql;

namespace ASC.Common.Data
{
    public interface IDbManager : IDisposable
    {
        DbConnection Connection { get; }
        string DatabaseId { get; }
        bool InTransaction { get; }

        IDbTransaction BeginTransaction();

        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);

        IDbTransaction BeginTransaction(bool nestedIfAlreadyOpen);
        
        List<object[]> ExecuteList(string sql, params object[] parameters);

        List<object[]> ExecuteList(ISqlInstruction sql);

        Task<List<object[]>> ExecuteListAsync(ISqlInstruction sql);

        List<T> ExecuteList<T>(ISqlInstruction sql, Converter<IDataRecord, T> converter);

        T ExecuteScalar<T>(string sql, params object[] parameters);

        T ExecuteScalar<T>(ISqlInstruction sql);

        int ExecuteNonQuery(string sql, params object[] parameters);

        int ExecuteNonQuery(ISqlInstruction sql);

        int ExecuteBatch(IEnumerable<ISqlInstruction> batch);
    }
}