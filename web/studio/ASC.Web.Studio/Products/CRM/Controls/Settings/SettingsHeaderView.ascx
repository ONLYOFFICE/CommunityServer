<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SettingsHeaderView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.SettingsHeaderView" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<% if (!string.IsNullOrEmpty(HeaderText))
   { %>
<div class="header-base settingsHeader"><%= HeaderText %></div>
<% } %>

<% if (!string.IsNullOrEmpty(DescriptionText))
   { %>
<p class="settingsFirstDescription"><%= DescriptionText %></p>
<% } %>

<% if (!string.IsNullOrEmpty(DescriptionTextEditDelete))
   { %>
<p class="settingsSecondDescription"><%= DescriptionTextEditDelete %></p>
<% } %>

<% if (!string.IsNullOrEmpty(TabsContainerId))
   { %>
<div id="<%= TabsContainerId %>"></div>
<% } %>

<% if (!string.IsNullOrEmpty(AddListButtonText))
   { %>
<div class="clearFix settingsNewItemBlock">
    <a id="<%= string.IsNullOrEmpty(AddListButtonId) ? "createNewItem" : AddListButtonId %>" class="gray button">
        <span class="plus"><%= AddListButtonText %></span>
    </a>
    <% if (ShowContactStatusAskingDialog)
       { %>
    <div class="settingsConfirmDialogBlock">
        <input type="checkbox" id="cbx_ChangeContactStatusWithoutAsking" <% if (ASC.Web.CRM.Classes.Global.TenantSettings.ChangeContactStatusGroupAuto != null) { %>checked="checked"<% } %> />
        <label for="cbx_ChangeContactStatusWithoutAsking"><%= CRMSettingResource.ChangeContactStatusWithoutAskingSettingsLabel %></label>
    </div>
    <% } %>
    <% if (ShowTagAskingDialog)
       { %>
    <div class="settingsConfirmDialogBlock cbx_AddTagWithoutAskingContainer display-none">
        <input type="checkbox" id="cbx_AddTagWithoutAsking" <% if (ASC.Web.CRM.Classes.Global.TenantSettings.AddTagToContactGroupAuto != null) { %>checked="checked"<% } %> />
        <label for="cbx_AddTagWithoutAsking"><%= CRMSettingResource.AddTagWithoutAskingSettingsLabel %></label>
    </div>
    <% } %>
</div>
<% } %>