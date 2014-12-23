<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountsTmpl" type="text/x-jquery-tmpl">
    <table class="accounts_list" id="common_mailboxes">
        <tbody>
            {{tmpl(common_mailboxes) "mailboxItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="server_mailboxes">
        <tbody>
            {{tmpl(server_mailboxes) "mailboxItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="groups">
        <tbody>
            {{tmpl(groups) "groupItemTmpl"}}
        </tbody>
    </table>
</script>

<script id="mailboxItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover {{if enabled!=true }} disabled{{/if}}">
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        {{if isTeamlabMailbox }}
            <td class="aliases_list {{if aliases.length == 1}} one_email {{/if}}">
                {{if aliases.length > 0}}
                    {{tmpl(aliases[0]) "aliasItemTmpl"}}
                    {{tmpl(aliases.length-1) "aliasesMoreTmpl"}}
                {{/if}}
            </td>
        {{/if}}
        {{if !isTeamlabMailbox }}
            <td class="notify_column">
                {{if oAuthConnection }}
                    <span class="notification" title="<%: MailScriptResource.AccountNotificationText %>"><%: MailScriptResource.AccountNotificationText %></span>
                {{/if}}
            </td>
        {{/if}}
        <td class="manage_signature_column">
            <div class="btn-row __list" title="<%: MailScriptResource.ManageSignatureLabel %>" onclick="accountsModal.showSignatureBox('${email}');">
                <%: MailScriptResource.ManageSignatureLabel %>
            </div>
        </td>
        <td class="menu_column"> 
            <div class="menu" title="<%: MailScriptResource.Actions %>" data_id="${email}"></div>
        </td>
    </tr>
</script>

<script id="groupItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover inactive">
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            <span class="notification" title="<%: MailResource.GroupNotificationText %>"><%: MailResource.GroupNotificationText %></span>
        </td>
    </tr>
</script>

<script id="aliasItemTmpl" type="text/x-jquery-tmpl">
  <span class="email" title="${email}">${email}</span>
</script>

<script id="aliasesMoreTmpl" type="text/x-jquery-tmpl">
  {{if $data > 0}}<span class="more-aliases link dotted">${"<%: MailScriptResource.More %>".replace('%1', $data)}</span>{{/if}}
</script>