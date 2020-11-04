<%@ Assembly Name="ASC.Web.People" %>

<%@ Page Language="C#" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" AutoEventWireup="true" CodeBehind="ProfileAction.aspx.cs" Inherits="ASC.Web.People.ProfileAction" %>

<%@ Import Namespace="ASC.Core.Users" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= PageTitle.HtmlEncode() %>"><%= PageTitle.HtmlEncode() %></span>
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
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PeoplePageContent" runat="server">
    <asp:PlaceHolder runat="server" ID="_contentHolderForEditForm"/>
</asp:Content>
