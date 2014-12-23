/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

window.administrationPage = (function ($) {
    var is_init = false,
        page,
        mailbox_action_buttons = [],
        group_action_buttons = [],
        domain_action_buttons = [];

    function init() {
        if (is_init === false) {
            is_init = true;

            page = $('#id_administration_page');

            administrationManager.events.bind('onadddomain', onAddDomain);
            administrationManager.events.bind('onaddmailbox', onAddMailbox);
            administrationManager.events.bind('onaddgroup', onAddMailGroup);
            administrationManager.events.bind('onremovemailbox', onRemoveMailbox);
            administrationManager.events.bind('onremovemailgroup', onRemoveMailGroup);
            administrationManager.events.bind('onremovemaildomain', onRemoveMailDomain);
            administrationManager.events.bind('ongetfullinformation', onRefreshPage);
            administrationManager.events.bind('onaddalias', onAddAlias);
            administrationManager.events.bind('onremovealias', onRemoveAlias);
            administrationManager.events.bind('onaddgroupaddress', onAddGroupAddress);
            administrationManager.events.bind('onremovegroupaddress', onRemoveGroupAddress);

            mailbox_action_buttons = [
                { selector: "#mailboxActionMenu .editMailbox", handler: editMailboxAliases },
                { selector: "#mailboxActionMenu .deleteMailbox", handler: showDeleteMailboxQuestion }
            ];

            group_action_buttons = [
                { selector: "#groupActionMenu .editGroup", handler: editMailGroupAddresses },
                { selector: "#groupActionMenu .deleteGroup", handler: showDeleteGroupQuestion }
            ];

            domain_action_buttons = [
                { selector: "#domainActionMenu .showDnsSettingsDomain", handler: showDomainDns },
                { selector: "#domainActionMenu .deleteDomain", handler: showDeleteDomainQuestion }
            ];

            page.find('#mail-server-add-domain').unbind('click').bind('click', function () {
                if ($(this).hasClass('disable'))
                    return false;
                createDomainModal.show(administrationManager.getServerInfo());
                return false;
            });
        }
    }
    
    function onRefreshPage(e, data) {
        page.find('.domains_list_position').empty();
        if (data.domains.length == 0) {
            showBlankPage();
            return;
        }

        var item_array = [];
        for (var i = 0; i < data.domains.length; i++) {
            var item = {};
            item.domain = data.domains[i];
            item.mailgroups = administrationManager.getMailGroupsByDomain(data.domains[i].id);
            item.mailboxes = administrationManager.getFreeMailboxesByDomain(data.domains[i].id);
            item_array.push(item);
        }

        var html = $.tmpl('administrationDataTmpl', { items: item_array });
        processAliasesMore(html);
        bindDnsSettingsBtn(html);
        page.find('.domains_list_position').append(html);
        $('#administation_data_container .domain').actionMenu('domainActionMenu', domain_action_buttons);
        $('#administation_data_container .mailbox_table_container').actionMenu('mailboxActionMenu', mailbox_action_buttons);
        $('#administation_data_container .group_menu').actionMenu('groupActionMenu', group_action_buttons);

        show();
        bindCreationLinks();
    }

    function bindCreationLinks() {
        $('.create_new_mailbox').unbind('click').bind('click', function () {
            var domain_id = $(this).closest('.domain_table_container').attr('domain_id');
            var domain = administrationManager.getDomain(domain_id);
            createMailboxModal.show(domain);
        });

        $('.create_new_mailgroup').unbind('click').bind('click', function () {
            var domain_id = $(this).closest('.domain_table_container').attr('domain_id');
            var domain = administrationManager.getDomain(domain_id);
            createMailgroupModal.show(domain);
        });
    }

    function bindDnsSettingsBtn($html) {
        $html.find('#dns_settings_button').unbind('click').bind('click', function () {
            var domain_id = $(this).attr('data_id');
            showDomainDns(domain_id);
        });
    }

    function showDomainDns(domainId) {
        var domain = administrationManager.getDomain(domainId);
        if (domain.dns)
            createDomainModal.showDnsSettings(domainId, { dns: domain.dns });
        else {
            serviceManager.getDomainDnsSettings(domainId, { domainId: domainId }, {
                success: function (params, dns) {
                    createDomainModal.showDnsSettings(params.domainId, { dns: dns });
                    domain.dns = dns;
                },
                error: function(e, error) {
                    administrationError.showErrorToastr("getDomainDnsSettings", error);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function showBlankPage() {
        page.hide();
        blankPages.showNoMailDomains();
    }

    function onAddDomain(e, domain) {
        var html;
        if (administrationManager.getMailDomains().length > 1) {
            html = $.tmpl('domainTableTmpl', { domain: domain, mailgroups: [], mailboxes: [] });
            page.find('#administation_data_container').append(html);
            bindDnsSettingsBtn(html);
            bindCreationLinks();
            $(html).find('.domain').actionMenu('domainActionMenu', domain_action_buttons);
        } else {
            blankPages.hide();
            onRefreshPage(e, { domains: [domain], mailgroups: [], mailboxes: [] });
        }
        window.toastr.success(window.MailActionCompleteResource.AddDomainSuccess.format(domain.name));
    }

    function onAddAlias(e, params) {
        var html = $.tmpl('mailboxTableRowTmpl', params.mailbox);
        $(html).actionMenu('mailboxActionMenu', mailbox_action_buttons);
        page.find('.mailbox_table .row[data_id="' + params.mailbox.id + '"]').replaceWith(html);
        window.toastr.success(window.MailActionCompleteResource.AddAliasSuccess.format(params.alias.email));
        processAliasesMore(html);
    }

    function onRemoveAlias(e, params) {
        var html = $.tmpl('mailboxTableRowTmpl', params.mailbox);
        $(html).actionMenu('mailboxActionMenu', mailbox_action_buttons);
        page.find('.mailbox_table .row[data_id="' + params.mailbox.id + '"]').replaceWith(html);
        window.toastr.success(window.MailActionCompleteResource.RemoveAliasSuccess.format(params.alias.email));
        processAliasesMore(html);
    }


    function showDeleteDomainQuestion(domainId) {
        var domain = administrationManager.getDomain(domainId);

        var question = window.MailAdministrationResource.DeleteShureText.replace(/%1/g, domain.name);
        var body = $.tmpl('questionBoxTmpl', {
            attentionText: window.MailAdministrationResource.DeleteDomainAttention,
            questionText: question
        });

        body.find('.button.remove').unbind('click').bind('click', function () {
            deleteDomain(domainId);
            popup.hide();
        });

        popup.addBig(window.MailAdministrationResource.DeleteDomain, body);
    }

    function showDeleteMailboxQuestion(mailboxId) {
        var mailbox = administrationManager.getMailbox(mailboxId);

        var question = window.MailAdministrationResource.DeleteShureText.replace(/%1/g, mailbox.address.email);
        var body = $.tmpl('questionBoxTmpl', {
            attentionText: window.MailAdministrationResource.DeleteMailboxAttention,
            questionText: question
        });

        body.find('.button.remove').unbind('click').bind('click', function () {
            deleteMailbox(mailboxId);
            popup.hide();
        });

        popup.addBig(window.MailAdministrationResource.DeleteMailboxLabel, body);
    }

    function showDeleteGroupQuestion(groupId) {
        var mail_group = administrationManager.getMailGroup(groupId);

        var question = window.MailAdministrationResource.DeleteShureText.replace(/%1/g, mail_group.address.email);
        var body = $.tmpl('questionBoxTmpl', {
            attentionText: window.MailAdministrationResource.DeleteGroupAttention,
            questionText: question
        });

        body.find('.button.remove').unbind('click').bind('click', function () {
            deleteMailGroup(groupId);
            popup.hide();
        });

        popup.addBig(window.MailAdministrationResource.DeleteGroupLabel, body);
    }

    function deleteDomain(domainId) {
        serviceManager.removeMailDomain(domainId, {},
            { error: administrationError.getErrorHandler("removeMailDomain") },
            ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onRemoveMailDomain(params, domain) {
        var domain_element = $('.domain_table_container[domain_id="' + domain.id + '"]');
        if (domain_element.length > 0)
            $(domain_element).remove();
        var domains = administrationManager.getMailDomains();
        if (domains.length == 0) {
            showBlankPage();
        }
        window.toastr.success(window.MailActionCompleteResource.RemoveDomainSuccess.format(domain.name));
    }

    function show() {
        page.show();
    }

    function onAddMailbox(e, mailbox) {
        var html = $.tmpl('mailboxTableRowTmpl', mailbox);
        $(html).actionMenu('mailboxActionMenu', mailbox_action_buttons);
        var domain_container = page.find('.domain_table_container[domain_id="' + mailbox.address.domainId + '"]');
        domain_container.find('.mailboxes_content .mailbox_table').append(html);

        domain_container.find('.create_new_mailgroup').show();

        if (administrationManager.getMailgroupsByDomain(mailbox.address.domainId).length > 0)
            domain_container.find('.mailboxes_group').show();

        window.toastr.success(window.MailActionCompleteResource.AddMailboxSuccess.format(mailbox.address.email));
    }

    function deleteMailbox(id) {
        window.LoadingBanner.hideLoading();

        var mailbox_element = $('.mailbox_table_container tr[data_id=' + id + ']');

        if (mailbox_element.length > 0) {
            serviceManager.removeMailbox(id, {}, { error: administrationError.getErrorHandler("removeMailbox") },
                ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function onRemoveMailbox(params, mailbox) {
        var mailbox_element = $('.mailbox_table tr[data_id=' + mailbox.id + ']');
        mailbox_element.remove();

        if (administrationManager.getFreeMailboxesByDomain(mailbox.address.domainId).length == 0) {
            var domain_container = page.find('.domain_table_container[domain_id="' + mailbox.address.domainId + '"]');
            domain_container.find('.mailboxes_group').hide();

            if(administrationManager.getMailGroupsByDomain(mailbox.address.domainId).length == 0)
                domain_container.find('.create_new_mailgroup').hide();
        }

        window.toastr.success(window.MailActionCompleteResource.RemoveMailboxSuccess.format(mailbox.address.email));
    }

    function editMailboxAliases(id) {
        window.LoadingBanner.hideLoading();
        editMailboxModal.show(id);
    }

    function deleteMailGroup(id) {
        window.LoadingBanner.hideLoading();

        var group_address_element = $('.group_table_container tr[data_id=' + id + ']');

        if (group_address_element.length > 0) {
            serviceManager.removeMailGroup(id, { }, { error: administrationError.getErrorHandler("removeMailGroup") }, ASC.Resources.Master.Resource.LoadingProcessing);
        }

        return false;
    }

    function onRemoveMailGroup(e, params) {
        var group_element = $('.group_table_container[group_id=' + params.group.id + ']');
        group_element.remove();

        refreshFreeMailboxes(params.group.address.domainId);

        if (params.showToastr)
            window.toastr.success(window.MailActionCompleteResource.RemoveMailGroupSuccess.format(params.group.address.email));
    }

    function editMailGroupAddresses(id) {
        window.LoadingBanner.hideLoading();
        editMailGroupModal.show(id);
        return false;
    }

    function onAddMailGroup(params, group) {
        var html = $.tmpl('groupTableTmpl', group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', group_action_buttons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailbox_action_buttons);
        var domain_container = page.find('.domain_table_container[domain_id="' + group.address.domainId + '"]');
        domain_container.find('.free_mailboxes').before(html);

        refreshFreeMailboxes(group.address.domainId);

        window.toastr.success(window.MailActionCompleteResource.AddMailGroupSuccess.format(group.address.email));
    }

    function refreshFreeMailboxes(domainId) {
        var domain_container = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var mailboxes = administrationManager.getFreeMailboxesByDomain(domainId);
        var groups = administrationManager.getMailGroupsByDomain(domainId);

        if (mailboxes.length > 0 && groups.length > 0) domain_container.find('.mailboxes_group').show();
        else domain_container.find('.mailboxes_group').hide();

        var html = $.tmpl('mailboxTableTmpl', { mailboxes: mailboxes });
        $(html).actionMenu('mailboxActionMenu', mailbox_action_buttons);
        domain_container.find('.free_mailboxes .mailbox_table_container').replaceWith(html);
    }

    function onAddGroupAddress(e, params) {
        var html = $.tmpl('groupTableTmpl', params.group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', group_action_buttons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailbox_action_buttons);
        page.find('.group_table_container[group_id="' + params.group.id + '"]').replaceWith(html);

        refreshFreeMailboxes(params.group.address.domainId);
        window.toastr.success(window.MailActionCompleteResource.AddAddressSuccess.format(params.address.email));
    }
    
    function onRemoveGroupAddress(e, params) {
        var html = $.tmpl('groupTableTmpl', params.group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', group_action_buttons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailbox_action_buttons);
        page.find('.group_table_container[group_id="' + params.group.id + '"]').replaceWith(html);

        refreshFreeMailboxes(params.group.address.domainId);

        if(params.showToastr)
            window.toastr.success(window.MailActionCompleteResource.RemoveAddressSuccess.format(params.address.email));
    }

    function processAliasesMore($html) {
        $html.find('.more_aliases').unbind('.processAliasesMore').bind('click.processAliasesMore', function (event) {
            var $this = $(this);
            $this.unbind('.processAliasesMore');
            var mailbox_id = $this.closest('.row').attr('data_id');
            var mailbox = administrationManager.getMailbox(mailbox_id);
            var buttons = [];
            for (var i = 1; i < mailbox.aliases.length; i++) {
                buttons.push({
                    'text': TMMail.ltgt(mailbox.aliases[i].email),
                    'title': MailAdministrationResource.AliasLabel + ': ' + TMMail.ltgt(mailbox.aliases[i].email),
                    'disabled': true
                });
            }
            $this.actionPanel({ 'buttons': buttons, 'show': true });
            event.preventDefault();
        });
    }

    function showGroupContent(groupId) {
        var group_el = page.find('.group_table_container[group_id="' + groupId + '"]');
        var show_group_link = group_el.find('.group_menu .name_column .show_group');
        show_group_link.toggleClass('open');
        
        if (show_group_link.hasClass('open')) {
            group_el.find('.group_content').show();
            show_group_link.text(window.MailScriptResource.HidePasswordLinkLabel);
        }
        else {
            group_el.find('.group_content').hide();
            show_group_link.text(window.MailScriptResource.ShowPasswordLinkLabel);
        }
    }

    function showMailboxesContent(domainId) {
        var domain_el = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var show_group_link = domain_el.find('.mailboxes_group .name_column .show_group');
        show_group_link.toggleClass('open');
        
        if (show_group_link.hasClass('open')) {
            domain_el.find('.mailboxes_content').show();
            show_group_link.text(window.MailScriptResource.HidePasswordLinkLabel);
        }
        else {
            domain_el.find('.mailboxes_content').hide();
            show_group_link.text(window.MailScriptResource.ShowPasswordLinkLabel);
        }
    }

    function hide() {
        page.hide();
    }

    return {
        init: init,
        hide: hide,
        show: show,
        showGroupContent: showGroupContent,
        showMailboxesContent: showMailboxesContent,
        bindCreationLinks: bindCreationLinks
    };

})(jQuery);