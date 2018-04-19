<%@ Assembly Name="ASC.Web.People" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SideNavigationPanel.ascx.cs" Inherits="ASC.Web.People.UserControls.SideNavigationPanel" %>
<%@ Import Namespace="ASC.Web.People" %>
<%@ Import Namespace="ASC.Web.People.Classes" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.ThirdParty.ImportContacts" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

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
<% } %>
  <div id="createNewButton" class="studio-action-panel">
    <ul class="dropdown-content">
      <li>
        <% if (EnableAddUsers) { %>
          <a class="dropdown-item add-profile" href="profileaction.aspx?action=create&type=user"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></a>
        <% } else { %>
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

  <ul class="menu-list">
    <%if (Profiles.Count > 0)
      {%>
      <li class="menu-item <%= (Groups.Count == 0) ? "none-" : ""%>sub-list <%= Page is Help ? "" : "currentCategory active" %>" data-id="@persons">
        <div class="category-wrapper">
          <%if (Groups.Count > 0)
            {%>
          <span class="expander"></span>
          <% } %>
          <a id="defaultLinkPeople" class="menu-item-label outer-text text-overflow" title="<%= CustomNamingPeople.Substitute<Resource>("Departments").HtmlEncode() %>" href="#sortorder=ascending">
            <span class="menu-item-icon people"></span>
            <span class="menu-item-label inner-text"><%= CustomNamingPeople.Substitute<Resource>("Departments").HtmlEncode() %></span>
          </a>
        </div>
    <%}%>
    <asp:Repeater ID="GroupRepeater" runat="server">
      <HeaderTemplate>
        <ul id="groupList" class="menu-sub-list">
      </HeaderTemplate>
      <ItemTemplate>
        <li class="menu-sub-item" data-id="<%#((MyGroup)Container.DataItem).Id%>">
          <a class="menu-item-label outer-text text-overflow" title="<%#((MyGroup)Container.DataItem).Title%>" href="#sortorder=ascending&status=active&group=<%# ((MyGroup)Container.DataItem).Id%>"><%#((MyGroup)Container.DataItem).Title%></a>
        </li>
      </ItemTemplate>
      <FooterTemplate>
        </ul>
      </FooterTemplate>
    </asp:Repeater>
    <%if (Groups.Count > 0)
      {%>
      </li>
    <%}%>
    <%--
    <li class="menu-item sub-list" data-id="@freelancers">
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
    </li>
    --%>

    <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>
    <% if (CurrentUserFullAdmin)
       { %>
        <li id="menuSettings" class="menu-item sub-list add-block">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) %>">
                    <span class="menu-item-icon settings"></span>
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
    <% if (CurrentUserAdmin && Import.Enable)
       { %>
    <div class="people-import-banner">
        <div class="people-import-banner_text"><%= PeopleResource.ImportPeople %></div>
        <div class="people-import-banner_img"></div>
    </div>
    <% } %>
</div>
