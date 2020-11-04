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
using System.Web;
using ASC.Blogs.Core.Resources;
using ASC.Blogs.Core.Security;
using ASC.Core;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;

namespace ASC.Web.Community.Blogs.Common
{
    public class BlogsModule : Module
    {
        public override Guid ID
        {
            get { return ASC.Blogs.Core.Constants.ModuleId; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return BlogsResource.AddonName; }
        }

        public override string Description
        {
            get { return BlogsResource.AddonDescriptionResourceKey; }
        }

        public override string StartURL
        {
            get { return ASC.Blogs.Core.Constants.BaseVirtualPath; }
        }        

        public BlogsModule()
        {
            Context = new ModuleContext
            {
                DefaultSortOrder = 1,
                SmallIconFileName = "blog_add.png",
                IconFileName = "blogiconwg.png",
                SubscriptionManager = new BlogsSubscriptionManager(),
                GetCreateContentPageAbsoluteUrl = () => CanEdit() ? VirtualPathUtility.ToAbsolute(ASC.Blogs.Core.Constants.BaseVirtualPath + "AddBlog.aspx") : null,
                SearchHandler = new BlogsSearchHandler(),
            };
        }

        private static bool CanEdit()
        {
            return CommunitySecurity.CheckPermissions(new PersonalBlogSecObject(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID)), ASC.Blogs.Core.Constants.Action_AddPost);
        }
    }
}
