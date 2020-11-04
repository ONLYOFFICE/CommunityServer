<%@ Assembly Name="ASC.Web.People" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="ASC.Web.People.Profile" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= ProfileHelper.UserInfo.DisplayUserName(true) %>"><%= ProfileHelper.UserInfo.DisplayUserName(true) %></span>
        
        <% if (IsAdmin() || ProfileHelper.UserInfo.IsMe())
        { %>
            <% if (ProfileHelper.UserInfo.IsLDAP())
            { %>
            <span class="ldap-lock-big" title="<%= Resources.Resource.LdapUsersListLockTitle %>"></span>
            <% }
            if (ProfileHelper.UserInfo.IsSSO())
            { %>
            <span class="sso-lock-big" title="<%= Resources.Resource.SsoUsersListLockTitle %>"></span>
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
                      data-switcher="1" data-showtext="<%= Resources.Resource.Show %>" data-hidetext="<%= Resources.Resource.Hide %>">
                    <%= Resources.Resource.Show %>
                </span>
            </div>
            <div id="subscriptionContainer" style="display: none;" class="tabs-content">
                <asp:PlaceHolder ID="_phSubscriptionView" runat="server" />
            </div>
        </div>
        <asp:PlaceHolder ID="_phTipsSettingsView" runat="server" />
    <% } %>
</asp:Content>