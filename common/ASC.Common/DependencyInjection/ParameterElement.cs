using System.Configuration;


namespace ASC.Common.DependencyInjection
{
    public class ParameterElement : ConfigurationElement
    {
        private const string NameAttributeName = "name";
        private const string ValueAttributeName = "value";
        private const string ListElementName = "list";
        private const string DictionaryElementName = "dictionary";
        internal const string Key = "name";

        [ConfigurationProperty(NameAttributeName, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this[NameAttributeName];
            }
        }

        [ConfigurationProperty(ValueAttributeName, IsRequired = false)]
        public string Value
        {
            get
            {
                return (string)this[ValueAttributeName];
            }
        }

        [ConfigurationProperty(ListElementName, DefaultValue = null, IsRequired = false)]
        public ListElementCollection List
        {
            get
            {
                return this[ListElementName] as ListElementCollection;
            }
        }

        [ConfigurationProperty(DictionaryElementName, DefaultValue = null, IsRequired = false)]
        public DictionaryElementCollection Dictionary
        {
            get
            {
                return this[DictionaryElementName] as DictionaryElementCollection;
            }
        }

        public object CoerceValue()
        {
            if (List.ElementInformation.IsPresent)
                return List;
            if (Dictionary.ElementInformation.IsPresent)
                return Dictionary;
            return Value;
        }
    }
}
