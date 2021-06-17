<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountLinkControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.AccountLinkControl" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<% if (SettingsView)
   { %>
<div id="accountLinks"></div>
<% }
   else
   { %>

<% if (InviteView)
   { %>
<div id="social">
    <div><%= Resource.LoginWithAccount %></div>
<% } %>

<ul class="account-links">
    <% foreach (var acc in Infos)
        { %>
    <li class="float-left">
        <a <%if (EnableOauth) {%>href="<%= acc.Url %>" <%} %> class="<%= !MobileDetector.IsMobile && !Request.DesktopApp() ? "popup " : "" %> <%= acc.Provider %> <%= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName %><%if (!EnableOauth || RenderDisabled) {%> disabled <%}%> " id="<%= acc.Provider %>">
            <span class="icon"></span>
        </a>
    </li>
    <% } %>
</ul>

<% if (InviteView)
   { %>
</div>
<% } %>

<% } %>