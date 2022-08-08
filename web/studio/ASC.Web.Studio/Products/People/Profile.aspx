<%@ Assembly Name="ASC.Web.People" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="ASC.Web.People.Profile" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= ProfileHelper.UserInfo.DisplayUserName(true) %>"><%= ProfileHelper.UserInfo.DisplayUserName(true) %></span>
        
        <% if (IsAdmin() || ProfileHelper.UserInfo.IsMe())
        { %>
            <% if (ProfileHelper.UserInfo.IsLDAP())
            { %>
            <span class="ldap-lock-big" title="<%= Resource.LdapUsersListLockTitle %>"></span>
            <% }
            if (ProfileHelper.UserInfo.IsSSO())
            { %>
            <span class="sso-lock-big" title="<%= Resource.SsoUsersListLockTitle %>"></span>
            <% } %>
        <% } %>

        <asp:PlaceHolder ID="actionsHolder" runat="server" />
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PeoplePageContent" runat="server">
    <div class="profile-container">
        <asp:PlaceHolder ID="CommonContainerHolder" runat="server" />
    </div>
    <% if (ProfileHelper.UserInfo.IsMe())
       { %>
        <div id="subscriptionBlockContainer" class="user-block">
            <div class="tabs-section">
                <span class="header-base"><%= PeopleResource.LblSubscriptions %></span>
                <span id="switcherSubscriptionButton" class="toggle-button"
                      data-switcher="1" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
                    <%= Resource.Show %>
                </span>
            </div>
            <div id="subscriptionContainer" style="display: none;" class="tabs-content">
                <asp:PlaceHolder ID="_phSubscriptionView" runat="server" />
            </div>
        </div>
        <asp:PlaceHolder ID="_phTipsSettingsView" runat="server" />

        <div id="connectionsBlockContainer" class="user-block">
            <div class="tabs-section">
                <span class="header-base"><%= PeopleResource.LblActiveConnections %></span>
                <% if (IsEmptyDbip) { %>
                <span id="emptyDbipSwitcher"class="HelpCenterSwitcher expl"></span>
                <div id="emptyDbipHelper"class="popup_helper">
                    <%= Resource.GeolocationNotAvailable %>
                    <% if (!string.IsNullOrEmpty(HelpLink)) { %>
                    <a href="<%= HelpLink + "/administration/active-connections.aspx" %>" target="_blank"><%= Resource.LearnMore %></a>
                    <% } %>
                </div>
                <% } %>
                <span id="switcherConnectionsButton" class="toggle-button"
                      data-switcher="1" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
                    <%= Resource.Show %>
                </span>
            </div>
            <div id="connectionsContainer" style="display: none;" class="tabs-content">
                <asp:PlaceHolder ID="_phConnectionsView" runat="server" />
            </div>
        </div>
    <% } %>
</asp:Content>