<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectAction.ascx.cs" Inherits="ASC.Web.Projects.Controls.Projects.ProjectAction" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<div id="projectActionPage">
    <div id="pageHeader">
        <div class="pageTitle"><%= GetPageTitle() %></div>

    <div style="clear: both"></div>
    </div>

    <div id="projectTitleContainer" class="requiredField">
        <span class="requiredErrorText"><%= ProjectsJSResource.EmptyProjectTitle %></span>
        <div class="headerPanelSmall">
            <%= ProjectResource.ProjectTitle %>
        </div>
        <div class="inputTitleContainer">
            <asp:TextBox ID="projectTitle" Width=100% runat="server" CssClass="textEdit" MaxLength="250" autocomplete="off"></asp:TextBox>
        </div>
        <div style="clear: both;"></div>
    </div>
    <div class="popup_helper" id="AnswerForFollowProject">
      <p><%=String.Format(ProjectsCommonResource.HelpAnswerFollowProject, "<br />", "<b>", "</b>")%></p>
    </div>     

    <div id="projectDescriptionContainer">
        <div class="headerPanelSmall">
            <%= ProjectResource.ProjectDescription %>
        </div>
        <div class="dottedHeaderContent">
            <asp:TextBox ID="projectDescription" Width=100% runat="server" TextMode="MultiLine" Rows="6" autocomplete="off"></asp:TextBox>
        </div>
    </div>

    <div id="projectManagerContainer" class="requiredField">
        <span class="requiredErrorText"><%= ProjectsJSResource.EmptyProjectManager %></span>
        <div class="headerPanelSmall">
            <%= ProjectResource.ProjectLeader %>
        </div>
        <asp:PlaceHolder ID="projectManagerPlaceHolder" runat="server"></asp:PlaceHolder>
        <div class="notifyManagerContainer">
            <input id="notifyManagerCheckbox" type="checkbox"/>
            <label for="notifyManagerCheckbox"><%= ProjectResource.NotifyProjectManager %></label>
                <% if (IsEditingProjectAvailable()) { %>
                    <input type="hidden" value="<%= Project.Responsible %>" id="projectResponsible"/>
                <% } %>
        </div>
        <div style="clear: both;"></div>
    </div>

    <% if (!IsEditingProjectAvailable()) { %>
        <div id="projectTeamContainer">
                <div class="headerPanelSmall">
                    <%= ProjectResource.ProjectTeam %>            
            </div>
            <div class="dottedHeaderContent">
                <a id="addNewPerson" class="baseLinkAction "><%=ProjectResource.ManagmentTeam%></a>
                <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForProjectTeam'});" title="<%=ProjectsCommonResource.HelpQuestionProjectTeam%>"></div> 
                <div class="popup_helper" id="AnswerForProjectTeam">
                    <p><%=String.Format(ProjectsCommonResource.HelpAnswerProjectTeam, "<br />", "<b>", "</b>")%><br />
                    <a href="<%= CommonLinkUtility.GetHelpLink() + "gettingstarted/projects.aspx#ManagingYourTeam_block" %>" target="_blank"><%=ProjectsCommonResource.LearnMoreLink%></a></p>
                </div>    
             </div>     
                <div id="projectParticipantsContainer" class="participantsContainer">
                    <table class="canedit"></table>
                </div>
                <asp:PlaceHolder runat="server" ID="projectTeamPlaceHolder"/>
                <input type="hidden" id="projectParticipants"/>
       
        </div>
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
    <% } else { %>
        <div id="projectStatusContainer">
            <div class="headerPanel clearFix"><div class="float-left"><%= ProjectResource.ProjectStatus %></div>
                <div class="HelpCenterSwitcher" style="margin-top:3px;" onclick="jq(this).helper({ BlockHelperID: 'AnswerForProjectStatus'});" title="<%=ProjectsCommonResource.HelpQuestionProjectStatus%>"></div> 
                <div class="popup_helper" id="AnswerForProjectStatus">
                      <p><%=String.Format(ProjectsCommonResource.HelpAnswerProjectStatus, "<br />", "<b>", "</b>")%></p>
                </div>     
            </div>
            <select id="projectStatus" class="comboBox">
                <option value="open" <%if(Project.Status == ProjectStatus.Open){%> selected="selected" <%} %>><%= ProjectResource.ActiveProject %></option>
                <option value="paused"<%if(Project.Status == ProjectStatus.Paused){%> selected="selected" <%} %>><%= ProjectResource.PausedProject %></option>
                <option value="closed " <%if(Project.Status == ProjectStatus.Closed){%> selected="selected" <%} %>><%= ProjectResource.ClosedProject %></option>
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
            <input id="projectTags" class="textEdit" maxlength="8000" value="<%=ProjectTags%>" placeholder="<%= ProjectResource.EnterTheTags %>"/>
            <div id="tagsAutocompleteContainer"></div>
        </div>
    </div>

    <div id="projectVisibilityContainer">
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
            <input id="projectPrivacyCkeckbox" type="checkbox" <%= RenderProjectPrivacyCheckboxValue() %>/>
            <label for="projectPrivacyCkeckbox"><%= ProjectResource.IUnerstandForEditHidden %></label>
        </div>
   
    </div>
     <% if (!IsEditingProjectAvailable()) { %>
     <div id="projectFollowContainer">
            <div class="followingCheckboxContainer">
                <input  id="followingProjectCheckbox" type="checkbox"/>
                <label for="followingProjectCheckbox"><%= ProjectResource.FollowingProjects %></label>
                <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForFollowProject'});" title="<%=ProjectsCommonResource.HelpQuestionFollowProject%>"></div> 
            </div>
    </div>
        <% } %>

    <div id="projectActionsContainer" class="big-button-container">
        <a id="projectActionButton" class="button blue big">
            <%= GetProjectActionButtonTitle() %>
        </a>
        <span class="splitter-buttons"></span>
        <% if (IsEditingProjectAvailable()) { %>
            <a id="cancelEditProject" href="<%= UrlProject%>" class="button gray big">
               <%= ProjectsCommonResource.Cancel %>
            </a>
            <span class="splitter-buttons"></span>
        <%if(ProjectSecurity.CurrentUserAdministrator){ %>
            <a id="projectDeleteButton" class="button gray big">
                <%=ProjectResource.ProjectDeleted%>
            </a>
            <% } %>
        <% } else { %>
            <a id="cancelEditProject" class="button gray big"><%= ProjectsCommonResource.Cancel %></a>
        <% } %>
    </div>
</div>
<div id="questionWindowDeleteProject" style="display: none">
    <sc:Container ID="_hintPopupDeleteProject" runat="server">
        <Header><%= ProjectResource.DeleteProject %></Header>
        <Body>        
            <p><%= ProjectResource.DeleteProjectPopup %> </p>
            <p><%= ProjectsCommonResource.PopupNoteUndone %></p>
            <div class="middle-button-container">
                <a class="button blue middle remove"><%= ProjectResource.DeleteProject %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a> 
            </div>            
        </Body>
    </sc:Container>
</div>

<div id="questionWindowActiveTasks" style="display: none">
    <sc:Container ID="_hintPopupActiveTasks" runat="server">
    <Header>
    <%= ProjectResource.CloseProject %>
    </Header>
    <Body>        
        <p><%= ProjectResource.NotClosePrjWithActiveTasks %></p>
        <div class="middle-button-container">
            <a class="button blue middle" href="<%= GetActiveTasksUrl() %>"><%= ProjectResource.ViewActiveTasks %></a>    
            <span class="splitter-buttons"></span>
            <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a>
        </div>
    </Body>
    </sc:Container>
</div>

<div id="questionWindowActiveMilestones" style="display: none">
    <sc:Container ID="_hintPopupActiveMilestones" runat="server">
    <Header>
    <%= ProjectResource.CloseProject %>
    </Header>
    <Body>        
        <p><%= ProjectResource.NotClosedPrjWithActiveMilestone %></p>
        <div class="middle-button-container">
            <a class="button blue middle" href="<%= GetActiveMilestonesUrl() %>"><%= ProjectResource.ViewActiveMilestones %></a>   
            <span class="splitter-buttons"></span>
            <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel %></a> 
        </div>    
    </Body>
    </sc:Container>
</div>

<script id="projectParticipant" type="text/x-jquery-tmpl">
    <tr participantId="${ID}">
        <td class="name">
            <span class="userLink">${Name}</span>
        </td>
        <td class="department">
            <span>${Group.Name}</span>
        </td>
        <td class="title">
            <span>${Title}</span>
        </td>
        <td class="delMember">
            <span userId="${ID}" title="<%=ProjectsCommonResource.Delete %>"></span>
        </td>
    </tr>
</script>