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

using ASC.Api.Employee;
using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "topic", Namespace = "")]
    public class ForumTopicWrapper : IApiSortableDate
    {
        ///<example type="int">10</example>
        /// <order>1</order>
        [DataMember(Order = 1)]
        public int Id { get; set; }

        ///<example>Sample topic</example>
        /// <order>2</order>
        [DataMember(Order = 2)]
        public string Title { get; set; }

        ///<example>2020-12-07T13:56:02.3259212Z</example>
        /// <order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Created { get; set; }

        private ApiDateTime _updated;

        ///<example>2020-12-07T13:56:02.3259212Z</example>
        /// <order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Updated
        {
            get { return _updated >= Created ? _updated : Created; }
            set { _updated = value; }
        }

        ///<example>This is sample post</example>
        /// <order>8</order>
        [DataMember(Order = 8)]
        public string Text { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        /// <order>9</order>
        [DataMember(Order = 9)]
        public EmployeeWraper UpdatedBy { get; set; }

        ///<example>null</example>
        /// <order>10</order>
        [DataMember(Order = 10)]
        public string ThreadTitile { get; set; }


        public ForumTopicWrapper(Topic topic)
        {
            Id = topic.ID;
            Title = topic.Title;
            Created = (ApiDateTime)topic.CreateDate;
            Text = ASC.Common.Utils.HtmlUtil.GetText(topic.RecentPostText, 160);
            Updated = (ApiDateTime)topic.RecentPostCreateDate;
            UpdatedBy = EmployeeWraper.Get(topic.RecentPostAuthorID);
            Status = topic.Status;
            Type = topic.Type;
            Tags = topic.Tags.Where(x => x.IsApproved).Select(x => x.Name).ToList();
            ThreadTitile = topic.ThreadTitle;
        }

        protected ForumTopicWrapper()
        {

        }

        ///<example type="int">0</example>
        /// <order>30</order>
        [DataMember(Order = 30)]
        public TopicStatus Status { get; set; }

        ///<example type="int">0</example>
        /// <order>30</order>
        [DataMember(Order = 30)]
        public TopicType Type { get; set; }

        ///<example>Tag1,Tag2</example>
        /// <order>100</order>
        /// <collection split=",">100</collection>
        [DataMember(Order = 100)]
        public List<string> Tags { get; set; }

        public static ForumTopicWrapper GetSample()
        {
            return new ForumTopicWrapper()
            {
                Created = ApiDateTime.GetSample(),
                Updated = ApiDateTime.GetSample(),
                Id = 10,
                UpdatedBy = EmployeeWraper.GetSample(),
                Text = "This is sample post",
                Status = TopicStatus.Sticky,
                Tags = new List<string> { "Tag1", "Tag2" },
                Title = "Sample topic",
                Type = TopicType.Informational
            };
        }
    }
}