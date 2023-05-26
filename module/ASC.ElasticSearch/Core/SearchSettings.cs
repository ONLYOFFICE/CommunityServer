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
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.ElasticSearch.Service;

using Autofac;

using Newtonsoft.Json;

namespace ASC.ElasticSearch.Core
{
    [Serializable]
    [DataContract]
    public class SearchSettings : BaseSettings<SearchSettings>
    {
        [DataMember(Name = "Data")]
        public string Data { get; set; }

        public override Guid ID
        {
            get { return new Guid("{93784AB2-10B5-4C2F-9B36-F2662CCCF316}"); }
        }

        public override ISettings GetDefault()
        {
            return new SearchSettings();
        }

        private List<SearchSettingsItem> items;
        private List<SearchSettingsItem> Items
        {
            get
            {
                if (items != null) return items;
                var parsed = JsonConvert.DeserializeObject<List<SearchSettingsItem>>(Data ?? "");
                return items = parsed ?? new List<SearchSettingsItem>();
            }
        }

        private static List<WrapperWithDoc> allItems;
        private static List<WrapperWithDoc> AllItems
        {
            get
            {
                return allItems ?? (allItems = FactoryIndexer.Builder.Resolve<IEnumerable<Wrapper>>()
                               .OfType<WrapperWithDoc>()
                               .ToList());
            }
        }

        public static bool CanIndexByContent<T>(int tenantId) where T : Wrapper, new()
        {
            return CanIndexByContent(typeof(T), tenantId);
        }

        public static bool CanIndexByContent(Type t, int tenantId)
        {
            if (!t.IsSubclassOf(typeof(WrapperWithDoc)))
            {
                return false;
            }

            if (Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["core.search-by-content"] ?? "false")) return true;

            if (!CoreContext.Configuration.Standalone) return true;

            var settings = LoadForTenant(tenantId);

            return settings.IsEnabled(((WrapperWithDoc)Activator.CreateInstance(t)).IndexName);
        }

        public static bool CanSearchByContent<T>() where T : Wrapper, new()
        {
            return CanSearchByContent(typeof(T));
        }

        public static bool CanSearchByContent(Type t)
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            if (!CanIndexByContent(t, tenantId)) return false;

            return CoreContext.Configuration.Standalone || CoreContext.TenantManager.GetTenantQuota(tenantId).ContentSearch;
        }

        public static List<SearchSettingsItem> GetAllItems()
        {
            if (!CoreContext.Configuration.Standalone) return new List<SearchSettingsItem>();

            var settings = Load();

            return AllItems.Select(r => new SearchSettingsItem
            {
                ID = r.IndexName,
                Enabled = settings.IsEnabled(r.IndexName),
                Title = r.SettingsTitle
            }).ToList();
        }

        public static void Set(List<SearchSettingsItem> items)
        {
            if (!CoreContext.Configuration.Standalone) return;

            var settings = Load();

            var toReIndex = !settings.Items.Any() ? items.Where(r => r.Enabled).ToList() : items.Where(item => settings.items.Any(r => r.ID == item.ID && r.Enabled != item.Enabled)).ToList();

            settings.items = items;
            settings.Data = JsonConvert.SerializeObject(items);
            settings.Save();

            using (var service = new ServiceClient())
            {
                service.ReIndex(toReIndex.Select(r => r.ID).ToList(), CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
        }

        private bool IsEnabled(string name)
        {
            var wrapper = Items.FirstOrDefault(r => r.ID == name);

            return wrapper != null && wrapper.Enabled;
        }
    }

    [Serializable]
    [DataContract]
    public class SearchSettingsItem
    {
        [DataMember(Name = "ID")]
        public string ID { get; set; }

        [DataMember(Name = "Enabled")]
        public bool Enabled { get; set; }

        public string Title { get; set; }
    }
}
