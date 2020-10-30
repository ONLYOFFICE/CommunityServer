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


using System.Configuration;

namespace ASC.Common.DependencyInjection
{
    public class ComponentElement : ConfigurationElement
    {
        private const string TypeAttributeName = "type";
        private const string ServiceAttributeName = "service";
        private const string ParametersElementName = "parameters";
        private const string NameAttributeName = "name";
        private const string InstanceScopeAttributeName = "instance-scope";
        private const string InjectPropertiesAttributeName = "inject-properties";
        internal const string Key = "type";

        [ConfigurationProperty(TypeAttributeName, IsRequired = true)]
        public string Type { get { return (string)this[TypeAttributeName]; } }

        [ConfigurationProperty(ServiceAttributeName, IsRequired = false)]
        public string Service { get { return (string)this[ServiceAttributeName]; } }

        [ConfigurationProperty(NameAttributeName, IsRequired = false)]
        public string Name { get { return (string)this[NameAttributeName]; } }

        [ConfigurationProperty(InstanceScopeAttributeName, IsRequired = false)]
        public string InstanceScope { get { return (string)this[InstanceScopeAttributeName]; } }

        [ConfigurationProperty(InjectPropertiesAttributeName, IsRequired = false)]
        public string InjectProperties { get { return (string)this[InjectPropertiesAttributeName]; } }

        [ConfigurationProperty(ParametersElementName, IsRequired = false)]
        public ParameterElementCollection Parameters { get { return (ParameterElementCollection)this[ParametersElementName]; } }
    }
}
