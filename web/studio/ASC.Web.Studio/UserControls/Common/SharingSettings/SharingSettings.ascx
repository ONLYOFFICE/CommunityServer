<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SharingSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.SharingSettings" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_sharingSettingsDialog">
    <sc:Container ID="_sharingDialogContainer" runat="server">
        <Header>
            <span class="share-container-head"><%= UserControlsCommonResource.SharingSettingsTitle %></span>
        </Header>
        <Body>
            <div id="sharingSettingsDialogBody">
                <% if (!CoreContext.Configuration.Personal)
                   { %>
                <div class="add-to-sharing-links">
                    <span id="shareUserSelector" class="addUserLink button middle gray"><%= Resource.AddUsersForSharingButton %></span>
                    <span id="shareUserDefAction" class="share-default-action button middle gray"><span></span></span>
                    <div id="shareUserDefSelect" class="studio-action-panel share-def-action"></div>
                    <span class="splitter-buttons"></span>
                    <span id="shareGroupSelector" class="addUserLink button middle gray"><%= Resource.AddGroupsForSharingButton %></span>
                    <span id="shareGroupDefAction" class="share-default-action button middle gray"><span></span></span>
                    <div id="shareGroupDefSelect" class="studio-action-panel share-def-action"></div>
                    <a id="shareGetLink" class="button middle gray" title="<%= UserControlsCommonResource.GetPortalLink %>">&nbsp;</a>
                </div>

                <div id="sharingSettingsItems"></div>
                <div class="sharing-empty describe-text"><%= string.Format(UserControlsCommonResource.SharingSettingsEmpty.HtmlEncode(), "<br />") %></div>

                <% if (EnableShareMessage)
                   { %>
                <div id="shareMessagePanel">
                    <label>
                        <input type="checkbox" id="shareMessageSend" checked="checked" class="checkbox" />
                        <%= UserControlsCommonResource.SendShareNotify %>
                    </label>
                    <a id="shareAddMessage" class="baseLinkAction linkMedium"><%= UserControlsCommonResource.AddShareMessage %></a>
                    <a id="shareRemoveMessage" class="baseLinkAction linkMedium"><%= UserControlsCommonResource.RemoveShareMessage %></a>
                </div>
                <textarea id="shareMessage"></textarea>
                <% } %>

                <% } %>
                <div class="middle-button-container clearFix">
                    <a class="sharing-notchanged-buttons sharing-cancel-button button middle gray"><%= Resource.CloseButton %></a>
                    <div class="sharing-changed-buttons">
                        <a class="sharing-save-button button blue middle"><%= Resource.SaveButton %></a>
                        <span class="splitter-buttons"></span>
                        <a class="sharing-cancel-button button middle gray"><%= Resource.CancelButton %></a>
                    </div>
                </div>
            </div>
        </Body>
    </sc:Container>
</div>
