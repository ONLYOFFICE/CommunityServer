<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TagsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.TagsPage" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div id="id_tags_page" class="hidden page_content">
    <div class="containerBodyBlock"></div>
</div>


<div class="popup_helper" id="TagsHelperBlock">
    <p><%=MailResource.TagsCommonInformationText%></p>
    <p><%=MailResource.TagsCommonNotificationText%></p>
    <div class="cornerHelpBlock pos_top"></div>
</div>


<div id="tagActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="editAccount dropdown-item with-icon edit"><%=MailResource.Edit%></a></li>
        <li><a class="deleteAccount dropdown-item with-icon delete"><%=MailScriptResource.Delete%></a></li>
    </ul>
</div>

<div id="addTagsPanel" class="actionPanel stick-over">
    <div id="tagsPanelContent" style="display: block;">
        <div class="actionPanelSection">
            <label for="markallrecipients" class="mark_all_checkbox">
                <input type="checkbox" id="markallrecipients"/>
                <span  id="markallrecipientsLabel"><%=MailScriptResource.MarkAllSendersLabel%></span>
            </label>
        </div>
        <div class="existsTags webkit-scrollbar"></div>
        <div class="h_line"></div>
        <div class="createTagMenu">
            <a title="<%: MailResource.CreateNewTagBtn %>" class="link entertag_button"><%: MailResource.CreateNewTagBtn %></a>
        </div>
    </div>
</div>