/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Configuration;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using TMResourceData.Model;

namespace TMResourceData
{
    public static class GetResource
    {
        static GetResource()
        {
            if (!DbRegistry.IsDatabaseRegistered("tmresource"))
            {
                DbRegistry.RegisterDatabase("tmresource", ConfigurationManager.ConnectionStrings["tmresource"]);
            }
            if (!DbRegistry.IsDatabaseRegistered("tmresourceTrans")
                && ConfigurationManager.ConnectionStrings["tmresourceTrans"] != null)
            {
                DbRegistry.RegisterDatabase("tmresourceTrans", ConfigurationManager.ConnectionStrings["tmresourceTrans"]);
            }
        }

        public static DateTime GetLastUpdate()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_files");

                sql.SelectMax("LastUpdate");

                return dbManager.ExecuteScalar<DateTime>(sql);
            }
        }

        #region Cultures

        public static IEnumerable<ResCulture> GetCultures()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_cultures");
                sql.Select(new[] {"title", "value", "available"})
                   .OrderBy("title", true);

                return dbManager.ExecuteList(sql).ConvertAll(r => GetCultureFromDB(r));
            }
        }

        public static Dictionary<ResCulture, List<string>> GetCulturesWithAuthors()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_authorslang ral")
                    .Select(new[] {"ral.authorLogin", "rc.title", "rc.value"})
                    .InnerJoin("res_cultures rc", Exp.EqColumns("rc.title", "ral.cultureTitle"))
                    .InnerJoin("res_authors ra", Exp.EqColumns("ra.login", "ral.authorLogin"))
                    .Where("ra.isAdmin", false);

                return dbManager.ExecuteList(sql)
                                .GroupBy(r => new ResCulture {Title = (string)r[1], Value = (string)r[2]}, r => (string)r[0])
                                .ToDictionary(r => r.Key, r => r.ToList());
            }
        }

        public static void SetCultureAvailable(string title)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlUpdate("res_cultures");
                sql.Set("available", true).Where("title", title);
                dbManager.ExecuteNonQuery(sql);
            }
        }

        private static ResCulture GetCultureFromDB(IList<object> r)
        {
            return new ResCulture {Title = (string)r[0], Value = (string)r[1], Available = (bool)r[2]};
        }

        public static List<ResCulture> GetListLanguages(int fileId, string title)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_cultures")
                    .Select("res_cultures.title", "res_cultures.value", "res_cultures.available")
                    .LeftOuterJoin("res_data", Exp.EqColumns("res_cultures.title", "res_data.cultureTitle"))
                    .Where("res_data.fileID", fileId)
                    .Where("res_data.title", title);

                var language = dbManager.ExecuteList(sql).ConvertAll(r => GetCultureFromDB(r));

                language.Remove(language.Find(p => p.Title == "Neutral"));

                return language;
            }
        }

        #endregion

        #region File

        public static List<ResFile> GetAllFiles()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_files");

                return dbManager.ExecuteList(sql.SelectAll()).Select(r => new ResFile
                    {
                        FileID = (int)r[0],
                        ProjectName = (string)r[1],
                        ModuleName = (string)r[2],
                        FileName = (string)r[3]
                    }).ToList();
            }
        }

        #endregion

        #region Project

        public static IEnumerable<ResProject> GetResProjects()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_files");
                sql.Select("projectName").Distinct();

                var projects = dbManager.ExecuteList(sql).Select(r => new ResProject {Name = (string)r[0]}).ToList();

                foreach (var resProject in projects)
                {
                    sql = new SqlQuery("res_files")
                        .Select("moduleName", "islock")
                        .Where("projectName", resProject.Name)
                        .Distinct();

                    resProject.Modules = dbManager.ExecuteList(sql).Select(r => new ResModule {Name = (string)r[0], IsLock = (bool)r[1]}).ToList();
                }

                return projects;
            }
        }

        #endregion

        #region Module

        public static IEnumerable<ResWord> GetListResWords(ResCurrent current, string search)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var exist = new SqlQuery("res_data rd3")
                    .Select("rd3.title")
                    .Where("rd3.fileid = rd1.fileid")
                    .Where("rd3.title = concat('del_', rd1.title)")
                    .Where("rd3.cultureTitle = rd1.cultureTitle");

                var sql = new SqlQuery("res_data rd1")
                    .Select("rd1.title", "rd1.fileid", "rd1.textValue", "rd1.description", "rd1.flag", "rd1.link", "rf.resName", "rd2.id", "rd2.flag", "rd2.textValue")
                    .LeftOuterJoin("res_data rd2", Exp.EqColumns("rd1.fileid", "rd2.fileid") & Exp.EqColumns("rd1.title", "rd2.title") & Exp.Eq("rd2.cultureTitle", current.Language.Title))
                    .InnerJoin("res_files rf", Exp.EqColumns("rf.ID", "rd1.fileID"))
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
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data")
                    .Select("title", "fileid", "textValue", "description", "flag", "link")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.ID", "res_data.fileID"))
                    .Where("moduleName", resFile.ModuleName)
                    .Where("projectName", resFile.ProjectName)
                    .Where("cultureTitle", to)
                    .Where("flag != 4")
                    .Where("resourceType", "text")
                    .OrderBy("res_data.id", true);

                if (!String.IsNullOrEmpty(resFile.FileName))
                    sql.Where("resName", resFile.FileName);

                if (!String.IsNullOrEmpty(search))
                    sql.Where(Exp.Like("textvalue", search));

                return dbManager.ExecuteList(sql).ConvertAll(r => GetWord(r));
            }
        }

        public static void GetListModules(ResCurrent currentData)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var exist = new SqlQuery("res_data rd1")
                    .Select("rd1.title")
                    .Where("rd1.fileid = rd.fileid")
                    .Where("rd1.title = concat('del_', rd.title)")
                    .Where("rd1.cultureTitle = rd.cultureTitle");

                var sql = new SqlQuery("res_files rf").Select("rf.moduleName",
                                                              string.Format("sum(case rd.cultureTitle when '{0}' then (case rd.flag when 3 then 0 else 1 end) else 0 end)", currentData.Language.Title),
                                                              string.Format("sum(case rd.cultureTitle when '{0}' then (case rd.flag when 3 then 1 else 0 end) else 0 end)", currentData.Language.Title),
                                                              string.Format("sum(case rd.cultureTitle when '{0}' then 1 else 0 end)", "Neutral"))
                                                      .InnerJoin("res_data rd", Exp.EqColumns("rd.fileid", "rf.id"))
                                                      .Where("rf.projectName", currentData.Project.Name)
                                                      .Where("rd.resourceType", "text")
                                                      .Where(!Exp.Like("rd.title", @"del\_", SqlLike.StartWith) & !Exp.Exists(exist))
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

        private static ResWord GetWord(IList<object> r)
        {
            return new ResWord {Title = (string)r[0], ResFile = new ResFile {FileID = (int)r[1]}, ValueFrom = (string)r[2], TextComment = (string)r[3], Flag = (int)r[4], Link = (string)r[5]};
        }

        private static ResWord GetSearchWord(IList<object> r)
        {
            var resfile = new ResFile {FileID = (int)r[1], FileName = (string)r[3], ModuleName = (string)r[4], ProjectName = (string)r[5]};
            return new ResWord {Title = (string)r[0], ResFile = resfile, ValueFrom = (string)r[2]};
        }

        public static void LockModules(string projectName, string modules)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sqlUpdate = new SqlUpdate("res_files");
                sqlUpdate.Set("isLock", 1).Where("projectName", projectName).Where(Exp.In("moduleName", modules.Split(',')));
                dbManager.ExecuteNonQuery(sqlUpdate);
            }
        }

        public static void UnLockModules()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sqlUpdate = new SqlUpdate("res_files");
                sqlUpdate.Set("isLock", 0);
                dbManager.ExecuteNonQuery(sqlUpdate);
            }
        }

        #endregion

        #region Word

        public static void AddLink(string resource, string fileName, string page)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var query = new SqlQuery("res_data");
                query.Select("res_data.id")
                     .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileid"))
                     .Where("res_data.title", resource).Where("res_files.resName", fileName).Where("res_data.cultureTitle", "Neutral");

                var key = dbManager.ExecuteScalar<int>(query);

                var update = new SqlUpdate("res_data");
                update.Set("link", page).Where("id", key);
                dbManager.ExecuteNonQuery(update);
            }
        }

        public static void GetResWordByKey(ResWord word, string to)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data")
                    .Select("textvalue", "description", "link")
                    .Where("fileID", word.ResFile.FileID)
                    .Where("cultureTitle", "Neutral")
                    .Where("title", word.Title);

                dbManager.ExecuteList(sql).ForEach(r => GetValue(word, to, r));

                GetValueByKey(word, to);

                sql = new SqlQuery("res_data as res1").Select("res1.textvalue").Distinct()
                                                      .InnerJoin("res_data as res2", Exp.EqColumns("res1.title", "res2.title") & Exp.EqColumns("res1.fileid", "res2.fileid"))
                                                      .Where("res1.cultureTitle", to)
                                                      .Where("res2.cultureTitle", "Neutral")
                                                      .Where("res2.textvalue", word.ValueFrom);

                word.Alternative = new List<string>();
                dbManager.ExecuteList(sql).ForEach(r => word.Alternative.Add((string)r[0]));
                word.Alternative.Remove(word.ValueTo);

                sql = new SqlQuery("res_files")
                    .Select("resname")
                    .Where("id", word.ResFile.FileID);

                word.ResFile.FileName = dbManager.ExecuteScalar<string>(sql);

                return;
            }
        }

        public static void GetValueByKey(ResWord word, string to)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data");
                sql.Select(new[] {"textvalue"})
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

            var langs = (ConfigurationManager.AppSettings["resources.com-lang"] ?? string.Empty).Split(';').ToList();
            var dom = langs.Exists(lang => lang == to) ? ".info" : ".com";

            word.Link = !String.IsNullOrEmpty((string)r[2]) ? String.Format("http://{0}-translator.teamlab{1}{2}", to, dom, r[2]) : "";
        }

        #endregion

        #region Author

        public static List<Author> GetListAuthors()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_authors")
                    .Select("login", "password", "isAdmin");

                return dbManager.ExecuteList(sql).ConvertAll(r => new Author {Login = (string)r[0], Password = (string)r[1], IsAdmin = Convert.ToBoolean(r[2])});
            }
        }

        public static Author GetAuthor(string login)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_authors")
                    .Select(new[] {"login", "password", "isAdmin"})
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
                        .Select(new[] {"rc.title", "rc.value", "rc.available"})
                        .InnerJoin("res_authorslang ral", Exp.EqColumns("rc.title", "ral.cultureTitle"))
                        .Where("ral.authorLogin", author.Login);

                    author.Langs = dbManager.ExecuteList(sql).ConvertAll(r => GetCultureFromDB(r));

                    sql = new SqlQuery("res_files rf")
                        .Select("rf.projectName").Distinct()
                        .InnerJoin("res_authorsfile raf", Exp.EqColumns("raf.fileid", "rf.id"))
                        .Where("raf.authorlogin", login)
                        .Where("rf.isLock", 0);

                    var projects = dbManager.ExecuteList(sql).Select(r => new ResProject {Name = (string)r[0]}).ToList();

                    foreach (var resProject in projects)
                    {
                        sql = new SqlQuery("res_files rf")
                            .Select("rf.moduleName").Distinct()
                            .InnerJoin("res_authorsfile raf", Exp.EqColumns("raf.fileid", "rf.id"))
                            .Where("rf.projectName", resProject.Name)
                            .Where("raf.authorlogin", login)
                            .Where("rf.isLock", 0);
                        resProject.Modules = dbManager.ExecuteList(sql).Select(r => new ResModule {Name = (string)r[0]}).ToList();
                    }

                    author.Projects = projects;
                }

                return author;
            }
        }

        public static void CreateAuthor(Author author, IEnumerable<string> languages, string modules)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sqlInsert = new SqlInsert("res_authors", true)
                    .InColumnValue("login", author.Login)
                    .InColumnValue("password", author.Password)
                    .InColumnValue("isAdmin", author.IsAdmin);

                dbManager.ExecuteNonQuery(sqlInsert);

                var delete = new SqlDelete("res_authorslang").Where("authorLogin", author.Login);
                dbManager.ExecuteNonQuery(delete);

                delete = new SqlDelete("res_authorsfile").Where("authorLogin", author.Login);
                dbManager.ExecuteNonQuery(delete);

                foreach (var lang in languages)
                {
                    sqlInsert = new SqlInsert("res_authorslang", true)
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
                        sqlInsert = new SqlInsert("res_authorsfile", true)
                            .InColumnValue("authorLogin", author.Login)
                            .InColumnValue("fileId", fileid); //.InColumnValue("writeAccess", Convert.ToBoolean(modData[1]));
                        dbManager.ExecuteNonQuery(sqlInsert);
                    }
                }
            }
        }

        public static void DeleteAuthor(string login)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlDelete("res_authors").Where("login", login);

                dbManager.ExecuteNonQuery(sql);
            }
        }

        public static bool IsAuthor(string login, string password)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_authors")
                    .SelectCount()
                    .Where("login", login)
                    .Where("password", password);

                return dbManager.ExecuteScalar<int>(sql) != 0;
            }
        }

        public static void SetAuthorOnline(string login)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlUpdate("res_authors")
                    .Set("lastVisit", DateTime.UtcNow)
                    .Where("login", login);

                dbManager.ExecuteNonQuery(sql);
            }
        }

        public static List<string> GetOnlineAuthors()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_authors")
                    .Select("login")
                    .Where(Exp.Ge("LastVisit", DateTime.UtcNow.AddMinutes(-5)));

                return dbManager.ExecuteList(sql).ConvertAll(r => (string)r[0]);
            }
        }

        public static void AddAuthorLang(string login, string cultureTitle)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlInsert("res_authorslang")
                    .InColumnValue("cultureTitle", cultureTitle)
                    .InColumnValue("authorLogin", login);

                dbManager.ExecuteNonQuery(sql);
            }
        }

        #endregion

        #region Statistics

        public static List<StatisticModule> GetStatistic()
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data")
                    .SelectCount().Select(new[] {"res_cultures.value", "res_files.projectName"})
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileid"))
                    .InnerJoin("res_cultures", Exp.EqColumns("res_data.cultureTitle", "res_cultures.title"))
                    .Where(Exp.Lt("flag", 3))
                    .Where("resourceType", "text")
                    .Where("isLock", 0)
                    .GroupBy(new[] {"value", "projectName"})
                    .OrderBy("value", true)
                    .OrderBy("projectName", true);

                var stat = dbManager.ExecuteList(sql);
                var allStat = new List<StatisticModule>();

                foreach (var culture in stat.Select(data => (string)data[1]).Distinct())
                {
                    var cultureData = new StatisticModule {Culture = culture};

                    foreach (var project in stat.Select(data => (string)data[2]).Distinct())
                    {
                        cultureData.Counts.Add(project, stat.Where(r => (string)r[1] == culture && (string)r[2] == project).Sum(r => (Convert.ToInt32(r[0]))));
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
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data as r1");

                sql.SelectCount().Select(new[] {"res_cultures.title", "r1.authorLogin", "sum(length(r2.textvalue))"})
                   .InnerJoin("res_data as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                   .InnerJoin("res_cultures", Exp.EqColumns("r1.cultureTitle", "res_cultures.title"))
                   .Where(!Exp.Eq("r1.flag", 4))
                   .Where(!Exp.Eq("r1.flag", 3))
                   .Where(!Exp.Eq("r1.authorLogin", "Console"))
                   .Where(!Exp.Eq("r1.cultureTitle", "Neutral"))
                   .Where(Exp.Ge("r1.timeChanges", from))
                   .Where(Exp.Le("r1.timeChanges", till))
                   .Where("r2.cultureTitle", "Neutral")
                   .GroupBy(new[] {"title", "authorLogin"})
                   .OrderBy("title", true)
                   .OrderBy("authorLogin", true);

                return dbManager.ExecuteList(sql).ConvertAll(r => GetUserStatisticFromDB(r));
            }
        }

        public static List<StatisticUser> GetUserStatisticForModules(string login, DateTime from, DateTime till)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data as r1");

                sql.SelectCount().Select(new[] {"concat_ws(':', res_files.projectName,res_files.moduleName)", "r1.authorLogin", "sum(length(r2.textvalue))"})
                   .InnerJoin("res_data as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                   .InnerJoin("res_files", Exp.EqColumns("r1.fileid", "res_files.id"))
                   .Where(!Exp.Eq("r1.flag", 4))
                   .Where(!Exp.Eq("r1.flag", 3))
                   .Where(!Exp.Eq("r1.authorLogin", "Console"))
                   .Where(!Exp.Eq("r1.cultureTitle", "Neutral"))
                   .Where(Exp.Ge("r1.timeChanges", from))
                   .Where(Exp.Le("r1.timeChanges", till))
                   .Where("r2.cultureTitle", "Neutral")
                   .Where(Exp.Eq("r1.authorLogin", login))
                   .GroupBy(new[] {"r1.fileid", "r1.authorLogin"})
                   .OrderBy("r1.fileid", true)
                   .OrderBy("r1.authorLogin", true);

                return dbManager.ExecuteList(sql).Select(r => new StatisticUser {WordsCount = Convert.ToInt32(r[0]), Module = (string)r[1], Login = (string)r[2], SignCount = Convert.ToInt32(r[3])}).ToList();
            }
        }

        private static StatisticUser GetUserStatisticFromDB(IList<object> r)
        {
            return new StatisticUser {WordsCount = Convert.ToInt32(r[0]), Culture = (string)r[1], Login = (string)r[2], SignCount = Convert.ToInt32(r[3])};
        }

        #endregion

        #region Search

        public static List<ResWord> SearchAll(string projectName, string moduleName, string languageTo, string searchText, string searchType)
        {
            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data")
                    .Select("title", "fileid", "textValue", "resName", "moduleName", "projectName")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.ID", "res_data.fileID"))
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

                return dbManager.ExecuteList(sql).ConvertAll(r => GetSearchWord(r));
            }
        }

        #endregion

        #region DBResourceManager

        public static void UpdateHashTable(ref Hashtable table, DateTime date)
        {
            using (var dbManager = new DbManager("tmresourceTrans"))
            {
                var sql = new SqlQuery("res_data")
                    .Select("res_data.textValue", "res_data.title", "res_files.ResName", "res_data.cultureTitle")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"))
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

        public static void UpdateDBRS(DBResourceSet databaseResourceSet, string fileName, string culture, DateTime date)
        {
            using (var dbManager = new DbManager("tmresourceTrans"))
            {
                var sql = new SqlQuery("res_data")
                    .Select("title", "textValue")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"))
                    .Where("ResName", fileName)
                    .Where("cultureTitle", culture)
                    .Where(Exp.Ge("timechanges", date));

                var list = dbManager.ExecuteList(sql);

                foreach (var t in list)
                {
                    databaseResourceSet.SetString(t[0], t[1]);
                }
            }
        }

        public static Hashtable GetAllData(string dataBaseId)
        {
            var hashTable = new Hashtable();

            using (var dbManager = new DbManager(dataBaseId))
            {
                var sql = new SqlQuery("res_data")
                    .Select("textValue", "title", "ResName", "cultureTitle")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"));

                var list = dbManager.ExecuteList(sql);

                foreach (var t in list.Where(t => !hashTable.ContainsKey((string)t[1] + (string)t[2] + (string)t[3])))
                    hashTable.Add(t[1] + t[2].ToString() + t[3], t[0]);
            }

            return hashTable;
        }

        #endregion
    }
}