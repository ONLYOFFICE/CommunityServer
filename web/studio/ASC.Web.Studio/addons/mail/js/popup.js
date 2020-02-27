/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.popup = (function($) {
    var $el;
    var popupQueue = [];
    var onUnblock;

    function init() {
        $el = $('#commonPopup');
    }

    function show(size, blockuiOpts) {
        blockuiOpts = blockuiOpts || {};

        var defaultOptions = {
            css: {
                top: '-100%'
            },
            bindEvents: false
        };

        var options = $.extend(true, defaultOptions, blockuiOpts);
        StudioBlockUIManager.blockUI($el, size, null, null, null, options);

        $el.closest(".blockUI").css({
            'top': '50%',
            'margin-top': '-{0}px'.format($el.height() / 2)
        });

        var cancelButton = $el.find('.popupCancel .cancelButton');
        cancelButton.attr('onclick', '');
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
            window.setImmediate(processQueue);
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