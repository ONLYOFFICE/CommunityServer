/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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