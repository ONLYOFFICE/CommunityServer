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


using ASC.Bookmarking;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Pojo;
using ASC.Web.Community.Bookmarking.UserControls;
using ASC.Web.Community.Bookmarking.Util;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;
using System.Collections.Generic;
using System.Web;

namespace ASC.Web.Community.Bookmarking
{
    public partial class BookmarkInfo : BookmarkingBasePage
    {
        protected string BookmarkTitle { get; set; }

        protected override void PageLoad()
        {
            BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.SelectedBookmark);

            var c = LoadControl(BookmarkUserControlPath.BookmarkInfoUserControlPath) as BookmarkInfoUserControl;
            InitBookmarkInfoUserControl(c);

            var pageTitle = BookmarkingUCResource.BookmarksNavigationItem;

            var bookmarks = new List<Bookmark> { c.Bookmark };

            var bookmarkingUserControl = LoadControl(BookmarkUserControlPath.BookmarkingUserControlPath) as BookmarkingUserControl;
            bookmarkingUserControl.Bookmarks = bookmarks;

            var b = LoadControl(BookmarkUserControlPath.BookmarkHeaderPageControlPath) as BookmarkHeaderPageControl;
            b.Title = ServiceHelper.BookmarkToAdd.Name;
            b.BookmarkID = ServiceHelper.BookmarkToAdd.ID;
            b.Author = ServiceHelper.BookmarkToAdd.UserCreatorID;
            BookmarkingHeaderPageContent.Controls.Add(b);

            BookmarkingPageContent.Controls.Add(bookmarkingUserControl);
            BookmarkingPageContent.Controls.Add(c);


            InitBreadcrumbs(pageTitle);
            Title = HeaderStringHelper.GetPageTitle(pageTitle);
        }

        #region Init Bookmark

        private void InitBookmarkInfoUserControl(BookmarkInfoUserControl c)
        {
            var b = ServiceHelper.GetBookmarkWithUserBookmarks();

            if (b == null)
            {
                var url = Request.QueryString[BookmarkingRequestConstants.UrlGetRequest];

                b = ServiceHelper.GetBookmarkWithUserBookmarks(url, false) ?? ServiceHelper.GetBookmarkWithUserBookmarks(url, true);

                if (b == null)
                {

                    var redirectUrl = BookmarkingRequestConstants.CreateBookmarkPageName;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url = BookmarkingServiceHelper.UpdateBookmarkInfoUrl(url);
                        redirectUrl += string.Format("?{0}={1}", BookmarkingRequestConstants.UrlGetRequest, url);
                    }

                    Response.Redirect(redirectUrl);
                }
            }
            c.Bookmark = b;
            c.UserBookmark = ServiceHelper.GetCurrentUserBookmark(b.UserBookmarks);
        }

        #endregion

        protected override void InitBreadcrumbs(string pageTitle)
        {
            //Get text from the search input
            var bookmarkName = string.Empty;
            if (ServiceHelper.BookmarkToAdd != null)
            {
                bookmarkName = ServiceHelper.BookmarkToAdd.Name;
            }
            BookmarkTitle = bookmarkName;
        }
    }
}