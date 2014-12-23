<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OnlineEditors.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.OnlineEditors" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="Resources" %>

<% if (SetupInfo.IsVisibleSettings<OnlineEditorsSettings>())
   { %>
<div class="clearFix">
    <div id="onlineEditorsSettings" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.OnlineEditorsSettings %>
        </div>
        <div class="clearFix">
            <input type="radio" id="onlineEditorsSchemeOld" name="onlineEditorsScheme" <%= !OnlineEditorsSettings.NewScheme ? "checked=''" : "" %> />
            <label for="onlineEditorsSchemeOld">
                <%= Resource.OnlineEditors25 %>
            </label>
        </div>
        <div class="clearFix">
            <input type="radio" id="onlineEditorsSchemeNew" name="onlineEditorsScheme" <%= OnlineEditorsSettings.NewScheme ? "checked=''" : "" %> />
            <label for="onlineEditorsSchemeNew">
                <%= Resource.OnlineEditors30 %>
            </label>
        </div>
        <div class="middle-button-container">
            <a id="onlineEditorsSettingsSave" class="button blue">
                <%= Resource.SaveButton %>
            </a>
        </div>
    </div>
    <div class="settings-help-block">
        <p>
            <%= String.Format(Resource.OnlineEditorsSettingsInfo, "<br />", "<b>", "</b>") %>
        </p>
    </div>
</div>
<% } %>