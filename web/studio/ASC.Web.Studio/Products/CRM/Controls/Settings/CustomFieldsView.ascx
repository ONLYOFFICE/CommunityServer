<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomFieldsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.CustomFieldsView" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div class="settingsHeaderWithViewSwitcher clearFix">
    <a id="createNewField" class="gray button display-none">
        <span class="plus"><%= CRMSettingResource.CreateNewFieldListButton%></span>
    </a>
    <div id="CustomFieldsTabs"></div>
</div>

<ul id="customFieldList" class="clearFix ui-sortable">
</ul>

<div id="customFieldActionMenu" class="studio-action-panel" fieldid="">
    <div class="corner-top right"></div>
    <ul class="dropdown-content">
        <li><a class="dropdown-item editField" onclick="ASC.CRM.SettingsPage.showEditFieldPanel();"><%= CRMSettingResource.EditCustomField %></a></li>
        <li><a class="dropdown-item deleteField" onclick="ASC.CRM.SettingsPage.deleteField();"><%= CRMSettingResource.DeleteCustomField %></a></li>
    </ul>
</div>