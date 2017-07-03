/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


if (typeof jq == "undefined")
    var jq = jQuery.noConflict();

toastr.options.hideDuration = 100;

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

// google analitics track
var trackingGoogleAnalitics = function (ctg, act, lbl) {
    try {
        if (window.ga) {
            window.ga('send', 'event', ctg, act, lbl);
        }
    } catch (err) {
    }
};

jQuery.fn.trackEvent = function (category, action, label, typeAction) { // only for static objects (don't use for elements added dynamically)

    switch (typeAction) {
        case "click":
            jq(this).on("click", function () {
                trackingGoogleAnalitics(category, action, label);
                return true;
            });
            break;
        case "enter":
            jq(this).keypress(function (e) {
                if (e.which == 13) {
                    trackingGoogleAnalitics(category, action, label);
                }
                return true;
            });
            break;
        default:
            jq(this).on("click", function () {
                trackingGoogleAnalitics(category, action, label);
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
            return '<a target="_new" href="' + (/^http/.test(str) ? str : 'http://' + str) + '">' + str + '</a>';
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

        var dateFormat = Teamlab.constants.dateFormats.date;
        var separator = "/";
        var dateFormatComponent = dateFormat.split(separator);
        if (dateFormatComponent.length == 1) {
            separator = ".";
            dateFormatComponent = dateFormat.split(separator);
            if (dateFormatComponent.length == 1) {
                separator = "-";
                dateFormatComponent = dateFormat.split(separator);
                if (dateFormatComponent.length == 1) {
                    separator = ". ";       // for czech language
                    dateFormatComponent = dateFormat.split(separator);
                    if (dateFormatComponent.length == 1) {
                        return "Unknown format date";
                    }
                }
            }
        }

        // split input date to month, day and year
        aoDate = txtDate.split(separator);
        // array length should be exactly 3 (no more no less)
        // the second condition for format dd.mm.yyyy. (for example in Latvia)
        if (aoDate.length !== 3 && txtDate.charAt(txtDate.length - 1) !== separator) {
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
        var reg = new RegExp(ASC.Resources.Master.EmailRegExpr, "i");
        if (reg.test(email) == true) {
            return true;
        }
        return false;
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

    showDropDownByContext: function (evt, target, dropdownItem) {
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

            dropdownItem.css({
                "top": baseTop - correctionY + (correctionY == 0 ? 2 : -target.outerHeight() - 2),
                "left": position.left + target.outerWidth() - ddiWidth + 10,
                "right": "auto"
            });
        } else {
            var correctionX = document.body.clientWidth - (evt.pageX - pageXOffset + ddiWidth) > 0 ? 0 : ddiWidth;
            correctionY =
                ddiHeight > evt.pageY
                    ? 0
                    : (scrHeight + scrScrollTop - evt.pageY > ddiHeight ? 0 : ddiHeight);

            dropdownItem.css(
                {
                    "top": evt.pageY - correctionY,
                    "left": evt.pageX - correctionX,
                    "right": "auto",
                    "margin": "0"
                });
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
    blockUI: function (obj, width, height, top, position, opts) {
        try {
            width = width | 0;
            height = height | 0;
            left = width > 0 ? (-width / 2 | 0) : -200;
            top = ((top || -height / 2) | 0) + (position ? jq(window).scrollTop() : 0);
            top = -Math.min(-top, jq(window).height() / 2);
            position = position ? position : "fixed";
            opts = opts || {};

            var defaultOptions = {
                message: jq(obj),
                css: {
                    backgroundColor: "transparent",
                    border: "none",
                    cursor: "default",
                    height: height > 0 ? height + "px" : "auto",
                    left: "50%",
                    marginLeft: left + "px",
                    marginTop: top + "px",
                    opacity: "1",
                    overflow: "visible",
                    padding: "0px",
                    position: position,
                    textAlign: "left",
                    top: "50%",
                    width: width > 0 ? width + "px" : "auto",
                },

                overlayCSS: {
                    backgroundColor: "#aaa",
                    cursor: "default",
                    opacity: "0.3"
                },

                focusInput: true,
                baseZ: 666,

                fadeIn: 0,
                fadeOut: 0
            };

            jq.blockUI(jq.extend(true, defaultOptions, opts));
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
        if (!jq('.popupContainerClass').is(':visible'))
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
            if (e.target.nodeName.toLowerCase() !== 'textarea' && PopupKeyUpActionProvider.EnterAction != null && PopupKeyUpActionProvider.EnterAction != '')
                eval(PopupKeyUpActionProvider.EnterAction);
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

    this.ClearActions = function () {
        this.CloseDialogAction = '';
        this.EnterAction = '';
        this.EnterActionCallback = '';
        this.CtrlEnterAction = '';
        this.EnableEsc = true;
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

    this.getLocationPathToModule = function (moduleName) { // example path = jq.getLocationPathToModule("projects") - http://localhost/asc/products/projects/
        var products = "products";
        var mass = location.href.toLowerCase().split(products);
        var path = mass[0] + products + "/" + moduleName + "/";
        return path;
    };

    this.getBasePathToModule = function () {
        var products = "products",
            addons = "addons",
            mass,
            parts,
            moduleName = "",
            path = "/";

        mass = location.pathname.toLowerCase().split(products);
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
        StudioBlockUIManager.blockUI("#studio_AddContentDialog", 400, 350, 0);
    };

    this.Disable = function (obj_id) {
        jq('#' + obj_id).addClass("disable");
    };

    this.Enable = function (obj_id) {
        jq('#' + obj_id).removeClass("disable");
    };

    this.initImageZoom = function (options) {
        var setting = jq.extend({
            type: 'image',
            tLoading: '', // remove text from preloader
            gallery: {
                enabled: true,
                tCounter: '%curr% / %total%',
                preload: [0,0]
            },
            image: {
                tError: jq.format(ASC.Resources.Master.Resource.MagnificImageError, '<a href="%url%">' ,'</a>') // Error message when image could not be loaded
            },
            ajax: {
                tError: jq.format(ASC.Resources.Master.Resource.MagnificContentError, '<a href="%url%">', '</a>') // Error message when resource could not be loaded
            }
        }, options);

        jq(".screenzoom").magnificPopup(setting);
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

/**
 * EventTracker
 */
var EventTracker = new function () {
    this.Track = function (event) {

        try {
            if (window.ga) {
                window.ga('send', 'pageview', event);
            }
        } catch (err) {
        }
    };
};

/*--------Error messages for required field-------*/
function ShowRequiredError (item, withouthScroll) {
    jq("div[class='infoPanel alert']").hide();
    jq("div[class='infoPanel alert']").empty();
    var parentBlock = jq(item).parents(".requiredField");
    jq(parentBlock).addClass("requiredFieldError");

    if (typeof(withouthScroll) == "undefined" || withouthScroll == false) {
        jq.scrollTo(jq(parentBlock).position().top - 50, {speed: 500});
    }
    jq(item).focus();
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
                    setTimeout(function () { document.location.reload(true); }, 3000);
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
        
        jq("#emailOperation_email").unbind("onkeyup");
        
        jq("#emailOperation_email").keyup(function (key) {
            if (jq(this).val() != userEmail) {
                var sendButton = jq("#btEmailOperationSend");
                sendButton.removeClass("disable");
                if (getKeyCode(key) == 13) {
                    sendButton.click();
                }
            } else {
                jq("#btEmailOperationSend").addClass("disable");
            }
            return false;
        });
        
        var getKeyCode = function (key) {
            return key.keyCode || key.which;
        };
    };

    function showResendInviteWindow (userEmail, userID, adminMode, responseAction) {
        jq("#divEmailOperationError").html("").hide();
        jq("#studio_emailOperationResult").hide();

        if (adminMode == true) {
            jq("#emailInputContainer").removeClass("display-none");
            jq("#emailMessageContainer").hide();
        } else {
            jq("#emailInputContainer").addClass("display-none");
            jq("#emailMessageContainer").show();

            jq("#emailActivationText").hide();
            jq("#emailChangeText").hide();
            jq("#resendInviteText").show();

            jq("#emailMessageContainer [name='userEmail']").attr("href", "../../addons/mail/#composeto/email=" + userEmail).html(userEmail);
        }

        jq("#emailActivationDialogPopupHeader").hide();
        jq("#emailChangeDialogPopupHeader").hide();
        jq("#resendInviteDialogPopupHeader").show();

        jq("#studio_emailOperationContent").removeClass("display-none");

        jq("#emailChangeDialogText").hide();
        jq("#emailActivationDialogText").hide();
        jq("#resendInviteDialogText").show();

        jq("#emailOperation_email").val(userEmail);
        jq("#btEmailOperationSend").removeClass("disable");
        jq("#emailOperation_email").unbind("onkeyup");
        
        openPopupDialog();

        jq("#btEmailOperationSend").unbind("click");

        jq("#btEmailOperationSend").click(function () {
            var newEmail = jq("#emailOperation_email").val();
            sendEmailActivationInstructions(newEmail, userID, responseAction);
            return false;
        });

    };

    function openPopupDialog () {
        StudioBlockUIManager.blockUI("#studio_emailChangeDialog", 425, 300, 0);

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

    return {
        sendInstructions: sendInstructions,
        sendEmailActivationInstructions: sendEmailActivationInstructions,
        showResendInviteWindow: showResendInviteWindow,
        showEmailChangeWindow: showEmailChangeWindow,
        closeEmailOperationWindow: closeEmailOperationWindow
    }
})();

/**
 * LoadingBanner
 */
LoadingBanner = function () {
    var animateDelay = 2000,
        displayDelay = 500,
        displayOpacity = 1,
        loaderCss = "",
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
        animateDelay: animateDelay,
        displayDelay: displayDelay,
        displayOpacity: displayOpacity,
        loaderCss: loaderCss,
        loaderId: loaderId,
        strLoading: strLoading,
        strDescription: strDescription,
        successId : successId,
        strSuccess: strSuccess,

        displayLoading: function (withoutDelay, withBackdrop) {
            var id = "#" + LoadingBanner.loaderId;

            if (jq(id).length != 0)
                return;

            var innerHtml = '<div id="{0}" class="loadingBanner {1}"><div class="loader-block">{2}<div>{3}</div></div></div>'
                .format(LoadingBanner.loaderId, LoadingBanner.loaderCss, LoadingBanner.strLoading, LoadingBanner.strDescription);

            if (withBackdrop)
                jq("body").append('<div class="loadingBannerBackDrop"></div>');

            jq("body").append(innerHtml).addClass("loading");

            if (jq.browser.mobile)
                jq(id).css("top", jq(window).scrollTop() + "px");

            if (withoutDelay) return;

            jq(id).animate({ opacity: 0 }, LoadingBanner.displayDelay, function() {
                jq(id).animate({ opacity: LoadingBanner.displayOpacity }, LoadingBanner.animateDelay);
            });
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

            var loaderHtml = "<div class=\"loader-container\">{0}</div>".format(LoadingBanner.strLoading),
                btnContainer = jq(block).find("[class*=\"button-container\"]");
            jq(btnContainer).siblings(".error-popup, .success-popup").each(function() {jq(this).hide();});
            btnContainer.find(".button").addClass("disable").attr("disabled" , true);
            jq(btnContainer).append(loaderHtml);

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
            $menuSpacerObj = jq(options.menuSpacerSelector);

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
            jq(".studio-action-panel.group-actions").css(
                {
                    "top": jq(document).scrollTop() + $menuObj.outerHeight() - 4 + "px",
                    "position": "absolute"
                });

            jq(".studio-action-panel.other-actions").css(
                {
                    top: $menuObj.outerHeight() - 4 + "px",
                    position: "fixed"
                });
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
            jq(".studio-action-panel.group-actions").css(
                {
                    "top": $menuObj.offset().top + $menuObj.outerHeight() - 4 + "px"
                });
            jq(".studio-action-panel.other-actions").css(
                {
                    top: $menuObj.offset().top + $menuObj.outerHeight() - 4 + "px",
                    position: "absolute"
                });
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
    function getUser(userId) {
        var users = ASC.Resources.Master.ApiResponses_Profiles.response;

        if (!users) {
            return null;
        }

        for (var i = 0; i < users.length; i++) {
            if (users[i].id == userId) {
                return users[i];
            }
        }

        return null;
    }
    
    function getUsers(ids) {
        if (!ids || !ids.length) {
            return [];
        }

        var users = ASC.Resources.Master.ApiResponses_Profiles.response;
        if (!users) {
            return [];
        }

        var result = [];
        for (var i = 0; i < users.length; i++) {
            if (~ids.indexOf(users[i].id)) {
                result.push(users[i]);
            }
        }

        return result;
    }

    return {
        getUser: getUser,
        getUsers: getUsers
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

    form.submit();
};

window.TipsManager = new function() {
    function neverShowTips() {
        Teamlab.updateTipsSettings({ show: false });
    }

    return {
        neverShowTips: neverShowTips
    };
};