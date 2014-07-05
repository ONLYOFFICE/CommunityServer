<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DiscussionDetails.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Messages.DiscussionDetails" %>
<%@ Import Namespace="ASC.Notify.Recipients" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Projects.Core.Domain"%>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility.HtmlUtility" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


    <div id="discussionActions" class="studio-action-panel">
        <div class="corner-top left"></div>
        <ul class="dropdown-content">
            <% if (CanEdit){%>
            <li><a href="<%= string.Format("messages.aspx?prjID={0}&id={1}&action=edit", Project.ID, Discussion.ID) %>" class="dropdown-item"><%= MessageResource.EditMessage %></a></li>
            <li><a id="deleteDiscussionButton" discussionid="<%= Discussion.ID %>" class="dropdown-item"><%= MessageResource.DeleteMessage %></a></li>
                <% if(RequestContext.CanCreateTask(true) && Project.Status == ProjectStatus.Open)
                   {%>
                <li><a id="createTaskOnDiscussion" class="dropdown-item"><%= MessageResource.CreateTaskOnDiscussion %></a></li>
                <% } %>          
            <% } %>
            <li id="changeSubscribeButton">
            </li>

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
            <%=  HtmlUtility.GetFull(Discussion.Content, ProductEntryPoint.ID) %>
        </div>
    </div>
</div>

<div id="discussionTabs">
    <div class="tabs-section" container="discussionParticipantsContainer">
        <span class="header-base"><%= GetTabTitle(discussionParticipantRepeater.Items.Count, MessageResource.DiscussionParticipants) %></span>
        <span id="switcherDiscussionParticipants" class="toggle-button" data-switcher="1" 
                data-showtext="<%= ProjectsCommonResource.Show %>" data-hidetext="<%= ProjectsCommonResource.Hide %>">
                <%= ProjectsCommonResource.Show %>
        </span>        
    </div>
    <div id="discussionParticipantsContainer" class="participantsContainer" style="display: none" data-private = "<%=Project.Private %>">
        <% if (CanEdit){%>
        <span class="manage-participants-button addUserLink">
            <span class="dottedLink"><%=ProjectsCommonResource.AddParticipants%></span> 
        </span>
        <% } %>
        <table id="discussionParticipantsTable">
            <asp:Repeater ID="discussionParticipantRepeater" ItemType="ParticipiantWrapper" runat="server">
                <ItemTemplate>
                     <tr class="discussionParticipant <%# Item.CanRead ? "" : "gray" %>" guid="<%# Item.ID %>">
                        <td class="name">
                            <span class="userLink"><%# Item.FullUserName %></span>
                        </td>
                        <td class="department">
                            <span><%# Item.Department %></span>
                        </td>
                         <td class="title">
                             <span><%# Item.Title %></span>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
            
            <% var currentParticipiant = new ParticipiantWrapper(Page.Participant.ID.ToString(), Discussion);%>
            <tr class="discussionParticipant hidden <%= currentParticipiant.CanRead ? "" : "gray" %>" id="currentLink" guid="<%= currentParticipiant.ID %>">
                <td class="name">
                    <span class="userLink"><%= currentParticipiant.FullUserName %></span>
                 </td>
                 <td class="department">
                    <span><%= currentParticipiant.Department %></span>
                 </td>
                 <td class="title">
                    <span><%= currentParticipiant.Title %></span>
                 </td>
            </tr>
        </table>
        <div style="clear: both;"></div>
        <% if (CanEdit) {%>
            <asp:PlaceHolder ID="discussionParticipantsSelectorHolder" runat="server"></asp:PlaceHolder>
        <% } %>
    </div>
        
<% if(CanReadFiles)
    if ((CanEditFiles && !MobileDetector.IsMobile) || (FilesCount > 0)) { %>
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
    
<div class="tabs-section" container="discussionCommentsContainer">
    <span class="header-base"><%= GetTabTitle(int.Parse(discussionComments.CommentsCountTitle), MessageResource.Comments) %></span>
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
