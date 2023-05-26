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
using ASC.Bookmarking.Pojo;
using ASC.Specific;

namespace ASC.Api.Bookmarks
{
    [DataContract(Name = "comment", Namespace = "")]
    public class BookmarkCommentWrapper : IApiSortableDate
    {
        public BookmarkCommentWrapper(Comment comment)
        {
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(comment.UserID));
            Updated = Created = (ApiDateTime)comment.Datetime;
            Id = comment.ID;
            Text = comment.Content;
            if (!string.IsNullOrEmpty(comment.Parent))
                ParentId = new Guid(comment.Parent);
        }

        private BookmarkCommentWrapper()
        {
        }

        ///<example>comment text</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string Text { get; set; }

        ///<example>2020-12-06T07:36:14.8151911Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-06T07:36:14.8151911Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Updated { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>2</order>
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public Guid ParentId { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>9</order>
        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        public static BookmarkCommentWrapper GetSample()
        {
            return new BookmarkCommentWrapper
            {
                CreatedBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Id = Guid.Empty,
                Text = "comment text",
                ParentId = Guid.Empty,
                Updated = ApiDateTime.GetSample()
            };
        }
    }
}