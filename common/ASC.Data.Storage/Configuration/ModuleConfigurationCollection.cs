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


using System.Configuration;

namespace ASC.Data.Storage.Configuration
{
    public class ModuleConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ModuleConfigurationElement();
        }

        internal ModuleConfigurationElement GetModuleElement(string name)
        {
            return (ModuleConfigurationElement)BaseGet(name);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ModuleConfigurationElement)element).Name;
        }

        public void Add(ModuleConfigurationElement element)
        {
            BaseAdd(element);
        }

        internal ModuleConfigurationElement GetModuleElement(int index)
        {
            return (ModuleConfigurationElement)BaseGet(index);
        }
    }
}