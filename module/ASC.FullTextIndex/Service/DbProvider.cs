/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.FullTextIndex.Service.Config;

namespace ASC.FullTextIndex.Service
{
    internal class DbProvider
    {
        public static bool CheckState()
        {
            using (var db = new DbManager("default"))
            {
                var last = db.ExecuteScalar<DateTime>(new SqlQuery("webstudio_index").SelectMax("last_modified"));
                var period = TextIndexCfg.ChangedCron.Period();
                if (!period.HasValue)
                    return false;
                return last.Add(period.Value) > DateTime.UtcNow;
            }
        }

        public static IEnumerable<string> GetIndexedModules()
        {
            using (var db = new DbManager("default"))
            {
                return db
                    .ExecuteList(new SqlQuery("webstudio_index").Select("index_name"))
                    .ConvertAll(r => (string)r[0]);
            }
        }

        public static void UpdateLastDeltaIndexDate(ModuleInfo module)
        {
            using (var db = new DbManager("default"))
            {
                db.ExecuteNonQuery(new SqlInsert("webstudio_index", true).InColumnValue("index_name", module.Delta));
            }
        }

        public static DateTime GetLastDeltaIndexDate(ModuleInfo module)
        {
            using (var db = new DbManager("default"))
            {
                return db.ExecuteScalar<DateTime>(new SqlQuery("webstudio_index").Select("last_modified").Where("index_name", module.Delta));
            }
        }

        public static IEnumerable<int> Search(ModuleInfo module)
        {
            using (var db = new DbManager(TextIndexCfg.ConnectionStringName))
            {
                return db.ExecuteList(module.SqlQuery).ConvertAll(r => GetId(r[0]));
            }
        }

        public static bool CheckDeltaIndexNotEmpty(ModuleInfo module)
        {
            using (var db = new DbManager(TextIndexCfg.ConnectionStringName))
            {
                var query = new SqlQuery(module.Delta).SelectCount();
                return db.ExecuteScalar<int>(query) > 0;
            }
        }

        private static int GetId(object r)
        {
            try
            {
                return Convert.ToInt32(r);
            }
            catch
            {
                return Convert.ToInt32(Convert.ToString(r).Split('_')[1]);
            }
        }
    }
}
