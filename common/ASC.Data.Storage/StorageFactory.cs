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


using ASC.Core;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.DiscStorage;
using ASC.Data.Storage.GoogleCloud;
using ASC.Data.Storage.RackspaceCloud;
using ASC.Data.Storage.S3;
using ASC.Data.Storage.Selectel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;

namespace ASC.Data.Storage
{
    public static class StorageFactory
    {
        private const string DefaultTenantName = "default";

        public static IDataStore GetStorage(string tenant, string module)
        {
            return GetStorage(string.Empty, tenant, module);
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module)
        {
            int tenantId;
            int.TryParse(tenant, out tenantId);
            return GetStorage(configpath, tenant, module, new TennantQuotaController(tenantId));
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module, IQuotaController controller)
        {
            if (tenant == null) tenant = DefaultTenantName;

            //Make tennant path
            tenant = TennantPath.CreatePath(tenant);

            var store = DataStoreCache.Get(tenant, module);
            if (store == null)
            {
                var section = GetSection(configpath);
                if (section == null)
                {
                    throw new InvalidOperationException("config section not found");
                }
                store = GetStoreAndCache(tenant, module, section, controller);
            }
            return store;
        }

        public static ICrossModuleTransferUtility GetCrossModuleTransferUtility(string srcConfigPath, int srcTenant, string srcModule, string destConfigPath, int destTenant, string destModule)
        {
            var srcConfigSection = GetSection(srcConfigPath);
            var descConfigSection = GetSection(destConfigPath);

            var srcModuleConfig = srcConfigSection.Modules.GetModuleElement(srcModule);
            var destModuleConfig = descConfigSection.Modules.GetModuleElement(destModule);

            var srcHandlerConfig = srcConfigSection.Handlers.GetHandler(srcModuleConfig.Type);
            var destHandlerConfig = descConfigSection.Handlers.GetHandler(destModuleConfig.Type);

            if (!string.Equals(srcModuleConfig.Type, destModuleConfig.Type, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Can't instance transfer utility for modules with different storage types");
            }
            if (string.Equals(srcModuleConfig.Type, "disc", StringComparison.OrdinalIgnoreCase))
            {
                return new DiscCrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(), TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());
            }

            if (string.Equals(srcModuleConfig.Type, "s3", StringComparison.OrdinalIgnoreCase))
            {
                return new S3CrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(), TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());
            }

            if (string.Equals(srcModuleConfig.Type, "google", StringComparison.OrdinalIgnoreCase))
            {
                return new GoogleCloudCrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(), TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());
            }

            if (string.Equals(srcModuleConfig.Type, "selectel", StringComparison.OrdinalIgnoreCase))
            {
                return new SelectelCrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(), TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());
            }

            if (string.Equals(srcModuleConfig.Type, "rackspace", StringComparison.OrdinalIgnoreCase))
            {
                return new RackspaceCloudCrossModuleTransferUtility(TennantPath.CreatePath(srcTenant.ToString()), srcModuleConfig, srcHandlerConfig.GetProperties(), TennantPath.CreatePath(destTenant.ToString()), destModuleConfig, destHandlerConfig.GetProperties());
            }

            return null;
        }


        public static IEnumerable<string> GetModuleList(string configpath)
        {
            var section = GetSection(configpath);
            return section.Modules.Cast<ModuleConfigurationElement>().Where(x => x.Visible).Select(x => x.Name);
        }

        public static IEnumerable<string> GetDomainList(string configpath, string modulename)
        {
            var section = GetSection(configpath);
            if (section == null)
            {
                throw new ArgumentException("config section not found");
            }
            return
                section.Modules
                    .Cast<ModuleConfigurationElement>()
                    .Single(x => x.Name.Equals(modulename, StringComparison.OrdinalIgnoreCase))
                    .Domains.Cast<DomainConfigurationElement>()
                    .Where(x => x.Visible)
                    .Select(x => x.Name);
        }

        public static void InitializeHttpHandlers(string config = null)
        {
            if (!HostingEnvironment.IsHosted)
            {
                throw new InvalidOperationException("Application not hosted.");
            }

            var section = GetSection(config);
            if (section != null)
            {
                //old scheme
                var discHandler = section.Handlers.GetHandler("disc");
                if (discHandler != null)
                {
                    var props = discHandler.GetProperties();
                    foreach (var m in section.Modules.Cast<ModuleConfigurationElement>().Where(m => m.Type == "disc"))
                    {
                        if (m.Path.Contains(Constants.STORAGE_ROOT_PARAM))
                            DiscDataHandler.RegisterVirtualPath(
                                PathUtils.ResolveVirtualPath(m.VirtualPath),
                                PathUtils.ResolvePhysicalPath(m.Path, props),
                                m.Public);

                        foreach (var d in m.Domains.Cast<DomainConfigurationElement>().Where(d => (d.Type == "disc" || string.IsNullOrEmpty(d.Type)) && d.Path.Contains(Constants.STORAGE_ROOT_PARAM)))
                        {
                            DiscDataHandler.RegisterVirtualPath(
                                PathUtils.ResolveVirtualPath(d.VirtualPath),
                                PathUtils.ResolvePhysicalPath(d.Path, props));
                        }
                    }
                }

                //new scheme
                foreach (var m in section.Modules.Cast<ModuleConfigurationElement>())
                {
                    //todo: add path criterion
                    if (m.Type == "disc" || !m.Public || m.Path.Contains(Constants.STORAGE_ROOT_PARAM))
                        StorageHandler.RegisterVirtualPath(
                            m.Name,
                            string.Empty,
                            m.Public);

                    //todo: add path criterion
                    foreach (var d in m.Domains.Cast<DomainConfigurationElement>().Where(d => d.Path.Contains(Constants.STORAGE_ROOT_PARAM)))
                    {
                        StorageHandler.RegisterVirtualPath(
                            m.Name,
                            d.Name,
                            d.Public);
                    }
                }

            }
        }


        private static IDataStore GetStoreAndCache(string tenant, string module, StorageConfigurationSection section, IQuotaController controller)
        {
            var store = GetDataStore(tenant, section, module, controller);
            if (store != null)
            {
                DataStoreCache.Put(store, tenant, module);
            }
            return store;
        }

        private static IDataStore GetDataStore(string tenant, StorageConfigurationSection section, string module, IQuotaController controller)
        {
            var moduleElement = section.Modules.GetModuleElement(module);
            if (moduleElement == null)
            {
                throw new ArgumentException("no such module", module);
            }

            var handler = section.Handlers.GetHandler(moduleElement.Type);
            return ((IDataStore)Activator.CreateInstance(handler.Type, tenant, handler, moduleElement))
                .Configure(handler.GetProperties())
                .SetQuotaController(moduleElement.Count ? controller : null /*don't count quota if specified on module*/ );
        }

        private static StorageConfigurationSection GetSection(string configpath)
        {
            StorageConfigurationSection section;
            if (!string.IsNullOrEmpty(configpath))
            {
                if (configpath.Contains(Path.DirectorySeparatorChar) && (!Uri.IsWellFormedUriString(configpath, UriKind.Relative) || WorkContext.IsMono))
                {
                    //Not mapped path
                    var filename = string.Compare(Path.GetExtension(configpath), ".config", true) == 0 ? configpath : Path.Combine(configpath, "web.config");
                    var configMap = new ExeConfigurationFileMap { ExeConfigFilename = filename };
                    section = (StorageConfigurationSection)ConfigurationManager
                        .OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None)
                        .GetSection(Schema.SECTION_NAME);
                    section.SetSourceFile(filename);
                }
                else
                {
                    section = (StorageConfigurationSection)WebConfigurationManager
                        .OpenWebConfiguration(configpath)
                        .GetSection(Schema.SECTION_NAME);
                }
            }
            else
            {
                section = (StorageConfigurationSection)ConfigurationManager.GetSection(Schema.SECTION_NAME);
            }
            return section;
        }
    }
}