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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.DiscStorage;

namespace ASC.Data.Storage
{
    public static class StorageFactory
    {
        private const string DefaultTenantName = "default";
        private static readonly ICacheNotify Cache;

        static StorageFactory()
        {
            Cache = AscCache.Notify;
            Cache.Subscribe<DataStoreCacheItem>((r, act) => DataStoreCache.Remove(r.TenantId, r.Module));
        }

        public static IDataStore GetStorage(string tenant, string module)
        {
            return GetStorage(string.Empty, tenant, module);
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module)
        {
            int tenantId;
            int.TryParse(tenant, out tenantId);
            return GetStorage(configpath, tenant, module, new TenantQuotaController(tenantId));
        }

        public static IDataStore GetStorage(string configpath, string tenant, string module, IQuotaController controller)
        {
            var tenantId = -2;
            if (string.IsNullOrEmpty(tenant))
            {
                tenant = DefaultTenantName;
            }
            else
            {
                tenantId = Convert.ToInt32(tenant);
            }

            //Make tenant path
            tenant = TenantPath.CreatePath(tenant);

            var store = DataStoreCache.Get(tenant, module);
            if (store == null)
            {
                var sectionKey = "StorageConfigurationSection" + (configpath ?? "").Replace("\\", "").Replace("/", "");
                var section = AscCache.Memory.Get<StorageConfigurationSection>(sectionKey);

                if (section == null)
                {
                    section = GetSection(configpath);
                    AscCache.Memory.Insert(sectionKey, section, DateTime.MaxValue);
                }

                if (section == null)
                {
                    throw new InvalidOperationException("config section not found");
                }

                var settings = StorageSettings.LoadForTenant(tenantId);

                store = GetStoreAndCache(tenant, module, section, settings.DataStoreConsumer, controller);
            }
            return store;
        }

        public static IDataStore GetStorageFromConsumer(string configpath, string tenant, string module, DataStoreConsumer consumer)
        {
            if (tenant == null) tenant = DefaultTenantName;

            //Make tenant path
            tenant = TenantPath.CreatePath(tenant);

            var section = GetSection(configpath);
            if (section == null)
            {
                throw new InvalidOperationException("config section not found");
            }

            int tenantId;
            int.TryParse(tenant, out tenantId);
            return GetDataStore(tenant, module, section, consumer, new TenantQuotaController(tenantId));
        }

        public static IEnumerable<string> GetModuleList(string configpath, bool exceptDisabledMigration = false)
        {
            var section = GetSection(configpath);
            return section.Modules.Cast<ModuleConfigurationElement>()
                .Where(x => x.Visible)
                .Where(x => !exceptDisabledMigration || !x.DisabledMigrate)
                .Select(x => x.Name);
        }

        public static IEnumerable<string> GetModuleList(string configpath, string type, bool exceptDisabledMigration = false)
        {
            var section = GetSection(configpath);
            return section.Modules.Cast<ModuleConfigurationElement>()
                .Where(x => x.Visible && x.Type == type)
                .Where(x => !exceptDisabledMigration || !x.DisabledMigrate)
                .Select(x => x.Name);
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

        internal static void ClearCache()
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString();
            var path = TenantPath.CreatePath(tenantId);
            foreach (var module in GetModuleList("", true))
            {
                Cache.Publish(DataStoreCacheItem.Create(path, module), CacheNotifyAction.Remove);
            }
        }

        private static IDataStore GetStoreAndCache(string tenant, string module, StorageConfigurationSection section, DataStoreConsumer consumer, IQuotaController controller)
        {
            var store = GetDataStore(tenant, module, section, consumer, controller);
            if (store != null)
            {
                DataStoreCache.Put(store, tenant, module);
            }
            return store;
        }

        private static IDataStore GetDataStore(string tenant, string module, StorageConfigurationSection section, DataStoreConsumer consumer, IQuotaController controller)
        {
            var moduleElement = section.Modules.GetModuleElement(module);
            if (moduleElement == null)
            {
                throw new ArgumentException("no such module", module);
            }

            var handler = section.Handlers.GetHandler(moduleElement.Type);
            Type instanceType;
            IDictionary<string, string> props;

            if (CoreContext.Configuration.Standalone &&
                !moduleElement.DisabledMigrate &&
                consumer.IsSet)
            {
                instanceType = consumer.HandlerType;
                props = consumer;
            }
            else
            {
                instanceType = handler.Type;
                props = handler.GetProperties();
            }

            return ((IDataStore)Activator.CreateInstance(instanceType, tenant, handler, moduleElement))
                .Configure(props)
                .SetQuotaController(moduleElement.Count ? controller : null
                /*don't count quota if specified on module*/);
        }

        private static StorageConfigurationSection GetSection(string configpath)
        {
            StorageConfigurationSection section;
            if (!string.IsNullOrEmpty(configpath))
            {
                if (configpath.Contains(Path.DirectorySeparatorChar) && (!Uri.IsWellFormedUriString(configpath, UriKind.Relative) || WorkContext.IsMono))
                {
                    //Not mapped path
                    var filename = string.Compare(Path.GetExtension(configpath), ".config", true) == 0 ? configpath : Path.Combine(configpath, "Web.config");
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
                var a = ConfigurationManagerExtension.GetSection(Schema.SECTION_NAME);
                LogManager.GetLogger("ASC").Debug("a != null " + (a != null));
                section = (StorageConfigurationSection)a;
            }
            return section;
        }
    }
}