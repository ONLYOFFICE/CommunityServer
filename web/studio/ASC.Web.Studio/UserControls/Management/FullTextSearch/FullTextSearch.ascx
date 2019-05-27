<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FullTextSearch.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.FullTextSearch" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<div id="settingsContainer" class="clearFix">
    <div class="settings-block">
        <div class="header-base"><%= Resource.FullTextSearchSettings %></div>
        <p class="fts-settings-text"><%= Resource.FullTextSearchSettingsText %> </p>
        <div class="fts-settings-block clearFix">
            <div class="fts-settings-item">
                <div class="host requiredField">
                    <div class="fts-settings-title"><%= Resource.HostName %>:</div>
                    <input type="text" class="fts-settings-field textEdit" value="<%= CurrentSettings.Host %>" />
                </div>
                <div class="port requiredField">
                    <div class="fts-settings-title"><%= Resource.Port %>:</div>
                    <input type="text" class="fts-settings-field textEdit" value="<%= CurrentSettings.Port.ToString(CultureInfo.InvariantCulture)%>" />
                </div>
            </div>
        </div>

        <div class="middle-button-container">
            <a id="ftsButtonSave" class="button blue" href="javascript:void(0);"><%=  Resource.SaveButton %></a>
            <span class="splitter-buttons"></span>
            <a id="ftsButtonTest" class="button gray" href="javascript:void(0);"><%=  Resource.FullTextSearchTestButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.FullTextSearchSettingsHelp.HtmlEncode(), "<br />") %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/server/windows/community/troubleshooting.aspx#SphinxIssue" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>