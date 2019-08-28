<%@ Assembly Name="ASC.Web.People" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SideNavigationPanel.ascx.cs" Inherits="ASC.Web.People.UserControls.SideNavigationPanel" %>
<%@ Import Namespace="ASC.Web.People" %>
<%@ Import Namespace="ASC.Web.People.Classes" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.ThirdParty.ImportContacts" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>


<!-- is first button -->
<button style="display:none">&nbsp;</button>

<div class="page-menu">
    <% if (CurrentUserAdmin)
       { %>
    <ul class="menu-actions">
        <li id="menuCreateNewButton" class="menu-main-button without-separator middle" title="<%=PeopleResource.LblCreateNew%>">
            <span id="menuCreateNewButtonText" class="main-button-text"><%=PeopleResource.LblCreateNew%></span>
            <span class="white-combobox"></span>
        </li>
        <li id="menuOtherActionsButton" class="menu-gray-button" title="<%= Resource.MoreActions %>">
            <span class="btn_other-actions">...</span>
        </li>
    </ul>

    <div id="createNewButton" class="studio-action-panel">
        <ul class="dropdown-content">
            <li>
                <% if (EnableAddUsers)
                   { %>
                <a class="dropdown-item add-profile" href="profileaction.aspx?action=create&type=user"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></a>
                <% }
                   else
                   { %>
                <span class="dropdown-item disable"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></span>
                <% } %>
            </li>
            <li><a class="dropdown-item add-profile" href="profileaction.aspx?action=create&type=guest"><%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %></a></li>
            <li><a class="dropdown-item add-group"><%= CustomNamingPeople.Substitute<Resource>("Department").HtmlEncode() %></a></li>
        </ul>
    </div>

    <div id="otherActions" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="dropdown-item invite-link" id="sideNavInviteLink"><%= PeopleResource.InviteLink %></a></li>
            <li><a class="dropdown-item add-profiles"><%= PeopleResource.ImportPeople %></a></li>
            <% if (HasPendingProfiles)
               { %>
            <li><a class="dropdown-item send-invites"><%= PeopleResource.LblResendInvites %></a></li>
            <% } %>
        </ul>
    </div>
    <% } %>

    <div id="groupListContainer">
        <ul class="menu-list">
            <li class="menu-item none-sub-list open <%= Page is Help ? "" : "currentCategory active" %>" data-id="@persons">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a id="defaultLinkPeople" class="menu-item-label outer-text text-overflow" title="<%= CustomNamingPeople.Substitute<Resource>("Departments").HtmlEncode() %>" href="/products/people/#sortorder=ascending">
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
                <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) %>">
                    <span class="menu-item-icon settings"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg></span>
                    <span class="menu-item-label inner-text gray-text"><%= PeopleResource.Settings %></span>
                </a>
            </div>
            <ul class="menu-sub-list">
                <li id="menuAccessRights" class="menu-sub-item filter">
                    <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) +"#people" %>">
                        <%= PeopleResource.AccessRightsSettings %>
                    </a>
                </li>
            </ul>
        </li>
        <% } %>
        <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
    </ul>
    <% if (CurrentUserAdmin && ASC.Web.Studio.ThirdParty.ImportContacts.Import.Enable)
       { %>
    <div class="people-import-banner">
        <div class="people-import-banner_text"><%= PeopleResource.ImportPeople %></div>
        <div class="people-import-banner_img"></div>
    </div>
    <% } %>
</div>
