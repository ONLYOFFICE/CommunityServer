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

using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Tasks
{
    public class DeletePortalTask : PortalTaskBase
    {
        public DeletePortalTask(Tenant tenant, string configPath)
            : base(tenant, configPath)
        {
        }

        public override void Run()
        {
            InvokeInfo("begin delete portal ({0})", Tenant.TenantAlias);
            List<IModuleSpecifics> modulesToProcess = GetModulesToProcess().Reverse().ToList();
            InitProgress(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);
            var dbFactory = new DbFactory(ConfigPath);
            foreach (var module in modulesToProcess)
            {
                DoDeleteModule(dbFactory, module);
            }
            if (ProcessStorage)
            {
                DoDeleteStorage();
            }
            InvokeInfo("end delete portal ({0})", Tenant.TenantAlias);
        }

        private void DoDeleteModule(DbFactory dbFactory, IModuleSpecifics module)
        {
            InvokeInfo("begin delete data for module ({0})", module.ModuleName);
            int tablesCount = module.Tables.Count();
            int tablesProcessed = 0;
            using (var connection = dbFactory.OpenConnection(module.ConnectionStringName))
            {
                foreach (var table in module.GetTablesOrdered().Reverse().Where(t => !IgnoredTables.Contains(t.Name)))
                {
                    ActionInvoker.Try(state =>
                        {
                            var t = (TableInfo)state;
                            module.CreateDeleteCommand(connection.Fix(), Tenant.TenantId, t).WithTimeout(120).ExecuteNonQuery();
                        }, table, 5, onFailure: error => { throw ThrowHelper.CantDeleteTable(table.Name, error); });
                    SetStepProgress((int)((++tablesProcessed*100)/(double)tablesCount));
                }
            }
            InvokeInfo("end delete data for module ({0})", module.ModuleName);
        }

        private void DoDeleteStorage()
        {
            InvokeInfo("begin delete storage");
            List<string> storageModules = StorageFactory.GetModuleList(ConfigPath).Where(IsStorageModuleAllowed).ToList();
            int modulesProcessed = 0;
            foreach (string module in storageModules)
            {
                IDataStore storage = StorageFactory.GetStorage(ConfigPath, Tenant.TenantId.ToString(), module, null, null);
                List<string> domains = StorageFactory.GetDomainList(ConfigPath, module).ToList();
                foreach (var domain in domains)
                {
                    ActionInvoker.Try(state => storage.DeleteFiles((string)state, "\\", "*.*", true), domain, 5,
                                      onFailure: error => InvokeWarning("Can't delete files for domain {0}: \r\n{1}", domain, error));
                }
                storage.DeleteFiles("\\", "*.*", true);
                SetStepProgress((int)((++modulesProcessed*100)/(double)storageModules.Count));
            }
            InvokeInfo("end delete storage");
        }
    }
}
