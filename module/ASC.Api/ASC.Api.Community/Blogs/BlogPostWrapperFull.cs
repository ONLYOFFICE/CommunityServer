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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Blogs.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Blogs
{
    [DataContract(Name = "post", Namespace = "")]
    public class BlogPostWrapperFull : IApiSortableDate
    {
        public BlogPostWrapperFull(Post post)
        {
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(post.UserID));
            Updated = Created = (ApiDateTime)post.Datetime;
            Id = post.ID;
            Tags = post.TagList.Select(x => x.Content).ToList();
            Title = post.Title;
            Text = post.Content;
        }

        private BlogPostWrapperFull()
        {

        }

        ///<example>Post text</example>
        ///<order>100</order>
        [DataMember(Order = 100)]
        public string Text { get; set; }

        ///<example>Example post</example>
        ///<order>5</order>
        [DataMember(Order = 5)]
        public string Title { get; set; }

        ///<example>2020-12-06T07:36:14.7981820Z</example>
        ///<order></order>
        [DataMember(Order = 6)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-06T07:36:14.7981820Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Updated { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        ///<example>Tag1,Tag2</example>
        ///<order>11</order>
        ///<collection split=",">list</collection>
        [DataMember(Order = 11)]
        protected List<string> Tags { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>9</order>
        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        public static BlogPostWrapperFull GetSample()
        {
            return new BlogPostWrapperFull
            {
                CreatedBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Updated = ApiDateTime.GetSample(),
                Id = Guid.Empty,
                Text = "Post text",
                Tags = new List<string> { "Tag1", "Tag2" },
                Title = "Example post"
            };
        }
    }
}