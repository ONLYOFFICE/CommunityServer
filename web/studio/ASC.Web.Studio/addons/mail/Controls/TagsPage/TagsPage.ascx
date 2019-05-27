<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TagsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.TagsPage" %>

<div id="id_tags_page" class="hidden page_content">
    <div class="containerBodyBlock">
        <div class="content-header">
            <a title="<%=ASC.Web.Mail.Resources.MailResource.CreateNewTagBtn%>" class="button gray" id="createNewTag">
                <div class="plus" style="background-position: -2px 1px;"><%=ASC.Web.Mail.Resources.MailResource.CreateNewTagBtn%></div>
            </a>
            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'TagsHelperBlock'});"></span>
        </div>
    </div>
</div>


<div class="popup_helper" id="TagsHelperBlock">
    <p><%=ASC.Web.Mail.Resources.MailResource.TagsCommonInformationText%></p>
    <% if (!CustomMode) { %>
    <p><%=ASC.Web.Mail.Resources.MailResource.TagsCommonNotificationText%></p>
    <% } %>
    <div class="cornerHelpBlock pos_top"></div>
</div>


<div id="tagActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="editAccount dropdown-item"><%=ASC.Web.Mail.Resources.MailResource.Edit%></a></li>
        <li><a class="deleteAccount dropdown-item"><%=ASC.Web.Mail.Resources.MailScriptResource.Delete%></a></li>
    </ul>
</div>