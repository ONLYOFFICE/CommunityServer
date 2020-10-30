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


using ASC.ElasticSearch;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Configuration;

namespace ASC.Web.Projects.Core.Search
{
    public sealed class DiscussionsWrapper : Wrapper
    {
        [Column("title", 1)]
        public string Title { get; set; }

        [Column("content", 2, charFilter: CharFilter.html | CharFilter.io)]
        public string Description { get; set; }

        protected override string Table { get { return "projects_messages"; } }

        public static implicit operator DiscussionsWrapper(Message message)
        {
            return ProductEntryPoint.Mapper.Map<DiscussionsWrapper>(message);
        }
    }
}