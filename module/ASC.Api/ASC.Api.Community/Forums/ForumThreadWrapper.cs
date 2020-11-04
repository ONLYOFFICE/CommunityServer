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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "thread", Namespace = "")]
    public class ForumThreadWrapper : IApiSortableDate
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Title { get; set; }

        [DataMember(Order = 10)]
        public string Description { get; set; }

        [DataMember(Order = 20)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 21)]
        public ApiDateTime Updated { get; set; }

        [DataMember(Order = 30)]
        public int RecentTopicId { get; set; }

        [DataMember(Order = 30)]
        public string RecentTopicTitle { get; set; }

        [DataMember(Order = 9)]
        public EmployeeWraper UpdatedBy { get; set; }

        public ForumThreadWrapper(Thread thread)
        {
            Id = thread.ID;
            Title = thread.Title;
            Description = thread.Description;
            Created = (ApiDateTime)thread.RecentTopicCreateDate;
            Updated = (ApiDateTime)thread.RecentPostCreateDate;
            RecentTopicTitle = thread.RecentTopicTitle;
            UpdatedBy = EmployeeWraper.Get(thread.RecentPosterID);
        }

        protected ForumThreadWrapper()
        {
        }

        public static ForumThreadWrapper GetSample()
        {
            return new ForumThreadWrapper()
                       {
                           Created = ApiDateTime.GetSample(),
                           Updated = ApiDateTime.GetSample(),
                           Description = "Sample thread",
                           Id = 10,
                           UpdatedBy = EmployeeWraper.GetSample(),
                           RecentTopicId = 1234,
                           RecentTopicTitle = "Sample topic",
                           Title = "The Thread"
                       };
        }
    }
}