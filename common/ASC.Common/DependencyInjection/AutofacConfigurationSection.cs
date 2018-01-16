using System.Configuration;
using System.Linq;

namespace ASC.Common.DependencyInjection
{
    public class AutofacConfigurationSection : ConfigurationSection
    {
        private const string ComponentsPropertyName = "components";

        [ConfigurationProperty(ComponentsPropertyName, IsRequired = false)]
        public ContainerElementCollection Containers
        {
            get
            {
                return (ContainerElementCollection)this[ComponentsPropertyName];
            }
        }

        public ComponentElementCollection GetComponents(string containerName)
        {
            return Containers.FirstOrDefault(r => r.Name == containerName);
        }
    }
}
