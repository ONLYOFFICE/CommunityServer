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
using System.Linq;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Common.Contracts;
using ASC.Core.Tenants;
using ASC.Data.Backup.Tasks;

using Newtonsoft.Json;

namespace ASC.Data.Backup.Storage
{
    internal class BackupRepository : IBackupRepository
    {
        private readonly string connectionStringName;

        public BackupRepository(string connectionStringName = null)
        {
            this.connectionStringName = connectionStringName ?? "default";
        }

        public void SaveBackupRecord(BackupRecord backupRecord)
        {
            var insert = new SqlInsert("backup_backup")
                .ReplaceExists(true)
                .InColumnValue("id", backupRecord.Id)
                .InColumnValue("tenant_id", backupRecord.TenantId)
                .InColumnValue("is_scheduled", backupRecord.IsScheduled)
                .InColumnValue("name", backupRecord.FileName)
                .InColumnValue("storage_type", (int)backupRecord.StorageType)
                .InColumnValue("storage_base_path", backupRecord.StorageBasePath)
                .InColumnValue("storage_path", backupRecord.StoragePath)
                .InColumnValue("created_on", backupRecord.CreatedOn)
                .InColumnValue("expires_on", backupRecord.ExpiresOn)
                .InColumnValue("storage_params", JsonConvert.SerializeObject(backupRecord.StorageParams))
                .InColumnValue("hash", backupRecord.Hash)
                .InColumnValue("removed", backupRecord.Removed);

            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(insert);
            }
        }

        public BackupRecord GetBackupRecord(Guid id)
        {
            var select = new SqlQuery("backup_backup")
                .Select("id", "tenant_id", "is_scheduled", "name", "storage_type", "storage_base_path", "storage_path", "created_on", "expires_on", "storage_params","hash")
                .Where("id", id);

            using (var db = GetDbManager())
            {
                return db.ExecuteList(select).Select(ToBackupRecord).SingleOrDefault();
            }
        }

        public BackupRecord GetBackupRecord(string hash, int tenant)
        {
            var select = new SqlQuery("backup_backup")
                .Select("id", "tenant_id", "is_scheduled", "name", "storage_type", "storage_base_path", "storage_path", "created_on", "expires_on", "storage_params", "hash")
                .Where("hash", hash)
                .Where("tenant_id", tenant);

            using (var db = GetDbManager())
            {
                return db.ExecuteList(select).Select(ToBackupRecord).SingleOrDefault();
            }
        }

        public List<BackupRecord> GetExpiredBackupRecords()
        {
            var select = new SqlQuery("backup_backup")
                .Select("id", "tenant_id", "is_scheduled", "name", "storage_type", "storage_base_path", "storage_path", "created_on", "expires_on", "storage_params", "hash")
                .Where(!Exp.Eq("expires_on", DateTime.MinValue) & Exp.Le("expires_on", DateTime.UtcNow))
                .Where(Exp.Eq("removed", false));

            using (var db = GetDbManager())
            {
                return db.ExecuteList(select).Select(ToBackupRecord).ToList();
            }
        }

        public List<BackupRecord> GetScheduledBackupRecords()
        {
            var select = new SqlQuery("backup_backup")
                .Select("id", "tenant_id", "is_scheduled", "name", "storage_type", "storage_base_path", "storage_path", "created_on", "expires_on", "storage_params", "hash")
                .Where(Exp.Eq("is_scheduled", true))
                .Where(Exp.Eq("removed", false));

            using (var db = GetDbManager())
            {
                return db.ExecuteList(select).Select(ToBackupRecord).ToList();
            }
        }

        public List<BackupRecord> GetBackupRecordsByTenantId(int tenantId)
        {
            return GetBackupRecordsByTenantIdInternal(tenantId, false);
        }

        private List<BackupRecord> GetBackupRecordsByTenantIdInternal(int tenantId, bool? visible = null)
        {
            var select = new SqlQuery("backup_backup")
                .Select("id", "tenant_id", "is_scheduled", "name", "storage_type", "storage_base_path", "storage_path", "created_on", "expires_on", "storage_params", "hash")
                .Where("tenant_id", tenantId);

            if (visible.HasValue)
            {
                select = select.Where(Exp.Eq("removed", visible.Value));
            }

            using (var db = GetDbManager())
            {
                return db.ExecuteList(select).Select(ToBackupRecord).ToList();
            }
        }

        public void MigrationBackupRecords(int tenantId, int newTenantId, string configPath)
        {
            var backupsRecords = GetBackupRecordsByTenantIdInternal(tenantId);

            if (backupsRecords.Any()) 
            {
                var dbFactory = new DbFactory(configPath);
                using (var db = dbFactory.OpenConnection())
                {
                    using (var command = db.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO backup_backup (id, tenant_id, is_scheduled, name, storage_type, storage_base_path, storage_path, created_on, expires_on, storage_params, hash, removed) VALUES ";
                        foreach (var backupRecord in backupsRecords)
                        {
                            command.CommandText += $"('{Guid.NewGuid()}', {newTenantId}, {backupRecord.IsScheduled}, '{backupRecord.FileName}', {(int)backupRecord.StorageType}, '{backupRecord.StorageBasePath}', '{backupRecord.StoragePath}', '{backupRecord.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss.fff")}', '{backupRecord.ExpiresOn.ToString("yyyy-MM-dd HH:mm:ss.fff")}', '{JsonConvert.SerializeObject(backupRecord.StorageParams)}', '{backupRecord.Hash}', {true}),";
                        }
                        command.CommandText = command.CommandText.Trim(',');
                        command.CommandText += ";";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteBackupRecord(Guid id)
        {
            var delete = new SqlUpdate("backup_backup")
                .Set("removed", true)
                .Where("id", id);

            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(delete);
            }
        }

        public void SaveBackupSchedule(Schedule schedule)
        {
            var query = new SqlInsert("backup_schedule")
                .ReplaceExists(true)
                .InColumnValue("tenant_id", schedule.TenantId)
                .InColumnValue("backup_mail", schedule.BackupMail)
                .InColumnValue("cron", schedule.Cron)
                .InColumnValue("backups_stored", schedule.NumberOfBackupsStored)
                .InColumnValue("storage_type", (int)schedule.StorageType)
                .InColumnValue("storage_base_path", schedule.StorageBasePath)
                .InColumnValue("last_backup_time", schedule.LastBackupTime)
                .InColumnValue("storage_params", JsonConvert.SerializeObject(schedule.StorageParams));

            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(query);
            }
        }

        public void DeleteBackupSchedule(int tenantId)
        {
            var query = new SqlDelete("backup_schedule")
                .Where("tenant_id", tenantId);

            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(query);
            }
        }

        public List<Schedule> GetBackupSchedules()
        {
            var query = new SqlQuery("backup_schedule s")
                .Select("s.tenant_id", "s.backup_mail", "s.cron", "s.backups_stored", "s.storage_type", "s.storage_base_path", "s.last_backup_time", "s.storage_params")
                .InnerJoin("tenants_tenants t", Exp.EqColumns("t.id", "s.tenant_id"))
                .Where("t.status", (int)TenantStatus.Active);

            using (var db = GetDbManager())
            {
                return db.ExecuteList(query).Select(ToSchedule).ToList();
            }
        }

        public Schedule GetBackupSchedule(int tenantId)
        {
            var query = new SqlQuery("backup_schedule")
                .Select("tenant_id", "backup_mail", "cron", "backups_stored", "storage_type", "storage_base_path", "last_backup_time", "storage_params")
                .Where("tenant_id", tenantId);

            using (var db = GetDbManager())
            {
                return db.ExecuteList(query).Select(ToSchedule).SingleOrDefault();
            }
        }

        private static BackupRecord ToBackupRecord(object[] row)
        {
            return new BackupRecord
            {
                Id = new Guid(Convert.ToString(row[0])),
                TenantId = Convert.ToInt32(row[1]),
                IsScheduled = Convert.ToBoolean(row[2]),
                FileName = Convert.ToString(row[3]),
                StorageType = (BackupStorageType)Convert.ToInt32(row[4]),
                StorageBasePath = Convert.ToString(row[5]),
                StoragePath = Convert.ToString(row[6]),
                CreatedOn = Convert.ToDateTime(row[7]),
                ExpiresOn = Convert.ToDateTime(row[8]),
                StorageParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(row[9])),
                Hash = Convert.ToString(row[10])
            };
        }

        private static Schedule ToSchedule(object[] row)
        {
            return new Schedule(Convert.ToInt32(row[0]))
            {
                BackupMail = Convert.ToBoolean(row[1]),
                Cron = Convert.ToString(row[2]),
                NumberOfBackupsStored = Convert.ToInt32(row[3]),
                StorageType = (BackupStorageType)Convert.ToInt32(row[4]),
                StorageBasePath = Convert.ToString(row[5]),
                LastBackupTime = Convert.ToDateTime(row[6]),
                StorageParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(row[7]))
            };
        }

        private IDbManager GetDbManager()
        {
            return new DbManager(connectionStringName);
        }
    }
}
