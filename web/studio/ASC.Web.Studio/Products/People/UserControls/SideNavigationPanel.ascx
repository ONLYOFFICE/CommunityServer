<%@ Assembly Name="ASC.Web.People" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SideNavigationPanel.ascx.cs" Inherits="ASC.Web.People.UserControls.SideNavigationPanel" %>
<%@ Import Namespace="ASC.Web.People" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<div class="page-menu">

    <div id="groupListContainer">
        <ul class="menu-list">
            <li class="menu-item none-sub-list open <%= Page is Help ? "" : "currentCategory active" %>" data-id="@persons">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a id="defaultLinkPeople" class="menu-item-label outer-text text-overflow" title="<%= CustomNamingPeople.Substitute<Resource>("Departments").HtmlEncode() %>" href="/Products/People/#sortorder=ascending">
                        <span class="menu-item-icon people"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenupeople"></use></svg></span>
                        <span class="menu-item-label inner-text"><%= CustomNamingPeople.Substitute<Resource>("Departments").HtmlEncode() %></span>
                    </a>
                </div>
                <ul id="groupList" class="menu-sub-list"></ul>
            </li>
        </ul>
    </div>

    <ul class="menu-list">
        <%--<li class="menu-item sub-list" data-id="@freelancers">
          <div class="category-wrapper">
            <span class="expander"></span>
            <a class="menu-item-label outer-text text-overflow" title="Freelancers">
              <span class="menu-item-label inner-text">Freelancers</span>
            </a>
          </div>
        </li>
        <li class="menu-item sub-list" data-id="@clients">
          <div class="category-wrapper">
            <span class="expander"></span>
            <a class="menu-item-label outer-text text-overflow" title="Clients">
              <span class="menu-item-label inner-text">Clients</span>
            </a>
          </div>
        </li>--%>

        <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>

        <% if (CurrentUserFullAdmin)
           { %>
        <li id="menuSettings" class="menu-item sub-list add-block">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) %>" title="<%= PeopleResource.Settings %>">
                    <span class="menu-item-icon settings"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg></span>
                    <span class="menu-item-label inner-text gray-text"><%= PeopleResource.Settings %></span>
                </a>
            </div>
            <ul class="menu-sub-list">
                <li id="menuAccessRights" class="menu-sub-item filter">
                    <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) +"#people" %>" title="<%= PeopleResource.AccessRightsSettings %>">
                        <%= PeopleResource.AccessRightsSettings %>
                    </a>
                </li>
            </ul>
        </li>
        <% } %>
        <asp:PlaceHolder ID="HelpHolder" runat="server"/>
        <asp:PlaceHolder ID="SupportHolder" runat="server"/>
        <asp:PlaceHolder ID="UserForumHolder" runat="server"/>
        <asp:PlaceHolder ID="VideoGuides" runat="server"/>
    </ul>

    <% if (CurrentUserAdmin && ASC.Web.Studio.ThirdParty.ImportContacts.Import.Enable)
        { %>
    <div class="people-import-banner">
        <div class="people-import-banner_text"><%= PeopleResource.ImportPeople %></div>
        <div class="people-import-banner_img"></div>
    </div>
    <% } %>

</div>