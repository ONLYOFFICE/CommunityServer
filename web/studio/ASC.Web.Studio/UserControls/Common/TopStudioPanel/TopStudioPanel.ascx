<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopStudioPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.TopStudioPanel" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Tenants" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Statistics" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="studio-top-panel mainPageLayout  <% if (CoreContext.Configuration.Personal)
                                                { %>try-welcome-top<% } %>">
    <ul>
        <li class="studio-top-logo ">
            <a class="top-logo" title="<%= Resource.TeamLabOfficeTitle %>" href="<%= CommonLinkUtility.GetDefault() %>">
                <img alt="" src="<%= GetAbsoluteCompanyTopLogoPath() %>" />
                <% if (IsAuthorizedPartner.HasValue)
                   { %>
                    <span class="top-logo-partner_name">
                        <%= IsAuthorizedPartner.Value && Partner != null ? Resource.For + " " + (Partner.DisplayName ?? Partner.CompanyName).HtmlEncode() : Resource.HostedNonAuthorizedVersion %></span>
                <% } %>
            </a>
        </li>

        <% if (CoreContext.Configuration.Personal && !SecurityContext.IsAuthenticated && !DisableLoginPersonal)
           {%>
            <li id="personalLogin" class="personal-login">
                <a><%= Resource.Login %></a>
            </li>
          <% }%>

        <% if (DisplayModuleList)
           { %>
            <li class="product-menu with-subitem">
                <span class="active-icon <%= CurrentProductClassName %>"></span>
                <a class="product-cur-link" title="<%= CurrentProductName %>">
                    <%= CurrentProductName.HtmlEncode() %>
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
                <a href="<%= VirtualPathUtility.ToAbsolute("~/auth.aspx?t=logout") %>" title="<%= Resource.LoginButton %>"><%= Resource.LoginButton %></a>
            </li>
        <% } %>

        <% if (!DisableSearch)
           { %>
            <li class="top-item-box search">
                <span class="searchActiveBox inner-text" title="<%= Resource.Search %>"></span>
            </li>
        <% } %>

        <% if (IsAdministrator && !DisableSettings)
           { %>
            <li class="top-item-box settings" >
                <a class="inner-text" href="<%= CommonLinkUtility.GetAdministration(ManagementType.Customization) %>" title="<%= Resource.Administration %>"></a>
            </li>
        <% } %>

        <% if (!DisableTariff && !CoreContext.Configuration.Standalone)
           { %>
            <li class="top-item-box tariffs <% if (DisplayTrialCountDays)
                                               { %>has-led<% } %>">
                <a class="inner-text" href="<%= TenantExtra.GetTariffPageLink() %>" title="<%= Resource.TariffSettings %>">
                    <% if (DisplayTrialCountDays)
                       { %>
                        <span class="inner-label">trial <%= TrialCountDays %></span>
                    <% } %>
                </a>
            </li>
        <% } %>

        <li class="clear"></li>
    </ul>

    <asp:PlaceHolder runat="server" ID="_productListHolder">
        <% if (DisplayModuleList)
           { %>
            <div id="studio_productListPopupPanel" class="studio-action-panel modules">
                <ul class="dropdown-content">
                    <asp:Repeater runat="server" ID="_productRepeater">
                        <ItemTemplate>
                            <li class="<%# ((IWebItem)Container.DataItem).ProductClassName + (((IWebItem)Container.DataItem).IsDisabled() ? " display-none" : string.Empty) %>">
                                <a href="<%# VirtualPathUtility.ToAbsolute(((IWebItem)Container.DataItem).StartURL) %>" class="dropdown-item menu-products-item 
                                    <%# ((IWebItem)Container.DataItem).ProductClassName == CurrentProductClassName ? "active" : "" %>">
                                    <%# (((IWebItem)Container.DataItem).Name).HtmlEncode() %>
                                </a>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <% if (CurrentUser != null && !CurrentUser.IsOutsider())
                       { %>
                    <li class="dropdown-item-seporator"></li>
                    <asp:Repeater runat="server" ID="_addonRepeater">
                        <ItemTemplate>
                            <li class="<%# ((IWebItem)Container.DataItem).ProductClassName + (((IWebItem)Container.DataItem).IsDisabled() ? " display-none" : string.Empty) %>">
                                <a href="<%# VirtualPathUtility.ToAbsolute(((IWebItem)Container.DataItem).StartURL) %>" class="dropdown-item menu-products-item">
                                    <%# (((IWebItem)Container.DataItem).Name).HtmlEncode() %>
                                </a>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                    <li class="feed"><a href="<%= VirtualPathUtility.ToAbsolute("~/feed.aspx") %>" class="dropdown-item menu-products-item"><%= UserControlsCommonResource.FeedTitle %></a></li>
                    <% } %>

                    <% if (IsAdministrator)
                       { %>
                        <li class="dropdown-item-seporator"></li>
                        <li class="settings"><a href="<%= CommonLinkUtility.GetAdministration(ManagementType.Customization) %>" title="<%= Resource.Administration %>" class="dropdown-item menu-products-item"><%= Resource.Administration %></a></li>
                    <% } %>
                    <% if (!DisableTariff && !CoreContext.Configuration.Standalone)
                       { %>
                        <li class="tarrifs"><a href="<%= TenantExtra.GetTariffPageLink() %>" title="<%= Resource.TariffSettings %>" class="dropdown-item menu-products-item"><%= Resource.TariffSettings %></a></li>
                    <% } %>
                </ul>
            </div>
        <% } %>
    </asp:PlaceHolder>

    <% if (!DisableSearch)
       { %>
        <div id="studio_searchPopupPanel" class="studio-action-panel">
            <div class="dropdown-content">
                <div class="search-input-box">
                    <input type="search" id="studio_search" class="search-input textEdit" placeholder="<%= UserControlsCommonResource.SearchHld %>" maxlength="255" data-url="<%= VirtualPathUtility.ToAbsolute("~/search.aspx") %>" />
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
                    <span class="dropdown-item" onclick=" StudioBlockUIManager.blockUI('#aboutCompanyPopup', 680, 600);jq('.studio-action-panel').hide(); "><%= Resource.AboutCompanyTitle %> </span>
                    <div id="aboutCompanyPopup" class="confirmation-popup" style="display: none;">
                        <sc:Container ID="aboutCompanyPopupContainer" runat="server">
                            <Header>
                                <span><%= Resource.AboutCompanyTitle %></span>
                            </Header>
                            <Body>
                                <img class="confirmation-popup_logo" src="<%= ConfirmationLogo %>" />                                                               
                                <div class="confirmation-popup_version gray-text">
                                    <%= string.IsNullOrEmpty(VersionNumber) ? "" : Resource.AboutCompanyVersion + " " + VersionNumber %>
                                </div>
                                <div class="confirmation-popup_licensor"><%= Resource.AboutCompanyLicensor %></div>
                                <div class="confirmation-popup_name">Ascensio System SIA</div>
                                <ul class="confirmation-popup_info">
                                    <li><span class="gray-text"><%= Resource.AboutCompanyAddressTitle %>: 
                                        </span>Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021</li>
                                    <li><span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: 
                                        </span><a href="mailto:support@onlyoffice.com" class="link">support@onlyoffice.com</a></li>
                                    <li><span class="gray-text"><%= Resource.AboutCompanyTelTitle %>: 
                                        </span>+371 660-16425</li>
                                    <li><a href="http://www.onlyoffice.com" target="_blank" class="link">www.onlyoffice.com</a></li>
                                </ul>

                                <% if (IsAuthorizedPartner.HasValue && IsAuthorizedPartner.Value && Partner != null)
                                   { %>
                                    <br />
                                    <div class="confirmation-popup_licensor"><%= Resource.AboutCompanySublicensee %></div>
                                    <div class="confirmation-popup_name"><%= (Partner.DisplayName ?? Partner.CompanyName).HtmlEncode() %></div>
                                    <ul class="confirmation-popup_info">
                                        <% if (!string.IsNullOrEmpty(Partner.Address))
                                           { %>
                                            <li><span class="gray-text"><%= Resource.AboutCompanyAddressTitle %>: 
                                                </span><%= Partner.Address.HtmlEncode() %></li>
                                        <% } %>
                                        <% if (!string.IsNullOrEmpty(Partner.SupportEmail))
                                           { %>
                                            <li><span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: 
                                                </span><a href="mailto:<%= Partner.SupportEmail %>" class="link"><%= Partner.SupportEmail %></a></li>
                                        <% }
                                           else if (!string.IsNullOrEmpty(Partner.Email))
                                           { %>
                                            <li><span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: 
                                                </span><a href="mailto:<%= Partner.Email %>" class="link"><%= Partner.Email %></a></li>
                                        <% } %>
                                        <% if (!string.IsNullOrEmpty(Partner.Phone))
                                           { %>
                                            <li><span class="gray-text"><%= Resource.AboutCompanyTelTitle %>: 
                                                </span><%= Partner.Phone.HtmlEncode() %></li>
                                        <% } %>
                                        <% if (!string.IsNullOrEmpty(Partner.Url))
                                           { %>
                                            <li><a href="<%= Partner.Url.StartsWith("http:") || Partner.Url.StartsWith("https:") ? Partner.Url : string.Concat("http://", Partner.Url) %>" target="_blank" class="link"><%= Partner.Url %></a></li>
                                        <% } %>
                                    </ul>
                                <% } %>

                                <% if (CoreContext.Configuration.Standalone && false)
                                   { %>
                                    <br />
                                    <div class="confirmation-popup_licensor"><%= Resource.AboutCompanyLicensee %></div>
                                    <% var license = LicenseReader.GetLicense();
                                       if (license != null)
                                       { %>
                                        <% if (!string.IsNullOrEmpty(license.getCustomerLogo()))
                                           { %>
                                            <br />
                                            <br />
                                            <img alt="" src="<%= license.getCustomerLogo() %>"/>
                                        <% } %>
                                        <div class="confirmation-popup_name"><%= license.getCustomer() %></div>
                                        <ul class="confirmation-popup_info">
                                            <% if (!string.IsNullOrEmpty(license.getCustomerAddr()))
                                               { %>
                                                <li>
                                                    <span class="gray-text"><%= Resource.AboutCompanyAddressTitle %>: </span>
                                                    <%= license.getCustomerAddr() %>
                                                </li>
                                            <% } %>
                                            <% if (!string.IsNullOrEmpty(license.getCustomerMail()))
                                               { %>
                                                <li>
                                                    <span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: </span>
                                                    <a href="mailto:<%= license.getCustomerMail() %>" class="link"><%= license.getCustomerMail() %></a>
                                                </li>
                                            <% } %>
                                            <% if (!string.IsNullOrEmpty(license.getCustomerWww()))
                                               { %>
                                                <li>
                                                    <a href="<%= license.getCustomerWww() %>" target="_blank" class="link"><%= license.getCustomerWww() %></a>
                                                </li>
                                            <% } %>
                                            <% if (!string.IsNullOrEmpty(license.getCustomerInfo()))
                                               { %>
                                                <li>
                                                    <%= license.getCustomerInfo() %>
                                                </li>
                                            <% } %>
                                            <% if (license.getUserQuota() > 0)
                                               { %>
                                                <li>
                                                    <span class="gray-text"><%= Resource.AboutCompanyUserTitle %>: </span>
                                                    <%= license.getUserQuota() %>
                                                </li>
                                            <% } %>
                                            <li>
                                                <% if (license.getEndDate() >= DateTime.UtcNow)
                                                   { %>
                                                    <span class="bold"><%= string.Format(Resource.AboutCompanyValidDate,
                                                                                         TenantUtil.DateTimeFromUtc(license.getStartDate()).ToShortDateString(),
                                                                                         TenantUtil.DateTimeFromUtc(license.getEndDate()).ToShortDateString()) %></span>
                                                <% }
                                                   else
                                                   { %>
                                                    <span class="bold red-text"><%= string.Format(Resource.AboutCompanyOverduedDate, TenantUtil.DateTimeFromUtc(license.getEndDate()).ToShortDateString()) %></span>
                                                <% } %>
                                            </li>
                                        </ul>
                                    <% }
                                       else
                                       { %>
                                        <div class="confirmation-popup_name red-text"><%= UserControlsCommonResource.TariffLicenseOver %></div>
                                    <% } %>
                                <% } %>
                            </Body>
                        </sc:Container>
                    </div>
                </li>
                <% if (DebugInfo.ShowDebugInfo) %>
                <%
                   { %>
                    <li>
                        <span class="dropdown-item" onclick=" StudioBlockUIManager.blockUI('#debugInfoPopUp', 1000, 300, -300);jq('.studio-action-panel').hide(); ">
                            Debug Info
                        </span>
                        <div id="debugInfoPopUp" style="display: none">
                            <sc:Container ID="debugInfoPopUpContainer" runat="server">
                                <Header>
                                    <span>Debug Info</span>
                                </Header>
                                <Body>
                                    <div style="height: 500px; overflow-y: scroll;">
                                        <%= DebugInfo.DebugString.HtmlEncode().Replace("\n\r", "<br/>").Replace("\n", "<br/>") %>
                                    </div>
                                    <div class="middle-button-container">
                                        <a class="button blue middle" href="javascript:void(0)" onclick=" jq.unblockUI(); ">Ok</a>
                                    </div>
                                </Body>
                            </sc:Container>
                        </div>
                    </li>
                <% } %>

                <%--Logout--%>
                <% if (!(CoreContext.Configuration.Personal && CoreContext.Configuration.Standalone)) { %>
                <li><a class="dropdown-item" href="<%= CommonLinkUtility.Logout %>">
                        <%= UserControlsCommonResource.LogoutButton %></a></li>
                <% } %>
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
            <a class="see-all-btn" href="<%= VirtualPathUtility.ToAbsolute("~/feed.aspx") %>">
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

    <div id="studio_dropVoipPopupPanel" class="studio-action-panel">
        <asp:PlaceHolder runat="server" ID="_voipPhonePlaceholder"></asp:PlaceHolder>
    </div>

    <asp:PlaceHolder runat="server" ID="_customNavControls"></asp:PlaceHolder>
</div>