<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserLanguage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserLanguage" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="System.Globalization" %>

<div class="field">
    <span class="field-title describe-text "><%= Resource.Language %>:</span>
    <span class="field-value usrLang <%= CultureInfo.CurrentCulture.Name %>">
        <span class="val"><%= CultureInfo.CurrentCulture.DisplayName %></span>        
    </span>
    <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'NotFoundLanguage'});"></div>
    <div class="popup_helper" id="NotFoundLanguage">
        <p>
            <%= string.Format(Resource.NotFoundLanguage, "<a href=\"mailto:documentation@onlyoffice.com\">", "</a>") %>
            <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
             { %>
            <a href="<%= CommonLinkUtility.GetHelpLink() + "guides/become-translator.aspx" %>" target="_blank">
                <%= Resource.LearnMore %></a>
            <% } %>
        </p>
    </div>
</div>

<div id="languageMenu" class="languageMenu studio-action-panel">
    <ul class="options dropdown-content">
        <% foreach (var ci in SetupInfo.EnabledCultures)
           { %>
        <li class="option dropdown-item <%= ci.Name %> <%= String.Equals(CultureInfo.CurrentCulture.Name, ci.Name) ? " selected" : "" %>" data="<%= ci.Name %>">
            <a><%= ci.DisplayName %></a>
        </li>
        <% } %>
    </ul>
</div>
