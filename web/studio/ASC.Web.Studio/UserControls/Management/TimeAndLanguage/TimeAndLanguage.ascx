<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeAndLanguage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TimeAndLanguage" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix langTimeZoneBlock">
    <div class="header-base-small headertitle">
        <%= Resource.Language %>:
        <% if (ShowHelper) { %>
        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'NotFoundLanguage'});"></span>
        <% } %>
    </div>
    <div class="timeandlangText">
        <%=RenderLanguageSelector()%>
    </div>
    <div class="header-base-small headertitle two">
        <%= Resource.TimeZone %>:
    </div>
    <div class="timeandlangText">
        <%= RenderTimeZoneSelector() %>
    </div>
    <% if (ShowHelper) { %>
    <div class="popup_helper" id="NotFoundLanguage">
        <p>
            <%= string.Format(Resource.NotFoundLanguage.HtmlEncode(), "<a href=\"mailto:documentation@onlyoffice.com\">", "</a>") %>
             <% if (!string.IsNullOrEmpty(HelpLink))
             { %>
            <a href="<%= HelpLink + "/guides/become-translator.aspx" %>" target="_blank">
                <%= Resource.LearnMore %></a>
            <% } %>
        </p>
    </div>
    <% } %>
</div>
<% if (!WithoutButton)
   { %>
<div class="middle-button-container">
    <a class="button blue" onclick="TimeAndLanguage.SaveLanguageTimeSettings();"
        href="javascript:void(0);">
        <%= Resource.SaveButton %></a>
</div>
<% } %>