/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Linq;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.EmptyScreens;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    public partial class _Default : MainPage
    {
        protected Product _showDocs;

        protected UserInfo CurrentUser;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.Configuration.Personal)
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
        }


        protected bool? IsAutorizePartner { get; set; }
        protected Partner Partner { get; set; }

        protected List<IWebItem> defaultListProducts;

        protected IEnumerable<CustomNavigationItem> CustomNavigationItems { get; set; }

        protected int ProductsCount { get; set; }

        protected string ResetCacheKey;
        protected string ControlPanelImg;

        protected void Page_Load(object sender, EventArgs e)
        {
            CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/skins/page_default_dark.less");
                ControlPanelImg = WebImageSupplier.GetAbsoluteWebPath("icon-controlpanel-dark.svg");
            }
            else
            {
                Page.RegisterStyle("~/skins/page_default.less");
                ControlPanelImg = WebImageSupplier.GetAbsoluteWebPath("icon-controlpanel.svg");
            }
            
            var defaultPageSettings = StudioDefaultPageSettings.Load() ?? (StudioDefaultPageSettings)new StudioDefaultPageSettings().GetDefault();
            var isDesktop = Request.DesktopApp();
            var defaultProductID = isDesktop ? WebItemManager.DocumentsProductID : defaultPageSettings.DefaultProductID;
            if (defaultProductID != Guid.Empty)
            {
                if (defaultProductID == defaultPageSettings.FeedModuleID && !CurrentUser.IsOutsider())
                {
                    Response.Redirect("Feed.aspx", true);
                }

                var webItem = WebItemManager.Instance[defaultProductID];
                if (webItem != null && webItem.Visible)
                {
                    var securityInfo = WebItemSecurity.GetSecurityInfo(defaultProductID.ToString());
                    if (securityInfo.Enabled && WebItemSecurity.IsAvailableForMe(defaultProductID))
                    {
                        var url = webItem.StartURL;
                        if (isDesktop)
                        {
                            url += "?desktop=true";
                            if (!string.IsNullOrEmpty(Request["first"]))
                            {
                                url += "&first=true";
                            }
                        }
                        Response.Redirect(url, true);
                    }
                }
            }

            Master.DisabledSidePanel = true;

            Title = Resource.MainPageTitle;
            defaultListProducts = WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.StartProductList);
            _showDocs = (Product)defaultListProducts.Find(r => r.ID == WebItemManager.DocumentsProductID);
            if (_showDocs != null)
            {
                defaultListProducts.RemoveAll(r => r.ID == _showDocs.ProductID);
            }

            var mailProduct = WebItemManager.Instance[WebItemManager.MailProductID];
            if (mailProduct != null && !mailProduct.IsDisabled())
            {
                defaultListProducts.Add(mailProduct);
            }

            var calendarProduct = WebItemManager.Instance[WebItemManager.CalendarProductID];
            if (calendarProduct != null && !calendarProduct.IsDisabled())
            {
                defaultListProducts.Add(calendarProduct);
            }

            var talkProduct = WebItemManager.Instance[WebItemManager.TalkProductID];
            if (talkProduct != null && !talkProduct.IsDisabled())
            {
                defaultListProducts.Add(talkProduct);
            }

            var priority = GetStartProductsPriority();

            defaultListProducts = defaultListProducts
                .Where(p => priority.Keys.Contains(p.ID))
                .OrderBy(p => priority[p.ID])
                .ToList();

            CustomNavigationItems = CustomNavigationSettings.Load().Items.Where(x => x.ShowOnHomePage);

            ProductsCount = defaultListProducts.Count() + CustomNavigationItems.Count() + (TenantExtra.EnableControlPanel ? 1 : 0);

            ResetCacheKey = ConfigurationManagerExtension.AppSettings["web.client.cache.resetkey"] ?? "";

            if (CurrentUser.IsOwner() && TenantExtra.Saas && !CoreContext.Configuration.CustomMode && !TenantExtra.GetTenantQuota().Free)
            {
                var collaboratorPopupSettings = CollaboratorSettings.LoadForCurrentUser();
                if (collaboratorPopupSettings.FirstVisit)
                {
                    WelcomePanelHolder.Controls.Add(LoadControl(WelcomeDashboard.Location));
                }
            }
        }

        private static Dictionary<Guid, Int32> GetStartProductsPriority()
        {
            var priority = new Dictionary<Guid, Int32>
                    {
                        {WebItemManager.ProjectsProductID, 0},
                        {WebItemManager.CRMProductID, 1},
                        {WebItemManager.MailProductID, 2},
                        {WebItemManager.PeopleProductID, 3},
                        {WebItemManager.CommunityProductID, 4},
                        {WebItemManager.SampleProductID, 5}
                    };

            if (!string.IsNullOrEmpty(SetupInfo.StartProductList))
            {
                var products = SetupInfo.StartProductList.Split(',');

                if (products.Any())
                {
                    priority = new Dictionary<Guid, int>();

                    for (var i = 0; i < products.Length; i++)
                    {
                        var productId = GetProductId(products[i]);
                        if (productId != Guid.Empty)
                            priority.Add(productId, i);
                    }
                }
            }

            return priority;
        }

        private static Guid GetProductId(string productName)
        {
            Guid productId;

            if (Guid.TryParse(productName, out productId))
            {
                var product = WebItemManager.Instance[productId];
                if (product != null) return productId;
            }

            switch (productName.ToLowerInvariant())
            {
                case "documents":
                    return WebItemManager.DocumentsProductID;
                case "projects":
                    return WebItemManager.ProjectsProductID;
                case "crm":
                    return WebItemManager.CRMProductID;
                case "people":
                    return WebItemManager.PeopleProductID;
                case "community":
                    return WebItemManager.CommunityProductID;
                case "mail":
                    return WebItemManager.MailProductID;
                case "calendar":
                    return WebItemManager.CalendarProductID;
                case "talk":
                    return WebItemManager.TalkProductID;
                default:
                    return Guid.Empty;
            }
        }
    }
}