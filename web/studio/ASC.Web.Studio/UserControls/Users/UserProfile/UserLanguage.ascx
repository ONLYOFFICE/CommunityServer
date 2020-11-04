<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserLanguage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserLanguage" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="System.Globalization" %>

<div class="field">
    <span class="field-title describe-text "><%= Resource.Language %>:</span>
    <span class="field-value usrLang <%= CultureInfo.CurrentCulture.Name %>">
        <span class="val"><%= CultureInfo.CurrentCulture.DisplayName %></span>        
    </span>
    <% if (ShowHelper) { %>
    <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'NotFoundLanguage'});"></div>
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

<div id="languageMenu" class="languageMenu studio-action-panel">
    <ul class="options dropdown-content">
        <% foreach (var ci in SetupInfo.EnabledCultures)
           { %>
        <li class="option dropdown-item <%= ci.Name %> <%= String.Equals(CultureInfo.CurrentCulture.Name, ci.Name) ? " selected" : "" %>" data="<%= ci.Name %>">
            <a><%= ci.DisplayName %></a>
        </li>
        <% } %>
        <% if (ShowHelper) { %>
        <li class="option dropdown-item more-langs" data="">
            <a><%= Resource.MoreLanguages %></a>
        </li>
        <% } %>
    </ul>
</div>
