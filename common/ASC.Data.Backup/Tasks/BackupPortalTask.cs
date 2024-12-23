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
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.Common;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Service;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;
using ASC.Data.Storage.ZipOperators;

using Newtonsoft.Json;

namespace ASC.Data.Backup.Tasks
{
    public class BackupPortalTask : PortalTaskBase
    {
        private const int MaxLength = 250;
        private const int BatchLimit = 5000;
        private readonly BackupMailServerFilesService backupMailServerFilesService;
        public string BackupFilePath { get; private set; }
        public int Limit { get; private set; }
        private bool Dump { get; set; }
        private bool ForMigration { get; set; }

        public BackupPortalTask(ILog logger, int tenantId, string fromConfigPath, string toFilePath, int limit, bool dump)
            : base(logger, tenantId, fromConfigPath)
        {
            if (string.IsNullOrEmpty(toFilePath))
                throw new ArgumentNullException("toFilePath");

            BackupFilePath = toFilePath;
            Limit = limit;
            Dump = dump && CoreContext.Configuration.Standalone;
            ForMigration = !dump && CoreContext.Configuration.Standalone;

            backupMailServerFilesService = new BackupMailServerFilesService(Logger);
        }

        public override void RunJob()
        {
            using (WriteOperator)
            {
                if (ForMigration)
                {
                    var ids = CoreContext.TenantManager.GetTenants().Select(t => t.TenantId);
                    SetStepsCount(ids.Count());
                    foreach (var id in ids)
                    {
                        Backup(id, $"{id}/");
                    }
                }
                else
                {
                    SetStepsCount(1);
                    Backup(TenantId);
                }
            }
        }

        public void Backup(int tenantId, string prefix = "")
        {
            Logger.DebugFormat("begin backup {0}", tenantId);

            if (Dump)
            {
                DoDump(WriteOperator);
            }
            else
            {
                var modulesToProcess = GetModulesToProcess().ToList();
                var count = modulesToProcess.Select(m => m.Tables.Count(t => !IgnoredTables.Contains(t.Name) && t.InsertMethod != InsertMethod.None)).Sum();
                IEnumerable<BackupFileInfo> files = null;
                if (ProcessStorage)
                {
                    files = GetFiles(tenantId);
                    count += files.Count();
                }

                var completedCount = 0;
                var dbFactory = new DbFactory(ConfigPath);
                foreach (var module in modulesToProcess)
                {
                    completedCount = DoBackupModule(WriteOperator, dbFactory, module, completedCount, count, tenantId, prefix);
                }
                if (ProcessStorage)
                {
                    DoBackupStorage(WriteOperator, files, completedCount, count, prefix: prefix);
                }
            }

            Logger.DebugFormat("end backup {0}", tenantId);
        }

        private void DoDump(IDataWriteOperator writer)
        {
            Dictionary<string, List<string>> databases = new Dictionary<string, List<string>>();
            if (!IgnoredModules.Contains(ModuleName.Mail)) 
            {
                using (var dbManager = new DbManager("default", 100000))
                {
                    dbManager.ExecuteList("select id, connection_string from mail_server_server").ForEach(r =>
                    {
                        var dbName = GetDbName((int)r[0], JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(r[1]))["DbConnection"].ToString());
                        using (var dbManager1 = new DbManager(dbName, 100000))
                        {
                            try
                            {
                                var tables = dbManager1.ExecuteList("show tables;").Select(res => Convert.ToString(res[0])).ToList();
                                databases.Add(dbName, tables);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                DbRegistry.UnRegisterDatabase(dbName);
                            }
                        }
                    });
                }
            }
            using (var dbManager = new DbManager("default", 100000))
            {
                var tables = dbManager.ExecuteList("show tables;").Select(res => Convert.ToString(res[0])).ToList();
                databases.Add("default", tables);
            }

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(true.ToString())))
            {
                writer.WriteEntry(KeyHelper.GetDumpKey(), stream);
            }

            IEnumerable<BackupFileInfo> files = null;

            var count = databases.Select(d => d.Value.Count * 4).Sum(); // (schema + data) * (dump + zip)
            var completedCount = count;

            if (ProcessStorage)
            {
                var tenants = CoreContext.TenantManager.GetTenants(false).Select(r => r.TenantId);
                files = GetFilesTenants(tenants);
                Logger.Debug("files:" + files.Count());
                count += files.Count();
            }

            SetStepsCount(1);

            foreach (var db in databases)
            {
                DoDump(writer, db.Key, db.Value);
            }
            var dir = Path.GetDirectoryName(BackupFilePath);
            var subDir = Path.Combine(dir, Path.GetFileNameWithoutExtension(BackupFilePath));
            Logger.DebugFormat("dir remove start {0}", subDir);
            Directory.Delete(subDir, true);
            Logger.DebugFormat("dir remove end {0}", subDir);

            if (ProcessStorage)
            {
                DoBackupStorage(writer, files, completedCount, count, true);
            }
        }

        private void DoDump(IDataWriteOperator writer, string dbName, List<string> tables)
        {
            var excluded = ModuleProvider.AllModules.Where(r => IgnoredModules.Contains(r.ModuleName)).SelectMany(r => r.Tables).Select(r => r.Name).ToList();
            excluded.AddRange(IgnoredTables);
            excluded.Add("res_");

            var dir = Path.GetDirectoryName(BackupFilePath);
            var subDir = Path.Combine(dir, Path.GetFileNameWithoutExtension(BackupFilePath));
            var schemeDir = "";
            var dataDir = "";
            if (dbName == "default")
            {
                schemeDir = Path.Combine(subDir, KeyHelper.GetDatabaseSchema());
                dataDir = Path.Combine(subDir, KeyHelper.GetDatabaseData());
            }
            else
            {
                schemeDir = Path.Combine(subDir, dbName, KeyHelper.GetDatabaseSchema());
                dataDir = Path.Combine(subDir, dbName, KeyHelper.GetDatabaseData());
            }

            if (!Directory.Exists(schemeDir))
            {
                Directory.CreateDirectory(schemeDir);
            }
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            var dict = new Dictionary<string, int>();
            foreach (var table in tables)
            {
                dict.Add(table, SelectCount(table, dbName));
            }
            tables.Sort((pair1, pair2) => dict[pair1].CompareTo(dict[pair2]));

            for (var i = 0; i < tables.Count; i += TasksLimit)
            {
                var tasks = new List<Task>(TasksLimit * 2);
                for (var j = 0; j < TasksLimit && i + j < tables.Count; j++)
                {
                    var t = tables[i + j];
                    tasks.Add(Task.Run(() => DumpTableScheme(t, schemeDir, dbName)));
                    if (!excluded.Any(t.StartsWith))
                    {
                        tasks.Add(Task.Run(() => DumpTableData(t, dataDir, dict[t], dbName, writer)));
                    }
                    else
                    {
                        SetStepCompleted(2);
                    }
                }

                Task.WaitAll(tasks.ToArray());

                ArchiveDir(writer, subDir);
            }
        }

        private IEnumerable<BackupFileInfo> GetFilesTenants(IEnumerable<int> tenantIds)
        {
            foreach (var tenantId in tenantIds)
            {
                var files = GetFiles(tenantId);
                foreach (var file in files)
                {
                    yield return file;
                }
            }
        }
        private IEnumerable<BackupFileInfo> GetFiles(int tenantId)
        {
            var files = GetFilesToProcess(tenantId);
            var exclude = new List<string>();

            using (var db = new DbManager("default"))
            {
                var query = new SqlQuery("backup_backup")
                    .Select("storage_path")
                    .Where("tenant_id", tenantId)
                    .Where("storage_type", 0)
                    .Where(!Exp.Eq("storage_path", null));

                exclude.AddRange(db.ExecuteList(query).Select(r => Convert.ToString(r[0])));
            }

            files = files.Where(f => !exclude.Any(e => f.Path.Replace('\\', '/').Contains(string.Format("/file_{0}/", e))));
            files = files.Where(f => !f.Path.Contains("temp"));
            return files;
        }

        private void DumpTableScheme(string t, string dir, string dbName)
        {
            try
            {
                Logger.DebugFormat("dump table scheme start {0}", t);
                using (var dbManager = new DbManager(dbName, 100000))
                {
                    var createScheme = dbManager.ExecuteList(string.Format("SHOW CREATE TABLE `{0}`", t));
                    var creates = new StringBuilder();
                    creates.AppendFormat("DROP TABLE IF EXISTS `{0}`;", t);
                    creates.AppendLine();
                    creates.Append(createScheme
                            .Select(r => Convert.ToString(r[1]))
                            .FirstOrDefault());
                    creates.Append(";");

                    var path = Path.Combine(dir, t);
                    using (var stream = File.OpenWrite(path))
                    {
                        var bytes = Encoding.UTF8.GetBytes(creates.ToString());
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    SetStepCompleted();
                }

                Logger.DebugFormat("dump table scheme stop {0}", t);
            }
            catch
            {

            }

        }

        private int SelectCount(string t, string dbName)
        {
            try
            {
                using (var dbManager = new DbManager(dbName, 100000))
                {
                    dbManager.ExecuteNonQuery("analyze table " + t);
                    return dbManager.ExecuteScalar<int>(new SqlQuery("information_schema.`TABLES`").Select("table_rows").Where("TABLE_NAME", t).Where("TABLE_SCHEMA", dbManager.Connection.Database));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }

        }

        private void DumpTableData(string t, string dir, int count, string dbName, IDataWriteOperator writer)
        {
            try
            {
                if (count == 0)
                {
                    Logger.DebugFormat("dump table data stop {0}", t);
                    SetStepCompleted(2);
                    return;
                }

                Logger.DebugFormat("dump table data start {0}", t);
                var searchWithPrimary = false;
                string primaryIndex;
                int primaryIndexStep = 0;
                int primaryIndexStart = 0;

                List<string> columns;
                using (var dbManager = new DbManager(dbName, 100000))
                {
                    var columnsData = dbManager.ExecuteList(string.Format("SHOW COLUMNS FROM `{0}`;", t));
                    columns = columnsData
                        .Select(r => "`" + Convert.ToString(r[0]) + "`")
                        .ToList();

                    primaryIndex = dbManager
                        .ExecuteList(
                            new SqlQuery("information_schema.`COLUMNS`")
                            .Select("COLUMN_NAME")
                            .Where("TABLE_SCHEMA", dbManager.Connection.Database)
                            .Where("TABLE_NAME", t)
                            .Where("COLUMN_KEY", "PRI")
                            .Where("DATA_TYPE", "int"))
                        .ConvertAll(r => Convert.ToString(r[0]))
                        .FirstOrDefault();

                    var isLeft = dbManager.ExecuteList(string.Format("SHOW INDEXES FROM {0} WHERE COLUMN_NAME='{1}' AND seq_in_index=1", t, primaryIndex));

                    searchWithPrimary = isLeft.Count == 1;

                    if (searchWithPrimary)
                    {
                        var minMax = dbManager
                            .ExecuteList(new SqlQuery(t).SelectMax(primaryIndex).SelectMin(primaryIndex))
                            .ConvertAll(r => new Tuple<int, int>(Convert.ToInt32(r[0]), Convert.ToInt32(r[1])))
                            .FirstOrDefault();

                        primaryIndexStart = minMax.Item2;
                        primaryIndexStep = (minMax.Item1 - minMax.Item2) / count;

                        if (primaryIndexStep < Limit)
                        {
                            primaryIndexStep = Limit;
                        }
                    }
                }

                var path = Path.Combine(dir, t);
                var offset = 0;

                do
                {
                    List<object[]> result;

                    if (searchWithPrimary)
                    {
                        result = GetDataWithPrimary(t, columns, primaryIndex, primaryIndexStart, primaryIndexStep, dbName);
                        primaryIndexStart += primaryIndexStep;
                    }
                    else
                    {
                        result = GetData(t, columns, offset, dbName);
                    }

                    offset += Limit;

                    var resultCount = result.Count;

                    if (resultCount == 0) break;

                    SaveToFile(path, t, columns, result);

                } while (true);

                SetStepCompleted();
                Logger.DebugFormat("dump table data stop {0}", t);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private List<object[]> GetData(string t, List<string> columns, int offset, string dbName)
        {
            using (var dbManager = new DbManager(dbName, 100000))
            {
                var query = new SqlQuery(t)
                    .Select(columns.ToArray())
                    .SetFirstResult(offset)
                    .SetMaxResults(Limit);
                return dbManager.ExecuteList(query);
            }
        }
        private List<object[]> GetDataWithPrimary(string t, List<string> columns, string primary, int start, int step, string dbName)
        {
            using (var dbManager = new DbManager(dbName, 100000))
            {
                var query = new SqlQuery(t)
                    .Select(columns.ToArray())
                    .Where(Exp.Between(primary, start, start + step));

                return dbManager.ExecuteList(query);
            }
        }

        private string GetDbName(int id, string connectionString)
        {
            connectionString = connectionString + ";convert zero datetime=True";
            var connectionSettings = new ConnectionStringSettings("mailservice-" + id, connectionString, "MySql.Data.MySqlClient");

            if (DbRegistry.IsDatabaseRegistered(connectionSettings.Name))
            {
                DbRegistry.UnRegisterDatabase(connectionSettings.Name);
            }

            DbRegistry.RegisterDatabase(connectionSettings.Name, connectionSettings);
            return connectionSettings.Name;
        }

        private void SaveToFile(string path, string t, IReadOnlyCollection<string> columns, List<object[]> data)
        {
            Logger.DebugFormat("save to file {0}", t);
            List<object[]> portion;
            while ((portion = data.Take(BatchLimit).ToList()).Any())
            {
                using (var sw = new StreamWriter(path, true))
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.QuoteChar = '\'';
                    writer.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    sw.Write("REPLACE INTO `{0}` ({1}) VALUES ", t, string.Join(",", columns));
                    sw.WriteLine();

                    for (var j = 0; j < portion.Count; j++)
                    {
                        var obj = portion[j];
                        sw.Write("(");

                        for (var i = 0; i < obj.Length; i++)
                        {
                            var byteArray = obj[i] as byte[];
                            if (byteArray != null && byteArray.Length != 0)
                            {
                                sw.Write("0x");
                                foreach (var b in byteArray)
                                    sw.Write("{0:x2}", b);
                            }
                            else
                            {
                                var s = obj[i] as string;
                                if (s != null)
                                {
                                    sw.Write("'" + s.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("'", "\\'").Replace("\n", "\\n") + "'");
                                }
                                else
                                {
                                    var ser = new JsonSerializer();
                                    ser.Serialize(writer, obj[i]);
                                }
                            }
                            if (i != obj.Length - 1)
                            {
                                sw.Write(",");
                            }
                        }

                        sw.Write(")");
                        if (j != portion.Count - 1)
                        {
                            sw.Write(",");
                        }
                        else
                        {
                            sw.Write(";");
                        }
                        sw.WriteLine();
                    }
                }
                data = data.Skip(BatchLimit).ToList();
            }
        }

        private void ArchiveDir(IDataWriteOperator writer, string subDir)
        {
            Logger.DebugFormat("archive dir start {0}", subDir);
            foreach (var enumerateFile in Directory.EnumerateFiles(subDir, "*", SearchOption.AllDirectories))
            {
                var f = enumerateFile;
                if (!WorkContext.IsMono && enumerateFile.Length > MaxLength)
                {
                    f = @"\\?\" + f;
                }
                using (var tmpFile = new FileStream(f, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                {
                    writer.WriteEntry(enumerateFile.Substring(subDir.Length), tmpFile);
                }
                SetStepCompleted();
            }
            Logger.DebugFormat("archive dir end {0}", subDir);
        }

        private int DoBackupModule(IDataWriteOperator writer, DbFactory dbFactory, IModuleSpecifics module, int completedCount, int count, int tenantId, string prefix = "")
        {
            Logger.DebugFormat("begin saving data for module {0}", module.ModuleName);
            var tablesToProcess = module.Tables.Where(t => !IgnoredTables.Contains(t.Name) && t.InsertMethod != InsertMethod.None).ToList();
            var tablesProcessed = completedCount;

            using (var connection = dbFactory.OpenConnection())
            {
                foreach (var table in tablesToProcess)
                {
                    Logger.DebugFormat("begin load table {0}", table.Name);
                    using (var data = new DataTable(table.Name))
                    {
                        ActionInvoker.Try(
                            state =>
                            {
                                data.Clear();
                                int counts;
                                var offset = 0;
                                do
                                {
                                    var t = (TableInfo)state;
                                    var dataAdapter = dbFactory.CreateDataAdapter();
                                    dataAdapter.SelectCommand = module.CreateSelectCommand(connection.Fix(), tenantId, t, Limit, offset).WithTimeout(600);
                                    counts = ((DbDataAdapter)dataAdapter).Fill(data);
                                    offset += Limit;
                                } while (counts == Limit);

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

                        Logger.DebugFormat("end load table {0}", table.Name);

                        Logger.DebugFormat("begin saving table {0}", table.Name);

                        using (var file = TempStream.Create())
                        {
                            data.WriteXml(file, XmlWriteMode.WriteSchema);
                            data.Clear();

                            writer.WriteEntry(prefix + KeyHelper.GetTableZipKey(module, data.TableName), file);
                        }

                        Logger.DebugFormat("end saving table {0}", table.Name);
                    }

                    SetCurrentStepProgress((int)((++tablesProcessed * 100) / (double)count));
                }
            }
            Logger.DebugFormat("end saving data for module {0}", module.ModuleName);
            return tablesProcessed;
        }

        private List<string> DoBackupMailTable(IDataWriteOperator writer, int tenantId)
        {
            Logger.DebugFormat("begin saving data for MailTable");

            List<string> result = new List<string>();

            string dbconnection = null;
            var module = new MailTableSpecifics();

            using (var dbManager = new DbManager("default", 100000))
            {
                dbManager.ExecuteList("select connection_string from mail_server_server").ForEach(r =>
                {
                    dbconnection = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(r[0]))["DbConnection"].ToString() + "; convert zero datetime=True";

                    Logger.Debug($"DoBackupMailTable: dbconnection: {dbconnection}.");
                });

                dbManager.ExecuteList($"SELECT name FROM mail_server_domain WHERE tenant={tenantId} AND is_verified=1").ForEach(r =>
                {
                    var findedDomain = Convert.ToString(r[0]);

                    var findedReverseDomain = string.Join(".", findedDomain.Split('.').Reverse().ToArray());

                    module.findValues["domain"].Add($"'{findedDomain}'");

                    module.findValues["reversedomain"].Add($"'{findedReverseDomain}'");

                    Logger.Debug($"DoBackupMailTable: find domain: {findedDomain}, findedReverseDomain: {findedReverseDomain}.");
                });
            }

            if (dbconnection == null)
            {
                Logger.Debug("DoBackupMailTable: mail_server_server have not connection_string.");
                return result;
            }

            if (module.findValues["domain"].Count == 0)
            {
                Logger.Debug($"DoBackupMailTable: TenantId {tenantId} hasn`t any custom domain.");
                return result;
            }

            var dbFactory = new DbFactory(ConfigPath) { ConnectionStringSettings = new ConnectionStringSettings("mailTable", dbconnection, "MySql.Data.MySqlClient") };

            using (var connection = dbFactory.OpenConnection())
            {
                foreach (var table in module.Tables)
                {
                    Logger.DebugFormat("begin load table {0}", table.Name);
                    using (var data = new DataTable(table.Name))
                    {
                        ActionInvoker.Try(
                            state =>
                            {
                                data.Clear();
                                int counts;
                                var offset = 0;
                                do
                                {
                                    var t = (MailTableInfo)state;
                                    var dataAdapter = dbFactory.CreateDataAdapter();
                                    dataAdapter.SelectCommand = module.CreateSelectCommand(connection.Fix(), t, Limit, offset).WithTimeout(600);
                                    Logger.Debug($"DoBackupMailTable SelectCommand is {dataAdapter.SelectCommand.CommandText}");
                                    counts = ((DbDataAdapter)dataAdapter).Fill(data);
                                    offset += Limit;
                                } while (counts == Limit);

                            },
                            table,
                            maxAttempts: 5,
                            onFailure: error => { throw ThrowHelper.CantBackupTable(table.Name, error); },
                            onAttemptFailure: error => Logger.Warn("backup attempt failure: {0}", error));

                        foreach (var col in data.Columns.Cast<DataColumn>().Where(col => col.DataType == typeof(DateTime)))
                        {
                            col.DateTimeMode = DataSetDateTime.Unspecified;
                        }

                        Logger.DebugFormat("end load table {0}", table.Name);

                        Logger.DebugFormat("begin saving table {0}", table.Name);

                        if (data.TableName == "maddr")
                        {
                            foreach (DataRow row in data.Rows)
                            {
                                module.findValues["id"].Add(row["id"]);

                                var bytes = row["email"] as byte[];

                                var sb = new StringBuilder();
                                sb.Append("0x");
                                foreach (var b in bytes)
                                    sb.AppendFormat("{0:x2}", b);

                                module.findValues["email"].Add(sb.ToString());

                                Logger.Debug($"DoBackupMailTable: In maddr finded email: {sb}.");
                            }
                        }

                        if (data.TableName == "mailbox")
                        {
                            foreach (DataRow row in data.Rows)
                            {
                                var storagenode = Convert.ToString(row["storagenode"]);
                                var maildir = Convert.ToString(row["maildir"]);

                                var sb = new StringBuilder();
                                sb.Append(backupMailServerFilesService.pathToMailFilesOnHost);

                                sb.Append('/');

                                sb.AppendFormat(storagenode);

                                sb.Append('/');

                                sb.AppendFormat(maildir);

                                result.Add(sb.ToString());

                                Logger.Debug($"DoBackupMailTable: In mailbox finded path: {sb}.");
                            }
                        }

                        using (var file = TempStream.Create())
                        {
                            data.WriteXml(file, XmlWriteMode.WriteSchema);
                            data.Clear();

                            writer.WriteEntry(KeyHelper.GetMailTableZipKey(data.TableName), file);
                        }

                        Logger.DebugFormat("end saving table {0}", table.Name);
                    }
                }
            }
            Logger.DebugFormat("end saving data for MailTable");

            return result;
        }

        private void DoBackupMailFiles(IDataWriteOperator writer, List<String> paths)
        {
            Logger.DebugFormat("begin saving files for Mail Server");

            int filesCount = 0;

            foreach (string path in paths)
            {
                filesCount += backupMailServerFilesService.SaveDirectory(path, writer.WriteEntry);
            }

            Logger.DebugFormat($"end saving {filesCount} files for Mail Server");
        }

        private void DoBackupStorage(IDataWriteOperator writer, IEnumerable<BackupFileInfo> files, int completedCount, int count, bool dump = false, string prefix = "")
        {
            Logger.Debug("begin backup storage");

            var filesProcessed = completedCount;

            using (var tmpFile = TempStream.Create())
            {
                var bytes = Encoding.UTF8.GetBytes("<storage_restore>");
                tmpFile.Write(bytes, 0, bytes.Length);
                var storages = new Dictionary<string, IDataStore>();
                foreach (var file in files)
                {
                    if (!storages.TryGetValue(file.Module + file.Tenant.ToString(), out var storage))
                    {
                        storage = StorageFactory.GetStorage(ConfigPath, file.Tenant.ToString(), file.Module);
                        storages.Add(file.Module + file.Tenant.ToString(), storage);
                    }
                    var path = file.GetZipKey();
                    if (dump)
                    {
                        path = Path.Combine(prefix, Path.DirectorySeparatorChar + "storage", path);
                    }
                    else
                    {
                        path = Path.Combine(prefix, path).Replace("\\", "/");
                    }

                    var file1 = file;
                    Stream fileStream = null;
                    ActionInvoker.Try(state =>
                    {
                        var f = (BackupFileInfo)state;
                        fileStream = storage.GetReadStream(f.Domain, f.Path);
                    }, file, 5, error => Logger.WarnFormat("can't backup file ({0}:{1}): {2}", file1.Module, file1.Path, error));
                    if (fileStream != null)
                    {
                        writer.WriteEntry(path, fileStream);
                        fileStream.Dispose();
                    }
                    SetCurrentStepProgress((int)(++filesProcessed * 100 / (double)count));

                    var restoreInfoXml = file.ToXElement();
                    restoreInfoXml.WriteTo(tmpFile);
                }

                bytes = Encoding.UTF8.GetBytes("</storage_restore>");
                tmpFile.Write(bytes, 0, bytes.Length);
                writer.WriteEntry(prefix + KeyHelper.GetStorageRestoreInfoZipKey(), tmpFile);

            }
            Logger.Debug("end backup storage");
        }
    }
}
