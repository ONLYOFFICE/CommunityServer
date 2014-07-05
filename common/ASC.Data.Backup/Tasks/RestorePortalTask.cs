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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ASC.Common.Data;
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
        public bool UnblockPortalAfterCompleted { get; set; }
        public bool ReplaceDate { get; set; }

        public RestorePortalTask(string toConfigPath, string fromFilePath, ColumnMapper columnMapper = null)
            : base(null, toConfigPath)
        {
            if (fromFilePath == null)
                throw new ArgumentNullException("fromFilePath");

            if (!File.Exists(fromFilePath))
                throw new FileNotFoundException("file not found at given path");

            BackupFilePath = fromFilePath;
            _columnMapper = columnMapper ?? new ColumnMapper();
        }

        public override void Run()
        {
            InvokeInfo("begin restore portal");
            
            List<IModuleSpecifics> modulesToProcess = GetModulesToProcess().ToList();
            InitProgress(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);

            using (var dataReader = new ZipReadOperator(BackupFilePath))
            {
                InvokeInfo("begin restore data");

                var dbFactory = new DbFactory(ConfigPath);
                foreach (var module in modulesToProcess)
                {
                    var restoreTask = new RestoreDbModuleTask(module, dataReader, _columnMapper, dbFactory, ReplaceDate);
                    foreach (var tableName in IgnoredTables)
                    {
                        restoreTask.IgnoreTable(tableName);
                    }
                    RunSubtask(restoreTask);
                }

                InvokeInfo("end restore data");

                if (ProcessStorage)
                    DoRestoreStorage(dataReader);

                if (UnblockPortalAfterCompleted)
                    SetTenantActive(dbFactory, _columnMapper.GetTenantMapping());
            }

            InvokeInfo("end restore portal");
        }

        private void DoRestoreStorage(IDataReadOperator dataReader)
        {
            InvokeInfo("begin restore storage");
            var fileGroups = GetFilesToProcess(dataReader).GroupBy(file => file.Module).ToList();
            int groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                IDataStore storage = StorageFactory.GetStorage(ConfigPath, _columnMapper.GetTenantMapping().ToString(), group.Key, null, null);
                foreach (BackupFileInfo file in group)
                {
                    string adjustedPath = file.Path;

                    IModuleSpecifics module = ModuleProvider.GetByStorageModule(file.Module);
                    if (module == null || module.TryAdjustFilePath(_columnMapper, ref adjustedPath))
                    {
                        Stream stream = dataReader.GetEntry(KeyHelper.GetFileZipKey(file));
                        try
                        {
                            storage.Save(file.Domain, adjustedPath, stream);
                        }
                        catch (Exception error)
                        {
                            InvokeWarning("can't restore file ({0}:{1}): {2}", file.Module, file.Path, error);
                        }
                    }
                }
                SetStepProgress((int)(++groupsProcessed*100/(double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
                SetStepCompleted();

            InvokeInfo("end restore storage");
        }

        private IEnumerable<BackupFileInfo> GetFilesToProcess(IDataReadOperator dataReader)
        {
            using (Stream stream = dataReader.GetEntry(KeyHelper.GetStorageRestoreInfoZipKey()))
            {
                if (stream == null)
                    return Enumerable.Empty<BackupFileInfo>();

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
            using (var connection = dbFactory.OpenConnection("core"))
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
