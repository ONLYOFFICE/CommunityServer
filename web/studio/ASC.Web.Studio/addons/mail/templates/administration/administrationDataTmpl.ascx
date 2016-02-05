<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="administrationDataTmpl" type="text/x-jquery-tmpl">
    <div id="administation_data_container">
        {{tmpl(items) "domainTableTmpl"}}
    </div>
</script>

<script id="domainTableTmpl" type="text/x-jquery-tmpl">
    <div class="domain_table_container" domain_id ="${domain.id}">
        {{tmpl(domain) "domainTableRowTmpl"}}
        <div class="domain_content">
            <div class="add_panel">
                <span class="create_new_mailbox addUserLink">
                    <a class="gray link dotline"><%= MailAdministrationResource.AddMailboxLabel %></a>
                </span>
                {{if !domain.isSharedDomain }}
                    <span class="create_new_mailgroup addUserLink" {{if mailboxes.length == 0 && mailgroups.length == 0}} style="display: none" {{/if}}>
                        <a class="gray link dotline">
                            <%= MailAdministrationResource.AddMailgroupLabel %>
                        </a>
                    </span>
                {{/if}}
            </div>
            {{tmpl(mailgroups) "groupTableTmpl"}}
            <div class="group_table_container free_mailboxes">
                <table class="group_menu mailboxes_group" {{if mailboxes.length == 0 || mailgroups.length == 0}} style="display: none" {{/if}}>
                    <tr class='row'>
                        <td class="name_column">
                            <div class="group_name">
                                <span class="group_icon"></span>
                                <span class="name bold"><%= MailAdministrationResource.NotInGroupLabel %></span>
                                <span class="show_group gray link dotline open"
                                     onclick="javascript:administrationPage.showMailboxesContent('${domain.id}');">
                            <%: MailScriptResource.HidePasswordLinkLabel %>
                                </span>
                            </div>
                        </td>
                    </tr>
                </table>
                <div class="mailboxes_content">
                    {{tmpl({mailboxes: mailboxes}) "mailboxTableTmpl"}}
                </div>
            </div>
        </div>
    </div>
</script>

<script id="domainTableRowTmpl" type="text/x-jquery-tmpl">
    <div class="domain">
        <table class="domain_menu">
            <tr>
                <td class="name_column"> 
                    <span class="name bold">${name}</span>
                    {{if isSharedDomain }}
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'DomainHelperBlock'});" style="margin-bottom: -2px"></span>
                    <div class="popup_helper" id="DomainHelperBlock">
                        <p><%=MailResource.DomainHelperInformationText_1%></p>
                        <p><%=MailResource.DomainHelperInformationText_2%></p>
                        <div class="cornerHelpBlock pos_top"></div>
                    </div>
                    {{/if}}
                </td>
                {{if !isSharedDomain }}
                    <td class="menu_column">
                        <div class="menu menu-small" title="Actions" data_id="${id}"></div>
                    </td>
                    <td class="verify_dns_column">
                        <div class = "verify_dns {{if dns.isVerified == true}} hidden {{/if}}">
                            <span class="text">
                                ${"<%: MailAdministrationResource.UnverifiedDomainRecordExplain %>".replace('%1', name)}
                            </span>
                            <a id="dns_settings_button" class="link dotline" href="#" onclick="return false;" data_id="${id}">
                                <%: MailAdministrationResource.DNSSettingsLabel %>
                            </a>
                        </div>
                    </td>
                {{/if}}
            </tr>
        </table>

    </div>
</script>
