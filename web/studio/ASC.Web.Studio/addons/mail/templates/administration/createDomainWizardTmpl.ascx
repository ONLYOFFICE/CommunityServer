<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="createDomainWizardTmpl" type="text/x-jquery-tmpl">
    <div id="mail_server_create_domain_wizard">
        {{tmpl({require_dns_info: require_dns_info, faq_url: faq_url, domain_name: domain_name}) "domainWizardStep" + step}}
        {{if step == 1 || step == 2 || step == 5}}
        <div class="progressContainer">
            <div class="loader" style="display: none;"><%= MailResource.LoadingLabel %></div>
        </div>
        {{/if}}
        <div class="buttons" style="{{if step == 1 || step == 2 || step == 5}}margin-top: 0;{{else}}margin-top: 15px;{{/if}}">
            <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
            <button class="button middle blue next pull-right" type="button">
                {{if step == 5}}
                    <%= MailAdministrationResource.DomainWizardCompleteBtn %>
                {{else}}
                    <%= MailAdministrationResource.DomainWizardNextBtn %>
                {{/if}}
            </button>
            {{if step != 1}}
            <button class="button middle gray prev pull-right" type="button" style="margin-right: 8px;"><%= MailAdministrationResource.DomainWizardPrevBtn %></button>
            {{/if}}
            <span class="step pull-right"> ${"<%: MailAdministrationResource.DomainWizardStep %>".replace('%1', step).replace('%2', 5)}</span>  
        </div>
    </div>
</script>

<script id="domainWizardStep1" type="text/x-jquery-tmpl">
    <table>
        <tbody>
            <tr>
                <td>
                    <div class="wizard_action">
                        <span><%= MailAdministrationResource.DomainWizardWriteDomainName %></span>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div id="wizard_add_domain" class="requiredField">
                        <span class="requiredErrorText"><%=MailScriptResource.ErrorEmptyField %></span>
                        <div class="headerPanelSmall bold"><%= MailAdministrationResource.WebDomainLabel %></div>
                        <input type="text" class="web_domain textEdit" maxlength="255" placeholder="yourcompany.com" />
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</script>

<script id="domainWizardStep2" type="text/x-jquery-tmpl">
    <table>
        <tbody>
            <tr>
                <td>
                    <div>
                        <span><%= MailAdministrationResource.DomainWizardProveDomainOwnershipInfo %></span>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <p><%= MailAdministrationResource.DomainWizardCopyInfo %></p>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardOwnershpCheckRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">TXT</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardName %> <span class="bold">${require_dns_info.dns.domainCheckRecord.name}</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class="bold">"${require_dns_info.dns.domainCheckRecord.value}"</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <span><%= MailAdministrationResource.DomainWizardGoNextStep %><%= MailAdministrationResource.DomainWizardToFAQ.Replace("{0}", "<a target=\"blank\" class=\"linkDescribe\" href=\"${faq_url}\">").Replace("{1}", "</a>") %></span>
                </td>
            </tr>
            <tr>
                <td>
                    <p>${"<%: MailAdministrationResource.DomainWizardDomainNameSymbolInfo %>".replace('%1', domain_name)}</p>
                </td>
            </tr>
            <tr>
                <td>
                    <span><%= MailAdministrationResource.DomainWizardWaitDNSrefrashInfo %></span>
                </td>
            </tr>
        </tbody>
    </table>
</script>

<script id="domainWizardStep3" type="text/x-jquery-tmpl">
    <table>
        <tbody>
            <tr>
                <td>
                    <div  class="wizard_action">
                        <span><%= MailAdministrationResource.DomainWizardAddRecord.Replace("%1", "MX") %></span>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardMXRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">MX</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class="bold">${require_dns_info.dns.mxRecord.host}.</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardPriority %> <span class="bold">${require_dns_info.dns.mxRecord.priority}</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</script>

<script id="domainWizardStep4" type="text/x-jquery-tmpl">
    <table>
        <tbody>
            <tr>
                <td>
                    <div  class="wizard_action">
                        <span><%= MailAdministrationResource.DomainWizardAddRecord.Replace("%1", "SPF") %></span>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardSPFRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">TXT</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardName %> <span class="bold">${require_dns_info.dns.spfRecord.name}</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class="bold">"${require_dns_info.dns.spfRecord.value}"</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</script>

<script id="domainWizardStep5" type="text/x-jquery-tmpl">
    <table>
        <tbody>
            <tr>
                <td>
                    <div  class="wizard_action">
                        <span><%= MailAdministrationResource.DomainWizardAddRecord.Replace("%1", "DKIM") %></span>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardDKIMRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">TXT</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardName %> <span class="bold">${require_dns_info.dns.dkimRecord.selector}._domainkey</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class="bold dkim_record">"${require_dns_info.dns.dkimRecord.publicKey}"</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</script>

<script id="domainAddedTmpl" type="text/x-jquery-tmpl">
    <div id="domain_added_container" class="popup popupMailBox">
        {{tmpl({
            errorBodyHeader     : $item.data.errorBodyHeader,
            errorBody           : $item.data.errorBody,
            errorBodyFooter     : $item.data.errorBodyFooter
        }) "errorBodyTmpl"}}
        <div class="buttons">
            <button class="button middle blue cancel" type="button"><%: MailScriptResource.OkBtnLabel %></button>
        </div>
    </div>
</script>

<script id="domainDnsSettingsTmpl" type="text/x-jquery-tmpl">
    <div id="mail_server_domain_dns_settings" class="popup">
        <div class ="unverified_warning {{if require_dns_info.dns.isVerified == true }}hidden{{/if}}">
            <span><%= MailAdministrationResource.UnverifiedDomainRecordWarning %></span>
        </div>
        <table>
            <tr>
                <td class ="verified_icon">
                    <div class="icon {{if require_dns_info.dns.mxRecord.isVerified == true }}verified{{/if}}"></div>
                </td>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardMXRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">MX</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class="bold">${require_dns_info.dns.mxRecord.host}.</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardPriority %> <span class="bold">${require_dns_info.dns.mxRecord.priority}</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
            <tr>
                <td class ="verified_icon">
                    <div class="icon {{if require_dns_info.dns.spfRecord.isVerified == true }}verified{{/if}}"></div>
                </td>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardSPFRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">TXT</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardName %> <span class="bold">${require_dns_info.dns.spfRecord.name}</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class="bold">"${require_dns_info.dns.spfRecord.value}"</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
            <tr>
                <td class ="verified_icon">
                    <div class="icon {{if require_dns_info.dns.dkimRecord.isVerified == true }}verified{{/if}}"></div>
                </td>
                <td>
                    <div class="dns_record">
                        <span class="dns_record_info"><%= MailAdministrationResource.DomainWizardDKIMRecord %></span>
                        <ul>
                            <li><span><%= MailAdministrationResource.DomainWizardType %> <span class="bold">TXT</span></span></li>
                            <li><span><%= MailAdministrationResource.DomainWizardName %> <span class="bold">${require_dns_info.dns.dkimRecord.selector}._domainkey</span></span></li>
                            <li><span class="dkim"><%= MailAdministrationResource.DomainWizardTextOrValue %> <span class=" dkim_record bold">"${require_dns_info.dns.dkimRecord.publicKey}"</span></span></li>
                        </ul>
                    </div>
                </td>
            </tr>
        </table>
        <div class="progressContainer">
            <div class="loader" style="display: none;"><%= MailResource.LoadingLabel %></div>
        </div>
        <div class="buttons">
            <button class="button middle blue cancel" type="button"><%: MailScriptResource.OkBtnLabel %></button>
            <button class="button middle gray verify {{if require_dns_info.dns.isVerified == true }}hidden{{/if}}" type="button"><%:MailAdministrationResource.VerifyButton %></button>
        </div>
    </div>
</script>
