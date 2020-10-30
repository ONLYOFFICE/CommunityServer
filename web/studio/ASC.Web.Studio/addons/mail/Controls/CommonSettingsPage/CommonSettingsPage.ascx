<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonSettingsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.CommonSettingsPage" %>
<%@ Import Namespace="ASC.Mail.Data.Contracts" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div id="mailCommonSettings" class="hidden page_content">
    <div class="containerBodyBlock">
        <div>
            <br />
            <input type="checkbox" id="cbxEnableConversations" class="on-off-checkbox" <%= MailCommonSettings.EnableConversations ? "checked='checked'" : "" %> />
            <label for="cbxEnableConversations">
                        <%= MailResource.EnableConversationsSettingsLabel %>
            </label>
            <br />
            <br />
            <input type="checkbox" id="cbxDisplayAllImages" class="on-off-checkbox" <%= MailCommonSettings.AlwaysDisplayImages ? "checked='checked'" : "" %> />
            <label for="cbxDisplayAllImages">
                        <%= MailResource.AlwaysDisplayAllImagesSettingsLabel %>
            </label>
            <br />
            <br />
            <!--<input type="checkbox" id="cbxCacheUnreadMessages" class="on-off-checkbox" <%= MailCommonSettings.CacheUnreadMessages ? "checked='checked'" : "" %> />
            <label for="cbxCacheUnreadMessages">
                    <%= MailResource.CacheUnreadMessagesSettingsLabel %>
                    <br />
                    <span class="text-medium-describe"><%= MailResource.CacheUnreadMessagesSettingsHelperBlock %></span>
            </label>
            <br />
            <br />-->
            <input type="checkbox" id="cbxGoNextAfterMove" class="on-off-checkbox" <%= MailCommonSettings.GoNextAfterMove ? "checked='checked'" : "" %> />
            <label for="cbxGoNextAfterMove">
                    <%= MailResource.GoNextAfterMoveSettingsLabel %>
                    <br />
                    <span class="text-medium-describe"><%= MailResource.GoNextAfterMoveSettingsHelperBlock %></span>
            </label>
            <br />
            <br />
            <input type="checkbox" id="cbxReplaceMessageBody" class="on-off-checkbox" <%= MailCommonSettings.ReplaceMessageBody ? "checked='checked'" : "" %> />
            <label for="cbxReplaceMessageBody">
                    <%= MailResource.ReplaceMessageBody %>
            </label>
            <br />
            <br />
        </div>
    </div>
</div>

<div class="popup_helper" id="CacheUnreadMessagesHelperBlock">
    <p><%= MailResource.CacheUnreadMessagesSettingsHelperBlock %></p>
    <div class="cornerHelpBlock pos_top"></div>
</div>

