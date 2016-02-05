<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmsValidationSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmsValidationSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="Resources" %>

<% if (isEnableSmsValidation) { %>
<div class="clearFix <%= SmsEnable ? "" : "disable" %>">
     <div id="studio_smsValidationSettings" class="settings-block">
         <a name="sms-auth"></a>
         <div class="header-base">
             <%= Resource.SmsAuthTitle %>
         </div>

         <div class="sms-validation-settings">
            <asp:PlaceHolder runat="server" ID="SmsBuyHolder"></asp:PlaceHolder>
            <% if (SmsEnable)
               { %>
            <br />
            <br />
             <% } %>
            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthEnable" name="chk2FactorAuth" <%= StudioSmsNotificationSettings.Enable ? "checked=\"checked\"" : "" %>
                    <%= SmsEnable ? "" : "disabled='disabled'" %> />
                <label for="chk2FactorAuthEnable">
                        <%= Resource.EnableUserButton %></label>
            </div>
             <div class="clearFix">
                 <input type="radio" id="chk2FactorAuthDisable" name="chk2FactorAuth" <%= !StudioSmsNotificationSettings.Enable ? "checked=\"checked\"" : "" %> 
                     <%= SmsEnable ? "" : "disabled='disabled'" %> />
                 <label for="chk2FactorAuthDisable">
                     <%= Resource.DisableUserButton %></label>
             </div>
            <div class="middle-button-container">
                <a id="chk2FactorAuthSave" class="button blue <%= SmsEnable ? "" : "disable" %> />" >
                    <%= Resource.SaveButton %></a>
            </div>
        </div>
     </div>
     <div class="settings-help-block">
         <p>
            <%= String.Format(Resource.SmsAuthDescription.HtmlEncode(), "<b>", "</b>", "<br/>", "<br/>", "<b>", "</b>") %>
         </p>
     </div>
</div>
<% } %>