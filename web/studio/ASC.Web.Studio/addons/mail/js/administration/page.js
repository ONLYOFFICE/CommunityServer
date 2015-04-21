/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


window.administrationPage = (function($) {
    var isInit = false,
        page,
        mailboxActionButtons = [],
        groupActionButtons = [],
        domainActionButtons = [];

    function init() {
        if (isInit === false) {
            isInit = true;

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

            mailboxActionButtons = [
                { selector: "#mailboxActionMenu .editMailbox", handler: editMailboxAliases },
                { selector: "#mailboxActionMenu .deleteMailbox", handler: showDeleteMailboxQuestion }
            ];

            groupActionButtons = [
                { selector: "#groupActionMenu .editGroup", handler: editMailGroupAddresses },
                { selector: "#groupActionMenu .deleteGroup", handler: showDeleteGroupQuestion }
            ];

            domainActionButtons = [
                { selector: "#domainActionMenu .showDnsSettingsDomain", handler: showDomainDns },
                { selector: "#domainActionMenu .deleteDomain", handler: showDeleteDomainQuestion }
            ];

            page.find('#mail-server-add-domain').unbind('click').bind('click', function() {
                if ($(this).hasClass('disable')) {
                    return false;
                }
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

        var itemArray = [];
        for (var i = 0; i < data.domains.length; i++) {
            var item = {};
            item.domain = data.domains[i];
            item.mailgroups = administrationManager.getMailGroupsByDomain(data.domains[i].id);
            item.mailboxes = administrationManager.getFreeMailboxesByDomain(data.domains[i].id);
            itemArray.push(item);
        }

        var html = $.tmpl('administrationDataTmpl', { items: itemArray });
        processAliasesMore(html);
        bindDnsSettingsBtn(html);
        page.find('.domains_list_position').append(html);
        $('#administation_data_container .domain').actionMenu('domainActionMenu', domainActionButtons);
        $('#administation_data_container .mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        $('#administation_data_container .group_menu').actionMenu('groupActionMenu', groupActionButtons);

        show();
        bindCreationLinks();
    }

    var pretreatment = function(id) {
        var domainId = $('#administation_data_container').find('tr[data_id="' + id + '"]').attr('domain_id'),
            isSharedDomain = false,
            domains = administrationManager.getMailDomains();

        for (var i = 0; i < domains.length; i++) {
            if (domains[i].isSharedDomain && domainId == domains[i].id) {
                isSharedDomain = true;
            }
        }
        var $editMailboxAlias = $('#mailboxActionMenu .editMailbox');
        if (isSharedDomain) {
            $editMailboxAlias.hide();
        } else {
            $editMailboxAlias.show();
            $editMailboxAlias.removeClass('disable');
            $editMailboxAlias.removeAttr('title');
        }
    };

    function bindCreationLinks() {
        $('.create_new_mailbox').unbind('click').bind('click', function() {
            var domainId = $(this).closest('.domain_table_container').attr('domain_id');
            var domain = administrationManager.getDomain(domainId);
            createMailboxModal.show(domain);
        });

        $('.create_new_mailgroup').unbind('click').bind('click', function() {
            var domainId = $(this).closest('.domain_table_container').attr('domain_id');
            var domain = administrationManager.getDomain(domainId);
            createMailgroupModal.show(domain);
        });
    }

    function bindDnsSettingsBtn($html) {
        $html.find('#dns_settings_button').unbind('click').bind('click', function() {
            var domainId = $(this).attr('data_id');
            showDomainDns(domainId);
        });
    }

    function showDomainDns(domainId) {
        var domain = administrationManager.getDomain(domainId);
        if (domain.dns) {
            createDomainModal.showDnsSettings(domainId, { dns: domain.dns });
        } else {
            serviceManager.getDomainDnsSettings(domainId, { domainId: domainId }, {
                success: function(params, dns) {
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
            $(html).find('.domain').actionMenu('domainActionMenu', domainActionButtons);
        } else {
            blankPages.hide();
            onRefreshPage(e, { domains: [domain], mailgroups: [], mailboxes: [] });
        }
    }

    function onAddAlias(e, params) {
        var html = $.tmpl('mailboxTableRowTmpl', params.mailbox);
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        page.find('.mailbox_table .row[data_id="' + params.mailbox.id + '"]').replaceWith(html);
        window.toastr.success(window.MailActionCompleteResource.AddAliasSuccess.format(params.alias.email));
        processAliasesMore(html);
    }

    function onRemoveAlias(e, params) {
        var html = $.tmpl('mailboxTableRowTmpl', params.mailbox);
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
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

        body.find('.button.remove').unbind('click').bind('click', function() {
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

        body.find('.button.remove').unbind('click').bind('click', function() {
            deleteMailbox(mailboxId);
            popup.hide();
        });

        popup.addBig(window.MailAdministrationResource.DeleteMailboxLabel, body);
    }

    function showDeleteGroupQuestion(groupId) {
        var mailGroup = administrationManager.getMailGroup(groupId);

        var question = window.MailAdministrationResource.DeleteShureText.replace(/%1/g, mailGroup.address.email);
        var body = $.tmpl('questionBoxTmpl', {
            attentionText: window.MailAdministrationResource.DeleteGroupAttention,
            questionText: question
        });

        body.find('.button.remove').unbind('click').bind('click', function() {
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
        var domainElement = $('.domain_table_container[domain_id="' + domain.id + '"]');
        if (domainElement.length > 0) {
            $(domainElement).remove();
        }
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
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        var domainContainer = page.find('.domain_table_container[domain_id="' + mailbox.address.domainId + '"]');
        domainContainer.find('.mailboxes_content .mailbox_table').append(html);

        domainContainer.find('.create_new_mailgroup').show();

        if (administrationManager.getMailgroupsByDomain(mailbox.address.domainId).length > 0) {
            domainContainer.find('.mailboxes_group').show();
        }

        window.toastr.success(window.MailActionCompleteResource.AddMailboxSuccess.format(mailbox.address.email));
    }

    function deleteMailbox(id) {
        window.LoadingBanner.hideLoading();

        var mailboxElement = $('.mailbox_table_container tr[data_id=' + id + ']');

        if (mailboxElement.length > 0) {
            serviceManager.removeMailbox(id, {}, { error: administrationError.getErrorHandler("removeMailbox") },
                ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function onRemoveMailbox(params, mailbox) {
        var mailboxElement = $('.mailbox_table tr[data_id=' + mailbox.id + ']');
        mailboxElement.remove();

        if (administrationManager.getFreeMailboxesByDomain(mailbox.address.domainId).length == 0) {
            var domainContainer = page.find('.domain_table_container[domain_id="' + mailbox.address.domainId + '"]');
            domainContainer.find('.mailboxes_group').hide();

            if (administrationManager.getMailGroupsByDomain(mailbox.address.domainId).length == 0) {
                domainContainer.find('.create_new_mailgroup').hide();
            }
        }

        window.toastr.success(window.MailActionCompleteResource.RemoveMailboxSuccess.format(mailbox.address.email));
    }

    function editMailboxAliases(id) {
        window.LoadingBanner.hideLoading();
        editMailboxModal.show(id);
    }

    function deleteMailGroup(id) {
        window.LoadingBanner.hideLoading();

        var groupAddressElement = $('.group_table_container tr[data_id=' + id + ']');

        if (groupAddressElement.length > 0) {
            serviceManager.removeMailGroup(id, {}, { error: administrationError.getErrorHandler("removeMailGroup") }, ASC.Resources.Master.Resource.LoadingProcessing);
        }

        return false;
    }

    function onRemoveMailGroup(e, params) {
        var groupElement = $('.group_table_container[group_id=' + params.group.id + ']');
        groupElement.remove();

        refreshFreeMailboxes(params.group.address.domainId);

        if (params.showToastr) {
            window.toastr.success(window.MailActionCompleteResource.RemoveMailGroupSuccess.format(params.group.address.email));
        }
    }

    function editMailGroupAddresses(id) {
        window.LoadingBanner.hideLoading();
        editMailGroupModal.show(id);
        return false;
    }

    function onAddMailGroup(params, group) {
        var html = $.tmpl('groupTableTmpl', group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', groupActionButtons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        var domainContainer = page.find('.domain_table_container[domain_id="' + group.address.domainId + '"]');
        domainContainer.find('.free_mailboxes').before(html);

        refreshFreeMailboxes(group.address.domainId);

        window.toastr.success(window.MailActionCompleteResource.AddMailGroupSuccess.format(group.address.email));
    }

    function refreshFreeMailboxes(domainId) {
        var domainContainer = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var mailboxes = administrationManager.getFreeMailboxesByDomain(domainId);
        var groups = administrationManager.getMailGroupsByDomain(domainId);

        if (mailboxes.length > 0 && groups.length > 0) {
            domainContainer.find('.mailboxes_group').show();
        } else {
            domainContainer.find('.mailboxes_group').hide();
        }

        var html = $.tmpl('mailboxTableTmpl', { mailboxes: mailboxes });
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        domainContainer.find('.free_mailboxes .mailbox_table_container').replaceWith(html);
    }

    function onAddGroupAddress(e, params) {
        var html = $.tmpl('groupTableTmpl', params.group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', groupActionButtons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        page.find('.group_table_container[group_id="' + params.group.id + '"]').replaceWith(html);

        refreshFreeMailboxes(params.group.address.domainId);
        window.toastr.success(window.MailActionCompleteResource.AddAddressSuccess.format(params.address.email));
    }

    function onRemoveGroupAddress(e, params) {
        var html = $.tmpl('groupTableTmpl', params.group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', groupActionButtons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatment);
        page.find('.group_table_container[group_id="' + params.group.id + '"]').replaceWith(html);

        refreshFreeMailboxes(params.group.address.domainId);

        if (params.showToastr) {
            window.toastr.success(window.MailActionCompleteResource.RemoveAddressSuccess.format(params.address.email));
        }
    }

    function processAliasesMore($html) {
        $html.find('.more_aliases').unbind('.processAliasesMore').bind('click.processAliasesMore', function(event) {
            var $this = $(this);
            $this.unbind('.processAliasesMore');
            var mailboxId = $this.closest('.row').attr('data_id');
            var mailbox = administrationManager.getMailbox(mailboxId);
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
        var groupEl = page.find('.group_table_container[group_id="' + groupId + '"]');
        var showGroupLink = groupEl.find('.group_menu .name_column .show_group');
        showGroupLink.toggleClass('open');

        if (showGroupLink.hasClass('open')) {
            groupEl.find('.group_content').show();
            showGroupLink.text(window.MailScriptResource.HidePasswordLinkLabel);
        } else {
            groupEl.find('.group_content').hide();
            showGroupLink.text(window.MailScriptResource.ShowPasswordLinkLabel);
        }
    }

    function showMailboxesContent(domainId) {
        var domainEl = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var showGroupLink = domainEl.find('.mailboxes_group .name_column .show_group');
        showGroupLink.toggleClass('open');

        if (showGroupLink.hasClass('open')) {
            domainEl.find('.mailboxes_content').show();
            showGroupLink.text(window.MailScriptResource.HidePasswordLinkLabel);
        } else {
            domainEl.find('.mailboxes_content').hide();
            showGroupLink.text(window.MailScriptResource.ShowPasswordLinkLabel);
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