<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountLinkControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.AccountLinkControl" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>

<% if (SettingsView) { %>
    <div class="account-links tabs-content"></div>

<% } else if(InviteView) {%>

<div id="social" class="invite">
    <center>
        <div class="account-links">
            <div class="info">
                <div style="width:100%;text-align:right;"><%= Resources.Resource.LoginWithAccount%></div>
            </div>
            <div class="float-left">
                <ul class="clearFix">
                    <% foreach (var acc in GetLinkableProviders()) { %>
                    <li class="float-left">
                        <a href="<%=acc.Url %>" class="popup <%=acc.Provider%>" id="<%=acc.Provider%>"></a>
                    </li>
                    <% }%>
                </ul>
            </div>
        </div>
    </center>
</div>

<% } else { %>
    <div class="account-links">
        <ul class="clearFix">
            <% foreach (var acc in Infos) { %>
                <li>
                    <a href="<%=acc.Url %>" class="<%= !MobileDetector.IsMobile ? "popup " : "" %> <%=acc.Provider%> <%=acc.Linked?" linked":""%>" id="<%=acc.Provider%>"></a>
                </li>
            <% } %>
        </ul>
    </div>
<% } %>