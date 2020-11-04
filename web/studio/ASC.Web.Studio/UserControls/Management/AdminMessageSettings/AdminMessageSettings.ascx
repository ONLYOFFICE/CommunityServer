<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminMessageSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AdminMessageSettings" %>
<%@ Import Namespace="Resources" %>
<div class="clearFix">
    <div id="studio_admMessSettings" class="settings-block">
        <div class="header-base clearFix" id="admMessSettingsTitle">
            <div class="title"><%= Resource.AdminMessageSettingsTitle %></div>
        </div>
        <div class="clearFix">
            <div class="clearFix">
                <input id="dont_chk_studio_admMess" type="radio" <%=(!_studioAdmMessNotifSettings.Enable?"checked=\"checked\"":"")%> <%=(Enabled ? "" : "disabled=\"disabled\"")%> name="ShowingAdmMessages" />
                <label for="dont_chk_studio_admMess"><%= Resource.DisableUserButton %></label>
            </div>
            <div class="clearFix">
                <input id="chk_studio_admMess" type="radio" <%=(_studioAdmMessNotifSettings.Enable?"checked=\"checked\"":"")%> <%=(Enabled ? "" : "disabled=\"disabled\"")%> name="ShowingAdmMessages" />
                <label for="chk_studio_admMess"><%= Resource.AdminMessageSettingsEnable %></label>
            </div>
        </div>
        <div class="middle-button-container">
            <a class="button blue <%=(Enabled ? "" : "disable")%>" onclick="AdmMess.SaveSettings(this); return false;" href="javascript:void(0);"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerAdminMessSettings.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#ChangingSecuritySettings_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>
