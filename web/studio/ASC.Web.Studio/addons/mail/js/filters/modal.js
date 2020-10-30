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


window.filterModal = (function($) {
    var isInit = false,
        wnd;

    function init() {
        if (isInit === true) return;

        isInit = true;

        wnd = $('#filterWnd');
    }

    function blockUi(width, message) {
        window.StudioBlockUIManager.blockUI(message, width, { bindEvents: false });

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