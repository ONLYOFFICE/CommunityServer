<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PortalAnalytics.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Statistics.PortalAnalytics" %>
<%@ Import Namespace="Resources" %>

<% if (Enabled)
   { %>
<div id="portalAnalyticsView" class="clearFix">
    <div class="settings-block">
        <div class="header-base clearFix">
            <div class="title"><%= Resource.PortalAnalyticsTitle %></div>
        </div>
        <div class="clearFix">
            <div class="clearFix">
                <input id="portalAnalyticsOn" type="radio" <%= SwitchedOn ? "checked=\"checked\"" : "" %> name="portalAnalytics" />
                <label for="portalAnalyticsOn"><%= Resource.IPSecurityEnable %></label>
            </div>
            <div class="clearFix">
                <input id="portalAnalyticsOff" type="radio" <%= !SwitchedOn ? "checked=\"checked\"" : "" %> name="portalAnalytics" />
                <label for="portalAnalyticsOff"><%= Resource.IPSecurityDisable %></label>
            </div>
        </div>
        <div class="middle-button-container">
            <a class="button blue"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= string.Format(Resource.HelpAnswerPortalAnalytics.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
    </div>
</div>
<% } %>