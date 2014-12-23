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