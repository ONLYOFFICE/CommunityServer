<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DiscussionDetails.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Messages.DiscussionDetails" %>

<%@ Import Namespace="ASC.Projects.Core.Domain"%>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility.HtmlUtility" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


    <div id="discussionActions" class="studio-action-panel">
        <ul class="dropdown-content">
            <% if (CanEdit){%>
            <li>
                <a id="changeStatus" class="dropdown-item" updateStatus="<%= Discussion.Status == MessageStatus.Open ? (int)MessageStatus.Archived : (int)MessageStatus.Open %>">
                    <%= Discussion.Status == MessageStatus.Open ? MessageResource.ArchiveDiscussion : MessageResource.OpenDiscussion %>
                </a>
            </li>
            <li><a href="<%= string.Format("messages.aspx?prjID={0}&id={1}&action=edit", Project.ID, Discussion.ID) %>" class="dropdown-item"><%= MessageResource.EditMessage %></a></li>
            <li><a id="deleteDiscussionButton" discussionid="<%= Discussion.ID %>" class="dropdown-item"><%= MessageResource.DeleteMessage %></a></li>
                <% if (Page.RequestContext.CanCreateTask(true) && Project.Status == ProjectStatus.Open)
                   {%>
                <li><a id="createTaskOnDiscussion" class="dropdown-item"><%= MessageResource.CreateTaskOnDiscussion %></a></li>
                <% } %>
            <% } %>

        </ul>
    </div>

<div class="clearFix">
    <div style="float: left;">
        <img src="<%= Author.GetBigPhotoURL()%>" alt="<%= Author.DisplayUserName() %>" class="discussiondetails-avatar" />
    </div>
    <div style="margin-left: 111px;">
        <div style="margin-bottom: 3px; color: #83888D;">
            <span><%= Discussion.CreateOn.ToString("d") %></span>
            <span style="padding-left: 16px"><%=  Discussion.CreateOn.ToString("t") %></span>
        </div>
        <div style="margin-bottom: 3px;">
            <span class="discussiondetails-author-caption"><%= MessageResource.AuthorTitle %>:</span>
            <a href="<%= Author.GetUserProfilePageURL() %>" class="discussiondetails-author">
                <%= Author.DisplayUserName() %>
            </a>
        </div>
        <div style="margin-bottom: 19px;">      
            <span class="discussiondetails-project-caption"><%= ProjectResource.Project %>:</span>
            <a href="<%= string.Format("tasks.aspx?prjID={0}", Project.ID) %>" class="discussiondetails-project">
                <%= Project.Title.HtmlEncode() %>
            </a>
        </div>
        <div>
            <%=  HtmlUtility.GetFull(Discussion.Description) %>
        </div>
    </div>
</div>

<div id="discussionTabs">
    <div class="tabs-section" container="discussionParticipantsContainer">
        <span class="header-base"><%= GetTabTitle(ParticipiantCount, MessageResource.DiscussionParticipants) %></span>
        <span id="switcherDiscussionParticipants" class="toggle-button" data-switcher="1" 
                data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
                <%= ProjectsCommonResource.Show %>
        </span>
    </div>
    <div id="discussionParticipantsContainer" class="participantsContainer" style="display: none" data-private = "<%=Project.Private %>">
        <% if (CanEdit){%>
        <div id="manageParticipantsSelector" class="link dotline plus"><%=ProjectsCommonResource.AddParticipants%></div>

        <% } %>
        <ul id="discussionParticipantsTable" class="items-display-list">
        </ul>
        <div style="clear: both;"></div>
    </div>
        
<% if (FilesAvailable) { %>
    <div class="tabs-section" count="<%= FilesCount %>" container="discussionFilesContainer">
        <span class="header-base"> <%= GetTabTitle(FilesCount, ProjectsCommonResource.DocsModuleTitle) %>
        </span>
        <span id="switcherFilesButton" class="toggle-button" data-switcher="0" 
                data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
                <%= ProjectsCommonResource.Hide %>
        </span>              
    </div>
    <div id="discussionFilesContainer" class="tabsContent" data-canEdit = "<%=CanEditFiles %>" data-projectFolderId = "<%=ProjectFolderId %>" data-projectName="<%= Project.Title.HtmlEncode() %>">
        <asp:PlaceHolder runat="server" ID="discussionFilesPlaceHolder" />
    </div>
<% } %>
<% if (CommentsAvailable)
   { %>
<div class="tabs-section" container="discussionCommentsContainer">
    <span class="header-base"><%= GetTabTitle(int.Parse(string.IsNullOrEmpty(CommentsCountTitle) ? "0" : CommentsCountTitle), MessageResource.Comments) %></span>
    <span id="switcherCommentsButton" class="toggle-button" data-switcher="0" 
                data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
                <%= ProjectsCommonResource.Hide %>
    </span>          
</div>
<div id="discussionCommentsContainer">
    <div id="commentsContainer" style="display: none">
        <scl:CommentsList ID="discussionComments" runat="server" BehaviorID="discussionComments">
        </scl:CommentsList>
    </div>
</div>  
<% } %>   
</div>
<input id="hiddenProductId" type="hidden" value="<%= ProductEntryPoint.ID.ToString() %>" />
<div id="questionWindow" style="display: none">
    <sc:Container ID="_hintPopup" runat="server">
        <header><%= MessageResource.DeleteMessage %></header>
        <body>
            <p>
                <%= MessageResource.DeleteDiscussionPopup %>
            </p>
            <p>
                <%= ProjectsCommonResource.PopupNoteUndone %></p>
            <div class="middle-button-container">
                <a class="button blue middle remove">
                    <%= MessageResource.DeleteMessage %></a> 
                <span class="splitter-buttons"></span>
                <a class="button gray middle cancel">
                    <%= ProjectsCommonResource.Cancel %></a>
            </div>
        </body>
    </sc:Container>
</div>
<div class="popup_helper" id="hintSubscribersPrivateProject">
      <p><%=ProjectsCommonResource.hintSubscribersPrivateProject %></p>
</div> 
<input type="hidden" id="discussionStatus" value="<%= Discussion.Status %>"/>