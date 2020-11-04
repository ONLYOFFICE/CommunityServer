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


using System.Collections.Generic;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage
{
    public class DataList : Dictionary<string, string>
    {
        public DataList(ModuleConfigurationElement config)
        {
            Add(string.Empty, config.Data);
            foreach (DomainConfigurationElement domain in config.Domains)
            {
                Add(domain.Name, domain.Data);
            }
        }

        public string GetData(string name)
        {
            if (ContainsKey(name))
            {
                return this[name] ?? string.Empty;
            }
            return string.Empty;
        }
    }
}