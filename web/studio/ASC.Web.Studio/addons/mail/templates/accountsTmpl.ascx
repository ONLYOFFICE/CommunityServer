<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountsTmpl" type="text/x-jquery-tmpl">
    <table class="accounts_list" id="common_mailboxes">
        <tbody>
            {{tmpl(common_mailboxes, {showSetDefaultIcon: showSetDefaultIcon, now: new Date()}) "mailboxItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="server_mailboxes">
        <tbody>
            {{tmpl(server_mailboxes, {showSetDefaultIcon: showSetDefaultIcon, now: new Date()}) "mailboxItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="aliases">
        <tbody>
            {{tmpl(aliases, {showSetDefaultIcon: showSetDefaultIcon, now: new Date()}) "aliasItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="groups">
        <tbody>
            {{tmpl(groups, {showSetDefaultIcon: showSetDefaultIcon}) "groupItemTmpl"}}
        </tbody>
    </table>
</script>

<script id="mailboxItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row with-entity-menu item-row row-hover {{if !enabled}}disabled{{/if}}">
        {{if $item.showSetDefaultIcon}}
        <td class="default_account_button_column">
            {{if !isDefault}}
                <div class="set_as_default_account_icon default_account_icon_block"
                    title="<%: MailScriptResource.SetAsDefaultAccountText %>"></div>
            {{else}}
                <div class="default_account_icon default_account_icon_block"
                    title="<%: MailScriptResource.DefaultAccountText %>"></div>
            {{/if}}
        </td>
        {{/if}}
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            {{if !enabled}}
                <span style="margin-left: 16px;display: inline-block;" class="red-text notification" title="<%: MailResource.AccountDisableForReceiving %>">
                    <%: MailResource.AccountDisableForReceiving %>
                    <a class="link dotline red-text" style="border-bottom: 1px dotted;display: inline-block;" onclick="javascript:accountsModal.activateAccount('${email}', true);"><%: MailResource.TurnOnAccountLabel %></a>
                </span>
            {{else}}
                {{if autoreply.turnOn && (!autoreply.turnOnToDate || new Date(autoreply.toDate) > $item.now) && new Date(autoreply.fromDate) <= $item.now}}
                    <span style="margin-left: 16px;display: inline-block;" class="red-text notification" title="<%: MailResource.EnabledAutoreplyNotification %>">
                        <%: MailResource.EnabledAutoreplyNotification %>
                        <a class="link dotline red-text" style="border-bottom: 1px dotted;display: inline-block;" onclick="javascript:accountsPage.turnAutoreply('${email}', false);"><%: MailResource.TurnOffAutoreplyLabel %></a>
                    </span>
                {{else}}
                    {{if !isTeamlabMailbox && oAuthConnection}}
                        <span class="notification" title="<%: MailScriptResource.AccountNotificationText %>"><%: MailScriptResource.AccountNotificationText %></span>
                    {{/if}}
                {{/if}}
            {{/if}}
        </td>
        <td class="manage_signature_column">
            <div class="btn-row __list" title="<%: MailScriptResource.ManageSignatureLabel %>" onclick="accountsModal.showSignatureBox('${email}');">
                <%: MailScriptResource.ManageSignatureLabel %>
            </div>
        </td>
        <td class="menu_column">
            <div class="entity-menu" title="<%: MailScriptResource.Actions %>" data_id="${email}"></div>
        </td>
    </tr>
</script>

<script id="groupItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover inactive {{if !enabled}}disabled{{/if}}">
        {{if $item.showSetDefaultIcon}}
            <td class="default_account_button_column">
                <div class="group_default_account_icon_block" title="<%: MailScriptResource.GroupCouldNotSetAsDefault %>"></div>
            </td>
        {{/if}}
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            <span class="notification" title="<%: MailResource.GroupNotificationText %>"><%: MailResource.GroupNotificationText %></span>
        </td>
    </tr>
</script>

<script id="aliasItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover inactive {{if !enabled}}disabled{{/if}}">
        {{if $item.showSetDefaultIcon}}
            <td class="default_account_button_column">
                {{if !isDefault}}
                    <div class="set_as_default_account_icon default_account_icon_block"
                         title="<%: MailScriptResource.SetAsDefaultAccountText %>"></div>
                {{else}}
                    <div class="default_account_icon default_account_icon_block"
                         title="<%: MailScriptResource.DefaultAccountText %>"></div>
                {{/if}}
            </td>
        {{/if}}
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            {{if !enabled}}
                <span style="margin-left: 16px;display: inline-block;" class="red-text notification" title="<%: MailResource.AccountDisableForReceiving %>">
                    <%: MailResource.AccountDisableForReceiving %>
                    <a class="link dotline red-text" style="border-bottom: 1px dotted;display: inline-block;" onclick="javascript:accountsModal.activateAccount('${email}', true);"><%: MailResource.TurnOnAccountLabel %></a>
                </span>
            {{else}}
                {{if autoreply.turnOn && (!autoreply.turnOnToDate || new Date(autoreply.toDate) > $item.now) && new Date(autoreply.fromDate) <= $item.now}}
                    <span style="margin-left: 16px;display: inline-block;" class="red-text notification" title="<%: MailResource.EnabledAutoreplyNotification %>">
                        <%: MailResource.EnabledAutoreplyNotification %>
                        <a class="link dotline red-text" style="border-bottom: 1px dotted;display: inline-block;" onclick="javascript:accountsPage.turnAutoreply('${email}', false);"><%: MailResource.TurnOffAutoreplyLabel %></a>
                    </span>
                {{else}}
                    <span class="notification" title="${"<%: MailScriptResource.AliasNotificationText %>".replace('%mailbox_address%', $data.realEmail)}">
                        ${"<%: MailScriptResource.AliasNotificationText %>".replace('%mailbox_address%', $data.realEmail)}</span>
                {{/if}}
            {{/if}}
        </td>
    </tr>
</script>

<script id="setDefaultIconItemTmpl" type="text/x-jquery-tmpl">
    <td class="default_account_button_column">
        {{if !isDefault}}
            <div class="set_as_default_account_icon default_account_icon_block"
                 title="<%: MailScriptResource.SetAsDefaultAccountText %>"></div>
        {{else}}
            <div class="default_account_icon default_account_icon_block"
                 title="<%: MailScriptResource.DefaultAccountText %>"></div>
        {{/if}}
    </td>
</script>