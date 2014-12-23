<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GreetingSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.GreetingSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="studio_greetingSettings" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%= Resource.GreetingSettingsTitle%>
        </div>
        <div class="clearFix">
            <asp:PlaceHolder ID="content" runat="server"></asp:PlaceHolder>
            <div class="middle-button-container">
                <a id="saveGreetSettingsBtn" class="button blue"  href="javascript:void(0);" ><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a id="restoreGreetSettingsBtn" class="button gray" href="javascript:void(0);" ><%= Resource.RestoreDefaultButton %></a>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerGreetingSettings, "<br />","<b>","</b>")%></p>
        <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
           { %>
        <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>