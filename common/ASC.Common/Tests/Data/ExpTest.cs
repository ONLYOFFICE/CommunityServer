/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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