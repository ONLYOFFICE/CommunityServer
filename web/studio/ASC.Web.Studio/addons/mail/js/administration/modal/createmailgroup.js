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

window.createMailgroupModal = (function ($) {
    var $root_el,
        current_domain,
        need_save_address;

    function show(domain) {
        current_domain = domain;
        need_save_address = [];

        var html = $.tmpl('createMailgroupPopupTmpl', { domain: domain });

        $(html).find('.save').unbind('click').bind('click', addMailgroup);
        
        $(html).find('.cancel').unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;
            popup.hide();
            return false;
        });

        initMailboxAddressSelector(html, '#mailboxAddressSelector');

        popup.hide();
        popup.addPopup(window.MailAdministrationResource.CreateMailgroupHeaderInfo, html, '392px');

        $root_el = $('#mail_server_create_mailgroup_popup');

        $root_el.find('.mailgroup_name').unbind('textchange').bind('textchange', function () {
            turnOffAllRequiredError();
        });
        
        $(document).unbind('keyup').bind('keyup', function (e) {
            if (e.which == 13) {
                if ($root_el.is(':visible')) {
                    $root_el.find('.save').trigger('click');
                }
            }
        });

        setFocusToInput();
        updateMailboxList();
    }

    function addMailgroup() {
        if ($(this).hasClass('disable'))
            return false;

        window.LoadingBanner.hideLoading();

        var is_valid = true;

        var mailgroup_name = $root_el.find('.mailgroup_name').val();
        if (mailgroup_name.length === 0) {
            TMMail.setRequiredHint('mail_server_add_mailgroup', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('mail_server_add_mailgroup', true);
            is_valid = false;
        }
        else if (!TMMail.reMailServerEmailStrict.test(mailgroup_name + '@' + current_domain.name)) {
            TMMail.setRequiredHint("mail_server_add_mailgroup", window.MailScriptResource.ErrorIncorrectEmail);
            TMMail.setRequiredError('mail_server_add_mailgroup', true);
            is_valid = false;
        }

        var address_ids = $.map(need_save_address, function (item) {
            return item.id;
        });

        if (address_ids.length == 0) {
            TMMail.setRequiredHint('mailboxesContainer', window.MailScriptResource.ErrorNoMailboxSelectedField);
            TMMail.setRequiredError('mailboxesContainer', true);
            is_valid = false;
        }

        if (!is_valid) {
            setFocusToInput();
            return false;
        }

        turnOffAllRequiredError();
        displayLoading(true);
        disableButtons(true);
        serviceManager.addMailGroup(mailgroup_name, current_domain.id, address_ids, {},
            {
                success: function () {
                    displayLoading(false);
                    disableButtons(false);
                    if ($root_el.is(':visible'))
                        $root_el.find('.cancel').trigger('click');
                },
                error: function (ev, error) {
                    administrationError.showErrorToastr("addMailGroup", error);
                    displayLoading(false);
                    disableButtons(false);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);

        return false;
    }

    function initMailboxAddressSelector(jqRootElement, userSelectorName) {
        var $mailbox_address_selector = $(jqRootElement).find(userSelectorName);
        $mailbox_address_selector.mailboxadvancedSelector({
            getAddresses: function () {
                return getUnselectedMailboxAddresses();
            },
            inPopup: true
        }).on("showList", function (e, items) {
            setFocusToInput();
            turnOffAllRequiredError();
            addAddress(items);
        });
    }

    function addAddress(items) {
        $.merge(need_save_address, items);

        for (var i = 0; i < items.length; i++) {
            var mailbox = administrationManager.getMailboxByEmail(items[i].title);
            var html = $.tmpl('addedMailboxTableRowTmpl', mailbox);
            $root_el.find('.mailbox_table table').append(html);
        }

        updateMailboxList();
    }

    function searchMailboxIndex(collection, mailboxId) {
        var pos = -1;
        var i, len = collection.length;
        for (i = 0; i < len; i++) {
            if (collection[i].id == mailboxId) {
                pos = i;
                break;
            }
        }
        return pos;
    }

    function updateMailboxList() {
        var mailbox_table = $root_el.find('.mailbox_table');
        var mailbox_rows = mailbox_table.find('.mailbox_row');
        for (var i = 0; i < mailbox_rows.length; i++) {
            var delete_button = $(mailbox_rows[i]).find('.delete_entity');
            delete_button.unbind('click').bind('click', deleteAddress).show();
        }

        mailbox_table.toggleClass('empty_list', mailbox_rows.length == 0);
    }

    function deleteAddress() {
        var row = $(this).closest('.mailbox_row');
        var mailbox_id = row.attr('mailbox_id');
        var mailbox = administrationManager.getMailbox(mailbox_id);
        var pos = searchMailboxIndex(need_save_address, mailbox.address.id);
        if (pos > -1) {
            need_save_address.splice(pos, 1);
        }
        row.remove();
        updateMailboxList();
    }

    function getUnselectedMailboxAddresses() {
        var domain_mailboxes = administrationManager.getMailboxesByDomain(current_domain.id);
        var mailbox_table = $root_el.find('.mailbox_table');
        var free_addresses = $.map(domain_mailboxes, function (domainMailbox) {
            return mailbox_table.find('.mailbox_row[mailbox_id="' + domainMailbox.id + '"]').length == 0 ? domainMailbox.address : null;
        });
        return free_addresses;
    }

    function turnOffAllRequiredError() {
        TMMail.setRequiredError('mail_server_add_mailgroup', false);
        TMMail.setRequiredError('mailboxesContainer', false);
    }
    
    function displayLoading(isVisible) {
        var loader = $root_el.find('.progressContainer .loader');
        if (loader) {
            if (isVisible)
                loader.show();
            else
                loader.hide();
        }
    }

    function disableButtons(disable) {
        $root_el.find('#mailboxAddressSelector').toggleClass('disabled', disable);
        TMMail.disableButton($root_el.find('.cancel'), disable);
        TMMail.disableButton($root_el.find('.save'), disable);
        TMMail.disableButton($('#commonPopup .cancelButton'), disable);
        popup.disableCancel(disable);
        TMMail.disableInput($root_el.find('.mailgroup_name'), disable);
    }

    function setFocusToInput() {
        $root_el.find('.mailgroup_name').focus();
    }

    return {
        show: show
    };

})(jQuery);