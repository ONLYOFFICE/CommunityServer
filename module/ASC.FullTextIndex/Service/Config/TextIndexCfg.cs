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
using System.Configuration;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Notify.Cron;

namespace ASC.FullTextIndex.Service.Config
{
    class TextIndexCfg
    {
        public readonly static int MaxQueryLength = 30;

        public static CronExpression ChangedCron { get; private set; }

        public static CronExpression RemovedCron { get; private set; }

        public static IList<ModuleInfo> Modules { get; private set; }

        public static string ConnectionStringName { get; private set; }

        public static string DataPath { get; private set; }

        public static string ConfPath { get; private set; }

        public static string LogPath { get; private set; }

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
                Modules = cfg.Modules
                    .Cast<TextIndexCfgModuleElement>()
                    .Select(e => new ModuleInfo(e.Name))
                    .ToList();
                DataPath = cfg.DataPath;

                var settings = InitConnectionSettings(cfg);
                if (settings.Host != "localhost")
                    return false;

                InitLocalhostSearcherAndIndexer();
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger("ASC").Fatal("Error while init config", e);
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
                var connectionStringParts = ParseConnectionString(connectionString.ConnectionString);
                settings = new FullTextSearchSettings
                {
                    Host = connectionStringParts["Server"],
                    Port = Convert.ToInt32(connectionStringParts["Port"]) 
                };
                CoreContext.Configuration.SaveSection(Tenant.DEFAULT_TENANT, settings);
            }

            return settings;
        }

        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            return connectionString.Split(';').ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1]);
        } 

        private static void InitLocalhostSearcherAndIndexer()
        {
            DataPath = PrepareFolderPath(DataPath);
            LogPath = PrepareFolderPath(GetLog4NetPath());
            ConfPath = PrepareFilePath(ConfName);

            PrepareSearcherConf();
        }

        private static string GetLog4NetPath()
        {
            var logFile = (log4net.Appender.FileAppender) log4net.LogManager.GetRepository().GetAppenders()
                                                              .FirstOrDefault(r => r.Name == "File");
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

        private static void PrepareSearcherConf()
        {
            var connectionStringParts = ParseConnectionString(ConfigurationManager.ConnectionStrings["default"].ConnectionString);

            File.WriteAllText(ConfPath, File.ReadAllText(ConfPath)
                                            .Replace("%SQL_HOST%", connectionStringParts["Server"])
                                            .Replace("%SQL_USER%", connectionStringParts["User ID"])
                                            .Replace("%SQL_PASS%", connectionStringParts["Password"])
                                            .Replace("%SQL_DB%", connectionStringParts["Database"])
                                            .Replace("%SQL_PORT%", "3306")
                                            .Replace("%INDEX_PATH%", ReplaceBackSlash(DataPath))
                                            .Replace("%LOG_PATH%", ReplaceBackSlash(LogPath)));
        }

        private static string ReplaceBackSlash(string path)
        {
            return path.Replace("\\", "/").TrimEnd('/');
        }
    }
}
