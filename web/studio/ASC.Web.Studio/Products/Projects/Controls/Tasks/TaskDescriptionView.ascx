<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskDescriptionView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Tasks.TaskDescriptionView" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div id="taskActions" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (CanEditTask && !Page.Participant.IsVisitor)
        if(Task.Status == TaskStatus.Open)
        {%>
        <li><a id="editTaskAction" class="dropdown-item"><%= TaskResource.EditTask %></a></li>
        <% }else
        {%>
        <li class="display-none"><a id="editTaskAction" class="dropdown-item"><%= TaskResource.EditTask %></a></li>
        <% }%>
        <% if (CanDeleteTask){%>
        <li><a id="removeTask" class="dropdown-item"><%=TaskResource.RemoveTask%></a></li>
        <% } %>
        <% if (CanCreateTimeSpend){%>
        <li><a id="startTaskTimer" class="dropdown-item" projectid="<%= Task.Project.ID %>" taskid="<%= Task.ID %>"><%= ProjectsCommonResource.AutoTimer %></a></li>
        <% } %>             
    </ul>
</div>

<div id="questionWindow" style="display: none">
    <sc:Container ID="_hintPopup" runat="server">
        <header>
    <%= TaskResource.ClosingTheTask %>
    </header>
        <body>
            <p>
                <%= TaskResource.TryingToCloseTheTask %>.
            </p>
            <p>
                <%= TaskResource.BetterToReturn %>.</p>
            <div class="middle-button-container">
                <a class="button blue middle end">
                    <%= TaskResource.EndAllSubtasksCloseTask %></a> 
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel">
                    <%= ProjectsCommonResource.Cancel %></a>
            </div>
        </body>
    </sc:Container>
</div>
<div id="questionWindowTaskRemove" style="display: none">
    <sc:Container ID="_hintPopupTaskRemove" runat="server">
        <header>
    <%= TaskResource.RemoveTask %>
    </header>
        <body>
            <p>
                <%= TaskResource.RemoveTaskPopup %>
            </p>
            <p>
                <%= ProjectsCommonResource.PopupNoteUndone %></p>
            <div class="errorBox display-none"></div>
            <div class="middle-button-container">
                <a class="button blue middle remove"><%= TaskResource.RemoveTask %></a> 
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="createNewLinkError" style="display: none">
    <sc:Container ID="_newLinkError" runat="server">
        <header>
            <%= TaskResource.ErrorCreateNewLink %>
        </header>
        <body>
            <p><%=TaskResource.ErrorCreateTaskLink %></p>
            <div class="middle-button-container">
                <a class="button gray middle cancel"><%= ProjectResource.OkButton %></a>
            </div>
        </body>
    </sc:Container>
</div>

<div class="commonInfoTaskDescription" data-time-spend="<%=TaskTimeSpend %>">
</div>
<input id="hiddenInput" style="display: none;" />
<div class="taskTabs">
    <%if(CanEditTask || SubtasksCount > 0)
      {%>
    <div id="subtaskTab" class="tabs-section <%if((int)Task.Status == 2 && SubtasksCount == 0){ %>display-none<% } %>">
        <span class="header-base">
            <%=TaskResource.Subtasks%>
            <span class="count"></span>
        </span>
        <span id="switcherSubtasksButton" class="toggle-button" data-switcher="0" 
            data-showtext="<%=ProjectsCommonResource.Show%>" data-hidetext="<%=ProjectsCommonResource.Hide%>">
            <%=ProjectsCommonResource.Hide%>
        </span>
    </div>
    <div id="subtaskContainer" class="<%if((int)Task.Status == 2 && SubtasksCount == 0){ %>display-none<% } %>">    
        <div class="subtasks"></div>
    </div>
    <% } %>
    
    <% if(CanReadFiles)
        if ((CanEditTask && !MobileDetector.IsMobile) || (AttachmentsCount > 0)) { %>
    <div id="filesTab" class="tabs-section">
        <span class="header-base">
            <%= ProjectsCommonResource.DocsModuleTitle %>
            <span class="count">
                <%if (AttachmentsCount>0){%>
                (<%=AttachmentsCount %>)
                <% } %>
            </span>
        </span>
        <span id="switcherTaskFilesButton" style="display: none" class="toggle-button" data-switcher="1" 
            data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
            <%= ProjectsCommonResource.Hide %>
        </span>
    </div>
    <div id="filesContainer" style="margin-bottom: 8px;" data-projectFolderId = "<%=ProjectFolderId %>">
        <asp:PlaceHolder runat="server" ID="phAttachmentsControl" />
    </div>
    <%} %>
    
    <div id="linkedTasksTab" class="tabs-section">
        <span class="header-base">
            <%=TaskResource.RelatedTask %>
            <span class="count"></span>
        </span>
        <span id="switcherLinkedTasksButton" style="display: none" class="toggle-button" data-switcher="0" 
            data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
            <%= ProjectsCommonResource.Hide %>
        </span>
    </div>
    <div id="linkedTasksContainer" class="">    
        <div class="linked-tasks-buttons">
            <%if(CanEditTask)
            {%>
            <span class="icon-link plus"><a id="addNewLink" class="baseLinkAction"><%=TaskResource.CreateNewLink%></a></span>
            <span class="splitter-buttons"></span>
            <% } %>
            <%if(ShowGanttChartFlag){ %>
            <span class="icon-link chart"><a href="ganttchart.aspx?prjID=<%= Task.Project.ID %>"><%=TaskResource.MoveToGanttChart%></a></span>
            <%} %>
        </div>
        <div class="related-task-loader display-none"></div>
        <table id="editTable" class="task-links display-none">
            <tbody>
                <tr class="linked-task edit-link-container">
                    <td class="title stretch">
                        <select id="taskSelector">
                            <option value="-1" selected="selected"><%=TaskResource.ChooseTaskForLink %></option>
                        </select>
                    </td>
                    <td class="link-type" colspan="4">
                        <span id="linkTypeSelector"></span>
<%--                        <select id="linkTypeSelector">
                            <option value="-1" selected="selected"><%=TaskResource.ChooseLinkType %></option>
                            <option value="0"><%=ProjectsJSResource.RelatedLinkTypeSS %></option>
                            <option value="1"><%=ProjectsJSResource.RelatedLinkTypeEE %></option>
                            <option value="2"><%=ProjectsJSResource.RelatedLinkTypeSE %></option>
                            <option value="3"><%=ProjectsJSResource.RelatedLinkTypeES %></option>
                        </select>--%>
                    </td>
                    <td class="actions"><a id="addLink" class="button gray small"><%=ProjectResource.OkButton %></a></td>
                </tr>
            </tbody>
        </table>
        <table id="relatedTasks" class="task-links">
        </table>
    </div>
    
    <div id="commentsTab" class="tabs-section">
        <span class="header-base">
            <%=ProjectResource.Comments %>
            <span class="count"></span>
        </span>
        <span id="switcherTaskCommentsButton" style="display: none" class="toggle-button" data-switcher="0" 
            data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
            <%= ProjectsCommonResource.Hide %>
        </span>
    </div>
    <div id="commentContainer">
        <div id="commentsListWrapper">
            <scl:CommentsList ID="commentList" runat="server" BehaviorID="commentsList">
            </scl:CommentsList>
        </div>
    </div>
    
    
</div>

<div class="popup_helper" id="hintInvalidLink">
      <p><%= TaskResource.RelatedLinkInvalid %>
          <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
             { %>
           <a href="<%= CommonLinkUtility.GetHelpLink() + "guides/link-tasks.aspx" %>" target="_blank"><%= ProjectsCommonResource.LearnMoreLink %></a>
          <% } %>
      </p>
</div> 

<div id="linkedTaskActionPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li id="linkEdit" class="dropdown-item"><span><%= TaskResource.Edit%></span></li>
        <li id="linkRemove" class="dropdown-item"><span><%= ProjectsCommonResource.Delete%></span></li>
    </ul>
</div>