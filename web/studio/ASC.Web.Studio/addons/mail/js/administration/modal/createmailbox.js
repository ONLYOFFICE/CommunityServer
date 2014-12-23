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

window.createMailboxModal = (function ($) {
    var $root_el,
        current_domain;

    function show(domain) {
        current_domain = domain;

        var html = $.tmpl('createMailboxPopupTmpl', { domain: domain });

        $(html).find('.save').unbind('click').bind('click', addMailbox);
        
        $(html).find('.cancel').unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;
            popup.hide();
            return false;
        });

        initUserSelector(html, '#mailboxUserSelector');

        popup.hide();
        popup.addPopup(window.MailAdministrationResource.CreateMailboxHeaderInfo, html, '392px');

        $root_el = $('#mail_server_create_mailbox_popup');

        $root_el.find('#mail_server_add_mailbox .mailbox_name').unbind('textchange').bind('textchange', function () {
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
    }

    function addMailbox() {
        if ($(this).hasClass('disable'))
            return false;

        window.LoadingBanner.hideLoading();

        var is_valid = true;

        var mailbox_name = $root_el.find('#mail_server_add_mailbox .mailbox_name').val();
        if (mailbox_name.length === 0) {
            TMMail.setRequiredHint('mail_server_add_mailbox', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('mail_server_add_mailbox', true);
            is_valid = false;
        }
        else if (!TMMail.reMailServerEmailStrict.test(mailbox_name + '@' + current_domain.name)) {
            TMMail.setRequiredHint("mail_server_add_mailbox", window.MailScriptResource.ErrorIncorrectEmail);
            TMMail.setRequiredError('mail_server_add_mailbox', true);
            is_valid = false;
        }

        var mailbox_user_id = $root_el.find('#mailboxUserSelector').attr("data-id");
        if (mailbox_user_id == "") {
            TMMail.setRequiredHint('mailboxUserContainer', window.MailScriptResource.ErrorNoUserSelectedField);
            TMMail.setRequiredError('mailboxUserContainer', true);
            is_valid = false;
        }

        if (!is_valid) {
            setFocusToInput();
            return false;
        }

        turnOffAllRequiredError();
        displayLoading(true);
        disableButtons(true);
        serviceManager.addMailbox(mailbox_name, current_domain.id, mailbox_user_id, {},
            {
                success: function () {
                    displayLoading(false);
                    disableButtons(false);
                    if ($root_el.is(':visible'))
                        $root_el.find('.cancel').trigger('click');
                },
                error: function (ev, error) {
                    administrationError.showErrorToastr("addMailbox", error);
                    displayLoading(false);
                    disableButtons(false);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);

        return false;
    }

    function initUserSelector(jqRootElement, userSelectorName) {
        var $mailbox_user_selector = $(jqRootElement).find(userSelectorName);
        $mailbox_user_selector.useradvancedSelector({
            itemsDisabledIds: [],
            canadd: false,
            withGuests: false,
            showGroups: true,
            onechosen: true,
            inPopup: true
        }).on("showList", function (e, item) {
            var id = item.id, name = item.title;
            $root_el.find(userSelectorName).html(name).attr("data-id", id).removeClass("plus");
            setFocusToInput();
            turnOffAllRequiredError();
        });
    }

    function turnOffAllRequiredError() {
        TMMail.setRequiredError('mail_server_add_mailbox', false);
        TMMail.setRequiredError('mailboxUserContainer', false);
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
        $root_el.find('#mailboxUserSelector').toggleClass('disabled', disable);
        TMMail.disableButton($root_el.find('.cancel'), disable);
        TMMail.disableButton($root_el.find('.save'), disable);
        TMMail.disableButton($('#commonPopup .cancelButton'), disable);
        popup.disableCancel(disable);
        TMMail.disableInput($root_el.find('.mailbox_name'), disable);
    }

    function setFocusToInput() {
        $root_el.find('.mailbox_name').focus();
    }

    return {
        show: show
    };

})(jQuery);