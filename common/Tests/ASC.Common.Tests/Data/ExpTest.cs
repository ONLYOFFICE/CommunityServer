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


#if DEBUG
using System.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Common.Tests.Data
{
    [TestClass]
    public class ExpTest
    {
        [TestMethod]
        public void JunctuinTest()
        {
            var exp = Exp.Eq("A", 0) & (Exp.Eq("B", 0) | Exp.Eq("C", 0));
            Assert.AreEqual(exp.ToString(), "A = ? and (B = ? or C = ?)");

            exp = Exp.Eq("A", 0) & (Exp.Eq("B", 0) & Exp.Eq("C", 0));
            Assert.AreEqual(exp.ToString(), "A = ? and B = ? and C = ?");

            exp = Exp.Eq("A", 0) | (Exp.Eq("B", 0) | Exp.Eq("C", 0));
            Assert.AreEqual(exp.ToString(), "A = ? or B = ? or C = ?");

            exp = (Exp.Eq("A", 0) & Exp.Eq("B", 0)) | Exp.Eq("C", 0);
            Assert.AreEqual(exp.ToString(), "(A = ? and B = ?) or C = ?");

            exp = (Exp.Eq("A", 0) & Exp.Eq("B", 0)) & Exp.Eq("C", 0) | Exp.Eq("D", 0);
            Assert.AreEqual(exp.ToString(), "(A = ? and B = ? and C = ?) or D = ?");

            exp = (Exp.Eq("A", 0) & Exp.Eq("B", 0)) | (Exp.Eq("C", 0) & Exp.Eq("D", 0));
            Assert.AreEqual(exp.ToString(), "(A = ? and B = ?) or (C = ? and D = ?)");

            exp = (Exp.Eq("A", 0) | Exp.Eq("B", 0)) & Exp.Eq("C", 0);
            Assert.AreEqual(exp.ToString(), "(A = ? or B = ?) and C = ?");

            exp = Exp.Eq("A", 0) | Exp.Eq("B", 0) & Exp.Eq("C", 0); // priority
            Assert.AreEqual(exp.ToString(), "A = ? or (B = ? and C = ?)");
        }

        [TestMethod]
        public void QueryTtest()
        {
            var query = new SqlQuery("Table1 t1")
                .From(new SqlQuery("Table2").Select("Id"), "t2")
                .Select("t1.Name")
                .Where(Exp.EqColumns("t1.Id", "t2.Id"));

            Assert.AreEqual("select t1.Name from Table1 t1, (select Id from Table2) as t2 where t1.Id = t2.Id", query.ToString());
        }

        [TestMethod]
        public void LGTest()
        {
            Assert.AreEqual("a < ?", Exp.Lt("a", 0).ToString());
            Assert.AreEqual("a <= ?", Exp.Le("a", 0).ToString());
            Assert.AreEqual("a > ?", Exp.Gt("a", 0).ToString());
            Assert.AreEqual("a >= ?", Exp.Ge("a", 0).ToString());
        }

        [TestMethod]
        public void InTest()
        {
            Assert.AreEqual("a = ?", Exp.In("a", new[] { 1 }).ToString());
            Assert.AreEqual("1 = 0", Exp.In("a", new int[0]).ToString());
            Assert.AreEqual("a in (?,?)", Exp.In("a", new[] { 1, 2 }).ToString());
            Assert.AreEqual("a in (select c)", Exp.In("a", new SqlQuery().Select("c")).ToString());

            Assert.AreEqual("a <> ?", (!Exp.In("a", new[] { 1 })).ToString());
            Assert.AreEqual("1 <> 0", (!Exp.In("a", new int[0])).ToString());
            Assert.AreEqual("a not in (?,?)", (!Exp.In("a", new[] { 1, 2 })).ToString());
            Assert.AreEqual("a not in (select c)", (!Exp.In("a", new SqlQuery().Select("c"))).ToString());
        }

        [TestMethod]
        public void NullTest()
        {
            Assert.AreEqual("a is null", Exp.Eq("a", null).ToString());
            Assert.AreEqual("a is not null", (!Exp.Eq("a", null)).ToString());
        }

        [TestMethod]
        public void SqlInsertTest()
        {
            var i = new SqlInsert("Table").InColumnValue("c1", 1);
            Assert.AreEqual("insert into Table(c1) values (?)", i.ToString());

            i = new SqlInsert("Table").ReplaceExists(true).InColumnValue("c1", 1);
            Assert.AreEqual("replace into Table(c1) values (?)", i.ToString());

            i = new SqlInsert("Table").IgnoreExists(true).InColumnValue("c1", 1);
            Assert.AreEqual("insert ignore into Table(c1) values (?)", i.ToString());

            i = new SqlInsert("Table").InColumns("c1", "c2").Values(1, 2);
            Assert.AreEqual("insert into Table(c1,c2) values (?,?)", i.ToString());

            i = new SqlInsert("Table").InColumns("c1", "c2").Values(1, 2, 3, 4);
            Assert.AreEqual("insert into Table(c1,c2) values (?,?),(?,?)", i.ToString());

            i = new SqlInsert("Table").InColumns("c1", "c2").Values(0, 2).Identity(0, 0, true);
            Assert.AreEqual("insert into Table(c2) values (?); select @@identity", i.ToString());

            i = new SqlInsert("Table").Values(0, 2);
            Assert.AreEqual("insert into Table values (?,?)", i.ToString());
        }

        [TestMethod]
        public void SqlUpdateTest()
        {
            var update = new SqlUpdate("Table")
                .Set("Column1", 1)
                .Set("Column1", 2)
                .Set("Column2", 3)
                .Set("Column3 = Column3 + 2");

            Assert.AreEqual("update Table set Column1 = ?, Column2 = ?, Column3 = Column3 + 2", update.ToString());

            update = new SqlUpdate("Table")
                .Set("Column1", 1)
                .Set("Column1", 2)
                .Set("Column3", new SqlQuery("Table2").Select("x").Where("y", 5));

            Assert.AreEqual("update Table set Column1 = ?, Column3 = (select x from Table2 where y = ?)", update.ToString());
        }

        [TestMethod]
        public void SqlUnionTest()
        {
            var union = new SqlQuery("t1").Select("c1").Where("c1", 4)
                .Union(new SqlQuery("t2").Select("c2").Where("c2", 7));

            Assert.AreEqual("select c1 from t1 where c1 = ? union select c2 from t2 where c2 = ?", union.ToString());
            Assert.AreEqual(4, union.GetParameters()[0]);
            Assert.AreEqual(7, union.GetParameters()[1]);
        }

        [TestMethod]
        public void SqlCreateTableTest()
        {
            var q = new SqlCreate.Table("t1")
                    .AddColumn("c1", DbType.String, 255);
            Assert.AreEqual("create table t1 (c1 string(255) null);\r\n", q.ToString());
        }
    }
}
#endif