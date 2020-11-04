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


using System.Runtime.Serialization;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "search_item", Namespace = "")]
    public class SearchItemWrapper
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 3)]
        public EntityType EntityType { get; set; }

        [DataMember(Order = 5)]
        public string Title { get; set; }

        [DataMember(Order = 10)]
        public string Description { get; set; }

        [DataMember(Order = 20)]
        public ApiDateTime Created { get; set; }


        private SearchItemWrapper()
        {
        }

        public SearchItemWrapper(SearchItem searchItem)
        {
            Id = searchItem.ID;
            Title = searchItem.Title;
            EntityType = searchItem.EntityType;
            Created = (ApiDateTime) searchItem.CreateOn;
            Description = searchItem.Description;
        }


        public static SearchItemWrapper GetSample()
        {
            return new SearchItemWrapper
                       {
                           Id = "345",
                           EntityType = EntityType.Project,
                           Title = "Sample title",
                           Description = "Sample desription",
                           Created = ApiDateTime.GetSample(),
                       };
        }
    }
}
