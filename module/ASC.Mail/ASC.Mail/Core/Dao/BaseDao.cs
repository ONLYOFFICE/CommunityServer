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


using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public abstract class BaseDao
    {
        protected int Tenant { get; private set; }

        public IDbManager Db { get; private set; }

        public ITable Table { get; private set; }

        protected BaseDao(ITable table, IDbManager dbManager, int tenant = -1)
        {
            Tenant = tenant;
            Db = dbManager;
            Table = table;
        }

        protected SqlQuery Query(string alias = null)
        {
            if (!Table.OrderedColumnCollection.Any())
                throw new InvalidDataException("Table.OrderedColumnCollection is null or empty");

            if (string.IsNullOrEmpty(alias))
            {
                return new SqlQuery(Table.Name).Select(Table.OrderedColumnCollection.ToArray());
            }

            return
                new SqlQuery(Table.Name.Alias(alias)).Select(
                    Table.OrderedColumnCollection.Select(c => c.Prefix(alias)).ToArray());
        }
    }
}
