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


using ASC.Api.Interfaces;

namespace ASC.Specific
{
    public class AscApiConfiguration : IApiConfiguration
    {
        public const uint DefaultItemsPerPage = 25;

        public string ApiPrefix { get; set; }
        public string ApiVersion { get; set; }
        public char ApiSeparator { get; set; }

        public AscApiConfiguration(string version)
            : this(string.Empty, version, DefaultItemsPerPage)
        {
        }

        public AscApiConfiguration(string prefix, string version)
            : this(prefix, version, DefaultItemsPerPage)
        {
        }

        public AscApiConfiguration(string prefix, string version, uint maxPage)
        {
            ApiSeparator = '/';
            ApiPrefix = prefix??string.Empty;
            ApiVersion = version;
            ItemsPerPage = maxPage;
        }

        private string basePath;
        public string GetBasePath()
        {
            return basePath ?? (basePath = (ApiPrefix + ApiSeparator + ApiVersion + ApiSeparator).TrimStart('/', '~'));
        }

        public uint ItemsPerPage { get; private set; }
    }
}