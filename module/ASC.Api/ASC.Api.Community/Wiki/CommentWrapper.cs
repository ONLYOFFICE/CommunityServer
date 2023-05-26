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
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Api.Wiki.Wrappers
{
    [DataContract(Name = "comment", Namespace = "")]
    public class CommentWrapper
    {
        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>0</order>
        [DataMember(Order = 0)]
        public Guid Id { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public Guid ParentId { get; set; }

        ///<example>Some page</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string Page { get; set; }

        ///<example>Comment content</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public string Content { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>4</order>
        [DataMember(Order = 4)]
        public EmployeeWraper Author { get; set; }

        ///<example>2020-12-08T17:37:03.3304954Z</example>
        ///<order>4</order>
        [DataMember(Order = 4)]
        public ApiDateTime LastModified { get; set; }

        ///<example>false</example>
        ///<order>5</order>
        [DataMember(Order = 5)]
        public bool Inactive { get; set; }

        public CommentWrapper(Comment comment)
        {
            Id = comment.Id;
            ParentId = comment.ParentId;
            Page = comment.PageName;
            Content = comment.Body;
            Author = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(comment.UserId));
            LastModified = (ApiDateTime)comment.Date;
            Inactive = comment.Inactive;
        }

        public CommentWrapper()
        {

        }

        public static CommentWrapper GetSample()
        {
            return new CommentWrapper
            {
                Author = EmployeeWraper.GetSample(),
                Content = "Comment content",
                Id = Guid.Empty,
                Page = "Some page",
                Inactive = false,
                LastModified = ApiDateTime.GetSample(),
                ParentId = Guid.Empty
            };
        }
    }
}
