<%@ Page MasterPageFile="~/Masters/BaseTemplate.master" Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="Management.aspx.cs" Inherits="ASC.Web.Studio.Management" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <asp:PlaceHolder ID="SettingsContainer" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="SidePanel" runat="server">
    <div class="page-menu">
        <ul class="menu-list">
            <% foreach (var category in GetCategoryList().Where(r => r.Modules == null || r.Modules.Any()))
               {
                   if ((category.Modules != null && DisplayModuleList(category)) || (NavigationList.Contains(category.ModuleUrl)))
                   { %>
            <li class="menu-item <%= category.Modules != null ? "" : "none-" %>sub-list 
                <%= category.Modules != null && category.Modules.Contains(CurrentModule) ? "currentCategory" : "" %>
                <%= (category.Modules == null && category.ModuleUrl == CurrentModule) ? "currentCategory active" : "" %> open-by-default">
                <%if (category.Modules != null && DisplayModuleList(category))
                  {%>
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <%} %>
                    <a class="menu-item-label outer-text text-overflow"
                        href="<%= category.GetNavigationUrl() %>" title="<%= category.Title %>">
                        <span class="menu-item-icon"> <svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/management-icons.svg#managementIcons<%= category.ClassName %>"></use></svg>  </span>
                        <span class="menu-item-label"><%= category.Title %></span>
                    </a>
                    <%if (category.Modules != null && DisplayModuleList(category))
                      {%>
                </div>

                <ul class="menu-sub-list">
                    <% foreach (var module in category.Modules) %>
                    <% { %>
                    <li class="menu-sub-item <% if (CurrentModule == module)
                                                { %>active<% } %>">
                        <a class="menu-item-label outer-text text-overflow" href="<%= category.GetNavigationUrl(module) %>" title="<%= GetNavigationTitle(module) %>">
                            <span class="menu-item-label inner-text"><%= GetNavigationTitle(module) %></span>
                        </a>
                    </li>
                    <% } %>
                </ul>
                <%} %>
            </li>
            <% } %>
            <% } %>

            <% if (TenantExtra.EnableControlPanel)
               { %>
            <li class="menu-item none-sub-list">
                <div class="category-wrapper">
                    <a class="menu-item-label outer-text text-overflow" href="<%= SetupInfo.ControlPanelUrl %>" target="_blank" title="<%= Resource.ControlPanelSettings %>">
                        <span class="menu-item-icon controlpanel">
                            <svg class="menu-item-svg">
                                <use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenucontrolpanel"></use>
                            </svg>
                        </span>
                        <span class="menu-item-label inner-text">
                            <%= Resource.ControlPanelSettings %>
                        </span>
                    </a>
                </div>
            </li>
            <% } %>

            <asp:PlaceHolder ID="InviteUserHolder" runat="server"/>

            <% if (TenantExtra.EnableTarrifSettings)
               { %>
            <li class="menu-item none-sub-list add-block">
                <div class="category-wrapper">
                    <a class="menu-item-label outer-text text-overflow" href="<%= TenantExtra.GetTariffPageLink() %>" title="<%= Resource.TariffSettings %>">
                        <span class="menu-item-icon">
                            <svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenupayments<%= CoreContext.Configuration.CustomMode ? "Rub" : "" %>"></use>
                            </svg>
                        </span>
                        <span class="menu-item-label inner-text">
                            <%= Resource.TariffSettings %>
                        </span>
                    </a>
                </div>
            </li>
            <% } %>

            <asp:PlaceHolder ID="HelpHolder" runat="server"/>
            <asp:PlaceHolder ID="SupportHolder" runat="server"/>
            <asp:PlaceHolder ID="UserForumHolder" runat="server"/>
            <asp:PlaceHolder ID="VideoGuides" runat="server"/>
        </ul>
    </div>
    <% if (!string.IsNullOrEmpty(SetupInfo.UserVoiceURL))
       { %>
    <script type="text/javascript" src="<%= SetupInfo.UserVoiceURL %>"></script>
    <% } %>
</asp:Content>
