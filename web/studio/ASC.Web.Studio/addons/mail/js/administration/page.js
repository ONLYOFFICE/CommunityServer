/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


window.administrationPage = (function($) {
    var isInit = false,
        page,
        header,
        mailboxActionButtons = [],
        groupActionButtons = [],
        domainActionButtons = [],
        progressBarIntervalId = null,
        GET_STATUS_TIMEOUT = 10000;

    function init() {
        if (isInit === false) {
            isInit = true;

            page = $('#id_administration_page');
            header = $('#pageActionContainer');

            administrationManager.events.bind('onadddomain', onAddDomain);
            administrationManager.events.bind('onremovedomain', onRemoveMailDomain);
            administrationManager.events.bind('onaddmailbox', onAddMailbox);
            administrationManager.events.bind('onaddgroup', onAddMailGroup);
            administrationManager.events.bind('onremovemailbox', onRemoveMailbox);
            administrationManager.events.bind('onremovemailgroup', onRemoveMailGroup);
            administrationManager.events.bind('onremovemaildomain', onRemoveMailDomain);
            administrationManager.events.bind('ongetfullinformation', onRefreshPage);
            administrationManager.events.bind('onupdatemailbox', onUpdateMailbox);
            administrationManager.events.bind('onupdatemailgroup', onUpdateMailgroup);

            mailboxActionButtons = [
                { selector: "#mailboxActionMenu .editMailbox", handler: editMailbox },
                { selector: "#mailboxActionMenu .deleteMailbox", handler: showDeleteMailboxQuestion },
                { selector: "#mailboxActionMenu .changeMailboxPassword", handler: changeMailboxPassowrd }
            ];

            groupActionButtons = [
                { selector: "#groupActionMenu .editGroup", handler: editMailGroupAddresses },
                { selector: "#groupActionMenu .deleteGroup", handler: showDeleteGroupQuestion }
            ];

            domainActionButtons = [
                { selector: "#domainActionMenu .showDnsSettingsDomain", handler: showDomainDns },
                { selector: "#domainActionMenu .showConnectionSettingsDomain", handler: showConnectionSettings },
                { selector: "#domainActionMenu .deleteDomain", handler: showDeleteDomainQuestion }
            ];

            header.on('click', '#mail-server-add-domain', function () {
                createDomainModal.show(administrationManager.getServerInfo());
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
        $('#administation_data_container .domain').actionMenu('domainActionMenu', domainActionButtons, pretreatmentDomains);
        $('#administation_data_container .mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatmentMailbox);
        $('#administation_data_container .group_menu').actionMenu('groupActionMenu', groupActionButtons);

        controlVisibilityDomainExpander();
        bindCreationLinks();
        makeSortablTables();
        show();
    }

    function pretreatmentDomains() {
        var $showConnectionSettingsDomain = $("#domainActionMenu .showConnectionSettingsDomain");

        if (ASC.Resources.Master.Standalone) {
            $showConnectionSettingsDomain.show();
        } else {
            $showConnectionSettingsDomain.hide();
        }
    }

    function pretreatmentMailbox(id) {
        var domainId = $('#administation_data_container').find('tr[data_id="' + id + '"]').attr('domain_id'),
            isSharedDomain = false,
            domains = administrationManager.getMailDomains();

        for (var i = 0; i < domains.length; i++) {
            if (domains[i].isSharedDomain && domainId == domains[i].id) {
                isSharedDomain = true;
            }
        }
        var $editMailboxAlias = $('#mailboxActionMenu .editMailbox');
        var $changeMailboxPassword = $('#mailboxActionMenu .changeMailboxPassword');

        if (isSharedDomain) {
            $editMailboxAlias.hide();
            $changeMailboxPassword.hide();
        } else {
            if (ASC.Resources.Master.Standalone) {
                $changeMailboxPassword.show();
            } else {
                $changeMailboxPassword.hide();
            }

            $editMailboxAlias.show();
            $editMailboxAlias.removeClass('disable');
            $editMailboxAlias.removeAttr('title');
        }
    }

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

    function showConnectionSettings() {
        var serverInfo = administrationManager.getServerInfo();

        var data =
        {
            server: serverInfo.inServer.hostname,
            port: serverInfo.inServer.port,
            incoming_encryption_type: serverInfo.inServer.socketType === "STARTTLS" ? 2 : 3,
            smtp_server: serverInfo.outServer.hostname,
            smtp_port: serverInfo.outServer.port,
            outcoming_encryption_type: serverInfo.outServer.socketType === "STARTTLS" ? 2 : 3
        };

        window.accountsModal.showConnectionSettings(data, true);
    }

    function showBlankPage() {
        hide();
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
            makeSortablTables();
            controlVisibilityDomainExpander();
        } else {
            blankPages.hide();
            onRefreshPage(e, { domains: [domain], mailgroups: [], mailboxes: [] });
        }
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
        window.LoadingBanner.hideLoading();

        serviceManager.removeMailDomain(domainId,
                { id: domainId },
                {
                    success: function (params, operation) {
                        window.LoadingBanner.displayMailLoading();

                        progressBarIntervalId = setInterval(function () {
                            return checkRemoveDomainStatus(operation, domainId);
                        },
                            GET_STATUS_TIMEOUT);
                    },
                    error: administrationError.getErrorHandler("removeMailDomain")
                },
                ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function checkRemoveDomainStatus(operation, id) {
        serviceManager.getMailOperationStatus(operation.id,
        null,
        {
            success: function (params, data) {
                if (data.completed) {
                    clearInterval(progressBarIntervalId);
                    progressBarIntervalId = null;
                    window.administrationManager.removeDomain(id);
                    window.LoadingBanner.hideLoading();
                }
            },
            error: function (e, error) {
                console.error("checkRemoveDomainStatus", e, error);
                clearInterval(progressBarIntervalId);
                progressBarIntervalId = null;
                administrationError.getErrorHandler("removeMailDomain");
                window.LoadingBanner.hideLoading();
            }
        });
    }

    function onRemoveMailDomain(params, domain) {
        var domainElement = $('.domain_table_container[domain_id="' + domain.id + '"]');
        if (domainElement.length > 0) {
            $(domainElement).remove();
        }
        var domains = administrationManager.getMailDomains();
        if (domains.length == 0) {
            showBlankPage();
        } else {
            controlVisibilityDomainExpander();
        }
        window.toastr.success(window.MailActionCompleteResource.RemoveDomainSuccess.format(domain.name));
    }

    function show() {
        header.html($.tmpl('administrationPageHeaderTmpl'));
        page.show();
    }

    function onAddMailbox(e, mailbox) {
        var html = $.tmpl('mailboxTableRowTmpl', mailbox);
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatmentMailbox);
        var domainContainer = page.find('.domain_table_container[domain_id="' + mailbox.address.domainId + '"]');
        domainContainer.find('.mailboxes_content .mailbox_table').append(html);

        domainContainer.find('.blankContent').hide();
        domainContainer.find('.domain_content').show();
        domainContainer.find('.help_center_column').show();

        refreshFreeMailboxes(mailbox.address.domainId);
        makeSortablTables();

        window.toastr.success(window.MailActionCompleteResource.AddMailboxSuccess.format(mailbox.address.email));
    }

    function onUpdateMailbox(e, params) {
        var html = $.tmpl('mailboxTableRowTmpl', params.mailbox);
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatmentMailbox);
        processAliasesMore(html);
        page.find('.mailbox_table .row[data_id="' + params.mailbox.id + '"]').replaceWith(html);
    }

    function deleteMailbox(id) {
        window.LoadingBanner.hideLoading();

        var mailboxElement = $('.mailbox_table_container tr[data_id=' + id + ']');

        if (mailboxElement.length > 0) {
            serviceManager.removeMailbox(id,
                { id: id },
                {
                    success: function(params, operation) {
                        window.LoadingBanner.displayMailLoading();

                        progressBarIntervalId = setInterval(function() {
                            return checkRemoveMailboxStatus(operation, id);
                            },
                            GET_STATUS_TIMEOUT);
                    },
                    error: administrationError.getErrorHandler("removeMailbox")
                },
                ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function checkRemoveMailboxStatus(operation, id) {
        serviceManager.getMailOperationStatus(operation.id,
        null,
        {
            success: function (params, data) {
                if (data.completed) {
                    clearInterval(progressBarIntervalId);
                    progressBarIntervalId = null;
                    window.administrationManager.removeMailbox(id);
                    window.LoadingBanner.hideLoading();
                }
            },
            error: function (e, error) {
                console.error("checkRemoveMailboxStatus", e, error);
                clearInterval(progressBarIntervalId);
                progressBarIntervalId = null;
                administrationError.getErrorHandler("removeMailbox");
                window.LoadingBanner.hideLoading();
            }
        });
    }

    function onRemoveMailbox(params, mailbox) {
        var mailboxElement = $('.mailbox_table tr[data_id=' + mailbox.id + ']');
        mailboxElement.remove();

        refreshFreeMailboxes(mailbox.address.domainId);
        if (administrationManager.getFreeMailboxesByDomain(mailbox.address.domainId).length == 0 && 
            administrationManager.getMailGroupsByDomain(mailbox.address.domainId).length == 0) {
            var domainContainer = page.find('.domain_table_container[domain_id="' + mailbox.address.domainId + '"]');
            domainContainer.find('.blankContent').show();
            domainContainer.find('.domain_content').hide();
            domainContainer.find('.help_center_column').hide();
        }

        window.toastr.success(window.MailActionCompleteResource.RemoveMailboxSuccess.format(mailbox.address.email));
    }

    function editMailbox(id) {
        window.LoadingBanner.hideLoading();
        editMailboxModal.show(id);
    }

    function changeMailboxPassowrd(id) {
        id = parseInt(id);

        var mailbox = administrationManager.getMailbox(id);
        if (!mailbox) return;

        window.accountsModal.changePassword(mailbox.address.email, id);
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
        makeSortablTables();

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
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatmentMailbox);
        var domainContainer = page.find('.domain_table_container[domain_id="' + group.address.domainId + '"]');
        domainContainer.find('.free_mailboxes').before(html);

        refreshFreeMailboxes(group.address.domainId);
        makeSortablTables();

        window.toastr.success(window.MailActionCompleteResource.AddMailGroupSuccess.format(group.address.email));
    }

    function refreshFreeMailboxes(domainId) {
        var domainContainer = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var mailboxes = administrationManager.getFreeMailboxesByDomain(domainId);

        if (mailboxes.length > 0) {
            domainContainer.find('.free_mailboxes').show();
        } else {
            domainContainer.find('.free_mailboxes').hide();
        }

        var html = $.tmpl('mailboxTableTmpl', { mailboxes: mailboxes });
        $(html).actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatmentMailbox);
        domainContainer.find('.free_mailboxes .mailbox_table_container').replaceWith(html);
    }

    function onUpdateMailgroup(e, params) {
        var html = $.tmpl('groupTableTmpl', params.group);
        $(html).find('.group_menu').actionMenu('groupActionMenu', groupActionButtons);
        $(html).find('.mailbox_table_container').actionMenu('mailboxActionMenu', mailboxActionButtons, pretreatmentMailbox);
        page.find('.group_table_container[group_id="' + params.group.id + '"]').replaceWith(html);

        refreshFreeMailboxes(params.group.address.domainId);
        makeSortablTables();
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
                    'text': mailbox.aliases[i].email,
                    'disabled': true
                });
            }
            $this.actionPanel({ 'buttons': buttons, 'show': true });
            event.preventDefault();
        });
    }

    function manageGroupContent(groupId) {
        var groupEl = page.find('.group_table_container[group_id="' + groupId + '"]');
        var expander = groupEl.find('.group_menu .name_column .expander-icon');
        expander.toggleClass('open');

        if (expander.hasClass('open')) {
            groupEl.find('.group_content').show();
        } else {
            groupEl.find('.group_content').hide();
        }
    }

    function manageDomainContent(domainId) {
        if (administrationManager.getMailDomains().length <= 1)
            return;

        var domainEl = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var expander = domainEl.find('.domain_menu .name_column .expander-icon');

        expander.toggleClass('open');

        var mbs = administrationManager.getMailboxesByDomain(domainId);

        var el = domainEl.find(mbs.length > 0 ? '.domain_content' : '.blankContent');

        if (expander.hasClass('open')) {
            el.show();
        } else {
            el.hide();
        }
    }

    function manageMailboxesContent(domainId) {
        var domainEl = page.find('.domain_table_container[domain_id="' + domainId + '"]');
        var expander = domainEl.find('.mailboxes_group .name_column .expander-icon');
        expander.toggleClass('open');

        if (expander.hasClass('open')) {
            domainEl.find('.mailboxes_content').show();
        } else {
            domainEl.find('.mailboxes_content').hide();
        }
    }

    function controlVisibilityDomainExpander() {
        var domains = administrationManager.getMailDomains();
        if (domains.length == 1) {
            var domainEl = page.find('.domain_table_container');
            if (administrationManager.getFreeMailboxesByDomain(domains[0].id).length != 0 ||
                administrationManager.getMailGroupsByDomain(domains[0].id).length != 0) {
                domainEl.find('.domain_content').show();
            }
            domainEl.find('.domain_menu .name_column .expander-icon').hide();
        }
        else if (domains.length > 1) {
            var expanders = page.find('.domain_table_container .domain_menu .name_column .expander-icon');
            expanders.each(function () {
                $(this).addClass('open');
                $(this).show();
            });
        }
    }

    function makeSortablTables() {
        var tables = $('#administation_data_container .mailbox_table');
        for (var i = 0; i < tables.length; i++) {
            sorttable.makeSortable(tables[i]);
        }
    }

    function hide() {
        header.empty();
        page.hide();
    }

    return {
        init: init,
        hide: hide,
        show: show,
        manageGroupContent: manageGroupContent,
        manageDomainContent: manageDomainContent,
        manageMailboxesContent: manageMailboxesContent,
        bindCreationLinks: bindCreationLinks
    };

})(jQuery);