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
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Business.Subscriptions;
using ASC.Web.Community.Bookmarking;
using ASC.Web.Community.Bookmarking.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Common.Search;

namespace ASC.Web.Community.Bookmarking
{
    public class BookmarkingModule : Module
    {
        public override Guid ID
        {
            get { return BookmarkingSettings.ModuleId; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return BookmarkingResource.AddonName; }
        }

        public override string Description
        {
            get { return BookmarkingResource.AddonDescriptionResourceKey; }
        }

        public override string StartURL
        {
            get { return "~/Products/Community/Modules/Bookmarking/"; }
        }

        public BookmarkingModule()
        {
            Context = new ModuleContext
                {
                    DefaultSortOrder = 4,
                    SmallIconFileName = "bookmarking_mini_icon.png",
                    IconFileName = "bookmarking_icon.png",
                    SubscriptionManager = new BookmarkingSubscriptionManager(),
                    SearchHandler = new BookmarkingSearchHandler(),
                    GetCreateContentPageAbsoluteUrl = () => BookmarkingPermissionsCheck.PermissionCheckCreateBookmark() ? VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/" + BookmarkingServiceHelper.GetCreateBookmarkPageUrl()) : null,
                };
        }
    }
}