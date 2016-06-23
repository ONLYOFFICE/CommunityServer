<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectNavigatePanel.ascx.cs" Inherits="ASC.Web.Projects.Controls.Projects.ProjectNavigatePanel" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="project-info-container">
    <div class="project-total-info">
        <div class="project-title header-with-menu">
            <% if (InConcreteProjectModule) { %>      
                <%if ((CurrentPage == "messages" || CurrentPage == "tasks"))
                  {%> 
                  <a class="header-back-link" href="<%= UpLink %>"></a>
                  <%}%>          
                <span class="main-title-icon <%=CurrentPage.ToLower() == "timetracking" ? "projects" : CurrentPage %>"></span>                 
            <% } else {%>         
                <span class="main-title-icon projects" ></span> 
                <% if (Project.Private){%><span class="private"></span><% } %>
            <% } %>
            <span id="essenceTitle" class="text-overflow truncated-text" title="<%= HttpUtility.HtmlEncode(Page.EssenceTitle) %>"><%= HttpUtility.HtmlEncode(Page.EssenceTitle)%></span>
            <span class="header-status" data-text="(<%=Page.EssenceStatus.ToLower() %>)"><%= !string.IsNullOrEmpty(Page.EssenceStatus) ? string.Format("({0})", Page.EssenceStatus.ToLower()) : ""%></span>
            <% if (!IsOutsider) { %>
            <% if (!InConcreteProjectModule)
                { %>
                 <%if(!IsInTeam)
                     if(!IsSubcribed){%>
                    <a id="followProject" class="follow-status unsubscribed" data-text="<%= ProjectsCommonResource.Unfollow %>" title="<%= ProjectsCommonResource.Follow %>"></a>
                    <% }else{ %>
                    <a id="followProject"  class="follow-status subscribed" data-followed="followed" data-text="<%= ProjectsCommonResource.Follow %>" title="<%= ProjectsCommonResource.Unfollow %>"></a>
                  <% } %>
            <% }else 
            if(CurrentPage == "messages"){%>
                <a id="changeSubscribeButton" subscribed="<%= IsSubcribed ? "1": "0" %>" class="follow-status <%= IsSubcribed ? "subscribed" : "unsubscribed"%>" title="<%= IsSubcribed ? ProjectsCommonResource.UnSubscribeOnNewComment : ProjectsCommonResource.SubscribeOnNewComment%>"></a>            
               <% } else 
            if(CurrentPage == "tasks"){%>
                <a id="followTaskActionTop" class="follow-status <%= IsSubcribed ? "subscribed" : "unsubscribed"%>" textvalue="<%= IsSubcribed ? TaskResource.FollowTask :TaskResource.UnfollowTask%>" onclick="ASC.Projects.TaskDescroptionPage.subscribeTask();" title="<%=IsSubcribed ? TaskResource.UnfollowTask : TaskResource.FollowTask%>"></a>           
            <% } %>
            <% } %>
            <span class="menu-small <% if (!InConcreteProjectModule && IsInTeam){ %> vertical-align-middle<%} %> <%if (Page.Participant.IsVisitor && Request["id"] != null){  %> visibility-hidden<% } %>"></span>    
        </div>
        <% if (ShowGanttChartFlag){ %>
            <a class="button blue middle gant-chart-link"  href="ganttchart.aspx?prjID=<%=Project.ID %>" target="_blank"><%=ProjectResource.GanttGart %></a>
        <% } %>
    </div>

    <div id="projectTabs" class="display-none"></div>
</div>
    <div id="projectActions" class="studio-action-panel">
        <ul class="dropdown-content">
            <% if (CanEditProject && !Page.Participant.IsVisitor){%>
                <li><a class="dropdown-item" href="projects.aspx?prjID=<%= Project.ID %>&action=edit"><%= ProjectsCommonResource.Edit %></a></li>
            <% }%>
            <% if (CanDeleteProject){%>
                <li><a id="deleteProject" class="dropdown-item"><%= ProjectsCommonResource.Delete %></a></li>
            <% } %>
            <li><a id="viewDescription" class="dropdown-item"><%= ProjectsCommonResource.ViewProjectInfo%></a></li>
        </ul>
    </div>

    <div id="questionWindowDelProj" style="display: none">
        <sc:Container ID="_hintPopup" runat="server">
            <header><%= ProjectResource.DeleteProject %></header>
            <body>
                <p>
                    <%= ProjectResource.DeleteProjectPopup %>
                </p>
                <p>
                    <%= ProjectsCommonResource.PopupNoteUndone %>
                </p>
                <div class="middle-button-container">
                    <a class="button blue middle remove">
                        <%= ProjectResource.DeleteProject %>
                    </a> 
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel">
                        <%= ProjectsCommonResource.Cancel %>
                    </a>
                </div>
            </body>
        </sc:Container>
    </div>
    
    <div class="projectDescriptionPopup">
        <sc:Container ID="_projectDescriptionPopup" runat="server">
            <header><%= ProjectsCommonResource.ProjectInformation%></header>
            <body>
                <div class="descriptionContainer">
                    <div class="section-name"><span><%=ProjectsCommonResource.ProjectName%></span> 
                        <%=HttpUtility.HtmlEncode(Project.Title)%></div>
                    <div id="managerNameInfo" class="section-name" data-managerid="<%=Project.Responsible %>"><span><%=ProjectResource.ProjectLeader%></span> 
                        <%=ProjectLeaderName%></div>
                    <div class="section-name"><span><%=ProjectsFilterResource.ByCreateDate %></span> 
                        <%=HttpUtility.HtmlEncode(Project.CreateOn.ToShortDateString()) %></div>
                    <%if (!string.IsNullOrEmpty(Project.Description))
                      {%>
                    <div class="section-name"><span><%= ProjectsCommonResource.Description %></span>
                        <p id="prjInfoDescription" data-description="<%= HttpUtility.HtmlEncode(Project.Description) %>"><%= HttpUtility.HtmlEncode(Project.Description) %></p></div>
                    <% } %>
                </div>
            </body>
        </sc:Container>
    </div>
