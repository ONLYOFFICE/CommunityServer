<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountsTmpl" type="text/x-jquery-tmpl">
    <table class="accounts_list">
        <tbody>
            {{tmpl(accounts) "accountItemTmpl"}}
        </tbody>
    </table>
</script>

<script id="accountItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${address}" class="row item-row row-hover {{if enabled!=true }}disabled{{/if}}">
        <td class="address">
            <span class="accountname" title="${address}">${address}</span>
        </td>
        <td class="notify_column">
            {{if oAuthConnection }}
                <span class="notification" title="<%: MailScriptResource.AccountNotificationText %>"><%: MailScriptResource.AccountNotificationText %></span>
            {{/if}}
        </td>
        <td class="manage_signature_column">
            <div class="btn-row __list" title="<%: MailScriptResource.ManageSignatureLabel %>" onclick="accountsModal.showSignatureBox('${address}');">
                <%: MailScriptResource.ManageSignatureLabel %>
            </div>
        </td>
        <td class="menu_column">
            <div class="menu" title="<%: MailScriptResource.Actions %>" data_id="${address}"></div>
        </td>
    </tr>
</script>
