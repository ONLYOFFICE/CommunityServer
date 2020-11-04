<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PortalAccessSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PortalAccessSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>
<% if (Enabled)
   { %>
<div class="clearFix">
    <div id="studio_portalAccessSettings" class="settings-block">
        <div class="header-base clearFix" id="portalAccessSettingsTitle">
            <div class="title">
                <%= Resource.PortalAccessSettingsTitle %>
            </div>
        </div>
        <div class="clearFix">
            <div class="clearFix">
                <input id="radioPrivatePortal" type="radio" <%= (!Settings.Anyone ? "checked=\"checked\"" : "") %> name="PortalAccess"/>
                <label for="radioPrivatePortal"><%= Resource.PortalAccessSettingsAuthorized %></label>
            </div>
            <div class="clearFix">
                <input id="radioPublicPortal" type="radio" <%= (Settings.Anyone ? "checked=\"checked\"" : "") %> name="PortalAccess"/>
                <label for="radioPublicPortal"><%= Resource.PortalAccessSettingsAnyone %></label>
                <div id="cbxRegisterUsersContainer" class="clearFix">
                    <input type="checkbox" id="cbxRegisterUsers" <%= (Settings.RegisterUsersImmediately ? "checked=\"checked\"" : "") %>/>
                    <label for="cbxRegisterUsers"><%= CustomNamingPeople.Substitute<Resource>("RegisterNewUsersImmediately").HtmlEncode() %></label>
                </div>
            </div>
        </div>
        <div class="middle-button-container">
            <a class="button blue" onclick="PortalAccess.SaveSettings(this)"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerPortalAccessSettings.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
             <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#PublicPortals" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>
<% } %>
