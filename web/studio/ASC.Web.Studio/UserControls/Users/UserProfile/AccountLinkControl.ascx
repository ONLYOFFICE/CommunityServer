<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountLinkControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.AccountLinkControl" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="Resources" %>

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
        <a href="<%= acc.Url %>" class="<%= !MobileDetector.IsMobile && !Request.DesktopApp() ? "popup " : "" %> <%= acc.Provider %> <%= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName %>" id="<%= acc.Provider %>">
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