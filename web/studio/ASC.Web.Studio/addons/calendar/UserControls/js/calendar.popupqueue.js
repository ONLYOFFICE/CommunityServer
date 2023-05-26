/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.CalendarPopupQueue = (function($) {
    var $el;
    var popupQueue = [];
    var onUnblock;

    function init() {
        $el = $('#commonPopup');
    }

    function show(size, blockuiOpts) {
        var options = $.extend(true, { bindEvents: false }, blockuiOpts || {});

        StudioBlockUIManager.blockUI($el, size, options);

        var cancelButton = $el.find('.popupCancel .cancelButton').attr('onclick', '');

        disableCancel(false);

        $el.find('.containerBodyBlock .buttons .cancel').add(cancelButton).off('click').on('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            hide();
            return false;
        });
    }

    function disableCancel(disable) {
        var cancelX = $el.find('.popupCancel .cancelButton');
        if (cancelX.length !== 0) {
            cancelX.toggleClass("disable", disable);
            if (disable) {
                cancelX.prop("disabled", true);
                cancelX.css('cursor', 'default');
            } else {
                cancelX.prop("disabled", false);
                cancelX.css('cursor', 'pointer');
            }
        }
    }

    function hide() {
        if ($el.is(':visible')) {
            $.unblockUI({ onUnblock: onUnblock });
            setTimeout(processQueue, 0);
        }
    }

    function processQueue() {
        if ($el.is(':visible')) {
            return;
        }
        var item = popupQueue.pop();
        if (item) {
            $el.find('div.containerHeaderBlock:first td:first').html(item.header);
            $el.find('div.containerBodyBlock:first').html(item.body);
            onUnblock = item.onUnblock;
            show(item.size, item.blockui_opts);
        }
    }

    function addPopup(header, body, unblockCallback, toBegining, blockuiOpts) {
        if (!$el.is(':visible') || header !== $el.find('div.containerHeaderBlock:first td:first')[0].innerHTML ||
            body !== $el.find('div.containerBodyBlock:first')[0].innerHTML) {
            var item = { header: header, body: body, onUnblock: unblockCallback, size: 530, blockui_opts: blockuiOpts };
            if (toBegining) {
                popupQueue.unshift(item);
            } else {
                popupQueue.push(item);
            }
            processQueue();
        }
    }

    return {
        init: init,
        hide: hide,
        addPopup: addPopup
    };
})(jQuery);