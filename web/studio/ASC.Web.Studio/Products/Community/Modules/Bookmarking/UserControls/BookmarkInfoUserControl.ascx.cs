/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Web.Studio.UserControls.Common.ViewSwitcher;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;

namespace ASC.Web.UserControls.Bookmarking
{
    public partial class BookmarkInfoUserControl : BookmarkInfoBase
    {

        public override void InitUserControl()
        {
            var singleBookmarkUserControl = LoadControl(BookmarkUserControlPath.SingleBookmarkUserControlPath) as BookmarkInfoBase;
            singleBookmarkUserControl.Bookmark = Bookmark;
            singleBookmarkUserControl.UserBookmark = UserBookmark;
            BookmarkInfoHolder.Controls.Add(singleBookmarkUserControl);

            var sortControl = new ViewSwitcher();
            sortControl.TabItems.Add(new ViewSwitcherTabItem
                {
                    TabName = BookmarkingUCResource.BookmarkedBy,
                    DivID = "BookmarkedByPanel",
                    IsSelected = ServiceHelper.SelectedTab == 1,
                    SkipRender = true
                });

            sortControl.TabItems.Add(new ViewSwitcherTabItem
                {
                    TabName = BookmarkingUCResource.Comments + String.Format(" ({0})", CommentsCount),
                    DivID = "BookmarkCommentsPanel",
                    IsSelected = ServiceHelper.SelectedTab == 0,
                    SkipRender = true
                });

            BookmarkInfoTabsContainer.Controls.Add(sortControl);

            //Init comments
            using (var c = LoadControl(BookmarkUserControlPath.CommentsUserControlPath) as CommentsUserControl)
            {
                c.BookmarkID = Bookmark.ID;
                c.BookmarkComments = ServiceHelper.GetBookmarkComments(Bookmark);
                c.InitComments();
                CommentsHolder.Controls.Add(c);
            }
            if (Bookmark != null)
            {
                var userBookmarks = Bookmark.UserBookmarks;
                if (userBookmarks != null && userBookmarks.Count > 0)
                {
                    //Init added by list
                    AddedByRepeater.DataSource = userBookmarks;
                    AddedByRepeater.DataBind();
                }
            }
        }

        public string GetAddedByTableItem(bool TintFlag, string UserImage, string UserPageLink, string UserBookmarkDescription, string DateAddedAsString, object userID)
        {
            return new BookmarkAddedByUserContorl().GetAddedByTableItem(TintFlag, UserImage, UserPageLink, UserBookmarkDescription, DateAddedAsString, userID);
        }
    }
}