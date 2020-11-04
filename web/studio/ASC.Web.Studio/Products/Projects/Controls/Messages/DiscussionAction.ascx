<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DiscussionAction.ascx.cs"     Inherits="ASC.Web.Projects.Controls.Messages.DiscussionAction" %>

<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<div id="discussionActionPage">
    <div id="pageHeader">
        <div class="pageTitle"><%= GetPageTitle() %></div>
        <div style="clear: both"></div>
    </div>

    <% if (Discussion == null && Project == null) { %>

    <div id="discussionProjectContainer" class="requiredField">
        <span class="requiredErrorText">
            <%= MessageResource.SelectProject %></span>
        <div class="headerPanelSmall">
            <%= ProjectResource.Project %>
        </div>
        
        <span id="discussionProjectSelect" data-id="" class="link dotline advansed-select-container">
            <%= ProjectsCommonResource.Select %>
        </span>
    </div>

    <% } %>

    <table width="100%">
        <tr>
            <td>
    <div id="discussionTitleContainer" class="requiredField">
        <span class="requiredErrorText">
            <%= ProjectsJSResource.EmptyMessageTitle %></span>
        <div class="headerPanelSmall">
            <%= MessageResource.MessageTitle %>
        </div>
        <asp:TextBox ID="discussionTitle" Width="100%" runat="server" CssClass="textEdit"
            MaxLength="250" />
    </div>
    <div id="discussionTextContainer" class="requiredField">
        <span class="requiredErrorText">
            <%= MessageResource.EmptyMessageText %></span>
        <div class="headerPanelSmall">
            <%= MessageResource.MessageContent %>
        </div>
        <textarea id="ckEditor" name="ckEditor" style="width: 100%;height: 400px;"><%=Text %></textarea>
    </div>
            </td>
            <% if (!MobileDetector.IsMobile)
               { %>
            <td class="teamlab-cut">
                <div class="title-teamlab-cut"><%: ProjectsCommonResource.TeamlabCutTitle %></div>
                <div class="text-teamlab-cut"><%: ProjectsCommonResource.TeamlabCutText %></div>
            </td>
            <% } %>
        </tr>
    </table>

    <div id="discussionTabs">
        <div>
            <div class="tabs-section" container="discussionParticipantsContainer">
                <span class="header-base"><%= MessageResource.DiscussionParticipants %></span>
                <span id="switcherParticipantsButton" class="toggle-button" data-switcher="0" 
                    data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
                    <%= ProjectsCommonResource.Hide %>
                </span>
            </div>
        </div>
            <div id="discussionParticipantsContainer" class="participantsContainer" <%if(Project != null) {%> data-private = "<%=Project.Private %>"<%} %>>
            <div class="inviteMessage">
                <%= MessageResource.SubscribePeopleInfoComment %></div>
            <div id="manageParticipantsSelector" class="link dotline plus"><%=ProjectsCommonResource.AddParticipants%></div>
            <ul id="discussionParticipants" class="items-display-list">
            </ul>
            <div style="clear: both;">
            </div>
        </div>    
    </div>
</div>
<div id="discussionPreviewContainer">
    <div class="middle-button-container">
        <a id="hideDiscussionPreviewButton" class="button blue">
            <%= ProjectsCommonResource.Collapse %></a>
    </div>
</div>

<div class="popup_helper" id="hintSubscribersPrivateProject">
      <p><%=ProjectsCommonResource.hintSubscribersPrivateProject %></p>
</div>