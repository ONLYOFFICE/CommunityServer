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
using ASC.Blogs.Core;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Core;
using ASC.Blogs.Core.Security;

namespace ASC.Web.Community.Blogs
{    
    public class BlogsShortcutProvider : IShortcutProvider
    {
        public static string GetCreateContentPageUrl()
        {
            if (SecurityContext.CheckPermissions(new PersonalBlogSecObject(CoreContext.UserManager.GetUsers(
                    SecurityContext.CurrentAccount.ID)), Constants.Action_AddPost))

                return VirtualPathUtility.ToAbsolute(Constants.BaseVirtualPath + "AddBlog.aspx");

            return null;
        }

        public string GetAbsoluteWebPathForShortcut(Guid shortcutID, string currentUrl)
        {
            return "";
        }

        public bool CheckPermissions(Guid shortcutID, string currentUrl)
        {
            if (shortcutID.Equals(new Guid("98DB8D88-EDF2-4f82-B3AF-B95E87E3EE5C")) || 
                shortcutID.Equals(new Guid("20673DF0-665E-4fc8-9B44-D48B2A783508")))
            {
                return SecurityContext.CheckPermissions(new PersonalBlogSecObject(CoreContext.UserManager.GetUsers(
                    SecurityContext.CurrentAccount.ID)), Constants.Action_AddPost);
            }            
            
            return false;
        }
    }
}
