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
        {{tmpl({domain: domain, mailgroups: mailgroups, mailboxes: mailboxes}) "domainTableRowTmpl"}}
        <div class="blankContent" {{if mailboxes.length != 0 || mailgroups.length != 0}} style="display: none" {{/if}}>
            <table>
                <tr>
                    <td class="infoImg">
                        <div class="icon"></div>
                    </td>
                    <td>
                        {{if !domain.isSharedDomain }}
                            <p class="info"><%= MailAdministrationResource.AddMailboxToNewDomainInfo %></p>
                            <p class="info"><%= MailAdministrationResource.NewDomainAttention %></p>
                        {{else}}
                            <p class="info"><%=MailResource.DomainHelperInformationText_1%></p>
                            <p class="info"><%=MailResource.DomainHelperInformationText_2%></p>
                        {{/if}}
                    </td>
                </tr>
            </table>
            <span class="create_new_mailbox addUserLink">
                <a class="gray link dotline"><%= MailAdministrationResource.AddMailboxLabel %></a>
            </span>
        </div>
        <div class="domain_content" {{if mailboxes.length == 0 && mailgroups.length == 0}} style="display: none" {{/if}}>
            <div class="add_panel">
                <span class="create_new_mailbox addUserLink">
                    <a class="gray link dotline"><%= MailAdministrationResource.AddMailboxLabel %></a>
                </span>
                {{if !domain.isSharedDomain }}
                    <span class="create_new_mailgroup addUserLink">
                        <a class="gray link dotline">
                            <%= MailAdministrationResource.AddMailgroupLabel %>
                        </a>
                    </span>
                {{/if}}
            </div>
            {{tmpl(mailgroups) "groupTableTmpl"}}
            <div class="group_table_container free_mailboxes" {{if mailboxes.length == 0}} style="display: none" {{/if}}>
                {{if !domain.isSharedDomain }}
                    <table class="group_menu mailboxes_group">
                        <tr class='row'>
                            <td class="name_column">
                                <div class="group_name" onclick="javascript:administrationPage.manageMailboxesContent('${domain.id}');">
                                    <span class="expander-icon open"></span>
                                    <span class="group_icon"></span>
                                    <span class="name bold"><%= MailAdministrationResource.NotInGroupLabel %></span>
                                </div>
                            </td>
                        </tr>
                    </table>
                {{/if}}
                <div class="mailboxes_content {{if domain.isSharedDomain }} shared {{/if}}">  
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
                <td class="name_column" onclick="javascript:administrationPage.manageDomainContent('${domain.id}');"> 
                    <span class="expander-icon open"></span>
                    <span class="name bold">${domain.name}</span>
                </td>
                {{if domain.isSharedDomain }}
                    <td class="help_center_column" {{if mailboxes.length == 0 && mailgroups.length == 0}} style="display: none" {{/if}}>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'DomainHelperBlock'});"></span>
                        <div class="popup_helper" id="DomainHelperBlock">
                            <p><%=MailResource.DomainHelperInformationText_1%></p>
                            <p><%=MailResource.DomainHelperInformationText_2%></p>
                            <div class="cornerHelpBlock pos_top"></div>
                        </div>
                    </td>
                {{else}}
                    <td class="menu_column with-entity-menu">
                        <div class="entity-menu" title="<%: MailScriptResource.Actions %>" data_id="${domain.id}"></div>
                    </td>
                    <td class="verify_dns_column">
                        <div class = "verify_dns {{if domain.dns.isVerified == true}} hidden {{/if}}">
                            <span class="text">
                                ${"<%: MailAdministrationResource.UnverifiedDomainRecordExplain %>".replace('%1', domain.name)}
                            </span>
                            <a id="dns_settings_button" class="link dotline" href="#" onclick="return false;" data_id="${domain.id}">
                                <%: MailAdministrationResource.DNSSettingsLabel %>
                            </a>
                        </div>
                    </td>
                {{/if}}
            </tr>
        </table>
    </div>
</script>
