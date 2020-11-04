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

namespace ASC.ElasticSearch.Config
{
    class ElasticSection : ConfigurationSection
    {
        [ConfigurationProperty("host", IsRequired = true, DefaultValue = "localhost")]
        public string Host
        {
            get { return (string)this["host"]; }
        }

        [ConfigurationProperty("port", IsRequired = false, DefaultValue = "9200")]
        public int Port
        {
            get { return Convert.ToInt32(this["port"]); }
        }

        [ConfigurationProperty("scheme", IsRequired = false, DefaultValue = "http")]
        public string Scheme
        {
            get { return (string)this["scheme"]; }
        }

        [ConfigurationProperty("period", IsRequired = false, DefaultValue = 1)]
        public int Period
        {
            get { return Convert.ToInt32(this["period"]); }
        }

        [ConfigurationProperty("memoryLimit", IsRequired = false, DefaultValue = 10 * 1024 * 1024L)]
        public long MemoryLimit
        {
            get { return Convert.ToInt64(this["memoryLimit"]); }
        }
    }
}
