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


#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Blogs.Core.Domain;
using ASC.Specific;

#endregion

namespace ASC.Api.Blogs
{
    [DataContract(Name = "post", Namespace = "")]
    public class BlogPostWrapperSummary : IApiSortableDate
    {
        public BlogPostWrapperSummary(Post post)
        {
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(post.UserID));
            Created = (ApiDateTime)post.Datetime;
            Updated = (ApiDateTime)post.Updated;

            Id = post.ID;
            Tags = post.TagList.Select(x => x.Content).ToList();
            Title = post.Title;
            Preview = post.GetPreviewText(100);

            BlogTitle = post.BlogTitle;
        }

        internal BlogPostWrapperSummary()
        {

        }

        ///<example>Preview post</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string Preview { get; set; }

        ///<example>Example post</example>
        ///<order>5</order>
        [DataMember(Order = 5)]
        public string Title { get; set; }

        ///<example>2020-12-03T21:36:12.0774137Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Created { get; set; }

        ///<example>2020-12-03T21:36:12.0774137Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime Updated
        { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        ///<example>Tag1,Tag1</example>
        ///<order>11</order>
        ///<collection split=",">list</collection>
        [DataMember(Order = 11)]
        public List<string> Tags { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>9</order>
        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        ///<example>Blog Title</example>
        ///<order>12</order>
        [DataMember(Order = 12)]
        public string BlogTitle { get; set; }

        public static BlogPostWrapperSummary GetSample()
        {
            return new BlogPostWrapperSummary
            {
                CreatedBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Id = Guid.Empty,
                Preview = "Preview post",
                Tags = new List<string> { "Tag1", "Tag2" },
                Title = "Example post",
                Updated = ApiDateTime.GetSample()
            };
        }
    }
}
