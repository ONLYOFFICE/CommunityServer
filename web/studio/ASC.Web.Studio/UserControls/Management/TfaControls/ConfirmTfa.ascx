<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmTfa.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TfaActivation" %>
<%@ Import Namespace="Google.Authenticator" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="Resources" %>

<div class="tfa-panel">
    <div id="errorTfaActivate" class="errorBox" style="display: none;"></div>

    <% if (Activation)
       { %>
    <div id="tfaActivationPanel">
        <div class="header-base medium bold"><%= UserControlsCommonResource.TfaAppCaption %></div>
        <br />
        <div class="text-base">
            <%= string.Format(UserControlsCommonResource.TfaAppDescription, "<b>Google Authenticator</b>", 
            "<a href=\"https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2\">Android</a>",
            "<a href=\"https://itunes.apple.com/app/google-authenticator/id388497605\">iOS</a>",
            "<b>Authenticator</b>", "<a href=\"https://www.microsoft.com/p/authenticator/9wzdncrfj3rj\">Windows Phone</a>") %>
        </div>
        <br />

        <div class="text-base">
            <%= string.Format(UserControlsCommonResource.TfaAppHowTo, "<b>" + SetupCode.ManualEntryKey + "</b>") %>
        </div>
    </div>
    <% }
       else
       { %>
    <div id="tfaAuthPanel">
        <div class="text-base">
            <%= UserControlsCommonResource.TfaAppAuthDescription %>
        </div>
    </div>
    <% } %>

    <br>

    <div id="tfaCodePanel">
        <input type="text" id="tfaAuthcode" name="tfaAuthcode" placeholder="<%= Resource.ActivateCodeLabel %>"
            pattern="<%= Activation ? "\\d{0,6}" : "[a-zA-Z0-9\\-_]{0,6}"  %>"
            maxlength="<%= 6 > SetupInfo.TfaAppBackupCodeLength ? 6 : SetupInfo.TfaAppBackupCodeLength %>" value="" class="pwdLoginTextbox" autofocus autocomplete="off" />

        <div class="middle-button-container">
            <a id="sendCodeButton" class="button blue big"><%= Activation ? UserControlsCommonResource.TfaAppConnctButton : Resource.ActivateSendButton %></a>
        </div>
    </div>
</div>

<div id="GreetingBlock">
    <div class="help-block-signin">
        <% if (Activation)
           { %>
        <div id="qrcode" style="background-image: url('<%= SetupCode.QrCodeSetupImageUrl %>')"></div>
        <% } %>

        <asp:PlaceHolder ID="_communitations" runat="server"></asp:PlaceHolder>
    </div>
</div>
