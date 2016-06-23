<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<!--list tasks popups-->
<script id="projects_taskRemoveWarning" type="text/x-jquery-tmpl">
            <p>
                <%= TaskResource.RemoveTaskPopup%>
            </p>
            <p>
                <%=ProjectsCommonResource.PopupNoteUndone %></p>
            <div class="errorBox display-none"></div>
            <div class="middle-button-container">
                <a id="popupRemoveTaskButton" class="button blue middle remove"><%= TaskResource.RemoveTask%></a> 
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel%></a>
            </div>

            <div class="hidden-title-text display-none"><%= TaskResource.RemoveTask%></div>
</script>

<script id="projects_moveTaskPopup" type="text/x-jquery-tmpl">
            <div class="describe-text"><%= TaskResource.Task %></div>
            <div class="taskTitls ms"><b id="moveTaskTitles"></b></div>
            <div class="describe-text ms"><%= TaskResource.WillBeMovedToMilestone%>:</div>
            <div class="milestonesList">
                <div class="milestonesButtons">
                    <input id="ms_0" type="radio" name="milestones" value="0" />
                    <label for="ms_0"><%= TaskResource.None%></label>
                </div>
            </div>

            <div class="middle-button-container">
                <a href="javascript:void(0)" class="button blue middle">
                    <%= TaskResource.MoveToMilestone%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:void(0)">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
            </div>
    <div class="hidden-title-text display-none"><%= TaskResource.MoveTaskToAnotherMilestone%></div>
</script>

<script id="projects_closedTaskQuestion" type="text/x-jquery-tmpl">
    <p><%= TaskResource.TryingToCloseTheTask%>.</p>
    <p><%= TaskResource.BetterToReturn%>.</p>
    <div class="middle-button-container">
        <a class="button blue middle end">
            <%= TaskResource.EndAllSubtasksCloseTask%></a> 
            <span class="splitter-buttons"></span>
            <a class="button gray middle cancel">
            <%= ProjectsCommonResource.Cancel%></a>
    </div>
    <div class="hidden-title-text display-none"><%= TaskResource.ClosingTheTask%></div>
</script>

<!--list projects popups-->
<script id="projects_projectOpenTaskWarning" type="text/x-jquery-tmpl">
    <p><%=ProjectResource.NotClosePrjWithActiveTasks%></p>
    <div class="middle-button-container">
        <a class="button blue middle" id="linkToTasks">
            <%= ProjectResource.ViewActiveTasks%></a> 
        <span class="splitter-buttons"></span>
        <a class="button gray middle">
            <%= ProjectsCommonResource.Cancel%></a>
    </div>
    <div class="hidden-title-text display-none"><%= ProjectResource.CloseProject%></div>
</script>

<script id="projects_projectOpenMilestoneWarning" type="text/x-jquery-tmpl">
    <p><%=ProjectResource.NotClosedPrjWithActiveMilestone%></p>
    <div class="middle-button-container">
        <a class="button blue middle" id="linkToMilestines">
            <%= ProjectResource.ViewActiveMilestones%></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle">
            <%= ProjectsCommonResource.Cancel%></a>
    </div>
    <div class="hidden-title-text display-none"><%= ProjectResource.CloseProject%></div>
</script>

<!--list milestones popups-->

<script id="projects_milestoneRemoveWarning" type="text/x-jquery-tmpl">
    <p><%= MilestoneResource.DeleteMilestonePopup %> </p>
    <p><%= ProjectsCommonResource.PopupNoteUndone %></p>
    <div class="middle-button-container">
        <a class="button blue middle remove"><%= MilestoneResource.DeleteMilestone %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
    </div>
    <div class="hidden-title-text display-none"><%= MilestoneResource.DeleteMilestone%></div>
</script>

<script id="projects_closeMilestoneWithOpenTasks" type="text/x-jquery-tmpl">
    <p><%= MilestoneResource.NotCloseMilWithActiveTasks %></p>
    <div class="middle-button-container">
        <a class="button blue middle" id="linkToTasksPage"><%= ProjectResource.ViewActiveTasks %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle"><%= ProjectsCommonResource.Cancel %></a>
    </div>
    <div class="hidden-title-text display-none"><%= MilestoneResource.CloseMilestone%></div>
</script>

<!--list time-traking popups-->

<script id="projects_ttakingRemoveWarning" type="text/x-jquery-tmpl">
    <p><%= TimeTrackingResource.DeleteTimersQuestion%></p>
    <p><%=ProjectsCommonResource.PopupNoteUndone %></p>
    <div class="errorBox display-none"></div>
    <div class="middle-button-container">
        <a id="deleteTimersButton" class="button blue middle"><%= TimeTrackingResource.DeleteTimers%></a> 
        <span class="splitter-buttons"></span>
        <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel%></a>
    </div>
    <div class="hidden-title-text display-none"><%= TimeTrackingResource.DeleteTimers%></div>
</script>

<script id="projects_editTimerPopup" type="text/x-jquery-tmpl">           
        <div id="TimeLogTaskTitle" class="header-base block-cnt-splitter"></div>
           
        <div class="addLogPanel-infoPanel">
            <div class="addLogPanel-infoPanelBody">
                <span class="header-base gray-text">
                    <%= ProjectsCommonResource.SpentTotally %>
                </span>
                <span class="splitter-buttons"></span>
                <span id="TotalHoursCount" class="header-base"></span>
                <span class="splitter-buttons"></span>
            </div>
        </div> 
        <div class="warnBox" style="display:none;" id="timeTrakingErrorPanel"></div>
        <div class="block-cnt-splitter" style="float:right">
            <div class="headerPanelSmall">
                <b><%= TaskResource.TaskResponsible %>:</b>
            </div>
            <select style="width: 220px;" class="comboBox pm-report-select" id="teamList"></select>
        </div>
        
        <div>   
        <div class="block-cnt-splitter" style="float:left;margin-right:20px">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.Time%>:</b>
            </div>
            <input id="inputTimeHours" type="text" placeholder="<%=ProjectsCommonResource.WatermarkHours %>" class="textEdit" maxlength="2" />
            <span class="splitter">:</span>
            <input id="inputTimeMinutes" type="text" placeholder="<%=ProjectsCommonResource.WatermarkMinutes %>" class="textEdit" maxlength="2" />
        </div>
           
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.Date %>:</b>
            </div>
            <input id="timeTrakingDate" class="textEditCalendar" style="margin-right: 3px"/>
        </div>
        </div>
        
        <div style="clear:both"></div>
           
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.ProjectDescription %>:</b>
            </div>
            <textarea id="timeDescription" rows="7" cols="20" maxlength="250"></textarea>
        </div>
         
        <div class="middle-button-container">
            <a href="javascript:void(0)" class="button blue middle">
                <%= ProjectsCommonResource.SaveChanges%>
            </a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                <%= ProjectsCommonResource.Cancel%>
            </a>
        </div>
    <div class="hidden-title-text display-none"><%= ProjectsCommonResource.TimeTracking %></div>
</script>