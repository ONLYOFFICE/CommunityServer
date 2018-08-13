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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Logging;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Tasks
{
    public class RestorePortalTask : PortalTaskBase
    {
        private readonly ColumnMapper _columnMapper;

        public string BackupFilePath { get; private set; }
        public bool UnblockPortalAfterCompleted { get; set; }
        public bool ReplaceDate { get; set; }

        public RestorePortalTask(ILog logger, string toConfigPath, string fromFilePath, ColumnMapper columnMapper = null)
            : this(logger, -1, toConfigPath, fromFilePath, columnMapper)
        {

        }

        public RestorePortalTask(ILog logger, int tenantId, string toConfigPath, string fromFilePath, ColumnMapper columnMapper = null)
            : base(logger, tenantId, toConfigPath)
        {
            if (fromFilePath == null)
                throw new ArgumentNullException("fromFilePath");

            if (!File.Exists(fromFilePath))
                throw new FileNotFoundException("file not found at given path");

            BackupFilePath = fromFilePath;
            _columnMapper = columnMapper ?? new ColumnMapper();
        }

        public override void RunJob()
        {
            Logger.Debug("begin restore portal");

            List<IModuleSpecifics> modulesToProcess = GetModulesToProcess().ToList();
            SetStepsCount(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);

            Logger.Debug("begin restore data");

            using (var dataReader = new ZipReadOperator(BackupFilePath))
            {
                var dbFactory = new DbFactory(ConfigPath);
                foreach (var module in modulesToProcess)
                {
                    var restoreTask = new RestoreDbModuleTask(Logger, module, dataReader, _columnMapper, dbFactory,
                        ReplaceDate);
                    restoreTask.ProgressChanged += (sender, args) => SetCurrentStepProgress(args.Progress);
                    foreach (var tableName in IgnoredTables)
                    {
                        restoreTask.IgnoreTable(tableName);
                    }
                    restoreTask.RunJob();
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
                AscCache.Default.Remove(new Regex(".*"));
            }

            Logger.Debug("end restore portal");
        }

        private void DoRestoreStorage(IDataReadOperator dataReader)
        {
            Logger.Debug("begin restore storage");

            var fileGroups = GetFilesToProcess(dataReader).GroupBy(file => file.Module).ToList();
            var groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                var storage = StorageFactory.GetStorage(ConfigPath, _columnMapper.GetTenantMapping().ToString(), group.Key);
                var quotaController = storage.QuotaController;
                try
                {
                    storage.SetQuotaController(null);
                    foreach (var file in group)
                    {
                        var adjustedPath = file.Path;
                        var module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                        if (module == null || module.TryAdjustFilePath(_columnMapper, ref adjustedPath))
                        {
                            using (var stream = dataReader.GetEntry(KeyHelper.GetFileZipKey(file)))
                            {
                                try
                                {
                                    storage.Save(file.Domain, adjustedPath, module != null ? module.PrepareData(KeyHelper.GetFileZipKey(file), stream, _columnMapper) : stream);
                                }
                                catch (Exception error)
                                {
                                    Logger.Warn("can't restore file ({0}:{1}): {2}", file.Module, file.Path, error);
                                }
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
                SetCurrentStepProgress((int)(++groupsProcessed*100/(double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
            {
                SetStepCompleted();
            }
            Logger.Debug("end restore storage");
        }

        private IEnumerable<BackupFileInfo> GetFilesToProcess(IDataReadOperator dataReader)
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

        protected override IEnumerable<BackupFileInfo> GetFilesToProcess()
        {
            throw new NotImplementedException();
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
