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


if (typeof jq == "undefined")
    var jq = jQuery.noConflict();

// Production steps of ECMA-262, Edition 6, 22.1.2.1
if (!Array.from) {
    Array.from = (function () {
        var toStr = Object.prototype.toString;
        var isCallable = function (fn) {
            return typeof fn === 'function' || toStr.call(fn) === '[object Function]';
        };
        var toInteger = function (value) {
            var number = Number(value);
            if (isNaN(number)) { return 0; }
            if (number === 0 || !isFinite(number)) { return number; }
            return (number > 0 ? 1 : -1) * Math.floor(Math.abs(number));
        };
        var maxSafeInteger = Math.pow(2, 53) - 1;
        var toLength = function (value) {
            var len = toInteger(value);
            return Math.min(Math.max(len, 0), maxSafeInteger);
        };

        // The length property of the from method is 1.
        return function from(arrayLike/*, mapFn, thisArg */) {
            // 1. Let C be the this value.
            var C = this;

            // 2. Let items be ToObject(arrayLike).
            var items = Object(arrayLike);

            // 3. ReturnIfAbrupt(items).
            if (arrayLike == null) {
                throw new TypeError('Array.from requires an array-like object - not null or undefined');
            }

            // 4. If mapfn is undefined, then let mapping be false.
            var mapFn = arguments.length > 1 ? arguments[1] : void undefined;
            var T;
            if (typeof mapFn !== 'undefined') {
                // 5. else
                // 5. a If IsCallable(mapfn) is false, throw a TypeError exception.
                if (!isCallable(mapFn)) {
                    throw new TypeError('Array.from: when provided, the second argument must be a function');
                }

                // 5. b. If thisArg was supplied, let T be thisArg; else let T be undefined.
                if (arguments.length > 2) {
                    T = arguments[2];
                }
            }

            // 10. Let lenValue be Get(items, "length").
            // 11. Let len be ToLength(lenValue).
            var len = toLength(items.length);

            // 13. If IsConstructor(C) is true, then
            // 13. a. Let A be the result of calling the [[Construct]] internal method 
            // of C with an argument list containing the single item len.
            // 14. a. Else, Let A be ArrayCreate(len).
            var A = isCallable(C) ? Object(new C(len)) : new Array(len);

            // 16. Let k be 0.
            var k = 0;
            // 17. Repeat, while k < len… (also steps a - h)
            var kValue;
            while (k < len) {
                kValue = items[k];
                if (mapFn) {
                    A[k] = typeof T === 'undefined' ? mapFn(kValue, k) : mapFn.call(T, kValue, k);
                } else {
                    A[k] = kValue;
                }
                k += 1;
            }
            // 18. Let putStatus be Put(A, "length", len, true).
            A.length = len;
            // 20. Return A.
            return A;
        };
    }());
}

//ie11
if (!Array.prototype.find) {
    Array.prototype.find = function (predicate) {
        if (this == null) {
            throw new TypeError('Array.prototype.find called on null or undefined');
        }
        if (typeof predicate !== 'function') {
            throw new TypeError('predicate must be a function');
        }
        var list = Object(this);
        var length = list.length >>> 0;
        var thisArg = arguments[1];
        var value;

        for (var i = 0; i < length; i++) {
            value = list[i];
            if (predicate.call(thisArg, value, i, list)) {
                return value;
            }
        }
        return undefined;
    };
}

toastr.options.hideDuration = 100;
toastr.options.timeOut = 8000;

jQuery.extend(
    jQuery.expr[":"],
    {
        reallyvisible: function (a) {
            return !(jQuery(a).is(':hidden') || jQuery(a).parents(':hidden').length);
        }
    }
);

jQuery.fn.yellowFade = function () {
    return (this.css({backgroundColor: "#ffffcc"}).animate(
        {backgroundColor: "#ffffff"},
        1500,
        function () {
            jq(this).css({backgroundColor: ""});
        }));
};

jQuery.fn.colorFade = function(color, tiemout, cb) {
    return (this.css({ backgroundColor: color, borderColor: color }).animate(
        { backgroundColor: "#ffffff" },
        tiemout,
        function() {
            jq(this).css({ backgroundColor: "", borderColor: "" });
            if (cb) {
                cb();
            }
        }));
};

// google analytics track
var trackingGoogleAnalytics = function (ctg, act, lbl) {
    try {
        if (window.ga) {
            window.ga('www.send', 'event', ctg, act, lbl);
            window.ga('testTracker.send', 'event', ctg, act, lbl);
        }
    } catch (err) {
    }
};

jQuery.fn.trackEvent = function (category, action, label, typeAction) { // only for static objects (don't use for elements added dynamically)

    switch (typeAction) {
        case "click":
            jq(this).on("click", function () {
                trackingGoogleAnalytics(category, action, label);
                return true;
            });
            break;
        case "enter":
            jq(this).keypress(function (e) {
                if (e.which == 13) {
                    trackingGoogleAnalytics(category, action, label);
                }
                return true;
            });
            break;
        default:
            jq(this).on("click", function () {
                trackingGoogleAnalytics(category, action, label);
                return true;
            });
            break;
    }
};

if (!jQuery.fn.zIndex) {
    jQuery.fn.zIndex = function(value) {
        if (value) {
            return this.css("z-index", value);
        }
        if (this.length) {
            for (var item = jQuery(this[0]); item.length && item[0] !== document;) {
                var position = item.css("position");
                var zIndex = parseInt(item.css("z-index"), 10);
                if (("absolute" === position || "relative" === position || "fixed" === position) && (!isNaN(zIndex) && 0 !== zIndex)) {
                    return zIndex;
                }
                item = item.parent();
            }
        }
        return 0;
    };
}

if (!jQuery.fn.andSelf) {
    jQuery.fn.andSelf = function () {
        return this.add(this.prevObject);
    };
}

jQuery.extend({
    loadTemplates: function(templateUrl) {
        jQuery.get(templateUrl, function(templates) {
            jQuery(templates).filter("script")
                .each(function() {
                    var templateName = jQuery(this).attr("id");
                    jQuery(this).template(templateName);
                });
        });
    },

    /*
    var result = $.format("Hello, {0}.", "world");
    //result -> "Hello, world."
    */
    format: function jQuery_dotnet_string_format(text) {
        //check if there are two arguments in the arguments list
        if (arguments.length <= 1) {
            //if there are not 2 or more arguments there's nothing to replace
            //just return the text
            return text;
        }
        //decrement to move to the second argument in the array
        var tokenCount = arguments.length - 2;
        for (var token = 0; token <= tokenCount; ++token) {
            //iterate through the tokens and replace their placeholders from the text in order
            text = text.replace(new RegExp("\\{" + token + "\\}", "gi"), arguments[token + 1]);
        }
        return text;
    },

    htmlEncodeLight: function(string) {
        var newStr = string.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\n/ig, '<br/>').replace('&amp;', '&');
        return newStr;
    },

    linksParser: function(val) {
        var replaceUrl = function(str) {
            return '<a target="_blank" href="' + (/^http/.test(str) ? str : 'http://' + str) + '">' + str + '</a>';
        };
        var regUrl = /(\b(((https?|ftp|file):\/\/)|(www.))[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig;
        return val.replace(regUrl, replaceUrl);
    },

    registerHeaderToggleClick: function(parentSelector, toggleLineSelector) {
        if (typeof (parentSelector) != "string")
            return;
        jq(parentSelector).on("click", ".headerToggle, .openBlockLink, .closeBlockLink", function () {
            var $lineElt = jq(this).parents(toggleLineSelector + ":first");
            $lineElt.nextUntil(toggleLineSelector).toggle();
            $lineElt.toggleClass("open");
        });
    },

    getURLParam: function(strParamName, urlToParse) {
        strParamName = strParamName.toLowerCase();

        var strReturn = "";
        var strHref = urlToParse ? urlToParse : window.location.href.toLowerCase();
        var bFound = false;

        var cmpstring = strParamName + "=";
        var cmplen = cmpstring.length;

        if (strHref.indexOf("?") > -1) {
            var strQueryString = strHref.substr(strHref.indexOf("?") + 1);
            var aQueryString = strQueryString.split("&");
            for (var iParam = 0; iParam < aQueryString.length; iParam++) {
                if (aQueryString[iParam].substr(0, cmplen) == cmpstring) {
                    var aParam = aQueryString[iParam].split("=");
                    strReturn = aParam[1];
                    bFound = true;
                    break;
                }
            }
        }
        if (bFound == false) {
            return null;
        }

        if (strReturn.indexOf("#") > -1) {
            return strReturn.split("#")[0];
        }

        return strReturn;
    },

    isDateFormat: function(txtDate) {
        var aoDate, // needed for creating array and object
            ms, // date in milliseconds
            month, day, year; // (integer) month, day and year

        var dateFormat = Teamlab.constants.dateFormats.date.replace(/ /g, '');
        var separator = "/";
        var dateFormatComponent = dateFormat.replace(/[^dDmMyY\/]/g, '').split(separator);
        if (dateFormatComponent.length == 1) {
            separator = ".";
            dateFormatComponent = dateFormat.replace(/[^dDmMyY.]/g, '').split(separator);
            if (dateFormatComponent.length == 1) {
                separator = "-";
                dateFormatComponent = dateFormat.replace(/[^dDmMyY-]/g, '').split(separator);
                if (dateFormatComponent.length == 1) {
                    return "Unknown format date";
                }
            }
        }

        dateFormatComponent = dateFormatComponent.filter(function (el) { return el; });

        var regex = new RegExp('[^\\[0-9\\]' + separator + ']', 'g');

        // split input date to month, day and year
        aoDate = txtDate.replace(regex, '').split(separator);

        aoDate = aoDate.filter(function (el) { return el; });

        // array length should be exactly 3 (no more no less)
        if (aoDate.length !== 3) {
            return false;
        }
        // define month, day and year from array (expected format is m/d/yyyy)
        // subtraction will cast variables to integer implicitly
        for (var i = 0; i < dateFormatComponent.length; i++) {
            if (dateFormatComponent[i][0]) {
                if (dateFormatComponent[i].trim()[0].toLowerCase() == "d") {
                    day = aoDate[i] - 0;
                }
                if (dateFormatComponent[i].trim()[0].toLowerCase() == "m") {
                    month = aoDate[i] - 1; // because months in JS start from 0
                }
                if (dateFormatComponent[i].trim()[0].toLowerCase() == "y") {
                    year = aoDate[i] - 0;
                }
            }
        }

        // test year range
        if (year < 1000 || year > 3000) {
            return false;
        }
        // convert input date to milliseconds
        ms = (new Date(year, month, day)).getTime();
        // initialize Date() object from milliseconds (reuse aoDate variable)
        aoDate = new Date();
        aoDate.setTime(ms);
        // compare input date and parts from Date() object
        // if difference exists then input date is not valid
        if (aoDate.getFullYear() !== year ||
            aoDate.getMonth() !== month ||
            aoDate.getDate() !== day) {
            return false;
        }
        // date is OK, return true
        return true;
    },

    isValidEmail: function(email) {
        return (ASC.Mail && ASC.Mail.Utility && ASC.Mail.Utility.IsValidEmail) ?
            ASC.Mail.Utility.IsValidEmail(email) :
            new RegExp(ASC.Resources.Master.EmailRegExpr, "i").test(email);
    },

    switcherAction: function(el, block) {
        var elem = jq(el);
        elem.on("click", function() {
            var bl = jq(block);
            var sw = elem.attr("data-switcher");

            if (sw == "0") {
                elem.attr("data-switcher", "1");
                elem.text(elem.attr("data-showtext"));
                bl.hide();
            } else {
                elem.attr("data-switcher", "0");
                elem.text(elem.attr("data-hidetext"));
                bl.show();
            }
        });
    },

    confirmBeforeUnload: function(check){
        window.onbeforeunload = function (e) {
            if (typeof (check) != "function" || check())
                return ASC.Resources.Master.Resource.WarningMessageBeforeUnload;
        };
    },

    fixEvent: function (e) {
        e = e || window.event;
        if (!e) {
            return {};
        }

        if (e.pageX == null && e.clientX != null) {
            var html = document.documentElement,
                body = document.body;
            e.pageX = e.clientX + (html && html.scrollLeft || body && body.scrollLeft || 0) - (html.clientLeft || 0);
            e.pageY = e.clientY + (html && html.scrollTop || body && body.scrollTop || 0) - (html.clientTop || 0);
        }

        if (!e.which && e.button) {
            e.which = e.button & 1 ? 1 : (e.button & 2 ? 3 : (e.button & 4 ? 2 : 0));
        }

        return e;
    },

    showDropDownByContext: function (evt, target, dropdownItem, showFunction) {
        var ddiHeight = dropdownItem.innerHeight(),
            ddiWidth = dropdownItem.innerWidth(),

            w = jq(window),
            scrScrollTop = w.scrollTop(),
            scrHeight = w.height();

        if (target.is(".entity-menu") || target.is(".menu-small")) {
            target.addClass("active");

            var position = jq.browser.mobile ? target.position() : target.offset();
            var baseTop = position.top + target.outerHeight() - 2;
            var correctionY =
                ddiHeight > position.top
                    ? 0
                    : (scrHeight + scrScrollTop - baseTop > ddiHeight ? 0 : ddiHeight);

            var top = baseTop - correctionY + (correctionY == 0 ? 2 : -target.outerHeight() - 2);
            var bottom = "auto";

            if (top + ddiHeight > document.body.clientHeight + scrScrollTop) {
                top = "auto";
                bottom = "0";
            }

            dropdownItem.css({
                "top": top,
                "left": position.left + target.outerWidth() - ddiWidth + 10,
                "right": "auto",
                "bottom": bottom
            });
        } else {
            var correctionX = document.body.clientWidth - (evt.pageX - pageXOffset + ddiWidth) > 0 ? 0 : ddiWidth;
            correctionY =
                ddiHeight > evt.pageY
                    ? 0
                    : (scrHeight + scrScrollTop - evt.pageY > ddiHeight ? 0 : ddiHeight);

            var top = evt.pageY - correctionY;
            var bottom = "auto";

            if (top + ddiHeight > document.body.clientHeight + scrScrollTop) {
                top = "auto";
                bottom = "0";
            }

            dropdownItem.css(
                {
                    "top": top,
                    "left": evt.pageX - correctionX,
                    "right": "auto",
                    "bottom": bottom,
                    "margin": "0"
                });
        }

        if (typeof showFunction === "function") {
            if (showFunction(evt) === false) {
                return;
            }
        }

        dropdownItem.show();
    }
});

String.prototype.format = function() {
    var txt = this,
        i = arguments.length;

    while (i--) {
        txt = txt.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return txt;
};

String.prototype.removeAt = function (start, length) {
    var chars = this.split("");
    chars.splice(start, length);
    return chars.join("");
}

StudioBlockUIManager = {
    blockUI: function (obj, width, opts) {
        try {
            var defaultOptions = {
                message: jq(obj),
                css: {
                    backgroundColor: "transparent",
                    border: "none",
                    cursor: "default",
                    left: "50%",
                    opacity: "1",
                    overflow: "visible",
                    padding: "0px",
                    position: "fixed",
                    textAlign: "left",
                    width: (typeof width == "number" && width > 0) ? width + "px" : "auto",
                    transform: "translate(-50%, -50%)"
                },

                overlayCSS: {
                    backgroundColor: "#aaa",
                    cursor: "default",
                    opacity: "0.3"
                },

                focusInput: true,
                baseZ: 666,

                fadeIn: 0,
                fadeOut: 0,

                onBlock: function () {
                    var matrix = this.css("transform").match(/-?\d+\.?\d*/g);
                    this.css("transform", "");
                    this.css("margin-left", parseInt(matrix[4]) + "px");
                    this.css("margin-top", parseInt(matrix[5]) + "px");

                    if (jq.browser.mobile) {
                        jq(obj).get(0).scrollIntoView({ block: "start", inline: "start" });
                    }
                }
            };

            jq.blockUI(jq.extend(true, defaultOptions, opts || {}));
        } catch (e) {
        }
    }
};

//------------ck uploads for comments-------------------
var FCKCommentsController = new function () {
    this.Callback = null;
    this.EditCommentHandler = function (commentID, text, domain, isEdit, callback) {
        this.Callback = callback;
        if (text == null || text == undefined)
            text = "";

        Teamlab.fckeEditCommentComplete({},
            {commentid : commentID, domain: domain, html: text, isedit: isEdit},
            this.CallbackHandler);
    };

    this.CancelCommentHandler = function (commentID, domain, isEdit, callback) {
        this.Callback = callback;

        Teamlab.fckeCancelCommentComplete({},
            { commentid: commentID, domain: domain, isedit: isEdit },
            this.CallbackHandler);
    };

    this.RemoveCommentHandler = function (commentID, domain, callback) {
        this.Callback = callback;

        Teamlab.fckeRemoveCommentComplete({},
            { commentid: commentID, domain: domain },
            this.CallbackHandler);
    };

    this.CallbackHandler = function (result) {
        if (FCKCommentsController.Callback != null && FCKCommentsController.Callback != '')
            ;
        eval(FCKCommentsController.Callback + '()');
    };
};

/**
 * PopupKeyUpActionProvider
 */
var PopupKeyUpActionProvider = new function () {
    //close dialog by esc
    jq(document).keyup(function (event) {
        if (!PopupKeyUpActionProvider.ForceBinding && !jq('.popupContainerClass').is(':visible'))
            return;

        var code;
        if (!e)
            var e = event;
        if (e.keyCode)
            code = e.keyCode;
        else if (e.which)
            code = e.which;

        if (code == 27 && PopupKeyUpActionProvider.EnableEsc) {
            PopupKeyUpActionProvider.CloseDialog();
        } else if ((code == 13) && e.ctrlKey) {
            if (PopupKeyUpActionProvider.CtrlEnterAction != null && PopupKeyUpActionProvider.CtrlEnterAction != '') {
                eval(PopupKeyUpActionProvider.CtrlEnterAction);
            }
        } else if (code == 13) {
            if (e.target.nodeName.toLowerCase() !== 'textarea' && PopupKeyUpActionProvider.EnterAction != null && PopupKeyUpActionProvider.EnterAction != '') {
                if (typeof PopupKeyUpActionProvider.EnterAction === "string") {
                    eval(PopupKeyUpActionProvider.EnterAction);
                } else if (typeof PopupKeyUpActionProvider.EnterAction === "function") {
                    PopupKeyUpActionProvider.EnterAction();
                }
            }
            if (e.target.nodeName.toLowerCase() !== 'textarea' && PopupKeyUpActionProvider.EnterActionCallback != null && PopupKeyUpActionProvider.EnterActionCallback != '')
                eval(PopupKeyUpActionProvider.EnterActionCallback);
        }

    });

    this.CloseDialog = function () {
        jq.unblockUI();

        if (PopupKeyUpActionProvider.CloseDialogAction != null && PopupKeyUpActionProvider.CloseDialogAction != '') {
            eval(PopupKeyUpActionProvider.CloseDialogAction);
        }
        PopupKeyUpActionProvider.ClearActions();
    };

    this.CloseDialogAction = '';
    this.EnterAction = '';
    this.EnterActionCallback = '';
    this.CtrlEnterAction = '';
    this.EnableEsc = true;
    this.ForceBinding = false;

    this.ClearActions = function () {
        this.CloseDialogAction = '';
        this.EnterAction = '';
        this.EnterActionCallback = '';
        this.CtrlEnterAction = '';
        this.EnableEsc = true;
        this.ForceBinding = false;
        if (typeof (TMTalk) != 'undefined') {
            TMTalk.hideDialog();
        }
    };
};

/**
 * StudioManager
 */
var StudioManager = new function () {
    this.GetImage = function (imageName) {
        return ASC.Resources.Master.ImageWebPath + "/" + imageName;
    };

    this.getLocationPathToModule = function (moduleName) { // example path = jq.getLocationPathToModule("Projects") - http://localhost/Products/Projects/
        return window.location.origin + "/Products/" + moduleName + "/";
    };

    this.getBasePathToModule = function () {
        var products = "Products",
            addons = "addons",
            mass,
            parts,
            moduleName = "",
            path = "/";

        mass = location.pathname.split(products);
        if (mass.length > 1) {
            parts = mass[1].split('/');
            if (parts.length > 1) {
                moduleName = parts[1] + "/";
            }
            path = mass[0] + products + "/" + moduleName;
            return path;
        }

        mass = location.pathname.toLowerCase().split(addons);
        if (mass.length > 1) {
            parts = mass[1].split('/');
            if (parts.length > 1) {
                moduleName = parts[1] + "/";
            }
            path = mass[0] + addons + "/" + moduleName;
            return path;
        }
        return path;
    };

    this.getCurrentModule = function () {
        var parts = location.pathname.toLowerCase().split("/");
        return parts.length > 2 ? parts[2] : (parts.length === 2 && parts[1] !== "" ? parts[1].replace(".aspx", "") : "main");
    };

    this.createCustomSelect = function (selects, hiddenBorder, AdditionalBottomOption) {
        if (typeof selects === 'string') {
            selects = document.getElementById(selects);
        }
        if (!selects || typeof selects !== 'object') {
            return undefined;
        }
        if (typeof selects.join !== 'function' && !(selects instanceof String)) {
            selects = [selects];
        }

        for (var i = 0, n = selects.length; i < n; i++) {
            var select = selects[i];
            var selectValue = select.value;


            if (select.className.indexOf('originalSelect') !== -1) {
                continue;
            }

            var container = document.createElement('div');
            container.setAttribute('value', selectValue);
            container.className = select.className + (select.className.length ? ' ' : '') + 'customSelect';
            var position = (document.defaultView && document.defaultView.getComputedStyle) ? document.defaultView.getComputedStyle(select, '').getPropertyValue('position') : (select.currentStyle ? select.currentStyle['position'] : 'static');
            container.style.position = position === 'static' ? 'relative' : position;

            var title = document.createElement('div');
            title.className = 'title' + ' ' + selectValue;
            title.style.height = '100%';
            title.style.position = 'relative';
            title.style.zIndex = '1';
            container.appendChild(title);

            var selector = document.createElement('div');
            selector.className = 'selector';
            selector.style.position = 'absolute';
            selector.style.zIndex = '1';
            selector.style.right = '0';
            selector.style.top = '0';
            selector.style.height = '100%';
            container.appendChild(selector);

            var optionsList = document.createElement('ul');
            optionsList.className = 'options';
            optionsList.style.display = 'none';
            optionsList.style.position = 'absolute';
            optionsList.style.zIndex = '777';
            optionsList.style.width = '115%';
            optionsList.style.maxHeight = '200px';
            optionsList.style.overflow = 'auto';
            container.appendChild(optionsList);

            var optionHtml = '',
                optionValue = '',
                optionTitle = '',
                optionClassName = '',
                fullClassName = '',
                option = null,
                options = select.getElementsByTagName('option');
            for (var j = 0, m = options.length; j < m; j++) {
                option = document.createElement('li');
                optionValue = options[j].value;
                optionHtml = options[j].innerHTML;
                optionTitle = optionHtml.replace(/&amp;/gi, "&");
                optionsList.appendChild(option);
                fullClassName = 'option' + ' ' + optionValue + ((optionClassName = options[j].className) ? ' ' + optionClassName : '');
                option.setAttribute('title', optionTitle);
                option.setAttribute('value', optionValue);
                if (selectValue === optionValue) {
                    title.innerHTML = optionHtml;
                    fullClassName += ' selected';
                }
                option.selected = selectValue === optionValue;
                option.innerHTML = optionHtml;
                option.className = fullClassName;
            }
            if (AdditionalBottomOption !== undefined && AdditionalBottomOption !== "") {
                optionsList.appendChild(AdditionalBottomOption);
            }

            select.parentNode.insertBefore(container, select);
            container.appendChild(select);

            select.className += (select.className.length ? ' ' : '') + 'originalSelect';
            select.style.display = 'none';

            if (hiddenBorder) {
                jq(container).addClass('comboBoxHiddenBorder');
            }


            jq(optionsList).bind('selectstart', function () {
                return false;
            }).mousedown(function () {
                return false;
            }).click(function (evt) {
                var $target = jq(evt.target);
                if ($target.hasClass('option')) {
                    var containerNewValue = evt.target.getAttribute('value'),
                        $container = $target.parents('div.customSelect:first'),
                        container = $container.get(0);
                    if (!container || container.getAttribute('value') === containerNewValue) {
                        return undefined;
                    }
                    container.setAttribute('value', containerNewValue);
                    $container.find('li.option').removeClass('selected').filter('li.' + containerNewValue + ':first').addClass('selected');
                    $container.find('div.title:first').html($target.html() || '&nbsp;')
                        .attr('className', 'title ' + containerNewValue)
                        .attr('class', 'title ' + containerNewValue);
                    $container.find('select.originalSelect:first').val(containerNewValue).change();
                }
            });
            if (jq.browser.msie && jq.browser.version < 7) {
                jq(optionsList).find('li.option').hover(
                    function () {
                        jq(this).addClass('hover');
                    }, function () {
                        jq(this).removeClass('hover');
                    });
            }

            jq(selector).add(title).bind('selectstart', function () {
                return false;
            }).mousedown(function () {
                return false;
            }).click(function (evt) {
                var $options = jq(this.parentNode).find('ul.options:first');
                if ($options.is(':hidden')) {
                    $options.css({
                        top: jq(this.parentNode).height() + 1 + 'px'
                    }).slideDown(1, function () {
                        jq(document).one('click', function () {
                            jq('div.customSelect ul.options').hide();
                            jq(container).removeClass('comboBoxHiddenBorderFocused');
                        });
                    });

                    if (hiddenBorder) {
                        if (!jq(container).hasClass('comboBoxHiddenBorderFocused')) {
                            container.className = 'comboBoxHiddenBorderFocused ' + container.className;
                        }
                        var leftOffset = jq(container).width() - jq(select).width() - 1;
                        //                        if (jQuery.browser.mozilla) { leftOffset -= 1;    }
                        $options.css({
                            'width': jq(select).width(),
                            'border-top': '1px solid #c7c7c7',
                            'left': leftOffset,
                            'top': jq(container).height()
                        });
                    }
                }
            });
        }
    };

    this.CloseAddContentDialog = function () {
        jq.unblockUI();
        return false;
    };

    this.ShowAddContentDialog = function () {
        StudioBlockUIManager.blockUI("#studio_AddContentDialog", 400);
    };

    this.Disable = function (obj_id) {
        jq('#' + obj_id).addClass("disable");
    };

    this.Enable = function (obj_id) {
        jq('#' + obj_id).removeClass("disable");
    };

    this.initImageZoom = function (options) {
        jq(".mediafile, .screenzoom").click(function (event) {
            event.stopPropagation();

            jq(window).click();

            var playlist = [];
            var selIndex = 0;

            jq(".mediafile, .screenzoom").each(function (i, v) {
                playlist.push({ title: v.title, id: i, src: v.href });
                if (v.href == event.currentTarget.href)
                    selIndex = i;
            });

            ASC.Files.MediaPlayer.init(-1, {
                playlist: playlist,
                playlistPos: selIndex,
                downloadAction: function (fileId) {
                    return playlist[fileId].src;
                }
            });

            return false;
        });
    };

    var pendingRequests = new Array();
    var pendingRequestMade = false;
    var pendingReqestTimer = null;
    this.addPendingRequest = function (func) {
        if (typeof func != "function") {
            return;
        }

        if (pendingRequestMade) {
            func.apply();
            return;
        }

        pendingRequests.push(func);

        if (pendingReqestTimer == null) {
            pendingReqestTimer = setTimeout(function () {
                pendingRequestMade = true;
                for (var i = 0; i < pendingRequests.length; i++) {
                    pendingRequests[i].apply();
                }
            }, 3600);
        }
    };
};

/*--------Error messages for required field-------*/
function ShowRequiredError (item, withouthScroll, withouthFocus) {
    jq("div[class='infoPanel alert']").hide();
    jq("div[class='infoPanel alert']").empty();
    var parentBlock = jq(item).parents(".requiredField");
    jq(parentBlock).addClass("requiredFieldError");

    if (typeof(withouthScroll) == "undefined" || withouthScroll == false) {
        jq.scrollTo(jq(parentBlock).position().top - 50, {speed: 500});
    }

    if (typeof (withouthFocus) == "undefined" || withouthFocus == false) {
        jq(item).focus();
    }
}

function HideRequiredError () {
    jq(".requiredField").removeClass("requiredFieldError");
}

function RemoveRequiredErrorClass (item) {
    var parentBlock = jq(item).parents(".requiredField");
    jq(parentBlock).removeClass("requiredFieldError");
}

function AddRequiredErrorText (item, text) {
    var parent = jq(item).parents(".requiredField");
    var errorSpan = jq(parent).children(".requiredErrorText");
    jq(errorSpan).text(text);
}

function SortData(a, b) {
    var compA = a.title.toLowerCase(),
        compB = b.title.toLowerCase();
    return (compA < compB) ? -1 : (compA > compB) ? 1 : 0;
}

/**
 * EmailOperationManager
 */

if (typeof (ASC) === 'undefined') {
    ASC = {};
}

ASC.EmailOperationManager = (function () {
    function sendInstructionsHelper(emailOperationServiceSendInstruction, responseAction, reload) {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_emailChangeDialog");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_emailChangeDialog");
            }
        };
        emailOperationServiceSendInstruction(function (response) {
            if (responseAction) {
                responseAction(response);
            }

            if (response.error != null) {
                toastr.error(response.error.Message);
            } else {
                PopupKeyUpActionProvider.ClearActions();
                jq.unblockUI();
                toastr.success(response.value);
                if (reload)
                    document.location.reload(true);
            }
        });
    };

    function sendEmailActivationInstructions(userEmail, userID, responseAction) {
        sendInstructionsHelper(EmailOperationService.SendEmailActivationInstructions.bind(EmailOperationService, userID, userEmail), responseAction);
    };

    function sendEmailChangeInstructions(userEmail, userID, responseAction) {
        sendInstructionsHelper(EmailOperationService.SendEmailChangeInstructions.bind(EmailOperationService, userID, userEmail), responseAction, Teamlab.profile.isAdmin);
    };

    function showEmailChangeWindow (userEmail, userID, responseAction) {
        jq("#divEmailOperationError").html("").hide();
        jq("#studio_emailOperationResult").hide();

        jq("#emailInputContainer").removeClass("display-none");
        jq("#emailMessageContainer").addClass("display-none");
        jq("#btEmailOperationSend").addClass("disable");

        jq("#resendInviteDialogPopupHeader").addClass("display-none");
        jq("#emailActivationDialogPopupHeader").addClass("display-none");
        jq("#emailChangeDialogPopupHeader").removeClass("display-none");

        jq("#studio_emailOperationContent").removeClass("display-none");

        jq("#emailChangeDialogText").removeClass("display-none");
        jq("#resendInviteDialogText").addClass("display-none");
        jq("#emailActivationDialogText").addClass("display-none");

        jq("#emailOperation_email").val(userEmail);
        
        openPopupDialog();
        
        jq("#btEmailOperationSend").unbind("click");

        jq("#btEmailOperationSend").click(function () {
            if (jq(this).hasClass("disable")) return false;
            var newEmail = jq("#emailOperation_email").val();
            sendEmailChangeInstructions(newEmail, userID, responseAction);
            return false;
        });
        
        jq("#emailOperation_email").off(".emailChange");

        jq("#emailOperation_email").on("keyup.emailChange paste.emailChange cut.emailChange", function (key) {
            var checkEmail = function () {
                if (jq("#emailOperation_email").val() != userEmail) {
                    var sendButton = jq("#btEmailOperationSend");
                    sendButton.removeClass("disable");
                    if (getKeyCode(key) == 13) {
                        sendButton.click();
                    }
                } else {
                    jq("#btEmailOperationSend").addClass("disable");
                }
            };
            if (key.type != "keyup") {
                setTimeout(checkEmail, 1);
                return true;
            }
            checkEmail();
            return false;
        });
        
        var getKeyCode = function (key) {
            return key.keyCode || key.which;
        };
    };

    function showResendInviteWindow (userEmail, userID, adminMode, responseAction) {
        jq("#divEmailOperationError").html("").hide();
        jq("#studio_emailOperationResult").addClass("display-none");

        if (adminMode == true) {
            jq("#emailInputContainer").removeClass("display-none");
            jq("#emailMessageContainer").addClass("display-none");
        } else {
            jq("#emailInputContainer").addClass("display-none");
            jq("#emailMessageContainer").removeClass("display-none");

            jq("#emailActivationText").addClass("display-none");
            jq("#emailChangeText").addClass("display-none");
            jq("#resendInviteText").removeClass("display-none");

            jq("#emailMessageContainer [name='userEmail']").attr("href", "../../addons/mail/#composeto/email=" + userEmail).html(userEmail);
        }

        jq("#emailActivationDialogPopupHeader").addClass("display-none");
        jq("#emailChangeDialogPopupHeader").addClass("display-none");
        jq("#resendInviteDialogPopupHeader").removeClass("display-none");

        jq("#studio_emailOperationContent").removeClass("display-none");

        jq("#emailChangeDialogText").addClass("display-none");
        jq("#emailActivationDialogText").addClass("display-none");
        jq("#resendInviteDialogText").removeClass("display-none");

        jq("#emailOperation_email").val(userEmail);
        jq("#btEmailOperationSend").removeClass("disable");
        jq("#emailOperation_email").off(".emailChange");
        
        openPopupDialog();

        jq("#btEmailOperationSend").unbind("click");

        jq("#btEmailOperationSend").click(function () {
            var newEmail = jq("#emailOperation_email").val();
            sendEmailActivationInstructions(newEmail, userID, responseAction);
            return false;
        });

    };

    function openPopupDialog () {
        StudioBlockUIManager.blockUI("#studio_emailChangeDialog", 425);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq(\"#btEmailOperationSend\").click();";
    };

    function closeEmailOperationWindow () {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
        document.location.reload(true);
    };

    function sendInstructions (userID, userEmail) {
        if (EmailOperationService) {
            EmailOperationService.SendEmailActivationInstructions(userID, userEmail, function (res) {
                if (res.error != null) {
                    return false;
                }
                jq(".info-box.excl .first-step").addClass("display-none");
                jq(".info-box.excl .second-step").removeClass("display-none");
            });
        }
    };

    function closeActivateEmailPanel(btn) {
        var activateEmailPanel = jq(btn).parents(".info-box");
        Teamlab.updateEmailActivationSettings({ show: false }, function () {
            activateEmailPanel.remove();
        });
    };

    return {
        sendInstructions: sendInstructions,
        sendEmailActivationInstructions: sendEmailActivationInstructions,
        showResendInviteWindow: showResendInviteWindow,
        showEmailChangeWindow: showEmailChangeWindow,
        closeEmailOperationWindow: closeEmailOperationWindow,
        closeActivateEmailPanel: closeActivateEmailPanel
    }
})();

/**
 * LoadingBanner
 */
LoadingBanner = function () {
    var loaderCss = "",
        loaderId = "loadingBanner",
        strLoading = "Loading...",
        strDescription = "Please wait...",
        successId = "successBanner",
        strSuccess = "Successfully completed",
        timer = null;
    
    var hideMesInfoBtn = function () {
        clearTimeout(timer);
        var $infoContainer = jq("#mesInfoBtn");
        if ($infoContainer.length) {
            $infoContainer.parent().css({ "margin-top": "32px" });
            $infoContainer.remove();
        }
    };

    return {
        loaderCss: loaderCss,
        loaderId: loaderId,
        strLoading: strLoading,
        strDescription: strDescription,
        successId : successId,
        strSuccess: strSuccess,

        displayLoading: function () {
            var id = "#" + LoadingBanner.loaderId;

            if (jq(id).length != 0)
                return;

            var innerHtml = '<div id="{0}" class="loadingBanner {1}"><div class="loader-block">{2}<div>{3}</div></div></div>'
                .format(LoadingBanner.loaderId, LoadingBanner.loaderCss, LoadingBanner.strLoading, LoadingBanner.strDescription);

            jq("body").append(innerHtml).addClass("loading");

            if (jq.browser.mobile)
                jq(id).css("top", jq(window).scrollTop() + "px");
        },

        hideLoading: function () {
            jq("#" + LoadingBanner.loaderId).remove();
            jq(".loadingBannerBackDrop").remove();
            jq("body").removeClass("loading");
        },

        displaySuccess: function (){
            var id = "#" + LoadingBanner.successId;

            if (jq(id).length != 0)
                return;

            var innerHtml = '<div id="{0}" class="loadingBanner"><div class="loader-block success">{1}</div></div>'
               .format(LoadingBanner.successId, LoadingBanner.strSuccess);

            jq("body").append(innerHtml);

            if (jq.browser.mobile)
                jq(id).css("top", jq(window).scrollTop() + "px");

            setTimeout(function () { jq(id).remove(); }, 3000);
        },
        showLoaderBtn: function(block) {
            hideMesInfoBtn();

            var btnContainer = jq(block).find("[class*=\"button-container\"]");

            if (!btnContainer.length || btnContainer.find(".loader-container").length)
                return;

            jq(btnContainer).siblings(".error-popup, .success-popup").each(function() {jq(this).hide();});
            btnContainer.find(".button").addClass("disable").attr("disabled" , true);
            jq(btnContainer).append("<div class=\"loader-container\">{0}</div>".format(LoadingBanner.strLoading));

        },

        hideLoaderBtn: function (block) {
            var btnContainer = jq(block).find("[class*=\"button-container\"]");
            btnContainer.find(".loader-container").remove();
            btnContainer.find(".button").removeClass("disable").attr("disabled", false);
        },

        showMesInfoBtn: function (block, text, type) {
            hideMesInfoBtn();
            
            var $btnContainer = jq(block).find("[class*=\"button-container\"]");
            $btnContainer.append("<div id=\"mesInfoBtn\" class=\"{0}-container\"><span>{1}</span></div>".format(type, text));

            var $tmpl = jq("#mesInfoBtn");
            if ($tmpl.outerHeight() > parseInt($btnContainer.css("marginTop"))) {
                $btnContainer.css({"margin-top": $tmpl.outerHeight() + 8 + "px"});
                $tmpl.css({"top": -$tmpl.outerHeight() - 4 + "px"});
            }

            timer = setTimeout(hideMesInfoBtn, 3500);
        }
    };
}();

PopupMessanger = function () {
    return {
        MessangeShow: function(container, text, type) {
            var className = type + "-popup",
                mes = "<div class=\"" + className + "\">" + text + "</div>";

            switch (type) {
            case "error":
                jq(container).find(".success-popup").hide();
                break;
            case "success":
                jq(container).find(".error-popup").hide();
                break;
            default:
                jq(container).find(".error-popup, .success-popup").hide();
            }

            if (jq(container).find("." + className).length == 0) {
                jq(container).find("[class*=\"button-container\"]").before(mes);
                jq(container).find("." + className).show();
            } else {
                jq(container).find("." + className).html(text).show();
            }
        }
    };
}();

/************************************************/
/**
 * LeftMenuManager
 */
var LeftMenuManager = new function () {
    var menuObjs = [],
        cookiePath = "/",
        cookieKey = "/";
    var init = function (cookiePath, menuObjs) {
        LeftMenuManager.cookiePath = cookiePath;
        LeftMenuManager.cookieKey = encodeURIComponent(cookiePath);
        LeftMenuManager.menuObjs = menuObjs;
    };

    var updateCookies = function () {
        var newMenuObjStates = [];
        for (var i = 0, n = LeftMenuManager.menuObjs.length; i < n; i++) {
            newMenuObjStates.push(jq(LeftMenuManager.menuObjs[i]).hasClass("open") ? "open" : "");
        }
        jq.cookies.set(LeftMenuManager.cookieKey, jq.toJSON(newMenuObjStates), {path: LeftMenuManager.cookiePath});
    };

    var restoreLeftMenu = function () {
        var menuObjStates = jq.cookies.get(LeftMenuManager.cookieKey);

        if (menuObjStates != null) {
            if (menuObjStates.length == LeftMenuManager.menuObjs.length) {
                jq(LeftMenuManager.menuObjs).filter(".open-by-default").removeClass("open-by-default");

                for (var i = 0, n = menuObjStates.length; i < n; i++) {
                    var $menu = jq(LeftMenuManager.menuObjs[i]);
                    if ($menu.hasClass("currentCategory") && !$menu.hasClass("active") && !$menu.hasClass("open")) {
                        $menu.addClass("open");
                    }
                    if (menuObjStates[i] == "")
                        continue;
                    if (!$menu.hasClass(menuObjStates[i])) {
                        $menu.addClass(menuObjStates[i]);
                    }
                }
            }
        } else {
            for (var i = 0, n = LeftMenuManager.menuObjs.length; i < n; i++) {
                var $menu = jq(LeftMenuManager.menuObjs[i]);
                if (($menu.hasClass("currentCategory") || $menu.hasClass("open-by-default")) && !$menu.hasClass("active") && !$menu.hasClass("open")) {
                    $menu.addClass("open");
                }
            }
            jq(LeftMenuManager.menuObjs).filter(".open-by-default").removeClass("open-by-default");
        }
        updateCookies();
    };

    var bindEvents = function () {
        jq(".page-menu").on("click", ".filter a", function () {
            jq(".page-menu .active").removeClass("active");
            jq(this).parent().addClass("active");
        });

        jq(".page-menu").on("click", ".expander", function () {
            var menuItem = jq(this).closest(".menu-item");
            if (jq(menuItem).hasClass("open")) {
                jq(menuItem).removeClass("open");
            } else {
                jq(menuItem).addClass("open");
            }
            updateCookies();
        });

        var $dropDownObj = jq("#createNewButton");
        if ($dropDownObj.length == 1) {
            jq.dropdownToggle({
                switcherSelector: ".without-separator",
                dropdownID: "createNewButton",
                noActiveSwitcherSelector: ".with-separator .white-combobox",
                inPopup: true,
                addTop: 4,
                addLeft: 0
            });

            jq.dropdownToggle({
                switcherSelector: ".with-separator .white-combobox",
                dropdownID: "createNewButton",
                noActiveSwitcherSelector: ".without-separator",
                position: "absolute",
                inPopup: true,
                addTop: 3,
                addLeft: 0
            });

            jq(".menu-main-button .main-button-text").click(function (event) {
                if (!jq(this).hasClass("override")) {
                    jq(".menu-main-button .white-combobox").click();
                    event.stopPropagation();
                }
            });
        }

        jq.dropdownToggle({
            switcherSelector: "#menuOtherActionsButton",
            dropdownID: "otherActions",
            position: "absolute",
            inPopup: true,
            addTop: 4,
            addLeft: 0,
            rightPos: true,
            afterShowFunction: function (switcherObj, dropdownItem) {
                jq(window).trigger("onOpenSideNavOtherActions", switcherObj, dropdownItem);
            }
        });
    };

    return {
        init: init,
        updateCookies: updateCookies,
        restoreLeftMenu: restoreLeftMenu,
        bindEvents: bindEvents
    };
};

/**
* ScrolledGroupMenu
*/

/* Example from CRM contact list: */
//    options = {
//      menuSelector: "#contactsHeaderMenu",
//      menuAnchorSelector: "#mainSelectAll",
//      menuSpacerSelector: "#companyListBox .header-menu-spacer",
//      userFuncInTop: function() { jq("#contactsHeaderMenu .menu-action-on-top").hide(); },
//      userFuncNotInTop: function() { jq("#contactsHeaderMenu .menu-action-on-top").show(); }
//    }
//    ScrolledGroupMenu.init(options);

var ScrolledGroupMenu = new function () {

    var init = function (options) {
        jq(window).scroll(function () {
            stickMenuToTheTop(options);
        });
        jq(window).resize(function () {
            resizeContentHeaderWidth(options.menuSelector);
        });

        jq(window).bind("resizeWinTimerWithMaxDelay", function (event) {
            resizeContentHeaderWidth(options.menuSelector);
        });
    };
    var stickMenuToTheTop = function (options) {
        if (typeof(options) != "object" ||
            typeof(options.menuSelector) == "undefined" ||
            typeof(options.menuAnchorSelector) == "undefined" ||
            typeof(options.menuSpacerSelector) == "undefined") {
            return;
        }

        var $menuObj = jq(options.menuSelector),
            $boxTop = jq(options.menuAnchorSelector),
            $menuSpacerObj = jq(options.menuSpacerSelector),
            $groupActionsObj = jq(".studio-action-panel.group-actions"),
            $otherActionsObj = jq(".studio-action-panel.other-actions"),
            $otherActionsBtn = $menuObj.find(".otherFunctions");

        if ($menuObj.length == 0 || $boxTop.length == 0 || $menuSpacerObj.length == 0) {
            return;
        }

        var winScrollTop = jq(window).scrollTop();
        var tempTop = 0;

        if ($menuSpacerObj.css("display") == "none") {
            tempTop += $menuObj.offset().top;
        } else {
            tempTop += $menuSpacerObj.offset().top;
        }

        if (winScrollTop >= tempTop && !(winScrollTop == 0 && tempTop == 0)) {
            $menuSpacerObj.show();

            fixContentHeaderWidth(jq(options.menuSelector));

            if (typeof(options.userFuncNotInTop) == "function") {
                options.userFuncNotInTop();
            }

            $menuObj.css(
                {
                    "top": "0px",
                    "position": "fixed",
                    "paddingTop": "8px"
                });

            $groupActionsObj.css(
                {
                    "top": jq(document).scrollTop() + $menuObj.outerHeight() - 4 + "px",
                    "position": "absolute"
                });

            $menuObj.offset({ left: $menuObj.parent().offset().left });

            if ($otherActionsObj.length && $otherActionsBtn.length) {
                $otherActionsObj.css(
                    {
                        top: $otherActionsBtn.offset().top - jq(document).scrollTop() + $otherActionsBtn.outerHeight() + 4,
                        position: "fixed",
                        left: $otherActionsBtn.offset().left - jq(document).scrollLeft() + $otherActionsBtn.outerWidth() - $otherActionsObj.outerWidth()
                    });
            }
        } else {
            $menuSpacerObj.hide();
            $menuObj.css("width", "auto");

            if (typeof(options.userFuncInTop) == "function") {
                options.userFuncInTop();
            }

            $menuObj.css(
                {
                    "position": "static",
                    "paddingTop": 0
                });

            $groupActionsObj.css(
                {
                    "top": $menuObj.offset().top + $menuObj.outerHeight() - 4 + "px"
                });

            if ($otherActionsObj.length && $otherActionsBtn.length) {
                $otherActionsObj.css(
                    {
                        top: $otherActionsBtn.offset().top + $otherActionsBtn.outerHeight() + 4,
                        position: "absolute",
                        left: $otherActionsBtn.offset().left + $otherActionsBtn.outerWidth() - $otherActionsObj.outerWidth()
                    });
            }
        }
    };

    var fixContentHeaderWidth = function (header) {
        var maxHeaderWidth = parseInt(jq(header).css("max-width")),
            headerWidth = jq(header).parent().innerWidth();

        if (maxHeaderWidth != 0 && headerWidth > maxHeaderWidth) {
            headerWidth = maxHeaderWidth;
        }

        jq(header).css("width",
                headerWidth
                - parseInt(jq(header).css("margin-left"))
                - parseInt(jq(header).css("margin-right"))
                - parseInt(jq(header).css("padding-left"))
                - parseInt(jq(header).css("padding-right")));
    };

    var resizeContentHeaderWidth = function (header) {
        jq(header).css("width", "");
        fixContentHeaderWidth(header);
    };

    return {
        init: init,
        stickMenuToTheTop: stickMenuToTheTop,
        fixContentHeaderWidth: fixContentHeaderWidth,
        resizeContentHeaderWidth: resizeContentHeaderWidth
    };
};

/**
 * File Size Manager
 */
var FileSizeManager = new function () {
    var filesSizeToString = function (size) {
        var sizeNames = ASC.Resources.Master.FileSizePostfix ? ASC.Resources.Master.FileSizePostfix.split(',') : ["bytes", "KB", "MB", "GB", "TB"];
        var power = 0;

        var resultSize = size || 0;
        if (1024 <= resultSize) {
            power = parseInt(Math.log(resultSize) / Math.log(1024));
            power = power < sizeNames.length ? power : sizeNames.length - 1;
            resultSize = resultSize / Math.pow(1024, power);
        }

        var intRegex = /^\d+$/;
        if (intRegex.test(resultSize)) {
            resultSize = parseInt(resultSize);
        } else {
            resultSize = parseFloat(resultSize.toFixed(2));
        }

        return jq.format("{0} {1}", resultSize, sizeNames[power]);
    };

    return {
        filesSizeToString: filesSizeToString
    };
};

var htmlUtility = new function () {
    return {
        getFull: function (text) {
            var doc = jq(document.createElement("div"));
            doc.html(text);

            doc.find("script").remove();

            doc.find("*").each(function () {
                var elem = this;
                jq.each(this.attributes, function () {
                    var attr = this.name;
                    var value = this.value;
                    if (attr.indexOf("on") == 0
                        || (typeof value == "string"
                            && (value.trim().indexOf("javascript") == 0
                                || value.trim().indexOf("data") == 0
                                || value.trim().indexOf("vbscript") == 0))) {
                        jq(elem).removeAttr(attr);
                    }
                });
            });

            return doc.html();
        }
    };
};

/**
 * Encoder
 */
Encoder = { EncodeType: "entity", isEmpty: function(val) { if (val) { return ((val === null) || val.length == 0 || /^\s+$/.test(val)); } else { return true; } }, HTML2Numerical: function(s) { var arr1 = new Array('&nbsp;', '&iexcl;', '&cent;', '&pound;', '&curren;', '&yen;', '&brvbar;', '&sect;', '&uml;', '&copy;', '&ordf;', '&laquo;', '&not;', '&shy;', '&reg;', '&macr;', '&deg;', '&plusmn;', '&sup2;', '&sup3;', '&acute;', '&micro;', '&para;', '&middot;', '&cedil;', '&sup1;', '&ordm;', '&raquo;', '&frac14;', '&frac12;', '&frac34;', '&iquest;', '&agrave;', '&aacute;', '&acirc;', '&atilde;', '&Auml;', '&aring;', '&aelig;', '&ccedil;', '&egrave;', '&eacute;', '&ecirc;', '&euml;', '&igrave;', '&iacute;', '&icirc;', '&iuml;', '&eth;', '&ntilde;', '&ograve;', '&oacute;', '&ocirc;', '&otilde;', '&Ouml;', '&times;', '&oslash;', '&ugrave;', '&uacute;', '&ucirc;', '&Uuml;', '&yacute;', '&thorn;', '&szlig;', '&agrave;', '&aacute;', '&acirc;', '&atilde;', '&auml;', '&aring;', '&aelig;', '&ccedil;', '&egrave;', '&eacute;', '&ecirc;', '&euml;', '&igrave;', '&iacute;', '&icirc;', '&iuml;', '&eth;', '&ntilde;', '&ograve;', '&oacute;', '&ocirc;', '&otilde;', '&ouml;', '&divide;', '&Oslash;', '&ugrave;', '&uacute;', '&ucirc;', '&uuml;', '&yacute;', '&thorn;', '&yuml;', '&quot;', '&amp;', '&lt;', '&gt;', '&oelig;', '&oelig;', '&scaron;', '&scaron;', '&yuml;', '&circ;', '&tilde;', '&ensp;', '&emsp;', '&thinsp;', '&zwnj;', '&zwj;', '&lrm;', '&rlm;', '&ndash;', '&mdash;', '&lsquo;', '&rsquo;', '&sbquo;', '&ldquo;', '&rdquo;', '&bdquo;', '&dagger;', '&dagger;', '&permil;', '&lsaquo;', '&rsaquo;', '&euro;', '&fnof;', '&alpha;', '&beta;', '&gamma;', '&delta;', '&epsilon;', '&zeta;', '&eta;', '&theta;', '&iota;', '&kappa;', '&lambda;', '&mu;', '&nu;', '&xi;', '&omicron;', '&pi;', '&rho;', '&sigma;', '&tau;', '&upsilon;', '&phi;', '&chi;', '&psi;', '&omega;', '&alpha;', '&beta;', '&gamma;', '&delta;', '&epsilon;', '&zeta;', '&eta;', '&theta;', '&iota;', '&kappa;', '&lambda;', '&mu;', '&nu;', '&xi;', '&omicron;', '&pi;', '&rho;', '&sigmaf;', '&sigma;', '&tau;', '&upsilon;', '&phi;', '&chi;', '&psi;', '&omega;', '&thetasym;', '&upsih;', '&piv;', '&bull;', '&hellip;', '&prime;', '&prime;', '&oline;', '&frasl;', '&weierp;', '&image;', '&real;', '&trade;', '&alefsym;', '&larr;', '&uarr;', '&rarr;', '&darr;', '&harr;', '&crarr;', '&larr;', '&uarr;', '&rarr;', '&darr;', '&harr;', '&forall;', '&part;', '&exist;', '&empty;', '&nabla;', '&isin;', '&notin;', '&ni;', '&prod;', '&sum;', '&minus;', '&lowast;', '&radic;', '&prop;', '&infin;', '&ang;', '&and;', '&or;', '&cap;', '&cup;', '&int;', '&there4;', '&sim;', '&cong;', '&asymp;', '&ne;', '&equiv;', '&le;', '&ge;', '&sub;', '&sup;', '&nsub;', '&sube;', '&supe;', '&oplus;', '&otimes;', '&perp;', '&sdot;', '&lceil;', '&rceil;', '&lfloor;', '&rfloor;', '&lang;', '&rang;', '&loz;', '&spades;', '&clubs;', '&hearts;', '&diams;'); var arr2 = new Array('&#160;', '&#161;', '&#162;', '&#163;', '&#164;', '&#165;', '&#166;', '&#167;', '&#168;', '&#169;', '&#170;', '&#171;', '&#172;', '&#173;', '&#174;', '&#175;', '&#176;', '&#177;', '&#178;', '&#179;', '&#180;', '&#181;', '&#182;', '&#183;', '&#184;', '&#185;', '&#186;', '&#187;', '&#188;', '&#189;', '&#190;', '&#191;', '&#192;', '&#193;', '&#194;', '&#195;', '&#196;', '&#197;', '&#198;', '&#199;', '&#200;', '&#201;', '&#202;', '&#203;', '&#204;', '&#205;', '&#206;', '&#207;', '&#208;', '&#209;', '&#210;', '&#211;', '&#212;', '&#213;', '&#214;', '&#215;', '&#216;', '&#217;', '&#218;', '&#219;', '&#220;', '&#221;', '&#222;', '&#223;', '&#224;', '&#225;', '&#226;', '&#227;', '&#228;', '&#229;', '&#230;', '&#231;', '&#232;', '&#233;', '&#234;', '&#235;', '&#236;', '&#237;', '&#238;', '&#239;', '&#240;', '&#241;', '&#242;', '&#243;', '&#244;', '&#245;', '&#246;', '&#247;', '&#248;', '&#249;', '&#250;', '&#251;', '&#252;', '&#253;', '&#254;', '&#255;', '&#34;', '&#38;', '&#60;', '&#62;', '&#338;', '&#339;', '&#352;', '&#353;', '&#376;', '&#710;', '&#732;', '&#8194;', '&#8195;', '&#8201;', '&#8204;', '&#8205;', '&#8206;', '&#8207;', '&#8211;', '&#8212;', '&#8216;', '&#8217;', '&#8218;', '&#8220;', '&#8221;', '&#8222;', '&#8224;', '&#8225;', '&#8240;', '&#8249;', '&#8250;', '&#8364;', '&#402;', '&#913;', '&#914;', '&#915;', '&#916;', '&#917;', '&#918;', '&#919;', '&#920;', '&#921;', '&#922;', '&#923;', '&#924;', '&#925;', '&#926;', '&#927;', '&#928;', '&#929;', '&#931;', '&#932;', '&#933;', '&#934;', '&#935;', '&#936;', '&#937;', '&#945;', '&#946;', '&#947;', '&#948;', '&#949;', '&#950;', '&#951;', '&#952;', '&#953;', '&#954;', '&#955;', '&#956;', '&#957;', '&#958;', '&#959;', '&#960;', '&#961;', '&#962;', '&#963;', '&#964;', '&#965;', '&#966;', '&#967;', '&#968;', '&#969;', '&#977;', '&#978;', '&#982;', '&#8226;', '&#8230;', '&#8242;', '&#8243;', '&#8254;', '&#8260;', '&#8472;', '&#8465;', '&#8476;', '&#8482;', '&#8501;', '&#8592;', '&#8593;', '&#8594;', '&#8595;', '&#8596;', '&#8629;', '&#8656;', '&#8657;', '&#8658;', '&#8659;', '&#8660;', '&#8704;', '&#8706;', '&#8707;', '&#8709;', '&#8711;', '&#8712;', '&#8713;', '&#8715;', '&#8719;', '&#8721;', '&#8722;', '&#8727;', '&#8730;', '&#8733;', '&#8734;', '&#8736;', '&#8743;', '&#8744;', '&#8745;', '&#8746;', '&#8747;', '&#8756;', '&#8764;', '&#8773;', '&#8776;', '&#8800;', '&#8801;', '&#8804;', '&#8805;', '&#8834;', '&#8835;', '&#8836;', '&#8838;', '&#8839;', '&#8853;', '&#8855;', '&#8869;', '&#8901;', '&#8968;', '&#8969;', '&#8970;', '&#8971;', '&#9001;', '&#9002;', '&#9674;', '&#9824;', '&#9827;', '&#9829;', '&#9830;'); return this.swapArrayVals(s, arr1, arr2); }, NumericalToHTML: function(s) { var arr1 = new Array('&#160;', '&#161;', '&#162;', '&#163;', '&#164;', '&#165;', '&#166;', '&#167;', '&#168;', '&#169;', '&#170;', '&#171;', '&#172;', '&#173;', '&#174;', '&#175;', '&#176;', '&#177;', '&#178;', '&#179;', '&#180;', '&#181;', '&#182;', '&#183;', '&#184;', '&#185;', '&#186;', '&#187;', '&#188;', '&#189;', '&#190;', '&#191;', '&#192;', '&#193;', '&#194;', '&#195;', '&#196;', '&#197;', '&#198;', '&#199;', '&#200;', '&#201;', '&#202;', '&#203;', '&#204;', '&#205;', '&#206;', '&#207;', '&#208;', '&#209;', '&#210;', '&#211;', '&#212;', '&#213;', '&#214;', '&#215;', '&#216;', '&#217;', '&#218;', '&#219;', '&#220;', '&#221;', '&#222;', '&#223;', '&#224;', '&#225;', '&#226;', '&#227;', '&#228;', '&#229;', '&#230;', '&#231;', '&#232;', '&#233;', '&#234;', '&#235;', '&#236;', '&#237;', '&#238;', '&#239;', '&#240;', '&#241;', '&#242;', '&#243;', '&#244;', '&#245;', '&#246;', '&#247;', '&#248;', '&#249;', '&#250;', '&#251;', '&#252;', '&#253;', '&#254;', '&#255;', '&#34;', '&#38;', '&#60;', '&#62;', '&#338;', '&#339;', '&#352;', '&#353;', '&#376;', '&#710;', '&#732;', '&#8194;', '&#8195;', '&#8201;', '&#8204;', '&#8205;', '&#8206;', '&#8207;', '&#8211;', '&#8212;', '&#8216;', '&#8217;', '&#8218;', '&#8220;', '&#8221;', '&#8222;', '&#8224;', '&#8225;', '&#8240;', '&#8249;', '&#8250;', '&#8364;', '&#402;', '&#913;', '&#914;', '&#915;', '&#916;', '&#917;', '&#918;', '&#919;', '&#920;', '&#921;', '&#922;', '&#923;', '&#924;', '&#925;', '&#926;', '&#927;', '&#928;', '&#929;', '&#931;', '&#932;', '&#933;', '&#934;', '&#935;', '&#936;', '&#937;', '&#945;', '&#946;', '&#947;', '&#948;', '&#949;', '&#950;', '&#951;', '&#952;', '&#953;', '&#954;', '&#955;', '&#956;', '&#957;', '&#958;', '&#959;', '&#960;', '&#961;', '&#962;', '&#963;', '&#964;', '&#965;', '&#966;', '&#967;', '&#968;', '&#969;', '&#977;', '&#978;', '&#982;', '&#8226;', '&#8230;', '&#8242;', '&#8243;', '&#8254;', '&#8260;', '&#8472;', '&#8465;', '&#8476;', '&#8482;', '&#8501;', '&#8592;', '&#8593;', '&#8594;', '&#8595;', '&#8596;', '&#8629;', '&#8656;', '&#8657;', '&#8658;', '&#8659;', '&#8660;', '&#8704;', '&#8706;', '&#8707;', '&#8709;', '&#8711;', '&#8712;', '&#8713;', '&#8715;', '&#8719;', '&#8721;', '&#8722;', '&#8727;', '&#8730;', '&#8733;', '&#8734;', '&#8736;', '&#8743;', '&#8744;', '&#8745;', '&#8746;', '&#8747;', '&#8756;', '&#8764;', '&#8773;', '&#8776;', '&#8800;', '&#8801;', '&#8804;', '&#8805;', '&#8834;', '&#8835;', '&#8836;', '&#8838;', '&#8839;', '&#8853;', '&#8855;', '&#8869;', '&#8901;', '&#8968;', '&#8969;', '&#8970;', '&#8971;', '&#9001;', '&#9002;', '&#9674;', '&#9824;', '&#9827;', '&#9829;', '&#9830;'); var arr2 = new Array('&nbsp;', '&iexcl;', '&cent;', '&pound;', '&curren;', '&yen;', '&brvbar;', '&sect;', '&uml;', '&copy;', '&ordf;', '&laquo;', '&not;', '&shy;', '&reg;', '&macr;', '&deg;', '&plusmn;', '&sup2;', '&sup3;', '&acute;', '&micro;', '&para;', '&middot;', '&cedil;', '&sup1;', '&ordm;', '&raquo;', '&frac14;', '&frac12;', '&frac34;', '&iquest;', '&agrave;', '&aacute;', '&acirc;', '&atilde;', '&Auml;', '&aring;', '&aelig;', '&ccedil;', '&egrave;', '&eacute;', '&ecirc;', '&euml;', '&igrave;', '&iacute;', '&icirc;', '&iuml;', '&eth;', '&ntilde;', '&ograve;', '&oacute;', '&ocirc;', '&otilde;', '&Ouml;', '&times;', '&oslash;', '&ugrave;', '&uacute;', '&ucirc;', '&Uuml;', '&yacute;', '&thorn;', '&szlig;', '&agrave;', '&aacute;', '&acirc;', '&atilde;', '&auml;', '&aring;', '&aelig;', '&ccedil;', '&egrave;', '&eacute;', '&ecirc;', '&euml;', '&igrave;', '&iacute;', '&icirc;', '&iuml;', '&eth;', '&ntilde;', '&ograve;', '&oacute;', '&ocirc;', '&otilde;', '&ouml;', '&divide;', '&Oslash;', '&ugrave;', '&uacute;', '&ucirc;', '&uuml;', '&yacute;', '&thorn;', '&yuml;', '&quot;', '&amp;', '&lt;', '&gt;', '&oelig;', '&oelig;', '&scaron;', '&scaron;', '&yuml;', '&circ;', '&tilde;', '&ensp;', '&emsp;', '&thinsp;', '&zwnj;', '&zwj;', '&lrm;', '&rlm;', '&ndash;', '&mdash;', '&lsquo;', '&rsquo;', '&sbquo;', '&ldquo;', '&rdquo;', '&bdquo;', '&dagger;', '&dagger;', '&permil;', '&lsaquo;', '&rsaquo;', '&euro;', '&fnof;', '&alpha;', '&beta;', '&gamma;', '&delta;', '&epsilon;', '&zeta;', '&eta;', '&theta;', '&iota;', '&kappa;', '&lambda;', '&mu;', '&nu;', '&xi;', '&omicron;', '&pi;', '&rho;', '&sigma;', '&tau;', '&upsilon;', '&phi;', '&chi;', '&psi;', '&omega;', '&alpha;', '&beta;', '&gamma;', '&delta;', '&epsilon;', '&zeta;', '&eta;', '&theta;', '&iota;', '&kappa;', '&lambda;', '&mu;', '&nu;', '&xi;', '&omicron;', '&pi;', '&rho;', '&sigmaf;', '&sigma;', '&tau;', '&upsilon;', '&phi;', '&chi;', '&psi;', '&omega;', '&thetasym;', '&upsih;', '&piv;', '&bull;', '&hellip;', '&prime;', '&prime;', '&oline;', '&frasl;', '&weierp;', '&image;', '&real;', '&trade;', '&alefsym;', '&larr;', '&uarr;', '&rarr;', '&darr;', '&harr;', '&crarr;', '&larr;', '&uarr;', '&rarr;', '&darr;', '&harr;', '&forall;', '&part;', '&exist;', '&empty;', '&nabla;', '&isin;', '&notin;', '&ni;', '&prod;', '&sum;', '&minus;', '&lowast;', '&radic;', '&prop;', '&infin;', '&ang;', '&and;', '&or;', '&cap;', '&cup;', '&int;', '&there4;', '&sim;', '&cong;', '&asymp;', '&ne;', '&equiv;', '&le;', '&ge;', '&sub;', '&sup;', '&nsub;', '&sube;', '&supe;', '&oplus;', '&otimes;', '&perp;', '&sdot;', '&lceil;', '&rceil;', '&lfloor;', '&rfloor;', '&lang;', '&rang;', '&loz;', '&spades;', '&clubs;', '&hearts;', '&diams;'); return this.swapArrayVals(s, arr1, arr2); }, numEncode: function(s) { if (this.isEmpty(s)) return ""; var e = ""; for (var i = 0; i < s.length; i++) { var c = s.charAt(i); if (c < " " || c > "~") { c = "&#" + c.charCodeAt() + ";"; } e += c; } return e; }, htmlDecode: function(s) { var c, m, d = s; if (this.isEmpty(d)) return ""; d = this.HTML2Numerical(d); arr = d.match(/&#[0-9]{1,5};/g); if (arr != null) { for (var x = 0; x < arr.length; x++) { m = arr[x]; c = m.substring(2, m.length - 1); if (c >= -32768 && c <= 65535) { d = d.replace(m, String.fromCharCode(c)); } else { d = d.replace(m, ""); } } } return d; }, htmlEncode: function(s, dbl) { if (this.isEmpty(s)) return ""; dbl = dbl | false; if (dbl) { if (this.EncodeType == "numerical") { s = s.replace(/&/g, "&#38;"); } else { s = s.replace(/&/g, "&amp;"); } } s = this.XSSEncode(s, false); if (this.EncodeType == "numerical" || !dbl) { s = this.HTML2Numerical(s); } s = this.numEncode(s); if (!dbl) { s = s.replace(/&#/g, "##AMPHASH##"); if (this.EncodeType == "numerical") { s = s.replace(/&/g, "&#38;"); } else { s = s.replace(/&/g, "&amp;"); } s = s.replace(/##AMPHASH##/g, "&#"); } s = s.replace(/&#\d*([^\d;]|$)/g, "$1"); if (!dbl) { s = this.correctEncoding(s); } if (this.EncodeType == "entity") { s = this.NumericalToHTML(s); } return s; }, XSSEncode: function(s, en) { if (!this.isEmpty(s)) { en = en || true; if (en) { s = s.replace(/\'/g, "&#39;"); s = s.replace(/\"/g, "&quot;"); s = s.replace(/</g, "&lt;"); s = s.replace(/>/g, "&gt;"); } else { s = s.replace(/\'/g, "&#39;"); s = s.replace(/\"/g, "&#34;"); s = s.replace(/</g, "&#60;"); s = s.replace(/>/g, "&#62;"); } return s; } else { return ""; } }, hasEncoded: function(s) { if (/&#[0-9]{1,5};/g.test(s)) { return true; } else if (/&[A-Z]{2,6};/gi.test(s)) { return true; } else { return false; } }, stripUnicode: function(s) { return s.replace(/[^\x20-\x7E]/g, ""); }, correctEncoding: function(s) { return s.replace(/(&amp;)(amp;)+/, "$1"); }, swapArrayVals: function(s, arr1, arr2) { if (this.isEmpty(s)) return ""; var re; if (arr1 && arr2) { if (arr1.length == arr2.length) { for (var x = 0, i = arr1.length; x < i; x++) { re = new RegExp(arr1[x], 'g'); s = s.replace(re, arr2[x]); } } } return s; }, inArray: function(item, arr) { for (var i = 0, x = arr.length; i < x; i++) { if (arr[i] === item) { return i; } } return -1; } }
less = {}; less.env = 'development';

/**
 * UserManager
 */
window.UserManager = new function() {
    var usersCache = null;
    var usersDisabledCache = null;
    var personCache = [];

    function init() {
        if (usersCache != null)
            return;

        var master = ASC.Resources.Master;
        usersCache = {};

        var activeUsers = master.ApiResponses_ActiveProfiles.response;
        var activeUsersLength = activeUsers.length;
        for (var i = 0; i < activeUsersLength; i++) {
            var activeUserItem = activeUsers[i];
            usersCache[activeUserItem.id] = activeUserItem;
        }

        delete master.ApiResponses_ActiveProfiles;

        usersDisabledCache = {};
        var disabledUsers = master.ApiResponses_DisabledProfiles.response;
        var disabledUsersLength = disabledUsers.length;
        for (var j = 0; j < disabledUsersLength; j++) {
            var disabledUserItem = disabledUsers[j];
            usersDisabledCache[disabledUserItem.id] = disabledUserItem;
        }

        delete master.ApiResponses_DisabledProfiles;
    }

    function getAllUsers(activeOnly) {
        init();
        return activeOnly ? usersCache : jq.extend({}, usersCache, usersDisabledCache);
    }

    function getUser(userId) {
        if (!userId)
            return null;

        init();

        if (usersCache[userId]) return usersCache[userId];

        if (usersDisabledCache[userId]) return usersDisabledCache[userId];

        return null;
    }
    
    function getPerson(id, personConstructor) {
        if (!id)
            return null;

        var result = personCache[id];

        if (result) return result;

        var user = getUser(id);
        if (!user) {
            user = getRemovedProfile();
        }

        result = personConstructor(user);
        personCache[id] = result;

        return result;
    }

    function getUsers(ids) {
        if (!ids || !ids.length)
            return [];

        init();

        var result = [];

        for (var userId in usersCache) {
            if (usersCache.hasOwnProperty(userId) && ~ids.indexOf(userId)) {
                result.push(usersCache[userId]);
            }
        }

        for (var disabledUserId in usersDisabledCache) {
            if (usersDisabledCache.hasOwnProperty(disabledUserId) && ~ids.indexOf(disabledUserId)) {
                result.push(usersDisabledCache[disabledUserId]);
            }
        }

        return result;
    }

    function getRemovedProfile() {
        return ASC.Resources.Master.ApiResponsesRemovedProfile.response;
    }

    function addNewUser(newUser) {
        usersCache[newUser.id] = newUser;
    }

    return {
        getAllUsers: getAllUsers,
        getUser: getUser,
        getPerson: getPerson,
        getUsers: getUsers,
        getRemovedProfile: getRemovedProfile,
        addNewUser: addNewUser
    };
};

/**
 * GroupManager
 */
window.GroupManager = new function () {
    var groups = null;
    var groupItems = null;
    var groupsCache = [];

    function comparer (a, b) {
        var compA = a.name.toLowerCase(),
            compB = b.name.toLowerCase();
        return (compA < compB) ? -1 : (compA > compB) ? 1 : 0;
    }

    function init() {
        if (groups != null)
            return;

        groups = ASC.Resources.Master.ApiResponses_Groups.response.sort(comparer);
    }

    function initGroupItems() {
        if (groupItems != null)
            return;

        init();

        groupItems = {};

        var i, j, k, n, groupId;

        for (i = 0, j = groups.length; i < j; i++) {
            groupId = groups[i].id;
            groupItems[groupId] = [];
        }

        var users = window.UserManager.getAllUsers();

        for (var userId in users) {
            if (!users.hasOwnProperty(userId)) continue;

            var user = users[userId];
            for (j = 0, k = user.groups.length; j < k; j++) {
                groupId = user.groups[j];
                groupItems[groupId] ? groupItems[groupId].push(userId) : groupItems[groupId] = [userId];
            }
        }
    }

    function getAllGroups() {
        init();
        return groups;
    }

    function getGroup(groupId) {
        if (!groupId)
            return null;

        var fromCache = groupsCache[groupId];

        if (fromCache) return fromCache;

        init();

        for (var i = 0, j = groups.length; i < j; i++) {
            var groupItem = groups[i];
            if (groupItem.id === groupId) {
                groupsCache[groupId] = groupItem;
                return groupItem;
            }
        }

        return null;
    }

    function getGroups(ids) {
        if (!ids || !ids.length)
            return [];

        init();

        var result = [];

        for (var i = 0; i < groups.length; i++)
            if (~ids.indexOf(groups[i].id))
                result.push(groups[i]);

        return result;
    }

    function getGroupItems(groupId) {
        if (!groupId)
            return null;

        initGroupItems();

        return groupItems[groupId];
    }

    return {
        getAllGroups: getAllGroups,
        getGroup: getGroup,
        getGroups: getGroups,
        getGroupItems: getGroupItems
    };
};

/*
 * doPostback
 */
window.submitForm = function (eventTarget, eventArgument) {
    var form = document.forms["aspnetForm"] || document.forms[0];

    if (!form) return;

    if (!form.__EVENTTARGET) {
        var target = document.createElement("input");
        target.setAttribute("id", "__EVENTTARGET");
        target.setAttribute("type", "hidden");
        target.setAttribute("value", "");
        target.setAttribute("name", "__EVENTTARGET");
        form.appendChild(target);
    }

    if (eventTarget) {
        form.__EVENTTARGET.value = eventTarget;
    }

    if (!form.__EVENTARGUMENT) {
        var argument = document.createElement("input");
        argument.setAttribute("id", "__EVENTARGUMENT");
        argument.setAttribute("type", "hidden");
        argument.setAttribute("value", "");
        argument.setAttribute("name", "__EVENTARGUMENT");
        form.appendChild(argument);
    }

    if (eventArgument) {
        form.__EVENTARGUMENT.value = eventArgument;
    }

    if (ASC.Desktop && ASC.Desktop.checkpwd) {
        ASC.Desktop.checkpwd();
    }

    form.submit();
};

window.hashPassword = function (password, callback) {
    var size = ASC.Resources.Master.PasswordHashSize;
    var iterations = ASC.Resources.Master.PasswordHashIterations;
    var salt = ASC.Resources.Master.PasswordHashSalt;

    var bits = sjcl.misc.pbkdf2(password, salt, iterations);
    bits = bits.slice(0, size / 32);
    var hash = sjcl.codec.hex.fromBits(bits);

    callback(hash);
};

window.TipsManager = new function() {
    function neverShowTips() {
        Teamlab.updateTipsSettings({ show: false });
    }

    return {
        neverShowTips: neverShowTips
    };
};