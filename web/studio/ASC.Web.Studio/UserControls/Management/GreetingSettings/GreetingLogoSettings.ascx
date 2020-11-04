<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GreetingLogoSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.GreetingLogoSettings" %>

<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="studio_greetingLogoSettings" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%= Resource.LogoSettingsTitle %>
        </div>
        <div class="clearFix">
            <div class="greetingContentLogo clearFix">
                <div class="header-base-small greetingContentLogoText">
                    <%= Resource.GreetingLogo %>:
                </div>
                <div >
                    <div class="clearFix">
                        <div class="greetingContentLogoImg">
                            <img id="studio_greetingLogo" class="borderBase" alt="" src="<%= _tenantInfoSettings.GetAbsoluteCompanyLogoPath() %>" />
                        </div>
                        <div class="greetingContentChangeLogo">
                            <input type="hidden" id="studio_greetingLogoPath" value="" />
                            <a id="studio_logoUploader" class="link dotline" href="javascript:void(0);">
                                <%= Resource.ChangeLogoButton %></a>
                        </div>
                    </div>
                </div>
            </div>


            <div class="middle-button-container">
                <a id="saveGreetLogoSettingsBtn" class="button blue"  href="javascript:void(0);" ><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a id="restoreGreetLogoSettingsBtn" class="button gray" href="javascript:void(0);" ><%= Resource.RestoreDefaultButton %></a>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerLogoSettings.HtmlEncode(), "<br />", "<b>", "</b>") %></p>

    </div>
</div>