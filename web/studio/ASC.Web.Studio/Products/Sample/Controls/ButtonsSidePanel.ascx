<%@ Import Namespace="ASC.Web.Sample.Resources" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ButtonsSidePanel.ascx.cs" Inherits="ASC.Web.Sample.Controls.ButtonsSidePanel" %>

<div class="page-menu">
    <ul class="menu-actions">
        <li id="menuCreateNewButton" class="menu-main-button without-separator middle" title="<%= SampleResource.NavigateTo %>">
            <span class="main-button-text"><%= SampleResource.NavigateTo %></span>
            <span class="white-combobox"></span>
        </li>
        <li id="menuOtherActionsButton" class="menu-gray-button" title="<%= SampleResource.MoreActions %>">
            <span class="btn_other-actions">...</span>
        </li>
    </ul>
    <div id="createNewButton" class="studio-action-panel">
        <ul class="dropdown-content">
            <li>
                <a class="dropdown-item" href="Default.aspx"><%= SampleResource.CreateModule %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Styles.aspx"><%= SampleResource.Styles %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Elements.aspx"><%= SampleResource.Elements %></a>
            </li>
            <li>
                <a class="dropdown-item" href="UserControls.aspx"><%= SampleResource.Controls %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Database.aspx"><%= SampleResource.Database %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Api.aspx"><%= SampleResource.Api %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Help.aspx"><%= SampleResource.Help %></a>
            </li>
        </ul>
    </div>
    <div id="otherActions" class="studio-action-panel">
        <ul class="dropdown-content">
            <li>
                <a class="dropdown-item" href="Default.aspx"><%= SampleResource.CreateModule %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Styles.aspx"><%= SampleResource.Styles %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Elements.aspx"><%= SampleResource.Elements %></a>
            </li>
            <li>
                <a class="dropdown-item" href="UserControls.aspx"><%= SampleResource.Controls %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Database.aspx"><%= SampleResource.Database %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Api.aspx"><%= SampleResource.Api %></a>
            </li>
            <li>
                <a class="dropdown-item" href="Help.aspx"><%= SampleResource.Help %></a>
            </li>
        </ul>
    </div>
</div>