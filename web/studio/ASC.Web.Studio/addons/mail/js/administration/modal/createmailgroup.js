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


window.createMailgroupModal = (function($) {
    var $rootEl,
        currentDomain,
        needSaveAddress;

    function show(domain) {
        currentDomain = domain;
        needSaveAddress = [];

        var html = $.tmpl('createMailgroupPopupTmpl', { domain: domain });

        $(html).find('.save').unbind('click').bind('click', addMailgroup);

        $(html).find('.cancel').unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            popup.hide();
            return false;
        });

        initMailboxAddressSelector(html, '#mailboxAddressSelector');

        popup.hide();
        popup.addBig(window.MailAdministrationResource.CreateMailgroupHeaderInfo, html);

        $rootEl = $('#mail_server_create_mailgroup_popup');

        $rootEl.find('.mailgroupName').unbind('textchange').bind('textchange', function () {
            turnOffAllRequiredError();
        });

        PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_create_mailgroup_popup:visible .save').trigger('click');";

        setFocusToInput();
        updateMailboxList();
    }

    function addMailgroup() {
        if ($(this).hasClass('disable')) {
            return false;
        }

        window.LoadingBanner.hideLoading();

        var isValid = true;

        var mailgroupName = $rootEl.find('.mailgroupName').val();
        if (mailgroupName.length === 0) {
            TMMail.setRequiredHint('mail_server_add_mailgroup', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('mail_server_add_mailgroup', true);
            isValid = false;
        } else if (!ASC.Mail.Utility.IsValidEmail(mailgroupName + '@' + currentDomain.name)) {
            TMMail.setRequiredHint("mail_server_add_mailgroup", window.MailScriptResource.ErrorIncorrectEmail);
            TMMail.setRequiredError('mail_server_add_mailgroup', true);
            isValid = false;
        }

        var addressIds = $.map(needSaveAddress, function(item) {
            return item.id;
        });

        if (addressIds.length == 0) {
            TMMail.setRequiredHint('mailboxesContainer', window.MailScriptResource.ErrorNoMailboxSelectedField);
            TMMail.setRequiredError('mailboxesContainer', true);
            isValid = false;
        }

        if (!isValid) {
            setFocusToInput();
            return false;
        }

        turnOffAllRequiredError();
        displayLoading(true);
        disableButtons(true);
        serviceManager.addMailGroup(mailgroupName, currentDomain.id, addressIds, {},
            {
                success: function() {
                    displayLoading(false);
                    disableButtons(false);
                    if ($rootEl.is(':visible')) {
                        $rootEl.find('.cancel').trigger('click');
                    }
                },
                error: function(ev, error) {
                    popup.error(administrationError.getErrorText("addMailGroup", error));
                    displayLoading(false);
                    disableButtons(false);
                }
            });

        return false;
    }

    function initMailboxAddressSelector(jqRootElement, userSelectorName) {
        var $mailboxAddressSelector = $(jqRootElement).find(userSelectorName);
        $mailboxAddressSelector.mailboxadvancedSelector({
            getAddresses: function() {
                return getUnselectedMailboxAddresses();
            },
            inPopup: true
        }).on("showList", function(e, items) {
            setFocusToInput();
            turnOffAllRequiredError();
            addAddress(items);
        });
    }

    function addAddress(items) {
        $.merge(needSaveAddress, items);

        for (var i = 0; i < items.length; i++) {
            var mailbox = administrationManager.getMailboxByEmail(items[i].title);
            var html = $.tmpl('addedMailboxTableRowTmpl', mailbox);
            $rootEl.find('.mailbox_table table').append(html);
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
        var mailboxTable = $rootEl.find('.mailbox_table');
        var mailboxRows = mailboxTable.find('.mailbox_row');
        for (var i = 0; i < mailboxRows.length; i++) {
            var deleteButton = $(mailboxRows[i]).find('.delete_entity');
            deleteButton.unbind('click').bind('click', deleteAddress).show();
        }

        mailboxTable.toggleClass('empty_list', mailboxRows.length == 0);
    }

    function deleteAddress() {
        var row = $(this).closest('.mailbox_row');
        var mailboxId = row.attr('mailbox_id');
        var mailbox = administrationManager.getMailbox(mailboxId);
        var pos = searchMailboxIndex(needSaveAddress, mailbox.address.id);
        if (pos > -1) {
            needSaveAddress.splice(pos, 1);
        }
        row.remove();
        updateMailboxList();
    }

    function getUnselectedMailboxAddresses() {
        var domainMailboxes = administrationManager.getMailboxesByDomain(currentDomain.id);
        var mailboxTable = $rootEl.find('.mailbox_table');
        var freeAddresses = $.map(domainMailboxes, function(domainMailbox) {
            return mailboxTable.find('.mailbox_row[mailbox_id="' + domainMailbox.id + '"]').length == 0 ? domainMailbox.address : null;
        });
        return freeAddresses;
    }

    function turnOffAllRequiredError() {
        TMMail.setRequiredError('mail_server_add_mailgroup', false);
        TMMail.setRequiredError('mailboxesContainer', false);
    }

    function displayLoading(isVisible) {
        var loader = $rootEl.find('.progressContainer .loader');
        if (loader) {
            if (isVisible) {
                loader.show();
            } else {
                loader.hide();
            }
        }
    }

    function disableButtons(disable) {
        $rootEl.find('#mailboxAddressSelector').toggleClass('disabled', disable);
        TMMail.disableButton($rootEl.find('.cancel'), disable);
        TMMail.disableButton($rootEl.find('.save'), disable);
        TMMail.disableButton($('#commonPopup .cancelButton'), disable);
        popup.disableCancel(disable);
        TMMail.disableInput($rootEl.find('.mailgroupName'), disable);
    }

    function setFocusToInput() {
        $rootEl.find('.mailgroupName').focus();
    }

    return {
        show: show
    };

})(jQuery);