/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;

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

            Logger.Debug("begin restore data");

            using (var dataReader = new ZipReadOperator(BackupFilePath))
            {
                using (var entry = dataReader.GetEntry(KeyHelper.GetDumpKey()))
                {
                    Dump = entry != null;
                }

                var dbFactory = new DbFactory(ConfigPath);
                if (Dump)
                {
                    RestoreFromDump(dataReader, dbFactory);
                }
                else
                {
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
                }

                Logger.Debug("end restore data");

                if (ProcessStorage)
                {
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

        private void RestoreFromDump(IDataReadOperator dataReader, DbFactory dbFactory)
        {
            var keyBase = KeyHelper.GetDatabaseSchema();
            var keys = dataReader.Entries.Where(r => r.StartsWith(keyBase)).ToList();
            var upgrades = new List<string>();

            if (!string.IsNullOrEmpty(UpgradesPath) && Directory.Exists(UpgradesPath))
            {
                upgrades = Directory.GetFiles(UpgradesPath).ToList();
            }

            var stepscount = keys.Count*2 + upgrades.Count;

            SetStepsCount(ProcessStorage ? stepscount + 1 : stepscount);

            for (var i = 0; i < keys.Count; i += TasksLimit)
            {
                var tasks = new List<Task>(TasksLimit * 2);

                for (var j = 0; j < TasksLimit && i+j < keys.Count; j++)
                {
                    var key1 = keys[i + j];
                    tasks.Add(RestoreFromDumpFile(dataReader, key1).ContinueWith(r => RestoreFromDumpFile(dataReader, KeyHelper.GetDatabaseData(key1.Substring(keyBase.Length + 1)))));
                }

                Task.WaitAll(tasks.ToArray());
            }

            var comparer = new SqlComparer();
            foreach (var u in upgrades.OrderBy(Path.GetFileName, comparer))
            {
                RunMysqlFile(dbFactory, u, true);
                SetStepCompleted();
            }
        }

        private async Task RestoreFromDumpFile(IDataReadOperator dataReader, string fileName)
        {
            Logger.DebugFormat("Restore from {0}", fileName);
            using (var stream = dataReader.GetEntry(fileName))
            {
                await RunMysqlFile(stream);
            }
            SetStepCompleted();
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

                SetCurrentStepProgress((int)(++groupsProcessed*100/(double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
            {
                SetStepCompleted();
            }
            Logger.Debug("end restore storage");
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


        protected override bool IsStorageModuleAllowed(string storageModuleName)
        {
            if (storageModuleName == "fckuploaders")
                return false;
            return base.IsStorageModuleAllowed(storageModuleName);
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
