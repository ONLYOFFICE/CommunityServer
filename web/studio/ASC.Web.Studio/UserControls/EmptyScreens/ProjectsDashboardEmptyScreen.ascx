<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectsDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.ProjectsDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<% if (!UseStaticPosition)
   { %>
    <div class="backdrop" blank-page=""></div>
<% } %>

<div id="content" blank-page="" class="dashboard-center-box projects <%= UseStaticPosition ? "static" : "" %>">
    <div class="header">
        <% if (!UseStaticPosition)
           { %>
            <span class="close"></span>
        <% } %>
        <%= ProjectsCommonResource.DashboardTitle %>
    </div>
    <div class="content clearFix">
        <div class="module-block">
            <img src="<%= VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/images/icon-tasks.png") %>" />
            <div class="title"><%= ProjectsCommonResource.TasksModuleTitle %></div>
            <ul>
                <li><%= ProjectsCommonResource.TasksModuleFirstLine %></li>
                <li><%= ProjectsCommonResource.TasksModuleSecondLine %></li>
                <li><%= ProjectsCommonResource.TasksModuleThirdLine %></li>
            </ul>
        </div>
        <div class="module-block">
            <img src="<%= VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/images/icon-milestones.png") %>" />
            <div class="title"><%= ProjectsCommonResource.MilestonesModuleTitle %></div>
            <ul>
                <li><%= ProjectsCommonResource.MilestonesModuleFirstLine %></li>
                <li><%= ProjectsCommonResource.MilestonesModuleSecondLine %></li>
                <li><%= ProjectsCommonResource.MilestonesModuleThirdLine %></li>
            </ul>
        </div>
        <div class="module-block">
            <img src="<%= VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/images/icon-document.png") %>" />
            <div class="title"><%= ProjectsCommonResource.DocsModuleTitle %></div>
            <ul>
                <li><%= ProjectsCommonResource.DocsModuleFirstLine %></li>
                <li><%= ProjectsCommonResource.DocsModuleSecondLine %></li>
                <li><%= ProjectsCommonResource.DocsModuleThirdLine %></li>
            </ul>
        </div>
        <div class="module-block">
            <img src="<%= VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/images/icon-discussion.png") %>" />
            <div class="title"><%= ProjectsCommonResource.DiscussionModuleTitle %></div>
            <ul>
                <li><%= ProjectsCommonResource.DiscussionModuleFirstLine %></li>
                <li><%= ProjectsCommonResource.DiscussionModuleSecondLine %></li>
                <li><%= ProjectsCommonResource.DiscussionModuleThirdLine %></li>
            </ul>
        </div>
        <div class="module-block">
            <img src="<%= VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/images/icon-report.png") %>" />
            <div class="title"><%= ProjectsCommonResource.ReportsModuleTitle %></div>
            <ul>
                <li><%= ProjectsCommonResource.ReportsModuleFirstLine %></li>
                <li><%= ProjectsCommonResource.ReportsModuleSecondLine %></li>
                <li><%= ProjectsCommonResource.ReportsModuleThirdLine %></li>
            </ul>
        </div>
    </div>
    <% if (IsAdmin) %>
    <%
       { %>
        <div class="dashboard-buttons">
            <a class="button huge create-button" href="<%= VirtualPathUtility.ToAbsolute("~/products/projects/projects.aspx?action=add") %>" >
                <%= ProjectsCommonResource.CreateProjectButtonText %>
            </a>
        </div>
    <% } %>
</div>