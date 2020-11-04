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
using System.Web;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Resources;
using ASC.Core;
using ASC.Web.Community.Product;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Community.Blogs
{
    public class BlogsSearchHandler : BaseSearchHandlerEx
    {
        public override SearchResultItem[] Search(string text)
        {
            var posts = BasePage.GetEngine().SearchPosts(text, new PagingQuery());
            var result = new List<SearchResultItem>(posts.Count);
            result.AddRange(posts.Select(post => new SearchResultItem
                {
                    Description = BlogsResource.Blogs + ", " + DisplayUserSettings.GetFullUserName(CoreContext.UserManager.GetUsers(post.UserID), false) + ", " + post.Datetime.ToLongDateString(),
                    Name = post.Title,
                    URL = VirtualPathUtility.ToAbsolute(Constants.BaseVirtualPath) + "ViewBlog.aspx?blogid=" + post.ID.ToString(),
                    Date = post.Datetime
                }));

            return result.ToArray();
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions {ImageFileName = "blog_add.png", PartID = Constants.ModuleID}; }
        }

        public override string SearchName
        {
            get { return BlogsResource.SearchDefaultString; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }

        public override Guid ModuleID
        {
            get { return Constants.ModuleID; }
        }

        public override IItemControl Control
        {
            get { return new ResultsView(); }
        }
    }
}