<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountErrorTmpl" type="text/x-jquery-tmpl">
    ${errorBodyHeader = '<%: MailScriptResource.AccountCreationErrorBodyHeader %>', ""}
    ${errorBody = '<%: MailResource.AccountCreationErrorBody %>', ""}
    <div id="account_error_container" class="popup popupMailBox">
        {{tmpl({
            errorBodyHeader     : errorBodyHeader,
            errorBody           : errorBody,
            errorBodyFooter     : $item.data.errorBodyFooter,
            errorAdvancedInfo   : $item.data.errorAdvancedInfo
        }) "errorBodyTmpl"}}
        <div class="buttons">
            <button class="button middle blue tryagain" id="tryagain" type="button"><%: MailScriptResource.TryAgainButton %></button>
            <button class="button middle gray cancel" type="button"><%: MailScriptResource.CancelBtnLabel %></button>
            <a id="advancedErrorLinkButton" class="anchorLinkButton link dotline"><%: MailResource.AdvancedLinkLabel %></a>
        </div>
    </div>
</script>

<script id="accountExistErrorTmpl" type="text/x-jquery-tmpl">
    ${errorBodyHeader = '<%: MailScriptResource.ErrorAccountAlreadyExists %>', ""}
    ${errorBody = '<%: MailScriptResource.NoActionText %>', ""}
    ${errorFooter = '<%: MailScriptResource.ExistAccountText %>', ""}
    <div class="popup popupMailBox">
        {{tmpl({
            errorBodyHeader     : errorBodyHeader,
            errorBody           : errorBody,
            errorBodyFooter     : errorFooter,
            errorAdvancedInfo   : 'undefined'
        }) "errorBodyTmpl"}}
        <div class="buttons">
            <button class="button gray middle cancel" type="button"><%: MailScriptResource.OkBtnLabel %></button>
        </div>
    </div>
</script>

<script id="messageOpenErrorTmpl" type="text/x-jquery-tmpl">
    ${errorBodyHeader = '<%: MailScriptResource.ErrorOpenMessage %>', ""}
    ${errorBody = '<%= Server.HtmlEncode(MailScriptResource.ErrorOpenMessageHelp)
                                                .Replace("{0}", "<a href=\"" + MailPage.GetMailSupportUri() + "\" target=\"_blank\">")
                                                .Replace("{1}", "</a>")%>', ""}
    <div class="body-error">
        {{tmpl({
                errorBodyHeader : errorBodyHeader,
                errorBody       : errorBody
            }) "errorBodyTmpl"}}

    </div>
</script>

<script id="messageParseErrorTmpl" type="text/x-jquery-tmpl">
    ${errorBodyHeader = '<%: MailScriptResource.ErrorOpenMessage %>', ""}
    ${errorBody = '<%= Server.HtmlEncode(MailScriptResource.ErrorParseMessageHelp)
                                                .Replace("{0}", "<a href=\"" + MailPage.GetMailSupportUri() + "\" target=\"_blank\">")
                                                .Replace("{1}", "</a>")%>', ""}
    <div class="body-error">
        {{tmpl({
                errorBodyHeader : errorBodyHeader,
                errorBody       : errorBody
            }) "errorBodyTmpl"}}
    </div>
</script>

<script id="errorBodyTmpl" type="text/x-jquery-tmpl">
    <table>
        <tbody>
            <tr>
                <td  class="errorImg"></td>
                <td>
                    <div class="errorDescription">
                        {{if typeof(errorBodyHeader)!=='undefined'}}
                            <div class="header">${errorBodyHeader}</div>
                        {{/if}}
                        {{if typeof(errorBody)!=='undefined'}}
                            {{if typeof(errorBodyHeader)!=='undefined'}}
                                <p class="body">{{html errorBody}}</p>
                            {{else}}
                                <span>{{html errorBody}}</span>
                            {{/if}}
                        {{/if}}
                        {{if typeof(errorBodyFooter)!=='undefined'}}
                            <p class="footer">{{html errorBodyFooter}}</p>
                        {{/if}}
                        {{if typeof(errorAdvancedInfo)!=='undefined'}}
                            <div id="mail_advanced_error_container" class="error-popup account_error" style="display:none;">
                                {{html errorAdvancedInfo}}
                            </div>
                        {{/if}}
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</script>