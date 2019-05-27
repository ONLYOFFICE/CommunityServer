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


using System;
using System.Collections.Generic;
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

        public bool CanSearchByContent<T>() where T : Wrapper, new ()
        {
            if (!SearchByContentEnabled) return false;

            if (!typeof(T).IsSubclassOf(typeof(WrapperWithDoc)))
            {
                return false;
            }

            return IsEnabled(new T().IndexName);
        }

        public static List<SearchSettingsItem> GetAllItems()
        {
            if (!SearchByContentEnabled) return new List<SearchSettingsItem>();

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
            if (!SearchByContentEnabled) return;

            var settings = Load();

            var toReIndex = !settings.Items.Any() ? items.Where(r=> r.Enabled).ToList() : items.Where(item => settings.items.Any(r => r.ID == item.ID && r.Enabled != item.Enabled)).ToList();

            settings.items = items;
            settings.Data = JsonConvert.SerializeObject(items);
            settings.Save();

            using (var service = new ServiceClient())
            {
                service.ReIndex(toReIndex.Select(r=> r.ID).ToList(), CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
        }

        private static bool SearchByContentEnabled
        {
            get
            {
                return CoreContext.Configuration.Standalone;
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
