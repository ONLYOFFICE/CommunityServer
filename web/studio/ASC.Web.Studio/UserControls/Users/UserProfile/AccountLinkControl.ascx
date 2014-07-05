<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountLinkControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.AccountLinkControl" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>

<% if (SettingsView) { %>
    <div id="accountLinks" class="tabs-content">
        <ul class="clearFix">
        <% foreach (var acc in Infos) { %>
            <li class="<%=acc.Provider%><%=acc.Linked ? " connected" : ""%><%=SettingsView ? "" : " float-left"%>">
                <span class="label"></span>
                <span class="<%=acc.Linked ? "linked" : ""%>">
                    <%= acc.Linked
                        ? Resources.Resource.AssociateAccountConnected
                        : Resources.Resource.AssociateAccountNotConnected%>.
                </span> 
                <a href="<%=acc.Url%>" class="popup <%=acc.Linked ? "linked" : ""%>" id="<%=acc.Provider%>">
                    <%= acc.Linked
                        ? Resources.Resource.AssociateAccountDisconnect
                        : Resources.Resource.AssociateAccountConnect%>
                </a>
            </li>
        <% } %>
        </ul>
    </div>

<% } else if(InviteView) {%>

    <div id="social" class="invite">
    <center>
        <div id="accountLinks">
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
    <div id="accountLinks">
        <ul class="clearFix">
            <% foreach (var acc in Infos) { %>
                <li><a href="<%=acc.Url %>" class="<%= !MobileDetector.IsMobile ? "popup" : "" %> <%=acc.Provider%> <%=acc.Linked?"linked":""%>" id="<%=acc.Provider%>"></a>
                </li>
            <% } %>
        </ul>
    </div>
<% } %>