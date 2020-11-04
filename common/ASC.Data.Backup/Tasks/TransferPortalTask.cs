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
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ASC.Core.Tenants;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Tasks
{
    public class TransferPortalTask : PortalTaskBase
    {
        public const string DefaultDirectoryName = "backup";

        public string ToConfigPath { get; private set; }

        public string BackupDirectory { get; set; }
        public bool DeleteBackupFileAfterCompletion { get; set; }
        public bool BlockOldPortalAfterStart { get; set; }
        public bool DeleteOldPortalAfterCompletion { get; set; }

        public int Limit { get; private set; }

        public TransferPortalTask(ILog logger, int tenantId, string fromConfigPath, string toConfigPath, int limit)
            : base(logger, tenantId, fromConfigPath)
        {
            if (toConfigPath == null)
                throw new ArgumentNullException("toConfigPath");

            ToConfigPath = toConfigPath;

            DeleteBackupFileAfterCompletion = true;
            BlockOldPortalAfterStart = true;
            DeleteOldPortalAfterCompletion = true;
            Limit = limit;
        }

        public override void RunJob()
        {
            Logger.DebugFormat("begin transfer {0}", TenantId);
            var fromDbFactory = new DbFactory(ConfigPath);
            var toDbFactory = new DbFactory(ToConfigPath);
            string tenantAlias = GetTenantAlias(fromDbFactory);
            string backupFilePath = GetBackupFilePath(tenantAlias);
            var columnMapper = new ColumnMapper();
            try
            {
                //target db can have error tenant from the previous attempts
                SaveTenant(toDbFactory, tenantAlias, TenantStatus.RemovePending, tenantAlias + "_error", "status = " + TenantStatus.Restoring.ToString("d"));

                if (BlockOldPortalAfterStart)
                {
                    SaveTenant(fromDbFactory, tenantAlias, TenantStatus.Transfering);
                }

                SetStepsCount(ProcessStorage ? 3 : 2);

                //save db data to temporary file
                var backupTask = new BackupPortalTask(Logger, TenantId, ConfigPath, backupFilePath, Limit) {ProcessStorage = false};
                backupTask.ProgressChanged += (sender, args) => SetCurrentStepProgress(args.Progress);
                foreach (var moduleName in IgnoredModules)
                {
                    backupTask.IgnoreModule(moduleName);
                }
                backupTask.RunJob();
                
                //restore db data from temporary file
                var restoreTask = new RestorePortalTask(Logger, ToConfigPath, backupFilePath, columnMapper) {ProcessStorage = false};
                restoreTask.ProgressChanged += (sender, args) => SetCurrentStepProgress(args.Progress);
                foreach (var moduleName in IgnoredModules)
                {
                    restoreTask.IgnoreModule(moduleName);
                }
                restoreTask.RunJob();

                //transfer files
                if (ProcessStorage)
                {
                    DoTransferStorage(columnMapper);
                }

                SaveTenant(toDbFactory, tenantAlias, TenantStatus.Active);
                if (DeleteOldPortalAfterCompletion)
                {
                    SaveTenant(fromDbFactory, tenantAlias, TenantStatus.RemovePending, tenantAlias + "_deleted");
                }
                else if (BlockOldPortalAfterStart)
                {
                    SaveTenant(fromDbFactory, tenantAlias, TenantStatus.Active);
                }
            }
            catch
            {
                SaveTenant(fromDbFactory, tenantAlias, TenantStatus.Active);
                if (columnMapper.GetTenantMapping() > 0)
                {
                    SaveTenant(toDbFactory, tenantAlias, TenantStatus.RemovePending, tenantAlias + "_error");
                }
                throw;
            }
            finally
            {
                if (DeleteBackupFileAfterCompletion)
                {
                    File.Delete(backupFilePath);
                }
                Logger.DebugFormat("end transfer {0}", TenantId);
            }
        }

        private void DoTransferStorage(ColumnMapper columnMapper)
        {
            Logger.Debug("begin transfer storage");
            var fileGroups = GetFilesToProcess(TenantId).GroupBy(file => file.Module).ToList();
            int groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                var baseStorage = StorageFactory.GetStorage(ConfigPath, TenantId.ToString(), group.Key);
                var destStorage = StorageFactory.GetStorage(ToConfigPath, columnMapper.GetTenantMapping().ToString(), group.Key);
                var utility = new CrossModuleTransferUtility(baseStorage, destStorage);

                foreach (BackupFileInfo file in group)
                {
                    string adjustedPath = file.Path;

                    IModuleSpecifics module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                    if (module == null || module.TryAdjustFilePath(false, columnMapper, ref adjustedPath))
                    {
                        try
                        {
                            utility.CopyFile(file.Domain, file.Path, file.Domain, adjustedPath);
                        }
                        catch (Exception error)
                        {
                            Logger.WarnFormat("Can't copy file ({0}:{1}): {2}", file.Module, file.Path, error);
                        }
                    }
                    else
                    {
                        Logger.WarnFormat("Can't adjust file path \"{0}\".", file.Path);
                    }
                }
                SetCurrentStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
                SetStepCompleted();

            Logger.Debug("end transfer storage");
        }

        private void SaveTenant(DbFactory dbFactory, string alias, TenantStatus status, string newAlias = null, string whereCondition = null)
        {
            using (var connection = dbFactory.OpenConnection())
            {
                if (newAlias == null)
                {
                    newAlias = alias;
                }
                else if (newAlias != alias)
                {
                    newAlias = GetUniqAlias(connection, newAlias);
                }

                var commandText = string.Format(
                    "update tenants_tenants " +
                    "set " +
                    "  status={0}, " +
                    "  alias = '{1}', " +
                    "  last_modified='{2}', " +
                    "  statuschanged='{2}' " +
                    "where alias = '{3}'",
                    status.ToString("d"),
                    newAlias,
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    alias);

                if (!string.IsNullOrEmpty(whereCondition))
                    commandText += (" and " + whereCondition);

                connection.CreateCommand(commandText).WithTimeout(120).ExecuteNonQuery();
            }
        }

        private string GetTenantAlias(DbFactory dbFactory)
        {
            using (var connection = dbFactory.OpenConnection())
            {
                var commandText = "select alias from tenants_tenants where id = " + TenantId;
                return connection.CreateCommand(commandText).WithTimeout(120).ExecuteScalar<string>();
            }
        }

        private static string GetUniqAlias(DbConnection connection, string alias)
        {
            return alias + connection.CreateCommand("select count(*) from tenants_tenants where alias like '" + alias + "%'")
                                     .WithTimeout(120)
                                     .ExecuteScalar<int>();
        }

        private string GetBackupFilePath(string tenantAlias)
        {
            if (!Directory.Exists(BackupDirectory ?? DefaultDirectoryName))
                Directory.CreateDirectory(BackupDirectory ?? DefaultDirectoryName);

            return Path.Combine(BackupDirectory ?? DefaultDirectoryName, tenantAlias + DateTime.UtcNow.ToString("(yyyy-MM-dd HH-mm-ss)") + ".backup");
        }
    }
}
