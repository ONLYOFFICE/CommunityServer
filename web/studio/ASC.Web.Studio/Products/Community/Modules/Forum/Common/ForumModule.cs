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
using System.Collections.Generic;
using System.Web;
using ASC.Web.Community.Forum.Common;
using ASC.Web.Community.Forum.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum.Common
{
    public class ForumModule : Module
    {
        public override Guid ID
        {
            get { return ForumManager.ModuleID; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return ForumResource.AddonName; }
        }

        public override string Description
        {
            get { return ForumResource.AddonDescription; }
        }

        public override string StartURL
        {
            get { return "~/Products/Community/Modules/Forum/"; }
        }
     
        public ForumModule()
        {
            Context = new ModuleContext
            {
                DefaultSortOrder = 2,
                SmallIconFileName = "forum_mini_icon.png",
                IconFileName = "forum_icon.png",
                SubscriptionManager = new ForumSubscriptionManager(),
                GetCreateContentPageAbsoluteUrl = ForumShortcutProvider.GetCreateContentPageUrl,
                SearchHandler = new ForumSearchHandler(),
            };
        }
    }
}
