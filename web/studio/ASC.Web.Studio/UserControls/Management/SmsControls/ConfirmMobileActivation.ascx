<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmMobileActivation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmMobileActivation" %>
<%@ Import Namespace="ASC.Web.Core.Sms" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="Resources" %>

<div class="mobilephone-panel">
    <div id="errorMobileActivate" class="errorBox" style="display: none;"></div>

    <% if (Activation)
       { %>
    <div id="mobileNumberPanel">
        <div class="header-base medium bold"><%= UserControlsCommonResource.MobilePhoneCaption %></div>
        <br />
        <div class="text-base">
            <%= string.IsNullOrEmpty(User.MobilePhone)
                    ? string.Empty
                    : (string.Format(UserControlsCommonResource.MobileCurrentNumber, "<b>" + "+" + SmsSender.GetPhoneValueDigits(User.MobilePhone) + "</b>") + "<br/>") %>
            <%= StudioSmsNotificationSettings.Enable
                    ? String.Format(UserControlsCommonResource.MobilePhoneDescription, "<br />")
                    : string.Empty %>
        </div>
        <br />
        <input type="tel" id="primaryPhone" placeholder="<%= Resource.MobileNewNumber %>" pattern="\+?\d{4,63}" maxlength="64"
            value="<%= User.MobilePhone %>" class="" autofocus autocomplete="off" data-country="<%= Country %>" />

        <div class="middle-button-container">
            <a id="sendPhoneButton" class="button blue big"><%= UserControlsCommonResource.MobileChangeButton %></a>
        </div>
    </div>
    <% } %>

    <div id="mobileCodePanel" <%= Activation ? " style='display:none' " : "" %>>
        <div class="text-base">
            <%= String.Format(UserControlsCommonResource.MobileCodeDescription,
                    "<span id=\"phoneNoise\">",
                    SmsSender.BuildPhoneNoise(User.MobilePhone),
                    "</span>",
                    Resource.ActivateSendButton,
                    Resource.ActivateAgainGetCodeButton,
                    "<br />") %>
        </div>
        <br />
        <input type="text" id="phoneAuthcode" name="phoneAuthcode" placeholder="<%= Resource.ActivateCodeLabel %>" pattern="\d{0,<%= SmsKeyStorage.KeyLength %>}" 
            maxlength="<%= SmsKeyStorage.KeyLength %>" value="" class="pwdLoginTextbox" autofocus autocomplete="off" />

        <div class="middle-button-container">
            <a id="sendCodeButton" class="button blue big"><%= Resource.ActivateSendButton %></a>
            <span class="splitter-buttons"></span>
            <a id="getCodeAgainButton" class="disable button gray big"><%= Resource.ActivateAgainGetCodeButton %><span></span></a>
        </div>
    </div>
</div>

<div id="GreetingBlock">
    <div class="help-block-signin">
        <asp:PlaceHolder ID="_communitations" runat="server"></asp:PlaceHolder>
    </div>
</div>
