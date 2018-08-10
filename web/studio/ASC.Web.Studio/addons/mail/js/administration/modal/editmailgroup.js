/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


window.editMailGroupModal = (function($) {
    var needSaveAddresses,
        needRemoveAddresses,
        group,
        events = $({});

    function show(idGroup) {
        needSaveAddresses = [];
        needRemoveAddresses = [];

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
        $html.find('.buttons .save').unbind('click').bind('click', function() {
            saveAddresses()
                .then(function() {
                    events.trigger('onupdategroup', group);
                    window.LoadingBanner.hideLoading();
                    window.toastr.success(window.MailActionCompleteResource.updateMailGroupSuccess.format(group.address.email));
                },
                    function (ev, error) {
                        events.trigger('onupdategroup', group);
                        window.LoadingBanner.hideLoading();
                        administrationError.showErrorToastr("updateMailgroup", error);
                    }
                );
        });

        popup.addSmall(window.MailAdministrationResource.EditGroupAddressesLabel, html);
        updateMailboxList();
    }

    function getFreeDomainAddress(domainId) {
        var domainMailboxes = administrationManager.getMailboxesByDomain(domainId);
        var mailboxTable = $('#mail_server_edit_group .mailbox_table');
        var freeAddresses = $.map(domainMailboxes, function(domainMailbox) {
            return mailboxTable.find('.mailbox_row[mailbox_id="' + domainMailbox.id + '"]').length == 0 ? domainMailbox.address : null;
        });
        return freeAddresses;
    }

    function deleteMailbox() {
        var row = $(this).closest('.mailbox_row');
        var mailboxId = row.attr('mailbox_id');
        var mailbox = administrationManager.getMailbox(mailboxId);
        var pos = searchElementByIndex(needSaveAddresses, mailbox.address.id);
        if (pos > -1) {
            needSaveAddresses.splice(pos, 1);
        } else {
            pos = searchElementByIndex(group.mailboxes, mailboxId);
            if (pos > -1) {
                var address = group.mailboxes[pos].address;
                needRemoveAddresses.push({ id: address.id, email: address.email });
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
        var mailboxRows = $('#mail_server_edit_group .mailbox_table').find('.mailbox_row');
        if (mailboxRows.length == 1) {
            $(mailboxRows[0]).find('.delete_entity').hide();
        } else if (mailboxRows.length >= 2) {
            for (var i = 0; i < mailboxRows.length; i++) {
                var deleteButton = $(mailboxRows[i]).find('.delete_entity');
                deleteButton.unbind('click').bind('click', deleteMailbox).show();
            }
        }
    }

    function addAddress(items) {
        for (var i = 0; i < items.length; i++) {
            var pos = searchElementByIndex(needRemoveAddresses, items[i].id);
            if (pos > -1) {
                needRemoveAddresses.splice(pos, 1);
            } else {
                needSaveAddresses.push({ id: items[i].id, email: items[i].title });
            }

            var mailbox = administrationManager.getMailboxByEmail(items[i].title);
            var html = $.tmpl('addedMailboxTableRowTmpl', mailbox);
            $('#mail_server_edit_group .mailbox_table table').append(html);
        }
        if (items.length) {
            updateMailboxList();
        }
    }

    function saveAddresses() {
        var dfd = jq.Deferred();
        var functionArray = [];
        
        var i, len = needSaveAddresses.length;
        for (i = 0; i < len; i++) {
            functionArray.push(deferredAddMailbox(group.id, needSaveAddresses[i]));
        }

        len = needRemoveAddresses.length;
        for (i = 0; i < len; i++) {
            functionArray.push(deferredRemoveMailbox(group.id, needRemoveAddresses[i]));
        }

        window.PopupKeyUpActionProvider.CloseDialog();
        window.LoadingBanner.displayMailLoading();

        jq.when.apply(jq, functionArray).done(function () {
            dfd.resolve();
        })
        .fail(dfd.reject);

        return dfd.promise();
    }

    function deferredAddMailbox(groupId, address) {
        var dfd = jq.Deferred();
        serviceManager.addMailGroupAddress(groupId, address.id, { address: address },
                {
                    success: function (params, srvGroup) {
                        group = administrationManager.convertServerGroup(srvGroup);
                        dfd.resolve(params, srvGroup);
                    },
                    error: dfd.reject
                });
        return dfd.promise();
    }

    function deferredRemoveMailbox(groupId, address) {
        var dfd = jq.Deferred();
        serviceManager.removeMailGroupAddress(groupId, address.id, { address: address },
                {
                    success: function (params, addressId) {
                        var mailbox = administrationManager.getMailboxByEmail(params.address.email);
                        var index = group.mailboxes.indexOf(mailbox);
                        if (index > -1) {
                            group.mailboxes.splice(index, 1);
                        }
                        dfd.resolve(params, addressId);
                    },
                    error: dfd.reject
                });
        return dfd.promise();
    }

    return {
        show: show,
        events: events
    };

})(jQuery);