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
    [DataContract(Name = "topic", Namespace = "")]
    public class ForumTopicWrapperFull:ForumTopicWrapper
    {
        [DataMember(Order = 100)]
        public List<ForumTopicPostWrapper> Posts { get; set; }


        public ForumTopicWrapperFull(Topic topic,IEnumerable<Post> posts) : base(topic)
        {
            if (topic.Type==TopicType.Poll)
            {
                //TODO: Deal with polls
            }
            Posts = posts.Where(x=>x.IsApproved).Select(x => new ForumTopicPostWrapper(x)).ToList();
        }

        private ForumTopicWrapperFull()
        {

        }

        public static new ForumTopicWrapperFull GetSample()
        {
            return new ForumTopicWrapperFull()
            {
                Created = ApiDateTime.GetSample(),
                Updated = ApiDateTime.GetSample(),
                Id = 10,
                UpdatedBy = EmployeeWraper.GetSample(),
                Text = "This is sample post",
                Status = TopicStatus.Sticky,
                Tags = new List<string> { "Tag1", "Tag2" },
                Title = "Sample topic",
                Type = TopicType.Informational,
                Posts = new List<ForumTopicPostWrapper> { ForumTopicPostWrapper.GetSample()}
            };
        }
    }
}