/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Caching;
using ASC.Core.Users;
using ASC.Web.Community.Controls;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Web.Studio;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;


namespace ASC.Web.Community
{
    public partial class _Default : MainPage
    {
        private static ICache showEmptyScreen = AscCache.Default;
        private const String NavigationPanelCookieName = "asc_minimized_np";

        private bool ShowEmptyScreen()
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                return false;
            }
            if (showEmptyScreen.Get("communityScreen" + TenantProvider.CurrentTenantID) != null)
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
                var dashboardEmptyScreen = (DashboardEmptyScreen)Page.LoadControl(DashboardEmptyScreen.Location);
                dashboardEmptyScreen.ProductStartUrl = GetStartUrl();
                emptyModuleCommunity.Controls.Add(dashboardEmptyScreen);
                ImportUsers.Controls.Add(new ImportUsersWebControl());
                return;
            }

            Response.Redirect(GetStartUrl());
        }


        protected string RenderGreetingTitle()
        {
            return CoreContext.TenantManager.GetCurrentTenant().Name.HtmlEncode();
        }

        private string GetStartUrl()
        {
            var enabledList = new Dictionary<int, string>();
            var modules = WebItemManager.Instance.GetSubItems(CommunityProduct.ID);

            foreach (var m in modules)
            {
                switch (m.GetSysName())
                {
                    case "community-blogs":
                        enabledList.Add(1, "modules/blogs/");
                        break;
                    case "community-news":
                        enabledList.Add(2, "modules/news/");
                        break;
                    case "community-forum":
                        enabledList.Add(3, "modules/forum/");
                        break;
                    case "community-bookmarking":
                        enabledList.Add(4, "modules/bookmarking/");
                        break;
                    case "community-wiki":
                        enabledList.Add(5, "modules/wiki/");
                        break;
                }
            }
            if (enabledList.Count > 0)
            {
                var keys = enabledList.Keys.ToList();
                keys.Sort();
                return enabledList[keys[0]];
            }
            return "modules/birthdays/";
        }

    }
}
