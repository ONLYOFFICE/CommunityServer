/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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