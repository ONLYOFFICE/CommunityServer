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
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Core;
using ASC.Core.Users;
using ASC.Migration;
using ASC.Migration.Core;
using ASC.Migration.Core.Models.Api;
using ASC.Web.Studio.Core.Notify;


namespace ASC.Api.Migration
{
    /// <summary>
    /// Migration API.
    /// </summary>
    public class MigrationApi : Interfaces.IApiEntryPoint
    {
        /// <summary>
        /// Api name entry
        /// </summary>
        public string Name => "migration";

        private readonly ApiContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public MigrationApi(ApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the temporary folder where all the migration files are stored.
        /// </summary>
        /// <short>
        /// Get migration temporary folder
        /// </short>
        /// <returns>Path to the migration temporary folder</returns>
        /// <path>api/2.0/migration/tmp</path>
        /// <httpMethod>GET</httpMethod>
        [Read("tmp")]
        public string GetTmpFolder()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();

            var tempFolder = Path.Combine(TempPath.GetTempPath(), "migration", DateTime.Now.ToString("dd.MM.yyyy_HH_mm"));

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            return tempFolder;
        }

        /// <summary>
        /// Returns all the available migrations.
        /// </summary>
        /// <short>
        /// Get migrations
        /// </short>
        /// <returns>List of migrations</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/migration/list</path>
        /// <httpMethod>GET</httpMethod>
        [Read("list")]
        public string[] List()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            return MigrationCore.GetAvailableMigrations();
        }

        /// <summary>
        /// Returns the information about the migrators with the names specified in the request.
        /// </summary>
        /// <short>
        /// Get migrator information
        /// </short>
        /// <param type="System.String[], System" name="migratorsName">List of migrator names</param>
        /// <returns>List of migrator information</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/migration/migratorsInfo</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        [Create("migratorsInfo")]
        public List<MigratorInfo> MigratorsInfo(string[] migratorsName)
        {
            var result = new List<MigratorInfo>();
            foreach (var migratorName in migratorsName)
            {
                var migratorMeta = MigrationCore.GetMigrator(migratorName);
                if (migratorMeta != null)
                {
                    result.Add(new MigratorInfo()
                    {
                        MigratorName = migratorMeta.MigratorInfo.Name,
                        NumberOfSteps = migratorMeta.MigratorInfo.NumberOfSteps,
                        ArchivesIsMultiple = migratorMeta.MigratorInfo.ArchivesIsMultiple
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Uploads a backup of a migrator specified in the request and initializes the import.
        /// </summary>
        /// <short>
        /// Initialize migration
        /// </short>
        /// <param type="System.String, System" name="migratorName">Migrator name</param>
        /// <param type="System.String, System" name="path">Path to the backup file</param>
        /// <path>api/2.0/migration/init/{migratorName}</path>
        /// <httpMethod>POST</httpMethod>
        [Create("init/{migratorName}")]
        public void UploadAndInit(string migratorName, string path)
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            if (GetOngoingMigration() != null)
            {
                throw new Exception("Migration is already in progress");
            }
            var cts = new CancellationTokenSource();
            var migratorMeta = MigrationCore.GetMigrator(migratorName);
            if (migratorMeta == null)
            {
                throw new ItemNotFoundException("No such migration provider");
            }
            var migrator = (IMigration)Activator.CreateInstance(migratorMeta.MigratorType);

            var ongoingMigration = new OngoingMigration { Migration = migrator, CancelTokenSource = cts };
            StoreOngoingMigration(ongoingMigration);
            ongoingMigration.ParseTask = QueueWorker.StartParsing(migrator, path, cts.Token);
        }

        /// <summary>
        /// Returns a status of the running migration process.
        /// </summary>
        /// <short>
        /// Get migration status
        /// </short>
        /// <returns>Object with the information about migration status</returns>
        /// <path>api/2.0/migration/status</path>
        /// <httpMethod>GET</httpMethod>
        [Read("status")]
        public object Status()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            var ongoingMigration = GetOngoingMigration();
            if (ongoingMigration == null) return null;

            var migratorName = ongoingMigration.Migration.GetType().GetCustomAttribute<ApiMigratorAttribute>().Name;

            if (ongoingMigration.IsCancel == true) return migratorName;

            var result = new MigrationStatus()
            {
                ParseResult = ongoingMigration.ParseTask != null ? ongoingMigration.ParseTask.migrationInfo : null,
                ParsingEnded = ongoingMigration.ParseTask.IsCompleted, 
                MigrationEnded = ongoingMigration.MigrationEnded,
                Progress = ongoingMigration.Migration.GetProgress(),
                ProgressStatus = ongoingMigration.Migration.GetProgressStatus(),
                MigratorName = migratorName
            };

            return result;
        }

        /// <summary>
        /// Cancels the running migration process.
        /// </summary>
        /// <short>
        /// Cancel migration
        /// </short>
        /// <path>api/2.0/migration/cancel</path>
        /// <httpMethod>POST</httpMethod>
        [Create("cancel")]
        public void Cancel()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            var ongoingMigration = GetOngoingMigration();
            if (ongoingMigration == null)
            {
                throw new Exception("No migration is in progress");
            }
            ongoingMigration.CancelTokenSource.Cancel();
            QueueWorker.Terminate();
            ongoingMigration.IsCancel = true;
        }

        /// <summary>
        /// Starts the migration process specifying the migration information to be imported (users, modules, groups, etc.).
        /// </summary>
        /// <short>
        /// Start migration
        /// </short>
        /// <param type="ASC.Migration.Core.Models.Api.MigrationApiInfo, ASC.Migration.Core.Models.Api" name="info">Migration information</param>
        /// <path>api/2.0/migration/migrate</path>
        /// <httpMethod>POST</httpMethod>
        [Create("migrate")]
        public void Migrate(MigrationApiInfo info)
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            var ongoingMigration = GetOngoingMigration();
            if (ongoingMigration == null)
            {
                throw new Exception("No migration is in progress");
            }
            else if (!ongoingMigration.ParseTask.IsCompleted)
            {
                throw new Exception("Parsing is still in progress");
            }

            ongoingMigration.MigrationTask = QueueWorker.StartMigration(ongoingMigration.Migration, info);
        }

        /// <summary>
        /// Returns the logs of the migration process.
        /// </summary>
        /// <short>
        /// Get migration logs
        /// </short>
        /// <returns>Migration logs</returns>
        /// <path>api/2.0/migration/logs</path>
        /// <httpMethod>GET</httpMethod>
        [Read("logs")]
        public MigrationLogApiContentResponce Logs()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            var ongoingMigration = GetOngoingMigration();
            if (ongoingMigration == null)
            {
                throw new Exception("No migration is in progress");
            }
            return new MigrationLogApiContentResponce(ongoingMigration.Migration.GetLogs(), "migration.log");
        }

        /// <summary>
        /// Completes the migration process.
        /// </summary>
        /// <short>
        /// Complete migration
        /// </short>
        /// <param type="System.Boolean, System" name="isSendWelcomeEmail">Specifies whether to send a welcome letter to the imported users or not</param>
        /// <path>api/2.0/migration/finish</path>
        /// <httpMethod>POST</httpMethod>
        [Create("finish")]
        public void Finish(bool isSendWelcomeEmail)
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            if (isSendWelcomeEmail)
            {
                var ongoingMigration = GetOngoingMigration();
                if (ongoingMigration == null)
                {
                    throw new Exception("No migration is in progress");
                }
                var guidUsers = ongoingMigration.Migration.GetGuidImportedUsers();
                foreach (var gu in guidUsers)
                {
                    var u = CoreContext.UserManager.GetUsers(gu);
                    StudioNotifyService.Instance.UserInfoActivation(u);
                }
            }
            ClearCache();
        }

        private const string MigrationCacheKey = "ASC.Migration.Ongoing";
        // ToDo: Use ASCCache
        private void StoreOngoingMigration(OngoingMigration migration)
        {
            MemoryCache.Default.Set(MigrationCacheKey, migration, new CacheItemPolicy() { RemovedCallback = ClearMigration, SlidingExpiration = TimeSpan.FromDays(1) });
        }

        private OngoingMigration GetOngoingMigration()
        {
            return (OngoingMigration)MemoryCache.Default.Get(MigrationCacheKey);
        }

        private void ClearCache()
        {
            MemoryCache.Default.Remove(MigrationCacheKey);
        }

        private void ClearMigration(CacheEntryRemovedArguments arguments)
        {
            if (typeof(OngoingMigration).IsAssignableFrom(arguments.CacheItem.Value.GetType()))
            {
                var ongoingMigration = (OngoingMigration)arguments.CacheItem.Value;
                ongoingMigration.Migration.Dispose();
            }
        }
    }
}
