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
using System.Data;
using System.IO;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Common.Data;
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

        public TransferPortalTask(Tenant tenant, string fromConfigPath, string toConfigPath)
            : base(tenant, fromConfigPath)
        {
            if (toConfigPath == null)
                throw new ArgumentNullException("toConfigPath");

            ToConfigPath = toConfigPath;

            DeleteBackupFileAfterCompletion = true;
            BlockOldPortalAfterStart = true;
            DeleteOldPortalAfterCompletion = true;
        }

        public override void Run()
        {
            InvokeInfo("begin transfer portal ({0})", Tenant.TenantAlias);
            string backupFilePath = GetBackupFilePath();
            var fromDbFactory = new DbFactory(ConfigPath);
            var toDbFactory = new DbFactory(ToConfigPath);
            var columnMapper = new ColumnMapper();
            try
            {
                //target db can have error tenant from the previous attempts
                SaveTenant(toDbFactory, TenantStatus.RemovePending, Tenant.TenantAlias + "_error", "status = " + TenantStatus.Restoring.ToString("d"));

                if (BlockOldPortalAfterStart)
                {
                    SaveTenant(fromDbFactory, TenantStatus.Transfering);
                }

                InitProgress(ProcessStorage ? 3 : 2);

                //save db data to temporary file
                var backupTask = new BackupPortalTask(Tenant, ConfigPath, backupFilePath) {ProcessStorage = false};
                foreach (var moduleName in IgnoredModules)
                {
                    backupTask.IgnoreModule(moduleName);
                }
                RunSubtask(backupTask);
                
                //restore db data from temporary file
                var restoreTask = new RestorePortalTask(ToConfigPath, backupFilePath, columnMapper) {ProcessStorage = false};
                foreach (var moduleName in IgnoredModules)
                {
                    restoreTask.IgnoreModule(moduleName);
                }
                RunSubtask(restoreTask);

                //transfer files
                if (ProcessStorage)
                {
                    DoTransferStorage(columnMapper);
                }

                SaveTenant(toDbFactory, TenantStatus.Active);
                if (DeleteOldPortalAfterCompletion)
                {
                    SaveTenant(fromDbFactory, TenantStatus.RemovePending, Tenant.TenantAlias + "_deleted");
                }
                else if (BlockOldPortalAfterStart)
                {
                    SaveTenant(fromDbFactory, TenantStatus.Active);
                }
            }
            catch
            {
                SaveTenant(fromDbFactory, TenantStatus.Active);
                if (columnMapper.GetTenantMapping() > 0)
                {
                    SaveTenant(toDbFactory, TenantStatus.RemovePending, Tenant.TenantAlias + "_error");
                }
                throw;
            }
            finally
            {
                if (DeleteBackupFileAfterCompletion)
                {
                    File.Delete(backupFilePath);
                }
                InvokeInfo("end transfer portal ({0})", Tenant.TenantAlias);
            }
        }

        private void DoTransferStorage(ColumnMapper columnMapper)
        {
            InvokeInfo("begin transfer storage");
            var fileGroups = GetFilesToProcess().GroupBy(file => file.Module).ToList();
            int groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                ICrossModuleTransferUtility transferUtility =
                    StorageFactory.GetCrossModuleTransferUtility(
                        ConfigPath, Tenant.TenantId, group.Key,
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
                            InvokeWarning("Can't copy file ({0}:{1}): {2}", file.Module, file.Path, error);
                        }
                    }
                    else
                    {
                        InvokeWarning("Can't adjust file path \"{0}\".", file.Path);
                    }
                }
                SetStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
                SetStepCompleted();

            InvokeInfo("end transfer storage");
        }

        private void SaveTenant(DbFactory dbFactory, TenantStatus status, string newAlias = null, string whereCondition = null)
        {
            using (var connection = dbFactory.OpenConnection("core"))
            {
                if (newAlias == null)
                {
                    newAlias = Tenant.TenantAlias;
                }
                else if (newAlias != Tenant.TenantAlias)
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
                    Tenant.TenantAlias);

                if (!string.IsNullOrEmpty(whereCondition))
                    commandText += (" and " + whereCondition);

                connection.CreateCommand(commandText).WithTimeout(120).ExecuteNonQuery();
            }
        }

        private static string GetUniqAlias(IDbConnection connection, string alias)
        {
            return alias + connection.CreateCommand("select count(*) from tenants_tenants where alias like '" + alias + "%'")
                                     .WithTimeout(120)
                                     .ExecuteScalar<int>();
        }

        private string GetBackupFilePath()
        {
            if (!Directory.Exists(BackupDirectory ?? DefaultDirectoryName))
                Directory.CreateDirectory(BackupDirectory ?? DefaultDirectoryName);

            return Path.Combine(BackupDirectory ?? DefaultDirectoryName, Tenant.TenantAlias + DateTime.UtcNow.ToString("(yyyy-MM-dd HH-mm-ss)") + ".backup");
        }
    }
}
