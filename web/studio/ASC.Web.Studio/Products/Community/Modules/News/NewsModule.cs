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
using ASC.Web.Community.News;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;

namespace ASC.Web.Community.News
{
    public class NewsModule : Module
    {
        public override Guid ID
        {
            get { return NewsConst.ModuleId; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return NewsResource.AddonName; }
        }

        public override string Description
        {
            get { return NewsResource.AddonDescriptionResourceKey; }
        }

        public override string StartURL
        {
            get { return "~/Products/Community/Modules/News/"; }
        }

        public NewsModule()
        {
            Context = new ModuleContext
                          {
                              SmallIconFileName = "newslogo.png",
                              IconFileName = "32x_news.png",
                              SubscriptionManager = new SubscriptionManager(),
                              GetCreateContentPageAbsoluteUrl = () => CommunitySecurity.CheckPermissions(NewsConst.Action_Add) ? FeedUrls.EditNewsUrl : null,
                              SearchHandler = new SearchHandler(),
                          };
        }
    }
}