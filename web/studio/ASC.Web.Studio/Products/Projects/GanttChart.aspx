<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="GanttChart.aspx.cs" Inherits="ASC.Web.Projects.GanttChart" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<asp:Content ContentPlaceHolderID="BTHeaderContent" runat="server">
</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">  

    <div class="gantt-chart-top-panel mainPageLayout page-menu">
        <ul id="mainActionButtons" class="menu-actions">
            <li id="menuCreateNewButton" class="menu-main-button without-separator middle disable" title="<%= ProjectsCommonResource.CreateNewButton %>">
                <span class="main-button-text"><%= ProjectsCommonResource.CreateNewButton %></span>
                <span class="white-combobox">&nbsp;</span>
            </li>
            <li id="undoButton" class="menu-upload-button disable" title="<%=ProjectResource.GanttUndo %>">
                <span class="undo-button"></span> 
            </li>
            <li id="reduButton" class="menu-upload-button disable" title="<%=ProjectResource.GanttRedo %>">
                <span class="redo-button"></span> 
            </li>
        </ul>
        <div id="createNewButton" class="studio-action-panel">
            <ul class="dropdown-content">
                <li><a id="createNewMilestone" class="dropdown-item" href="javascript:void(0)"><%= MilestoneResource.Milestone %></a></li>
                <li><a id="createNewTask" class="dropdown-item" href="javascript:void(0)"><%= TaskResource.Task%></a></li>
            </ul>
        </div>
        <div class="zoom-presets">
                <span><%=ProjectResource.GanttTimelineScale%>: </span>
                <div class="scale-conteiner">
                    <select id="zoomScale">
                        <option value="1" selected="selected"><%=ProjectResource.GanttTimelineDays%></option>
                        <option value="7"><%=ProjectResource.GanttTimelineWeek%></option>
                        <option value="4"><%=ProjectResource.GanttTimelineMonth %></option>
                    </select>
                </div>
                <div id="todayPreset" class="preset" title="<%=ProjectResource.GanttTodayTitle %>"><%=ProjectResource.GanttPresetToday %></div>
        </div>
        <ul class="gantt-chart-zoom-container menu-actions">
            <li id="ganttChartMoveLeft" class="menu-upload-button display-none" title="<%=ProjectResource.GanttZoomLeft %>">
                <span class="move-left-button"></span> 
            </li>
            <li><div id="ganttChartZoom" class="gantt-chart-zoom" title="<%=ProjectResource.GanttTimlineTitle %>"></div></li>
            <li id="ganttChartMoveRight" class="menu-upload-button display-none" title="<%=ProjectResource.GanttZoomRight %>">
                <span class="move-right-button"></span> 
            </li>
        </ul>

    </div>
    <div class="addition mainPageLayout">
        <div class="filter-container">
            <div class="task-filter-container">
                <input id="openTaskOnly" type="checkbox" name="openTaskOnly"/>
                <label for="openTaskOnly"><%=ProjectResource.GanttOpenTaskOnly %></label>
            </div>
            <div class="task-filter-container" style="display:none;"> <!-- скрыла пока не определиться в каком формате будет этот фильтр -->
                <select id="teamMemberFilter">
                    <option value="-1"><%=ProjectResource.GanttAllTeamMembers %></option>
                </select>
            </div>
            <%--<div class="task-filter-container">
                <a class="link underline" href="Tasks.aspx?prjID=<%=Project.ID%>" target="_blank"><%=ProjectsJSResource.ToTaskList %></a>        если список будет формироваться из разных проектов, то эта ссылка не нужна
            </div>--%>
        </div>
        <div class="mode-button-container">
            <span class="refresh" title="<%=ProjectResource.RefreshChart%>"></span>
            <span class="print" title="<%=ProjectResource.GanttPrint%>"></span>
            <span id="showGanttHelp" class="HelpCenterSwitcher" title="<%=ProjectResource.GanttLegendTitle %>"></span>
            <%--<span class="mode full-screen active" title="<%=ProjectResource.GanttChartFullMode%>"></span>
            <span class="mode left-panel" title="<%=ProjectResource.GanttChartLeftPanelMode%>"></span>--%>
        </div>
    </div>

    <div id="ganttHelpPanel" class="studio-action-panel gantt-legend gantt-context-menu">
         <% if (!string.IsNullOrEmpty(HelpLink))
             { %>
        <a class="link underline blue to-full-help" href="<%= HelpLink %>" target="_blank"><%=ProjectResource.GanttSeeFullGuide %></a>
         <% } %>
        <div class="header-base middle"><%=ProjectResource.GanttLegend %>:</div>
        <i class="gray-text"><%=ProjectResource.GanttLegendSimbDesc %></i>
        <div class="clearFix">
            <ul class="dropdown-content cell">
                <li class="edit dropdown-item" title="<%= ProjectResource.GanttEditTitle%>"><%= ProjectResource.GanttEditTitle%></li>
                <li class="delete dropdown-item" title="<%= ProjectsCommonResource.Delete%>"><%= ProjectsCommonResource.Delete%></li>
                <li class="close dropdown-item" title="<%= ProjectResource.GanttClose%>"><%= ProjectResource.GanttClose%></li>
                <li class="addlink dropdown-item" title="<%= ProjectResource.GanttCreateLink%>"><%= ProjectResource.GanttCreateLink%></li>
            </ul>
            <ul class="dropdown-content cell">
                <li class="addMilestoneTask dropdown-item" title="<%= ProjectResource.GanttAddTask%>"><%= ProjectResource.GanttAddTask%></li>
                <li class="fitToScreen dropdown-item" title="<%= ProjectResource.GanttShowWhole%>"><%= ProjectResource.GanttShowWhole%></li>
                <li class="open dropdown-item" title="<%= ProjectResource.GanttReopen%>"><%= ProjectResource.GanttReopen%></li>
                <li class="responsible dropdown-item" title="<%= ProjectResource.GanttAssignResponsible%>"><%= ProjectResource.GanttAssignResponsible%></li>
            </ul>
        </div>
        <i class="gray-text"><%=ProjectResource.GanttLegendAppearence %></i>
        <table class="gantt-colors">
            <tr>
                <td><div class="closed" title="<%= ProjectResource.GanttLegendAppClosed%>"><%= ProjectResource.GanttLegendAppClosed%></div></td>
                <td><div class="create" title="<%= ProjectResource.GanttCreateLinkDesc%>"><%= ProjectResource.GanttCreateLinkDesc%></div></td>
            </tr>
            <tr>
                <td><div class="active" title="<%= ProjectResource.GanttLegendAppActive%>"><%=ProjectResource.GanttLegendAppActive%></div></td>
                <td><div class="invalid" title="<%= ProjectResource.GanttLegendInvalidLink%>"><%=ProjectResource.GanttLegendInvalidLink %></div></td>
            </tr>
            <tr>
                <td><div class="overdue" title="<%= ProjectResource.GanttLegendAppOverdue%>"><%=ProjectResource.GanttLegendAppOverdue %></div></td>
                <td><div class="priority" title="<%= ProjectResource.GanttHighPriority%>"><%=ProjectResource.GanttHighPriority %></div></td>
            </tr>
            <tr>
                <td><div class="infinity" title="<%= ProjectResource.GanttTaskWithoutDeadline %>"><%=ProjectResource.GanttTaskWithoutDeadline %></div></td>
                <td><div class="key" title="<%= MilestoneResource.RootMilestone%>"><%= MilestoneResource.RootMilestone%></div></td>
            </tr>
        </table>
        <i class="gray-text"><%=ProjectResource.GanttLegendHelpActionPanel %></i>        
    </div>

    <div id="ganttActions" class="popupContainerClass gantt-actions display-none">
        <div id="hideActionsButton" class="cancelButton"></div>
        <table class="gantt-actions-table">
            <tr>
                <td><%=ProjectResource.GanttRightEmptyClickDesc %></td>
                <td class="gray-text"><%=ProjectResource.GanttRightEmptyClick %></td>
            </tr>
            <tr>
                <td><%=ProjectResource.GanttRightEmptyClickStrafeDesc %></td>
                <td class="gray-text"><%=ProjectResource.GanttRightEmptyClickStrafe %></td>
            </tr>
            <tr>
                <td><%=ProjectResource.GanttRightClickTaskMilDesc %></td>
                <td class="gray-text"><%=ProjectResource.GanttRightClickTaskMil %></td>
            </tr>
            <tr>
                <td><%=ProjectResource.GanttEditTitle %></td>
                <td class="gray-text"><%=ProjectResource.GanttDoubleClickonTitle %></td>
            </tr>
            <tr>
                <td><%=ProjectResource.GanttAssignResponsible %></td>
                <td class="gray-text"><%=ProjectResource.GanttDoubleClickResp %></td>
            </tr>
            <tr>
                <td><%=ProjectResource.GanttLinkTasks %></td>
                <td class="gray-text"><%=ProjectResource.GanttLinkTasksDesc %></td>
            </tr>
        </table>
    </div>

    <div id="questionWindowTaskRemove" style="display: none">
        <sc:Container ID="_hintPopupTaskRemove" runat="server">
            <Header>
                <%= TaskResource.RemoveTask%>
            </Header>
            <Body>
                <p>
                    <%= TaskResource.RemoveTaskPopup%> 
                </p>
                <p id="noteAboutLinks" class="display-none"><%=ProjectsCommonResource.NoteTaskHaveLinks %></p>
                <p>
                    <%=ProjectsCommonResource.PopupNoteUndone %>&nbsp;<%=ProjectResource.GanttUndoNote %></p>
                <div class="errorBox display-none"></div>
                <div class="middle-button-container">
                    <a class="button blue middle remove"><%= TaskResource.RemoveTask%></a> 
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel%></a>
                </div>
            </Body>
        </sc:Container>
    </div>

    <div id="addNewLinkPopup" style="display: none">
        <sc:Container ID="_addNewLinkPopup" runat="server">
            <Header>
                <%=TaskResource.CreateNewLink%>
            </Header>
            <Body>
                <div>
                    <span class="bold"><%=TaskResource.Task %>:</span>
                    <span class="splitter-buttons"></span>
                    <span id="parentTaskName"></span>
                </div>
                <div class="task-select">
                    <span class="bold vertical-align-top"><%= ProjectResource.GanttLinkWith %>:</span>
                    <span class="splitter-buttons"></span>
                    <select id="dependentTaskSelect">
                        <option value="-1"><%=TaskResource.ChooseTaskForLink %></option>
                    </select>
                </div>
                <div class="link-type-select">
                    <span class="bold vertical-align-top"><%= ProjectResource.GanttLinkType %>:</span>
                    <span class="splitter-buttons"></span>
                    <span id="linkTypeSelector"></span>
                    
                    <%--<select id="linkTypeSelector">
                            <option value="-1" selected="selected"><%=TaskResource.ChooseLinkType %></option>
                            <option value="0"><%=ProjectsJSResource.RelatedLinkTypeSS %></option>
                            <option value="1"><%=ProjectsJSResource.RelatedLinkTypeEE %></option>
                            <option value="2"><%=ProjectsJSResource.RelatedLinkTypeSE %></option>
                            <option value="3"><%=ProjectsJSResource.RelatedLinkTypeES %></option>
                    </select>--%>
                </div>

                <div class="middle-button-container">
                    <a class="button blue middle save disable"><%=ProjectsCommonResource.Save%></a> 
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel%></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    
    <div id="questionWindowDeleteMilestone" style="display: none">
        <sc:Container ID="_hintPopupMilestoneRemove" runat="server">
            <Header>
                <%= MilestoneResource.DeleteMilestone %>
            </Header>
            <Body>
                <p><%= MilestoneResource.DeleteMilestonePopup %> </p>
                <div class="middle-button-container">
                    <a class="button blue middle remove"><%= MilestoneResource.DeleteMilestone %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    
    <div id="questionWindowMilestoneTasks" style="display: none">
        <sc:Container ID="_hintPopupMilestoneTasks" runat="server">
            <Header>
                <%= MilestoneResource.CloseMilestone %>
            </Header>
            <Body>
                <p><%= MilestoneResource.NotCloseMilWithActiveTasks %></p>
                <div class="middle-button-container">
                    <a class="button gray middle cancel"><%= ProjectResource.OkButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    
    <div id="questionWindowTaskWithSubtasks" style="display: none">
        <sc:Container ID="_hintPopupTaskWithSubtasks" runat="server">
            <Header>
                <%= TaskResource.ClosingTheTask%>
            </Header>
            <Body>
                <p><%= TaskResource.TryingToCloseTheTask%>.</p>
                <p><%= TaskResource.BetterToReturn%>.</p>
                <div class="middle-button-container">
                    <a class="button blue middle end">
                        <%= TaskResource.EndAllSubtasksCloseTask%></a> 
                        <span class="splitter-buttons"></span>
                        <a class="button gray middle cancel">
                        <%= ProjectsCommonResource.Cancel%></a>
                </div>
            </Body>
        </sc:Container>
    </div>

    <div id="moveTaskOutMilestone" style="display: none">
        <sc:Container ID="_moveTaskOutMilestone" runat="server">
            <Header>
                <%= ProjectsCommonResource.MoveTaskHeader %>
            </Header>
            <Body>
                <p><%= String.Format(ProjectsCommonResource.TaskMoveNote, "<br/>") %> </p>
                <div class="middle-button-container">
                    <a class="button blue middle move-all"><%=ProjectsCommonResource.AllTaskMoveButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button blue middle one-move"><%=ProjectsCommonResource.OneTaskMoveButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
                </div>
            </Body>
        </sc:Container>
    </div>

    <div id="emptyScreenForGanttLayout" class="gantt-empty-screen-layout">
        <div id="emptyScreenForGantt" class="gantt-empty-screen">
            <div class="header-empty-screen"><%=ProjectResource.GanttEmptyScreenTitle%></div>
            <p class="gray-text"><%=ProjectResource.GanttEmptyScreenDescription%></p>
        </div>
    </div>

    <div id="statusListContainer" class="studio-action-panel gantt-context-menu">
        <ul id="statusList" class="dropdown-content">
            <li class="open dropdown-item"><%= TaskResource.Open%></li>
            <li class="closed dropdown-item"><%= TaskResource.Closed%></li>
        </ul>
    </div>

    <div id="statusListTaskContainer" class="studio-action-panel gantt-context-menu">
        <ul id="statusListTask" class="dropdown-content">
            <% foreach(var s in Statuses) {  %>
            <li data-id="<%= s.Id %>" class="<%= s.Id %> dropdown-item" style="background: url('data:<%= s.ImageType %>;base64,<%= s.Image %>') no-repeat 2px 4px"><%= s.Title %></li>
            <%} %>
        </ul>
    </div>

    <!--Context Menus -->

    <div id="taskContextMenu" class="studio-action-panel gantt-context-menu">
        <ul class="dropdown-content">
            <li class="edit dropdown-item"><%= ProjectResource.GanttEditTitle%></li>
            <li class="delete dropdown-item"><%= ProjectsCommonResource.Delete%></li>
            <li id="taskStatus" class="closed dropdown-item" data-closetext="<%= ProjectResource.GanttClose%>" data-opentext="<%= ProjectResource.GanttReopen%>"><%= ProjectResource.GanttClose%></li>
            <li class="addlink dropdown-item"><%= ProjectResource.GanttCreateLink%></li>
            <li id="taskResponsible" class="responsible dropdown-item" ><%= ProjectResource.GanttAssignResponsible%></li>
        </ul>
    </div>

    <div id="milestoneContextMenu" class="studio-action-panel gantt-context-menu">
        <ul class="dropdown-content">
            <li class="edit dropdown-item"><%= ProjectResource.GanttEditTitle%></li>
            <li class="delete dropdown-item"><%= ProjectsCommonResource.Delete%></li>
            <li class="addMilestoneTask dropdown-item"><%= ProjectResource.GanttAddTask%></li>
            <li class="fitToScreen dropdown-item"><%= ProjectResource.GanttShowWhole%></li>
            <li id="milestoneStatus" class="closed dropdown-item" data-closetext="<%= ProjectResource.GanttClose%>" data-opentext="<%= ProjectResource.GanttReopen%>"><%= ProjectResource.GanttClose%></li>
            <li id="milestoneResponsible" class="responsible dropdown-item" ><%= ProjectResource.GanttAssignResponsible%></li>
        </ul>
    </div>

    <div id="responsiblesContainer" class="studio-action-panel gantt-context-menu">
        <ul class="dropdown-content">

        </ul>
        <div id="setResponsible" class="small-button-container"><span class="button gray"><%=ProjectResource.OkButton %></span></div>
    </div>

    <div class="fake-background gray"></div>
    <div id="ganttCanvasContainer">
        <canvas id="layer0" class="gantt-canvas layer0" width="640" height="480"> </canvas>
        <canvas id="layer1" class="gantt-canvas layer1" width="640" height="480"> </canvas>
    </div>
<asp:PlaceHolder ID="_milestoneAction" runat="server"/>
</asp:Content>
