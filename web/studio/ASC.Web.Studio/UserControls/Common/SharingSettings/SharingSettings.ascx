<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SharingSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.SharingSettings" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
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
                <div class="header-base">
                    <%= UserControlsCommonResource.SharingSettingsItemsTitle %>
                </div>

                <div class="add-to-sharing-links borderBase clearFix">
                    <span id="shareUserSelector" class="addUserLink">
                        <a class="link dotline"><%= CustomNamingPeople.Substitute<Resource>("AddUsersForSharingButton") %></a>
                        <span class="sort-down-black"></span>
                    </span>
                    <span id="shareGroupSelector" class="addUserLink">
                        <a class="link dotline"><%= CustomNamingPeople.Substitute<Resource>("AddGroupsForSharingButton") %></a>
                        <span class="sort-down-black"></span>
                    </span>
                </div>

                <div id="sharingSettingsItems"></div>

                <% if (EnableShareMessage)
                   { %>
                <div id="shareMessagePanel">
                    <label>
                        <input type="checkbox" id="shareMessageSend" checked="checked" />
                        <%= UserControlsCommonResource.SendShareNotify %>
                    </label>
                    <a id="shareAddMessage" class="baseLinkAction linkMedium">
                        <%= UserControlsCommonResource.AddShareMessage %></a> <a id="shareRemoveMessage" class="baseLinkAction linkMedium">
                            <%= UserControlsCommonResource.RemoveShareMessage %></a>
                    <textarea id="shareMessage"></textarea>
                </div>

                <% } %>
                <% } %>
                <div class="middle-button-container clearFix">
                    <a id="sharingSettingsSaveButton" class="button blue middle"><%= Resource.SaveButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="sharing-cancel-button button middle gray"><%= Resource.CancelButton %></a>
                </div>
            </div>
        </Body>
    </sc:Container>
</div>
