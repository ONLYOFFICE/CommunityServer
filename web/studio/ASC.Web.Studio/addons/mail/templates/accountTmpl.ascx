<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountTmpl" type="text/x-jquery-tmpl">
    <div  id="account_container" class="popup popupMailBox advanced">
        <table>
            <tbody>
                <tr>
                    <td>
                        <div id="mail_EMailContainer" class="requiredField">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall" ><%: MailResource.EMailLabel %></div>
                            <input id="email" type="email" value="${email}" class="textEdit"/>
                        </div>
                    </td>
                    <td>
                        <div id="mail_NameContainer">
                            <div class="headerPanelSmall" ><%: MailResource.AccountLabel %></div>
                            <input id="name" type="text" value="${name}" class="textEdit"/>
                        </div>
                    </td>
                </tr>
                <tr class="header-server">
                    <td>
                        <span class="bold"><%: MailResource.ReceiveMailLabel %>:</span>
                    </td>
                    <td>
                        <span class="bold"><%: MailResource.SendMail %></span>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mail_POPServerContainer" class="requiredField">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div id="receive-server-header" class="headerPanelSmall">{{if imap}}<%: MailResource.ImapServerLabel %>{{else}}<%: MailResource.PopServerLabel %>{{/if}}</div>
                            <div class="receive-server">
                                <select id="server-type" class="form-control mini">
                                    <option value="pop" {{if typeof(imap)==='undefined' || !imap}}selected="selected"{{/if}}>POP</option>
                                    <option value="imap" {{if typeof(imap)!=='undefined' && imap}}selected="selected"{{/if}}>IMAP</option>
                                </select>
                                <input id="server" type="text" value="${server}" class="textEdit"/>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div id="mail_SMTPServerContainer" class="requiredField">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall" ><%: MailResource.SmtpServerLabel %></div>
                            <input id="smtp_server" type="text" value="${smtp_server}" class="textEdit"/>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mail_POPPortContainer" class="requiredField">
                            <div class="headerPanelSmall portTitle"><%: MailResource.Port %></div>
                            <div class="headerPanelSmall authTitle"><%: MailResource.AuthenticationType %></div>
                            <div>
                                <input id="port" type="text" maxlength="5" class="mini textEdit" value="{{if typeof(port)!=='undefined' && port !=''}}${port}{{else}}110{{/if}}" />
                                <select id="auth_type_in_sel" class="authenticationTypeBox form-control">
                                    <option value="1" {{if typeof(auth_type_in)==='undefined' || auth_type_in == 1}}selected="selected"{{/if}}><%: MailResource.SimplePasswordAuthentication %></option>
                                    <option value="4" {{if typeof(auth_type_in)!=='undefined' && auth_type_in == 4}}selected="selected"{{/if}}><%: MailResource.EncryptedPaswordAuthentication %></option>
                                </select>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div id="mail_SMTPPortContainer" class="requiredField">
                            <div class="headerPanelSmall portTitle"><%: MailResource.Port %></div>
                            <div class="headerPanelSmall authTitle"><%: MailResource.AuthenticationType %></div>
                            <div>
                                <input id="smtp_port" type="text" maxlength="5" class="mini textEdit" value="{{if typeof(smtp_port)!=='undefined' && smtp_port !=''}}${smtp_port}{{else}}25{{/if}}" />
                                <select id="auth_type_smtp_sel" class="authenticationTypeBox form-control">
                                    <option value="0" {{if typeof(auth_type_smtp)==='undefined' || auth_type_smtp == 0}}selected="selected"{{/if}}><%: MailResource.NonePasswordAuthentication %></option>
                                    <option value="1" {{if typeof(auth_type_smtp)!=='undefined' && auth_type_smtp == 1}}selected="selected"{{/if}}><%: MailResource.SimplePasswordAuthentication %></option>
                                    <option value="4" {{if typeof(auth_type_smtp)!=='undefined' && auth_type_smtp == 4}}selected="selected"{{/if}}><%: MailResource.EncryptedPaswordAuthentication %></option>
                                </select>
                            </div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mail_POPLoginContainer" class="requiredField">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall" ><%: MailResource.Login %></div>
                            <div>
                                <input id="account" type="text" value="${account}" class="textEdit"/>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div id="mail_SMTPLoginContainer" class="{{if smtp_auth == true}} requiredField {{/if}}">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall" ><%: MailResource.Login %></div>
                            <div>
                                <input id="smtp_account" type="text" value="${smtp_account}" class="textEdit"{{if smtp_auth!=true}} disabled="true"{{/if}}/>
                            </div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mail_POPPasswordContainer" class="requiredField">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall" >
                                <%: MailResource.Password %>
                                <a class="link dotline password-view off"><%: MailScriptResource.ShowPasswordLinkLabel %></a>
                            </div>
                            <div>
                                <input id="password" type="password" value="${password}" class="textEdit"/>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div id="mail_SMTPPasswordContainer" class="{{if smtp_auth == true}} requiredField {{/if}}">
                            <span class="requiredErrorText"><%: MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall" >
                                <%: MailResource.Password %>
                                <a class="link dotline password-view off"><%: MailScriptResource.ShowPasswordLinkLabel %></a>
                            </div>
                            <div>
                                <input id="smtp_password" type="password" value="${smtp_password}" class="textEdit"{{if smtp_auth!=true}} disabled="true"{{/if}}/>
                            </div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div>
                            <div id="use_incomming_ssl_label" class="headerPanelSmall">{{if typeof(imap)==='undefined' || !imap}}<%: MailResource.UsePOP3SSL %>{{else}}<%: MailResource.UseImapSSL %>{{/if}}</div>
                            <select id="incoming_encryption_type" class="encryptionTypeSelect form-control">
                                <option value="0" {{if typeof(incoming_encryption_type)==='undefined' || incoming_encryption_type == 0}}selected="selected"{{/if}}><%: MailResource.NoEcncryptionNeeded %></option>
                                <option value="1" {{if typeof(incoming_encryption_type)!=='undefined' && incoming_encryption_type == 1}}selected="selected"{{/if}}>SSL</option>
                                <option value="2" {{if typeof(incoming_encryption_type)!=='undefined' && incoming_encryption_type == 2}}selected="selected"{{/if}}>STARTTLS</option>
                            </select>
                        </div>
                    </td>
                    <td>
                        <div>
                            <div class="headerPanelSmall"><%: MailResource.UseSmtpSSL %></div>
                            <select id="outcoming_encryption_type" class="encryptionTypeSelect form-control">
                                <option value="0" {{if typeof(outcoming_encryption_type)==='undefined' || outcoming_encryption_type == 0}}selected="selected"{{/if}}><%: MailResource.NoEcncryptionNeeded %></option>
                                <option value="1" {{if typeof(outcoming_encryption_type)!=='undefined' && outcoming_encryption_type == 1}}selected="selected"{{/if}}>SSL</option>
                                <option value="2" {{if typeof(outcoming_encryption_type)!=='undefined' && outcoming_encryption_type == 2}}selected="selected"{{/if}}>STARTTLS</option>
                            </select>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
        <div class="mail-limit">
            <label class="checkbox">
                <input id="mail-limit" type="checkbox" {{if typeof(restrict)!=='undefined' && restrict}}value="1" checked="true"{{else}}value="0"{{/if}}/>
                <span><%: MailResource.MailLimitLabel %></span>
            </label>
        </div>
        <div class="progressContainer" style="padding-top: 7px;">
            <div class="loader" style="display: none;"></div>
        </div>
        <div class="error-popup hidden">
            <span class="text"></span>
        </div>
        <div class="buttons new-account">
            <button id="save" class="button middle blue" type="button"><%: MailResource.SaveBtnLabel %></button>
            <button id="cancel" class="button middle gray cancel" type="button"><%: MailScriptResource.CancelBtnLabel %></button>
            <a id="getDefaultSettings" class="anchorLinkButton link dotline"><%: MailResource.GetDefaultSettingsLinkLabel %></a>
        </div>
    </div>
</script>


<script id="fileSelectorMailAdditionalButton" type="text/x-jquery-tmpl">
    <a id="filesFolderUnlinkButton" class="button gray middle"><%: MailResource.FilesFolderUnlinkButton %></a>
</script>

<script id="accountSuccessTmpl" type="text/x-jquery-tmpl">
    ${errorBodyHeader = '<%: MailScriptResource.ImportAccountHeader %>', ""}
    <div id="account_success_container" class="popup popupMailBox">
        {{tmpl({
            errorBodyHeader     : errorBodyHeader,
            errorBody           : $item.data.errorBody,
            errorBodyFooter     : $item.data.errorBodyFooter
        }) "errorBodyTmpl"}}
        <div class="buttons">
            <button class="button middle blue cancel" type="button"><%: MailScriptResource.OkBtnLabel %></button>
        </div>
    </div>
</script>