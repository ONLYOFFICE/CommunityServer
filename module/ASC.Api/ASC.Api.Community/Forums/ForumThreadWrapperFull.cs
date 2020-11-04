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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "thread", Namespace = "")]
    public class ForumThreadWrapperFull : ForumThreadWrapper
    {
       
        [DataMember(Order = 100)]
        public List<ForumTopicWrapper> Topics { get; set; }

        public ForumThreadWrapperFull(Thread thread, IEnumerable<Topic> topics):base(thread)
        {
            Topics = topics.Where(x=>x.IsApproved).Select(x => new ForumTopicWrapper(x)).ToList();
        }

        protected ForumThreadWrapperFull()
        {
        }

        public static new ForumThreadWrapperFull GetSample()
        {
            return new ForumThreadWrapperFull()
            {
                Created = ApiDateTime.GetSample(),
                Updated = ApiDateTime.GetSample(),
                Description = "Sample thread",
                Id = 10,
                UpdatedBy = EmployeeWraper.GetSample(),
                RecentTopicId = 1234,
                RecentTopicTitle = "Sample topic",
                Title = "The Thread",
                Topics = new List<ForumTopicWrapper>{ForumTopicWrapper.GetSample()}
            };
        }
    }
}