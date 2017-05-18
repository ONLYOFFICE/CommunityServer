<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonSettingsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.CommonSettingsPage" %>
<%@ Import Namespace="ASC.Mail.Aggregator.Common" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div id="mailCommonSettings" class="hidden page_content">
    <span class="header-base"><%= MailResource.CommonSettingsLabel %></span>
    <br />
    <br />
    <label>
        <input type="checkbox" id="cbxEnableConversations" class="checkbox" <%= MailCommonSettings.EnableConversations ? "checked='checked'" : "" %> />
                <%= MailResource.EnableConversationsSettingsLabel %>
    </label>
    <br />
    <br />
    <label>
        <input type="checkbox" id="cbxDisplayAllImages" class="checkbox" <%= MailCommonSettings.AlwaysDisplayImages ? "checked='checked'" : "" %> />
                <%= MailResource.AlwaysDisplayAllImagesSettingsLabel %>
    </label>
    <br />
    <br />                                                                                                                 
    <label>
        <input type="checkbox" id="cbxCacheUnreadMessages" class="checkbox" <%= MailCommonSettings.CacheUnreadMessages ? "checked='checked'" : "" %> />
            <%= MailResource.CacheUnreadMessagesSettingsLabel %>
            <br />
            <span class="text-medium-describe"><%= MailResource.CacheUnreadMessagesSettingsHelperBlock %></span>
    </label>
    <br />
    <br />
    <label>
        <input type="checkbox" id="cbxGoNextAfterMove" class="checkbox" <%= MailCommonSettings.GoNextAfterMove ? "checked='checked'" : "" %> />
            <%= MailResource.GoNextAfterMoveSettingsLabel %>
            <br />
            <span class="text-medium-describe"><%= MailResource.GoNextAfterMoveSettingsHelperBlock %></span>
    </label>
    <br />
    <br />
</div>

<div class="popup_helper" id="CacheUnreadMessagesHelperBlock">
    <p><%= MailResource.CacheUnreadMessagesSettingsHelperBlock %></p>
    <div class="cornerHelpBlock pos_top"></div>
</div>

