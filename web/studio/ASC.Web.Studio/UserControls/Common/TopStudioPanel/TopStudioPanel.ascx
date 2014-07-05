<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopStudioPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.TopStudioPanel" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Tenants" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Statistics" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="studio-top-panel mainPageLayout  <% if (CoreContext.Configuration.Personal) { %>try-welcome-top <% } %>">
    <ul>
        <li class="studio-top-logo ">
            <a class="top-logo" title="<%= Resource.TeamLabOfficeTitle %>" href="<%= CommonLinkUtility.GetDefault() %>">
                <img alt="" src="<%= GetAbsoluteCompanyTopLogoPath() %>" />
                <% if( IsAutorizePartner.HasValue && Partner != null){ %>
                <span class="top-logo-partner_name">
                    <%= IsAutorizePartner.Value ? Resource.For + " " + (Partner.DisplayName ?? Partner.CompanyName) : Resource.HostedNonAuthorizedVersion %></span>
                <%} %>
            </a>
        </li>

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
                    <span class="usr-prof" title="<%= ASC.Core.Users.UserInfoExtension.DisplayUserName(CurrentUser) %>">
                        <%= ASC.Core.Users.UserInfoExtension.DisplayUserName(CurrentUser) %>
                    </span>
                </span>
            </li>
            <%= RenderCustomNavigation() %>
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
                <a class="inner-text" href="<%= CommonLinkUtility.GetAdministration(ManagementType.General) %>" title="<%= Resource.Administration %>"></a>
            </li>
        <% } %>

        <% if (!DisableTariff && !CoreContext.Configuration.Standalone)
           { %>
            <li class="top-item-box tariffs <%if (DisplayTrialCountDays){ %>has-led<%} %>">
                <a class="inner-text" href="<%= TenantExtra.GetTariffPageLink() %>" title="<%= Resource.TariffSettings %>">
                     <%if (DisplayTrialCountDays){ %>
                    <span class="inner-label">trial <%= TrialCountDays %></span>
                    <%} %>
                </a>
            </li>
        <% } %>
        
        <% if (!DisableVideo)
           { %>
            <li class="top-item-box video">
                <a class="videoActiveBox inner-text" href="<%= AllVideoLink %>" target="_blank" title="<%= Resource.VideoGuides %>" data-videoUrl="<%= AllVideoLink %>">
                    <span class="inner-label"></span>
                </a>
            </li>
        <% } %>
        
        <li class="clear"></li>
    </ul>
    
    <asp:PlaceHolder runat="server" ID="_productListHolder">
        <% if (DisplayModuleList)
           { %>
            <div id="studio_productListPopupPanel" class="studio-action-panel modules">
                <div class="corner-top right"></div>
                <ul class="dropdown-content">
                    <asp:Repeater runat="server" ID="_productRepeater" ItemType="ASC.Web.Core.IWebItem">
                        <ItemTemplate>
                            <li class="<%# Item.ProductClassName + (Item.IsDisabled() ? " display-none" : string.Empty) %>">
                                <a href="<%# VirtualPathUtility.ToAbsolute(Item.StartURL) %>" class="dropdown-item menu-products-item">
                                    <%# (Item.Name).HtmlEncode() %>
                                </a>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                    <li class="dropdown-item-seporator"></li>
                    <asp:Repeater runat="server" ID="_addonRepeater" ItemType="ASC.Web.Core.IWebItem">
                        <ItemTemplate>
                            <li class="<%# Item.ProductClassName + (Item.IsDisabled() ? " display-none" : string.Empty) %>">
                                <a href="<%# VirtualPathUtility.ToAbsolute(Item.StartURL) %>" class="dropdown-item menu-products-item">
                                    <%# (Item.Name).HtmlEncode() %>
                                </a>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <li class="feed"><a href="<%= VirtualPathUtility.ToAbsolute("~/feed.aspx") %>" class="dropdown-item menu-products-item"><%= UserControlsCommonResource.FeedTitle %></a></li>

                    <% if (IsAdministrator)
                       { %>
                        <li class="dropdown-item-seporator"></li>
                        <li class="settings"><a href="<%= CommonLinkUtility.GetAdministration(ManagementType.General) %>" title="<%= Resource.Administration %>" class="dropdown-item menu-products-item"><%= Resource.Administration %></a></li>
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
            <div class="corner-top left"></div>
            <div class="dropdown-content">
                <div class="search-input-box">
                    <input type="search" id="studio_search" class="search-input textEdit" placeholder="<%= UserControlsCommonResource.SearchHld %>" maxlength="255" data-url="<%= VirtualPathUtility.ToAbsolute("~/search.aspx") %>" />
                    <button class="button blue search-btn"></button>
                </div>
                <div class="header-base small bold"><%= UserControlsCommonResource.SeeInModulesHdr %></div>
                <div class="search-options-box clearFix">
                    <% foreach (var product in SearchProducts)
                       { %>
                    <div class="search-option-box">
                        <label>
                            <input id="searchfield_<%=product.ProductClassName %>" type="checkbox" data-product-id="<%= product.ID %>"/>
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
        <div class="corner-top right"> </div>
        <ul class="dropdown-content">
            <% if (!TenantStatisticsProvider.IsNotPaid() && !CoreContext.Configuration.Personal)
               { %>
                <li>
                    <a class="dropdown-item" href="<%= CommonLinkUtility.GetMyStaff() %>">
                        <%= Resource.Profile %>
                    </a>
                </li>
            
                <%--Logout--%>
            <% } %>
            <li>
                <span class="dropdown-item" onclick="StudioBlockUIManager.blockUI('#aboutCompanyPopup', 680, 600);jq('.studio-action-panel').hide()"><%=Resource.AboutCompanyTitle %> </span>
                <div id="aboutCompanyPopup" class="confirmation-popup" style="display: none;">
                    <sc:Container ID="aboutCompanyPopupContainer" runat="server">
                            <Header>
                                <span><%=Resource.ConfirmationTitle %></span>
                            </Header>
                            <Body>                                
                                <img class="confirmation-popup_logo" src="<%=ConfirmationLogo %>" />                                                               
                                <div class="confirmation-popup_version gray-text">
                                    <%= string.IsNullOrEmpty(VersionNumber) ? "" : Resource.AboutCompanyVersion + " " + VersionNumber %>
                                </div>
                                <div class="confirmation-popup_licensor"><%=Resource.AboutCompanyLicensor %></div>
                                <div class="confirmation-popup_name">Ascensio System SIA</div>
                                <ul class="confirmation-popup_info">
                                    <li><span class="gray-text"><%=Resource.AboutCompanyAddressTitle %>: 
                                        </span>Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021</li>
                                   <li><span class="gray-text"><%=Resource.AboutCompanyEmailTitle %>: 
                                        </span><a href="mailto:support@onlyoffice.com" class="link">support@onlyoffice.com</a></li>
                                    <li><span class="gray-text"><%=Resource.AboutCompanyTelTitle %>: 
                                        </span>+371 660-16425</li>
                                    <li><a href="http://www.onlyoffice.com" target="_blank" class="link">www.onlyoffice.com</a></li>
                                </ul>

                                 <% if (IsAutorizePartner.HasValue && IsAutorizePartner.Value && Partner != null)
                                    { %>
                                <br />
                                <div class="confirmation-popup_licensor"><%= Resource.AboutCompanySublicensee %></div>
                                <div class="confirmation-popup_name"><%= Partner.DisplayName ?? Partner.CompanyName %></div>
                                <ul class="confirmation-popup_info">
                                    <% if (!string.IsNullOrEmpty(Partner.Address)) { %>
                                    <li><span class="gray-text"><%= Resource.AboutCompanyAddressTitle %>: 
                                        </span><%= Partner.Address %></li>
                                    <%} %>
                                    <% if (!string.IsNullOrEmpty(Partner.SupportEmail)) {%>
                                    <li><span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: 
                                        </span><a href="mailto:<%= Partner.SupportEmail %>" class="link"><%= Partner.SupportEmail %></a></li>
                                    <%}
                                       else if (!string.IsNullOrEmpty(Partner.Email)){%>
                                    <li><span class="gray-text"><%= Resource.AboutCompanyEmailTitle %>: 
                                        </span><a href="mailto:<%= Partner.Email %>" class="link"><%= Partner.Email %></a></li>
                                    <%} %>
                                    <% if (!string.IsNullOrEmpty(Partner.Phone))
                                       {%>
                                    <li><span class="gray-text"><%= Resource.AboutCompanyTelTitle %>: 
                                        </span><%= Partner.Phone%></li>
                                    <%} %>
                                    <% if (!string.IsNullOrEmpty(Partner.Url))
                                       {%>
                                    <li><a href="<%= Partner.Url %>" target="_blank" class="link"><%= Partner.Url %></a></li>
                                    <%} %>
                                </ul>
                                <%} %>

                                <!--<% if (CoreContext.Configuration.Standalone)
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
                                <% } %>-->
                            </Body>
                        </sc:Container>
                </div>
            </li>
            <% if (DebugInfo.ShowDebugInfo) %>
            <%
               { %>
                <li>
                    <a class="dropdown-item" href="javascript:void(0);" onclick=" StudioBlockUIManager.blockUI('#debugInfoPopUp', 1000, 300, -300);jq('.studio-action-panel').hide()">
                        Debug Info
                    </a>
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
            <li><a class="dropdown-item" href="<%= CommonLinkUtility.Logout %>">
                    <%= UserControlsCommonResource.LogoutButton %></a></li>
        </ul>
    </div>
    <% } %>

    <div id="studio_dropFeedsPopupPanel" class="studio-action-panel">
        <div class="corner-top right"></div>
        
        <div id="drop-feeds-box">
            <div class="list display-none"></div>
            <div class="loader-text-block"><%= FeedResource.LoadingMsg %></div>
            <div class="feeds-readed-msg"><span><%= FeedResource.FeedsReadedMsg %></span></div>
            <a class="see-all-btn" href="<%= VirtualPathUtility.ToAbsolute("~/feed.aspx") %>">
                <%= FeedResource.SeeAllFeedsBtn %>
            </a>
        </div>
    </div>
    
    <% if (!DisableVideo)
       { %>
        <div id="studio_videoPopupPanel" class="video-popup-panel studio-action-panel">
            <div class="corner-top right"></div>
            <div id="dropVideoList" class="drop-list">
                <ul class="video-list">
                    <% foreach (var data in VideoGuideItems)
                       { %>
                        <li>
                            <a class="link" id="<%= data.Id %>" href="<%= data.Link %>" target="_blank"><%= data.Title %></a>
                        </li>
                    <% } %>
                </ul>
            </div>
            <a class="video-link" href="<%= AllVideoLink %>">
                <%= Resource.SeeAllVideos %>
            </a> 
            <a id="markVideoRead" class="video-link" href="javascript:void(0);">
                <%= Resource.MarkAllAsRead %>
            </a>     
        </div>
    <% } %>
    <asp:PlaceHolder runat="server" ID="_customNavControls"></asp:PlaceHolder>
</div>