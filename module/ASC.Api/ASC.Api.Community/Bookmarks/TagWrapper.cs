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


using System.Runtime.Serialization;

using ASC.Bookmarking.Pojo;

namespace ASC.Api.Bookmarks
{
    [DataContract(Name = "tag", Namespace = "")]
    public class TagWrapper
    {

        public TagWrapper(Tag tagStat)
        {
            Name = tagStat.Name;
            Count = tagStat.Populatiry;
        }

        private TagWrapper()
        {
        }

        ///<example>Sample tag</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        ///<example type="int">10</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public long Count { get; set; }

        public static TagWrapper GetSample()
        {
            return new TagWrapper
            {
                Count = 10,
                Name = "Sample tag"
            };
        }
    }
}