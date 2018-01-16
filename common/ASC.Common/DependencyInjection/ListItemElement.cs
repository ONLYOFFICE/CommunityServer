using System.Configuration;

namespace ASC.Common.DependencyInjection
{
    public class ListItemElement : ConfigurationElement
    {
        private const string ValueAttributeName = "value";
        private const string KeyAttributeName = "key";

        [ConfigurationProperty(KeyAttributeName, IsRequired = false)]
        public string Key
        {
            get
            {
                return (string)this[KeyAttributeName];
            }
        }

        [ConfigurationProperty(ValueAttributeName, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this[ValueAttributeName];
            }
        }
    }
}
