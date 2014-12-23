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

window.popup = (function($) {
    var _el,
        _popup_queue = [],
        _onUnblock;

    function init() {
        _el = $('#commonPopup');
    }

    function _show(size, blockui_opts) {
        var margintop = $(window).scrollTop() - 135;
        margintop = margintop + 'px';

        var blockui_defaults = {
            message: _el,
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: size,

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-261px',
                'margin-top': margintop,
                'background-color': 'White'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0
        };

        var opts = $.extend(blockui_defaults, blockui_opts);

        $.blockUI(opts);

        var cancelButton = _el.find('.popupCancel .cancelButton');
        cancelButton.attr('onclick', '');
        disableCancel(false);

        _el.find('.containerBodyBlock .buttons .cancel').add(cancelButton).unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;
            hide();
            return false;
        });
    };

    function hide() {
        if (_el.is(':visible')) {
            $.unblockUI({ onUnblock: _onUnblock });
            window.setImmediate(_process_queue);
        }
    }

    function _process_queue() {
        if (_el.is(':visible'))
            return;
        var item = _popup_queue.pop();
        if (item) {
            _el.find('div.containerHeaderBlock:first td:first').html(item.header);
            _el.find('div.containerBodyBlock:first').html(item.body);
            _onUnblock = item.onUnblock;
            _show(item.size, item.blockui_opts);
        }
    }

    function addBig(header, body, onUnblock, to_begining, blockui_opts) {
        addPopup(header, body, '530px', onUnblock, to_begining, blockui_opts);
    }

    function addSmall(header, body, onUnblock, to_begining, blockui_opts) {
        addPopup(header, body, '350px', onUnblock, to_begining, blockui_opts);
    }

    function addPopup(header, body, size, onUnblock, to_begining, blockui_opts) {
        if (!_el.is(':visible') || header != _el.find('div.containerHeaderBlock:first td:first')[0].innerHTML ||
            body != _el.find('div.containerBodyBlock:first')[0].innerHTML) {
            var item = { header: header, body: body, onUnblock: onUnblock, size: size, blockui_opts: blockui_opts };
            if(to_begining){
                _popup_queue.unshift(item);
            } else {
                _popup_queue.push(item);
            }
            _process_queue();
        }
    }

    function disableCancel(disable) {
        var cancel_x = _el.find('.popupCancel .cancelButton');
        if (cancel_x.length != 0) {
            TMMail.disableButton(cancel_x, disable);
            if (disable)
                cancel_x.css('cursor', 'default');
            else
                cancel_x.css('cursor', 'pointer');
        }
    }

    return {
        init: init,
        hide: hide,
        addBig: addBig,
        addSmall: addSmall,
        addPopup: addPopup,
        disableCancel: disableCancel
    };

})(jQuery);