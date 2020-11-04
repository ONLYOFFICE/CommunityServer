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


window.popup = (function($) {
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

        $el.find('.containerBodyBlock .buttons .cancel').add(cancelButton).unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            hide();
            return false;
        });
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

    function addBig(header, body, unblockCallback, toBegining, blockuiOpts) {
        addPopup(header, body, 530, unblockCallback, toBegining, blockuiOpts);
    }

    function addSmall(header, body, unblockCallback, toBegining, blockuiOpts) {
        addPopup(header, body, 350, unblockCallback, toBegining, blockuiOpts);
    }

    function addPopup(header, body, size, unblockCallback, toBegining, blockuiOpts) {
        if (!$el.is(':visible') || header !== $el.find('div.containerHeaderBlock:first td:first')[0].innerHTML ||
            body !== $el.find('div.containerBodyBlock:first')[0].innerHTML) {
            var item = { header: header, body: body, onUnblock: unblockCallback, size: size, blockui_opts: blockuiOpts };
            if (toBegining) {
                popupQueue.unshift(item);
            } else {
                popupQueue.push(item);
            }
            processQueue();
        }
    }

    function disableCancel(disable) {
        var cancelX = $el.find('.popupCancel .cancelButton');
        if (cancelX.length !== 0) {
            TMMail.disableButton(cancelX, disable);
            if (disable) {
                cancelX.css('cursor', 'default');
            } else {
                cancelX.css('cursor', 'pointer');
            }
        }
    }
    
    function success(text) {
        if (!$el.is(':visible')) return;
        showNotification("toast-success", text);
    }
    
    function info(text) {
        if (!$el.is(':visible')) return;
        showNotification("toast-info", text);
    }

    function warning(text) {
        if (!$el.is(':visible')) return;
        showNotification("toast-warning", text);
    }

    function error(text) {
        if (!$el.is(':visible')) return;
        showNotification("toast-error", text);
    }

    function showNotification(cssClass, text) {
        var $notification = $.tmpl("popupNotificationTmpl", { cssClass: cssClass, text: text });

        $el.find('.toast-popup-container').remove();

        var $progressBox = $el.find('.progressContainer');
        var $buttonsBox = $el.find('.buttons');

        if ($progressBox.length) {
            $progressBox.find('.loader').hide();
            $progressBox.append($notification);
        } else if ($buttonsBox.length) {
            $buttonsBox.before($notification);
        } else {
            $el.find('.containerBodyBlock').append($notification);
        }

        setTimeout(function() {
            $notification.remove();
        }, 5000);
    }

    return {
        init: init,
        hide: hide,
        addBig: addBig,
        addSmall: addSmall,
        addPopup: addPopup,
        disableCancel: disableCancel,
        
        success: success,
        info: info,
        warning: warning,
        error: error
    };
})(jQuery);