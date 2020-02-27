/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
