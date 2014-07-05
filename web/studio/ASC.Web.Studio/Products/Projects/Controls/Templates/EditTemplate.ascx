<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditTemplate.ascx.cs" Inherits="ASC.Web.Projects.Controls.Templates.EditTemplate" %>

<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="projects-templates-container">    
<div class="display-none">
    <p id="milestoneError"><%=ProjectTemplatesResource.MilestoneError %></p>
    <p id="taskError"><%=ProjectTemplatesResource.TaskError %></p>
</div>

<div id="pageHeader">
    <div class="pageTitle"><%= GetPageTitle() %></div>

<div style="clear: both"></div>
</div>

<div id="templateTitleContainer" class="requiredField">
    <span class="requiredErrorText"><%=ProjectTemplatesResource.TitleErrorTemplate %></span>  
    <div class="templTitle headerPanelSmall"><%=ProjectTemplatesResource.TemplateTitle %>:</div>
    <input id="templateTitle" type="text" value="" class="textEdit"/>
</div>
    
<div class="templTitle headerPanelSmall"><%=RequestContext.IsInConcreteProjectModule ? ProjectTemplatesResource.EditTmplStructure : ProjectTemplatesResource.TemplateStructure %></div>

<div id="listAddedMilestone">
    
</div>

<div id="addMilestone"><a class="baseLinkAction"><%=ProjectTemplatesResource.AddMilestone%></a></div>

<p class="unlocatedTaskTitle"><%=ProjectTemplatesResource.TasksWithoutMilestone%></p>
<div id="noAssignTaskContainer">
    <div id="listNoAssignListTask"></div>
    <div class="addTaskContainer">
           <a class="baseLinkAction"><%=ProjectTemplatesResource.AddTask %></a>
    </div>
</div>

<div id="addMilestoneContainer" target="">
    <div>
        <select>
            <option selected="selected" duration="0.5" value='<%=ChooseMonthNumeralCase(0.5)%>'><%=ChooseMonthNumeralCase(0.5)%></option>
        <% for (double i  = 1; i  <= 12; i = i + 0.5)
           {%>
            <option duration="<%=i.ToString() %>" value='<%=ChooseMonthNumeralCase(i)%>'><%=ChooseMonthNumeralCase(i)%></option>
           <%} %>
        </select>
        <input id="newMilestoneTitle" placeholder="<%=ProjectTemplatesResource.AddMilestoneTitle %>"/>
    </div>
    <div>
        <a class="button gray">Ok</a>
    </div>
</div>

<div id="addTaskContainer" target="">
    <div>
        <input id="newTaskTitle" placeholder="<%=ProjectTemplatesResource.AddTaskTitle %>"/>
    </div>
    <div>
        <a class="button gray">Ok</a>
    </div>
</div>
<div  class="buttonContainer big-button-container">
    <a id="saveTemplate" class="button blue big">
        <%= !String.IsNullOrEmpty(UrlParameters.EntityID) ? ProjectTemplatesResource.SaveChanges :  ProjectTemplatesResource.SaveTemplate%>
    </a>
    <span class="splitter-buttons"></span>
    <a id="createProject" href="javascript:void(0)" class="button gray big">
        <%= !String.IsNullOrEmpty(UrlParameters.EntityID) ? ProjectTemplatesResource.CreateProjFromTmpl : ProjectTemplatesResource.SaveAndCreateProjFromTmpl%>
    </a>
    <span class="splitter-buttons"></span>
    <a id="cancelCreateProjectTemplate" class="button gray big" href="projectTemplates.aspx"><%=ProjectsCommonResource.Cancel%></a>
</div>

<div id="taskActionPanel" class="studio-action-panel">
	<div class="corner-top right"></div>
	<ul class="actionList dropdown-content">
	    <li id="editTask" class="dropdown-item"><%=ProjectTemplatesResource.Edit %></li>
	    <li id="removeTask" class="dropdown-item"><%=ProjectTemplatesResource.Delete %></li>
	</ul>        
</div>

<div id="milestoneActions" class="studio-action-panel">
	<div class="corner-top right"></div>
	<ul class="actionList dropdown-content">
	    <li id="editMilestone" class="dropdown-item"><%=ProjectTemplatesResource.Edit %></li>
	    <li id="addTaskInMilestone" class="dropdown-item"><%=ProjectTemplatesResource.AddTask%></li>
	    <li id="removeMilestone" class="dropdown-item"><%=ProjectTemplatesResource.Delete %></li>
	</ul>        
</div>
</div>

<div id="attentionPopup" style="display: none">
    <sc:Container ID="_attantion" runat="server">
    <Header>
        <%= ProjectTemplatesResource.AttantionUnsavedData%>
    </Header>
    <Body>
        <p><%= ProjectTemplatesResource.AttantionUnsavedData%> </p>
        <div class="middle-button-container">
            <a class="button blue middle continue"><%= ProjectTemplatesResource.ProjTmplContinue%></a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
        </div>
    </Body>
    </sc:Container>
</div>
