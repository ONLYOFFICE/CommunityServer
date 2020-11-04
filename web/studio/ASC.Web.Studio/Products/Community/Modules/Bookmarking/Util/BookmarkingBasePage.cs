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

            Page.RegisterStyle("~/Products/Community/Modules/Bookmarking/App_Themes/default/css/bookmarkingstyle.css")
                .RegisterBodyScripts("~/Products/Community/Modules/Bookmarking/js/bookmarking.js",
                "~/Products/Community/js/tagsautocompletebox.js");

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