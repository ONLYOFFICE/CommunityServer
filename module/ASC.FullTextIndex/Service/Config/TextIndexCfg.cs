/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Configuration;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Notify.Cron;
using log4net;
using log4net.Appender;

namespace ASC.FullTextIndex.Service.Config
{
    class TextIndexCfg
    {
        public readonly static int MaxQueryLength = 30;

        public static CronExpression ChangedCron { get; private set; }

        public static CronExpression RemovedCron { get; private set; }

        public static CronExpression MergeCron { get; private set; }

        public static IList<ModuleInfo> Modules { get; private set; }

        public static string ConnectionStringName { get; private set; }

        public static string DataPath { get; private set; }

        public static string ConfPath { get; private set; }

        public static int Chunks { get; private set; }

        public static int Dimension { get; private set; }

        public const string SearcherName = "searchd";
        public const string IndexerName = "indexer";
        private const string ConfName = "sphinx-min.conf.in";

        public static bool Init()
        {
            try
            {
                var cfg = (TextIndexCfgSectionHandler)ConfigurationManager.GetSection("fullTextIndex");
                ChangedCron = new CronExpression(cfg.ChangedCron);
                RemovedCron = new CronExpression(cfg.RemovedCron);
                MergeCron = new CronExpression(cfg.MergeCron);
                Chunks = cfg.Chunks;
                Dimension = cfg.Dimension;
                Modules = cfg.Modules
                    .Cast<TextIndexCfgModuleElement>()
                    .Select(e => new ModuleInfo(e.Name))
                    .ToList();
                DataPath = PrepareFolderPath(cfg.DataPath);
                ConfPath = PrepareFilePath(ConfName);

                var settings = InitConnectionSettings(cfg);
                if (settings.Host != "localhost")
                    return false;

                var logPath = PrepareFolderPath(GetLog4NetPath());
                var connectionStringParts = ParseConnectionString(ConfigurationManager.ConnectionStrings["default"].ConnectionString);

                var shinxCfg = new SphinxCfg(connectionStringParts, DataPath, logPath, ConfPath, Modules);
                shinxCfg.Init();
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Fatal("Error while init config", e);
                return false;
            }
            return true;
        }

        private static FullTextSearchSettings InitConnectionSettings(TextIndexCfgSectionHandler cfg)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[cfg.ConnectionStringName];
            ConnectionStringName = cfg.ConnectionStringName;

            var settings = CoreContext.Configuration.GetSection<FullTextSearchSettings>(Tenant.DEFAULT_TENANT);
            if (connectionString == null)
            {
                if (settings == null)
                    throw new Exception("connection string can not be empty");

                DbRegistry.RegisterDatabase(ConnectionStringName,
                    new ConnectionStringSettings
                    {
                        Name = cfg.ConnectionStringName,
                        ConnectionString = settings.ConnectionString,
                        ProviderName = "MySql.Data.MySqlClient"
                    });
            }
            else
            {
                var parsed = ParseConnectionString(connectionString.ConnectionString);
                settings = new FullTextSearchSettings
                {
                    Host = parsed.ContainsKey("Server") ? parsed["Server"] : "localhost",
                    Port = Convert.ToInt32(parsed.ContainsKey("Port") ? parsed["Port"] : "9306")
                };
                CoreContext.Configuration.SaveSection(Tenant.DEFAULT_TENANT, settings);
            }

            return settings;
        }

        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            return connectionString.Split(';').ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1]);
        }

        private static string GetLog4NetPath()
        {
            var logFile = (FileAppender)LogManager.GetRepository().GetAppenders().FirstOrDefault(r => r.Name == "File");
            return logFile != null ? Path.GetDirectoryName(logFile.File) : "";
        }

        private static string PrepareFilePath(string path)
        {
            var result = PreparePath(path);

            if (!File.Exists(result))
                throw new FileNotFoundException(result);

            return result;
        }

        private static string PrepareFolderPath(string path)
        {
            var result = PreparePath(path);

            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);

            return result;
        }

        private static string PreparePath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return Path.GetFullPath(path);
        }
    }

    class SphinxCfg
    {
        private readonly Dictionary<string, string> connectionStringParts;
        private readonly string confPath;
        private readonly string dataPath;
        private readonly string logPath;
        private readonly IList<ModuleInfo> modules;

        public SphinxCfg(Dictionary<string, string> connectionStringParts, string dataPath, string logPath, string confPath, IList<ModuleInfo> modules)
        {
            this.connectionStringParts = connectionStringParts;
            this.dataPath = dataPath;
            this.logPath = logPath;
            this.confPath = confPath;
            this.modules = modules;
        }

        public void Init()
        {
            File.WriteAllText(confPath, File.ReadAllText(confPath)
                                            .Replace("%SQL_HOST%", connectionStringParts["Server"])
                                            .Replace("%SQL_USER%", connectionStringParts["User ID"])
                                            .Replace("%SQL_PASS%", connectionStringParts["Password"])
                                            .Replace("%SQL_DB%", connectionStringParts["Database"])
                                            .Replace("%SQL_PORT%", "3306")
                                            .Replace("%INDEX_PATH%", ReplaceBackSlash(dataPath))
                                            .Replace("%LOG_PATH%", ReplaceBackSlash(logPath)));

            if (TextIndexCfg.Chunks <= 1)
            {
                File.WriteAllText(confPath,
                    File.ReadAllText(confPath).Replace("%having%", ""));
                return;
            }

            var oldFileDataLines = File.ReadAllLines(confPath);
            var newFileDataLines = new List<string>();

            for (var oldFileDataLineIndex = 0; oldFileDataLineIndex < oldFileDataLines.Count(); oldFileDataLineIndex++)
            {
                var result = ReplaceSource(oldFileDataLines, ref oldFileDataLineIndex);
                result.AddRange(ReplaceIndex(oldFileDataLines, ref oldFileDataLineIndex));

                if (!result.Any())
                {
                    newFileDataLines.Add(oldFileDataLines[oldFileDataLineIndex]);
                }
                else
                {
                    newFileDataLines.AddRange(result);
                }
            }

            File.WriteAllLines(confPath, newFileDataLines);
        }

        private List<string> ReplaceSource(IList<string> oldFileDataLines, ref int oldFileDataLineIndex)
        {
            var result = new List<string>();
            var newDataLineIndex = oldFileDataLineIndex;

            var moduleInfo = modules.FirstOrDefault(r => oldFileDataLines[newDataLineIndex].Contains(string.Format("source {0} ", r.Main)));
            if (moduleInfo == null) return result;

            for (var chunk = 1; chunk <= TextIndexCfg.Chunks; chunk++)
            {
                var idSelector = GetIdSelector("tenant_id", chunk);

                for (newDataLineIndex = oldFileDataLineIndex; newDataLineIndex < oldFileDataLines.Count() && oldFileDataLines[newDataLineIndex - 1] != "}"; newDataLineIndex++)
                {
                    var line = oldFileDataLines[newDataLineIndex].Replace(moduleInfo.Main, moduleInfo.GetChunk(chunk));

                    if (line.Contains("%having%"))
                    {
                        line = line.Replace("%having%", string.Format(" having {0}", idSelector));
                    }

                    result.Add(line);
                }
            }
            oldFileDataLineIndex = newDataLineIndex - 1;
            return result;
        }

        private static string GetIdSelector(string idColumn, int chunk)
        {
            var idSelector = "";
            if (chunk == 1) idSelector = string.Format("{0}<{1}", idColumn, chunk * TextIndexCfg.Dimension);
            if (chunk > 1 && chunk < TextIndexCfg.Chunks) idSelector = string.Format("{0} between {1} and {2}", idColumn, (chunk - 1) * TextIndexCfg.Dimension, chunk * TextIndexCfg.Dimension);
            if (chunk == TextIndexCfg.Chunks) idSelector = string.Format("{0}>{1}", idColumn, (chunk - 1) * TextIndexCfg.Dimension);
            return idSelector;
        }

        private IEnumerable<string> ReplaceIndex(IList<string> oldFileDataLines, ref int oldFileDataLineIndex)
        {
            var result = new List<string>();
            var newDataLineIndex = oldFileDataLineIndex;

            var moduleInfo = modules.FirstOrDefault(r => oldFileDataLines[newDataLineIndex].Contains(string.Format("index {0} ", r.Main)));
            if (moduleInfo == null || oldFileDataLines.Any(r => r.Contains(GetLocalIndex(moduleInfo.Main + "_1")))) return result;

            var mainIndex = new List<string> { string.Format("index {0}", moduleInfo.Main), "{", "type  = distributed" };

            for (var chunk = 1; chunk <= TextIndexCfg.Chunks; chunk++)
            {
                var moduleChunk = moduleInfo.GetChunk(chunk);
                for (newDataLineIndex = oldFileDataLineIndex; newDataLineIndex < oldFileDataLines.Count() && oldFileDataLines[newDataLineIndex - 1] != "}"; newDataLineIndex++)
                {
                    var line = oldFileDataLines[newDataLineIndex].Replace(moduleInfo.Main, moduleChunk).Replace("/main", "/" + moduleChunk);
                    result.Add(line);
                }
                mainIndex.Add(GetLocalIndex(moduleChunk));
            }

            mainIndex.Add("}");
            result.AddRange(mainIndex);

            oldFileDataLineIndex = newDataLineIndex - 1;
            return result;
        }

        private static string GetLocalIndex(string indexTitle)
        {
            return string.Format("local = {0}", indexTitle);
        }

        private static string ReplaceBackSlash(string path)
        {
            return path.Replace("\\", "/").TrimEnd('/');
        }
    }
}
