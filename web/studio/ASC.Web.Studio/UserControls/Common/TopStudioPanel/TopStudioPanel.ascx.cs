/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI;

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

                _topLogo = CoreContext.Configuration.Personal ?
                    WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal.png") :
                    TenantLogoManager.GetTopLogo(!TenantLogoManager.IsRetina(Request));

                return _topLogo;
            }
            set { _topLogo = value; }
        }

        protected string ConfirmationLogo
        {
            get
            {
                if (CoreContext.Configuration.Personal)
                    return WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal_auth.png");

                var general = !TenantLogoManager.IsRetina(Request);
                if (TenantLogoManager.WhiteLabelEnabled)
                {
                    return TenantLogoManager.GetLogoDark(general);
                }
                return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
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

        protected bool? IsAuthorizedPartner { get; set; }
        protected Partner Partner { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
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

            if (!SecurityContext.IsAuthenticated || !TenantExtra.EnableTarrifSettings || CoreContext.Configuration.Personal || CurrentUser.IsVisitor())
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
                _productRepeater.DataSource = productsList;

                _productRepeater.DataBind();

                var addons = _customNavItems.Where(pr => ((pr.ID == WebItemManager.CalendarProductID || pr.ID == WebItemManager.TalkProductID || pr.ID == WebItemManager.MailProductID)));
                _addonRepeater.DataSource = addons.ToList();
                _addonRepeater.DataBind();
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
                    log4net.LogManager.GetLogger("ASC.Web.Studio").Error(ex);
                }
            }

            if (CoreContext.Configuration.PartnerHosted)
            {
                IsAuthorizedPartner = false;
                var partner = CoreContext.PaymentManager.GetApprovedPartner();
                if (partner != null)
                {
                    IsAuthorizedPartner = !string.IsNullOrEmpty(partner.AuthorizedKey);
                    Partner = partner;
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

            if (VoipNavigation.VoipEnabled)
                _voipPhonePlaceholder.Controls.Add(LoadControl(VoipPhoneControl.Location));
        }

        #region currentProduct

        private IProduct _currentProduct;

        protected IProduct CurrentProduct
        {
            get
            {
                if (_currentProduct == null)
                {
                    var currentProductID =
                        String.IsNullOrEmpty(Request["productID"])
                            ? CommonLinkUtility.GetProductID()
                            : new Guid(Request["productID"]);

                    _currentProduct = WebItemManager.Instance[currentProductID] as IProduct;
                }
                return _currentProduct;
            }
        }

        private IWebItem GetCurrentWebItem
        {
            get { return CommonLinkUtility.GetWebItemByUrl(Context.Request.Url.AbsoluteUri); }
        }

        private string GetAddonNameOrEmptyClass()
        {
            if (Page is Studio.Feed)
                return "feed";

            if (Page is Studio.Management)
                return "settings";

            var item = GetCurrentWebItem;

            if (item == null)
                return "";

            if (item.ID == WebItemManager.CalendarProductID || item.ID == WebItemManager.TalkProductID || item.ID == WebItemManager.MailProductID)
                return item.ProductClassName;

            return "";
        }

        private string GetAddonNameOrEmpty()
        {
            if (Page is Studio.Feed)
                return UserControlsCommonResource.FeedTitle;

            if (Page is Studio.Management)
                return Resource.Administration;

            var item = GetCurrentWebItem;

            if (item == null)
                return Resource.SelectProduct;

            if (item.ID == WebItemManager.CalendarProductID || item.ID == WebItemManager.TalkProductID || item.ID == WebItemManager.MailProductID)
                return item.Name;

            return Resource.SelectProduct;
        }

        protected string CurrentProductName
        {
            get
            {
                return
                    CurrentProduct == null
                        ? GetAddonNameOrEmpty()
                        : CurrentProduct.Name;
            }
        }

        protected string CurrentProductClassName
        {
            get
            {
                return
                    CurrentProduct == null
                        ? GetAddonNameOrEmptyClass()
                        : CurrentProduct.ProductClassName;
            }
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

            rendered = VoipNavigation.RenderCustomNavigation(Page);
            if (!string.IsNullOrEmpty(rendered))
            {
                sb.Append(rendered);
            }

            return sb.ToString();
        }

        protected string GetAbsoluteCompanyTopLogoPath()
        {
            return string.IsNullOrEmpty(SetupInfo.MainLogoURL) ? TopLogo : SetupInfo.MainLogoURL;
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