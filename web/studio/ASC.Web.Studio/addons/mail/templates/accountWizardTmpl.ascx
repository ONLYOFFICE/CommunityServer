<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>


<script id="accountWizardTmpl" type="text/x-jquery-tmpl">
<div  id="account_simple_container" class="popup popupMailBox">
    <table>
        <tbody>
            <tr>
                <td>
                    <div id="mail_EMailContainer" class="requiredField">
                        <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                        <div class="headerPanelSmall bold"><%: MailResource.EMailLabel %></div>
                        <input id="email" type="email"  value="" class="textEdit" />
                    </div>
                    <div id="mail_PasswordContainer" class="requiredField">
                        <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                        <div class="headerPanelSmall bold">
                            <%: MailResource.Password %>
                            <a class="link dotline password-view off"><%: MailScriptResource.ShowPasswordLinkLabel %></a>
                        </div>
                        <input id="password" type="password" value="" class="textEdit" />
                    </div>
                    <div class="progressContainer">
                        <div class="loader" style="display: none"></div>
                    </div>
                    <div class="buttons new-account">
                        <button id="save" class="button middle blue" type="button"><%: MailScriptResource.AddAccountBtnLabel %></button>
                        <button id="cancel" class="button middle gray cancel" type="button"><%: MailScriptResource.CancelBtnLabel %></button>
                        <a id="advancedLinkButton" class="anchorLinkButton link dotline"><%: MailResource.AdvancedLinkLabel %></a>
                    </div>
                </td>
                <% if (MailPage.IsTurnOnOAuth()) 
                    { %>
                    <td></td>
                    <td>
                        <div>
                            <div class="headerPanelSmall bold">
                                <%: MailResource.OAuthLabel %>
                            </div>
                            <div id="oauth_frame_blocker"></div>
                            <iframe id="ifr" src="<%: MailPage.GetImportOAuthAccessUrl() %>"></iframe>
                        </div>
                    </td>
                <% } %>
            </tr>
        </tbody>
    </table>
</div>
</script>