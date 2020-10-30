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


/*#if DEBUG
using ASC.Common.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;

namespace ASC.Common.Tests.Data
{
    [TestClass]
    public class DataTest
    {
        private string dbId = Guid.NewGuid().ToString();
        private string cs = "Data Source=dbtest.sqlite;Version=3";

        public DataTest()
        {
            DbRegistry.RegisterDatabase(dbId, new SQLiteFactory(), cs);
        }

        [TestMethod]
        public void RegistryTest()
        {
            Assert.AreEqual(cs, DbRegistry.GetConnectionString(dbId));
            Assert.IsTrue(DbRegistry.IsDatabaseRegistered(dbId));
            Assert.IsNotNull(DbRegistry.CreateDbConnection(dbId));
        }

        [TestMethod]
        public void DbTransactionTest()
        {
            var dbManager = new DbManager(dbId);
            dbManager.ExecuteNonQuery("create table if not exists a(c1 TEXT)", null);

            var tx = dbManager.BeginTransaction();
            dbManager.ExecuteNonQuery("insert into a(c1) values (?)", "s");
            dbManager.ExecuteNonQuery("insert into a(c1) values (?)", "s2");
            tx.Dispose();

            dbManager.ExecuteNonQuery("insert into a(c1) values (?)", "s3");
        }

        [TestMethod]
        public void GroupConcatTest()
        {
            using (var connect = new SQLiteConnection("Data Source=:memory:"))
            {
                connect.Open();
                var command = new SQLiteCommand("create table a(c1 TEXT)", connect);
                command.ExecuteNonQuery();

                command.CommandText = "insert into a values (NULL);insert into a values ('x');insert into a values ('y');";
                command.ExecuteNonQuery();

                command.CommandText = "select group_concat(c1, 4) from a";
                var result1 = command.ExecuteScalar<string>();
                Assert.AreEqual("x4y", result1);

                command.CommandText = "select group_concat(c1) from a";
                var result2 = command.ExecuteScalar<string>();
                Assert.AreEqual("x,y", result2);

                command.CommandText = "select group_concat(c1) from a where 1 = 0";
                var result3 = command.ExecuteScalar<string>();
                Assert.AreEqual(null, result3);

                command.CommandText = "select group_concat(c1, NULL) from a";
                var result4 = command.ExecuteScalar<string>();
                Assert.AreEqual("x,y", result4);

                command.CommandText = "select concat(1, NULL, '4566')";
                var result5 = command.ExecuteScalar<string>();
                Assert.AreEqual("14566", result5);

                command.CommandText = "select concat()";
                var result6 = command.ExecuteScalar<string>();
                Assert.AreEqual(null, result6);

                command.CommandText = "select concat_ws(',', 45, 77)";
                var result7 = command.ExecuteScalar<string>();
                Assert.AreEqual("45,77", result7);
            }
        }

        [TestMethod]
        public void ExecuteScalarTest()
        {
            using (var connect = new SQLiteConnection("Data Source=:memory:"))
            {
                connect.Open();
                var command = new SQLiteCommand("create table a(c1 TEXT, c2 DATETIME)", connect);
                command.ExecuteNonQuery();

                command.CommandText = "insert into a values (NULL, '2012-01-01 00:00:00');insert into a values ('x', NULL);insert into a values ('y', '2012-01-02 00:00:00');";
                command.ExecuteNonQuery();

                var value1 = command.ExecuteScalar<object>("select c1 from a where c1 = 'x'");
                Assert.AreEqual("x", value1);

                var value2 = command.ExecuteScalar<string>("select c1 from a where c1 = 'x'");
                Assert.AreEqual("x", value2);

                var value3 = command.ExecuteScalar<object>("select c1 from a where c1 = 'xxx'");
                Assert.IsNull(value3);

                var value4 = command.ExecuteScalar<DateTime>("select c2 from a where c1 = 'y'");
                Assert.AreEqual(new DateTime(2012, 1, 2), value4);

                var value5 = command.ExecuteScalar<DateTime>("select c2 from a where c1 = 'x'");
                Assert.AreEqual(DateTime.MinValue, value5);

                var value6 = command.ExecuteScalar<DateTime?>("select c2 from a where c1 = 'y'");
                Assert.AreEqual(new DateTime(2012, 1, 2), value6);

                var value7 = command.ExecuteScalar<DateTime?>("select c2 from a where c1 = 'x'");
                Assert.IsNull(value7);
            }
        }

        [TestMethod]
        public void ExecuteListWithParamsTest()
        {
            using (var connect = new SQLiteConnection("Data Source=:memory:"))
            {
                connect.Open();
                connect.ExecuteList("select @p", new { p = "ok" });
            }
        }

        [TestMethod]
        public void MySqlUnnamedParametersTest()
        {
            using (var db = new DbManager("core"))
            {
                db.ExecuteNonQuery("insert into _test(c2,c3,c4) values (?,?,?),(?,?,?)", "sdfaf", "eryrtyre", 4, "a", "aa", 5);
            }
        }
    }
}
#endif*/