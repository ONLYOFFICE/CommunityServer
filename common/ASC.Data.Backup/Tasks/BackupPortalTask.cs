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
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ASC.Core.Tenants;
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

        public BackupPortalTask(Tenant tenant, string fromConfigPath, string toFilePath)
            : base(tenant, fromConfigPath)
        {
            if (string.IsNullOrEmpty(toFilePath))
                throw new ArgumentNullException("toFilePath");

            BackupFilePath = toFilePath;
        }

        public override void Run()
        {
            InvokeInfo("begin backup portal ({0})", Tenant.TenantAlias);
            List<IModuleSpecifics> modulesToProcess = GetModulesToProcess().ToList();
            InitProgress(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);
            using (var writer = new ZipWriteOperator(BackupFilePath))
            {
                var dbFactory = new DbFactory(ConfigPath);
                foreach (var module in modulesToProcess)
                {
                    DoBackupModule(writer, dbFactory, module);
                }
                if (ProcessStorage)
                {
                    DoBackupStorage(writer);
                }
            }
            InvokeInfo("end backup portal ({0})", Tenant.TenantAlias);
        }

        private void DoBackupModule(IDataWriteOperator writer, DbFactory dbFactory, IModuleSpecifics module)
        {
            InvokeInfo("begin saving data for module {0}", module.ModuleName);
            int tablesCount = module.Tables.Count();
            int tablesProcessed = 0;
            using (var connection = dbFactory.OpenConnection(module.ConnectionStringName))
            {
                foreach (var table in module.Tables)
                {
                    InvokeInfo("begin saving table {0}", table.Name);
                    using (var data = new DataTable(table.Name))
                    {
                        ActionInvoker.Try(
                            state =>
                                {
                                    data.Clear();
                                    var t = (TableInfo)state;
                                    var dataAdapter = dbFactory.CreateDataAdapter(module.ConnectionStringName);
                                    dataAdapter.SelectCommand = module.CreateSelectCommand(connection.Fix(), Tenant.TenantId, t).WithTimeout(600);
                                    ((DbDataAdapter)dataAdapter).Fill(data);

                                },
                            table,
                            maxAttempts: 5,
                            onFailure: error => { throw ThrowHelper.CantBackupTable(table.Name, error); },
                            onAttemptFailure: error => InvokeWarning("backup attempt failure: {0}", error));

                        foreach (var col in data.Columns.Cast<DataColumn>().Where(col => col.DataType == typeof(DateTime)))
                        {
                            col.DateTimeMode = DataSetDateTime.Unspecified;
                        }

                        var stream = writer.BeginWriteEntry(KeyHelper.GetTableZipKey(module, data.TableName));
                        data.WriteXml(stream, XmlWriteMode.WriteSchema);
                        writer.EndWriteEntry();
                        data.Clear();
                    }

                    SetStepProgress((int)((++tablesProcessed*100)/(double)tablesCount));
                }
            }
            InvokeInfo("end saving data for module {0}", module.ModuleName);
        }

        private void DoBackupStorage(IDataWriteOperator writer)
        {
            InvokeInfo("begin backup storage", Tenant.TenantAlias);
            var fileGroups = GetFilesToProcess().GroupBy(file => file.Module).ToList();
            int groupsProcessed = 0;
            foreach (var group in fileGroups)
            {
                IDataStore storage = StorageFactory.GetStorage(ConfigPath, Tenant.TenantId.ToString(), group.Key, null, null);
                foreach (BackupFileInfo file in group)
                {
                    Stream stream = writer.BeginWriteEntry(KeyHelper.GetFileZipKey(file));

                    int offset = 0;
                    ActionInvoker.Try(state =>
                        {
                            var f = (BackupFileInfo)state;
                            using (var fileStream = storage.GetReadStream(f.Domain, f.Path, offset))
                            {
                                var buffer = new byte[2048];
                                int readed;
                                while ((readed = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    stream.Write(buffer, 0, readed);
                                    offset += readed;
                                }
                            }
                        }, file, 5, error => InvokeWarning("can't backup file ({0}:{1}): {2}", file.Module, file.Path, error));

                    writer.EndWriteEntry();
                }
                SetStepProgress((int)(++groupsProcessed*100/(double)fileGroups.Count));
            }

            if (fileGroups.Count == 0)
                SetStepCompleted();

            var restoreInfoXml = new XElement(
                "storage_restore",
                fileGroups
                    .SelectMany(group => group.Select(file => (object)file.ToXElement()))
                    .ToArray());

            Stream restoreInfoStream = writer.BeginWriteEntry(KeyHelper.GetStorageRestoreInfoZipKey());
            restoreInfoXml.WriteTo(restoreInfoStream);
            writer.EndWriteEntry();

            InvokeInfo("end backup storage", Tenant.TenantAlias);
        }
    }
}
