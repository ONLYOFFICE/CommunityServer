<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GreetingSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.GreetingSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>

<div class="clearFix">
    <div id="studio_greetingSettings" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%= Resource.GreetingSettingsTitle%>
        </div>
        <div class="clearFix">
            <div class="clearFix">
                <div class="header-base-small greetingContentTitle">
                    <%=Resources.Resource.GreetingTitle%>:
                </div>
                <div>
                    <input type="text" class="textEdit" maxlength="150" id="studio_greetingHeader"
                        value="<%=HttpUtility.HtmlEncode(ASC.Core.CoreContext.TenantManager.GetCurrentTenant().Name)%>" />
                </div>
            </div>


            <div class="middle-button-container">
                <a id="saveGreetSettingsBtn" class="button blue"  href="javascript:void(0);" ><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a id="restoreGreetSettingsBtn" class="button gray" href="javascript:void(0);" ><%= Resource.RestoreDefaultButton %></a>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerGreetingSettings.HtmlEncode(), "<br />","<b>","</b>")%></p>
    </div>
</div>

<asp:PlaceHolder ID="logoSettingsContainer" runat="server"></asp:PlaceHolder>