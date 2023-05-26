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

using ASC.Common.DependencyInjection;

using Autofac;

namespace ASC.Core.Common.Configuration
{
    public class ConsumerConfigurationSection : ConfigurationSection
    {
        private const string ComponentsPropertyName = "components";

        [ConfigurationProperty(ComponentsPropertyName, IsRequired = false)]
        public ConsumersElementCollection Containers
        {
            get
            {
                return (ConsumersElementCollection)this[ComponentsPropertyName];
            }
        }
    }

    public class ConsumersElementCollection : ConfigurationElementCollection<ConsumerElement>
    {
        [ConfigurationProperty("name", IsRequired = false)]
        public string Name { get { return (string)this["name"]; } }

        public ConsumersElementCollection()
            : base("component")
        {
        }
    }

    public class ConsumerElement : ComponentElement
    {
        public const string OrderElement = "order";
        public const string PropsElement = "props";
        public const string AdditionalElement = "additional";

        [ConfigurationProperty(OrderElement, IsRequired = false)]
        public int Order { get { return Convert.ToInt32(this[OrderElement]); } }

        [ConfigurationProperty(PropsElement, IsRequired = false)]
        public PropDictionaryElementCollection Props { get { return this[PropsElement] as PropDictionaryElementCollection; } }
    }

    public class PropListItemElement : ConfigurationElement
    {
        private const string valueAttributeName = "value";
        private const string keyAttributeName = "key";
        private const string hiddenAttributeName = "hidden";
        private const string optionalAttributeName = "optional";
        private const string passwordAttributeName = "password";

        [ConfigurationProperty(keyAttributeName, IsRequired = true)]
        public string Key
        {
            get
            {
                return (string)this[keyAttributeName];
            }
        }

        [ConfigurationProperty(valueAttributeName, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this[valueAttributeName];
            }
        }

        [ConfigurationProperty(hiddenAttributeName, IsRequired = false)]
        public bool Hidden
        {
            get
            {
                return (bool)this[hiddenAttributeName];
            }
        }

        [ConfigurationProperty(optionalAttributeName, IsRequired = false)]
        public bool Optional
        {
            get
            {
                return (bool)this[optionalAttributeName];
            }
        }

        [ConfigurationProperty(passwordAttributeName, IsRequired = false)]
        public bool Password
        {
            get
            {
                return (bool)this[passwordAttributeName];
            }
        }
    }

    public class PropDictionaryElementCollection : ConfigurationElementCollection<PropListItemElement>
    {
        public PropDictionaryElementCollection()
          : base("item")
        {
        }
    }

    public class Prop
    {
        public string value;
        public bool optional;
        public bool password;
    }

    public class ConsumerConfigLoader
    {
        public static ContainerBuilder LoadConsumers(string section)
        {
            var container = new ContainerBuilder();

            var autofacConfigurationSection = (ConsumerConfigurationSection)ConfigurationManagerExtension.GetSection(section);

            foreach (var component in autofacConfigurationSection.Containers)
            {
                var componentType = Type.GetType(component.Type);
                var builder = container.RegisterType(componentType)
                    .AsSelf()
                    .As<Consumer>()
                    .SingleInstance();

                if (!string.IsNullOrEmpty(component.Name))
                {
                    builder
                        .Named<Consumer>(component.Name)
                        .Named(component.Name, componentType)
                        .Named<Consumer>(component.Name.ToLower())
                        .Named(component.Name.ToLower(), componentType);
                }

                builder.WithParameter(new NamedParameter("name", component.Name));
                builder.WithParameter(new NamedParameter(ConsumerElement.OrderElement, component.Order));

                if (component.Props != null && component.Props.Any())
                {
                    var props = component.Props.Where(r=> !r.Hidden).ToDictionary(r => r.Key, r => new Prop() { value = r.Value, password = r.Password, optional = r.Optional });
                    var additional = component.Props.Where(r=> r.Hidden).ToDictionary(r => r.Key, r => new Prop() { value = r.Value, password = r.Password, optional = r.Optional });
                    builder.WithParameter(new NamedParameter(ConsumerElement.PropsElement, props));
                    builder.WithParameter(new NamedParameter(ConsumerElement.AdditionalElement, additional));
                }
            }

            return container;
        }
    }
}
