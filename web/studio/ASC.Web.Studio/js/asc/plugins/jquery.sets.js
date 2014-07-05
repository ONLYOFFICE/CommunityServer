/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
(function (jq, win, doc, body) {
    jQuery.extend({
        sets: function (options) {
            options = jQuery.extend({
                start: 0
            }, options);

            var _show = function () {
                var w = jq(window);

                drow(options.start);

                function drow(i) {
                    var $firstElem = jq(options.elems[i].selector);

                    if (($firstElem.length == 0) || ($firstElem.is(':visible') == false) && options.start != options.elems.length - 1) {
                        drow(i + 1);
                        return;
                    }

                    var id = "helperSet-" + i,
                        el = options.elems[parseInt(i)].selector,
                        content = options.elems[parseInt(i)].text,
                        collection = 0;

                    for (var j = i + 1; j < options.elems.length; j++) {
                        if ((jq(options.elems[j].selector).length != 0) && (jq(options.elems[j].selector).is(':visible') == true)) {
                            collection++;
                        }
                    }

                    jq('body').append('<div class="popup_helper" id="' + id + '">' + content + '</div>');
                    jq(el).helper({
                        BlockHelperID: id,
                        close: true,
                        next: true,
                        enableAutoHide: false,
                        addTop: options.elems[i].top,
                        addLeft: options.elems[i].left,
                        posDown: options.elems[i].posDown
                    });
                    w.scrollTop(jq(el).offset().top - w.height() / 2);

                    if (collection != 0) {

                        jq('#' + id + ' .nextHelp').on('click', function () {
                            jq('#' + id).hide();
                            while (jq(options.elems[parseInt(i + 1)].selector).is(':visible') == false) {
                                i++;
                            }
                            drow(i + 1);
                            w.scrollTop(jq('#helperSet-' + (i + 1)).offset().top - w.height() / 2);
                        });
                    }

                    if ((i == options.elems.length - 1) || (collection == 0)) {
                        jq('#' + id + ' .nextHelp').replaceWith("<a class='button gray close-tour'>" + ASC.Resources.Master.Resource.CloseButton + "</a>");
                    }

                    jq(".close-tour, .neverShow, .closeBlock").on('click', function () {
                        jq('#' + id).hide();
                    });
                }
            };

            if (options.elems) {
                _show();
            }

            return {
                show: _show
            };
        }
    });
})(jQuery, window, document, document.body);
