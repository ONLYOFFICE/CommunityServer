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
    [DataContract(Name = "comment", Namespace = "")]
    public class EventCommentWrapper : IApiSortableDate
    {
        public EventCommentWrapper(FeedComment comment)
        {
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(new Guid(comment.Creator)));
            Updated = Created = (ApiDateTime)comment.Date;

            Id = comment.Id;
            Text = comment.Comment;
            ParentId = comment.ParentId;
        }

        private EventCommentWrapper()
        {
        }

        ///<example>comment text</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string Text { get; set; }

        ///<example>2020-12-07T13:56:02.2899133Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-07T13:56:02.2899133Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Updated
        {
            get;
            set;
        }

        ///<example type="int">10</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public long Id { get; set; }

        ///<example type="int">123</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public long ParentId { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>9</order>
        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        public static EventCommentWrapper GetSample()
        {
            return new EventCommentWrapper
            {
                CreatedBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Id = 10,
                ParentId = 123,
                Text = "comment text",
                Updated = ApiDateTime.GetSample()
            };
        }
    }
}