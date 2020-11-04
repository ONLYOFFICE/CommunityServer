<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="My.aspx.cs" Inherits="ASC.Web.Studio.MyStaff" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu my">
        <span class="header text-overflow" title="<%= PageTitle.HtmlEncode() %>"><%= PageTitle.HtmlEncode() %></span>

        <% if (IsAdmin() || Helper.UserInfo.IsMe())
           { %>
            <% if (Helper.UserInfo.IsLDAP())
               { %>
            <span class="ldap-lock-big" title="<%= Resource.LdapUsersListLockTitle %>"></span>
            <% }
               if (Helper.UserInfo.IsSSO())
               { %>
            <span class="sso-lock-big" title="<%= Resource.SsoUsersListLockTitle %>"></span>
            <% } %>
        <% } %>

        <% if (!EditProfileFlag)
           { %>
        <asp:PlaceHolder ID="actionsHolder" runat="server" />
        <% } %>
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <% if (!EditProfileFlag)
       { %>
    <div class="profile-container my">
        <asp:PlaceHolder runat="server" ID="_contentHolderForProfile"/>
        <% if (!IsPersonal)
           { %>
        <div id="subscriptionBlockContainer" class="user-block">
            <div class="tabs-section">
                <span class="header-base"><%= Resource.Subscriptions %></span>
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
        <% } %>
    </div>
    <% }
       else
       { %>
    <div class="profile-container my">
        <asp:PlaceHolder runat="server" ID="_contentHolderForEditForm"/>
    </div>
    <% } %>
</asp:Content>
