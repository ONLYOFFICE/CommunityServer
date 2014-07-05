<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    CodeBehind="projectTeam.aspx.cs" Inherits="ASC.Web.Projects.ProjectTeam" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>


<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
       
    <div class="header-base block-cnt-splitter">
        <%= ProjectResource.ProjectLeader %>
    </div>
    
    <div class="block-cnt-splitter clearFix">
        <div class="pm-projectTeam-projectLeaderCard" style="float:left;">
            <img class="managerAvatar" src="<%=ManagerAvatar %>"/>
            <div class="manager-info" data-manager-guid = "<%=Manager.ID %>">
                <div><a class="link header-base middle bold" href="<%=ManagerProfileUrl %>"><%=ManagerName%></a></div>
                <div><%=ManagerDepartmentUrl%></div>
                <div><%=HttpUtility.HtmlEncode(Manager.Title) %></div>
            </div>
        </div>
        <div style="margin-left: 380px;padding: 0 20px;">
            <div><%= ProjectResource.ClosedProjectTeamManagerPermission %>:</div>
            <div class="simple-marker-list"><%= ProjectResource.ClosedProjectTeamManagerPermissionArticleA %></div>
            <div class="simple-marker-list"><%= String.Format(ProjectResource.ClosedProjectTeamManagerPermissionArticleB, "<span id='PrivateProjectHelp' class='baseLinkAction'>", "</span>") %></div>
            <div class="simple-marker-list"><%= String.Format(ProjectResource.ClosedProjectTeamManagerPermissionArticleC, "<span id='RestrictAccessHelp' class='baseLinkAction'>", "</span>", "<br/>") %></div>
            
            <div class="popup_helper" id="AnswerForPrivateProjectTeam">
                <p><%= String.Format(ProjectsCommonResource.HelpAnswerPrivateProjectTeam, "<br />", "<b>", "</b>") %>
                <a target="_blank" href="<%= CommonLinkUtility.GetHelpLink() + "gettingstarted/projects.aspx#ManagingYourTeam_block" %>"> <%= ProjectsCommonResource.LearnMoreLink %></a></p>
            </div> 
            
            <div class="popup_helper" id="AnswerForRestrictAccessTeam">
                <p><%= String.Format(ProjectsCommonResource.HelpAnswerRestrictAccessTeam, "<br />", "<b>", "</b>") %>
                <a target="_blank" href="<%= CommonLinkUtility.GetHelpLink() + "gettingstarted/projects.aspx#ManagingYourTeam_block" %>"> <%= ProjectsCommonResource.LearnMoreLink %></a></p>
            </div>     
            
        </div>
    </div>
    
    <div class="block-cnt-splitter">
        <% if (CanEditTeam)%>
        <%
           { %>
        <div id="pm-projectTeam-Selector" class="link dotline plus"><%= ProjectResource.ManagmentTeam %></div>
        <% } %>
    </div>
   <% if (CanEditTeam) {%>
    <table id="team_container" class="block-cnt-splitter listContainer">
    </table>
   <% }else
      {%>
    <table id="team_container" class="block-cnt-splitter no-access listContainer">
    </table>         
   <% } %>     
    

    <div id="userActionPanel" class="studio-action-panel" data-userid="" data-username="" data-email="" data-user="">
	    <div class="corner-top right"></div>        
        <ul class="dropdown-content">
            <% if (CanCreateTask()){ %>
            <li id="addNewTask" title="<%= TaskResource.AddNewTask %>" class="dropdown-item"><a><%= TaskResource.AddNewTask %></a></li>
            <% } %>
            <% if (!Participant.IsVisitor){ %>
            <li id="reportOpenTasks" title="<%= ReportResource.ReportOpenTasks %>" class="dropdown-item"><%= ReportResource.ReportOpenTasks %></li>
            <li id="reportClosedTasks" title="<%= ReportResource.ReportClosedTasks %>" class="dropdown-item"><%= ReportResource.ReportClosedTasks %></li>
            <% } %> 
            <li id="viewOpenTasks" title="<%= ProjectsJSResource.ViewAllOpenTasks %>" class="dropdown-item"><%= ProjectsJSResource.ViewAllOpenTasks %></li>
            <li id="sendEmail" title="<%= ProjectResource.ClosedProjectTeamWriteMail %>" class="dropdown-item"><%= ProjectResource.ClosedProjectTeamWriteMail %></li>
            <li id="writeJabber" title="<%= ProjectResource.ClosedProjectTeamWriteInMessenger %>" class="dropdown-item"><%= ProjectResource.ClosedProjectTeamWriteInMessenger %></li>
            <% if (CanEditTeam)
               { %>
            <li id="removeFromTeam" title="<%= ProjectsCommonResource.RemoveMemberFromTeam %>" class="dropdown-item"><%= ProjectsCommonResource.RemoveMemberFromTeam %></li>
            <% } %>
        </ul>
    </div>
    
    <script id="memberTemplate" type="text/x-jquery-tmpl">
        <tr class="with-entity-menu pm-projectTeam-participantContainer {{if isAdministrator || isManager}} disable{{/if}}" data-partisipantId="${id}" data-email="${email}" data-user="${userName}">
            <td style="" class="user-info">
                        <div class="pm-projectTeam-userPhotoContainer">
                            <img src="${avatarSmall}" />
                            {{if status == 2}}
                                <span class="status-blocked"></span>
                            {{/if}}
                            <% if (CanEditTeam)%>
                            <% { %>
                                {{if isVisitor}}
                                    <span class="role collaborator"></span>
                                {{/if}}                          
                                {{if (isAdmin || isAdministrator) && status!=2}}
                                    <span class="role admin"></span>
                                {{/if}}
                            <% } %>
                        </div>
                        <div class="user-info-container">                           
                            <a class="user-name" href="<%=UserProfileLink%>&user=${userName}" title="${displayName}">
                                ${displayName}
                            </a>                                                       
                            <span title="${title}">${title}</span>
                        </div>
            </td>
           <% if (Project.Private && CanEditTeam)%>
                <%{%>
            <td class="right-settings">
                {{if status!=2}}
                <div>
                    {{if canReadMessages}}
                    <span class="right-checker pm-projectTeam-modulePermissionOn" data-flag="Messages">
                    {{else}}
                    <span class="right-checker pm-projectTeam-modulePermissionOff" data-flag="Messages">
                    {{/if}}
                        <span>
                            <%= MessageResource.Messages %>
                        </span>
                    </span>    
                    <span class="splitter"></span>
                    {{if canReadFiles}}
                    <span class="right-checker pm-projectTeam-modulePermissionOn" data-flag="Files">
                    {{else}}
                    <span class="right-checker pm-projectTeam-modulePermissionOff" data-flag="Files">
                    {{/if}}
                        <span>
                            <%= ProjectsFileResource.Documents %>
                        </span>
                    </span>
                    <span class="splitter"></span>
                    {{if canReadTasks}}
                    <span class="right-checker pm-projectTeam-modulePermissionOn" data-flag="Tasks">
                    {{else}}
                    <span class="right-checker pm-projectTeam-modulePermissionOff" data-flag="Tasks">
                    {{/if}}
                        <span>
                            <%= TaskResource.AllTasks %>
                        </span>
                    </span>
                    <span class="splitter"></span>
                    {{if canReadMilestones}}
                    <span class="right-checker pm-projectTeam-modulePermissionOn" data-flag="Milestone">
                    {{else}}
                    <span class="right-checker pm-projectTeam-modulePermissionOff" data-flag="Milestone">
                    {{/if}}
                        <span>
                            <%= MilestoneResource.Milestones %>
                        </span>
                    </span>
                    <span class="splitter"></span>
                    {{if canReadContacts}}
                    <span class="right-checker pm-projectTeam-modulePermissionOn" data-flag="Contacts">
                    {{else}}
                        <span class="right-checker pm-projectTeam-modulePermissionOff {{if isVisitor}} no-dotted{{/if}}" data-flag="Contacts">
                    {{/if}}
                        <span>
                            <%= ProjectsCommonResource.ModuleContacts %>
                        </span>
                    </span>
                </div>
                {{/if}}
            </td>
            <% } %>
 
            <td class="menupoint-container">           
                {{if Teamlab.profile.isVisitor}}
                    {{if Teamlab.profile.id != id}} 
                    <div class="entity-menu">
                    </div>
                    {{/if}}
                {{else}}
                    <div class="entity-menu" data-isVisitor="${isVisitor}" data-status="${status}">
                    </div>
                {{/if}}
            </td>
        </tr>            
    </script>
</asp:Content>

