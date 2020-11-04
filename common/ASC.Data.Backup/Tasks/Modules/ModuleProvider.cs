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

namespace ASC.Data.Backup.Tasks.Modules
{
    internal static class ModuleProvider
    {
        public static readonly List<IModuleSpecifics> AllModules = new List<IModuleSpecifics>
            {
                new TenantsModuleSpecifics(),
                new AuditModuleSpecifics(),
                new CommunityModuleSpecifics(),
                new CalendarModuleSpecifics(),
                new ProjectsModuleSpecifics(),
                new CrmModuleSpecifics(),
                new FilesModuleSpecifics(),
                new MailModuleSpecifics(),
                new CrmModuleSpecifics2(),
                new FilesModuleSpecifics2(),
                new CrmInvoiceModuleSpecifics(),
                new WebStudioModuleSpecifics(),
                new CoreModuleSpecifics()
            }
            .ToList();

        public static IModuleSpecifics GetByStorageModule(string storageModuleName, string storageDomainName = null)
        {
            switch (storageModuleName)
            {
                case "files":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Files);

                case "projects":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Projects);

                case "crm":
                    return AllModules.FirstOrDefault(m => m.ModuleName == (storageDomainName == "mail_messages" ? ModuleName.Crm2 : ModuleName.Crm));

                case "forum":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Community);
                
                case "mailaggregator":
                    return AllModules.FirstOrDefault(m => m.ModuleName == ModuleName.Mail);

                default:
                    return null;
            }
        }
    }
}
