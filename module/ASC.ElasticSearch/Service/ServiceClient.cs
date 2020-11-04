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
using ASC.Common.Module;
using ASC.ElasticSearch.Core;

namespace ASC.ElasticSearch.Service
{
    public class ServiceClient : BaseWcfClient<IService>, IService
    {
        public bool Support(string table)
        {
            return Channel.Support(table);
        }

        public void ReIndex(List<string> tables, int tenant)
        {
            Channel.ReIndex(tables, tenant);
        }

        public State GetState()
        {
            return Channel.GetState();
        }
    }
}
