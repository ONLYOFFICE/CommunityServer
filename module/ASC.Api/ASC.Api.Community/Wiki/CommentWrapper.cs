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
using System.Text;
using ASC.Specific;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Api.Employee;

namespace ASC.Api.Wiki.Wrappers
{
    [DataContract(Name = "comment", Namespace = "")]
    public class CommentWrapper
    {
        [DataMember(Order = 0)]
        public Guid Id { get; set; }

        [DataMember(Order = 1)]
        public Guid ParentId { get; set; }

        [DataMember(Order = 2)]
        public string Page { get; set; }

        [DataMember(Order = 3)]
        public string Content { get; set; }

        [DataMember(Order = 4)]
        public EmployeeWraper Author { get; set; }

        [DataMember(Order = 4)]
        public ApiDateTime LastModified { get; set; }

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
