/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Collections.Generic;
using ASC.Bookmarking.Pojo;
using ASC.Web.Community.Bookmarking.Util;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.Community.Bookmarking.UserControls;
using ASC.Web.UserControls.Bookmarking.Resources;

namespace ASC.Web.Community.Bookmarking
{
    public partial class BookmarkInfo : BookmarkingBasePage
    {
        protected string BookmarkTitle { get; set; }

        protected override void PageLoad()
        {
            ServiceHelper.DisplayMode = BookmarkingServiceHelper.BookmarkDisplayMode.SelectedBookmark;

            var c = LoadControl(BookmarkUserControlPath.BookmarkInfoUserControlPath) as BookmarkInfoUserControl;
            InitBookmarkInfoUserControl(c);

            var pageTitle = BookmarkingUCResource.BookmarksNavigationItem;

            var bookmarks = new List<Bookmark> { c.Bookmark };

            var bookmarkingUserControl = LoadControl(BookmarkUserControlPath.BookmarkingUserControlPath) as BookmarkingUserControl;
            bookmarkingUserControl.Bookmarks = bookmarks;

            var b = LoadControl(BookmarkUserControlPath.BookmarkHeaderPageControlPath) as BookmarkHeaderPageControl;
            b.Title = ServiceHelper.BookmarkToAdd.Name;
            BookmarkingPageContent.Controls.Add(b);

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