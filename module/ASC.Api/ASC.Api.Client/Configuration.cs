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

namespace ASC.Api.Client
{
    public class ApiClientConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("root", DefaultValue = "api/2.0")]
        public string ApiRoot
        {
            get { return (string)this["root"]; }
            set { this["root"] = value; }
        }

        [ConfigurationProperty("scheme", DefaultValue = UriScheme.Http)]
        public UriScheme UriScheme
        {
            get { return (UriScheme)this["scheme"]; }
            set { this["scheme"] = value; }
        }

        public static ApiClientConfiguration GetSection()
        {
            return ConfigurationManagerExtension.GetSection("apiClient") as ApiClientConfiguration ?? new ApiClientConfiguration();
        }
    }
}
