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
            Updated = (ApiDateTime) post.Updated;

            Id = post.ID;
            Tags = post.TagList.Select(x => x.Content).ToList();
            Title = post.Title;
            Preview = post.GetPreviewText(100);

            BlogTitle = post.BlogTitle;
        }

        internal BlogPostWrapperSummary()
        {

        }

        [DataMember(Order = 10)]
        public string Preview { get; set; }

        [DataMember(Order = 5)]
        public string Title { get; set; }

        [DataMember(Order = 6)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 6)]
        public ApiDateTime Updated
        { get; set; }

        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 11)]
        public List<string> Tags { get; set; }

        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

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
                           Tags = new List<string> {"Tag1", "Tag2"},
                           Title = "Example post",
                           Updated = ApiDateTime.GetSample()
                       };
        }
    }
}
