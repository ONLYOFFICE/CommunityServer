/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System.Collections.Generic;
using System.Linq;
using ASC.Common.Logging;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Tasks
{
    public class DeletePortalTask : PortalTaskBase
    {
        public DeletePortalTask(ILog logger, int tenantId, string configPath)
            : base(logger, tenantId, configPath)
        {
        }

        public override void RunJob()
        {
            Logger.DebugFormat("begin delete {0}", TenantId);
            List<IModuleSpecifics> modulesToProcess = GetModulesToProcess().Reverse().ToList();
            SetStepsCount(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);
            var dbFactory = new DbFactory(ConfigPath);
            foreach (var module in modulesToProcess)
            {
                DoDeleteModule(dbFactory, module);
            }
            if (ProcessStorage)
            {
                DoDeleteStorage();
            }
            Logger.DebugFormat("end delete {0}", TenantId);
        }

        private void DoDeleteModule(DbFactory dbFactory, IModuleSpecifics module)
        {
            Logger.DebugFormat("begin delete data for module ({0})", module.ModuleName);
            int tablesCount = module.Tables.Count();
            int tablesProcessed = 0;
            using (var connection = dbFactory.OpenConnection())
            {
                foreach (var table in module.GetTablesOrdered().Reverse().Where(t => !IgnoredTables.Contains(t.Name)))
                {
                    ActionInvoker.Try(state =>
                        {
                            var t = (TableInfo)state;
                            module.CreateDeleteCommand(connection.Fix(), TenantId, t).WithTimeout(120).ExecuteNonQuery();
                        }, table, 5, onFailure: error => { throw ThrowHelper.CantDeleteTable(table.Name, error); });
                    SetCurrentStepProgress((int)((++tablesProcessed*100)/(double)tablesCount));
                }
            }
            Logger.DebugFormat("end delete data for module ({0})", module.ModuleName);
        }

        private void DoDeleteStorage()
        {
            Logger.Debug("begin delete storage");
            List<string> storageModules = StorageFactory.GetModuleList(ConfigPath).Where(IsStorageModuleAllowed).ToList();
            int modulesProcessed = 0;
            foreach (string module in storageModules)
            {
                IDataStore storage = StorageFactory.GetStorage(ConfigPath, TenantId.ToString(), module);
                List<string> domains = StorageFactory.GetDomainList(ConfigPath, module).ToList();
                foreach (var domain in domains)
                {
                    ActionInvoker.Try(state => storage.DeleteFiles((string)state, "\\", "*.*", true), domain, 5,
                                      onFailure: error => Logger.WarnFormat("Can't delete files for domain {0}: \r\n{1}", domain, error));
                }
                storage.DeleteFiles("\\", "*.*", true);
                SetCurrentStepProgress((int)((++modulesProcessed*100)/(double)storageModules.Count));
            }
            Logger.Debug("end delete storage");
        }
    }
}
