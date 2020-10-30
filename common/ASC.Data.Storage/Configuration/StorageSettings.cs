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
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Optimization;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Settings;

namespace ASC.Data.Storage.Configuration
{
    [Serializable]
    [DataContract]
    public abstract class BaseStorageSettings<T> : BaseSettings<T> where T: class, ISettings, new()
    {
        [DataMember(Name = "Module")]
        public string Module { get; set; }

        [DataMember(Name = "Props")]
        public Dictionary<string, string> Props { get; set; }

        public override ISettings GetDefault()
        {
            return new T();
        }

        static BaseStorageSettings()
        {
            AscCache.Notify.Subscribe<Consumer>((i, a) =>
            {
                if (a == CacheNotifyAction.Remove)
                {
                    var settings = StorageSettings.Load();
                    if (i.Name == settings.Module)
                    {
                        settings.Clear();
                    }

                    var cdnSettings = CdnStorageSettings.Load();
                    if (i.Name == cdnSettings.Module)
                    {
                        cdnSettings.Clear();
                    }
                }
            });
        }

        public override bool Save()
        {
            StorageFactory.ClearCache();
            dataStoreConsumer = null;
            return base.Save();
        }

        public virtual void Clear()
        {
            Module = null;
            Props = null;
            Save();
        }

        private DataStoreConsumer dataStoreConsumer;
        public DataStoreConsumer DataStoreConsumer
        {
            get
            {
                if (string.IsNullOrEmpty(Module) || Props == null) return dataStoreConsumer = new DataStoreConsumer();

                var consumer = ConsumerFactory.GetByName<DataStoreConsumer>(Module);

                if (!consumer.IsSet) return dataStoreConsumer = new DataStoreConsumer();

                dataStoreConsumer = (DataStoreConsumer)consumer.Clone();

                foreach (var prop in Props)
                {
                    dataStoreConsumer[prop.Key] = prop.Value;
                }

                return dataStoreConsumer;
            }
        }

        private IDataStore dataStore;
        public IDataStore DataStore
        {
            get
            {
                if (dataStore != null) return dataStore;

                if (DataStoreConsumer.HandlerType == null) return null;

                return dataStore = ((IDataStore)
                    Activator.CreateInstance(DataStoreConsumer.HandlerType, CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString()))
                    .Configure(DataStoreConsumer);
            }
        }

        public virtual Func<DataStoreConsumer, DataStoreConsumer> Switch { get { return d => d; } }
    }

    [Serializable]
    [DataContract]
    public class StorageSettings : BaseStorageSettings<StorageSettings>
    {
        public override Guid ID
        {
            get { return new Guid("F13EAF2D-FA53-44F1-A6D6-A5AEDA46FA2B"); }
        }
    }

    [Serializable]
    [DataContract]
    public class CdnStorageSettings : BaseStorageSettings<CdnStorageSettings>
    {
        public override Guid ID
        {
            get { return new Guid("0E9AE034-F398-42FE-B5EE-F86D954E9FB2"); }
        }

        public override Func<DataStoreConsumer, DataStoreConsumer> Switch { get { return d => d.Cdn; } }

        public override void Clear()
        {
            base.Clear();
            BundleTable.Bundles.Clear();
        }
    }

}
