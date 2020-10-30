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
using System.Configuration;
using System.Linq;

using Autofac;

namespace ASC.Common.DependencyInjection
{
    public class AutofacConfigLoader
    {
        public static ContainerBuilder Load(string containerName, string section = "autofac")
        {
            var container = new ContainerBuilder();

            var autofacConfigurationSection = (AutofacConfigurationSection)ConfigurationManagerExtension.GetSection(section);

            if (autofacConfigurationSection == null) return null;

            foreach (var component in autofacConfigurationSection.GetComponents(containerName))
            {
                var componentType = Type.GetType(component.Type);
                if (componentType == null) continue;

                var builder = container.RegisterType(componentType).AsSelf();

                if (!string.IsNullOrEmpty(component.Service))
                {
                    var serviceType = Type.GetType(component.Service);
                    builder.As(serviceType);

                    if (!string.IsNullOrEmpty(component.Name))
                    {
                        builder.Named(component.Name, serviceType)
                            .Named(component.Name.ToLower(), serviceType);
                    }
                }

                if (!string.IsNullOrEmpty(component.Name))
                {
                    builder.Named(component.Name, componentType)
                        .Named(component.Name.ToLower(), componentType);
                }

                if (component.Parameters != null && component.Parameters.Count > 0)
                {
                    foreach (var parameter in component.Parameters)
                    {
                        if (!string.IsNullOrEmpty(parameter.Type))
                        {
                            var parameterType = Type.GetType(parameter.Type);
                            var parameterName = parameter.Name;
                            if (parameterType == null) continue;

                            builder.WithParameter(
                                (pi, ctx) => parameterType.IsSubclassOf(pi.ParameterType) && pi.Name == parameterName,
                                (pi, ctx) => Activator.CreateInstance(parameterType));
                        }
                        else if (parameter.List != null && parameter.List.Any())
                        {
                            builder.WithParameter(new NamedParameter(parameter.Name, parameter.List.Select(r => r.Value).ToArray()));
                        }
                        else if (parameter.Dictionary != null && parameter.Dictionary.Any())
                        {
                            builder.WithParameter(new NamedParameter(parameter.Name, parameter.Dictionary.ToDictionary(r => r.Key, r => r.Value)));
                        }
                        else
                        {
                            builder.WithParameter(new NamedParameter(parameter.Name, parameter.Value));
                        }
                    }
                }

                switch (component.InstanceScope)
                {
                    case "single-instance":
                        builder.SingleInstance();
                        break;
                    case "per-lifetime-scope":
                        builder.InstancePerLifetimeScope();
                        break;
                    case "per-request":
                        builder.InstancePerRequest();
                        break;
                }

                if (component.InjectProperties == "yes")
                {
                    builder.PropertiesAutowired();
                }
            }

            return container;
        }
    }
}
