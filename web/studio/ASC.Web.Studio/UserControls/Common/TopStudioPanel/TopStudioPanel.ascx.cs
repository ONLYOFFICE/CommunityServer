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


using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI;
using ASC.Common.Logging;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class TopStudioPanel : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/TopStudioPanel/TopStudioPanel.ascx"; }
        }

        protected UserInfo CurrentUser;
        protected bool DisplayModuleList;
        protected bool UserInfoVisible;

        public bool? DisableUserInfo;

        public bool DisableProductNavigation { get; set; }
        public bool DisableSearch { get; set; }
        public bool DisableSettings { get; set; }
        public bool DisableTariff { get; set; }
        public bool DisableLoginPersonal { get; set; }

        private string _topLogo = "";
        public string TopLogo
        {
            get
            {
                if (!String.IsNullOrEmpty(_topLogo)) return _topLogo;

                _topLogo = CoreContext.Configuration.Personal && !CoreContext.Configuration.CustomMode ?
                    WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal.png") :
                    TenantLogoManager.GetTopLogo(!TenantLogoManager.IsRetina(Request));

                return _topLogo;
            }
            set { _topLogo = value; }
        }

        protected string ConfirmationLogoStyle
        {
            get
            {
                if (CoreContext.Configuration.Personal && !CoreContext.Configuration.CustomMode)
                    return String.Format("height:72px; width: 220px; background: url('{0}') no-repeat;",
                                         WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal_auth_old.png"));

                var general = !TenantLogoManager.IsRetina(Request);

                var height = TenantWhiteLabelSettings.logoDarkSize.Height / 2;

                var width = TenantWhiteLabelSettings.logoDarkSize.Width / 2;

                if (TenantLogoManager.WhiteLabelEnabled)
                {
                    return String.Format("height:{0}px; width: {1}px; background: url('{2}') no-repeat; background-size: {1}px {0}px;",
                                         height, width, TenantLogoManager.GetLogoDark(general));
                }

                return String.Format("height:{0}px; width: {1}px; background: url('{2}') no-repeat; background-size: {1}px {0}px;",
                                     height, width, TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark,general));
            }
        }

        protected string VersionNumber {
            get
            {
                return ConfigurationManager.AppSettings["version.number"] ?? String.Empty;
                  
            }
        }
        protected IEnumerable<IWebItem> SearchProducts { get; set; }

        private List<IWebItem> _customNavItems;

        protected bool DisplayTrialCountDays;
        protected int TariffDays;
        protected CompanyWhiteLabelSettings Settings { get; set; }
        protected string CurrentProductName { get; set; }
        protected string CurrentProductClassName { get; set; }

        private List<AuthService> _authServiceList;
        protected List<AuthService> AuthServiceList
        {
            get
            {
                return _authServiceList ?? (_authServiceList = new AuthorizationKeys().AuthServiceList);
            }
        }

        protected bool ShowAppsNavItem { get; set; }
        protected bool ShowDesktopNavItem { get; set; }

        protected List<IWebItem> Modules { get; set; }
        protected IEnumerable<IWebItem> Addons { get; set; }
        protected List<IWebItem> CustomModules { get; set; }
        protected IEnumerable<CustomNavigationItem> CustomNavigationItems { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var currentProductId = string.IsNullOrEmpty(Request["productID"]) ? CommonLinkUtility.GetProductID() : new Guid(Request["productID"]);
            CurrentProduct = WebItemManager.Instance[currentProductId] as IProduct;

            if (CurrentProduct != null)
            {
                CurrentProductClassName = CurrentProduct.ProductClassName;
                CurrentProductName = CurrentProduct.Name;
            }
            else
            {
                GetAddonNameAndClass();
            }

            if (!DisableSearch)
            {
                RenderSearchProducts();
                DisableSearch = DisableSearch || !SearchProducts.Any() || CoreContext.Configuration.Personal;
            }

            if (SecurityContext.IsAuthenticated)
            {
                CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                if (CurrentUser.IsOutsider())
                    DisableUserInfo = true;

                UserInfoVisible = !DisableUserInfo.HasValue || !DisableUserInfo.Value;
            }

            if (!SecurityContext.IsAuthenticated || !TenantExtra.EnableTarrifSettings || CoreContext.Configuration.Personal || CurrentUser.IsVisitor() || (!CurrentUser.IsAdmin() && TariffSettings.HidePricingPage))
            {
                DisableTariff = true;
            }

            _customNavItems = WebItemManager.Instance.GetItems(WebZoneType.CustomProductList, ItemAvailableState.Normal);

            if (DisableProductNavigation && SecurityContext.IsAuthenticated)
                _productListHolder.Visible = false;
            else
            {
                var productsList = WebItemManager.Instance.GetItems(WebZoneType.TopNavigationProductList, ItemAvailableState.Normal);

                DisplayModuleList = productsList.Any() && !CoreContext.Configuration.Personal;

                Modules = new List<IWebItem>();
                CustomModules = new List<IWebItem>();

                foreach (var webItem in productsList)
                {
                    if (webItem.ID != WebItemManager.DocumentsProductID
                        && webItem.ID != WebItemManager.ProjectsProductID 
                        && webItem.ID != WebItemManager.CRMProductID 
                        && webItem.ID != WebItemManager.PeopleProductID
                        && webItem.ID != WebItemManager.CommunityProductID)
                    {
                        CustomModules.Add(webItem);
                    }
                }

                var currentItem = productsList.Find(r => r.ID == WebItemManager.DocumentsProductID);
                if (currentItem != null)
                {
                    Modules.Add(currentItem);
                    productsList.Remove(currentItem);
                }
                currentItem = productsList.Find(r => r.ID == WebItemManager.ProjectsProductID);
                if (currentItem != null)
                {
                    Modules.Add(currentItem);
                    productsList.Remove(currentItem);
                }
                currentItem = productsList.Find(r => r.ID == WebItemManager.CRMProductID);
                if (currentItem != null)
                {
                    Modules.Add(currentItem);
                    productsList.Remove(currentItem);
                }
                if (CurrentUser != null && !CurrentUser.IsOutsider())
                {
                    currentItem = _customNavItems.Find(r => r.ID == WebItemManager.MailProductID);
                    if (currentItem != null)
                    {
                        Modules.Add(currentItem);
                    }
                }
                currentItem = productsList.Find(r => r.ID == WebItemManager.PeopleProductID);
                if (currentItem != null)
                {
                    Modules.Add(currentItem);
                    productsList.Remove(currentItem);
                }
                currentItem = productsList.Find(r => r.ID == WebItemManager.CommunityProductID);
                if (currentItem != null)
                {
                    Modules.Add(currentItem);
                    productsList.Remove(currentItem);
                }

                Addons = _customNavItems
                    .Where(item =>
                           (item.ID == WebItemManager.CalendarProductID ||
                            item.ID == WebItemManager.TalkProductID))
                    .OrderBy(item => item.Context.DefaultSortOrder);

                CustomNavigationItems = CustomNavigationSettings.Load().Items.Where(x => x.ShowInMenu);
            }

            foreach (var item in _customNavItems)
            {
                var render = WebItemManager.Instance[item.ID] as IRenderCustomNavigation;
                if (render == null) continue;

                try
                {
                    var control = render.LoadCustomNavigationControl(Page);
                    if (control != null)
                    {
                        _customNavControls.Controls.Add(control);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC.Web.Studio").Error(ex);
                }
            }

            if (!DisableTariff)
            {
                var tariff = TenantExtra.GetCurrentTariff();
                TariffDays = tariff.DueDate.Date.Subtract(DateTime.Today).Days;

                if (tariff.State == TariffState.Trial && TariffDays >= 0)
                {
                    DisplayTrialCountDays = true;
                }
            }

            Settings = CompanyWhiteLabelSettings.Instance;

            ShowAppsNavItem = SetupInfo.IsVisibleSettings("AppsNavItem") && !CoreContext.Configuration.CustomMode;

            ShowDesktopNavItem = !CoreContext.Configuration.CustomMode;
        }

        #region currentProduct

        protected IProduct CurrentProduct { get; set; }

        private IWebItem GetCurrentWebItem
        {
            get { return CommonLinkUtility.GetWebItemByUrl(Context.Request.Url.AbsoluteUri); }
        }

        private void GetAddonNameAndClass()
        {
            if (Page is Studio.Feed)
            {
                CurrentProductClassName = "feed";
                CurrentProductName = UserControlsCommonResource.FeedTitle;
                return;
            }
            if (Page is Studio.AppInstall)
            {
                CurrentProductClassName = "install-apps";
                CurrentProductName = Resource.OnlyofficeApps;
                return;
            }

            if (Page is Studio.Tariffs)
            {
                CurrentProductClassName = "payments";
                CurrentProductName = Resource.Payments;
                return;
            }


            if (Page is Studio.Management)
            {
                var typeUrlParam = Request["type"];

                if (!string.IsNullOrEmpty(typeUrlParam))
                {
                    int managementType;

                    if (Int32.TryParse(Request["type"], out managementType) &&
                        Enum.IsDefined(typeof (ManagementType), managementType) &&
                        (ManagementType) managementType == ManagementType.ThirdPartyAuthorization)
                    {
                        CurrentProductClassName = "apps";
                        CurrentProductName = Resource.Apps;
                        return;
                    }

                }

                CurrentProductClassName = "settings";
                CurrentProductName = Resource.Administration;
                return;
            }

            var item = GetCurrentWebItem;

            if (item == null)
            {
                CurrentProductClassName = "";
                CurrentProductName = Resource.SelectProduct;
                return;
            }

            if (item.ID == WebItemManager.CalendarProductID || item.ID == WebItemManager.TalkProductID ||
                item.ID == WebItemManager.MailProductID)
            {
                CurrentProductClassName = item.ProductClassName;
                CurrentProductName = item.Name;
                return;
            }

            CurrentProductClassName = "";
            CurrentProductName = Resource.SelectProduct;
        }

        #endregion

        protected bool IsAdministrator
        {
            get { return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID); }
        }

        protected string RenderCustomNavigation()
        {
            if (TenantStatisticsProvider.IsNotPaid())
                return string.Empty;

            var sb = new StringBuilder();
            _customNavItems.Reverse();
            string rendered;
            foreach (var item in _customNavItems)
            {
                var render = WebItemManager.Instance[item.ID] as IRenderCustomNavigation;
                if (render == null) continue;

                if (WebItemManager.Instance[item.ID].IsDisabled()) continue;

                rendered = render.RenderCustomNavigation(Page);
                if (!string.IsNullOrEmpty(rendered))
                {
                    sb.Append(rendered);
                }
            }

            rendered = Studio.Feed.RenderCustomNavigation(Page);
            if (!string.IsNullOrEmpty(rendered))
            {
                sb.Append(rendered);
            }

/*            rendered = VoipNavigation.RenderCustomNavigation(Page);
            if (!string.IsNullOrEmpty(rendered))
            {
                sb.Append(rendered);
            }*/

            return sb.ToString();
        }

        protected string GetAbsoluteCompanyTopLogoPath()
        {
            return string.IsNullOrEmpty(SetupInfo.MainLogoURL) ? TopLogo : SetupInfo.MainLogoURL;
        }

        protected string GetAbsoluteCompanyTopLogoTitle()
        {
            return TenantLogoManager.GetLogoText();
        }

        protected void RenderSearchProducts()
        {
            var handlers = SearchHandlerManager.GetAllHandlersEx();

            SearchProducts = handlers
                .Select(sh => sh.ProductID)
                .Distinct()
                .Select(productID => WebItemManager.Instance[productID])
                .Where(product => product != null && !product.IsDisabled());
        }
    }
}