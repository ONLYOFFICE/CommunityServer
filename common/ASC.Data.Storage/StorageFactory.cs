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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.DiscStorage;
using ASC.Data.Storage.S3;

namespace ASC.Data.Storage
{
    public static class StorageFactory
    {
        private const string DefaultTenantName = "default";

        public static IDataStore GetStorage(string tenant, string module)
        {
            return GetStorage(tenant, module, HttpContext.Current);
        }

        public static IDataStore GetStorage(string tenant, string module, HttpContext context)
        {
            return GetStorage(string.Empty, tenant, module, context);
        }

        private static IDataStore GetStoreAndCache(string tenant, string module, StorageConfigurationSection section,
                                                   HttpContext context, IQuotaController controller)
        {
            IDataStore store = GetDataStore(tenant, section, module, context, controller);
            if (store != null)
            {
                DataStoreCache.Put(store, tenant, module);
            }
            return store;
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module)
        {
            return GetStorage(configpath, tenant, module, HttpContext.Current);
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module, HttpContext context)
        {
            int tenantId;
            int.TryParse(tenant, out tenantId);
            return GetStorage(configpath, tenant, module, context, new TennantQuotaController(tenantId));
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module, HttpContext context,
                                            IQuotaController controller)
        {
            if (tenant == null) tenant = DefaultTenantName;

            //Make tennant path
            tenant = TennantPath.CreatePath(tenant);

            IDataStore store = DataStoreCache.Get(tenant, module);
            if (store == null)
            {
                StorageConfigurationSection section = GetSection(configpath);
                if (section == null) throw new InvalidOperationException("config section not found");
                store = GetStoreAndCache(tenant, module, section, context, controller);
            }
            return store;
        }

        private static IDataStore GetDataStore(string tenant, StorageConfigurationSection section, string module,
                                               HttpContext context, IQuotaController controller)
        {
            ModuleConfigurationElement moduleElement = section.Modules.GetModuleElement(module);
            if (moduleElement == null) throw new ArgumentException("no such module");
            HandlerConfigurationElement handler = section.Handlers.GetHandler(moduleElement.Type);
            return
                ((IDataStore) Activator.CreateInstance(handler.Type, tenant, moduleElement, context)).Configure(
                handler.GetProperties()).SetQuotaController(moduleElement.Count?controller:null/*don't count quota if specified on module*/);
        }


        public static ICrossModuleTransferUtility GetCrossModuleTransferUtility(string srcConfigPath, int srcTenant, string srcModule,
                                                                                string destConfigPath, int destTenant, string destModule)
        {
            var srcConfigSection = GetSection(srcConfigPath);
            var descConfigSection = GetSection(destConfigPath);

            var srcModuleConfig = srcConfigSection.Modules.GetModuleElement(srcModule);
            var destModuleConfig = descConfigSection.Modules.GetModuleElement(destModule);

            var srcHandlerConfig = srcConfigSection.Handlers.GetHandler(srcModuleConfig.Type);
            var destHandlerConfig = descConfigSection.Handlers.GetHandler(destModuleConfig.Type);

            if (!string.Equals(srcModuleConfig.Type, destModuleConfig.Type, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Can't instance transfer utility for modules with different storage types");

            if (string.Equals(srcModuleConfig.Type, "disc", StringComparison.OrdinalIgnoreCase))
                return new DiscCrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(),
                                                          TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());

            if (string.Equals(srcModuleConfig.Type, "s3", StringComparison.OrdinalIgnoreCase))
                return new S3CrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(),
                                                        TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());

            return null;
        }


        public static IEnumerable<string> GetModuleList(string configpath)
        {
            StorageConfigurationSection section = GetSection(configpath);
            return section.Modules.Cast<ModuleConfigurationElement>().Where(x => x.Visible).Select(x => x.Name);
        }

        private static StorageConfigurationSection GetSection(string configpath)
        {
            StorageConfigurationSection section;
            if (!string.IsNullOrEmpty(configpath))
            {
                if (configpath.Contains("\\") && !Uri.IsWellFormedUriString(configpath, UriKind.Relative))
                    //Not mapped path
                {
                    var configMap = new ExeConfigurationFileMap
                                        {
                                            ExeConfigFilename =
                                                string.Compare(Path.GetExtension(configpath), ".config", true) == 0
                                                    ? configpath
                                                    : Path.Combine(configpath, "web.config")
                                        };
                    section =
                        (StorageConfigurationSection)
                        ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None).
                            GetSection(Schema.SECTION_NAME);
                }
                else
                {
                    section = (StorageConfigurationSection)
                              WebConfigurationManager.OpenWebConfiguration(configpath).GetSection(Schema.SECTION_NAME);
                }
            }
            else
            {
                //Nothing worked, try local
                section = (StorageConfigurationSection) ConfigurationManager.GetSection(Schema.SECTION_NAME);
            }
            return section;
        }

        public static IEnumerable<string> GetDomainList(string configpath, string modulename)
        {
            StorageConfigurationSection section = GetSection(configpath);
            if (section == null) throw new ArgumentException("config section not found");
            return
                section.Modules.Cast<ModuleConfigurationElement>().Where(
                    x => x.Name.Equals(modulename, StringComparison.OrdinalIgnoreCase)).Single().Domains.Cast
                    <DomainConfigurationElement>().Where(x => x.Visible).Select(x => x.Name);
        }
    }
}