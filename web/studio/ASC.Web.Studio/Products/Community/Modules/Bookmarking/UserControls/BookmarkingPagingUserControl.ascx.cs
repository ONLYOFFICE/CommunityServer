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
using System.Web.UI;
using ASC.Bookmarking;
using ASC.Bookmarking.Common;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using AjaxPro;

namespace ASC.Web.UserControls.Bookmarking
{
    [AjaxNamespace("BookmarkPage")]
    public partial class BookmarkingPagingUserControl : UserControl
    {
        protected bool SingleBookmark { get; private set; }

        protected int ItemCounter { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var pageCounter = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);

            var helper = BookmarkingServiceHelper.GetCurrentInstanse();

            var bookmarks = helper.GetBookmarks(pageCounter);

            if (bookmarks == null || bookmarks.Count <= 0) return;

            var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

            if (BookmarkDisplayMode.SelectedBookmark.Equals(displayMode))
            {
                SingleBookmark = true;
            }
            else
            {
                SingleBookmark = false;

                var pageNavigator = new PageNavigator();

                helper.InitPageNavigator(pageNavigator, pageCounter);

                ItemCounter = pageNavigator.EntryCount;

                BookmarkingPaginationContainer.Controls.Add(pageNavigator);
            }
        }
    }
}