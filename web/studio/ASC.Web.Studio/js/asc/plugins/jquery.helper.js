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
    jq.fn.helper = function(opts) {
        var options = jQuery.extend({
            addTop: 0,
            addLeft: 0,
            position: "absolute",
            fixWinSize: false,
            popup: false,
            BlockHelperID: '', //obligatory  parameter
            enableAutoHide: true,
            close: false,
            next: false,
            posDown: false,
            inRelativeCnt: false
        }, opts);

        return this.each(function() {
            var w = jq(window),
                scrWidth = w.width(),
                scrHeight = w.height(),
                addTop = options.addTop-20,
                addLeft = options.addLeft-29,         // the left padding of the arrow
                topPadding = w.scrollTop(),
                leftPadding = w.scrollLeft();            

            if (options.position == "fixed") {
                addTop -= topPadding;
                addLeft -= leftPadding;
            }


            var $helpBlock = jq('#' + options.BlockHelperID),
                elem = jq(this),
                elemPos = elem.offset(),
                elemPosLeft = elemPos.left,
                elemPosTop = elemPos.top - $helpBlock.outerHeight();

            if (options.popup || options.inRelativeCnt) {
                elemPosTop = elem.position().top - $helpBlock.outerHeight();
                elemPosLeft = elem.position().left;
            }

            if (options.close) {
                if (jq('#' + options.BlockHelperID + ' .closeBlock').length == 0) {
                    $helpBlock.prepend('<div class="closeBlock"></div>');
                    jq('#' + options.BlockHelperID + ' .closeBlock').click(function() {
                        $helpBlock.hide();
                    });
                }
            }
            if (options.next) {
                if (jq('#' + options.BlockHelperID + ' .buttons').length == 0) {
                     $helpBlock.append('<div class="buttons"><a class="button gray nextHelp">' 
                     + ASC.Resources.Master.Resource.ShowNext + '</a><a class="neverShow">'
                     + ASC.Resources.Master.Resource.NeverShow + '</a></div>');                                     
                }
            }          

            jq('#' + options.BlockHelperID + ' ' + '.cornerHelpBlock').remove();

            if (options.fixWinSize && (elemPosLeft + addLeft + $helpBlock.outerWidth()) > (leftPadding + scrWidth)) {
                elemPosLeft = Math.max(0, leftPadding + scrWidth - $helpBlock.outerWidth()) - addLeft;
            }

            if ((elemPosTop + addTop < 0) && (!options.inRelativeCnt) || ((options.fixWinSize) && (elemPosTop > topPadding) &&
               ((elemPos.top + $helpBlock.outerHeight() + jq(this).outerHeight()) > (topPadding + scrHeight))) || (options.posDown)) {

                if ((elemPosLeft + addLeft + $helpBlock.outerWidth()) > jq(document).width()) {
                    elemPosLeft = elemPosLeft - addLeft - $helpBlock.outerWidth() + 40; // 40 for correct display of the direction corner
                    $helpBlock.prepend('<div class="cornerHelpBlock pos_bottom_left"></div>');
                } else {
                    $helpBlock.prepend('<div class="cornerHelpBlock pos_bottom"></div>');
                }
                elemPosTop = elemPos.top + jq(this).outerHeight();
                addTop = -addTop;

            } else {
                if ((elemPosLeft + addLeft + $helpBlock.outerWidth()) > jq(document).width()) {
                    elemPosLeft = elemPosLeft - addLeft - $helpBlock.outerWidth() + 40; // 40 for correct display of the direction corner
                    $helpBlock.append('<div class="cornerHelpBlock pos_top_left"></div>');
                } else {
                    $helpBlock.append('<div class="cornerHelpBlock pos_top"></div>');
                }
            }

            if (options.enableAutoHide) {

                jq(document).click(function(e) {
                    if (!jq(e.target).parents().addBack().is(elem)) {
                        $helpBlock.hide();
                    }
                });
                //                elem.click(function(e) {
                //                    e.stopPropagation();
                //                });
            }

            $helpBlock.css(
            {
              "top": elemPosTop + addTop,
              "left": elemPosLeft + addLeft,
              "position": options.position
            });
            jq(window).resize(function(){
                elemPosLeft = elem.offset().left;
                if ((elemPosLeft + addLeft + $helpBlock.outerWidth()) > jq(window).width()) {
                    elemPosLeft = elemPosLeft - addLeft - $helpBlock.outerWidth() + 40;
                }
                $helpBlock.css(
                {
                    "left": elemPosLeft + addLeft
                });
            });

            if ($helpBlock.css('display') == "none") {
                $helpBlock.show();
            } else {
                $helpBlock.hide();
            }
        });
    };

})(jQuery, window, document, document.body);