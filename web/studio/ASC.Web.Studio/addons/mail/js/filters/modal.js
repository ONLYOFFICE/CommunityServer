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


window.filterModal = (function($) {
    var isInit = false,
        wnd;

    function init() {
        if (isInit === true) return;

        isInit = true;

        wnd = $('#filterWnd');
    }

    function blockUi(width, message) {
        var defaultOptions = {
            css: {
                top: '-100%'
            },
            bindEvents: false
        };

        window.StudioBlockUIManager.blockUI(message, width, null, null, null, defaultOptions);

        message.closest(".blockUI").css({
            'top': '50%',
            'margin-top': '-{0}px'.format(message.height() / 2)
        });

        $('#manageWindow .cancelButton').css('cursor', 'pointer');
        $('.containerBodyBlock .buttons .cancel').unbind('click').bind('click', function () {
            $.unblockUI();
            return false;
        });
    }

    function show(filter, type) {
        wnd.find('.buttons .button').hide();
        wnd.find('.mail-confirmationAction').hide();
        wnd.find('.buttons .cancel').show();

        var headerText;

        switch (type) {
        case 'apply':
            wnd.find('.save').show();

            var actionsContainer = wnd.find('.mail-confirmationAction p.actions');

            actionsContainer.empty();

            var i, n = filter.actions.length;
            for (i = 0; i < n; i++) {
                var a = filter.actions[i],
                    actionEl = $('<span class="actionText"></span>');

                var actionText = "&mdash; " + filtersPage.translateAction(a.action);
                if (a.data && a.data !== '') {
                    actionText += "&nbsp;<span class=\"actionValue\">\"{0}\"</span>".format(TMMail
                        .htmlEncode(filtersPage.translateActionData(a.action, a.data)));
                }
                actionEl.html(actionText);

                actionsContainer.append(actionEl).append("<br>");
            }

            headerText = wnd.attr('apply_header');
            break;
        case 'delete':
            wnd.find('.del').show();
            headerText = wnd.attr('delete_header');
            break;
        case 'confirm':
            wnd.find('.confirm').show();
            wnd.find('.change').show();
            headerText = wnd.attr('confirm_header');
            break;
        default:
            console.error("Modal window type is not accepted");
            return;
        }

        wnd.find('div.containerHeaderBlock:first td:first').html(headerText);

        blockUi(523, wnd);

        window.PopupKeyUpActionProvider.EnterAction = "jq('#filterWnd .containerBodyBlock .buttons .button.blue:visible').click();";

        wnd.find('.buttons .cancel')
            .unbind('click')
            .bind('click',
                function () {
                    hide();
                    return false;
                });
    }

    function showApply(filter, options) {
        if (!filter) return;

        init();

        show(filter, 'apply');

        wnd.find('.buttons .save')
            .unbind('click')
            .bind('click',
                function () {
                    hide();
                    options.onSuccess(filter);
                });
    }

    function showDelete(filter, options) {
        if (!filter || !filter.id) return;

        init();

        show(filter, 'delete');

        wnd.find('.buttons .del')
            .unbind('click')
            .bind('click',
                function () {
                    hide();
                    options.onSuccess(filter);
                });
    }

    function showDeleteActionConfirm(filter, options) {
        init();

        show(filter, 'confirm');

        wnd.find('.buttons .confirm')
            .unbind('click')
            .bind('click',
                function () {
                    hide();
                    options.onConfirm(filter);
                });

        wnd.find('.buttons .change')
            .unbind('click')
            .bind('click',
                function() {
                    hide();
                    options.onChange(filter);
                });
    }

    function hide() {
        window.PopupKeyUpActionProvider.ClearActions();
        $.unblockUI();
    }

    return {
        init: init,
        showApply: showApply,
        showDelete: showDelete,
        showDeleteActionConfirm: showDeleteActionConfirm,
        hide: hide
    };
})(jQuery);