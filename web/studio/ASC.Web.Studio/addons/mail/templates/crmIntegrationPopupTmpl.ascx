<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="crmLinkPopupTmpl" type="text/x-jquery-tmpl">
    ${crmIntegrationHeaderInfo          = '<%: MailResource.LinkConversationPopupInfoMessage %>', ""}
    ${crmIntegrationHeaderInfoExt       = '<%: MailResource.LinkConversationPopupChooseMessage %>', ""}
    ${crmIntegrationSelectEntityLabel   = '<%: MailResource.LinkConversationPopupLinkLabel %>', ""}
    ${crmIntegrationSaveBtnLabel        = '<%: MailResource.LinkConversationLinkButtonLabel %>', ""}
    ${crmIntegrationUnSaveBtnLabel      = '<%: MailResource.LinkConversationUnlinkAllBtnLabel %>', ""}

    <div id="crm_link_popup_container" class="popup popupMailBox crm_popup">
        {{tmpl({
            crmIntegrationHeaderInfo            : crmIntegrationHeaderInfo,
            crmIntegrationHeaderInfoExt         : crmIntegrationHeaderInfoExt,
            crmIntegrationSelectEntityLabel     : crmIntegrationSelectEntityLabel,
            crmIntegrationSaveBtnLabel          : crmIntegrationSaveBtnLabel,
            crmIntegrationUnSaveBtnLabel        : crmIntegrationUnSaveBtnLabel,
            needLoader                          : true
        }) "crmIntegrationBodyTmpl"}}
    </div>
</script>

<script id="crmUnlinkAllPopupTmpl" type="text/x-jquery-tmpl">
    <div id="crm_unlink_all_popup_message" class="popup popupMailBox crm_popup">
        <span><%= MailResource.UnlinkAllInformationMessage %></span>
        <p><%= MailResource.UnlinkAllConfirmationMessage %></p>
        <div class="buttons">
            <button class="button middle blue unlink" type="button"><%= MailResource.UnlinkAllPopupUnlinkBtnLabel %></button>
            <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
        </div>
    </div>
</script>

<script id="crmLinkConfirmPopupTmpl" type="text/x-jquery-tmpl">
    <div id="crm_link_confirm_popup_message" class="popup popupMailBox crm_popup">
        <span><%= MailResource.LinkConversationPopupInfoMessage %></span>
        <p><%= MailResource.CreateAndLinkToCrmConfirmMessage %></p>
        <div class="buttons">
            <button class="button middle blue createAndLink" type="button"><%= MailResource.CreateCrmContactAndLinkPopupBtnLabel %></button>
            <button class="button middle gray justCreate" type="button"><%= MailResource.CreateCrmWithoutLinkPopupBtnLabel %></button>
        </div>
    </div>
</script>

<script id="crmExportPopupTmpl" type="text/x-jquery-tmpl">
    ${crmIntegrationHeaderInfo          = '<%: MailResource.ExportMessagePopupInfoMessage %>', ""}
    ${crmIntegrationHeaderInfoExt       = '<%: MailResource.ExportMessagePopupChooseMessage %>', ""}
    ${crmIntegrationSelectEntityLabel   = '<%: MailResource.ExportMessagePopupExportLabel %>', ""}
    ${crmIntegrationSaveBtnLabel        = '<%: MailResource.ExportMessagePopupExportBtnLbl %>', ""}

    <div id="crm_export_popup_container" class="popup popupMailBox crm_popup">
        {{tmpl({
            crmIntegrationHeaderInfo            : crmIntegrationHeaderInfo,
            crmIntegrationHeaderInfoExt         : crmIntegrationHeaderInfoExt,
            crmIntegrationSelectEntityLabel     : crmIntegrationSelectEntityLabel,
            crmIntegrationSaveBtnLabel          : crmIntegrationSaveBtnLabel,
            crmIntegrationUnSaveBtnLabel        : undefined,
            needLoader                          : false
        }) "crmIntegrationBodyTmpl"}}
    </div>
</script>

<script id="crmIntegrationBodyTmpl" type="text/x-jquery-tmpl">
    <div>
        <span>${crmIntegrationHeaderInfo}</span>
        <p>${crmIntegrationHeaderInfoExt}</p>
        <div class="bold">${crmIntegrationSelectEntityLabel}</div>
        <div>
            <select id="entity-type" class="entity-type">
                <option value="1" selected="selected"><%= MailResource.LinkConversationOptionContactsLbl %></option>
                <option value="2"><%= MailResource.LinkConversationOptionCasesLbl %></option>
                <option value="3"><%= MailResource.LinkConversationOptionOpportunitiesLbl %></option>
            </select>
        </div>
        <div class="choose_contact bold"><%= MailResource.LinkConversationChooseContactsLabel %></div>
        <div>
            <table cellspacing="0" cellpadding="1" class="search_panel">
                <tbody>
                    <tr>
                        <td width="16px" style="border-right: 0 none;">
                            <img align="absmiddle" src="/skins/default/imagescss/search_16.png" class="crm_search_contact_icon"/>
                            <img align="absmiddle" src="/skins/default/images/loader_16.gif" class="crm_search_loading_icon" style="display: none"/>
                        </td>
                        <td style="border-left: 0 none">
                            <div>
                                <input id="link_search_panel" class="search_input" placeholder="<%= MailResource.LinkConversationSearchPlaceHolderLabel %>"/>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
    <div class="linked_table_parent" style="display: none;">
        <table class="linked_contacts_table" cellspacing="0" cellpadding="0">
        </table>
    </div>
    {{if needLoader == true }}
        <div class="loader">
            <img src="/skins/default/images/loader_32.gif"/>
            <div><%= MailResource.LoadingLabel %></div>
        </div>
    {{/if}}
    <div class="buttons">
        <button class="button middle blue link_btn" type="button">${crmIntegrationSaveBtnLabel}</button>
        {{if crmIntegrationUnSaveBtnLabel != undefined }}
            <button class="button middle gray unlink_all" type="button">${crmIntegrationUnSaveBtnLabel}</button>
        {{/if}}
        <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
    </div>
</script>

<script id="crmContactItemTmpl" type="text/x-jquery-tmpl">
    <tr data-entity_id="${entity_id}" entity_type="${entity_type}" class='linked_entity_row'>
        <td class="linked_entity_row_avatar_column">
        {{if entity_type == 1}}
            <img src="${avatarLink}" class="crm_avatar_img"/>
        {{/if}}
        {{if entity_type == 2}}
            <div class="crm_avatar_img case"/>
        {{/if}}
        {{if entity_type == 3}}
            <div class="crm_avatar_img opportunities"/>
        {{/if}}
        </td>
        <td class="linked_entity_row_title_column">
            <span>
                {{if entity_type == 1}}
                <a href="/products/crm/default.aspx?id=${entity_id}" title="${title}" class="link" target="_blank">${title}</a>
                {{/if}}
                {{if entity_type == 2}}
                <a href="/products/crm/cases.aspx?id=${entity_id}" title="${title}" class="link" target="_blank">${title}</a>
                {{/if}}
                {{if entity_type == 3}}
                <a href="/products/crm/deals.aspx?id=${entity_id}" title="${title}" class="link" target="_blank">${title}</a>
                {{/if}}
            </span>
        </td>
        <td class="linked_entity_row_button_column">
            <div class="unlink_entity"></div>
        </td>
    </tr>
</script>