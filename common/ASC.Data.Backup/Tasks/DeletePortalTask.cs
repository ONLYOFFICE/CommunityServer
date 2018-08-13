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


using System.Collections.Generic;
using System.Linq;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Logging;
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
            Logger.Debug("begin delete {0}", TenantId);
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
            Logger.Debug("end delete {0}", TenantId);
        }

        private void DoDeleteModule(DbFactory dbFactory, IModuleSpecifics module)
        {
            Logger.Debug("begin delete data for module ({0})", module.ModuleName);
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
            Logger.Debug("end delete data for module ({0})", module.ModuleName);
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
                                      onFailure: error => Logger.Warn("Can't delete files for domain {0}: \r\n{1}", domain, error));
                }
                storage.DeleteFiles("\\", "*.*", true);
                SetCurrentStepProgress((int)((++modulesProcessed*100)/(double)storageModules.Count));
            }
            Logger.Debug("end delete storage");
        }
    }
}
