<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HelpCenter.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.HelpCenter.HelpCenter" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<% if (HelpCenterItems != null)
   { %>

<% if (IsSideBar)
   { %>

<li class="menu-item sub-list help-center add-block">
    <div class="category-wrapper">
        <span class="expander"></span>
        <a class="menu-item-label outer-text text-overflow" href="<%= HelpLink %>" title="<%= Resource.HelpCenter %>">
            <span class="menu-item-icon help"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuhelp"></use></svg></span>
            <span class="menu-item-label inner-text"><%= Resource.HelpCenter %></span>
        </a>
    </div>
    <ul class="menu-sub-list">
        <% for (var i = 0; i < HelpCenterItems.Count; i++)
           { %>
        <li class="menu-sub-item">
            <a class="menu-item-label outer-text text-overflow" href="<%= HelpLinkBlock + i %>" title="<%= HelpCenterItems[i].Title %>">
                <%= HelpCenterItems[i].Title %>
            </a>
        </li>
        <% } %>
        <asp:PlaceHolder ID="VideoGuidesSubItem" runat="server"></asp:PlaceHolder>
    </ul>
</li>

<% }
   else
   { %>

<div class="help-center-content">
    <% for (var i = 0; i < HelpCenterItems.Count; i++)
       { %>
    <div id="contentHelp-<%= i %>" class="display-none">
        <div class="header-base-big"><%= HelpCenterItems[i].Title %></div>
        <div class="help-content"><%= HelpCenterItems[i].Content %></div>
    </div>
    <% } %>
</div>

<asp:PlaceHolder runat="server" ID="MediaViewersPlaceHolder"></asp:PlaceHolder>
<% }%>

<% } %>

<% if (IsSideBar)
   { %>
<asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
<% } %>