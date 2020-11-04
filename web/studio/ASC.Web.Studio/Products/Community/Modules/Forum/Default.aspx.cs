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
using ASC.Core;
using ASC.Core.Users;
using ASC.Forum;
using ASC.Web.Community.Forum.Resources;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public partial class Default : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.Default);

            List<ThreadCategory> categories;
            List<Thread> threads;

            ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, true, out categories, out threads);

            if (0 < categories.Count)
            {
                var categoryListControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/ThreadCategoryListControl.ascx") as UserControls.Forum.ThreadCategoryListControl;
                categoryListControl.Categories = categories;
                categoryListControl.Threads = threads;
                categoryListControl.SettingsID = ForumManager.Settings.ID;
                forumListHolder.Controls.Add(categoryListControl);

                (Master as ForumMasterPage).CurrentPageCaption = ForumResource.ForumsBreadCrumbs;
            }
            else
            {
                _headerHolder.Visible = false;

                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("forums_icon.png", ForumManager.Settings.ModuleID),
                        Header = ForumResource.EmptyScreenForumCaption,
                        Describe = currentUser.IsVisitor() ? ForumResource.EmptyScreenForumTextVisitor : ForumResource.EmptyScreenForumText,
                        ButtonHTML = ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null) ? String.Format("<a class='link underline blue plus' href='NewForum.aspx'>{0}</a>", ForumResource.EmptyScreenForumLink) : String.Empty
                    };
                forumListHolder.Controls.Add(emptyScreenControl);
            }

            Title = HeaderStringHelper.GetPageTitle((Master as ForumMasterPage).CurrentPageCaption ?? ForumResource.AddonName);
        }
    }
}