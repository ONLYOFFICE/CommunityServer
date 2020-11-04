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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using TMResourceData.Model;

namespace TMResourceData
{
    public static class ResourceData
    {
        private const string Dbid = "tmresource";
        private const string ResDataTable = "res_data";
        private const string ResCultureTable = "res_cultures";
        private const string ResFilesTable = "res_files";
        private const string ResReserveTable = "res_reserve";
        private const string ResAuthorsTable = "res_authors";
        private const string ResAuthorsLangTable = "res_authorslang";
        private const string ResAuthorsFileTable = "res_authorsfile";

        public static DateTime GetLastUpdate()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResFilesTable).SelectMax("LastUpdate");

                return dbManager.ExecuteScalar<DateTime>(sql);
            }
        }

        public static List<ResCulture> GetListLanguages(int fileId, string title)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResCultureTable)
                    .Select("res_cultures.title", "res_cultures.value", "res_cultures.available")
                    .LeftOuterJoin("res_data", Exp.EqColumns("res_cultures.title", "res_data.cultureTitle"))
                    .Where("res_data.fileID", fileId)
                    .Where("res_data.title", title);

                var language = dbManager.ExecuteList(sql).ConvertAll(GetCultureFromDB);

                language.Remove(language.Find(p => p.Title == "Neutral"));

                return language;
            }
        }

        public static Dictionary<ResCulture, List<string>> GetCulturesWithAuthors()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_authorslang ral")
                    .Select(new[] { "ral.authorLogin", "rc.title", "rc.value" })
                    .InnerJoin("res_cultures rc", Exp.EqColumns("rc.title", "ral.cultureTitle"))
                    .InnerJoin("res_authors ra", Exp.EqColumns("ra.login", "ral.authorLogin"))
                    .Where("ra.isAdmin", false);

                return dbManager.ExecuteList(sql)
                                .GroupBy(r => new ResCulture { Title = (string)r[1], Value = (string)r[2] }, r => (string)r[0])
                                .ToDictionary(r => r.Key, r => r.ToList());
            }
        }

        public static void AddCulture(string cultureTitle, string name)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sqlInsert = new SqlInsert(ResCultureTable);
                sqlInsert.InColumnValue("title", cultureTitle).InColumnValue("value", name);
                dbManager.ExecuteNonQuery(sqlInsert);
            }
        }

        public static void AddResource(string cultureTitle, string resType, DateTime date, ResWord word, bool isConsole, string authorLogin, bool updateIfExist = true)
        {
            using (var db = new DbManager(Dbid))
            {
                var resData = db.ExecuteScalar<string>(GetQuery(ResDataTable, cultureTitle, word));
                var resReserve = db.ExecuteScalar<string>(GetQuery(ResReserveTable, cultureTitle, word));

                //нет ключа
                if (string.IsNullOrEmpty(resData))
                {
                    //добавляем в основную таблицу
                    db.ExecuteNonQuery(Insert(ResDataTable, cultureTitle, word)
                                              .InColumnValue("resourceType", resType)
                                              .InColumnValue("timechanges", date)
                                              .InColumnValue("flag", 2)
                                              .InColumnValue("authorLogin", authorLogin));

                    //добавляем в резервную таблицу
                    if (isConsole) db.ExecuteNonQuery(Insert(ResReserveTable, cultureTitle, word));
                }
                else
                {
                    if (cultureTitle == "Neutral" && isConsole)
                    {
                        updateIfExist = db.ExecuteScalar<int>(new SqlQuery(ResDataTable)
                            .SelectCount()
                            .Where("fileID", word.ResFile.FileID)
                            .Where(!Exp.Eq("cultureTitle", cultureTitle))
                            .Where("title", word.Title)) == 0;
                    }

                    var isChangeResData = resData != word.ValueFrom;
                    var isChangeResReserve = resReserve != word.ValueFrom;

                    if (!updateIfExist) return;

                    //при работе с консолью изменилось по сравнению с res_data и res_reserve, либо при работе с сайтом изменилось по сравнению с res_reserve
                    if ((isConsole && isChangeResData && isChangeResReserve) || !isConsole)
                    {
                        // изменилась нейтральная культура - выставлен флаг у всех ключей из выбранного файла с выбранным title
                        if (cultureTitle == "Neutral")
                        {
                            var update = new SqlUpdate(ResDataTable)
                                            .Set("flag", 3)
                                            .Where("fileID", word.ResFile.FileID)
                                            .Where("title", word.Title);

                            db.ExecuteNonQuery(update);
                        }
                        // изменилась не нейтральная культура 
                        db.ExecuteNonQuery(Insert(ResDataTable, cultureTitle, word)
                                              .InColumnValue("resourceType", resType)
                                              .InColumnValue("timechanges", date)
                                              .InColumnValue("flag", 2)
                                              .InColumnValue("authorLogin", authorLogin));

                        if (isConsole) db.ExecuteNonQuery(Update(ResReserveTable, cultureTitle, word));
                    }
                    else if (isChangeResData)
                    {
                        db.ExecuteNonQuery(Update(ResReserveTable, cultureTitle, word));
                    }
                }
            }
        }

        public static void EditEnglish(ResWord word)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var update = new SqlUpdate(ResDataTable);
                update.Set("textvalue", word.ValueFrom).Where("fileID", word.ResFile.FileID).Where("title", word.Title).Where("cultureTitle", "Neutral");
                dbManager.ExecuteNonQuery(update);

            }
        }

        public static void AddComment(ResWord word)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sqlUpdate = new SqlUpdate(ResDataTable);
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

            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResFilesTable)
                    .SelectCount()
                    .Where("resName", fileName)
                    .Where("projectName", projectName)
                    .Where("moduleName", moduleName);

                if (dbManager.ExecuteScalar<int>(sql) == 0)
                {
                    var insert = new SqlInsert(ResFilesTable)
                        .InColumns("resName", "projectName", "moduleName")
                        .Values(fileName, projectName, moduleName);

                    dbManager.ExecuteNonQuery(insert);
                }

                var update = new SqlUpdate(ResFilesTable)
                    .Set("lastUpdate", DateTime.UtcNow.AddHours(4));
                dbManager.ExecuteNonQuery(update);

                sql = new SqlQuery(ResFilesTable)
                    .Select("id")
                    .Where("resName", fileName)
                    .Where("projectName", projectName)
                    .Where("moduleName", moduleName);
                return dbManager.ExecuteScalar<int>(sql);
            }
        }


        public static IEnumerable<ResCulture> GetCultures()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResCultureTable);
                sql.Select("title", "value", "available")
                   .OrderBy("title", true);

                return dbManager.ExecuteList(sql).ConvertAll(GetCultureFromDB);
            }
        }

        public static void SetCultureAvailable(string title)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlUpdate(ResCultureTable);
                sql.Set("available", true).Where("title", title);
                dbManager.ExecuteNonQuery(sql);
            }
        }

        private static ResCulture GetCultureFromDB(IList<object> r)
        {
            return new ResCulture { Title = (string)r[0], Value = (string)r[1], Available = (bool)r[2] };
        }

        public static List<ResFile> GetAllFiles()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResFilesTable);

                return dbManager.ExecuteList(sql.SelectAll()).Select(r => new ResFile
                {
                    FileID = (int)r[0],
                    ProjectName = (string)r[1],
                    ModuleName = (string)r[2],
                    FileName = (string)r[3]
                }).ToList();
            }
        }

        public static IEnumerable<ResProject> GetResProjects()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResFilesTable);
                sql.Select("projectName").Distinct();

                var projects = dbManager.ExecuteList(sql).Select(r => new ResProject { Name = (string)r[0] }).ToList();

                foreach (var resProject in projects)
                {
                    sql = new SqlQuery(ResFilesTable)
                        .Select("moduleName", "islock")
                        .Where("projectName", resProject.Name)
                        .Distinct();

                    resProject.Modules = dbManager.ExecuteList(sql).Select(r => new ResModule { Name = (string)r[0], IsLock = (bool)r[1] }).ToList();
                }

                return projects;
            }
        }

        public static IEnumerable<ResWord> GetListResWords(ResCurrent current, string search)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var exist = new SqlQuery(ResDataTable + " rd3")
                    .Select("rd3.title")
                    .Where("rd3.fileid = rd1.fileid")
                    .Where("rd3.title = concat('del_', rd1.title)")
                    .Where("rd3.cultureTitle = rd1.cultureTitle");

                var sql = new SqlQuery(ResDataTable + " rd1")
                    .Select("rd1.title", "rd1.fileid", "rd1.textValue", "rd1.description", "rd1.flag", "rd1.link", "rf.resName", "rd2.id", "rd2.flag", "rd2.textValue")
                    .LeftOuterJoin(ResDataTable + " rd2", Exp.EqColumns("rd1.fileid", "rd2.fileid") & Exp.EqColumns("rd1.title", "rd2.title") & Exp.Eq("rd2.cultureTitle", current.Language.Title))
                    .InnerJoin(ResFilesTable + " rf", Exp.EqColumns("rf.ID", "rd1.fileID"))
                    .Where("rf.moduleName", current.Module.Name)
                    .Where("rf.projectName", current.Project.Name)
                    .Where("rd1.cultureTitle", "Neutral")
                    .Where("rd1.flag != 4")
                    .Where("rd1.resourceType", "text")
                    .Where(!Exp.Like("rd1.title", @"del\_", SqlLike.StartWith) & !Exp.Exists(exist))
                    .OrderBy("rd1.id", true);

                if (!String.IsNullOrEmpty(search))
                    sql.Where(Exp.Like("rd1.textvalue", search));

                return dbManager.ExecuteList(sql).ConvertAll(r =>
                {
                    var word = GetWord(r);
                    word.ResFile.FileName = Convert.ToString(r[6]);

                    if (r[7] != null)
                    {
                        word.Status = (int)r[8] == 3 ? WordStatusEnum.Changed : WordStatusEnum.Translated;
                        word.ValueTo = Convert.ToString(r[9]);
                    }
                    else
                    {
                        word.Status = WordStatusEnum.Untranslated;
                    }

                    return word;
                }).OrderBy(r => r.ValueFrom);
            }
        }

        public static List<ResWord> GetListResWords(ResFile resFile, string to, string search)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable)
                    .Select("title", "fileid", "textValue", "description", "flag", "link")
                    .InnerJoin(ResFilesTable, Exp.EqColumns(ResFilesTable + ".ID", ResDataTable + ".fileID"))
                    .Where("moduleName", resFile.ModuleName)
                    .Where("projectName", resFile.ProjectName)
                    .Where("cultureTitle", to)
                    .Where("flag != 4")
                    .Where("resourceType", "text")
                    .OrderBy(ResDataTable + ".id", true);

                if (!String.IsNullOrEmpty(resFile.FileName))
                    sql.Where("resName", resFile.FileName);

                if (!String.IsNullOrEmpty(search))
                    sql.Where(Exp.Like("textvalue", search));

                return dbManager.ExecuteList(sql).ConvertAll(GetWord);
            }
        }

        public static void GetListModules(ResCurrent currentData)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var notExist = new SqlQuery(ResDataTable + " rd1")
                    .Select("1")
                    .Where("rd1.fileid = rd.fileid")
                    .Where("rd1.title = concat('del_', rd.title)")
                    .Where("rd1.cultureTitle = 'Neutral'");

                var exist = new SqlQuery(ResDataTable + " rd2")
                    .Select("1")
                    .Where("rd2.fileid = rd.fileid")
                    .Where("rd2.title = rd.title")
                    .Where("rd2.cultureTitle = 'Neutral'");

                var sql = new SqlQuery(ResFilesTable + " rf").Select("rf.moduleName",
                                                              string.Format("sum(case rd.cultureTitle when '{0}' then (case rd.flag when 3 then 0 else 1 end) else 0 end)", currentData.Language.Title),
                                                              string.Format("sum(case rd.cultureTitle when '{0}' then (case rd.flag when 3 then 1 else 0 end) else 0 end)", currentData.Language.Title),
                                                              string.Format("sum(case rd.cultureTitle when '{0}' then 1 else 0 end)", "Neutral"))
                                                      .InnerJoin(ResDataTable + " rd", Exp.EqColumns("rd.fileid", "rf.id"))
                                                      .Where("rf.projectName", currentData.Project.Name)
                                                      .Where("rd.resourceType", "text")
                                                      .Where(!Exp.Like("rd.title", @"del\_", SqlLike.StartWith) & Exp.Exists(exist) & !Exp.Exists(notExist))
                                                      .GroupBy("moduleName");


                dbManager.ExecuteList(sql).ForEach(r =>
                {
                    var module = currentData.Project.Modules.Find(mod => mod.Name == r[0].ToString());
                    if (module == null) return;
                    module.Counts[WordStatusEnum.Translated] = Convert.ToInt32(r[1]);
                    module.Counts[WordStatusEnum.Changed] = Convert.ToInt32(r[2]);
                    module.Counts[WordStatusEnum.All] = Convert.ToInt32(r[3]);
                    module.Counts[WordStatusEnum.Untranslated] = module.Counts[WordStatusEnum.All] - module.Counts[WordStatusEnum.Changed] - module.Counts[WordStatusEnum.Translated];
                });
            }
        }

        public static void LockModules(string projectName, string modules)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sqlUpdate = new SqlUpdate(ResFilesTable);
                sqlUpdate.Set("isLock", 1).Where("projectName", projectName).Where(Exp.In("moduleName", modules.Split(',')));
                dbManager.ExecuteNonQuery(sqlUpdate);
            }
        }

        public static void UnLockModules()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sqlUpdate = new SqlUpdate(ResFilesTable);
                sqlUpdate.Set("isLock", 0);
                dbManager.ExecuteNonQuery(sqlUpdate);
            }
        }

        public static void AddLink(string resource, string fileName, string page)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var query = new SqlQuery(ResDataTable);
                query.Select(ResDataTable + ".id")
                     .InnerJoin(ResFilesTable, Exp.EqColumns(ResFilesTable + ".id", ResDataTable + ".fileid"))
                     .Where(ResDataTable + ".title", resource).Where(ResFilesTable + ".resName", fileName).Where(ResDataTable + ".cultureTitle", "Neutral");

                var key = dbManager.ExecuteScalar<int>(query);

                var update = new SqlUpdate(ResDataTable);
                update.Set("link", page).Where("id", key);
                dbManager.ExecuteNonQuery(update);
            }
        }

        public static void GetResWordByKey(ResWord word, string to)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable)
                    .Select("textvalue", "description", "link")
                    .Where("fileID", word.ResFile.FileID)
                    .Where("cultureTitle", "Neutral")
                    .Where("title", word.Title);

                dbManager.ExecuteList(sql).ForEach(r => GetValue(word, to, r));

                GetValueByKey(word, to);

                sql = new SqlQuery(ResDataTable + " as res1").Select("res1.textvalue").Distinct()
                                                      .InnerJoin(ResDataTable + " as res2", Exp.EqColumns("res1.title", "res2.title") & Exp.EqColumns("res1.fileid", "res2.fileid"))
                                                      .Where("res1.cultureTitle", to)
                                                      .Where("res2.cultureTitle", "Neutral")
                                                      .Where("res2.textvalue", word.ValueFrom);

                word.Alternative = new List<string>();
                dbManager.ExecuteList(sql).ForEach(r => word.Alternative.Add((string)r[0]));
                word.Alternative.Remove(word.ValueTo);

                sql = new SqlQuery(ResFilesTable)
                    .Select("resname")
                    .Where("id", word.ResFile.FileID);

                word.ResFile.FileName = dbManager.ExecuteScalar<string>(sql);
            }
        }

        public static void GetValueByKey(ResWord word, string to)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable);
                sql.Select("textvalue")
                   .Where("fileID", word.ResFile.FileID)
                   .Where("cultureTitle", to)
                   .Where("title", word.Title);

                word.ValueTo = dbManager.ExecuteScalar<string>(sql) ?? "";
            }
        }

        private static void GetValue(ResWord word, string to, IList<object> r)
        {
            word.ValueFrom = (string)r[0] ?? "";
            word.TextComment = (string)r[1] ?? "";

            var langs = (ConfigurationManagerExtension.AppSettings["resources.com-lang"] ?? string.Empty).Split(';').ToList();
            var dom = langs.Exists(lang => lang == to) ? ".info" : ".com";

            word.Link = !String.IsNullOrEmpty((string)r[2]) ? String.Format("http://{0}-translator.teamlab{1}{2}", to, dom, r[2]) : "";
        }

        public static List<Author> GetListAuthors()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResAuthorsTable)
                    .Select("login", "password", "isAdmin");

                return dbManager.ExecuteList(sql).ConvertAll(r => new Author { Login = (string)r[0], Password = (string)r[1], IsAdmin = Convert.ToBoolean(r[2]) });
            }
        }

        public static Author GetAuthor(string login)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResAuthorsTable)
                    .Select("login", "password", "isAdmin")
                    .Where("login", login);

                var author = dbManager.ExecuteList(sql)
                                      .ConvertAll(r => new Author
                                      {
                                          Login = (string)r[0],
                                          Password = (string)r[1],
                                          IsAdmin = Convert.ToBoolean(r[2])
                                      }).FirstOrDefault();

                if (author != null)
                {
                    sql = new SqlQuery("res_cultures rc")
                        .Select("rc.title", "rc.value", "rc.available")
                        .InnerJoin(ResAuthorsLangTable + " ral", Exp.EqColumns("rc.title", "ral.cultureTitle"))
                        .Where("ral.authorLogin", author.Login);

                    author.Langs = dbManager.ExecuteList(sql).ConvertAll(GetCultureFromDB);

                    sql = new SqlQuery(ResFilesTable + " rf")
                        .Select("rf.projectName").Distinct()
                        .InnerJoin(ResAuthorsFileTable + " raf", Exp.EqColumns("raf.fileid", "rf.id"))
                        .Where("raf.authorlogin", login)
                        .Where("rf.isLock", 0);

                    var projects = dbManager.ExecuteList(sql).Select(r => new ResProject { Name = (string)r[0] }).ToList();

                    foreach (var resProject in projects)
                    {
                        sql = new SqlQuery(ResFilesTable + " rf")
                            .Select("rf.moduleName").Distinct()
                            .InnerJoin(ResAuthorsFileTable + " raf", Exp.EqColumns("raf.fileid", "rf.id"))
                            .Where("rf.projectName", resProject.Name)
                            .Where("raf.authorlogin", login)
                            .Where("rf.isLock", 0);
                        resProject.Modules = dbManager.ExecuteList(sql).Select(r => new ResModule { Name = (string)r[0] }).ToList();
                    }

                    author.Projects = projects;
                }

                return author;
            }
        }

        public static void CreateAuthor(Author author, IEnumerable<string> languages, string modules)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sqlInsert = new SqlInsert(ResAuthorsTable, true)
                    .InColumnValue("login", author.Login)
                    .InColumnValue("password", author.Password)
                    .InColumnValue("isAdmin", author.IsAdmin);

                dbManager.ExecuteNonQuery(sqlInsert);

                var delete = new SqlDelete(ResAuthorsLangTable).Where("authorLogin", author.Login);
                dbManager.ExecuteNonQuery(delete);

                delete = new SqlDelete(ResAuthorsFileTable).Where("authorLogin", author.Login);
                dbManager.ExecuteNonQuery(delete);

                foreach (var lang in languages)
                {
                    sqlInsert = new SqlInsert(ResAuthorsLangTable, true)
                        .InColumnValue("authorLogin", author.Login)
                        .InColumnValue("cultureTitle", lang);

                    dbManager.ExecuteNonQuery(sqlInsert);
                }

                var resFiles = GetAllFiles();
                //project1:module1-access1,module2-access2;project2:module3-access3,module4-access4
                foreach (var projectData in modules.Split(';').Select(project => project.Split(':')))
                {
                    foreach (var mod in projectData[1].Split(','))
                    {
                        //var modData = mod.Split('-');
                        var fileid = resFiles.Where(r => r.ModuleName == mod && r.ProjectName == projectData[0]).Select(r => r.FileID).FirstOrDefault();
                        sqlInsert = new SqlInsert(ResAuthorsFileTable, true)
                            .InColumnValue("authorLogin", author.Login)
                            .InColumnValue("fileId", fileid); //.InColumnValue("writeAccess", Convert.ToBoolean(modData[1]));
                        dbManager.ExecuteNonQuery(sqlInsert);
                    }
                }
            }
        }

        public static void DeleteAuthor(string login)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlDelete(ResAuthorsTable).Where("login", login);

                dbManager.ExecuteNonQuery(sql);
            }
        }

        public static bool IsAuthor(string login, string password)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResAuthorsTable)
                    .SelectCount()
                    .Where("login", login)
                    .Where("password", password);

                return dbManager.ExecuteScalar<int>(sql) != 0;
            }
        }

        public static void SetAuthorOnline(string login)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlUpdate(ResAuthorsTable)
                    .Set("lastVisit", DateTime.UtcNow)
                    .Where("login", login);

                dbManager.ExecuteNonQuery(sql);
            }
        }

        public static List<string> GetOnlineAuthors()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResAuthorsTable)
                    .Select("login")
                    .Where(Exp.Ge("LastVisit", DateTime.UtcNow.AddMinutes(-5)));

                return dbManager.ExecuteList(sql).ConvertAll(r => (string)r[0]);
            }
        }

        public static void AddAuthorLang(string login, string cultureTitle)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlInsert(ResAuthorsLangTable)
                    .InColumnValue("cultureTitle", cultureTitle)
                    .InColumnValue("authorLogin", login);

                dbManager.ExecuteNonQuery(sql);
            }
        }

        public static List<StatisticModule> GetStatistic()
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable + " r1")
                    .SelectCount("r1.title")
                    .Select("rc.value", "rf.projectName")
                    .Select("sum(LENGTH(r2.textvalue) - LENGTH(REPLACE(r2.textvalue, ' ', '')) + 1)")
                    .InnerJoin(ResFilesTable + " as rf", Exp.EqColumns("rf.id", "r1.fileid"))
                    .InnerJoin(ResCultureTable + " as rc", Exp.EqColumns("r1.cultureTitle", "rc.title"))
                    .InnerJoin(ResDataTable + " as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                    .Where(Exp.Lt("r1.flag", 3))
                    .Where("r1.resourceType", "text")
                    .Where("rf.isLock", 0)
                    .Where("r2.cultureTitle", "Neutral")
                    .Where(!Exp.In("rf.id", new List<int>{259, 260, 261}))
                    .GroupBy("rc.value", "rf.projectName")
                    .OrderBy("rc.value", true)
                    .OrderBy("rf.projectName", true);

                var stat = dbManager.ExecuteList(sql);
                var allStat = new List<StatisticModule>();

                foreach (var culture in stat.Select(data => (string)data[1]).Distinct())
                {
                    var cultureData = new StatisticModule { Culture = culture };

                    foreach (var project in stat.Select(data => (string)data[2]).Distinct())
                    {
                        var data = stat.Where(r => (string) r[1] == culture && (string) r[2] == project).ToList();
                        cultureData.Counts.Add(project, new Tuple<int, int>(data.Sum(r => Convert.ToInt32(r[0])), data.Sum(r => Convert.ToInt32(r[3]))));
                    }

                    allStat.Add(cultureData);
                }

                return allStat;
            }
        }

        public static List<StatisticUser> GetUserStatisticForLang()
        {
            return GetUserStatisticForLang(DateTime.Now.Date, DateTime.Now.Date.AddDays(1));
        }

        public static List<StatisticUser> GetUserStatisticForLang(DateTime from, DateTime till)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable + " as r1");

                sql.SelectCount().Select(ResCultureTable + ".title", "r1.authorLogin", "sum(length(r2.textvalue))")
                   .InnerJoin(ResDataTable + " as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                   .InnerJoin(ResCultureTable, Exp.EqColumns("r1.cultureTitle", ResCultureTable + ".title"))
                   .Where(!Exp.Eq("r1.flag", 4))
                   .Where(!Exp.Eq("r1.flag", 3))
                   .Where(!Exp.Eq("r1.authorLogin", "Console"))
                   .Where(!Exp.Eq("r1.cultureTitle", "Neutral"))
                   .Where(Exp.Ge("r1.timeChanges", from))
                   .Where(Exp.Le("r1.timeChanges", till))
                   .Where("r2.cultureTitle", "Neutral")
                   .GroupBy("title", "authorLogin")
                   .OrderBy("title", true)
                   .OrderBy("authorLogin", true);

                return dbManager.ExecuteList(sql).ConvertAll(GetUserStatisticFromDB);
            }
        }

        public static List<StatisticUser> GetUserStatisticForModules(string login, DateTime from, DateTime till)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable + " as r1");

                sql.SelectCount().Select("concat_ws(':', res_files.projectName,res_files.moduleName)", "r1.authorLogin", "sum(length(r2.textvalue))")
                   .InnerJoin(ResDataTable + " as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                   .InnerJoin(ResFilesTable, Exp.EqColumns("r1.fileid", ResFilesTable + ".id"))
                   .Where(!Exp.Eq("r1.flag", 4))
                   .Where(!Exp.Eq("r1.flag", 3))
                   .Where(!Exp.Eq("r1.authorLogin", "Console"))
                   .Where(!Exp.Eq("r1.cultureTitle", "Neutral"))
                   .Where(Exp.Ge("r1.timeChanges", from))
                   .Where(Exp.Le("r1.timeChanges", till))
                   .Where("r2.cultureTitle", "Neutral")
                   .Where(Exp.Eq("r1.authorLogin", login))
                   .GroupBy("r1.fileid", "r1.authorLogin")
                   .OrderBy("r1.fileid", true)
                   .OrderBy("r1.authorLogin", true);

                return dbManager.ExecuteList(sql).Select(r => new StatisticUser { WordsCount = Convert.ToInt32(r[0]), Module = (string)r[1], Login = (string)r[2], SignCount = Convert.ToInt32(r[3]) }).ToList();
            }
        }

        private static StatisticUser GetUserStatisticFromDB(IList<object> r)
        {
            return new StatisticUser { WordsCount = Convert.ToInt32(r[0]), Culture = (string)r[1], Login = (string)r[2], SignCount = Convert.ToInt32(r[3]) };
        }

        public static List<ResWord> SearchAll(string projectName, string moduleName, string languageTo, string searchText, string searchType)
        {
            using (var dbManager = new DbManager(Dbid))
            {
                var sql = new SqlQuery(ResDataTable)
                    .Select("title", "fileid", "textValue", "resName", "moduleName", "projectName")
                    .InnerJoin(ResFilesTable, Exp.EqColumns(ResFilesTable + ".ID", ResDataTable + ".fileID"))
                    .Where("cultureTitle", languageTo)
                    .Where("flag != 4")
                    .Where(Exp.Like(searchType, searchText))
                    .Where("resourceType", "text")
                    .OrderBy("textValue", true);

                if (!string.IsNullOrEmpty(projectName) && projectName != "All")
                {
                    sql.Where("projectName", projectName);

                    if (!string.IsNullOrEmpty(moduleName) && moduleName != "All")
                        sql.Where("moduleName", moduleName);
                }

                return dbManager.ExecuteList(sql).ConvertAll(GetSearchWord);
            }
        }

        public static void UpdateHashTable(ref Hashtable table, DateTime date)
        {
            using (var dbManager = new DbManager("tmresourceTrans"))
            {
                var sql = new SqlQuery(ResDataTable)
                    .Select(ResDataTable + ".textValue", ResDataTable + ".title", ResFilesTable + ".ResName", ResDataTable + ".cultureTitle")
                    .InnerJoin(ResFilesTable, Exp.EqColumns(ResFilesTable + ".id", ResDataTable + ".fileID"))
                    .Where(Exp.Ge("timechanges", date));

                var list = dbManager.ExecuteList(sql);

                foreach (var t in list)
                {
                    var key = t[1] + t[2].ToString() + t[3];

                    if (table.ContainsKey(key))
                        table[key] = t[0];
                    else
                        table.Add(key, t[0]);
                }
            }
        }


        private static ResWord GetWord(IList<object> r)
        {
            return new ResWord { Title = (string)r[0], ResFile = new ResFile { FileID = (int)r[1] }, ValueFrom = (string)r[2], TextComment = (string)r[3], Flag = (int)r[4], Link = (string)r[5] };
        }

        private static ResWord GetSearchWord(IList<object> r)
        {
            var resfile = new ResFile { FileID = (int)r[1], FileName = (string)r[3], ModuleName = (string)r[4], ProjectName = (string)r[5] };
            return new ResWord { Title = (string)r[0], ResFile = resfile, ValueFrom = (string)r[2] };
        }

        private static SqlQuery GetQuery(string table, string cultureTitle, ResWord word)
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
                    .InColumns("title", "textvalue", "cultureTitle", "fileID")
                    .Values(word.Title, word.ValueFrom, cultureTitle, word.ResFile.FileID);

        }
    }
}