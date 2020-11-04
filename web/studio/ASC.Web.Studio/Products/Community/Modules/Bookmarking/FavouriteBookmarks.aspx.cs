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
using ASC.Web.Community.Bookmarking.Util;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;
using ASC.Bookmarking.Common;
using ASC.Bookmarking;

namespace ASC.Web.Community.Bookmarking
{
    public partial class FavouriteBookmarks : BookmarkingBasePage
    {
        protected string PageTitle { get; private set; }

        protected override void PageLoad()
        {
            BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.Favourites);

            var c = LoadControl(BookmarkUserControlPath.BookmarkingUserControlPath);

            BookmarkingPageContent.Controls.Add(c);

            PageTitle = BookmarkingUCResource.FavouritesNavigationItem;

            Title = HeaderStringHelper.GetPageTitle(PageTitle);
        }
    }
}