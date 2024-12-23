/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Storage;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;
using ASC.Data.Storage.ZipOperators;

using Newtonsoft.Json;

namespace ASC.Data.Backup.Tasks
{
    public class RestorePortalTask : PortalTaskBase
    {
        private readonly ColumnMapper _columnMapper;

        public string BackupFilePath { get; private set; }
        public string UpgradesPath { get; private set; }
        public bool UnblockPortalAfterCompleted { get; set; }
        public bool ReplaceDate { get; set; }
        public bool Dump { get; set; }

        public RestorePortalTask(ILog logger, string toConfigPath, string fromFilePath, ColumnMapper columnMapper = null, string upgradesPath = null)
            : this(logger, -1, toConfigPath, fromFilePath, columnMapper, upgradesPath)
        {

        }

        public RestorePortalTask(ILog logger, int tenantId, string toConfigPath, string fromFilePath, ColumnMapper columnMapper = null, string upgradesPath = null)
            : base(logger, tenantId, toConfigPath)
        {
            if (fromFilePath == null)
                throw new ArgumentNullException("fromFilePath");

            if (!File.Exists(fromFilePath))
                throw new FileNotFoundException("file not found at given path");

            BackupFilePath = fromFilePath;
            UpgradesPath = upgradesPath;
            _columnMapper = columnMapper ?? new ColumnMapper();
        }

        public override void RunJob()
        {
            Logger.Debug("begin restore portal");

            Logger.Debug($"begin restore data from {BackupFilePath}");

            using (var dataReader = new ZipReadOperator(BackupFilePath))
            {
                using (var entry = dataReader.GetEntry(KeyHelper.GetDumpKey()))
                {
                    Dump = entry != null && CoreContext.Configuration.Standalone;
                }

                Logger.Debug($"restore data dbFactory: {ConfigPath}");

                var dbFactory = new DbFactory(ConfigPath);
                if (Dump)
                {
                    Logger.Debug("begin restore from dump.");

                    RestoreFromDump(dataReader);
                }
                else
                {
                    Logger.Debug("begin restore not from dump.");

                    var modulesToProcess = GetModulesToProcess().ToList();
                    SetStepsCount(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);

                    foreach (var module in modulesToProcess)
                    {
                        var restoreTask = new RestoreDbModuleTask(Logger, module, dataReader, _columnMapper, dbFactory, ReplaceDate, Dump);
                        restoreTask.ProgressChanged += (sender, args) => SetCurrentStepProgress(args.Progress);
                        foreach (var tableName in IgnoredTables)
                        {
                            restoreTask.IgnoreTable(tableName);
                        }
                        restoreTask.RunJob();
                    }
                    RestoreMailTable(dataReader);

                    var backupRepository = BackupStorageFactory.GetBackupRepository();
                    backupRepository.MigrationBackupRecords(TenantId, _columnMapper.GetTenantMapping(), ConfigPath);
                }

                Logger.Debug("end restore data");

                if (ProcessStorage)
                {
                    if (CoreContext.Configuration.Standalone)
                    {
                        Logger.Debug("clear cache");
                        AscCache.ClearCache();
                    }
                    DoRestoreStorage(dataReader);
                }
                if (UnblockPortalAfterCompleted)
                {
                    SetTenantActive(dbFactory, _columnMapper.GetTenantMapping());
                }
            }

            if (CoreContext.Configuration.Standalone)
            {
                Logger.Debug("refresh license");
                try
                {
                    LicenseReader.RejectLicense();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                Logger.Debug("clear cache");
                AscCache.ClearCache();
            }

            Logger.Debug("end restore portal");
        }

        private void RestoreMailTable(IDataReadOperator dataReader)
        {
            if (!CoreContext.Configuration.Standalone || !dataReader.GetDirectories("").Any(r => r.EndsWith("mailtable")))
            {
                Logger.Debug($"RestoreMailTable: Not Standalone, skip restore.");

                return;
            }

            try
            {
                string dbconnection = null;
                using (var dbManager = new DbManager("default", 100000))
                {
                    dbManager.ExecuteList("select connection_string from mail_server_server").ForEach(r =>
                    {
                        dbconnection = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(r[0]))["DbConnection"].ToString();
                    });
                }
                Logger.Debug($"RestoreMailTable connection string: {dbconnection}");

                if (string.IsNullOrEmpty(dbconnection)) dbconnection = "Server=onlyoffice-mysql-server;Database=onlyoffice_mailserver;User ID=root;Password=my-secret-pw;Pooling=True;Character Set=utf8;AutoEnlist=false;SSL Mode=None;Connection Timeout=30;Maximum Pool Size=300;";

                Logger.Debug($"RestoreMailTable connection string: {dbconnection}");

                if (dbconnection != null)
                {
                    var restoreTask = new RestoreMailTableTask(Logger, dataReader, ConfigPath, dbconnection);
                    restoreTask.RunJob();
                }

                using (var dbManager = new DbManager("default", 100000))
                {
                    //set new domain name in dns record
                    dbManager.ExecuteNonQuery("update mail_server_dns set mx=(select hostname from mail_mailbox_server where id_provider=-1 limit 1)");

                    dbManager.ExecuteNonQuery("update mail_mailbox set id_smtp_server = (select id from mail_mailbox_server where id_provider = -1 and type = 'smtp' limit 1), id_in_server = (select id from mail_mailbox_server where id_provider = -1 and type = 'imap' limit 1) where is_server_mailbox=1");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"RestoreMailTable error: {ex}");
            }
        }

        private void RestoreFromDump(IDataReadOperator dataReader)
        {
            var keyBase = KeyHelper.GetDatabaseSchema();
            var keys = dataReader.GetEntries(keyBase).Select(r => Path.GetFileName(r)).ToList();
            var dbs = dataReader.GetDirectories("").Where(r => Path.GetFileName(r).StartsWith("mailservice")).Select(r => Path.GetFileName(r)).ToList();
            var upgrades = new List<string>();

            var upgradesPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), UpgradesPath));
            if (!string.IsNullOrEmpty(upgradesPath) && Directory.Exists(upgradesPath))
            {
                upgrades = Directory.GetFiles(upgradesPath).ToList();
            }

            var stepscount = keys.Count * 2 + upgrades.Count;

            Dictionary<string, List<string>> databases = new Dictionary<string, List<string>>();
            foreach (var db in dbs)
            {
                var keys1 = dataReader.GetEntries(db + "/" + keyBase).Select(k => Path.GetFileName(k)).ToList();
                stepscount += keys1.Count() * 2;
                databases.Add(db, keys1);
            }

            if (ProcessStorage)
            {
                var storageModules = StorageFactory.GetModuleList(ConfigPath).Where(IsStorageModuleAllowed);
                var tenants = CoreContext.TenantManager.GetTenants(false);

                stepscount += storageModules.Count() * tenants.Count;

                SetStepsCount(stepscount + 1);

                DoDeleteStorage(storageModules, tenants);
            }
            else
            {
                SetStepsCount(stepscount);
            }

            for (var i = 0; i < keys.Count; i += TasksLimit)
            {
                var tasks = new List<Task>(TasksLimit * 2);

                for (var j = 0; j < TasksLimit && i + j < keys.Count; j++)
                {
                    var key1 = Path.Combine(KeyHelper.GetDatabaseSchema(), keys[i + j]);
                    var key2 = Path.Combine(KeyHelper.GetDatabaseData(), keys[i + j]);
                    tasks.Add(RestoreFromDumpFile(dataReader, key1, key2));
                }

                Task.WaitAll(tasks.ToArray());
            }

            using (var dbManager = new DbManager("default", 100000))
            {
                dbManager.ExecuteList("select id, connection_string from mail_server_server").ForEach(r =>
                {
                    RegisterDatabase((int)r[0], JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(r[1]))["DbConnection"].ToString());
                });
            }

            foreach (var database in databases)
            {

                for (var i = 0; i < database.Value.Count; i += TasksLimit)
                {
                    var tasks = new List<Task>(TasksLimit * 2);

                    for (var j = 0; j < TasksLimit && i + j < database.Value.Count; j++)
                    {
                        var key1 = Path.Combine(database.Key, KeyHelper.GetDatabaseSchema(), database.Value[i + j]);
                        var key2 = Path.Combine(database.Key, KeyHelper.GetDatabaseData(), database.Value[i + j]);
                        tasks.Add(RestoreFromDumpFile(dataReader, key1, key2, database.Key));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
            }

            var comparer = new SqlComparer();
            foreach (var u in upgrades.OrderBy(Path.GetFileName, comparer))
            {
                using (var s = File.OpenRead(u))
                {
                    if (u.Contains(".upgrade."))
                    {
                        RunMysqlFile(s, "default", null).Wait();
                    }
                    else if (u.Contains(".data") || u.Contains(".upgrade"))
                    {
                        RunMysqlProcedure(s).Wait();
                    }
                    else
                    {
                        RunMysqlFile(s, "default").Wait();
                    }
                }

                SetStepCompleted();
            }
        }

        private async Task RestoreFromDumpFile(IDataReadOperator dataReader, string fileName1, string fileName2 = null, string db = "default")
        {
            Logger.DebugFormat("Restore from {0}", fileName1);
            using (var stream = dataReader.GetEntry(fileName1))
            {
                await RunMysqlFile(stream, db);
            }
            SetStepCompleted();

            Logger.DebugFormat("Restore from {0}", fileName2);
            if (fileName2 != null)
            {
                using (var stream = dataReader.GetEntry(fileName2))
                {
                    await RunMysqlFile(stream, db);
                }

                SetStepCompleted();
            }
        }

        private class SqlComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y)
                {
                    return 0;
                }

                if (!string.IsNullOrEmpty(x))
                {
                    var splittedX = x.Split('.');
                    if (splittedX.Length <= 2) return -1;
                    if (splittedX[1] == "upgrade") return 1;

                    if (splittedX[1].StartsWith("upgrade") && !string.IsNullOrEmpty(y))
                    {
                        var splittedY = y.Split('.');
                        if (splittedY.Length <= 2) return 1;
                        if (splittedY[1] == "upgrade") return -1;

                        if (splittedY[1].StartsWith("upgrade"))
                        {
                            return string.Compare(x, y, StringComparison.Ordinal);
                        }

                        return -1;
                    }

                    return -1;
                }

                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }

        private void DoRestoreStorage(IDataReadOperator dataReader)
        {
            Logger.Debug("begin restore storage");

            var fileGroups = GetFilesToProcess(dataReader).GroupBy(file => file.Module).ToList();
            var groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                foreach (var file in group)
                {
                    var storage = StorageFactory.GetStorage(ConfigPath, Dump ? file.Tenant.ToString() : _columnMapper.GetTenantMapping().ToString(), group.Key);
                    var quotaController = storage.QuotaController;
                    storage.SetQuotaController(null);

                    try
                    {
                        var adjustedPath = file.Path;
                        var module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                        if (module == null || module.TryAdjustFilePath(Dump, _columnMapper, ref adjustedPath))
                        {
                            var key = file.GetZipKey();
                            if (Dump)
                            {
                                key = Path.Combine(KeyHelper.GetStorage(), key);
                            }
                            using (var stream = dataReader.GetEntry(key))
                            {
                                try
                                {
                                    storage.Save(file.Domain, adjustedPath, module != null ? module.PrepareData(key, stream, _columnMapper) : stream);
                                }
                                catch (Exception error)
                                {
                                    Logger.WarnFormat("can't restore file ({0}:{1}): {2}", file.Module, file.Path, error);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (quotaController != null)
                        {
                            storage.SetQuotaController(quotaController);
                        }
                    }
                }

                SetCurrentStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
            {
                SetStepCompleted();
            }
            Logger.Debug("end restore storage");
        }

        private void DoDeleteStorage(IEnumerable<string> storageModules, IEnumerable<Tenant> tenants)
        {
            Logger.Debug("begin delete storage");

            foreach (var tenant in tenants)
            {
                foreach (var module in storageModules)
                {
                    var storage = StorageFactory.GetStorage(ConfigPath, tenant.TenantId.ToString(), module);
                    var domains = StorageFactory.GetDomainList(ConfigPath, module).ToList();

                    domains.Add(string.Empty); //instead storage.DeleteFiles("\\", "*.*", true);

                    foreach (var domain in domains)
                    {
                        ActionInvoker.Try(
                            state =>
                            {
                                if (storage.IsDirectory((string)state))
                                {
                                    storage.DeleteFiles((string)state, "\\", "*.*", true);
                                }
                            },
                            domain,
                            5,
                            onFailure: error => Logger.WarnFormat("Can't delete files for domain {0}: \r\n{1}", domain, error)
                        );
                    }

                    SetStepCompleted();
                }
            }

            Logger.Debug("end delete storage");
        }

        private static IEnumerable<BackupFileInfo> GetFilesToProcess(IDataReadOperator dataReader)
        {
            using (var stream = dataReader.GetEntry(KeyHelper.GetStorageRestoreInfoZipKey()))
            {
                if (stream == null)
                {
                    return Enumerable.Empty<BackupFileInfo>();
                }
                var restoreInfo = XElement.Load(new StreamReader(stream));
                return restoreInfo.Elements("file").Select(BackupFileInfo.FromXElement).ToList();
            }
        }

        private static void SetTenantActive(DbFactory dbFactory, int tenantId)
        {
            using (var connection = dbFactory.OpenConnection())
            {
                var commandText = string.Format(
                    "update tenants_tenants " +
                    "set " +
                    "  status={0}, " +
                    "  last_modified='{1}', " +
                    "  statuschanged='{1}' " +
                    "where id = '{2}'",
                    (int)TenantStatus.Active,
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    tenantId);

                connection.CreateCommand(commandText).WithTimeout(120).ExecuteNonQuery();
            }
        }
    }
}
