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


window.createMailboxModal = (function($) {
    var $rootEl,
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
        popup.addPopup(window.MailAdministrationResource.CreateMailboxHeaderInfo, html, 392);

        $rootEl = $('#mail_server_create_mailbox_popup');

        $rootEl.find('#mail_server_add_mailbox .mailbox_name').unbind('textchange').bind('textchange', function() {
            turnOffAllRequiredError();
        });

        PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_create_mailbox_popup:visible .save').trigger('click');";

        setFocusToInput();
    }

    function addMailbox() {
        if ($(this).hasClass('disable')) {
            return false;
        }

        window.LoadingBanner.hideLoading();

        var isValid = true;

        var mailboxName = $rootEl.find('#mail_server_add_mailbox .mailbox_name').val();
        if (mailboxName.length === 0) {
            TMMail.setRequiredHint('mail_server_add_mailbox', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('mail_server_add_mailbox', true);
            isValid = false;
        } else if (!TMMail.reMailServerEmailStrict.test(mailboxName + '@' + currentDomain.name)) {
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
            setFocusToInput();
            return false;
        }

        turnOffAllRequiredError();
        displayLoading(true);
        disableButtons(true);
        serviceManager.addMailbox(mailboxName, currentDomain.id, mailboxUserId, {},
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
            itemsDisabledIds: [],
            canadd: false,
            withGuests: false,
            showGroups: true,
            onechosen: true,
            inPopup: true
        }).on("showList", function(e, item) {
            var id = item.id, name = item.title;
            $rootEl.find(userSelectorName).html(name).attr("data-id", id).removeClass("plus");
            setFocusToInput();
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
        TMMail.disableInput($rootEl.find('.mailbox_name'), disable);
    }

    function setFocusToInput() {
        $rootEl.find('.mailbox_name').focus();
    }

    return {
        show: show
    };

})(jQuery);