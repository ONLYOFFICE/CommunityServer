<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="editMailboxTmpl" type="text/x-jquery-tmpl">
    <div id="mail_server_edit_mailbox">
        <table>
            <tbody>
                <tr>
                    <td>
                        <div class="block-cnt-splitter header-address">
                            <div class="bold">Mailbox address:</div>
                            <div>"${mailbox.user.displayName}" &lt;${mailbox.address.email}&gt;</div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mailbox_add_alias" class="requiredField">
                            <span class="requiredErrorText"><%=MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall bold"><%= MailAdministrationResource.MailboxAliasLabel %></div>
                            <input type="text" class="alias_name textEdit" maxlength="64" />
                            <span>@${domain.name}</span>
                            <a class="plus plusmail addAlias" title="<%= MailAdministrationResource.AddButton %>" style="float: none;"></a>
                            <div class="mailbox_aliases">
                                <table>
                                    {{tmpl(mailbox.aliases) "mailboxAliasTableRowTmpl"}}
                                </table>
                            </div>
                        </div>

                    </td>
                </tr>
            </tbody>
        </table>
        <div class="buttons">
            <button class="button middle blue save" type="button"><%= MailResource.SaveBtnLabel %></button>
            <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
        </div>
    </div>
</script>

<script id="mailboxAliasTableRowTmpl" type="text/x-jquery-tmpl">
    <tr class='alias_row' alias_id="${id}">
        <td>
            <span class="alias_address">${email}</span>
            <div class="delete_entity"></div>
        </td>
    </tr>
</script>
