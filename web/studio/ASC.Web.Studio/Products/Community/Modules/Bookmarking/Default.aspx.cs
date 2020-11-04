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
using ASC.Web.Community.Bookmarking.Resources;
using ASC.Web.Community.Bookmarking.Util;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;

namespace ASC.Web.Community.Bookmarking
{
    public partial class Bookmarking : BookmarkingBasePage
    {
        protected override void PageLoad()
        {
            BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.AllBookmarks);

            var pageTitle = BookmarkingResource.PageTitle;
            BookmarkingPageContent.Controls.Add(LoadControl(BookmarkUserControlPath.BookmarkingUserControlPath));
            BookmarkingPagingContent.Controls.Add(LoadControl(BookmarkUserControlPath.BookmarkingPagingUserControlPath));
            InitBreadcrumbs(pageTitle);
            Title = HeaderStringHelper.GetPageTitle(pageTitle);
        }
    }
}