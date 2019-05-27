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
        public DictionaryElementCollection Props { get { return this[PropsElement] as DictionaryElementCollection; } }

        [ConfigurationProperty(AdditionalElement, IsRequired = false)]
        public DictionaryElementCollection Additional { get { return this[AdditionalElement] as DictionaryElementCollection; } }
        }

    public class ConsumerConfigLoader
    {
        public static ContainerBuilder LoadConsumers(string section)
        {
            var container = new ContainerBuilder();

            var autofacConfigurationSection = (ConsumerConfigurationSection)ConfigurationManager.GetSection(section);

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
                    builder.WithParameter(new NamedParameter(ConsumerElement.PropsElement, component.Props.ToDictionary(r => r.Key, r => r.Value)));
        }

                if (component.Additional != null && component.Additional.Any())
        {
                    builder.WithParameter(new NamedParameter(ConsumerElement.AdditionalElement, component.Additional.ToDictionary(r => r.Key, r => r.Value)));
        }
            }

            return container;
        }
    }
}
