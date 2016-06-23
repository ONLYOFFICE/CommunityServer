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
using System.Data;
using System.IO;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Common.Data;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Logging;
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

        public TransferPortalTask(ILog logger, int tenantId, string fromConfigPath, string toConfigPath)
            : base(logger, tenantId, fromConfigPath)
        {
            if (toConfigPath == null)
                throw new ArgumentNullException("toConfigPath");

            ToConfigPath = toConfigPath;

            DeleteBackupFileAfterCompletion = true;
            BlockOldPortalAfterStart = true;
            DeleteOldPortalAfterCompletion = true;
        }

        public override void RunJob()
        {
            Logger.Debug("begin transfer {0}", TenantId);
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
                var backupTask = new BackupPortalTask(Logger, TenantId, ConfigPath, backupFilePath) {ProcessStorage = false};
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
                Logger.Debug("end transfer {0}", TenantId);
            }
        }

        private void DoTransferStorage(ColumnMapper columnMapper)
        {
            Logger.Debug("begin transfer storage");
            var fileGroups = GetFilesToProcess().GroupBy(file => file.Module).ToList();
            int groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                ICrossModuleTransferUtility transferUtility =
                    StorageFactory.GetCrossModuleTransferUtility(
                        ConfigPath, TenantId, group.Key,
                        ToConfigPath, columnMapper.GetTenantMapping(), group.Key);

                foreach (BackupFileInfo file in group)
                {
                    string adjustedPath = file.Path;

                    IModuleSpecifics module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                    if (module == null || module.TryAdjustFilePath(columnMapper, ref adjustedPath))
                    {
                        try
                        {
                            transferUtility.CopyFile(file.Domain, file.Path, file.Domain, adjustedPath);
                        }
                        catch (Exception error)
                        {
                            Logger.Warn("Can't copy file ({0}:{1}): {2}", file.Module, file.Path, error);
                        }
                    }
                    else
                    {
                        Logger.Warn("Can't adjust file path \"{0}\".", file.Path);
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

        private static string GetUniqAlias(IDbConnection connection, string alias)
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
