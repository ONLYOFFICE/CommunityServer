<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationSidePanel.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.NavigationSidePanel" %>

<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>


<div class="page-menu">
<% if (ShowCreateButton)
 {%>
<asp:PlaceHolder ID="_taskAction" runat="server"/>
<asp:PlaceHolder ID="_milestoneAction" runat="server"/>
<div id="createNewButton" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (ParticipantSecurityInfo["Project"])
           {%>
        <li><a id="createNewProject" class="dropdown-item" href="projects.aspx?action=add"><%= ProjectResource.Project %></a></li>
        <% }
           if (ParticipantSecurityInfo["Milestone"])
           {%>
        <li><a id="createNewMilestone" class="dropdown-item" href="javascript:void(0)"><%= MilestoneResource.Milestone %></a></li>
        <% }
           if (ParticipantSecurityInfo["Task"])
           {%>
        <li><a id="createNewTask" class="dropdown-item" href="javascript:void(0)"><%= TaskResource.Task %></a></li>
        <% }
           if (ParticipantSecurityInfo["Discussion"])
           {%>
               <%if(RequestContext.IsInConcreteProject) {%>
                    <li><a id="createNewDiscussion"  class="dropdown-item" href="messages.aspx?action=add&prjID=<%=RequestContext.GetCurrentProjectId() %>"><%= MessageResource.Message %></a></li>
                <%}else{%>
                    <li><a id="createNewDiscussion"  class="dropdown-item" href="messages.aspx?action=add"><%= MessageResource.Message %></a></li>
                <%}%>

        <% }
           if (ParticipantSecurityInfo["Time"])
           {%>
        <li><a id="createNewTimer" class="dropdown-item" href="javascript:void(0)"><%= ProjectsCommonResource.AutoTimer %></a></li>
        <% }
           if (ParticipantSecurityInfo["ProjectTemplate"])
           {%>
        <li><a id="createProjectTempl" class="dropdown-item" href="projectTemplates.aspx?action=add"><%= ProjectResource.ProjectTemplate %></a></li>
        <% } %>

        <% if (Page is TMDocs)
           { %>
            <li><div class="dropdown-item-seporator"></div></li>
            <asp:PlaceHolder runat="server" ID="CreateDocsHolder"></asp:PlaceHolder>
        <% } %>
    </ul>
</div>
<ul class="menu-actions">
    <li id="menuCreateNewButton" class="menu-main-button without-separator <%= Page is TMDocs ? "middle" : "big" %>">
        <span class="main-button-text" style="<%= Page is TMDocs ? "padding-top:8px;" : "" %>"><%= ProjectsCommonResource.CreateNewButton %></span>
        <span class="white-combobox">&nbsp;</span>
    </li>
    <% if (Page is TMDocs)
        { %>
    <li id="buttonUpload" class="menu-upload-button" title="<%= ProjectsFileResource.ButtonUpload %>">
        <span class="menu-upload-icon">&nbsp;</span> 
    </li>
    <% } %>
</ul>
<%}%>
        <ul class="menu-list">
            <li id="menuProjects" class="menu-item sub-list">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow" href="projects.aspx">
                        <span class="menu-item-icon projects"></span>
                        <span class="menu-item-label inner-text"><%= ProjectResource.Projects %></span>
                    </a>
                    <span id="feed-new-projects-count" class="feed-new-count"></span>
                </div>
                <ul class="menu-sub-list">
                    <% if (!IsOutsider) { %>
                    <% if (MyProjects.Count != 0) { %>
                       <li id="myProjectsConteiner" class="menu-sub-item myProjectsConteiner">
                            <div class="menu-item sub-list">
                                <span id="myProjectsExpander" class="expander"></span>
                                <a id="menuMyProjects" class="menu-item-label outer-text text-overflow" href=""><%= ProjectsCommonResource.LeftMenuMyProjects%></a>
                            </div>
                            <ul class="menu-sub-list">
                                <% foreach (var myProject in MyProjects)
                                   {%>
                                    <li class="menu-sub-item" id="<%=myProject.ID %>">
                                        <a href="tasks.aspx?prjID=<%= myProject.ID %>" title="<%= HttpUtility.HtmlEncode(myProject.Title)%>"><%= HttpUtility.HtmlEncode(myProject.Title)%></a>
                                    </li>
                                   <% } %>
                            </ul>
                        </li>
                    <% } else { %>
                       <li class="menu-sub-item filter myProjectsConteiner">
                            <a id="menuMyProjects" class="menu-item-label outer-text text-overflow" href=""><%= ProjectsCommonResource.LeftMenuMyProjects%></a>
                       </li>
                    <% } %>
                    <li id="followedProjectsConteiner" class="menu-sub-item filter">
                        <a id="menuFollowedProjects" class="menu-item-label outer-text text-overflow" href="#followed=true&status=open"><%=ProjectsCommonResource.LeftMenuFollowedProjects%></a>
                    </li>
                    <% } %>
                    <li class="menu-sub-item filter">
                        <a id="menuActiveProjects" class="menu-item-label outer-text text-overflow" href="#status=open"><%=ProjectsCommonResource.LeftMenuActiveProjects%></a>
                    </li>
                </ul>
            </li>
            <li id="menuMilestones" class="menu-item sub-list">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow" href="milestones.aspx">
                        <span class="menu-item-icon milestones"></span>
                        <span class="menu-item-label inner-text"><%=MilestoneResource.Milestones%></span>
                    </a>
                    <span id="feed-new-milestones-count" class="feed-new-count"></span>
                </div>
                <ul class="menu-sub-list">
                    <% if (!Page.Participant.IsVisitor)
                       { %>
                    <li class="menu-sub-item filter">
                        <a id="menuMyMilestones" class="menu-item-label outer-text text-overflow" href=""><%=ProjectsFilterResource.MyMilestones%></a>
                    </li>
                    <%} %>
                    <li class="menu-sub-item filter">
                        <a id="menuUpcomingMilestones" class="menu-item-label outer-text text-overflow" href=""><%=ProjectsFilterResource.Upcoming%></a>
                    </li>
                </ul>
            </li>
            <li id="menuTasks" class="menu-item <%if (!Page.Participant.IsVisitor) {%>sub-list <%}else {%> none-sub-list <%}%>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow" href="tasks.aspx">
                        <span class="menu-item-icon tasks"></span>
                        <span class="menu-item-label inner-text"><%=TaskResource.Tasks%></span>
                    </a>
                    <span id="feed-new-tasks-count" class="feed-new-count"></span>
                </div>
                <% if (!Page.Participant.IsVisitor)
                   { %>
                <ul class="menu-sub-list">
                    <li class="menu-sub-item filter">
                        <a id="menuMyTasks" class="menu-item-label outer-text text-overflow" href=""><%=ProjectsFilterResource.MyTasks%></a>
                    </li>
                    <li class="menu-sub-item filter">
                        <a id="menuUpcomingTasks" class="menu-item-label outer-text text-overflow" href=""><%=ProjectsFilterResource.Upcoming%></a>
                    </li>
                </ul>
                <%} %>
            </li>
            <li id="menuMessages" class="menu-item sub-list">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow" href="messages.aspx">
                        <span class="menu-item-icon messages"></span>
                        <span class="menu-item-label inner-text"><%=MessageResource.Messages%></span>
                    </a>
                    <span id="feed-new-discussions-count" class="feed-new-count"></span>
                </div>
                <ul class="menu-sub-list">
                    <% if (!Page.Participant.IsVisitor)
                       { %>
                    <li class="menu-sub-item filter">
                        <a id="menuMyDiscussions" class="menu-item-label outer-text text-overflow" href=""><%=ProjectsCommonResource.LeftMenuMyDiscussions%></a>
                    </li>
                    <%} %>
                    <li class="menu-sub-item filter">
                        <a id="menuLatestDiscussion" class="menu-item-label outer-text text-overflow" href=""><%=ProjectsCommonResource.LeftMenuLatest%></a>
                    </li>
                </ul>
            </li>
            <% if (!MobileDetector.IsMobile)
               { %>
            <li id="menuGanttChart" class="menu-item none-sub-list">
                <a class="menu-item-label outer-text text-overflow" href="ganttchart.aspx">
                    <span class="menu-item-icon chart"></span>
                    <span class="menu-item-label inner-text"><%= ProjectResource.GanttGart %></span>
                </a>
            </li>
            <% } %>
            <%if (!Page.Participant.IsVisitor)
              {%>
            <li id="menuTimeTracking" class="menu-item none-sub-list">
                <a class="menu-item-label outer-text text-overflow" href="timeTracking.aspx">
                    <span class="menu-item-icon timetrack"></span>
                    <span class="menu-item-label inner-text"><%=ProjectsCommonResource.TimeTracking%></span>
                </a>
            </li>
            <%} %>            
            <li id="menuTMDocs" class="menu-item none-sub-list">
                <% if (Page is TMDocs)
                   {%>
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow">
                        <span class="menu-item-icon documents"></span>
                        <span class="menu-item-label inner-text"><%= ProjectsFileResource.Documents %></span>
                    </a>
                    <span class="new-label-menu is-new display-none" title="<%= ASC.Web.Files.Resources.FilesUCResource.RemoveIsNew %>" data-id="<%= ASC.Web.Files.Classes.Global.FolderProjects %>"></span>
                </div>
                <div class="menu-sub-list documentTreeNavigation">
                    <asp:PlaceHolder runat="server" ID="placeHolderFolderTree"></asp:PlaceHolder>
                </div>
                <% } else{%>
                   <a class="menu-item-label outer-text text-overflow" href="tmdocs.aspx">
                        <span class="menu-item-icon documents"></span>
                        <span class="menu-item-label inner-text"><%= ProjectsFileResource.Documents %></span>
                   </a>
                <% } %>
            </li>
            <% if (!Page.Participant.IsVisitor)
               { %>
                <li id="menuReports" class="menu-item none-sub-list">
                    <a class="menu-item-label outer-text text-overflow" href="reports.aspx">
                        <span class="menu-item-icon reports"></span>
                        <span class="menu-item-label inner-text"><%= ProjectsCommonResource.ReportsModuleTitle %></span>
                    </a>     
                </li>
                
                <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>
                <% if (IsFullAdmin || IsProjectAdmin)
                   { %>
                    <li id="menuSettings" class="menu-item sub-list add-block">
                        <div class="category-wrapper">
                            <span class="expander"></span>
                            <a class="menu-item-label outer-text text-overflow" href="projectTemplates.aspx">
                                <span class="menu-item-icon settings"></span>
                                <span class="menu-item-label inner-text"><%= ProjectsCommonResource.Settings %></span>
                            </a>
                        </div>
                        <ul class="menu-sub-list">
                            <% if (IsFullAdmin || IsProjectAdmin)
                               { %>
                                <li id="menuTemplates" class="menu-sub-item filter">
                                    <a id="menuProjectTemplate" class="menu-item-label outer-text text-overflow" href="projectTemplates.aspx"><%= ProjectResource.ProjectTemplates %></a>
                                </li>
                            <% } %>
                            <% if (IsFullAdmin)
                               { %>
                                <li id="menuImport" class="menu-sub-item filter">
                                    <a id="menuImport" class="menu-item-label outer-text text-overflow" href="import.aspx"><%= ImportResource.Import %></a>
                                </li>                   
                              <li id="menuAccessRightsItem" class="menu-sub-item filter">
                                    <a id="menuAccessRights" class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) + "#projects" %>">
                                        <%= ProjectResource.AccessRightSettings %>
                                    </a>
                              </li>  
                            <% } %>                          
                        </ul>
                    </li>
            <% }
            } %>
            <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
        </ul>
</div>

<% if (DisplayAppsBanner) {%>
<div class="mobileApp-banner">
    <div class="mobileApp-banner_text">
        <div class="mobileApp-banner_text_i"><%=ProjectsCommonResource.BannerMobileText %></div>
        <div class="mobileApp-banner_text_i"><%=ProjectsCommonResource.BannerMobileProjects %></div>
    </div>
    <div class="mobileApp-banner_btns">
        <a href="https://itunes.apple.com/us/app/teamlab-pm/id670964140?ls=1&mt=8" target="_blank" class="mobileApp-banner_btn app-store"></a>
        <a href="https://play.google.com/store/apps/details?id=com.teamlab.projects&hl=en" target="_blank" class="mobileApp-banner_btn google-play"></a>
    </div>
</div>
<%} %>

