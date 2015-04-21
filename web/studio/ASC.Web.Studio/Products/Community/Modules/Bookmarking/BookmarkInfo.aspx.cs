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