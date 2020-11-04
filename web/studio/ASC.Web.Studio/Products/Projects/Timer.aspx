<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="Timer.aspx.cs" Inherits="ASC.Web.Projects.Timer" %>

<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <style type="text/css">
        #studioPageContent main {
            padding: 0;
        }
        #studioPageContent .mainPageLayout {
            min-width: 270px;
        }
        .mainContainerClass {
            float: left;
        }
        .containerBodyBlock, .mainPageLayout {
            padding: 0;
            overflow: hidden;
        }
        .mainContainerClass {
            border: medium none;
        }
        body {
            overflow: auto;
        }
        #studioPageContent{
            min-width: 270px;
        }
</style>

<% if (Project != null)
{ %>                   
    <div id="timerTime">
        <div id="firstViewStyle">
            <div class="h">00</div>
            <div class="m">00</div>
            <div class="s">00</div>
            <div class="clearfix">
                <div class="start button blue" title="<%= ProjectsCommonResource.AutoTimerStart %>" 
                    data-title-pause="<%= ProjectsCommonResource.AutoTimerPause %>"
                    data-title-start="<%= ProjectsCommonResource.AutoTimerStart %>" ><div></div></div>
                <div class="reset button gray" title="<%= ProjectsCommonResource.AutoTimerReset %>">
                    <div class="refresh-icon"></div>
                </div>
            </div>
        </div>
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.Project %>:</b>
            </div>
            <select id="selectUserProjects" class="comboBox">
                <% foreach (var project in UserProjects) %>
                <%
                   { %>
                    <% if (project.ID == Project.ID) %>
                    <%
                       { %>
                    <option selected="selected" value="<%= project.ID %>" id="optionUserProject_<%= project.ID %>"><%= project.Title.HtmlEncode() %></option>
                    <% } %>
                    <%
                       else %>
                    <%
                       { %>
                    <option value="<%= project.ID %>" id="optionUserProject_<%= project.ID %>"><%= project.Title.HtmlEncode() %></option>
                    <% } %>
                <% } %>
            </select>
        </div>
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= TaskResource.Task %>:</b>
            </div>
            <select id="selectUserTasks" class="comboBox">
                <optgroup id="openTasks" label="<%= TimeTrackingResource.OpenTasks %>">
                <% foreach (var task in OpenUserTasks) %>
                <%
                   { %>
                    <option <% if (Target != -1 && task.ID == Target)
                               { %> selected="selected"<% } %> value="<%= task.ID %>" id="optionUserTask_<%= task.ID %>"><%= task.Title.HtmlEncode() %></option>
                <% } %>
                </optgroup>
                <optgroup id="closedTasks" label="<%= TimeTrackingResource.ClosedTasks %>">
                <% foreach (var task in ClosedUserTasks) %>
                <%
                   { %>
                    <option <% if (Target != -1 && task.ID == Target)
                               { %> selected="selected"<% } %> value="<%= task.ID %>"  id="optionUserTask_<%= task.ID %>"><%= task.Title.HtmlEncode() %></option>
                <% } %>
                </optgroup>
            </select>
        </div>
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= TaskResource.ByResponsible %>:</b>
            </div>
            <select id="teamList" class="comboBox">
                <% foreach (var user in Users) %>
                <% { %>
                    <% if (user.ID == Participant.ID) %>
                    <% { %>
                        <option selected="selected" value="<%= user.ID %>" id="optionUser_<%= user %>"><%= DisplayUserSettings.GetFullUserName(user.UserInfo) %></option>
                    <% }
                       else if (!Participant.IsVisitor)
                       { %>
                        <option value="<%= user.ID %>" id="optionUser_<%= user.ID %>"><%= DisplayUserSettings.GetFullUserName(user.UserInfo) %></option>
                    <% } %>
                <% } %>
            </select>
        </div>  
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= TimeTrackingResource.Date %>:</b>
            </div>
            <input type="text" id="inputDate" class="textEditCalendar" autocomplete="off"/>
        </div>      
        <div class="block-cnt-splitter time-fields-container">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.ManualTimeInput %>:</b>
            </div>
            <input id="inputTimeHours" type="text" placeholder="<%= ProjectsCommonResource.WatermarkHours %>" class="textEdit" maxlength="2" />
            <span class="splitter">:</span>
            <input id="inputTimeMinutes" type="text" placeholder="<%= ProjectsCommonResource.WatermarkMinutes %>" class="textEdit" maxlength="2" />
            <div id="timeTrakingErrorPanel"></div>
        </div>
        <div class="block-cnt-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.ProjectDescription %>:</b>
            </div>
            <textarea id="textareaTimeDesc" style="resize:none;max-height:55px;height:55px;" MaxLength="250" cols="22" rows="3"></textarea>
        </div>
        <div class="block-cnt-splitter">
            <a class="button blue middle" id="addLog">
                <%= ProjectsCommonResource.AutoTimerLogHours %>
            </a>
        </div>    
    </div>
<% }
   else
   { %>
   <div id="timerTime">
    <div class="timer-img"></div>
    <p class="message-for-empty-timer"><%=TimeTrackingResource.MessageForNoTaskForTrack%></p>
   </div>
<% } %>

</asp:Content>