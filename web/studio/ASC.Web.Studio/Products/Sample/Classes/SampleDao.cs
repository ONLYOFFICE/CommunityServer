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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.Sample.Classes
{
    public static class SampleDao
    {
        private const string DbId = "core";
        private const string Table = "sample_table";
        private static bool _tableExist;

        private static DbManager GetDb()
        {
            return new DbManager(DbId);
        }


        public static void CheckTable()
        {
            if (_tableExist) return;
            
            _tableExist = CheckTableExist();

            if (_tableExist) return;

            CreateTable();

            _tableExist = true;
        }

        private static bool CheckTableExist()
        {
            using (var db = GetDb())
            {
                var query = string.Format(
                    "select count(*) from information_schema.tables where table_schema = '{0}' and table_name = '{1}' limit 1",
                    db.Connection.Database,
                    Table);

                var count = db.ExecuteScalar<int>(query);

                return count > 0;
            }
        }

        private static void CreateTable()
        {
            using (var db = GetDb())
            {
                var query = new SqlCreate.Table(Table, true)
                .AddColumn(new SqlCreate.Column("id", DbType.Int32).NotNull(true).Autoincrement(true).PrimaryKey(true))
                .AddColumn("value", DbType.String, 255, true);

                db.ExecuteNonQuery(query);
            }
        }


        public static SampleClass Create(string value)
        {
            var result = new SampleClass
                {
                    Value = value
                };
            
            using (var db = GetDb())
            {
                var query = new SqlInsert(Table, true)
                    .InColumnValue("id", 0)
                    .InColumnValue("value", value)
                    .Identity(0, 0, true);
                
                result.Id = db.ExecuteScalar<int>(query);
            }

            return result;
        }

        public static SampleClass Read(int id)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(Table)
                    .Select("id", "value")
                    .Where(Exp.Eq("id", id));

                var result = db.ExecuteList(query).ConvertAll(x => new SampleClass
                    {
                        Id = Convert.ToInt32(x[0]),
                        Value = Convert.ToString(x[1])
                    });

                return result.Count > 0 ? result[0] : null;
            }
        }

        public static List<SampleClass> Read()
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(Table)
                    .Select("id", "value");

                return db.ExecuteList(query).ConvertAll(x => new SampleClass
                {
                    Id = Convert.ToInt32(x[0]),
                    Value = Convert.ToString(x[1])
                });
            }
        }

        public static void Update(int id, string value)
        {
            using (var db = GetDb())
            {
                var existQuery = new SqlQuery(Table).SelectCount().Where(Exp.Eq("id", id));

                if (db.ExecuteScalar<int>(existQuery) == 0)
                    throw new Exception("item not found");

                var updateQuery = new SqlUpdate(Table)
                    .Set("value", value)
                    .Where(Exp.Eq("id", id));

                db.ExecuteNonQuery(updateQuery);
            }
        }

        public static void Delete(int id)
        {
            using (var db = GetDb())
            {
                var query = new SqlDelete(Table).Where("id", id);

                db.ExecuteNonQuery(query);
            }
        }
    }
}