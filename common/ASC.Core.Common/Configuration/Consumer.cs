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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using ASC.Common.Caching;
using ASC.Core.Tenants;

using Autofac;

namespace ASC.Core.Common.Configuration
{
    public class Consumer : IDictionary<string, string>
    {
        private static readonly ICacheNotify Cache = AscCache.Notify;

        public bool CanSet { get; private set; }

        public int Order { get; private set; }

        public string Name { get; private set; }

        protected readonly Dictionary<string, string> Props;
        public IEnumerable<string> ManagedKeys
        {
            get { return Props.Select(r => r.Key).ToList(); }
        }

        protected readonly Dictionary<string, string> Additional;
        public virtual IEnumerable<string> AdditionalKeys
        {
            get { return Additional.Select(r => r.Key).ToList(); }
        }

        public ICollection<string> Keys { get { return AllProps.Keys; } }
        public ICollection<string> Values { get { return AllProps.Values; } }

        private Dictionary<string, string> AllProps
        {
            get
            {
                var result = Props.ToDictionary(item => item.Key, item => item.Value);

                foreach (var item in Additional.Where(item => !result.ContainsKey(item.Key)))
                {
                    result.Add(item.Key, item.Value);
                }

                return result;
            }
        }

        private ICollection<string> Optionals { get; set; }
        private ICollection<string> Passwords { get; set; }

        private static readonly bool OnlyDefault;

        public bool IsSet
        {
            get { return Props.Any() && !Props.All(r => string.IsNullOrEmpty(this[r.Key])); }
        }

        static Consumer()
        {
            OnlyDefault = ConfigurationManagerExtension.AppSettings["core.default-consumers"] == "true";
        }

        public Consumer()
        {
            Name = "";
            Order = int.MaxValue;
            Props = new Dictionary<string, string>();
            Additional = new Dictionary<string, string>();
            Optionals = new List<string>();
            Passwords = new List<string>();
        }

        public Consumer(string name, int order, Dictionary<string, Prop> additional)
        {
            Name = name;
            Order = order;
            Props = new Dictionary<string, string>();
            Additional = ConvertToDictionaryString(additional);
            Optionals = GetOptionalKeys(additional);
            Passwords = GetPasswordsKeys(additional);
        }

        public Consumer(string name, int order, Dictionary<string, Prop> props, Dictionary<string, Prop> additional)
        {
            Name = name;
            Order = order;
            Props = ConvertToDictionaryString(props) ?? new Dictionary<string, string>();
            Additional = ConvertToDictionaryString(additional) ?? new Dictionary<string, string>();
            Optionals = GetOptionalKeys(props, additional);
            Passwords = GetPasswordsKeys(props, additional);
            if (props != null && props.Any())
            {
                CanSet = props.All(r => string.IsNullOrEmpty(r.Value.value));
            }
        }

        public bool IsOptional(string key)
        {
            return Optionals.Contains(key);
        }

        public bool IsPassword(string key)
        {
            return Passwords.Contains(key);
        }


        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return AllProps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
        }

        public void Clear()
        {
            if (!CanSet)
            {
                throw new NotSupportedException("Key for read only. Consumer " + Name);
            }

            foreach (var providerProp in Props)
            {
                this[providerProp.Key] = null;
            }

            Cache.Publish(new ConsumerCacheItem { Name = this.Name, TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId }, CacheNotifyAction.Remove);
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return AllProps.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return AllProps.Remove(item.Key);
        }

        public int Count { get { return AllProps.Count; } }

        public bool IsReadOnly { get { return true; } }

        public bool ContainsKey(string key)
        {
            return AllProps.ContainsKey(key);
        }

        public void Add(string key, string value)
        {
        }

        public bool Remove(string key)
        {
            return false;
        }

        public bool TryGetValue(string key, out string value)
        {
            return AllProps.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        private string Get(string name)
        {
            string value = null;

            if (!OnlyDefault && CanSet)
            {
                var tenant = CoreContext.Configuration.Standalone
                                 ? Tenant.DEFAULT_TENANT
                                 : CoreContext.TenantManager.GetCurrentTenant().TenantId;

                value = CoreContext.Configuration.GetSetting(GetSettingsKey(name), tenant);
            }

            if (string.IsNullOrEmpty(value))
            {
                if (AllProps.ContainsKey(name))
                {
                    value = AllProps[name];
                }
            }

            return value;
        }

        private void Set(string name, string value)
        {
            if (!CanSet)
            {
                throw new NotSupportedException("Key for read only. Key " + name);
            }

            if (!ManagedKeys.Contains(name))
            {
                if (Additional.ContainsKey(name))
                {
                    Additional[name] = value;
                }
                else
                {
                    Additional.Add(name, value);
                }
                return;
            }

            var tenant = CoreContext.Configuration.Standalone
                             ? Tenant.DEFAULT_TENANT
                             : CoreContext.TenantManager.GetCurrentTenant().TenantId;
            CoreContext.Configuration.SaveSetting(GetSettingsKey(name), value, tenant);
        }

        protected virtual string GetSettingsKey(string name)
        {
            return "AuthKey_" + name;
        }

        protected Dictionary<string, string> ConvertToDictionaryString(Dictionary<string, Prop> props)
        {
            if (props == null) return null;
            return props.ToDictionary(r => r.Key, r => r.Value.value);
        }

        protected Dictionary<string, Prop> ConvertToDictionaryProp(Dictionary<string, string> props)
        {
            if (props == null) return null;
            return props.ToDictionary(r => r.Key, r => new Prop() { value = r.Value , optional = Optionals.Contains(r.Key), password = Passwords.Contains(r.Key)});
        }

        private List<string> GetOptionalKeys(Dictionary<string, Prop> props)
        {
            if (props == null) return null;
            return props.Where(r => r.Value.optional).Select(r=> r.Key).ToList();
        }

        private List<string> GetOptionalKeys(Dictionary<string, Prop> props1, Dictionary<string, Prop> props2)
        {
            if (props1 == null && props2 == null) return null;

            props1 = props1 ?? new Dictionary<string, Prop>();
            props2 = props2 ?? new Dictionary<string, Prop>();

            var props = props1.Union(props2);
            return props.Where(r => r.Value.optional).Select(r => r.Key).ToList();
        }

        private List<string> GetPasswordsKeys(Dictionary<string, Prop> props)
        {
            if (props == null) return null;
            return props.Where(r => r.Value.password).Select(r => r.Key).ToList();
        }

        private List<string> GetPasswordsKeys(Dictionary<string, Prop> props1, Dictionary<string, Prop> props2)
        {
            if (props1 == null && props2 == null) return null;

            props1 = props1 ?? new Dictionary<string, Prop>();
            props2 = props2 ?? new Dictionary<string, Prop>();

            var props = props1.Union(props2);
            return props.Where(r => r.Value.password).Select(r => r.Key).ToList();
        }
    }

    public class DataStoreConsumer : Consumer, ICloneable
    {
        public Type HandlerType { get; private set; }
        public DataStoreConsumer Cdn { get; private set; }

        public const string HandlerTypeKey = "handlerType";
        public const string CdnKey = "cdn";

        public DataStoreConsumer()
        {

        }

        public DataStoreConsumer(string name, int order, Dictionary<string, Prop> additional)
            : base(name, order, additional)
        {
            Init(additional);
        }

        public DataStoreConsumer(string name, int order, Dictionary<string, Prop> props, Dictionary<string, Prop> additional)
            : base(name, order, props, additional)
        {
            Init(additional);
        }

        public override IEnumerable<string> AdditionalKeys
        {
            get { return base.AdditionalKeys.Where(r => r != HandlerTypeKey && r != "cdn").ToList(); }
        }

        protected override string GetSettingsKey(string name)
        {
            return base.GetSettingsKey(Name + name);
        }

        private void Init(IReadOnlyDictionary<string, Prop> additional)
        {
            if (additional == null || !additional.ContainsKey(HandlerTypeKey))
                throw new ArgumentException(HandlerTypeKey);

            HandlerType = Type.GetType(additional[HandlerTypeKey].value);

            if (additional.ContainsKey(CdnKey))
            {
                Cdn = GetCdn(additional[CdnKey].value);
            }
        }

        private DataStoreConsumer GetCdn(string cdn)
        {
            var fromConfig = ConsumerFactory.GetByName<Consumer>(cdn);

            var props = ManagedKeys.ToDictionary(prop => prop, prop => this[prop]);
            var additional = fromConfig.AdditionalKeys.ToDictionary(prop => prop, prop => fromConfig[prop]);
            additional.Add(HandlerTypeKey, HandlerType.AssemblyQualifiedName);

            return new DataStoreConsumer(fromConfig.Name, fromConfig.Order, ConvertToDictionaryProp(props), ConvertToDictionaryProp(additional));
        }

        public object Clone()
        {
            var props = ConvertToDictionaryProp(Props);
            var additional = ConvertToDictionaryProp(Additional);
            return new DataStoreConsumer(Name, Order, props, additional);
        }
    }

    public class ConsumerFactory
    {
        public static IEnumerable<Consumer> Consumers { get; private set; }

        private static IContainer Builder { get; set; }

        static ConsumerFactory()
        {
            var container = ConsumerConfigLoader.LoadConsumers("consumers");
            Builder = container.Build();
            Consumers = Builder.Resolve<IEnumerable<Consumer>>();
        }

        public static Consumer GetByName(string name)
        {
            object result;
            if (Builder.TryResolveNamed(name, typeof(Consumer), out result))
            {
                return (Consumer)result;
            }

            return new Consumer();
        }

        public static T GetByName<T>(string name) where T : Consumer, new()
        {
            object result;
            if (Builder.TryResolveNamed(name, typeof(T), out result))
            {
                return (T)result;
            }

            return new T();
        }

        public static T Get<T>() where T : Consumer, new()
        {
            T result;
            if (Builder.TryResolve(out result))
            {
                return result;
            }

            return new T();
        }

        public static IEnumerable<T> GetAll<T>() where T : Consumer, new()
        {
            return Builder.Resolve<IEnumerable<T>>();
        }
    }

    public class ConsumerCacheItem
    {
        public string Name { get; set; }
        public int TenantId { get; set; }

    }
}
