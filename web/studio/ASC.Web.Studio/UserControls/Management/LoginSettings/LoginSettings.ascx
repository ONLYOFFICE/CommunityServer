<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LoginSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="studio_loginSettings" class="clearFix">
    <div class="settings-block">
        <div class="header-base clearFix">
            <div class="title"><%= Resource.LoginSettingsTitle %></div>
        </div>
        <div class="clearFix">
            <div class="header-base-small"><%= Resource.LoginAttemptsCount %>:</div>
            <div>
                <input type="text" class="textEdit" maxlength="4" id="studio_attemptsCount" value="<%= Settings.AttemptCount %>"/>
            </div>
            <div class="header-base-small"><%= Resource.LoginBlockTime %>:</div>
            <div>
                <input type="text" class="textEdit" maxlength="4" id="studio_blockTime" value="<%= Settings.BlockTime %>"/>
            </div>
            <div class="header-base-small"><%= Resource.LoginCheckPeriod %>:</div>
            <div>
                <input type="text" class="textEdit" maxlength="4" id="studio_checkPeriod" value="<%= Settings.CheckPeriod %>"/>
            </div>
        </div>
        <div class="middle-button-container">
            <a id="loginSettingsSaveBtn" class="button blue"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= string.Format(Resource.HelpAnswerLoginSettings.HtmlEncode(), "<b>", "</b>", "<br/>") %></p>
    </div>
</div>
