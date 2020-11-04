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
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Common;
using ASC.Web.Community.Bookmarking.Util;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;
using System.Web;

namespace ASC.Web.Community.Bookmarking
{
    public partial class CreateBookmark : BookmarkingBasePage
    {
        protected override void PageLoad()
        {
            if (!BookmarkingPermissionsCheck.PermissionCheckCreateBookmark())
            {
                Response.Redirect(BookmarkingRequestConstants.BookmarkingPageName);
            }

            BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.CreateBookmark);

            var c = LoadControl(BookmarkUserControlPath.CreateBookmarkUserControlPath) as CreateBookmarkUserControl;
            c.IsNewBookmark = true;
            BookmarkingPageContent.Controls.Add(c);

            var url = Request.QueryString[BookmarkingRequestConstants.UrlGetRequest];
            var s = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                s = string.Format(" getBookmarkUrlInput().val(\"{0}\"); getBookmarkByUrlButtonClick(); ", url.Replace("\"", "\\\""));
            }

            var script = string.Format("showAddBookmarkPanel(); {0}", s);

            Page.RegisterInlineScript(script);

            InitBreadcrumbs(BookmarkingUCResource.AddBookmarkLink);
            Title = HeaderStringHelper.GetPageTitle(BookmarkingUCResource.AddBookmarkLink);
        }

        protected override void InitBreadcrumbs(string pageTitle)
        {
            Title = pageTitle;
        }
    }
}