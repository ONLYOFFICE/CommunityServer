/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Web;
using ASC.Web.Studio;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.Community.Product;
using ASC.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Resources;
using ASC.Bookmarking;

namespace ASC.Web.Community.Bookmarking.Util
{
    public abstract class BookmarkingBasePage : MainPage
    {
        protected BookmarkingServiceHelper ServiceHelper;

        /// <summary>
        /// Page_Load of the Page Controller pattern.
        /// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnpatterns/html/ImpPageController.asp
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            BookmarkingBusinessConstants.CommunityProductID = CommunityProduct.ID;

            Page.RegisterStyle("~/products/community/modules/bookmarking/app_themes/default/css/bookmarkingstyle.css");
            Page.RegisterBodyScripts("~/products/community/modules/bookmarking/js/bookmarking.js",
                "~/js/asc/plugins/tagsautocompletebox.js");

            ServiceHelper = BookmarkingServiceHelper.GetCurrentInstanse();

            PageLoad();
        }

        protected abstract void PageLoad();

        protected virtual void InitBreadcrumbs(string pageTitle)
        {
            var searchText = ServiceHelper.GetSearchText();
            if (!String.IsNullOrEmpty(searchText))
            {
                BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.SearchBookmarks);
                BookmarkingServiceHelper.UpdateCurrentInstanse(ServiceHelper);
                return;
            }

            searchText = ServiceHelper.GetSearchTag();
            if (!String.IsNullOrEmpty(searchText))
            {
                BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.SearchByTag);
                BookmarkingServiceHelper.UpdateCurrentInstanse(ServiceHelper);
                var searchResults = String.Format("{0} {1}", BookmarkingUCResource.TagBookmarks, searchText);
                Title = searchResults;
            }
        }
    }
}