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
using ASC.Web.Community.Product;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;

namespace ASC.Web.UserControls.Bookmarking.Common.Search
{
    public class BookmarkingSearchHandler : BaseSearchHandlerEx
    {
        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "bookmarking_mini_icon.png", PartID = ModuleID }; }
        }

        public override string SearchName
        {
            get { return BookmarkingUCResource.BookmarkingSearch; }
        }

        public override SearchResultItem[] Search(string text)
        {
            return BookmarkingServiceHelper.GetCurrentInstanse().SearchBookmarksBySearchString(text);
        }

        public override Guid ModuleID
        {
            get { return BookmarkingSettings.ModuleId; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }

        public override IItemControl Control
        {
            get { return new ResultsView(); }
        }
    }
}