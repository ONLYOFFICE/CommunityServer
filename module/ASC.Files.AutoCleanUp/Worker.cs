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
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Configuration;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;

namespace ASC.Files.AutoCleanUp
{
    public class Worker
    {
        private readonly ILog _logger;
        private readonly CancellationTokenSource _cancellationToken;
        private bool _isStarted;
        private Timer _timer;
        private int _maxDegreeOfParallelism;
        private TimeSpan _period;

        public Worker(ConfigSection configSection, ILog log, CancellationTokenSource cancellationTokenSource)
        {
            _logger = log;
            _cancellationToken = cancellationTokenSource;
            _period = configSection.Period;
            _maxDegreeOfParallelism = configSection.MaxDegreeOfParallelism;
        }


        public void Start()
        {
            if (!_isStarted)
            {
                _logger.Info("Service: Starting");
                _timer = new Timer(DeleteExpiredFilesInTrash, null, TimeSpan.Zero, _period);
                _logger.Info("Service: Started");
                _isStarted = true;
            }
        }

        public void Stop()
        {
            if (_isStarted)
            {
                _logger.Info("Service: Stopping");
                if (_timer != null)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();
                    _timer = null;
                }

                if (_cancellationToken != null)
                {
                    _cancellationToken.Dispose();
                }

                _isStarted = false;
                _logger.Info("Service: Stopped");
            }
        }


        public void DeleteExpiredFilesInTrash(object _)
        {
            if (_cancellationToken != null && _cancellationToken.IsCancellationRequested)
            {
                Stop();
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.Info("Start procedure");

            var activeTenantsUsers = GetTenantsUsers();

            if (!activeTenantsUsers.Any())
            {
                _logger.InfoFormat("Waiting for data. Sleep {0}.", _period);
                _timer.Change(_period, TimeSpan.FromMilliseconds(-1));
                return;
            }

            _logger.InfoFormat("Found {0} active users", activeTenantsUsers.Count);

            Parallel.ForEach(activeTenantsUsers, new ParallelOptions {
               MaxDegreeOfParallelism = _maxDegreeOfParallelism,
               CancellationToken = _cancellationToken.Token
            }, DeleteFilesAndFolders);

            _logger.Info("Finish procedure");
            _timer.Change(_period, _period);
        }

        private void DeleteFilesAndFolders(TenantUserSettings tenantUser)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(tenantUser.TenantId);
                var userAccount = CoreContext.Authentication.GetAccountByID(tenantUser.UserId);

                if (userAccount == Constants.Guest) return;

                SecurityContext.CurrentAccount = userAccount;

                using (var fileDao = Global.DaoFactory.GetFileDao())
                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    var now = DateTime.UtcNow;

                    var itemList = new ItemList<string>();
                    var trashId = folderDao.GetFolderIDTrash(false, tenantUser.UserId);
                    
                    itemList.AddRange(folderDao.GetFolders(trashId)
                        .Where(x => FileDateTime.GetModifiedOnWithAutoCleanUp(x.ModifiedOn, tenantUser.Setting, true) < now)
                        .Select(f => "folder_" + f.ID));
                    itemList.AddRange(fileDao.GetFiles(trashId, null, default(FilterType), false, Guid.Empty, string.Empty, false, null)
                        .Where(x => FileDateTime.GetModifiedOnWithAutoCleanUp(x.ModifiedOn, tenantUser.Setting, true) < now)
                        .Select(y => "file_" + y.ID));

                    if (!itemList.Any())
                    {
                        _logger.InfoFormat("No data to delete for tenant {0}, user {1}", tenantUser.TenantId, SecurityContext.CurrentAccount.ID);

                        return;
                    }

                    _logger.InfoFormat("Start clean up tenant {0}, folder {1}", tenantUser.TenantId, trashId);

                    var statuses =  Global.FileStorageService.DeleteItems("delete", itemList, true, false, true);
                    
                    _logger.InfoFormat("Waiting for tenant {0}, folder {1}...", tenantUser.TenantId, trashId);

                    do
                    {
                        Thread.Sleep(1000);

                        statuses = Global.FileStorageService.GetTasksStatuses();

                    } while (statuses.TrueForAll(r => !r.Finished));

                    _logger.InfoFormat("Finish clean up tenant {0}, folder {1}", tenantUser.TenantId, trashId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                SecurityContext.Logout();
            }
        }

        private List<TenantUserSettings> GetTenantsUsers()
        {
            var query = new SqlQuery("tenants_tenants t")
                .Select("t.id")
                .Select("wss.userid")
                .Select("JSON_EXTRACT(`Data`, '$.AutomaticallyCleanUp.Gap')")
                .Select("JSON_EXTRACT(`Data`, '$.AutomaticallyCleanUp.IsAutoCleanUp') settings")
                .InnerJoin("webstudio_settings wss", Exp.EqColumns("wss.TenantID", "t.id"))
                .Where("wss.id", new FilesSettings().ID)
                .Where("t.status", (int)TenantStatus.Active)
                .Having(Exp.Sql("settings = true"));

            using (var dbManager = new DbManager("default", 180000))
            {
                return dbManager.ExecuteList(query).Select(r => new TenantUserSettings()
                {
                    TenantId = Convert.ToInt32(r[0]),
                    UserId = new Guid(Convert.ToString(r[1])),
                    Setting = (DateToAutoCleanUp)Convert.ToInt32(r[2])
                }).ToList();
            }
        }
    }
}