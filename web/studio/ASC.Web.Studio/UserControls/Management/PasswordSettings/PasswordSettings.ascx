<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PasswordSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PasswordSettings" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="Resources" %>

<% if (Enabled)
   { %>
<div class="clearFix">
     <div id="studio_passwordSettings" class="settings-block">
         <div class="header-base clearFix">
             <%= Resource.StudioPasswordSettings %>
         </div>

        <div class="clearFix slider">
            <div class="header-base-small">
                <%= Resource.PasswordMinLength %></div>
            <div class="clearFix passwordLengthBox">
                <div class="sliderPassword">
                    <div id="slider" data-min="<%= ((PasswordSettings)new PasswordSettings().GetDefault()).MinLength %>" data-max="<%= PasswordSettings.MaxLength %>"></div>
                </div>
                <div class="float-left" id="count">
                </div>
                <div class="countLabel float-left">
                <%= Resource.PasswordSymbolsCountLabel %>
                </div>
            </div>
        </div>
        <div class="clearFix fieldsBox">
            <div class="clearFix">
                <input type="checkbox" id="chkUpperCase" />
                <label for="chkUpperCase"><%= Resource.PasswordUseUpperCase %></label>
            </div>
            <div class="clearFix">
                <input type="checkbox" id="chkDigits" />
                <label for="chkDigits"><%= Resource.PasswordUseDigits %></label>
            </div>
            <div class="clearFix">
                <input type="checkbox" id="chkSpecSymbols" />
                <label for="chkSpecSymbols"><%= Resource.PasswordUseSpecialSymbols %></label>
            </div>
        </div>

        <div class="middle-button-container">
            <a class="button blue" id="savePasswordSettingsBtn" href="javascript:void(0);">
                <%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerPasswordSettings.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
            <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#ChangingSecuritySettings_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>
<% } %>