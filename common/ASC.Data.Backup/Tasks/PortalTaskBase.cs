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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Tasks
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public int Progress { get; private set; }

        public ProgressChangedEventArgs(int progress)
        {
            Progress = progress;
        }
    }

    public abstract class PortalTaskBase
    {
        protected const int TasksLimit = 10;
        protected readonly List<ModuleName> IgnoredModules = new List<ModuleName>();
        protected readonly List<string> IgnoredTables = new List<string>(); //todo: add using to backup and transfer tasks

        protected ILog Logger;

        public int Progress { get; private set; }

        public int TenantId { get; private set; }
        public string ConfigPath { get; private set; }

        public bool ProcessStorage { get; set; }

        protected PortalTaskBase(ILog logger, int tenantId, string configPath)
        {
            Logger = logger ?? LogManager.GetLogger("ASC");
            TenantId = tenantId;
            ConfigPath = configPath;
            ProcessStorage = true;
        }

        public void IgnoreModule(ModuleName moduleName)
        {
            if (!IgnoredModules.Contains(moduleName))
                IgnoredModules.Add(moduleName);
        }

        public void IgnoreTable(string tableName)
        {
            if (!IgnoredTables.Contains(tableName))
                IgnoredTables.Add(tableName);
        }

        public abstract void RunJob();

        internal virtual IEnumerable<IModuleSpecifics> GetModulesToProcess()
        {
            return ModuleProvider.AllModules.Where(module => !IgnoredModules.Contains(module.ModuleName));
        }

        protected IEnumerable<BackupFileInfo> GetFilesToProcess(int tenantId)
        {
            var files = new List<BackupFileInfo>();
            foreach (var module in StorageFactory.GetModuleList(ConfigPath).Where(IsStorageModuleAllowed))
            {
                var store = StorageFactory.GetStorage(ConfigPath, tenantId.ToString(), module);
                var domains = StorageFactory.GetDomainList(ConfigPath, module).ToArray();

                foreach (var domain in domains)
                {
                    files.AddRange(
                        store.ListFilesRelative(domain, "\\", "*.*", true)
                        .Select(path => new BackupFileInfo(domain, module, path, tenantId)));
                }

                files.AddRange(
                    store.ListFilesRelative(string.Empty, "\\", "*.*", true)
                         .Where(path => domains.All(domain => !path.Contains(domain + "/")))
                         .Select(path => new BackupFileInfo(string.Empty, module, path, tenantId)));
            }

            return files.Distinct();
        }

        protected virtual bool IsStorageModuleAllowed(string storageModuleName)
        {
            var allowedStorageModules = new List<string>
                {
                    "forum",
                    "photo",
                    "bookmarking",
                    "wiki",
                    "files",
                    "crm",
                    "projects",
                    "logo",
                    "fckuploaders",
                    "talk",
                    "mailaggregator",
                    "whitelabel"
                };

            if (!allowedStorageModules.Contains(storageModuleName))
                return false;

            IModuleSpecifics moduleSpecifics = ModuleProvider.GetByStorageModule(storageModuleName);
            return moduleSpecifics == null || !IgnoredModules.Contains(moduleSpecifics.ModuleName);
        }

        #region Progress

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        private int stepsCount = 1;
        private volatile int stepsCompleted;

        protected void SetStepsCount(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            stepsCount = value;
            Logger.Debug("Steps: " + stepsCount);
        }

        protected void SetStepCompleted(int increment = 1)
        {
            if (stepsCount == 1)
            {
                return;
            }
            if (stepsCompleted == stepsCount)
            {
                throw new InvalidOperationException("All steps completed.");
            }
            stepsCompleted += increment;
            SetProgress(100 * stepsCompleted / stepsCount);
        }

        protected void SetCurrentStepProgress(int value)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (value == 100)
            {
                SetStepCompleted();
            }
            else
            {
                SetProgress((100 * stepsCompleted + value) / stepsCount);
            }
        }

        protected void SetProgress(int value)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (Progress != value)
            {
                Progress = value;
                OnProgressChanged(new ProgressChangedEventArgs(value));
            }
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs eventArgs)
        {
            var handler = ProgressChanged;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        #endregion

        protected Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            var result = new Dictionary<string, string>();

            var parsed = connectionString.Split(';');

            foreach (var p in parsed)
            {
                if (string.IsNullOrEmpty(p.Trim())) continue;
                var keyValue = p.Split('=');
                result.Add(keyValue[0].ToLowerInvariant(), keyValue[1]);
            }

            return result;
        }

        protected void RunMysqlFile(DbFactory dbFactory, string file, bool db = false)
        {
            var connectionString = ParseConnectionString(dbFactory.ConnectionStringSettings.ConnectionString);
            var args = new StringBuilder()
                .AppendFormat("-h {0} ", connectionString["server"])
                .AppendFormat("-u {0} ", connectionString["user id"])
                .AppendFormat("-p{0} ", connectionString["password"]);

            if (db)
            {
                args.AppendFormat("-D {0} ", connectionString["database"]);
            }

            args.AppendFormat("-e \" source {0}\"", file);
            Logger.DebugFormat("run mysql file {0} {1}", file, args.ToString());

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = "mysql",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = args.ToString()
            };

            using (var proc = Process.Start(startInfo))
            {
                if (proc != null)
                {
                    proc.WaitForExit();

                    var error = proc.StandardError.ReadToEnd();
                    Logger.Error(!string.IsNullOrEmpty(error) ? error : proc.StandardOutput.ReadToEnd());
                }
            }

            Logger.DebugFormat("complete mysql file {0}", file);
        }

        protected async Task RunMysqlFile(Stream stream, string delimiter = ";")
        {
            using (var dbManager = new DbManager("default", 100000))
            using (var tr = dbManager.BeginTransaction())
            {
                if (stream == null) return;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string commandText;

                    while ((commandText = await reader.ReadLineAsync()) != null)
                    {
                        while (!commandText.EndsWith(delimiter))
                        {
                            var newline = await reader.ReadLineAsync();
                            if (newline == null)
                            {
                                break;
                            }
                            commandText += newline;
                        }

                        try
                        {
                            await dbManager.ExecuteNonQueryAsync(commandText, null);
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Restore", e);
                        }
                    }
                }

                tr.Commit();
            }
        }
    }
}
