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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "category", Namespace = "")]
    public class ForumCategoryWrapper : IApiSortableDate
    {
        ///<example type="int">0</example>
        ///<order>0</order>
        [DataMember(Order = 0)]
        public int Id { get; set; }

        ///<example>Sample title</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public string Title { get; set; }

        ///<example>2020-12-07T13:56:02.3459179Z</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-07T13:56:02.3459179Z</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Updated { get; set; }

        ///<example>Sample category</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string Description { get; set; }

        public ForumCategoryWrapper(ThreadCategory category, IEnumerable<Thread> threads)
        {
            Id = category.ID;

            Title = category.Title;
            Updated = Created = (ApiDateTime)category.CreateDate;

            Description = category.Description;
            Threads = (from thread in threads where thread.IsApproved select new ForumThreadWrapper(thread)).ToList();
        }

        private ForumCategoryWrapper()
        {
        }

        ///<type>ASC.Api.Forums.ForumThreadWrapper, ASC.Api.Community</type>
        ///<order>100</order>
        ///<collection>list</collection>
        [DataMember(Order = 100)]
        public List<ForumThreadWrapper> Threads { get; set; }

        public static ForumCategoryWrapper GetSample()
        {
            return new ForumCategoryWrapper()
            {
                Id = 0,
                Created = ApiDateTime.GetSample(),
                Description = "Sample category",
                Title = "Sample title",
                Updated = ApiDateTime.GetSample(),
                Threads = new List<ForumThreadWrapper> { ForumThreadWrapper.GetSample() }
            };
        }
    }
}