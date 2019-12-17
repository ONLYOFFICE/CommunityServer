<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CookieSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.CookieSettings" %>
<%@ Import Namespace="Resources" %>

<% if (Enabled)
   { %>
<div class="clearFix">
    <div id="studio_cookieSettings" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%= Resource.CookieSettingsTitle%>
        </div>
        <div class="clearFix">
            <div class="clearFix">
                <div class="clearFix">
                    <input id="cookieSettingsOff" type="radio" name="cookieSettingsSwitch" <%= LifeTime > 0 ? "" : "checked=\"checked\"" %> />
                    <label for="cookieSettingsOff"><%= Resource.IPSecurityDisable %></label>
                </div>
                <div class="clearFix">
                    <input id="cookieSettingsOn" type="radio" name="cookieSettingsSwitch" <%= LifeTime > 0 ? "checked=\"checked\"" : "" %> />
                    <label for="cookieSettingsOn"><%= Resource.IPSecurityEnable %></label>
                </div>
            </div>

            <div id="lifeTimeSettings" class="<%= LifeTime > 0 ? "" : "display-none" %>" style="margin-top: 15px;">
                <div class="header-base-small">
                    <%= Resource.CookieSettingsLifeTime%>:
                </div>
                <div>
                    <input type="text" class="textEdit" maxlength="4" id="lifeTimeTxt" value="<%= LifeTime > 0 ? LifeTime : 1440 %>" style="width: 100px; margin-top: 6px;" />
                </div>
            </div>

            <div class="middle-button-container">
                <a id="saveCookieSettingsBtn" class="button blue" href="javascript:void(0);"><%= Resource.SaveButton %></a>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.CookieSettingsHelp.HtmlEncode(), "<b>", "</b>", "<br/>")%></p>
    </div>
</div>
<% } %>