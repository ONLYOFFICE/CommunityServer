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


using System;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Specific;
using ASC.Web.Community.News.Code;

namespace ASC.Api.Events
{
    [DataContract(Name = "event", Namespace = "")]
    public class EventWrapper : IApiSortableDate
    {
        ///<example type="int">10</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public long Id { get; set; }

        ///<example>Manager</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string Title { get; set; }

        ///<example>2020-12-07T13:56:02.2729203Z</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-07T13:56:02.2729203Z</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Updated
        { get; set; }

        ///<example type="int">1</example>
        ///<order>4</order>
        [DataMember(Order = 4)]
        public FeedType Type { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>9</order>
        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        public EventWrapper(ASC.Web.Community.News.Code.Feed feed)
        {
            Id = feed.Id;
            Title = feed.Caption;
            Updated = Created = (ApiDateTime)feed.Date;
            Type = feed.FeedType;
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(new Guid(feed.Creator)));
        }

        protected EventWrapper()
        {

        }

        public static EventWrapper GetSample()
        {
            return new EventWrapper()
            {
                CreatedBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Id = 10,
                Type = FeedType.News,
                Title = "Sample news",
                Updated = ApiDateTime.GetSample()
            };
        }

    }
}