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
