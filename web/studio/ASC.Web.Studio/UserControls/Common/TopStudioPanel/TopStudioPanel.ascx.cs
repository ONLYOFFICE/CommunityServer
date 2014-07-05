/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.HelpCenter;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using System.Configuration;
using ASC.Core.Billing;

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

        protected List<VideoGuideItem> VideoGuideItems { get; set; }
        protected string AllVideoLink = CommonLinkUtility.GetHelpLink(true) + "video.aspx";

        public bool? DisableUserInfo;

        public bool DisableProductNavigation { get; set; }
        public bool DisableSearch { get; set; }
        public bool DisableSettings { get; set; }
        public bool DisableTariff { get; set; }
        public bool DisableVideo { get; set; }

        public string TopLogo =
            CoreContext.Configuration.Personal
                ? WebImageSupplier.GetAbsoluteWebPath("logo/logo_personal.png")
                : WebImageSupplier.GetAbsoluteWebPath("logo/top_logo.png");

        protected string ConfirmationLogo
        {
            get
            {
                return CoreContext.Configuration.Personal
                    ? WebImageSupplier.GetAbsoluteWebPath("logo/logo_personal_auth.png")
                    : WebImageSupplier.GetAbsoluteWebPath("logo/logo.png");
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

        protected bool DisplayTrialCountDays
        {
            get
            {
                var tariff = TenantExtra.GetCurrentTariff();
                return (tariff.State == TariffState.Trial && tariff.DueDate.Subtract(DateTime.Today.Date).Days > 0);
            }
        }
        protected int TrialCountDays
        {
            get
            {
                return TenantExtra.GetCurrentTariff().DueDate.Subtract(DateTime.Today.Date).Days;
            }
        }

        protected bool? IsAutorizePartner { get; set; }
        protected Partner Partner { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            debugInfoPopUpContainer.Options.IsPopup = true;

            aboutCompanyPopupContainer.Options.IsPopup = true;
            RenderVideoHandlers();

            if (!DisableSearch)
            {
                RenderSearchProducts();
                DisableSearch = DisableSearch || !SearchProducts.Any() || CoreContext.Configuration.Personal;
            }

            UserInfoVisible =
                (!DisableUserInfo.HasValue || !DisableUserInfo.Value)
                && SecurityContext.IsAuthenticated;

            if (SecurityContext.IsAuthenticated)
                CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!SecurityContext.IsAuthenticated || !TenantExtra.EnableTarrifSettings || CoreContext.Configuration.Personal || CurrentUser.IsVisitor())
            {
                DisableTariff = true;
            }

            _customNavItems = WebItemManager.Instance.GetItems(WebZoneType.CustomProductList, ItemAvailableState.Normal);

            if (CurrentUser.IsVisitor())
            {
                _customNavItems.RemoveAll(item => item.ID == WebItemManager.MailProductID); // remove mail for guest
            }

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
                IsAutorizePartner = false;
                var partner = CoreContext.PaymentManager.GetApprovedPartner();
                if (partner != null)
                {
                    IsAutorizePartner = !string.IsNullOrEmpty(partner.AuthorizedKey);
                    Partner = partner;
                }
            }
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

                    _currentProduct = (IProduct)WebItemManager.Instance[currentProductID];
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

            return sb.ToString();
        }

        protected string GetAbsoluteCompanyTopLogoPath()
        {
            return string.IsNullOrEmpty(SetupInfo.MainLogoURL) ? TopLogo : SetupInfo.MainLogoURL;
        }

        protected void RenderVideoHandlers()
        {
            if (string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink(false)))
            {
                DisableVideo = true;
                return;
            }

            VideoGuideItems = HelpCenterHelper.GetVideoGuides();

            if (VideoGuideItems.Count > 0)
            {
                AjaxPro.Utility.RegisterTypeForAjax(typeof(UserVideoSettings));
            }
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