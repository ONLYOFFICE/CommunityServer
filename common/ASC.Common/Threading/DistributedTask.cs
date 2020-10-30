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


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Common.Threading
{
    public class DistributedTask : ISerializable
    {
        [JsonProperty]
        private readonly Dictionary<string, string> props = new Dictionary<string, string>();


        internal Action<DistributedTask> Publication { get; set; }

        [JsonProperty]
        public string InstanseId { get; internal set; }

        [JsonProperty]
        public string Id { get; private set; }

        [JsonProperty]
        public DistributedTaskStatus Status { get; internal set; }

        [JsonProperty]
        public AggregateException Exception { get; internal set; }



        public DistributedTask()
        {
            Id = Guid.NewGuid().ToString();
        }

        protected DistributedTask(SerializationInfo info, StreamingContext context)
        {
            InstanseId = info.GetValue("InstanseId", typeof(object)).ToString();
            Id = info.GetValue("Id", typeof(object)).ToString();
            Status = (DistributedTaskStatus)info.GetValue("Status", typeof(DistributedTaskStatus));
            Exception = (AggregateException)info.GetValue("Exception", typeof(AggregateException));
            foreach (var p in info)
            {
                if (p.Name.StartsWith("_"))
                {
                    props[p.Name.TrimStart('_')] = p.Value.ToString();
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("InstanseId", InstanseId);
            info.AddValue("Id", Id);
            info.AddValue("Status", Status);
            info.AddValue("Exception", Exception);
            foreach (var p in props)
            {
                info.AddValue("_" + p.Key, p.Value);
            }
        }


        public T GetProperty<T>(string name)
        {
            return props.ContainsKey(name) ? JsonConvert.DeserializeObject<T>(props[name]) : default(T);
        }

        public void SetProperty(string name, object value)
        {
            if (value != null)
            {
                props[name] = JsonConvert.SerializeObject(value);
            }
            else
            {
                props.Remove(name);
            }
        }

        public void PublishChanges()
        {
            if (Publication == null)
            {
                throw new InvalidOperationException("Publication not found.");
            }
            Publication(this);
        }
    }
}
