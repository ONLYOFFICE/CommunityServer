<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddMilestoneContainer.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.AddMilestoneContainer" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

    <div class="display-none">
        <p id="milestoneError"><%=ProjectTemplatesResource.MilestoneError %></p>
        <p id="taskError"><%=ProjectTemplatesResource.TaskError %></p>
    </div>
    <div class="block-cnt-splitter">
        <div class="headerPanelSmall"><%= ProjectTemplatesResource.EditProjStructure %></div>

        <div id="listAddedMilestone">
        </div>

        <div id="addMilestone"><a class="link dotline plus"><%= ProjectResource.AddMilestone %></a></div>

        <p class="unlocatedTaskTitle"><%= ProjectTemplatesResource.TasksWithoutMilestone %></p>
        <div id="noAssignTaskContainer">
            <div id="listNoAssignListTask"></div>
            <div class="addTaskContainer">
                <a class="link dotline plus"><%= ProjectResource.AddTask %></a>
            </div>
        </div>

        <div id="addMilestoneContainer" target="">
            <div>
                <% if (Edit)
                   { %> 
                    <select>
                        <option selected="selected" duration="0.5" value='<%= ChooseMonthNumeralCase(0.5) %>'><%= ChooseMonthNumeralCase(0.5) %></option>
                        <% for (double i = 1; i <= 12; i = i + 0.5)
                           { %>
                        <option duration="<%= i.ToString(CultureInfo.InvariantCulture) %>" value='<%= ChooseMonthNumeralCase(i) %>'><%= ChooseMonthNumeralCase(i) %></option>
                        <% } %>
                    </select>
                <% }
                   else {%>
                    <input class="textEditCalendar" id="dueDate" type="text" />
                <% }%>
                <input id="newMilestoneTitle" type="text" placeholder="<%=ProjectTemplatesResource.AddMilestoneTitle %>" maxlength="250" />
            </div>
            <div>
                <a class="button gray">Ok</a>
            </div>
        </div>

        <div id="addTaskContainer" target="">
            <div>
                <input id="newTaskTitle" type="text" placeholder="<%=ProjectTemplatesResource.AddTaskTitle %>" maxlength="250" />
            </div>
            <div>
                <a class="button gray">Ok</a>
            </div>
        </div>
        <div id="taskActionPanel" class="studio-action-panel">
            <ul class="actionList dropdown-content">
                <li id="editTask" class="dropdown-item"><%=ProjectTemplatesResource.Edit %></li>
                <li id="removeTask" class="dropdown-item"><%=ProjectTemplatesResource.Delete %></li>
            </ul>
        </div>

        <div id="milestoneActions" class="studio-action-panel">
            <ul class="actionList dropdown-content">
                <li id="editMilestone" class="dropdown-item"><%=ProjectTemplatesResource.Edit %></li>
                <li id="addTaskInMilestone" class="dropdown-item"><%=ProjectTemplatesResource.AddTask%></li>
                <li id="removeMilestone" class="dropdown-item"><%=ProjectTemplatesResource.Delete %></li>
            </ul>
        </div>

        <div id="projectMemberPanel" class="studio-action-panel">
            <ul class="dropdown-content actionList" nobodyitemtext="<%=ProjectTemplatesResource.NoResponsible %>" chooseresptext="<%=ProjectTemplatesResource.ChooseResponsible %>" class="actionList">
            </ul>
        </div>
</div>