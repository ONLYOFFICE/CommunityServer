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
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.Community.Wiki
{
    public class WikiModule : Module
    {
        public override Guid ID
        {
            get { return WikiManager.ModuleId; }
        }

        public override Guid ProjectId
        {
            get { return CommunityProduct.ID; }
        }

        public override string Name
        {
            get { return WikiResource.ModuleName; }
        }

        public override string Description
        {
            get { return WikiResource.ModuleDescription; }
        }

        public override string StartURL
        {
            get { return "~/Products/Community/Modules/Wiki/"; }
        }

        public static string GetCreateContentPageUrl()
        {
            return VirtualPathUtility.ToAbsolute(WikiManager.BaseVirtualPath + "/Default.aspx") + "?action=new";
        }


        public WikiModule()
        {
            Context = new ModuleContext
            {
                DefaultSortOrder = 5,
                SmallIconFileName = "wikilogo16.png",
                IconFileName = "wikilogo32.png",
                SubscriptionManager = new WikiSubscriptionManager(),
                GetCreateContentPageAbsoluteUrl = GetCreateContentPageUrl,
                SearchHandler = new WikiSearchHandler(),
            };
        }
    }
}
