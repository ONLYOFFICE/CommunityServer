<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="createMailgroupPopupTmpl" type="text/x-jquery-tmpl">
    <div id="mail_server_create_mailgroup_popup">
        <table>
            <tbody>
                <tr>
                    <td>
                        <div class="block-cnt-splitter">
                            <span><%= MailAdministrationResource.CreateMailgroupPopupHeader %></span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mail_server_add_mailgroup" class="block-cnt-splitter requiredField">
                            <span class="requiredErrorText"><%= MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall bold"><%= MailAdministrationResource.MailgroupAddressLabel %></div>
                            <input type="text" class="mailgroup_name textEdit" maxlength="64" />
                            <span>@${domain.name}</span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mailboxesContainer" class="requiredField">
                            <span class="requiredErrorText"><%= MailScriptResource.ErrorNoMailboxSelectedField %></span>
                            <div class="headerPanelSmall bold">
                                <%= MailAdministrationResource.MailgroupMailboxesLabel %>
                            </div>
                            <div>
                                <span id="mailboxAddressSelector" data-id="" class="link dotline plus"><%= MailAdministrationResource.AddMailboxLabel %></span>
                            </div>
                            <div class="mailbox_table">
                                <table></table>
                            </div>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
        <div class="progressContainer" style="margin-top: -1px;">
            <div class="loader" style="display: none;"><%= MailResource.LoadingLabel %></div>
        </div>
        <div class="buttons" style="margin-top: 0;">
            <button class="button middle blue save" type="button"><%= MailResource.SaveBtnLabel %></button>
            <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
        </div>
    </div>
</script>
