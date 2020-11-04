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

using ASC.Core.Common.Configuration;
using ASC.Core.Common.Settings;
using ASC.Data.Storage.Configuration;
using ASC.Web.Studio.UserControls.Management;

namespace ASC.Api.Settings
{
    public class StorageWrapper
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public List<AuthKey> Properties { get; set; }

        public bool Current { get; set; }

        public bool IsSet { get; set; }

        public StorageWrapper(DataStoreConsumer consumer, StorageSettings current)
        {
            StorageWrapperInit(consumer, current);
        }

        public StorageWrapper(DataStoreConsumer consumer, CdnStorageSettings current)
        {
            StorageWrapperInit(consumer, current);
        }

        private void StorageWrapperInit<T>(DataStoreConsumer consumer, BaseStorageSettings<T> current) where T : class, ISettings, new()
        {
            Id = consumer.Name;
            Title = consumer.GetResourceString(consumer.Name) ?? consumer.Name;
            Current = consumer.Name == current.Module;
            IsSet = consumer.IsSet;

            var props = Current
                ? current.Props
                : current.Switch(consumer).AdditionalKeys.ToDictionary(r => r, a => consumer[a]);

            Properties = props.Select(
                r => new AuthKey
                {
                    Name = r.Key,
                    Value = r.Value,
                    Title = consumer.GetResourceString(consumer.Name + r.Key) ?? r.Key,
                    Description = consumer.GetResourceString(consumer.Name + r.Key + "Description"),
                }).ToList();
        }
    }
}
