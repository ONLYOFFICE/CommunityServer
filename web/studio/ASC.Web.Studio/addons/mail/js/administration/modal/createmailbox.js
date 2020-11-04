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


window.createMailboxModal = (function($) {
    var $rootEl,
        currentMailboxUser,
        currentDomain;

    function show(domain) {
        currentDomain = domain;

        var html = $.tmpl('createMailboxPopupTmpl', { domain: domain });

        $(html).find('.save').unbind('click').bind('click', addMailbox);

        $(html).find('.cancel').unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            popup.hide();
            return false;
        });

        initUserSelector(html, '#mailboxUserSelector');

        popup.hide();
        popup.addPopup(window.MailAdministrationResource.CreateMailboxHeaderInfo, html, 392, null, null, { focusInput: false });

        $rootEl = $('#mail_server_create_mailbox_popup');

        $rootEl.find('#mail_server_add_mailbox .mailboxName').unbind('textchange').bind('textchange', function() {
            turnOffAllRequiredError();
        });

        PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_create_mailbox_popup:visible .save').trigger('click');";

        setFocusToMailboxInput();
    }

    function addMailbox() {
        if ($(this).hasClass('disable')) {
            return false;
        }

        window.LoadingBanner.hideLoading();

        var isValid = true;

        var localPart = $rootEl.find('#mail_server_add_mailbox .mailboxName').val();
        var name = $rootEl.find('#mailboxSenderName .senderName').val();
        if (localPart.length === 0) {
            TMMail.setRequiredHint('mail_server_add_mailbox', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('mail_server_add_mailbox', true);
            isValid = false;
        } else if (!ASC.Mail.Utility.IsValidEmail(localPart + '@' + currentDomain.name)) {
            TMMail.setRequiredHint("mail_server_add_mailbox", window.MailScriptResource.ErrorIncorrectEmail);
            TMMail.setRequiredError('mail_server_add_mailbox', true);
            isValid = false;
        }

        var mailboxUserId = $rootEl.find('#mailboxUserSelector').attr("data-id");
        if (mailboxUserId == "") {
            TMMail.setRequiredHint('mailboxUserContainer', window.MailScriptResource.ErrorNoUserSelectedField);
            TMMail.setRequiredError('mailboxUserContainer', true);
            isValid = false;
        }

        if (!isValid) {
            setFocusToMailboxInput();
            return false;
        }

        turnOffAllRequiredError();
        displayLoading(true);
        disableButtons(true);

        serviceManager.addMailbox(name,
            localPart,
            currentDomain.id,
            mailboxUserId,
            true,
            true,
            {},
            {
                success: function() {
                    displayLoading(false);
                    disableButtons(false);
                    if ($rootEl.is(':visible')) {
                        $rootEl.find('.cancel').trigger('click');
                    }
                },
                error: function(ev, error) {
                    popup.error(administrationError.getErrorText("addMailbox", error));
                    displayLoading(false);
                    disableButtons(false);
                }
            });

        return false;
    }
    
    function initUserSelector(jqRootElement, userSelectorName) {
        var $mailboxUserSelector = $(jqRootElement).find(userSelectorName);
        $mailboxUserSelector.useradvancedSelector({
                showme: true,
                canadd: false,
                withGuests: false,
                showGroups: false,
                onechosen: true,
                inPopup: true
            })
            .on("showList",
                function(e, item) {
                    var id = item.id, name = item.title;
                    $rootEl.find(userSelectorName).text(name).attr("data-id", id);
                    var senderName = $rootEl.find('.senderName');
                    var sender = senderName.val().trim();
                    if (!sender || sender == currentMailboxUser) {
                        currentMailboxUser = (id == window.Teamlab.profile.id)
                            ? TMMail.htmlDecode(Teamlab.profile.displayName)
                            : TMMail.htmlDecode(name);
                        senderName.val(currentMailboxUser);
                    }
                    setFocusToMailboxInput();
                    turnOffAllRequiredError();
                });
    }

    function turnOffAllRequiredError() {
        TMMail.setRequiredError('mail_server_add_mailbox', false);
        TMMail.setRequiredError('mailboxUserContainer', false);
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
        $rootEl.find('#mailboxUserSelector').toggleClass('disabled', disable);

        TMMail.disableButton($rootEl.find('.cancel'), disable);
        TMMail.disableButton($rootEl.find('.save'), disable);
        TMMail.disableButton($('#commonPopup .cancelButton'), disable);
        popup.disableCancel(disable);
        TMMail.disableInput($rootEl.find('.mailboxName'), disable);
    }

    function setFocusToMailboxInput() {
        $rootEl.find('.mailboxName').focus();
    }

    return {
        show: show
    };

})(jQuery);