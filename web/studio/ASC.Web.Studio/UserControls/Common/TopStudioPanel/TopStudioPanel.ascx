<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopStudioPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.TopStudioPanel" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Core.WhiteLabel" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Statistics" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<div class="studio-top-panel mainPageLayout  <% if (CoreContext.Configuration.Personal)
                                                { %>try-welcome-top<% } %>">
    <ul>
        <li class="studio-top-logo">
            <a class="top-logo" title="<%: GetAbsoluteCompanyTopLogoTitle() %>" href="<%= CommonLinkUtility.GetDefault() %>">
                <img alt="" src="<%= GetAbsoluteCompanyTopLogoPath() %>" />
            </a>
        </li>

        <% if (CoreContext.Configuration.Personal && !SecurityContext.IsAuthenticated && !DisableLoginPersonal)
           {
                if (!CoreContext.Configuration.CustomMode)
                { %>
                    <div class="personal-languages">
                        <div class="personal-languages_select <%= CultureInfo.CurrentUICulture.Name %>" data-lang="<%= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName %>">
                            <span><%= CultureInfo.CurrentUICulture.DisplayName %></span>
                        </div>
                        <div id="AuthFormLanguagesPanel" class="studio-action-panel">
                            <ul class="personal-languages_list dropdown-content">
                                <% foreach (var ci in SetupInfo.EnabledCulturesPersonal)
                                    { %>
                                <li class="dropdown-item <%= ci.Name %>">
                                    <a href="<%= Request.Path %>?lang=<%= ci.TwoLetterISOLanguageName %>"><%= ci.DisplayName %></a>
                                </li>
                                <% } %>
                            </ul>
                        </div>
                    </div>
                <% } %>
            <li id="personalLogin" class="personal-login">
                <% if (CoreContext.Configuration.CustomMode){ %><a><%= CustomModeResource.LoginCustomMode %></a><% } %>
                <%else{%><a><%= Resource.Login %></a><% } %>
            </li>
          <% }%>

        <% if (DisplayModuleList)
           { %>
            <li class="product-menu with-subitem">
           <span class="active-icon-svg">
               <svg class="active-icon"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenu<%= CurrentProductClassName + (CurrentProductClassNamePostfix ?? "") %>"></use></svg>
           </span>
                <a class="product-cur-link" title="<%= CurrentProductName %>">
                    <%: CurrentProductName %>
                </a>
            </li>
        <% } %>

        <% if (UserInfoVisible)
           { %>
            <%--my staff--%>

            <li class="staff-profile-box" >
                <span class="userLink">
                    <span class="usr-prof" title="<%= UserInfoExtension.DisplayUserName(CurrentUser) %>">
                        <%= UserInfoExtension.DisplayUserName(CurrentUser) %>
                    </span>
                </span>
            </li>
            <%= RenderCustomNavigation() %>
        <% } %>

        <% if (CurrentUser != null && CurrentUser.IsOutsider())
           { %>
            <li class="top-item-box signin" >
                <a href="<%= VirtualPathUtility.ToAbsolute("~/Auth.aspx?t=logout") %>" title="<%= Resource.LoginButton %>"><%= Resource.LoginButton %></a>
            </li>
        <% } %>

        <% if (!DisableSearch)
           { %>
            <li class="top-item-box search">
                <span class="searchActiveBox inner-text" title="<%= Resource.Search %>">
                    <svg><use base="<%= WebPath.GetPath("/") %>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusearch"></use></svg>
                </span>
            </li>
        <% } %>

        <% if (IsAdministrator && !DisableSettings)
           { %>
            <li class="top-item-box settings" >
                <a class="inner-text" href="<%= CommonLinkUtility.GetAdministration(ManagementType.Customization) %>" title="<%= Resource.Administration %>">
                    <svg><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg>
                </a>
            </li>
        <% } %>

        <% if (!DisableTariff)
           { %>
            <li class="top-item-box tariffs <%= DisplayTrialCountDays ? "has-led" : "" %>">
                <a class="inner-text" href="<%= TenantExtra.GetTariffPageLink() %>" title="<%= Resource.TariffSettings %>">
                    <svg><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenupayments<%= CoreContext.Configuration.CustomMode ? "Rub" : "" %>"></use></svg>
                    <% if (DisplayTrialCountDays)
                       { %>
                        <span class="inner-label"><%= CoreContext.Configuration.CustomMode ? Resource.Trial.ToLower() : "trial" %> <%= TariffDays %></span>
                    <% } %>
                </a>
            </li>
        <% } %>

        <% if (!DisableGift)
           { %>
            <li class="top-item-box gift has-led">
                <a class="inner-text giftBox" title="<%= Resource.Present %>">
                    <svg><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenugift"></use></svg>
                    <span class="inner-label">1</span>
                </a>
            </li>
        <% } %>

        <li class="clear"></li>
    </ul>

    <asp:PlaceHolder runat="server" ID="_productListHolder">
        <% if (DisplayModuleList)
           { %>
            <div id="studio_productListPopupPanel" class="studio-action-panel modules">
                <div class="wrapper">
                    <div class="columns">

                        <div class="tile main-nav-items">
                            <ul class="dropdown-content">
                                <% foreach (var item in Modules) { %>
                                <li class="<%= item.ProductClassName + (item.IsDisabled() ? " display-none" : string.Empty) %>">
                                    <a href="<%= VirtualPathUtility.ToAbsolute(item.StartURL) %>" class="dropdown-item menu-products-item <%= item.ProductClassName == CurrentProductClassName ? "active" : "" %>">
                                        <span class="dropdown-item-icon">
                                            <svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenu<%= item.ProductClassName %>"></use></svg>
                                        </span>
                                        <%= item.Name.HtmlEncode() %>
                                    </a>
                                </li>
                                <% } %>

                                <% if (TenantExtra.EnableControlPanel){ %>
                                <li class="controlpanel">
                                    <a href="<%= SetupInfo.ControlPanelUrl %>" target="_blank" class="dropdown-item menu-products-item controlpanel">
                                        <span class="dropdown-item-icon">
                                            <svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenucontrolpanel"></use></svg>
                                        </span>
                                        <%= Resource.ControlPanelLabel %>
                                    </a>
                                </li>
                                <%} %>

                                <% if (CurrentUser != null && !CurrentUser.IsOutsider()) { %>
                                <% foreach (var item in Addons) { %>
                                <li class="<%= item.ProductClassName + (item.IsDisabled() ? " display-none" : string.Empty) %>">
                                    <a href="<%= VirtualPathUtility.ToAbsolute(item.StartURL) %>" class="dropdown-item menu-products-item <%= item.ProductClassName == CurrentProductClassName ? "active" : "" %>">
                                        <span class="dropdown-item-icon">
                                            <svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenu<%= item.ProductClassName %>"></use></svg>
                                        </span>
                                        <%= item.Name.HtmlEncode() %>
                                    </a>
                                </li>
                                <% } %>
                                <li class="feed">
                                    <a href="<%= VirtualPathUtility.ToAbsolute("~/Feed.aspx") %>" class="dropdown-item menu-products-item <%= "feed" == CurrentProductClassName ? "active" : "" %>">
                                        <span class="dropdown-item-icon"> <svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenufeed"></use></svg> </span>
                                        <%= UserControlsCommonResource.FeedTitle %>
                                    </a>
                                </li>
                                <% } %>
                            </ul>           
                        </div>

                        <div class="tile custom-nav-items">
                            <ul class="dropdown-content">
                                <% foreach (var item in CustomModules) { %>
                                <li class="<%= item.ProductClassName + (item.IsDisabled() ? " display-none" : string.Empty) %>">
                                    <a href="<%= VirtualPathUtility.ToAbsolute(item.StartURL) %>" class="dropdown-item menu-products-item <%= item.ProductClassName == CurrentProductClassName ? "active" : "" %>">
                                        <span class="dropdown-item-icon">
                                            <svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenu<%= item.ProductClassName %>"></use></svg>
                                        </span>
                                        <%= item.Name.HtmlEncode() %>
                                    </a>
                                </li>
                                <% } %>
                                <% foreach (var item in CustomNavigationItems) { %>
                                <li id="topNavCustomItem_<%= item.Id %>">
                                    <a href="<%= item.Url.HtmlEncode() %>" target="_blank" class="dropdown-item menu-products-item">
                                        <span class="dropdown-item-icon" style="background: url('<%= item.SmallImg %>')"></span>
                                        <%= item.Label.HtmlEncode() %>
                                    </a>
                                </li>
                                <% } %>
                            </ul>
                        </div>

                        <% if (IsAdministrator || (ShowAppsNavItem && AuthServiceList.Any()) || ShowDesktopNavItem || !DisableTariff)
                           { %>
                        <div class="tile spec-nav-items">
                            <ul class="dropdown-content">
                                <% if (IsAdministrator) { %>
                                <li class="settings"><a href="<%= CommonLinkUtility.GetAdministration(ManagementType.Customization) %>" title="<%= Resource.Administration %>" class="dropdown-item menu-products-item <%= "settings" == CurrentProductClassName ? "active" : "" %>">
                                    <span class="dropdown-item-icon"><svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg></span><%= Resource.Administration %></a></li>
                                <% } %>
                                
                                <% if (ShowAppsNavItem) { %>
                                <li class="apps">
                                    <% if (IsAdministrator) { %>
                                    <a href="<%= CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) %>" title="<%= Resource.Apps %>" class="dropdown-item menu-products-item <%= "apps" == CurrentProductClassName ? "active" : "" %>"><span class="dropdown-item-icon"><svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuproductsandinstruments"></use></svg></span><%= Resource.Apps %></a>
                                    <% } else if (AuthServiceList.Any()) { %>
                                    <a title="<%= Resource.Apps %>" class="dropdown-item menu-products-item"><span class="dropdown-item-icon"><svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuproductsandinstruments"></use></svg></span><%= Resource.Apps %></a>
                                    <div id="appsPopupBody" class="display-none">
                                        <p>
                                            <%= Resource.AppsDescription %><br>
                                            <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink())) { %>
                                            <%= string.Format(Resource.AppsDescriptionHelp, "<a href=\"" + CommonLinkUtility.GetHelpLink() + "/server/windows/community/authorization-keys.aspx\" class=\"link underline\" target=\"_blank\">", "</a>") %>
                                            <% } %>
                                        </p>
                                        <div class="apps-list">
                                        <% foreach (var service in AuthServiceList) { %>
                                            <span>
                                                <img src="<%= VirtualPathUtility.ToAbsolute("~/UserControls/Management/AuthorizationKeys/img/" + service.Name.ToLower() + ".svg") %>" alt="<%= service.Title %>" />
                                            </span>
                                        <% } %>
                                        </div>
                                        <div class="small-button-container">
                                            <a class="button gray middle" onclick="jq.unblockUI();"><%= Resource.OKButton %></a>        
                                        </div>
                                    </div>
                                    <% } %>
                                </li>
                                <% } %>
                                <% if (!DisableTariff) { %>
                                <li class="tarrifs"><a href="<%= TenantExtra.GetTariffPageLink() %>" title="<%= Resource.TariffSettings %>" class="dropdown-item menu-products-item <%= CurrentProductClassName == "payments" ? "active" : "" %>"><span class="dropdown-item-icon"> <svg class="dropdown-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenupayments<%= CoreContext.Configuration.CustomMode ? "Rub" : "" %>"></use></svg> </span><%= Resource.TariffSettings %></a></li>
                                <% } %>
                            </ul>
                        </div>
                        <% } %>

                    </div>
                </div>
            </div>
        <% } %>
    </asp:PlaceHolder>

    <% if (!DisableSearch)
       { %>
        <div id="studio_searchPopupPanel" class="studio-action-panel">
            <div class="dropdown-content">
                <div class="search-input-box">
                    <input type="search" id="studio_search" class="search-input textEdit" placeholder="<%= UserControlsCommonResource.SearchHld %>" maxlength="255" data-url="<%= VirtualPathUtility.ToAbsolute("~/Search.aspx") %>" />
                    <button class="button blue search-btn" type="button"></button>
                </div>
                <div class="header-base small bold"><%= UserControlsCommonResource.SeeInModulesHdr %></div>
                <div class="search-options-box clearFix">
                    <% foreach (var product in SearchProducts)
                       { %>
                        <div class="search-option-box">
                            <label>
                                <input id="searchfield_<%= product.ProductClassName %>" type="checkbox" data-product-id="<%= product.ID %>"/>
                                <%= product.Name %>
                            </label>
                        </div>
                    <% } %>
                </div>
            </div>
        </div>
    <% } %>
    
    <% if (UserInfoVisible)
       { %>
        <div id="studio_myStaffPopupPanel" class="studio-action-panel">
            <ul class="dropdown-content">
                <% if (!TenantStatisticsProvider.IsNotPaid())
                   { %>
                    <li>
                        <a class="dropdown-item" <%if (CoreContext.Configuration.Personal){ %> target="_blank" <%} %> href="<%= CommonLinkUtility.GetMyStaff() %>">
                            <%= Resource.Profile %>
                        </a>
                    </li>
                <% } %>

                <li>
                    <span class="dropdown-item dropdown-about-btn"><%= Resource.AboutCompanyTitle %> </span>

                    <div id="aboutCompanyPopupBody" class="display-none">
                        <div class="confirmation-popup_logo" style="<%= ConfirmationLogoStyle %>"></div>                                                           
                        <div class="confirmation-popup_version gray-text">
                            <%= string.IsNullOrEmpty(VersionNumber) ? "" : Resource.AboutCompanyVersion + " " + VersionNumber %>
                        </div>

                        <% if (Settings.IsLicensor) { %>
                        <div class="confirmation-popup_licensor"><%= Resource.AboutCompanyLicensor %></div>
                        <% } %>

                        <% if (!String.IsNullOrEmpty(Settings.CompanyName)) { %>
                        <div class="confirmation-popup_name"><%= Settings.CompanyName %></div>
                        <% } %>

                        <ul class="confirmation-popup_info">
                            <% if (!String.IsNullOrEmpty(Settings.Address)) { %>
                            <li>
                                <span class="gray-text"><%= Resource.AboutCompanyAddressTitle %>: 
                                </span><%= Settings.Address %>
                            </li>
                            <% } %>
                            <% if (!String.IsNullOrEmpty(Settings.Email)) { %>
                            <li>
                                <span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: 
                                </span><a href="mailto:<%= Settings.Email %>" class="link"><%= Settings.Email %></a>
                            </li>
                            <% } %>
                            <% if (!String.IsNullOrEmpty(Settings.Phone)) { %>
                            <li>
                                <span class="gray-text"><%= Resource.AboutCompanyTelTitle %>: 
                                </span><%= Settings.Phone %>
                            </li>
                            <% } %>
                            <% if (!String.IsNullOrEmpty(Settings.Site)) { %>
                            <li>
                                <a href="<%= Settings.Site %>" target="_blank" class="link">
                                    <%= Settings.Site.Replace(Uri.UriSchemeHttp + Uri.SchemeDelimiter, String.Empty).Replace(Uri.UriSchemeHttps + Uri.SchemeDelimiter, String.Empty) %>
                                </a>
                            </li>
                            <% } %>
                        </ul>

                        <% if (TenantExtra.Opensource)
                           { %>
                        <br />
                        <div><%= string.Format(UserControlsCommonResource.LicensedUnderApache, "<a href=\"http://www.apache.org/licenses/LICENSE-2.0\" class=\"link underline\" target=\"_blank\">", "</a>") %></div>
                        <div><%= string.Format(UserControlsCommonResource.SourceCode, "<a href=\"https://github.com/ONLYOFFICE/CommunityServer\" class=\"link underline\" target=\"_blank\">", "</a>") %></div>
                        <% } %>

                        <% if (!Settings.IsDefault && !Settings.IsLicensor)
                           {
                               var defaultSettings = Settings.GetDefault() as CompanyWhiteLabelSettings;
                        %>
                            <div class="confirmation-copyright gray-text">
                                <%= string.Format(Resource.AboutCopyright, DateTime.Now.Year, defaultSettings.CompanyName) %>
                                <a href="<%= defaultSettings.Site %>" target="_blank" class="link gray-text">
                                    <%= defaultSettings.Site.Replace(Uri.UriSchemeHttp + Uri.SchemeDelimiter, String.Empty).Replace(Uri.UriSchemeHttps + Uri.SchemeDelimiter, String.Empty) %>
                                </a>
                            </div>
                        <% } %>
                    </div>
                </li>
                <% if (DebugInfo.ShowDebugInfo) %>
                <% { %>
                    <li>
                        <span class="dropdown-item dropdown-debuginfo-btn">Debug Info</span>
                        <input id="debugInfoPopUpBody" type="hidden" value="<%: DebugInfo.DebugString %>">
                    </li>
                <% } %>

                <%--Logout--%>
                <li id="logout_ref"><a class="dropdown-item" href="<%= CommonLinkUtility.Logout %>"><%= UserControlsCommonResource.LogoutButton %></a></li>
            </ul>
        </div>
    <% } %>
    
   <%-- Feed popup block--%>
    <% if (!CoreContext.Configuration.Personal)
       { %>
    <div id="studio_dropFeedsPopupPanel" class="studio-action-panel">
        <div id="drop-feeds-box" class="drop-list-box">
            <div class="list display-none"></div>
            <div class="loader-text-block"><%= FeedResource.LoadingMsg %></div>
            <div class="feeds-readed-msg"><span><%= FeedResource.FeedsReadedMsg %></span></div>
            <a class="see-all-btn" href="<%= VirtualPathUtility.ToAbsolute("~/Feed.aspx") %>">
                <%= FeedResource.SeeAllFeedsBtn %>
            </a>
        </div>
    </div>
    <% } %>
    
   <%--Unread mail popup block--%>

    <div id="studio_dropMailPopupPanel" class="studio-action-panel">
        <div id="drop-mail-box" class="drop-list-box mail">
            <div class="list"></div>
            <div class="loader-text-block"><%= FeedResource.LoadingMsg %></div>
            <div class="mail-readed-msg"><span><%= Resource.MailReadedMsg %></span></div>
            <a class="see-all-btn " href="<%= VirtualPathUtility.ToAbsolute("~/addons/mail/") %>">
                <%= Resource.SeeAllBtn %>
            </a>
            <span class="mark-all-btn"><%= Resource.MarkAllAsRead %></span>
        </div>
    </div>

    <% if (!DisableGift)
       { %>
    <div id="studio_dropGiftPopupPanel" class="studio-action-panel">
        <div id="drop-gift-box" class="drop-list-box">
            <% if (IsAdministrator) { %>
            <div class="hdr"><%= Resource.PresentAdminHdr %></div>
            <% } else { %>
            <div class="hdr"><%= Resource.PresentUserHdr %></div>
            <% } %>
            <div class="gift-list">
                <div class="gift-item gift-item-pro"><%= Resource.PresentItemPro %></div>
                <% if (false && !CoreContext.Configuration.CustomMode) { %>
                <div class="gift-item gift-item-private"><%= Resource.PresentItemPrivate %></div>
                <% } %>
                <div class="gift-item gift-item-mobile"><%= Resource.PresentItemMobile %></div>
            </div>
            <% if (IsAdministrator) { %>
            <div class="btn-box">
                <span class="left-btn" data-url="<%= SetupInfo.ControlPanelUrl.TrimEnd('/') + "/gift" %>"><%= Resource.PresentAdminOkBtn %></span>
                <span class="right-btn"><%= Resource.PresentAdminHideBtn %></span>
            </div>
            <% } else { %>
            <div class="btn-box">
                <span class="left-btn"><%= Resource.PresentUserOkBtn %></span>
                <span class="right-btn"><%= Resource.PresentUserHideBtn %></span>
            </div>
             <% } %>
        </div>
    </div>
    <% } %>

    <asp:PlaceHolder runat="server" ID="_customNavControls"></asp:PlaceHolder>
</div>