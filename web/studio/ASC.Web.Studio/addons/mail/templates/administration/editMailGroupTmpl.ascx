<%@ Control Language="C#" AutoEventWireup="true"%>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="editMailGroupTmpl" type="text/x-jquery-tmpl">
    <div id="mail_server_edit_group">
        <table>
            <tbody>
                <tr>
                    <td>
                        <div class="block-cnt-splitter header-address">
                            <div class="bold"><%= MailAdministrationResource.MailgroupAddressLabel %>:</div>
                            <div>${address.email}</div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="mailgroup_add_mailbox">
                            <div class="headerPanelSmall bold">
                                <%= MailAdministrationResource.MailgroupMailboxesLabel %>
                            </div>
                            <div>
                                <span id="add_mailbox" class="link dotline plus"><%= MailAdministrationResource.AddMailboxLabel %></span>
                            </div>
                            <div class="mailbox_table">
                                <table>
                                    {{tmpl(mailboxes) "addedMailboxTableRowTmpl"}}
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

<script id="addedMailboxTableRowTmpl" type="text/x-jquery-tmpl">
    <tr class='mailbox_row' mailbox_id="${id}">
        <td>
            <span class="mailbox_address">${address.email}</span>
            <div class="delete_entity"></div>
        </td>
    </tr>
</script>