<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="mailboxTableTmpl" type="text/x-jquery-tmpl">
    <div class="mailbox_table_container">
        <table class="mailbox_table">
            <tbody>
                {{tmpl(mailboxes) "mailboxTableRowTmpl"}}
            </tbody>
        </table>
    </div>
</script>


<script id="mailboxTableRowTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${id}" address_id="${address.id}" domain_id="${address.domainId}" class='row'>
        <td class="addresses_column">
            <span class="mailbox_address title="${address.email}">
                ${address.email}
            </span>
        </td>
        <td class="aliases_column">
            <div class="mailbox_aliases">
                {{if aliases.length > 0}}
                <div class="email {{if aliases.length == 1}}one_email{{/if}}" title="<%: MailAdministrationResource.AliasLabel %>: ${aliases[0].email}">
                    ${aliases[0].email}
                </div>
                {{/if}}
                <div class="more_aliases">
                    {{if aliases.length > 1}}
                        {{tmpl(aliases.length-1) "mailboxAliasesMoreTmpl"}}
                    {{/if}}
                </div>
            </div>
        </td>
        <td class="user_column" title="${user.displayName}">
            <span>${user.displayName}</span>
        </td>
        <td class="menu_column">
            <div class="menu" title="<%: MailScriptResource.Actions %>" data_id="${id}"></div>
        </td>
    </tr>
</script>

<script id="mailboxAliasesMoreTmpl" type="text/x-jquery-tmpl">
  {{if $data > 0}}<span class="link dotted">${"<%: MailScriptResource.More %>".replace('%1', $data)}</span>{{/if}}
</script>