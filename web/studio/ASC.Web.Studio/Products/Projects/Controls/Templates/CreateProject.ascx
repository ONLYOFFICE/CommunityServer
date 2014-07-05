
<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateProject.ascx.cs" Inherits="ASC.Web.Projects.Controls.Templates.CreateProject" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div class="display-none">
    <p id="milestoneError"><%=ProjectTemplatesResource.MilestoneError %></p>
    <p id="taskError"><%=ProjectTemplatesResource.TaskError %></p>
</div>

<div class="projects-templates-container">
<div id="pageHeader">
    <div class="pageTitle"><%= ProjectTemplatesResource.CreateProjFromTmpl%></div>
    <div style="clear: both"></div>
</div>
<div id="projectTitleContainer" class="requiredField">
    <span class="requiredErrorText"><%=ProjectTemplatesResource.TitleErrorProject %></span>  
    <div class="templTitle headerPanelSmall"><%=ProjectTemplatesResource.TemplateTitle %></div>
    <input id="projectTitle" type="text" class="textEdit" defText="<%=ProjectTemplatesResource.DefaultProjTitle %>"/>
</div>
    
<div class="templTitle headerPanelSmall"><%=ProjectTemplatesResource.Description %></div>
    <textarea cols="20" rows="5" id="projectDescription" class="textEdit"></textarea> 

<div id="pmContainer" class="requiredField">
    <span class="requiredErrorText"><%=ProjectTemplatesResource.ErrorManager %></span>
    <div class="templTitle headerPanelSmall"><%=ProjectTemplatesResource.DefinePM %></div>
    <div id="projectManagerContainer" class="requiredField">
        <asp:PlaceHolder ID="projectManagerPlaceHolder" runat="server"></asp:PlaceHolder>
        <% if (IsAdmin) { %>
            <div class="notifyContainer">
                <input id="notifyManagerCheckbox" disabled="disabled" type="checkbox" checked="checked"/>
                <label for="notifyManagerCheckbox"><%=ProjectTemplatesResource.NotifyPM%></label>
            </div>
        <% } %>
        <div style="clear: both;"></div>
    </div>
</div>

<div class="templTitle headerPanelSmall"><%=ProjectTemplatesResource.DefineTeam %></div>
<div id="projectTeamContainer">
    <div id="Team" class="participantsContainer">
        <table>
        </table>        
    </div>
    <span id="manageTeamButton" class="dottedLink"><%= ProjectTemplatesResource.AddTeamMembers%></span>

    <asp:PlaceHolder runat="server" ID="projectTeamPlaceHolder" />
</div>
      
<div class="templTitle headerPanelSmall"><%=ProjectTemplatesResource.EditProjStructure %></div>

<div id="listAddedMilestone">
    
</div>

<div id="addMilestone"><a class="baseLinkAction"><%=ProjectResource.AddMilestone%></a></div>

<p class="unlocatedTaskTitle"><%=ProjectTemplatesResource.TasksWithoutMilestone%></p>
<div id="noAssignTaskContainer">
    <div id="listNoAssignListTask"></div>
    <div class="addTaskContainer">
           <a class="baseLinkAction"><%=ProjectResource.AddTask %></a>
    </div>
</div>

<div id="addMilestoneContainer" target="">
    <div>
        <input class="textEditCalendar" id="dueDate" type="text"/>
        <input id="newMilestoneTitle" type="text" placeholder="<%=ProjectTemplatesResource.AddMilestoneTitle %>"/>
    </div>
    <div>
        <a class="button gray">Ok</a>
    </div>
</div>

<div id="addTaskContainer"  target="">
    <div>
        <input id="newTaskTitle" type="text" placeholder="<%=ProjectTemplatesResource.AddTaskTitle %>"/>
    </div>
    <div>
        <a class="button gray">Ok</a>
    </div>
</div>
<div class="notifyContainer">
    <div class="headerPanelSmall clearFix">
        <div class="float-left">
        <%= ProjectResource.HiddenProject %>
        </div>
        <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForPrivateProject'});" title="<%=ProjectsCommonResource.HelpQuestionPrivateProject%>"></div> 
        <div class="popup_helper" id="AnswerForPrivateProject">
                 <p><%=String.Format(ProjectsCommonResource.HelpAnswerPrivateProject, "<br />", "<b>", "</b>")%><br />
                 <a href="<%= CommonLinkUtility.GetHelpLink() + "gettingstarted/projects.aspx#ManagingYourTeam_block" %>" target="_blank"><%=ProjectsCommonResource.LearnMoreLink%></a></p>
        </div>   
    </div>
    <div class="checkboxPrivateProj">
        <input id="projectPrivacyCkeckbox" type="checkbox" />
        <label for="projectPrivacyCkeckbox"><%= ProjectResource.IUnerstandForEditHidden %></label>
    </div>
</div>

<div class="notifyContainer">
     <input id="notifyResponsibles" type="checkbox"/>
     <label for="notifyResponsibles"><%=ProjectTemplatesResource.NotifyResponsibles%></label>
</div>

<div  class="buttonContainer big-button-container">
    <a id="createProject" class="button blue big"><%=ProjectTemplatesResource.CreateProject %></a>
    <span class="splitter-buttons"></span>
    <a id="cancelCreateProjectTemplate" class="button gray big"><%=ProjectsCommonResource.Cancel%></a>
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

<div id="projectMemberPanel" class="studio-action-panel">
	<div class="corner-top right"></div>
	<ul class="dropdown-content actionList" nobodyItemText="<%=ProjectTemplatesResource.NoResponsible %>" chooseRespText="<%=ProjectTemplatesResource.ChooseResponsible %>" class="actionList">

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