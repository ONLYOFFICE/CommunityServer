/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
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

                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("forums_icon.png", ForumManager.Settings.ModuleID),
                        Header = ForumResource.EmptyScreenForumCaption,
                        Describe = ForumResource.EmptyScreenForumText,
                        ButtonHTML = ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null) ? String.Format("<a class='link underline blue plus' href='NewForum.aspx'>{0}</a>", ForumResource.EmptyScreenForumLink) : String.Empty
                    };
                forumListHolder.Controls.Add(emptyScreenControl);
            }

            Title = HeaderStringHelper.GetPageTitle((Master as ForumMasterPage).CurrentPageCaption ?? ForumResource.AddonName);
        }
    }
}