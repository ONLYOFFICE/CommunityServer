<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TagSettingsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.TagSettingsView" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div class="settingsHeaderWithViewSwitcher clearFix">
    <a id="createNewTagSettings" class="gray button display-none">
        <span class="plus"><%= CRMSettingResource.CreateNewTagListButton %></span>
    </a>

    <div class="cbx_AddTagWithoutAskingContainer display-none">
        <input type="checkbox" style="float: left;" id="cbx_AddTagWithoutAsking"
            <% if (ASC.Web.CRM.Classes.Global.TenantSettings.AddTagToContactGroupAuto != null) { %>checked="checked"<% } %> />
        <label style="float:left; padding: 2px 0 0 4px;" for="cbx_AddTagWithoutAsking">
            <%= CRMSettingResource.AddTagWithoutAskingSettingsLabel %>
        </label>
    </div>

    <div id="TagSettingsTabs"></div>
</div>

<ul id="tagList" class="clearFix">
</ul>