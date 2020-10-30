<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ButtonSidePanel.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.ButtonSidePanel" %>

<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>


<% if (ShowCreateButton)
   { %>
<div class="page-menu">
    <div id="createNewButton" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a id="createNewProject" class="dropdown-item" href="Projects.aspx?action=add"><%= ProjectResource.Project %></a></li>
            <li><a id="createNewMilestone" class="dropdown-item" href="javascript:void(0)"><%= MilestoneResource.Milestone %></a></li>
            <li><a id="createNewTask" class="dropdown-item" href="javascript:void(0)"><%= TaskResource.Task %></a></li>
            <li><a id="createNewDiscussion"  class="dropdown-item" href="Messages.aspx?action=add"><%= MessageResource.Message %></a></li>
            <li><a id="createNewTimer" class="dropdown-item" href="javascript:void(0)"><%= ProjectsCommonResource.AutoTimer %></a></li>
            <li><a id="createProjectTempl" class="dropdown-item" href="ProjectTemplates.aspx?action=add"><%= ProjectResource.ProjectTemplate %></a></li>

            <% if (Page is TMDocs)
               { %>
                <li><div class="dropdown-item-seporator"></div></li>
                <asp:PlaceHolder runat="server" ID="CreateDocsHolder"></asp:PlaceHolder>
            <% } %>
        </ul>
    </div>

    <ul class="menu-actions">
        <li id="menuCreateNewButton" class="menu-main-button without-separator <%= Page is TMDocs ? "middle" : "big" %>" title="<%= ProjectsCommonResource.CreateNewButton %>">
            <span class="main-button-text" style="<%= Page is TMDocs ? "padding-top:8px;" : "" %>"><%= ProjectsCommonResource.CreateNewButton %></span>
            <span class="white-combobox">&nbsp;</span>
        </li>
        <% if (Page is TMDocs)
           { %>
        <li id="menuUploadActionsButton" class="menu-upload-button menu-gray-button disable" title="<%= ProjectsFileResource.ButtonUpload %>">
            <span class="menu-upload-icon btn_other-actions"><svg class="upload-svg"><use base="<%= WebPath.GetPath("/") %>" href="/skins/default/images/svg/projects-icons.svg#projectsIconsupload"></use></svg></span>
        </li>
        <% } %>
    </ul>

    <% if (Page is TMDocs)
       { %>
    <div id="uploadActions" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a id="buttonUpload" class="dropdown-item disable not-ready"><%= ProjectsFileResource.ButtonUploadFiles %></a></li>
            <% if (!MobileDetector.IsMobile)
               { %>
                <li><a id="buttonFolderUpload" class="dropdown-item disable not-ready"><%= ProjectsFileResource.ButtonUploadFolder %></a></li>
            <% } %>
        </ul>
    </div>
    <% } %>
    </div>
<% } %>