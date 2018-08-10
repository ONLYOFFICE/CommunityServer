/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


(function () {
    var dropdownToggleHash = {};
    jQuery.extend({
        dropdownToggle: function(options) {
            // default options
            options = jQuery.extend({
                //switcherSelector: "#id" or ".class",          - button
                //dropdownID: "id",                             - drop panel
                //anchorSelector: "#id" or ".class",            - near field
                //noActiveSwitcherSelector: "#id" or ".class",  - dont hide
                addTop: 0,
                addLeft: 0,
                position: "absolute",
                fixWinSize: true,
                enableAutoHide: true,
                beforeShowFunction: null,
                showFunction: null,
                afterShowFunction: null,
                hideFunction: null,
                alwaysUp: false,
                simpleToggle: false,
                rightPos: false,
                inPopup: false,
                toggleOnOver: false,
                sideToggle: false,
            }, options);

            var _toggle = function(switcherObj, dropdownID, addTop, addLeft, fixWinSize, position, anchorSelector, showFunction, alwaysUp, simpleToggle, beforeShowFunction, afterShowFunction, toggle, sideToggle) {
                var dropdownItem = jq("#" + dropdownID);

                if (typeof beforeShowFunction === "function") {
                    beforeShowFunction(switcherObj, dropdownItem);
                }

                if (typeof(simpleToggle) == "undefined" || simpleToggle === false) {
                    var $selector = jq(anchorSelector || switcherObj);
                    var targetPos = options.inPopup || jq.browser.mobile ? $selector.position() : $selector.offset();

                    if (!targetPos) {
                        return;
                    }

                    fixWinSize = fixWinSize === true;
                    addTop = addTop || 0;
                    addLeft = addLeft || 0;
                    position = position || "absolute";

                    var w = jq(window);
                    var topPadding = w.scrollTop();
                    var leftPadding = w.scrollLeft();

                    if (position == "fixed") {
                        addTop -= topPadding;
                        addLeft -= leftPadding;
                    }

                    var ddiOuterHeight = dropdownItem.outerHeight();
                    var ddiOuterWidth = dropdownItem.outerWidth();

                    var selectorHeight = $selector.outerHeight();
                    var selectorWidth = $selector.outerWidth();

                    var scrHeight = w.height();
                    var scrWidth = w.width();

                    var elemPosTop = targetPos.top + (sideToggle ? 0 : selectorHeight);
                    if (alwaysUp
                        || (fixWinSize
                            && (elemPosTop + addTop + ddiOuterHeight) > (topPadding + scrHeight)
                            && (targetPos.top - ddiOuterHeight) > topPadding)) {
                        elemPosTop = targetPos.top - ddiOuterHeight + (sideToggle ? selectorHeight : 0);
                        addTop *= -1;
                    }

                    var elemPosLeft = targetPos.left + (sideToggle ? selectorWidth :0);
                    if (options.rightPos) {
                        if (!sideToggle) {
                            elemPosLeft = Math.max(0, targetPos.left - ddiOuterWidth + selectorWidth);
                        }
                    } else if (fixWinSize
                        && (elemPosLeft + addLeft + ddiOuterWidth) > (leftPadding + scrWidth)) {
                        if (sideToggle) {
                            elemPosLeft = Math.max(0, targetPos.left - ddiOuterWidth);
                            addLeft *= -1;
                        } else {
                            elemPosLeft = Math.max(0, leftPadding + scrWidth - ddiOuterWidth) - addLeft;
                        }
                    }

                    dropdownItem.css(
                        {
                            "position": position,
                            "top": elemPosTop + addTop,
                            "left": elemPosLeft + addLeft
                        });
                }
                if (typeof showFunction === "function") {
                    showFunction(switcherObj, dropdownItem);
                }

                dropdownItem.toggle(toggle);

                if (typeof afterShowFunction === "function") {
                    afterShowFunction(switcherObj, dropdownItem);
                }
            };

            var _registerAutoHide = function(event, switcherSelector, dropdownSelector, hideFunction) {
                if (jq(dropdownSelector).is(":visible")) {
                    var $targetElement = jq((event.target) ? event.target : event.srcElement);
                    if (!$targetElement.parents().addBack().is(switcherSelector + ", " + dropdownSelector)) {
                        var e = jq.fixEvent(event);
                        if (typeof hideFunction === "function") {
                            if (hideFunction(e) === false) {
                                return;
                            }
                        }
                        if (e.button == 2) {
                            return;
                        }
                        jq(dropdownSelector).hide();
                    }
                }
            };

            if (options.switcherSelector && options.dropdownID) {
                var toggleFunc = function() {
                    _toggle(jq(this), options.dropdownID, options.addTop, options.addLeft, options.fixWinSize, options.position, options.anchorSelector, options.showFunction, options.alwaysUp, options.simpleToggle, options.beforeShowFunction, options.afterShowFunction, undefined, options.sideToggle);
                };
                if (!dropdownToggleHash.hasOwnProperty(options.switcherSelector + options.dropdownID)) {
                    jq(document).on("click", options.switcherSelector, toggleFunc);
                    dropdownToggleHash[options.switcherSelector + options.dropdownID] = true;

                    if (options.toggleOnOver && jq.browser.mobile === false) {
                        var timerToggle = null;

                        jq(document).on("mouseover mouseleave", options.switcherSelector + ",#" + options.dropdownID, function (e) {
                            clearTimeout(timerToggle);
                            var show = e.type == "mouseover";
                            if (show != jq("#" + options.dropdownID).is(":visible")) {
                                timerToggle = setTimeout(function () {
                                    _toggle(options.switcherSelector, options.dropdownID, options.addTop, options.addLeft, options.fixWinSize, options.position, options.anchorSelector, options.showFunction, options.alwaysUp, options.simpleToggle, options.beforeShowFunction, options.afterShowFunction, show, options.sideToggle);
                                }, 100);
                            }
                        });
                        jq(document).off("click", options.switcherSelector, toggleFunc);
                    }
                }
            }

            if (options.enableAutoHide && options.dropdownID) {
                var hideFunc = function(e) {
                    var allSwitcherSelectors = options.noActiveSwitcherSelector ?
                        options.switcherSelector + ", " + options.noActiveSwitcherSelector : options.switcherSelector;
                    _registerAutoHide(e, allSwitcherSelectors, "#" + options.dropdownID, options.hideFunction);

                };
                jq(document).unbind("click", hideFunc);
                jq(document).bind("click", hideFunc);
            }

            return {
                toggle: _toggle,
                registerAutoHide: _registerAutoHide
            };
        }
    });
})();