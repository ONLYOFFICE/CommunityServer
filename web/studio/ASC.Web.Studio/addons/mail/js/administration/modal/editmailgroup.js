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

window.editMailGroupModal = (function ($) {
    var need_save_addresses,
        need_remove_addresses,
        group;

    function show(idGroup) {
        need_save_addresses = [];
        need_remove_addresses = [];
        group = administrationManager.getMailGroup(idGroup);
        var html = $.tmpl('editMailGroupTmpl', group);
        var $html = $(html);
        $html.find('.delete_entity').unbind('click').bind('click', deleteMailbox);
        $html.find('#add_mailbox').mailboxadvancedSelector({
            inPopup: true,
            getAddresses: function() {
                return getFreeDomainAddress(group.address.domainId);
            }
        }).on('showList', function(e, items) {
            addAddress(items);
        });
        $html.find('.buttons .save').unbind('click').bind('click', saveAddresses);

        popup.addSmall(window.MailAdministrationResource.EditGroupAddressesLabel, html);
        updateMailboxList();
    }

    function getFreeDomainAddress(domainId) {
        var domain_mailboxes = administrationManager.getMailboxesByDomain(domainId);
        var mailbox_table = $('#mail_server_edit_group .mailbox_table');
        var free_addresses = $.map(domain_mailboxes, function (domainMailbox) {
            return mailbox_table.find('.mailbox_row[mailbox_id="' + domainMailbox.id + '"]').length == 0 ? domainMailbox.address : null;
        });
        return free_addresses;
    }

    function deleteMailbox() {
        var row = $(this).closest('.mailbox_row');
        var mailbox_id = row.attr('mailbox_id');
        var mailbox = administrationManager.getMailbox(mailbox_id);
        var pos = searchElementByIndex(need_save_addresses, mailbox.address.id);
        if (pos > -1) {
            need_save_addresses.splice(pos, 1);
        } else {
            pos = searchElementByIndex(group.mailboxes, mailbox_id);
            if (pos > -1) {
                var address = group.mailboxes[pos].address;
                need_remove_addresses.push({ id: address.id, email: address.email });
            }
        }
        row.remove();
        updateMailboxList();
    }

    function searchElementByIndex(collection, mailboxId) {
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
        var mailbox_rows = $('#mail_server_edit_group .mailbox_table').find('.mailbox_row');
        if (mailbox_rows.length == 1) $(mailbox_rows[0]).find('.delete_entity').hide();
        else if (mailbox_rows.length >= 2) {
            for (var i = 0; i < mailbox_rows.length; i++) {
                var delete_button = $(mailbox_rows[i]).find('.delete_entity');
                delete_button.unbind('click').bind('click', deleteMailbox).show();
            }
        }
    }

    function addAddress(items) {
        for (var i = 0; i < items.length; i++) {
            var pos = searchElementByIndex(need_remove_addresses, items[i].id);
            if (pos > -1) {
                need_remove_addresses.splice(pos, 1);
            } else {
                need_save_addresses.push({ id: items[i].id, email: items[i].title });
            }

            var mailbox = administrationManager.getMailboxByEmail(items[i].title);
            var html = $.tmpl('addedMailboxTableRowTmpl', mailbox);
            $('#mail_server_edit_group .mailbox_table table').append(html);
        }
        if (items.length) updateMailboxList();
    }

    function saveAddresses() {
        var i, len = need_save_addresses.length, address_id;
        for (i = 0; i < len; i++) {
            address_id = need_save_addresses[i].id;
            serviceManager.addMailGroupAddress(group.id, address_id, { address: need_save_addresses[i] },
                {
                    error: function (e, error) {
                        administrationError.showErrorToastr("addMailGroupAddress", error);
                    }
                });
        }

        len = need_remove_addresses.length;
        for (i = 0; i < len; i++) {
            address_id = need_remove_addresses[i].id;
            serviceManager.removeMailGroupAddress(group.id, address_id,
                { group: group, address: need_remove_addresses[i] },
                {
                    error: function (e, error) {
                        administrationError.showErrorToastr("removeMailGroupAddress", error);
                    }
                });
        }

        window.PopupKeyUpActionProvider.CloseDialog();
    }


    return {
        show: show
    };

})(jQuery);