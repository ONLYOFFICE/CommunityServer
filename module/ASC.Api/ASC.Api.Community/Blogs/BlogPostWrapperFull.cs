/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
    public class BlogPostWrapperFull:IApiSortableDate
    {
        public BlogPostWrapperFull(Post post)
        {
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(post.UserID));
            Updated = Created = (ApiDateTime) post.Datetime;
            Id = post.ID;
            Tags = post.TagList.Select(x=>x.Content).ToList();
            Title = post.Title;
            Text = post.Content;
        }

        private BlogPostWrapperFull()
        {
            
        }

        [DataMember(Order = 100)]
        public string Text { get; set; }

        [DataMember(Order = 5)]
        public string Title { get; set; }

        [DataMember(Order = 6)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 6)]
        public ApiDateTime Updated { get; set; }

        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 11)]
        protected List<string> Tags { get; set; }

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