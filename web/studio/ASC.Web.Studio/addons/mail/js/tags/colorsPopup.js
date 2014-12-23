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

window.tagsColorsPopup = (function($) {
    var _callback;
    var _obj;
    var _panel;
    var _corner_left = 14;

    var init = function() {
        _panel = $('#tagsColorsPanel');

        _panel.find('div[colorstyle]').bind('click', function(e) {
            var style = $(this).attr('colorstyle');
            _callback(_obj, style);
        });
    };

    var show = function(obj, callback_func) {
        _callback = callback_func;
        _obj = obj;
        var $obj = $(obj);
        x = $obj.offset().left - _corner_left + $obj.width()/2;
        y = $obj.offset().top + $obj.height();
        _panel.css({ left: x-2, top: y, display: 'block', 'z-index': 2001 });

        $('body').bind('click.tagsColorsPopup', function(event) {
            var elt = (event.target) ? event.target : event.srcElement;
            if (!($(elt).is('.square') || $(elt).is('.square *') || $(elt).is('.leftRow span')))
                hide();
        });
    };

    var hide = function(obj) {
        _panel.hide();
        $('body').unbind("click.tagsColorsPopup");
    };

    return {
        init: init,
        show: show,
        hide: hide
    };
})(jQuery);