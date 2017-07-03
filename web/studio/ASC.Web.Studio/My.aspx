<%@ Page Language="C#" MasterPageFile="~/Masters/basetemplate.master" AutoEventWireup="true" CodeBehind="My.aspx.cs" Inherits="ASC.Web.Studio.MyStaff" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <style type="text/css" rel="stylesheet">
        .profile-container {
            margin-left: 288px;
        }
    </style>

    <%if (!EditProfileFlag)
      {%>
    <style type="text/css" rel="stylesheet">
        .profile-title {
            margin-left: 288px;
        }
    </style>
    <div class="clearFix profile-title header-with-menu">
         <span class="header text-overflow" title="<%= UserName %>"><%= UserName %></span>
        <% if (IsAdmin() || _helper.UserInfo.IsMe())
        { %>
            <% if (_helper.UserInfo.IsLDAP())
            { %>
            <span class="ldap-lock-big" title="<%= Resource.LdapUsersListLockTitle %>"></span>
            <% }
                else if (_helper.UserInfo.IsSSO())
                { %>
            <span class="sso-lock-big" title="<%= Resource.SsoUsersListLockTitle %>"></span>
            <% } %>
        <% } %>
        <asp:PlaceHolder ID="actionsHolder" runat="server" />
    </div>
    <div class="profile-container">
        <asp:PlaceHolder runat="server" ID="_contentHolderForProfile"></asp:PlaceHolder>
        <% if (!isPersonal)
           { %>
        <div id="subscriptionBlockContainer" class="user-block">
            <div class="tabs-section">
                <span class="header-base"><%= Resource.Subscriptions%></span>
                <span id="switcherSubscriptionButton" class="toggle-button"
                    data-switcher="1" data-showtext="<%=Resource.Show%>" data-hidetext="<%=Resource.Hide%>">
                    <%=Resource.Show%>
                </span>
            </div>
            <div id="subscriptionContainer" style="display: none;" class="tabs-content">
                <asp:PlaceHolder ID="_phSubscriptionView" runat="server" />
            </div>
        </div>
        <asp:PlaceHolder ID="_phTipsSettingsView" runat="server" />
        <%} %>
    </div>
    <%}
      else
      { %>
    <div class="profile-container">
        <asp:PlaceHolder runat="server" ID="_contentHolderForEditForm"></asp:PlaceHolder>
    </div>
    <%} %>
</asp:Content>
