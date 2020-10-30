<%@ Assembly Name="ASC.Web.People" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SideButtonsPanel.ascx.cs" Inherits="ASC.Web.People.UserControls.SideButtonsPanel" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>


<!-- is first button -->
<button style="display:none">&nbsp;</button>

<% if (CurrentUserAdmin)
    { %>

<div class="page-menu">

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
                <a class="dropdown-item add-profile" href="ProfileAction.aspx?action=create&type=user"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></a>
                <% }
                   else
                   { %>
                <span class="dropdown-item disable"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></span>
                <% } %>
            </li>
            <li><a class="dropdown-item add-profile" href="ProfileAction.aspx?action=create&type=guest"><%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %></a></li>
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

</div>

<% } %>
