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
using System.IO;

namespace ASC.Data.Storage.DiscStorage
{
    internal class MappedPath
    {
        public string PhysicalPath { get; set; }


        private MappedPath()
        {
        }

        public MappedPath(string tenant, bool appendTenant, string ppath, IDictionary<string, string> storageConfig)
        {
            tenant = tenant.Trim('/');

            ppath = PathUtils.ResolvePhysicalPath(ppath, storageConfig);
            PhysicalPath = ppath.IndexOf('{') == -1 && appendTenant ? Path.Combine(ppath, tenant) : string.Format(ppath, tenant);
        }

        public MappedPath AppendDomain(string domain)
        {
            domain = domain.Replace('.', '_'); //Domain prep. Remove dots
            return new MappedPath
                {
                    PhysicalPath = Path.Combine(PhysicalPath, PathUtils.Normalize(domain, true)),
                };
        }
    }
}