
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Core;
using ASC.Core.Users;
using ASC.Migration.Core;
using ASC.Migration.Core.Models.Api;
using ASC.Web.Studio.Core.Notify;


namespace ASC.Api.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class MigrationApi : Interfaces.IApiEntryPoint
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name => "migration";

        private readonly ApiContext _context;

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        public MigrationApi(ApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Read("backuptmp")]
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
        /// 
        /// </summary>
        /// <returns></returns>
        [Read("list")]
        public string[] List()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            return MigrationCore.GetAvailableMigrations();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migratorName"></param>
        /// <param name="path"></param>
        [Create("init/{migratorName}")]
        public void UploadAndInit(string migratorName, string path)
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            if (GetOngoingMigration() != null)
            {
                throw new Exception("Migration is already in progress");
            }

            var migratorMeta = MigrationCore.GetMigrator(migratorName);
            if (migratorMeta == null)
            {
                throw new ItemNotFoundException("No such migration provider");
            }
            var cts = new CancellationTokenSource();
            var migrator = (IMigration)Activator.CreateInstance(migratorMeta.MigratorType);
            try
            {
                migrator.Init(path, cts.Token);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while initializing {migratorMeta.MigratorInfo.Name} migrator", ex);
            }

            var ongoingMigration = new OngoingMigration { Migration = migrator, CancelTokenSource = cts };
            StoreOngoingMigration(ongoingMigration);

            ongoingMigration.ParseTask = Task.Run(migrator.Parse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Read("status")]
        public object Status()
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            var ongoingMigration = GetOngoingMigration();
            if (ongoingMigration == null)
            {
                return null;
            }

            if(ongoingMigration.CancelTokenSource.IsCancellationRequested == true)
            {
                var migratorName = ongoingMigration.Migration.GetType().GetCustomAttribute<ApiMigratorAttribute>().Name;
                return migratorName;
            }

            var result = new MigrationStatus()
            {
                ParseResult = ongoingMigration.ParseTask.IsCompleted ? ongoingMigration.ParseTask.Result : null,
                MigrationEnded = ongoingMigration.MigrationEnded,
                Progress = ongoingMigration.Migration.GetProgress(),
                ProgressStatus = ongoingMigration.Migration.GetProgressStatus()
            };

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
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

            ongoingMigration.MigrationTask = ongoingMigration.Migration.Migrate(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="isSendWelcomeEmail"></param>
        [Create("finish")]
        public void Finish(bool isSendWelcomeEmail)
        {
            if (!CoreContext.Configuration.Standalone || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new System.Security.SecurityException();
            if(isSendWelcomeEmail)
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
