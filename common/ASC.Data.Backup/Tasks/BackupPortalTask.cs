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
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ASC.Data.Backup.Logging;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Backup.Extensions;
using ASC.Data.Storage;
using ASC.Data.Backup.Exceptions;

namespace ASC.Data.Backup.Tasks
{
    public class BackupPortalTask : PortalTaskBase
    {
        public string BackupFilePath { get; private set; }

        public BackupPortalTask(ILog logger, int tenantId, string fromConfigPath, string toFilePath)
            : base(logger, tenantId, fromConfigPath)
        {
            if (string.IsNullOrEmpty(toFilePath))
                throw new ArgumentNullException("toFilePath");

            BackupFilePath = toFilePath;
        }

        public override void RunJob()
        {
            Logger.Debug("begin backup {0}", TenantId);
            List<IModuleSpecifics> modulesToProcess = GetModulesToProcess().ToList();
            SetStepsCount(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);
            using (var writer = new ZipWriteOperator(BackupFilePath))
            {
                var dbFactory = new DbFactory(ConfigPath);
                foreach (var module in modulesToProcess)
                {
                    DoBackupModule(writer, dbFactory, module);
                }
                if (ProcessStorage)
                {
                    DoBackupStorage(writer, dbFactory);
                }
            }
            Logger.Debug("end backup {0}", TenantId);
        }

        private void DoBackupModule(IDataWriteOperator writer, DbFactory dbFactory, IModuleSpecifics module)
        {
            Logger.Debug("begin saving data for module {0}", module.ModuleName);
            var tablesToProcess = module.Tables.Where(t => !IgnoredTables.Contains(t.Name) && t.InsertMethod != InsertMethod.None).ToList();
            int tablesCount = tablesToProcess.Count;
            int tablesProcessed = 0;
            using (var connection = dbFactory.OpenConnection())
            {
                foreach (var table in tablesToProcess)
                {
                    Logger.Debug("begin load table {0}", table.Name);
                    using (var data = new DataTable(table.Name))
                    {
                        ActionInvoker.Try(
                            state =>
                            {
                                data.Clear();
                                var t = (TableInfo)state;
                                var dataAdapter = dbFactory.CreateDataAdapter();
                                dataAdapter.SelectCommand = module.CreateSelectCommand(connection.Fix(), TenantId, t).WithTimeout(600);
                                ((DbDataAdapter)dataAdapter).Fill(data);

                            },
                            table,
                            maxAttempts: 5,
                            onFailure: error => { throw ThrowHelper.CantBackupTable(table.Name, error); },
                            onAttemptFailure: error => Logger.Warn("backup attempt failure: {0}", error));

                        foreach (var col in data.Columns.Cast<DataColumn>().Where(col => col.DataType == typeof(DateTime)))
                        {
                            col.DateTimeMode = DataSetDateTime.Unspecified;
                        }

                        module.PrepareData(data);

                        Logger.Debug("end load table {0}", table.Name);

                        Logger.Debug("begin saving table {0}", table.Name);

                        var tmp = Path.GetTempFileName();
                        using (var file = File.OpenWrite(tmp))
                        {
                            data.WriteXml(file, XmlWriteMode.WriteSchema);
                            data.Clear();
                        }

                        writer.WriteEntry(KeyHelper.GetTableZipKey(module, data.TableName), tmp);
                        File.Delete(tmp);

                        Logger.Debug("end saving table {0}", table.Name);
                    }

                    SetCurrentStepProgress((int)((++tablesProcessed * 100) / (double)tablesCount));
                }
            }
            Logger.Debug("end saving data for module {0}", module.ModuleName);
        }

        private void DoBackupStorage(IDataWriteOperator writer, DbFactory dbFactory)
        {
            Logger.Debug("begin backup storage");

            var files = GetFilesToProcess();
            var exclude = new List<string>();
            using (var db = dbFactory.OpenConnection())
            using (var command = db.CreateCommand())
            {
                command.CommandText = "select storage_path from backup_backup where tenant_id = " + TenantId + " and storage_type = 0 and storage_path is not null";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exclude.Add(reader.GetString(0));
                    }
                }
            }
            files = files.Where(f => !exclude.Any(e => f.Path.Contains(string.Format("/file_{0}/", e))));

            var fileGroups = files.GroupBy(file => file.Module).ToList();
            var groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                var storage = StorageFactory.GetStorage(ConfigPath, TenantId.ToString(), group.Key);
                foreach (var file in group)
                {
                    ActionInvoker.Try(state =>
                    {
                        var f = (BackupFileInfo)state;
                        using (var fileStream = storage.GetReadStream(f.Domain, f.Path))
                        {
                            var tmp = Path.GetTempFileName();
                            try
                            {
                                using (var tmpFile = File.OpenWrite(tmp))
                                {
                                    fileStream.CopyTo(tmpFile);
                                }

                                writer.WriteEntry(KeyHelper.GetFileZipKey(file), tmp);
                            }
                            finally
                            {
                                if (File.Exists(tmp))
                                {
                                    File.Delete(tmp);
                                }
                            }
                        }
                    }, file, 5, error => Logger.Warn("can't backup file ({0}:{1}): {2}", file.Module, file.Path, error));
                }
                SetCurrentStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
                SetStepCompleted();

            var restoreInfoXml = new XElement(
                "storage_restore",
                fileGroups
                    .SelectMany(group => group.Select(file => (object)file.ToXElement()))
                    .ToArray());

            var tmpPath = Path.GetTempFileName();
            using (var tmpFile = File.OpenWrite(tmpPath))
            {
                restoreInfoXml.WriteTo(tmpFile);
            }

            writer.WriteEntry(KeyHelper.GetStorageRestoreInfoZipKey(), tmpPath);
            File.Delete(tmpPath);


            Logger.Debug("end backup storage");
        }
    }
}
