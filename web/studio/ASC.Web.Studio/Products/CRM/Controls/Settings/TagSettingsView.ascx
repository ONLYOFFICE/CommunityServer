<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TagSettingsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.TagSettingsView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div class="header-base settingsHeader"><%= CRMCommonResource.Tags %></div>

<div id="TagSettingsTabs"></div>

<div class="clearFix settingsNewItemBlock">
    <a id="createNewTagSettings" class="gray button display-none">
        <span class="plus"><%= CRMSettingResource.CreateNewTagListButton %></span>
    </a>
    <div class="settingsConfirmDialogBlock cbx_AddTagWithoutAskingContainer display-none">
        <input type="checkbox" id="cbx_AddTagWithoutAsking" <% if (ASC.Web.CRM.Classes.Global.TenantSettings.AddTagToContactGroupAuto != null) { %>checked="checked"<% } %> />
        <label for="cbx_AddTagWithoutAsking"><%= CRMSettingResource.AddTagWithoutAskingSettingsLabel %></label>
    </div>
</div>

<ul id="tagList" class="clearFix"></ul>