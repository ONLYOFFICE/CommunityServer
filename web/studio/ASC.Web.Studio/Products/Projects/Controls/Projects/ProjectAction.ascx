<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectAction.ascx.cs" Inherits="ASC.Web.Projects.Controls.Projects.ProjectAction" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<div id="projectActionPage">
    <div id="pageHeader">
        <div class="pageTitle"><%= PageTitle %></div>
        <div style="clear: both"></div>
    </div>
    <% if (!IsEditingProjectAvailable && TemplatesCount > 0)
       { %>
    <div id="templateContainer" class="block-cnt-splitter">
        <div class="headerPanelSmall">
            <%= ProjectResource.SelectProjectTemplate %>
        </div>
        
        <span id="templateSelect" data-id="" class="link dotline advansed-select-container">
            <%= ProjectsCommonResource.Select %>
        </span>
    </div>
    <% } %>
    <div id="projectTitleContainer" class="block-cnt-splitter requiredField">
        <span class="requiredErrorText"><%= ProjectsJSResource.EmptyProjectTitle %></span>
        <div class="headerPanelSmall">
            <%= ProjectResource.ProjectTitle %>
        </div>
        <div class="inputTitleContainer">
            <asp:TextBox autocomplete="off" CssClass="textEdit" ID="projectTitle" MaxLength="250" runat="server" Width="100%"></asp:TextBox>
        </div>
        <div style="clear: both;"></div>
    </div>
    <div class="popup_helper" id="AnswerForFollowProject">
        <p><%=String.Format(ProjectsCommonResource.HelpAnswerFollowProject, "<br />", "<b>", "</b>")%></p>
    </div>

    <div id="projectDescriptionContainer" class="block-cnt-splitter">
        <div class="headerPanelSmall">
            <%= ProjectResource.ProjectDescription %>
        </div>
        <div class="dottedHeaderContent">
            <asp:TextBox ID="projectDescription" Width="100%" runat="server" TextMode="MultiLine" Rows="6" autocomplete="off"></asp:TextBox>
        </div>
    </div>

    <div id="projectManagerContainer" class="block-cnt-splitter requiredField">
        <span class="requiredErrorText"><%= ProjectsJSResource.EmptyProjectManager %></span>
        <div class="headerPanelSmall">
            <%= ProjectResource.ProjectLeader %>
        </div>
        <div>
           <% if (Project == null || CoreContext.UserManager.GetUsers(Project.Responsible).IsVisitor())
              {%>
            <span id="projectManagerSelector" data-id="" class="link dotline plus"><%= ProjectResource.AddProjectManager %></span>
            <%} else { %>
            <span id="projectManagerSelector" data-id="<%= Project.Responsible.ToString()%>" class="link dotline"><%= ProjectManagerName %></span>
            <%} %>
        </div>
        <div class="notifyManagerContainer">
            <%  if(!HideChooseTeam)
                {%>
            <input id="notifyManagerCheckbox" type="checkbox" />
            <label for="notifyManagerCheckbox"><%= ProjectResource.NotifyProjectManager %></label>
            <% }%>
            <% if (IsEditingProjectAvailable)
               { %>
            <input type="hidden" value="<%= Project.Responsible %>" id="projectResponsible" />
            <% } %>
        </div>
        <div style="clear: both;"></div>
    </div>


    <div id="projectTeamContainer" class="block-cnt-splitter <%if (HideChooseTeam) {%> display-none<%} %>">
        <div class="headerPanelSmall clearFix">
            <div class="float-left">
                <%= ProjectResource.ProjectTeam %>
            </div>
            <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForProjectTeam'});" title="<%=ProjectsCommonResource.HelpQuestionProjectTeam%>"></div>
            <div class="popup_helper" id="AnswerForProjectTeam">
                <p>
                    <%=String.Format(ProjectsCommonResource.HelpAnswerProjectTeam, "<br />", "<b>", "</b>")%><br />
                     <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
                       { %>
                    <a href="<%= CommonLinkUtility.GetHelpLink() + "gettingstarted/projects.aspx#ManagingYourTeam_block" %>" target="_blank"><%=ProjectsCommonResource.LearnMoreLink%></a>
                    <% } %>
                </p>
            </div>
        </div>

        <div>
            <span id="projectTeamSelector" class="link dotline plus"><%= ProjectResource.ManagmentTeam%></span>
        </div>
        <div id="projectParticipantsContainer" class="participantsContainer">
            <ul class="items-display-list"></ul>
        </div>
        <input type="hidden" id="projectParticipants" />
    </div>
    <asp:PlaceHolder ID="ControlPlaceHolder" runat="server"/>
    <%if (IsProjectCreatedFromCrm)
      { %>
    <div id="projectContactsContainer">
        <div class="headerPanelSmall"><%= ProjectsCommonResource.ProjectLinkedWithContacts %></div>
        <div class="no-linked-contacts"><%= ProjectsCommonResource.NoCRMContactsForLink %></div>
        <div id="contactListBox">
            <table id="contactTable" class="tableBase" cellpadding="4" cellspacing="0">
                <tbody>
                </tbody>
            </table>
        </div>
    </div>
    <% } %>
    <% if (IsEditingProjectAvailable)
       { %>
    <div id="projectStatusContainer">
        <div class="headerPanel clearFix">
            <div class="float-left"><%= ProjectResource.ProjectStatus %></div>
            <div class="HelpCenterSwitcher" style="margin-top: 3px;" onclick="jq(this).helper({ BlockHelperID: 'AnswerForProjectStatus'});" title="<%=ProjectsCommonResource.HelpQuestionProjectStatus%>"></div>
            <div class="popup_helper" id="AnswerForProjectStatus">
                <p><%=String.Format(ProjectsCommonResource.HelpAnswerProjectStatus, "<br />", "<b>", "</b>")%></p>
            </div>
        </div>
        <select id="projectStatus" class="comboBox">
            <option value="open" <%if (Project.Status == ProjectStatus.Open)
                                   {%> selected="selected" <%} %>><%= ProjectResource.ActiveProject %></option>
            <option value="paused" <%if (Project.Status == ProjectStatus.Paused)
                                     {%> selected="selected" <%} %>><%= ProjectResource.PausedProject %></option>
            <option value="closed " <%if (Project.Status == ProjectStatus.Closed)
                                      {%> selected="selected" <%} %>><%= ProjectResource.ClosedProject %></option>
        </select>
    </div>
    <input type="hidden" id="activeTasks" value="<%= ActiveTasksCount %>" />
    <input type="hidden" id="activeMilestones" value="<%= ActiveMilestonesCount %>" />
    <% } %>
    <div id="projectTagsContainer" class="dottedHeader">
        <div class="headerPanelSmall">
            <%= ProjectResource.Tags %>
        </div>
        <div class="dottedHeaderContent">
            <input id="projectTags" class="textEdit" maxlength="8000" value="<%=ProjectTags%>" placeholder="<%= ProjectResource.EnterTheTags %>" />
            <div id="tagsAutocompleteContainer"></div>
        </div>
    </div>

    <% if (!ProjectSecurity.IsPrivateDisabled) { %>
    <div id="projectVisibilityContainer">
        <div class="headerPanelSmall clearFix">
            <div class="popup_helper" id="AnswerForPrivateProject">
                <p>
                    <%=String.Format(ProjectsCommonResource.HelpAnswerPrivateProject, "<br />", "<b>", "</b>")%><br />
                     <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
                       { %>
                    <a href="<%= CommonLinkUtility.GetHelpLink() + "gettingstarted/projects.aspx#ManagingYourTeam_block" %>" target="_blank"><%=ProjectsCommonResource.LearnMoreLink%></a>
                    <% } %>
                </p>
            </div>
        </div>
        <div class="checkboxPrivateProj">
            <input id="projectPrivacyCkeckbox" type="checkbox" <% if(RenderProjectPrivacyCheckboxValue) {%> <%="checked" %><%} %> />
            <label for="projectPrivacyCkeckbox"><%= ProjectResource.IUnerstandForEditHidden %></label>
            <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForPrivateProject'});" title="<%=ProjectsCommonResource.HelpQuestionPrivateProject%>"></div>
        </div>
    </div>
    <% } %>
    <% if (!IsEditingProjectAvailable && !HideChooseTeam)
       { %>
    <div id="projectFollowContainer">
        <div class="followingCheckboxContainer">
            <input id="followingProjectCheckbox" type="checkbox" />
            <label for="followingProjectCheckbox"><%= ProjectResource.FollowingProjects %></label>
            <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForFollowProject'});" title="<%=ProjectsCommonResource.HelpQuestionFollowProject%>"></div>
        </div>
    </div>
    <% } %>

    <div id="projectActionsContainer" class="big-button-container">
        <a id="projectActionButton" class="button blue big">
            <%= ProjectActionButtonTitle %>
        </a>
        <span class="splitter-buttons"></span>
        <% if (IsEditingProjectAvailable)
           { %>
        <a id="cancelEditProject" href="<%= UrlProject%>" class="button gray big">
            <%= ProjectsCommonResource.Cancel %>
        </a>
        <span class="splitter-buttons"></span>
        <%if (ProjectSecurity.CurrentUserAdministrator)
          { %>
        <a id="projectDeleteButton" class="button gray big">
            <%=ProjectResource.ProjectDeleted%>
        </a>
        <% } %>
        <% }
           else
           { %>
        <a id="cancelEditProject" class="button gray big"><%= ProjectsCommonResource.Cancel %></a>
        <% } %>
    </div>
</div>
<div id="questionWindowDeleteProject" style="display: none">
    <sc:Container ID="_hintPopupDeleteProject" runat="server">
        <header><%= ProjectResource.DeleteProject %></header>
        <body>
            <p><%= ProjectResource.DeleteProjectPopup %> </p>
            <p><%= ProjectsCommonResource.PopupNoteUndone %></p>
            <div class="middle-button-container">
                <a class="button blue middle remove"><%= ProjectResource.DeleteProject %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="questionWindowActiveTasks" style="display: none">
    <sc:Container ID="_hintPopupActiveTasks" runat="server">
        <header>
            <%= ProjectResource.CloseProject %>
        </header>
        <body>
            <p><%= ProjectResource.NotClosePrjWithActiveTasks %></p>
            <div class="middle-button-container">
                <a class="button blue middle" href="<%= ActiveTasksUrl %>"><%= ProjectResource.ViewActiveTasks %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="questionWindowActiveMilestones" style="display: none">
    <sc:Container ID="_hintPopupActiveMilestones" runat="server">
        <header>
            <%= ProjectResource.CloseProject %>
        </header>
        <body>
            <p><%= ProjectResource.NotClosedPrjWithActiveMilestone %></p>
            <div class="middle-button-container">
                <a class="button blue middle" href="<%= ActiveMilestonesUrl %>"><%= ProjectResource.ViewActiveMilestones %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
            </div>
        </body>
    </sc:Container>
</div>

<script id="projectParticipant" type="text/x-jquery-tmpl">
    <li class="items-display-list_i" participantid="${id}">
        <span class="item-name"><a class="link" href="${profileUrl}" target="_blank">${title}</a></span>
        <div class="reset-action" data-userid="${id}" title="<%=ProjectsCommonResource.Delete %>"></div>
    </li>
</script>
