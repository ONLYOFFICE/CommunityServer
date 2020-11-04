<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ColorThemes.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ColorThemes" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="colorThemeBlock" class="settings-block">
        <div class="header-base clearFix"><%= Resource.ColorThemesTitle %></div>
        <% foreach (var theme in ColorThemesList)
           {%>
            <div class="clearFix">
                <input id="chk-<%= theme.Value %>" value="<%= theme.Value %>" type="radio" <%= (theme.Value.Equals(ChosenTheme) ? "checked=\"checked\"":"") %> name="colorTheme"/>
                <label for="chk-<%= theme.Value %>"><%= theme.Title %></label>
            </div>
        <% } %>     
        <div class="preview-theme-image <%= ChosenTheme%>">
        </div>
         <div class="middle-button-container">
            <a class="button blue" id="saveColorThemeBtn">
                <%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerColorTheme.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
         <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>