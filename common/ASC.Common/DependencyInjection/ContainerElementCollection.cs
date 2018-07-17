using System.Configuration;

namespace ASC.Common.DependencyInjection
{
    public class ContainerElementCollection : ConfigurationElementCollection<ComponentElementCollection>
    {
        [ConfigurationProperty("name", IsRequired = false)]
        public string Name { get { return (string)this["name"]; } }

        public ContainerElementCollection()
          : base("container")
        {
        }
    }

    public class ComponentElementCollection : ConfigurationElementCollection<ComponentElement>
    {
        [ConfigurationProperty("name", IsRequired = false)]
        public string Name { get { return (string)this["name"]; } }

        public ComponentElementCollection()
          : base("component")
        {
        }
    }
}
