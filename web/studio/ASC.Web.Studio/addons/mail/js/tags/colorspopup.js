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


window.tagsColorsPopup = (function($) {
    var callback;
    var obj;
    var panel;
    var cornerLeft = 14;

    function init() {
        panel = $('#tagsColorsPanel');

        panel.find('div[colorstyle]').bind('click', function() {
            var style = $(this).attr('colorstyle');
            callback(obj, style);
        });
    }

    function show(objParam, callbackFunc) {
        callback = callbackFunc;
        obj = objParam;
        var $obj = $(objParam);
        var x = $obj.offset().left - cornerLeft + $obj.width() / 2;
        var y = $obj.offset().top + $obj.height();
        panel.css({ left: x - 2, top: y, display: 'block', 'z-index': 2001 });

        $('body').bind('click.tagsColorsPopup', function(event) {
            var elt = (event.target) ? event.target : event.srcElement;
            if (!($(elt).is('.square') || $(elt).is('.square *') || $(elt).is('.leftRow span'))) {
                hide();
            }
        });
    }

    function hide() {
        panel.hide();
        $('body').unbind("click.tagsColorsPopup");
    }

    return {
        init: init,
        show: show,
        hide: hide
    };
})(jQuery);