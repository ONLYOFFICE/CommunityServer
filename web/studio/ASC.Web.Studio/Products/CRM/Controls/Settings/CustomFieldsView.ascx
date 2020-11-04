<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomFieldsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.CustomFieldsView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<ul id="customFieldList" class="clearFix ui-sortable"></ul>

<div id="customFieldActionMenu" class="studio-action-panel" fieldid="">
    <ul class="dropdown-content">
        <li><a class="dropdown-item with-icon edit editField" onclick="ASC.CRM.SettingsPage.showEditFieldPanel();"><%= CRMSettingResource.EditCustomField %></a></li>
        <li><a class="dropdown-item with-icon delete deleteField" onclick="ASC.CRM.SettingsPage.deleteField();"><%= CRMSettingResource.DeleteCustomField %></a></li>
    </ul>
</div>