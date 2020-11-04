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


using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Studio;
using ASC.Web.Studio.UserControls.EmptyScreens;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Web.Community
{
    public partial class _Default : MainPage
    {
        private static readonly ICache showEmptyScreen = AscCache.Default;

        private static bool ShowEmptyScreen()
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                return false;
            }
            if (showEmptyScreen.Get<object>("communityScreen" + TenantProvider.CurrentTenantID) != null)
            {
                return false;
            }

            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            using (var db = new DbManager("community"))
            {
                var hasacitvity = db.ExecuteScalar<bool>(@"select exists(select 1 from blogs_posts where tenant = @tid) or " +
                    "exists(select 1 from bookmarking_bookmark where tenant = @tid) or exists(select 1 from events_feed where tenant = @tid) or " +
                    "exists(select 1 from forum_category where tenantid = @tid)", new { tid = tenantId });
                if (hasacitvity)
                {
                    showEmptyScreen.Insert("communityScreen" + tenantId, new object(), TimeSpan.FromMinutes(30));
                }
                return !hasacitvity;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (ShowEmptyScreen())
            {
                var dashboardEmptyScreen = (CommunityDashboardEmptyScreen)Page.LoadControl(CommunityDashboardEmptyScreen.Location);
                dashboardEmptyScreen.ProductStartUrl = GetStartUrl();
                emptyModuleCommunity.Controls.Add(dashboardEmptyScreen);
                return;
            }

            Response.Redirect(GetStartUrl());
        }


        protected string RenderGreetingTitle()
        {
            return CoreContext.TenantManager.GetCurrentTenant().Name.HtmlEncode();
        }

        private static string GetStartUrl()
        {
            var enabledList = new Dictionary<int, string>();
            var modules = WebItemManager.Instance.GetSubItems(CommunityProduct.ID);

            foreach (var m in modules)
            {
                switch (m.GetSysName())
                {
                    case "community-blogs":
                        enabledList.Add(1, "Modules/Blogs/");
                        break;
                    case "community-news":
                        enabledList.Add(2, "Modules/News/");
                        break;
                    case "community-forum":
                        enabledList.Add(3, "Modules/Forum/");
                        break;
                    case "community-bookmarking":
                        enabledList.Add(4, "Modules/Bookmarking/");
                        break;
                    case "community-wiki":
                        enabledList.Add(5, "Modules/Wiki/");
                        break;
                }
            }
            if (enabledList.Count > 0)
            {
                var keys = enabledList.Keys.ToList();
                keys.Sort();
                return enabledList[keys[0]];
            }
            return "Modules/Birthdays/";
        }
    }
}