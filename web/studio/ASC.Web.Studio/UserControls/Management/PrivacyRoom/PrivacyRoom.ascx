<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PrivacyRoom.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PrivacyRoom" %>
<%@ Import Namespace="Resources" %>


<div class="clearFix">
    <div id="studio_encryptionSettings" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.StudioEncryptionSettings %>
        </div>

        <div class="clearFix">
            <div class="clearFix">
                <input id="encryption_on" type="radio" name="EncryptionStatus" <%= EncryptionEnabled ? "checked='checked'" : "" %> />
                <label for="encryption_on"><%= Resource.EncryptionOn %></label>
            </div>
            <div class="clearFix">
                <input id="encryption_off" type="radio" name="EncryptionStatus" <%= EncryptionEnabled ? "" : "checked='checked'" %> />
                <label for="encryption_off"><%= Resource.EncryptionOff %></label>
            </div>
        </div>

        <div class="middle-button-container">
            <a class="button blue" id="saveEncryption" href="javascript:void(0);">
                <%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpPrivacyRoomSettings.HtmlEncode(),
                "<br />", "<b>", "</b>",
                "<a href=\"https://www.onlyoffice.com/download-desktop.aspx\" target=\"_blank\">",
                "</a>",
                "<a href=\"https://www.onlyoffice.com/private-rooms.aspx\" target=\"_blank\">",
                "</a>") %></p>
    </div>
</div>
