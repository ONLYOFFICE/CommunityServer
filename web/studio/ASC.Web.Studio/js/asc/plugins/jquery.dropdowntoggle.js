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
                    var targetPos = options.inPopup ? $selector.position() : $selector.offset();

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

                    var top = elemPosTop + addTop;
                    var bottom = "auto";

                    if (top + ddiOuterHeight > document.body.clientHeight + topPadding) {
                        top = "auto";
                        bottom = "0";
                    }

                    dropdownItem.css(
                        {
                            "position": position,
                            "top": top,
                            "left": elemPosLeft + addLeft,
                            "bottom": bottom
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