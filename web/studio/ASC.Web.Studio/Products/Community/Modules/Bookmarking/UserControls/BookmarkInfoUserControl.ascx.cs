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