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
using System.Configuration;
using System.IO;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

using TMResourceData.Model;

namespace TMResourceData
{
    public static class ResourceData
    {
        public static void AddCulture(string cultureTitle, string name)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sqlInsert = new SqlInsert("res_cultures");
                sqlInsert.InColumnValue("title", cultureTitle).InColumnValue("value", name);
                dbManager.ExecuteNonQuery(sqlInsert);
            }
        }

        public static void AddResource(string cultureTitle, string resType, DateTime date, ResWord word, bool isConsole, string authorLogin, bool updateIfExist = true)
        {
            using (var db = new DbManager("tmresource"))
            {
                var resData = db.ExecuteScalar<string>(GetQuery("res_data", cultureTitle, word));
                var resReserve = db.ExecuteScalar<string>(GetQuery("res_reserve", cultureTitle, word)); 

                //нет ключа
                if (string.IsNullOrEmpty(resData))
                {
                    //добавляем в основную таблицу
                    db.ExecuteNonQuery(Insert("res_data", cultureTitle, word)
                                              .InColumnValue("resourceType", resType)
                                              .InColumnValue("timechanges", date)
                                              .InColumnValue("flag", 2)
                                              .InColumnValue("authorLogin", authorLogin));

                    //добавляем в резервную таблицу
                    if (isConsole)  db.ExecuteNonQuery(Insert("res_reserve", cultureTitle, word));
                }
                else if(updateIfExist)
                {
                    var isChangeResData = resData != word.ValueFrom;
                    var isChangeResReserve = resReserve != word.ValueFrom;

                    //при работе с консолью изменилось по сравнению с res_data и res_reserve, либо при работе с сайтом изменилось по сравнению с res_reserve
                    if ((isConsole && isChangeResData && isChangeResReserve) || !isConsole)
                    {
                        // изменилась нейтральная культура - выставлен флаг у всех ключей из выбранного файла с выбранным title
                        if (cultureTitle == "Neutral")
                        {
                            var update = new SqlUpdate("res_data")
                                            .Set("flag", 3)
                                            .Where("fileID", word.ResFile.FileID)
                                            .Where("title", word.Title);

                            db.ExecuteNonQuery(update);
                        }
                        // изменилась не нейтральная культура 
                        db.ExecuteNonQuery(Insert("res_data", cultureTitle, word)
                                              .InColumnValue("resourceType", resType)
                                              .InColumnValue("timechanges", date)
                                              .InColumnValue("flag", 2)
                                              .InColumnValue("authorLogin", authorLogin));

                        if (isConsole) db.ExecuteNonQuery(Update("res_reserve", cultureTitle, word));
                    }
                    else if (isChangeResData)
                    {
                        db.ExecuteNonQuery(Update("res_reserve", cultureTitle, word));
                    }
                }
            }
        }

        private static SqlQuery GetQuery(string table,string cultureTitle, ResWord word)
        {
            return new SqlQuery(table)
                      .Select("textvalue")
                      .Where("fileID", word.ResFile.FileID)
                      .Where("cultureTitle", cultureTitle)
                      .Where("title", word.Title);

        }

        private static SqlUpdate Update(string table, string cultureTitle, ResWord word)
        {
            return new SqlUpdate(table)
                       .Set("flag", 2)
                       .Set("textvalue", word.ValueFrom)
                       .Where("fileID", word.ResFile.FileID)
                       .Where("title", word.Title)
                       .Where("cultureTitle", cultureTitle);

        }

        private static SqlInsert Insert(string table, string cultureTitle, ResWord word)
        {
            return new SqlInsert(table, true)
                    .InColumns(new[] { "title", "textvalue", "cultureTitle", "fileID" })
                    .Values(new object[] { word.Title, word.ValueFrom, cultureTitle, word.ResFile.FileID });

        }

        public static void EditEnglish(ResWord word)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var update = new SqlUpdate("res_data");

                update.Set("textvalue", word.ValueFrom).Where("fileID", word.ResFile.FileID).Where("title", word.Title).Where("cultureTitle", "Neutral");

                dbManager.ExecuteNonQuery(update);

            }
        }

        public static void AddComment(ResWord word)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sqlUpdate = new SqlUpdate("res_data");
                sqlUpdate.Set("description", word.TextComment).Where("title", word.Title).Where("fileID", word.ResFile.FileID).Where("cultureTitle", "Neutral");
                dbManager.ExecuteNonQuery(sqlUpdate);
            }
        }

        public static int AddFile(string fileName, string projectName, string moduleName)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (fileNameWithoutExtension != null && fileNameWithoutExtension.Split('.').Length > 1)
            {
                fileName = fileNameWithoutExtension.Split('.')[0] + Path.GetExtension(fileName);
            }

            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_files")
                    .SelectCount()
                    .Where("resName", fileName)
                    .Where("projectName", projectName)
                    .Where("moduleName", moduleName);

                if (dbManager.ExecuteScalar<int>(sql) == 0)
                {
                    var insert = new SqlInsert("res_files")
                        .InColumns(new[] { "resName", "projectName", "moduleName" })
                        .Values(new[] { fileName, projectName, moduleName });

                    dbManager.ExecuteNonQuery(insert);
                }

                var update = new SqlUpdate("res_files")
                    .Set("lastUpdate", DateTime.UtcNow.AddHours(4));
                dbManager.ExecuteNonQuery(update);

                sql = new SqlQuery("res_files")
                    .Select("id")
                    .Where("resName", fileName)
                    .Where("projectName", projectName)
                    .Where("moduleName", moduleName);
                return dbManager.ExecuteScalar<int>(sql);
            }
        }

        public static void UpdateDeletedData(int fileId, DateTime date)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sqlUpdate = new SqlUpdate("res_data");
                sqlUpdate.Set("flag", 4).Where("flag", 1).Where(Exp.Lt("TimeChanges", date)).Where("fileID", fileId);
                dbManager.ExecuteNonQuery(sqlUpdate);
            }
        }
    }
}