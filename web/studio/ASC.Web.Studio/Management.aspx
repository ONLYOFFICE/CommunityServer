<%@ Page MasterPageFile="~/Masters/basetemplate.master" Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="Management.aspx.cs" Inherits="ASC.Web.Studio.Management" Title="ONLYOFFICE™" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <asp:PlaceHolder ID="SettingsContainer" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="SidePanel" runat="server">
    <div class="page-menu">
        <ul class="menu-list">
            <% foreach (var module in GetNavigationList()) %>
            <% { %>
                <li class="menu-item none-sub-list <% if (CurrentModule == module) { %>active<% } %>">
                    <a class="menu-item-label outer-text text-overflow" href="<%= GetNavigationUrl(module) %>" title="<%= GetNavigationTitle(module) %>">
                        <span class="menu-item-icon <%= module.ToString().ToLowerInvariant() %>"></span>
                        <span class="menu-item-label inner-text"><%= GetNavigationTitle(module) %></span>
                    </a>
                </li>
            <% } %>

            <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
        </ul>
    </div>
</asp:Content>