using System;
using System.Configuration;
using Autofac;

namespace ASC.Common.DependencyInjection
{
    public class AutofacConfigLoader
    {
        public static ContainerBuilder Load(string containerName)
        {
            var container = new ContainerBuilder();

            var autofacConfigurationSection = (AutofacConfigurationSection) ConfigurationManager.GetSection("autofac");

            foreach (var component in autofacConfigurationSection.GetComponents(containerName))
            {
                var builder = container.RegisterType(Type.GetType(component.Type)).AsSelf();

                if (!string.IsNullOrEmpty(component.Service))
                {
                    var serviceType = Type.GetType(component.Service);
                    builder.As(serviceType);

                    if (!string.IsNullOrEmpty(component.Name))
                    {
                        builder.Named(component.Name, serviceType);
                    }
                }

                if (component.Parameters != null && component.Parameters.Count > 0)
                {
                    foreach (var parameter in component.Parameters)
                    {
                        builder.WithParameter(new NamedParameter(parameter.Name, parameter.Value));
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
