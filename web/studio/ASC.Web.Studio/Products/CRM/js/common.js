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
/*******************************************************************************
JQuery Extension
*******************************************************************************/

jq.browser.safari = (jq.browser.safari && !/chrome/.test(navigator.userAgent.toLowerCase())) == !0;

/*******************************************************************************
*******************************************************************************/

if (typeof ASC === "undefined") {
    ASC = {};
}
if (typeof ASC.CRM === "undefined") {
    ASC.CRM = (function() { return {} })();
}

jQuery.extend({

    forceNumericOnly: function() {
        return this.each(function() {
            jq(this).keydown(function(e) {
                var key = e.charCode || e.keyCode || 0;
                // allow backspace, tab, delete, arrows, numbers and keypad numbers ONLY
                return (
                    key == 8 || key == 9 || key == 46 ||
                    (key >= 37 && key <= 40) ||
                    (key >= 48 && key <= 57) ||
                    (key >= 96 && key <= 105));
            });
        });
    },

    forcePhoneSymbolsOnly: function(objSelector) {
        var phoneSymbolsReg = /^\+?[0-9-()\s]+$/;
        jq(objSelector).each(function() {
            jq(this).keypress(function(event) {

                var index = jq(this).val().indexOf("+"),
                    caretPos = jq(this).caret().start,
                    controlKeys = [8, 9],
                    isControlKey = controlKeys.join(",").match(new RegExp(event.which)),
                    keyCode = event.which;

                if (!keyCode ||
                    (48 <= keyCode && keyCode <= 57) //number
                    || isControlKey
                    || (keyCode == 43 && caretPos == 0 && index == -1) //plus
                    || (keyCode == 32 && caretPos != 0) //space
                    || keyCode == 45 || keyCode == 40 || keyCode == 41) {
                    return;
                } else {
                    event.preventDefault();
                }
            });

            jq(this).unbind('paste').bind('paste', function(e) {
                var oldValue = this.value,
                    $obj = this;
                setTimeout(
                    function() {
                        var text = jq($obj).val();
                        if (!phoneSymbolsReg.test(text)) {
                            jq($obj).val(oldValue);
                        }
                    }, 0);
                return true;
            });
        });
    },

    forceIntegerOnly: function(ObjSelector, doIfInteger) {
        jq(ObjSelector).each(function() {
            jq(this).keypress(function(event) {
                // Backspace, Del
                var controlKeys = [8, 9],
                // IE doesn't support indexOf
                    isControlKey = controlKeys.join(",").match(new RegExp(event.which));
                // Some browsers just don't raise events for control keys. Easy.

                if (!event.which // Control keys in most browsers. e.g. Firefox tab is 0
                    || (48 <= event.which && event.which <= 57) // Always 0 through 9
                    || isControlKey) {
                    return;
                } else {
                    event.preventDefault();
                }
            });

            jq(this).unbind('paste').bind('paste', function(e) {
                var oldValue = this.value,
                    $obj = this;
                setTimeout(
                    function() {
                        var text = jq($obj).val();
                        if (isNaN(text) || text.indexOf(".") != -1) {
                            jq($obj).val(oldValue);
                        }
                        else if (typeof (doIfInteger) != "undefined") {
                            doIfInteger();
                        }
                    }, 0);
                return true;
            });

        });
    },


    forceCurrencySymbolsOnly: function(objSelector, lengthAfterSeparator) {
        var reg = "";

        if (typeof (lengthAfterSeparator) != "undefined" && !isNaN(lengthAfterSeparator)) {
            reg = new RegExp(['^\\d*(', ASC.CRM.Data.CurrencyDecimalSeparator, '\\d{1,', lengthAfterSeparator, '})?$'].join(''));
        } else {
            reg = new RegExp(['^\\d*(', ASC.CRM.Data.CurrencyDecimalSeparator, '\\d*)?$'].join(''));
        }

        jq(objSelector).keypress(function (event) {
            var text = jq(this).val(),
            // Backspace, Del,
                controlKeys = [8, 9],
            // IE doesn't support indexOf
                isControlKey = controlKeys.join(",").match(new RegExp(event.which));
            jq.data(this, "currencyValue", text);
            // Some browsers just don't raise events for control keys. Easy.
            if ((!event.which || // Control keys in most browsers. e.g. Firefox tab is 0
                (48 <= event.which && event.which <= 57) || // Always 0 through 9
                    isControlKey) ||
                        text.length - text.replace(ASC.CRM.Data.CurrencyDecimalSeparator, '').length < 1
                        && event.which == ASC.CRM.Data.KeyCodeCurrencyDecimalSeparator * 1)
            {
                jq.data(this, "currencyValue", text);
                return;
            } else {
                event.preventDefault();
            }
        });

        jq(objSelector).unbind('paste').bind('paste', function (e) {
            var $obj = this,
                old_text = jq($obj).val();
            setTimeout(
                function() {
                    var text = jq($obj).val();
                    if (reg.test(text)) {
                        jq($obj).val(text);
                    } else {
                        jq($obj).val(old_text);
                    }
                }, 0);
            return true;
        });
    },

    /**
    * Returns get parameters.
    *
    * If the desired param does not exist, null will be returned
    *
    * @example value = $.getURLParam("paramName");
    */
    getURLParam: function(strParamName) {
        strParamName = strParamName.toLowerCase();

        var strReturn = "",
            strHref = window.location.href.toLowerCase(),
            bFound = false,

            cmpstring = strParamName + "=",
            cmplen = cmpstring.length;

        if (strHref.indexOf("?") > -1) {
            var strQueryString = strHref.substr(strHref.indexOf("?") + 1),
                aQueryString = strQueryString.split("&");
            for (var iParam = 0; iParam < aQueryString.length; iParam++) {
                if (aQueryString[iParam].substr(0, cmplen) == cmpstring) {
                    var aParam = aQueryString[iParam].split("=");
                    strReturn = aParam[1];
                    bFound = true;
                    break;
                }

            }
        }
        if (bFound == false) { return null; }

        if (strReturn.indexOf("#") > -1) {
            return strReturn.split("#")[0];
        }
        return strReturn;
    }
}
);

jq.fn.datepickerWithButton = function(options) {
    return this.datepicker(jQuery.extend({
        showOn: "both",
        buttonText: "",
        buttonImageOnly: false
    }, options));
}

/*******************************************************************************
*******************************************************************************/

ASC.CRM.Common = (function() {
    return {

        tooltip: function (target_items, name, alwaysLeft) {
            var cornerHTML = "",
                tooltip = null,
                my_tooltip = null,
                left = 0,
                top = 0,
                addLeft = 0;

            var hideDescrPanel = function (tooltipItem) {
                setTimeout(function () {
                    if (!tooltipItem.data("overTaskDescrPanel")) {
                        tooltipItem.hide();
                    }
                }, 150);
            };


            jq(target_items).each(function () {
                var id = jq.trim(jq(this).attr('id')).split('_')[1],
                    title = jq.trim(jq(this).attr('title')),
                    params = ASC.CRM.Common.getTooltipParams(jq(this)),
                    my_tooltip = null;
                if (title == "" && params == "") {
                    return;
                }
                if (jq("#" + name + id).length > 0) {
                    jq("#" + name + id).remove();
                }

                jq("body").append(["<div class='studio-action-panel tooltip' id='",
                                        name, id, "'>",
                                        "<div class='corner-top ",
                                        typeof (alwaysLeft) != "undefined" && alwaysLeft === true ? "right" : "left",
                                        "'>",
                                        "</div>",
                                        jq.htmlEncodeLight(title).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                                        params,
                                        "</div>"].join(''));

                my_tooltip = jq("#" + name + id);

                jq(this).removeAttr("title")
                    .unbind("mouseenter").mouseenter(function () {
                        var $obj = jq(this),
                            top = $obj.position().top + $obj.height() + 5,
                            left = $obj.position().left + 5;

                        jq(my_tooltip).data("overTaskDescrPanel", true);
                        jq(my_tooltip).css("top", top);

                        if (typeof (alwaysLeft) != "undefined" && alwaysLeft === true) {
                            var right = jq(window).width() - left - 35;
                            jq(my_tooltip).css("right", right);

                        } else {
                            jq(my_tooltip).css("left", left);
                        }
                        jq("div[id^='" + name + "']:not(div[id='" + name + id + "'])").hide();
                        my_tooltip.fadeIn(300);
                    })
                    .unbind("mouseleave").mouseleave(function () {
                        jq(my_tooltip).data("overTaskDescrPanel", false);
                        hideDescrPanel(my_tooltip);
                    });

                jq(my_tooltip)
                    .mouseenter(function () {
                        jq(my_tooltip).data("overTaskDescrPanel", true);
                    })
                    .mouseleave(function () {
                        jq(my_tooltip).data("overTaskDescrPanel", false);
                        hideDescrPanel(my_tooltip);
                    });
            });
        },

        getTooltipParams: function (obj) {
            var $o = jq(obj),
                label = jq.trim($o.attr("ttl_label")),
                value = jq.trim($o.attr("ttl_value")),
                dscr_label = jq.trim($o.attr("dscr_label")),
                dscr_value = jq.trim($o.attr("dscr_value")),
                resp_label = jq.trim($o.attr("resp_label")),
                resp_value = jq.trim($o.attr("resp_value"));

            $o.removeAttr("ttl_label");
            $o.removeAttr("ttl_value");
            $o.removeAttr("dscr_label");
            $o.removeAttr("dscr_value");
            $o.removeAttr("resp_label");
            $o.removeAttr("resp_value");

            if ((label == "" || value == "") &&
                (dscr_label == "" || dscr_value == "") &&
                (resp_label == "" || resp_value == "")) {
                return "";
            }
            var res = ["<table class='tooltipTable' cellspacing='0' cellpadding='0'>",
                        "<colgroup>",
                            "<col style='width: 1%;' />",
                            "<col style='width: 15px;' />",
                            "<col />",
                        "</colgroup>",
                       "<tbody>",
            ].join('');


            if (label != "" && value != "") {
                res += ["<tr><td><div class='param'>",
                        jq.htmlEncodeLight(label).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                        ":</div></td>",
                        "<td></td>",
                        "<td><div class='value'>",
                        jq.htmlEncodeLight(value).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                        "</div></td></tr>"].join('');
            }

            if (dscr_label != "" && dscr_value != "") {
                res += ["<tr><td><div class='param'>",
                        jq.htmlEncodeLight(dscr_label).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                        ":</div></td>",
                        "<td></td>",
                        "<td><div class='value'>",
                        jq.htmlEncodeLight(dscr_value).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                        "</div></td></tr>"].join('');
            }

            if (resp_label != "" && resp_value != "") {
                res += ["<tr><td><div class='param'>",
                        jq.htmlEncodeLight(resp_label).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                        ":</div></td>",
                        "<td></td>",
                        "<td><div class='value'>",
                        jq.htmlEncodeLight(resp_value).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;"),
                        "</div></td></tr>"].join('');
            }

            res += "</tbody></table>";
            return res;
        },

        registerChangeHoverStateByParent: function (elemSelector, hoverParentSelector) {
            var hasParent = true;
            if (typeof (hoverParentSelector) == "undefined" || hoverParentSelector == null || hoverParentSelector == "") {
                hasParent = false;
                hoverParentSelector = elemSelector;
            }

            jq(document).on("mouseover", hoverParentSelector, function () {
                var $elt = {};
                if (hasParent) {
                    $elt = jq(this).find(elemSelector);
                } else {
                    $elt = jq(this);
                }
                if ($elt.length == 0) return;
                $elt.addClass("hover");
            });

            jq(document).on("mouseout", hoverParentSelector, function () {
                var $elt;
                if (hasParent) {
                    $elt = jq(this).find(elemSelector);
                } else {
                    $elt = jq(this);
                }
                if ($elt.length == 0) return;
                $elt.removeClass("hover");
            });
        },

        RegisterContactInfoCard: function () {
            if (jq("#contactInfoCardResources").length == 0) {
                jq.tmpl("contactInfoCardResourcesTmpl", null).appendTo("body");
            }

            var CRMContactInfoCard = new PopupBox('popup_ContactInfoCard', 320, 140, 'tintLight', 'borderBase', '',
            {
                apiMethodName: "Teamlab.getCrmContact",
                tmplName: "contactInfoCardTmpl",
                customFactory: ASC.CRM.Common.contactItemFactory
            });

            jq(window).bind("registerContactInfoCard", function (event, newElem) {
                var id = newElem.attr('id');
                if (id != null && id != '') {
                    CRMContactInfoCard.RegistryElement(id, newElem.attr('data-id'));
                }
            });

            jq(".crm-companyInfoCardLink, .crm-peopleInfoCardLink").each(function (index) {
                jq(window).trigger("registerContactInfoCard", [jq(this)]);
            });
        },

        contactItemFactory: function (contact, params) {
            contact.showCompanyLink = (typeof (params) != "undefined" && params != null && params.hasOwnProperty("showCompanyLink") && params.showCompanyLink);
            contact.showUnlinkBtn = (typeof (params) != "undefined" && params != null && params.hasOwnProperty("showUnlinkBtn") && params.showUnlinkBtn);
            contact.showActionMenu = (typeof (params) != "undefined" && params != null && params.hasOwnProperty("showActionMenu") && params.showActionMenu);

            var basePathForLink = StudioManager.getLocationPathToModule("crm") + "default.aspx?id=";

            contact.contactLink = basePathForLink + contact.id;
            if (contact.showCompanyLink && typeof (contact.company) && contact.company != null) {
                contact.company.contactLink = basePathForLink + contact.company.id;
            }

            contact.primaryAddress = null;
            contact.primaryEmail = null;
            if (typeof (contact.addresses) != "undefined" && contact.addresses.length != 0) {
                for (var addr in contact.addresses) {
                    if (contact.addresses.hasOwnProperty(addr)) {
                        var addressObj = contact.addresses[addr];
                        if (addressObj.isPrimary) {
                            var query = ASC.CRM.Common.getAddressQueryForMap(addressObj);
                            contact.primaryAddress = {
                                Data: ASC.CRM.Common.getAddressTextForDisplay(addressObj),
                                Href: "http://maps.google.com/maps?q=" + query,
                                categoryName: addressObj.categoryName
                            };
                            break;
                        }
                    }
                }
            }

            if (typeof (contact.commonData) != "undefined" && contact.commonData.length != 0) {
                for (var item in contact.commonData) {
                    if (contact.commonData.hasOwnProperty(item)) {
                        var itemObj = contact.commonData[item];
                        if (itemObj.infoType == 1 && itemObj.isPrimary) {//Primary Email
                            contact.primaryEmail = itemObj;
                            break;
                        }
                    }
                }
            }
        },

        getAddressTextForDisplay: function (addressObj) {
            if (typeof (addressObj) != "object") return "";

            var text = jq.htmlEncodeLight(addressObj.street),
                tmp = addressObj.city != "" ? jq.htmlEncodeLight(addressObj.city) + ", " : "";

            if (addressObj.state != "") {
                tmp += jq.htmlEncodeLight(addressObj.state) + ", ";
            }
            if (addressObj.zip != "") {
                tmp += jq.htmlEncodeLight(addressObj.zip);
            }
            tmp = jq.trim(tmp);
            tmp = tmp.charAt(tmp.length - 1) === ',' ? tmp.substring(0, tmp.length - 1) : tmp;
            if (tmp != "") {
                text = text != "" ? text + ",<br/>" + tmp : tmp;
            }
            text = text != "" && addressObj.country != ""
                    ? text + ",<br/>" + jq.htmlEncodeLight(addressObj.country)
                    : (addressObj.country != "" ? jq.htmlEncodeLight(addressObj.country) : text);
            return text;
        },

        getAddressQueryForMap: function (addressObj) {
            if (typeof (addressObj) != "object") return "";

            var query = "";
            if (addressObj.street != "") {
                query += jq.htmlEncodeLight(addressObj.street) + ", ";
            }
            if (addressObj.city != "") {
                query += jq.htmlEncodeLight(addressObj.city) + ", ";
            }
            if (addressObj.state != "") {
                query += jq.htmlEncodeLight(addressObj.state) + ", ";
            }
            if (addressObj.zip != "") {
                query += jq.htmlEncodeLight(addressObj.zip) + ", ";
            }
            if (addressObj.country != "") {
                query += addressObj.country + ", ";
            }
            query = jq.trim(query);
            query = query.charAt(query.length - 1) === ',' ? query.substring(0, query.length - 1) : query;
            return query;
        },


        isHiddenForScroll: function (el) {
            var pos = el.position(),
                scrTop = jq(window).scrollTop(),
                scrLeft = jq(window).scrollLeft();
            if (pos.top > scrTop && pos.left > scrLeft &&
                pos.top + el.height() < scrTop + jq(window).height() &&
                    pos.left + el.width() < scrLeft + jq(window).width()) {
                return false;
            }
            return true;
        },

        renderCustomFields: function (customFieldList, custom_field_id_prefix, customFieldTmplID, customFieldTmplAppendToObjSelector) {
            var tmpSelectFields = [],
                field = {};
            for (var i = 0, n = customFieldList.length; i < n; i++) {
                field = customFieldList[i];

                if (jQuery.trim(field.mask) == "") {
                    continue;
                }

                field.mask = jq.evalJSON(field.mask);
                if (customFieldList[i].fieldType == 2) {
                    tmpSelectFields.push(customFieldList[i]);
                }
            }
            jq.tmpl(customFieldTmplID, customFieldList).appendTo(customFieldTmplAppendToObjSelector);

            for (var k = 0, len = tmpSelectFields.length; k < len; k++) {
                jq('#' + custom_field_id_prefix + tmpSelectFields[k].id).val(tmpSelectFields[k].value);
            }
        },

        sortCustomFieldList: function (customFieldList) {
            var sortedList = [],
                subList = {
                    label: "",
                    list: []
                };

            for (var i = 0, n = customFieldList.length; i < n; i++) {
                var field = customFieldList[i];
                if (jQuery.trim(field.mask) != "") {
                    field.mask = jq.evalJSON(field.mask);
                }
                if (field.fieldType == 4 || i == n - 1) {
                    if (field.fieldType != 4) {
                        subList.list.push(field);
                    }

                    sortedList.push(subList);

                    if (field.fieldType == 4) {
                        subList = {
                            label: field.label,
                            list: []
                        };
                    }
                } else {
                    subList.list.push(field);
                }
            }

            for (var i = 0, n = sortedList.length; i < n; i++) {
                if (sortedList[i].list.length == 0) {
                    sortedList.splice(i, 1);
                    i--;
                    n--;
                }
            }
            return sortedList;
        },

        initTextEditCalendars: function () {
            var $calendars = jq("input.textEditCalendar"),
                $c = null,
                value = "";
            for (var i = 0, n = $calendars.length; i < n; i++) {
                $c = jq($calendars[i]);
                value = jq.trim($c.val());
                $c.mask(ASC.Resources.Master.DatePatternJQ);

                if (value == "") {
                    $c.val("");
                }
                $c.datepickerWithButton();
            }
        },

        changeCountInTab: function (actionOrCount, tabAnchorName) {
            var contactTab;

            if (typeof (tabAnchorName) != "undefined" && tabAnchorName != "") {
                if (jq("li.viewSwitcherTab_" + tabAnchorName).length == 0) {
                    return false;
                }
                contactTab = jq("li.viewSwitcherTab_" + tabAnchorName);
            } else {
                if (jq("li[id$=_ViewSwitcherTab].viewSwitcherTabSelected").length == 0) {
                    return false;
                }
                contactTab = jq("li[id$=_ViewSwitcherTab].viewSwitcherTabSelected")[0];
            }

            var contactTabTextArray = jq(contactTab).text().split(" ");

            if (typeof (actionOrCount) == "string") {
                if (actionOrCount == "add") {
                    if (contactTabTextArray.length > 1) {
                        var newValue = parseInt(contactTabTextArray[contactTabTextArray.length - 1].replace("(", "").replace(")", "")) + 1;
                        contactTabTextArray[contactTabTextArray.length - 1] = jq.format("({0})", newValue);
                        jq(contactTab).text(contactTabTextArray.join(" "));
                    } else {
                        jq(contactTab).text(jq.format("{0} (1)", contactTabTextArray[0]));
                    }
                } else if (actionOrCount == "delete") {
                    var newValue = parseInt(contactTabTextArray[contactTabTextArray.length - 1].replace("(", "").replace(")", "")) - 1;
                    if (newValue > 0) {
                        contactTabTextArray[contactTabTextArray.length - 1] = jq.format("({0})", newValue);
                        jq(contactTab).text(contactTabTextArray.join(" "));
                    } else {
                        jq(contactTab).text(contactTabTextArray[0]);
                    }
                }
            } else if (typeof (actionOrCount) == "number") {
                var count = parseInt(actionOrCount);
                if (count > 0) {
                    jq(contactTab).text(jq.format("{0} ({1})", contactTabTextArray[0], count));
                } else {
                    jq(contactTab).text(contactTabTextArray[0]);
                }
            }
        },

        getHexRGBColor: function (color) {
            color = color.replace(/\s/g, "");
            var aRGB = color.match(/^rgb\((\d{1,3}[%]?),(\d{1,3}[%]?),(\d{1,3}[%]?)\)$/i);

            if (aRGB) {
                color = '#';
                for (var i = 1; i <= 3; i++) {
                    color += Math.round((aRGB[i][aRGB[i].length - 1] == "%" ? 2.55 : 1) * parseInt(aRGB[i])).toString(16).replace(/^(.)$/, '0$1');
                }
            } else {
                color = color.replace(/^#?([\da-f])([\da-f])([\da-f])$/i, '$1$1$2$2$3$3');
            }

            return color;
        },

        animateElement: function (options) {
            if (typeof (options) != "object") {
                return;
            }
            var element = options.element;

            if (ASC.CRM.Common.isHiddenForScroll(element)) {
                jq.scrollTo(element);
                if (typeof (options.whenScrollFunc) === "function") {
                    options.whenScrollFunc();
                }
            }

            if (typeof (options.afterScrollFunc) === "function") {
                options.afterScrollFunc();
            }

            element.css({ "background-color": "#ffffcc" });
            element.animate({ backgroundColor: '#ffffff' }, 2000, function () {
                element.css({ "background-color": "" });
            });
        },

        removeExportButtons: function () {
            if (jq("#exportListToCSV").length == 1) {
                jq("#exportListToCSV").parent().remove();
            }
            if (jq("#openListInEditor").length == 1) {
                jq("#openListInEditor").parent().remove();
            }
        },

        hideExportButtons: function () {
            if (jq("#exportListToCSV").length == 1) {
                jq("#exportListToCSV").parent().addClass("display-none");
            }
            if (jq("#openListInEditor").length == 1) {
                jq("#openListInEditor").parent().addClass("display-none");
            }
        },

        showExportButtons: function () {
            if (jq("#exportListToCSV").length == 1) {
                jq("#exportListToCSV").parent().removeClass("display-none");
            }
            if (jq("#openListInEditor").length == 1) {
                jq("#openListInEditor").parent().removeClass("display-none");
            }
        },

        exportCurrentListToCsv : function() {
            var index = window.location.href.indexOf('#'),
                basePath = index >= 0 ? window.location.href.substr(0, index) : window.location.href,
                anchor = index >= 0 ? window.location.href.substr(index, window.location.href.length) : "";
            jq("#otherActions").hide();
            window.location.href = [
                basePath,
                window.location.search != null && window.location.search != "" ? "&" : "?",
                "action=export",
                anchor].join('');
        },

        openExportFile : function() {
            var index = window.location.href.indexOf('#'),
                basePath = index >= 0 ? window.location.href.substr(0, index) : window.location.href;
            jq("#otherActions").hide();

            window.open([
                basePath,
                window.location.search != null && window.location.search != "" ? "&" : "?",
                "action=export&view=editor"]
                .join(''));
        },

        fixEvent: function (e) {
            e = e || window.event;
            if (e.pageX == null && e.clientX != null) {
                var html = document.documentElement,
                    body = document.body;
                e.pageX = e.clientX + (html && html.scrollLeft || body && body.scrollLeft || 0) - (html.clientLeft || 0);
                e.pageY = e.clientY + (html && html.scrollTop || body && body.scrollTop || 0) - (html.clientTop || 0);
            }
            //            if (!e.which && e.button) {
            //                e.which = e.button & 1 ? 1 : (e.button & 2 ? 3 : (e.button & 4 ? 2 : 0));
            //            }
            return e;
        },

        getMailModuleBasePath: function () {
            var products = "products",
                mass = location.href.toLowerCase().split(products);
            return mass[0] + "addons/mail/";
        },

        isArray: function (o) {
            return o ? o.constructor.toString().indexOf("Array") != -1 : false;
        },

        findUserByGuid: function (id) {
            for (var i = 0, n = ASC.Resources.Master.ApiResponses_Profiles.response.length; i < n; i++) {
                if (ASC.Resources.Master.ApiResponses_Profiles.response[i].id == id) {
                    return ASC.Resources.Master.ApiResponses_Profiles.response[i];
                }
            }
            return null;
        },

        getAdminUsers: function () {
            var admins = [];
            for (var i = 0, n = ASC.Resources.Master.ApiResponses_Profiles.response.length; i < n; i++) {
                if (ASC.Resources.Master.ApiResponses_Profiles.response[i].isAdmin === true) {
                    admins.push(ASC.Resources.Master.ApiResponses_Profiles.response[i]);
                }
            }
            return admins;
        },

        getAccessListHtml: function (userGuids) {
            if (typeof (userGuids) == "undefined" || userGuids.length == 0) { return ""; }
            var html = "";
            for (var i = 0, n = userGuids.length; i < n; i++) {
                var usr = ASC.CRM.Common.findUserByGuid(userGuids[i]);
                html += [
                    i == 0 ? "" : ",<span class='splitter'></span>",
                    usr != null ? usr.displayName : ASC.CRM.Data.ProfileRemoved
                ].join('');
            }
            return html;
        },

        isUserActive: function (id) {
            //ASC.Resources.Master.ApiResponses_Profiles.response - only active users
            for (var i = 0, n = ASC.Resources.Master.ApiResponses_Profiles.response.length; i < n; i++) {
                if (ASC.Resources.Master.ApiResponses_Profiles.response[i].id === id) {
                    return true;
                }
            }
            return false;
        },

        convertText: function (str, toText) {
            str = typeof str === 'string' ? str : '';
            if (!str) {
                return '';
            }

            if (toText === true) {
                var
                    symbols = [
                    ['&lt;', '<'],
                    ['&gt;', '>'],
                    ['&and;', '\\^'],
                    ['&sim;', '~'],
                    ['&amp;', '&']
                    ];

                var symInd = symbols.length;
                while (symInd--) {
                    str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
                }
                return str;
            }

            var o = document.createElement('textarea');
            o.innerHTML = str;
            return o.value;
        },

        loadContactFoto: function (curImgObj, newImgObj, handlerSrc, withAnimation) {
            if (handlerSrc.indexOf("filehandler.ashx") != -1) {
                jq.ajax({
                    type: "GET",
                    url: handlerSrc,
                    success: function (response) {
                        try {
                            if (curImgObj.attr("src").indexOf(response) === -1) {
                                newImgObj.one("load", function () {
                                    if (typeof (withAnimation) != "undefined" && withAnimation === true) {
                                        var left = (newImgObj.parent().width() - newImgObj.width()) / 2,
                                            top = (newImgObj.parent().height() - newImgObj.height()) / 2;
                                        newImgObj
                                            .css("left", left)
                                            .css("top", top);

                                        curImgObj.fadeOut(500);
                                        newImgObj.fadeIn(500);
                                    } else {
                                        curImgObj.hide();
                                        newImgObj.show();
                                    }
                                });
                                newImgObj.attr("src", [response, "?", new Date().getTime()].join(''));
                            }
                        }
                        catch (e) { }
                    }
                });
            } else {
                newImgObj.attr("src", handlerSrc);
                curImgObj.hide();
                newImgObj.show();
            }
        },

        getPagingParamsFromCookie : function (cookieKey) {
            var cookie = jq.cookies.get(cookieKey);
            if (cookie != null) {
                if (cookie.hasOwnProperty("page") && cookie.hasOwnProperty("countOnPage")) {
                    return {
                        countOnPage : cookie.countOnPage,
                        page : cookie.page
                    };
                }
            }
            return null;
        },

        arrayUnique : function(array) {
            var a = array.concat();
            for (var i  = 0; i < a.length; ++i) {
                for (var j = i + 1; j < a.length; ++j) {
                    if (a[i] === a[j])
                        a.splice(j--, 1);
                }
            }
            return a;
        },

        numberFormat: function (number, _cfg) {
            var _cfg_default = {
                before: '',
                after: '',
                decimals: 2,
                dec_point: '.',
                thousands_sep: ','
            };

            function thousands_sep(num, sep) {
                if (num.length <= 3) { return num; }
                var _count = num.length,
                    num_parser = '',
                    count_digits = 0;
                for (var _p = (_count - 1) ; _p >= 0; _p--) {
                    var num_digit = num.substr(_p, 1);
                    if (count_digits % 3 == 0 && count_digits != 0 && !isNaN(parseFloat(num_digit))) num_parser = sep + num_parser;
                    num_parser = num_digit + num_parser;
                    count_digits++;
                }
                return num_parser;
            }

            if (typeof number !== 'number') {
                number = parseFloat(number);
                if (isNaN(number)) return false;
            }

            if (_cfg && typeof _cfg === 'object') {
                _cfg = jq.extend(_cfg_default, _cfg);
            } else {
                _cfg = _cfg_default;
            }
            number = number.toFixed(_cfg.decimals);
            if (number.indexOf('.') != -1) {
                var number_arr = number.split('.'),
                    number = thousands_sep(number_arr[0], _cfg.thousands_sep) + _cfg.dec_point + number_arr[1];
            } else {
                var number = thousands_sep(number, _cfg.thousands_sep);
            }
            return [_cfg.before, number, _cfg.after].join('');
        },

        historyEventMsgObjFactory : function(eventItem) {
            var content = jq.parseJSON(eventItem.content);
            eventItem.content = content.subject != null && content.subject != "" ? content.subject : ASC.CRM.Resources.CRMJSResource.NoSubject;
            eventItem.message = {
                id : eventItem.id,
                subject : eventItem.content,
                is_sended : content.is_sended,
                important: content.important,
                from : content.from,
                to : content.to,
                cc : content.cc,
                bcc: content.bcc,
                message_url: content.message_url
            };
            if (content.hasOwnProperty("date_created")) {
                try {
                    eventItem.message.date_created = Teamlab.getDisplayDatetime(Teamlab.serializeDate(content.date_created));
                } catch(e) {
                    eventItem.message.date_created = content.date_created;
                }
            } else {
                eventItem.message.date_created = eventItem.displayCrtdate
            }
            if (content.cc != "" || content.bcc != "") {
                var cc_text = content.cc != "" ? content.cc : "";
                cc_text += content.bcc != "" ?
                    ((cc_text != "" ? ", " : "") + content.bcc) :
                    "";
                eventItem.message.cc_text = cc_text;
            }

            var basePathMail = ASC.CRM.Common.getMailModuleBasePath();
            eventItem.message.fromHref = [
                                            basePathMail,
                                            "#composeto/email=",
                                            eventItem.message.from
            ].join('');

            eventItem.message.toHref = [
                                            basePathMail,
                                            "#composeto/email=",
                                            eventItem.message.to
            ].join('');
        },

        setDocumentTitle : function (module) {
            document.title = jq.format("{0} - {1}", module, ASC.CRM.Resources.CRMCommonResource.ProductName);
        },

        getCurrencySymbol: function (symbol, abbr) {
            return abbr != "RUB" ? symbol : "<span class='rub'>Р</span>";
        },

        bindOnbeforeUnloadEvent: function () {
            if (window.onbeforeunload == null) {
                window.onbeforeunload = function (e) {
                    return ASC.Resources.Master.Resource.WarningMessageBeforeUnload;
                };
            }
        },
        unbindOnbeforeUnloadEvent: function () {
            window.onbeforeunload = null;
        },

        getRelativeItemsLinkString: function (count, typeUrl, view) {
            var relativeItemsString = "",
                oneElement = "",
                manyElements = "",
                noElements = "";

            if (typeUrl == null) { typeUrl = ""; }
            if (view == null) { view = ""; }

            switch (jq.trim(typeUrl)) {
                case "deal_milestone":
                    oneElement = ASC.CRM.Resources.CRMJSResource.OneDeal;
                    manyElements = ASC.CRM.Resources.CRMJSResource.ManyDeals;
                    noElements = ASC.CRM.Resources.CRMJSResource.NoDeals;
                    break;
                case "contact_stage":
                case "contact_type":
                    oneElement = ASC.CRM.Resources.CRMJSResource.OneContact;
                    manyElements = ASC.CRM.Resources.CRMJSResource.ManyContacts;
                    noElements = ASC.CRM.Resources.CRMJSResource.NoContacts;
                    break;
                case "task_category":
                    oneElement = ASC.CRM.Resources.CRMJSResource.OneTask;
                    manyElements = ASC.CRM.Resources.CRMJSResource.ManyTasks;
                    noElements = ASC.CRM.Resources.CRMJSResource.NoTasks;
                    break;
                case "history_category":
                    oneElement = ASC.CRM.Resources.CRMJSResource.OneEvent;
                    manyElements = ASC.CRM.Resources.CRMJSResource.ManyEvents;
                    noElements = ASC.CRM.Resources.CRMJSResource.NoEvents;
                    break;
                case "custom_field":
                case "tag":
                    switch (jq.trim(view)) {
                        case "opportunity":
                            oneElement = ASC.CRM.Resources.CRMJSResource.OneDeal;
                            manyElements = ASC.CRM.Resources.CRMJSResource.ManyDeals;
                            noElements = ASC.CRM.Resources.CRMJSResource.NoDeals;
                            break;
                        case "case":
                            oneElement = ASC.CRM.Resources.CRMJSResource.OneCase;
                            manyElements = ASC.CRM.Resources.CRMJSResource.ManyCases;
                            noElements = ASC.CRM.Resources.CRMJSResource.NoCases;
                            break;

                        case "company":
                            oneElement = ASC.CRM.Resources.CRMJSResource.OneCompany;
                            manyElements = ASC.CRM.Resources.CRMJSResource.ManyCompanies;
                            noElements = ASC.CRM.Resources.CRMJSResource.NoCompanies;
                            break;
                        case "people":
                        case "person":
                            oneElement = ASC.CRM.Resources.CRMJSResource.OnePerson;
                            manyElements = ASC.CRM.Resources.CRMJSResource.ManyPersons;
                            noElements = ASC.CRM.Resources.CRMJSResource.NoPersons;
                            break;

                        default:
                            oneElement = ASC.CRM.Resources.CRMJSResource.OneContact;
                            manyElements = ASC.CRM.Resources.CRMJSResource.ManyContacts;
                            noElements = ASC.CRM.Resources.CRMJSResource.NoContacts;
                            break;
                    }
            }

            if (count == 0) {
                return noElements;
            }
            if (count == 1) {
                relativeItemsString = "1 " + oneElement;
            } else {
                relativeItemsString = count + " " + manyElements;
            }
            return relativeItemsString;
        },

        createInvoiceMail: function (invoice, newTab) {
            var message = new ASC.Mail.Message();
            message.to = [typeof (invoice.contact.email) != "undefined" && invoice.contact.email != null ? invoice.contact.email.data : ""];
            message.subject = '';
            message.body = invoice.description;
            message.AddDocumentsForSave([invoice.fileId]);

            ASC.Mail.Utility.Unbind(ASC.Mail.Utility.Events.OnSuccess);
            ASC.Mail.Utility.Bind(ASC.Mail.Utility.Events.OnSuccess, function (event, data) {
                LoadingBanner.hideLoading();
                if (newTab && newTab.location) {
                    newTab.location.href = data.messageUrl;
                } else {
                    window.location.href = data.messageUrl;
                }
            });

            ASC.Mail.Utility.Unbind(ASC.Mail.Utility.Events.OnError);
            ASC.Mail.Utility.Bind(ASC.Mail.Utility.Events.OnError, function (event, message) {
                console.log(event, message);
                LoadingBanner.hideLoading();
            });

            ASC.Mail.Utility.Unbind(ASC.Mail.Utility.Events.OnNoAccounts);
            ASC.Mail.Utility.Bind(ASC.Mail.Utility.Events.OnNoAccounts, function (event, message) {
                if (newTab) {
                    newTab.close();
                }
                if (jq("#noMailAccountsError").length == 0) {
                    jq.tmpl("blockUIPanelTemplate", {
                        id: "noMailAccountsError",
                        headerTest: ASC.CRM.Resources.CRMCommonResource.Alert,
                        questionText: "",
                        innerHtmlText: ['<div>', ASC.CRM.Resources.CRMCommonResource.NoMailAccountsAvailableError, '</div>'].join(''),
                        CancelBtn: ASC.CRM.Resources.CRMCommonResource.Close,
                        OKBtn: ASC.CRM.Resources.CRMCommonResource.AddMailAccount,
                        OKBtnClass: "OKAddMailAccount"
                    }).appendTo(".mainContainerClass .containerBodyBlock");

                    jq("#noMailAccountsError").on("click", ".OKAddMailAccount", function () {
                        window.open(ASC.CRM.Common.getMailModuleBasePath(), "_blank");
                        jq.unblockUI();
                    });
                }
                LoadingBanner.hideLoading();
                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#noMailAccountsError", 500, 200, 0);
            });

            ASC.Mail.Utility.SaveMessageInDrafts(message);

        }
    }
})();

ASC.CRM.PrivatePanel = (function() {
    return {
        changeIsPrivateCheckBox: function () {
            if (jq("#isPrivate").is(":checked")) {
                jq("#privateSettingsBlock").show();
            } else {
                jq("#privateSettingsBlock").hide();
            }
        },
        init: function (showNotifyPanel, notifyLabel, usersWhoHasAccess, userIDs, disabledUserIDs) {
            ASC.CRM.UserSelectorListView.Init(
                    "",
                    "UserSelectorListView",
                    showNotifyPanel,
                    notifyLabel,
                    usersWhoHasAccess,
                    userIDs,
                    disabledUserIDs,
                    "#privateSettingsBlock",
                    true);
            if (jq("#privateSettingsBlock").is(":visible")) {
                jq("#isPrivate").prop("checked", true);
            } else {
                jq("#isPrivate").prop("checked", false);
            }
        }
    };
})();


ASC.CRM.HistoryView = (function() {
    function onGetException(params, errors) {
        console.log('common.js ', errors);
        LoadingBanner.hideLoading();
    };

    var _changeItem = function(item) {
        jq("#itemID").val(item.id);
    };

    var _renderContent = function() {
        if (ASC.CRM.HistoryView.isFirstTime == true) {
            ASC.CRM.HistoryView.isFirstTime = false;
            return;
        }
        LoadingBanner.displayLoading();
        ASC.CRM.HistoryView.ShowMore = true;
        _getEvents(0);
    };

    var _addRecordsToContent = function() {
        if (!ASC.CRM.HistoryView.ShowMore) { return false; }

        jq("#showMoreEventsButtons .crm-showMoreLink").hide();
        jq("#showMoreEventsButtons .loading-link").show();

        _getEvents(jq("#eventsTable tr").length);
    };

    var _getEvents = function(startIndex) {
        var filterSettings = {};
        filterSettings = ASC.CRM.HistoryView.getFilterSettings();

        filterSettings.entityid = ASC.CRM.HistoryView.ContactID != 0 ? ASC.CRM.HistoryView.ContactID : ASC.CRM.HistoryView.EntityID;
        filterSettings.entitytype = ASC.CRM.ListTaskView.EntityType;
        filterSettings.Count = ASC.CRM.HistoryView.CountOfRows;

        filterSettings.startIndex = startIndex;

        Teamlab.getCrmHistoryEvents({ startIndex: startIndex || 0 }, { filter: filterSettings, success: ASC.CRM.HistoryView.CallbackMethods.get_events_by_filter });
    };

    var _readDataEvent = function () {
        var content = jq("#historyBlock textarea").val();
        if (CKEDITOR.instances.historyCKEditor) {
            content = CKEDITOR.instances.historyCKEditor.getData();
        }
        var _contactID, _entityType, _entityID,
            dataEvent = {
                content: content,
                categoryId: eventCategorySelector.CategoryID
            };
        if (ASC.CRM.HistoryView.ContactID != 0) {
            dataEvent.contactID = ASC.CRM.HistoryView.ContactID;
            if (jq("#itemID").val() != "" && parseInt(jq("#itemID").val()) > 0 && jq("#typeID").val() != "") {
                dataEvent.entityId = jq("#itemID").val();
                dataEvent.entityType = jq("#typeID").val();
            }
        } else {
            if (jq("#contactID").val() != 0) dataEvent.contactID = jq("#contactID").val();
            dataEvent.entityId = ASC.CRM.HistoryView.EntityID;
            dataEvent.entityType = ASC.CRM.HistoryView.EntityType;
        }

        if (jq.trim(jq("#historyBlock .textEditCalendar").val()) != "") {
            dataEvent.created = jq("#historyBlock .textEditCalendar").datepicker('getDate');
            var now = new Date();
            dataEvent.created.setHours(now.getHours(), now.getMinutes(), now.getSeconds());

        } else {
            dataEvent.created = new Date();
            jq("#historyBlock .textEditCalendar").datepicker('setDate', dataEvent.created);
        }

        if (window.SelectedUsers_HistoryUserSelector.IDs.length != 0) {
            dataEvent.notifyUserList = window.SelectedUsers_HistoryUserSelector.IDs;
        }
        return dataEvent;
    };

    var _resizeFilter = function() {
        var visible = jq("#eventsFilterContainer").is(":hidden") == false;
        if (ASC.CRM.HistoryView.isFilterVisible == false && visible) {
            ASC.CRM.HistoryView.isFilterVisible = true;
            if (ASC.CRM.HistoryView.advansedFilter) {
                jq("#eventsAdvansedFilter").advansedFilter("resize");
            }
        }
    };

    var _fillEventToEntityItems = function() {
        var entityTypeOpportunity = 1,
            entityTypeCase = 7,
            dealsFilters = {
                contactID: ASC.CRM.HistoryView.ContactID,
                contactAlsoIsParticipant: true
            },
            casesFilters = {
                contactID: ASC.CRM.HistoryView.ContactID
            };

        Teamlab.joint()
            .getCrmOpportunities({}, { filter: dealsFilters })
            .getCrmCases({}, { filter: casesFilters })
            .start({}, {
                success: function(params, opportunities, cases) {
                    jq("#eventLinkToPanel").replaceWith(jq.tmpl("eventLinkToPanelTmpl",
                    {
                        contactID: ASC.CRM.HistoryView.ContactID,
                        entityTypeOpportunity: entityTypeOpportunity,
                        entityTypeCase: entityTypeCase,
                        deals: opportunities,
                        cases: cases
                    }));

                    jq("#typeSwitcherSelect")
                     .val(-2)
                     .change(function (evt) {
                         switch (this.value) {
                             case "-2":
                                 ASC.CRM.HistoryView.changeType(-2, '');
                                 break;
                             case "-1":
                                 jq(this).val(-2).change();
                                 break;
                             default:
                                 for (var i = 0, n = ASC.CRM.Data.historyEntityTypes.length; i < n; i++) {
                                     if (this.value == ASC.CRM.Data.historyEntityTypes[i].value) {
                                         ASC.CRM.HistoryView.changeType(ASC.CRM.Data.historyEntityTypes[i].value, ASC.CRM.Data.historyEntityTypes[i].apiname);
                                         break;
                                     }
                                 }
                                 break;
                         }
                     })
                     .tlCombobox();

                    jq("#dealsSwitcherSelect")
                        .show()
                        .val(-2)
                        .change(function (evt) {
                            _changeItem({ id: this.value });
                        })
                        .tlCombobox().tlCombobox("hide");

                    jq("#casesSwitcherSelect")
                        .show()
                        .val(-2)
                        .change(function (evt) {
                            _changeItem({ id: this.value });
                        })
                        .tlCombobox().tlCombobox("hide");

                    if (!(params[0].__count > 0)) {
                        jq("#typeSwitcherSelect>option[value='" + entityTypeOpportunity + "']:not(.hidden)").addClass("hidden");
                        jq("#typeSwitcherSelect").tlCombobox();
                    }

                    if (!(params[1].__count > 0)) {
                        jq("#typeSwitcherSelect>option[value='" + entityTypeCase + "']:not(.hidden)").addClass("hidden");
                        jq("#typeSwitcherSelect").tlCombobox();
                    }

                    if (params[0].__count > 0 || params[1].__count > 0) {
                        jq("#eventLinkToPanel").removeClass("empty-select");
                    } else {
                        jq("#eventLinkToPanel").addClass("empty-select");
                    }
                }
            });
    };

    var _fillEventToEntityContacts = function () {
        Teamlab.getCrmEntityMembers({
            showCompanyLink: true,
            showUnlinkBtn: false,
            showActionMenu: true
        },
        ASC.CRM.HistoryView.EntityType,
        ASC.CRM.HistoryView.EntityID,
        {
            success: function (params, contacts) {
                jq("#eventLinkToPanel").replaceWith(jq.tmpl("eventLinkToPanelTmpl",
                {
                    contactID: 0,
                    contacts: contacts
                }));

                jq("#typeSwitcherSelect")
                     .val(-2)
                     .change(function (evt) {
                         switch (this.value) {
                             case "-2":
                                 ASC.CRM.HistoryView.changeType(-2, '');
                                 break;
                             case "-1":
                                 jq(this).val(-2).change();
                                 break;
                             default:
                                 for (var i = 0, n = ASC.CRM.Data.historyEntityTypes.length; i < n; i++) {
                                     if (this.value == ASC.CRM.Data.historyEntityTypes[i].value) {
                                         ASC.CRM.HistoryView.changeType(ASC.CRM.Data.historyEntityTypes[i].value, ASC.CRM.Data.historyEntityTypes[i].apiname);
                                         break;
                                     }
                                 }
                                 break;
                         }
                     })
                     .tlCombobox();

                jq("#contactSwitcherSelect")
                       .val(-2)
                       .change(function (evt) {
                           switch (this.value) {
                               case "-2":
                                   ASC.CRM.HistoryView.changeContact(-1);
                                   break;
                               case "-1":
                                   ASC.CRM.HistoryView.changeContact(-1);
                                   jq(this).val(-2).change();
                                   break;
                               default:
                                   ASC.CRM.HistoryView.changeContact(this.value);
                                   break;
                           }
                       })
                       .tlCombobox();

                        if (params.__count > 0) {
                            jq("#eventLinkToPanel").removeClass("empty-select");
                        } else {
                            jq("#eventLinkToPanel").addClass("empty-select");
                        }
            }
        });
    };

    var _initEmptyScreen = function(){
        jq.tmpl("emptyScrTmpl",
            { ID: "emptyContentForEventsFilter",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_filter"],
                Header: ASC.CRM.Resources.CRMCommonResource.EmptyHistoryHeader,
                Describe: ASC.CRM.Resources.CRMCommonResource.EmptyHistoryDescription,
                ButtonHTML: ["<a class='clearFilterButton link dotline' href='javascript:void(0);' ",
                        "onclick='ASC.CRM.HistoryView.advansedFilter.advansedFilter(null);'>",
                        ASC.CRM.Resources.CRMCommonResource.ClearFilter,
                        "</a>"
                ].join(''),
                CssClass: "display-none"
            }).insertAfter("#eventsList");
    };

    var _initFileUploader = function() {
        ASC.CRM.FileUploader.OnBeginCallback_function = function () {
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.AddThisNoteProggress;
            LoadingBanner.showLoaderBtn("#historyBlock");
            jq("#historyBlock textarea, #historyBlock input, #historyBlock select").attr("disabled", true);
            jq(".attachLink").addClass("disabledLink");
        };

        ASC.CRM.FileUploader.OnAllUploadCompleteCallback_function = function() {
            var data = _readDataEvent();
            data.fileId = ASC.CRM.FileUploader.fileIDs;

            Teamlab.addCrmHistoryEvent({}, data,
                {
                    success: ASC.CRM.HistoryView.CallbackMethods.add_event,
                    after: function (params) {
                        LoadingBanner.hideLoaderBtn("#historyBlock");
                        jq("#historyBlock .middle-button-container a.button.blue.middle").addClass("disable");                        
                        jq("#historyBlock textarea, #historyBlock input, #historyBlock select").attr("disabled", false);
                        jq(".attachLink").removeClass("disabledLink");
                    }
                });
        };

        ASC.CRM.Common.registerChangeHoverStateByParent("label.event_category", "#eventsTable>tbody>tr");

        ASC.CRM.FileUploader.activateUploader();
        ASC.CRM.FileUploader.fileIDs.clear();

        //Teamlab.bind(Teamlab.events.getException, onGetException);
    };

    var _initEventCategorySelector = function () {
        var helpInfoText = ASC.CRM.Data.IsCRMAdmin ? jq.format(ASC.CRM.Resources.CRMCommonResource.HistoryCategoriesHelpInfo,
            "<a class='linkAction' href='settings.aspx?type=history_category' target='blank'>",
            "</a>") : "",
            selectedCategory = {};

        if (ASC.CRM.Data.eventsCategories.length > 0) {
            selectedCategory = ASC.CRM.Data.eventsCategories[0];
        } else {
            selectedCategory = { id: 0, title: "", imgSrc: "" };
        }

        window.eventCategorySelector = new ASC.CRM.CategorySelector("eventCategorySelector", selectedCategory);
        eventCategorySelector.renderControl(ASC.CRM.Data.eventsCategories, selectedCategory, "#categorySelectorContainer", 160, helpInfoText);
    };

    var _getHistoryEventByID = function (id) {
        for (var i = 0, n = ASC.CRM.HistoryView.EventsList.length; i < n; i++) {
            if (ASC.CRM.HistoryView.EventsList[i].id == id) {
                return ASC.CRM.HistoryView.EventsList[i];
            }
        }
        return null;
    };

    var _deleteHistoryEventByID = function (id) {
        for (var i = 0, n = ASC.CRM.HistoryView.EventsList.length; i < n; i++) {
            if (ASC.CRM.HistoryView.EventsList[i].id == id) {
                ASC.CRM.HistoryView.EventsList.splice(i, 1);
                return;
            }
        }
    };

    var _initFilter = function () {
        if (!jq("#eventsAdvansedFilter").advansedFilter) return;

        var tmpDate = new Date(),
            today = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0),
            yesterday = new Date(new Date(today).setDate(tmpDate.getDate() - 1)),

            todayString = Teamlab.serializeTimestamp(today),
            yesterdayString = Teamlab.serializeTimestamp(yesterday);

        ASC.CRM.HistoryView.advansedFilter = jq("#eventsAdvansedFilter")
                .advansedFilter({
                    anykey: false,
                    hint: ASC.CRM.Resources.CRMCommonResource.AdvansedFilterInfoText.format(
                                '<b>',
                                '</b>',
                                '<br/><br/><a href="' + ASC.Resources.Master.FilterHelpCenterLink + '" target="_blank">',
                                '</a>'),
                    maxfilters: 3,
                    maxlength: "100",
                    store: false,
                    filters: [
                                {
                                    type: "flag",
                                    id: "my",
                                    apiparamname: "createBy",
                                    title: ASC.CRM.Resources.CRMCommonResource.MyEventsFilter,
                                    group: ASC.CRM.Resources.CRMCommonResource.FilterByResponsible,
                                    groupby: "responsible",
                                    defaultparams: { value: Teamlab.profile.id }
                                },
                                {
                                    type: "person",
                                    id: "responsibleID",
                                    apiparamname: "createBy",
                                    title: ASC.CRM.Resources.CRMCommonResource.CustomResponsibleFilter,
                                    group: ASC.CRM.Resources.CRMCommonResource.FilterByResponsible,
                                    groupby: "responsible",
                                    filtertitle: ASC.CRM.Resources.CRMCommonResource.FilterByResponsible
                                },
                                {
                                    type: "combobox",
                                    id: "today",
                                    apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                    title: ASC.CRM.Resources.CRMCommonResource.Today,
                                    filtertitle: ASC.CRM.Resources.CRMCommonResource.Date,
                                    group: ASC.CRM.Resources.CRMCommonResource.Date,
                                    groupby: "date",
                                    options:
                                            [
                                            { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today, def: true },
                                            { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday }
                                            ]
                                },
                                {
                                    type: "combobox",
                                    id: "yesterday",
                                    apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                    title: ASC.CRM.Resources.CRMCommonResource.Yesterday,
                                    filtertitle: ASC.CRM.Resources.CRMCommonResource.Date,
                                    group: ASC.CRM.Resources.CRMCommonResource.Date,
                                    groupby: "date",
                                    options:
                                            [
                                            { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                            { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday, def: true }
                                            ]
                                },
                                {
                                    type: "daterange",
                                    id: "fromToDate",
                                    title: ASC.CRM.Resources.CRMCommonResource.CustomDateFilter,
                                    group: ASC.CRM.Resources.CRMCommonResource.Date,
                                    groupby: "date",
                                    filtertitle: ASC.CRM.Resources.CRMCommonResource.Date
                                },
                                {
                                    type: "combobox",
                                    id: "categoryID",
                                    apiparamname: "categoryID",
                                    title: ASC.CRM.Resources.CRMCommonResource.ByCategory,
                                    group: ASC.CRM.Resources.CRMCommonResource.Other,
                                    options: jq.merge(jq.merge([], ASC.CRM.Data.eventsCategories), ASC.CRM.Data.systemCategories),
                                    defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose,
                                    defaultvalue: "0"
                                }
                    ],
                    sorters: [
                                { id: "created", title: ASC.CRM.Resources.CRMCommonResource.Date, dsc: true, def: true },
                                { id: "createby", title: ASC.CRM.Resources.CRMCommonResource.Author, dsc: false, def: false },
                                { id: "content", title: ASC.CRM.Resources.CRMCommonResource.Content, dsc: false, def: false },
                                { id: "category", title: ASC.CRM.Resources.CRMCommonResource.Category, dsc: false, def: false }
                    ]
                })
                .bind("setfilter", ASC.CRM.HistoryView.setFilter)
                .bind("resetfilter", ASC.CRM.HistoryView.resetFilter);
    };

    var _initUserSelectorForNotify = function () {

        if (typeof (window.UserList_HistoryUserSelector) != "undefined" && window.UserList_HistoryUserSelector != null) {
            var adminsGuids = [];
            for (var i = 0, n = ASC.CRM.Data.crmAdminList.length; i < n; i++) {
                adminsGuids.push(ASC.CRM.Data.crmAdminList[i].id);
            }
            window.UserList_HistoryUserSelector = ASC.CRM.Common.arrayUnique(window.UserList_HistoryUserSelector.concat(adminsGuids));
            for (var i = 0, n = window.UserList_HistoryUserSelector.length; i < n; i++) {
                if (window.UserList_HistoryUserSelector[i] === Teamlab.profile.id) {
                    window.UserList_HistoryUserSelector.splice(i, 1);
                    break;
                }
            }
        } else {
            window.UserList_HistoryUserSelector = null;
        }

        ASC.CRM.UserSelectorListView.Init("_HistoryUserSelector",
                                          "HistoryUserSelector",
                                          false,
                                          "",
                                          [],
                                          window.UserList_HistoryUserSelector,
                                          new Array(Teamlab.profile.id),
                                          "#historyViewUserSelector",
                                          false);

        jq("#selectedUsers_HistoryUserSelector").appendTo("#selectedUsers_HistoryUserSelector_Container");

        if (window.UserList_HistoryUserSelector != null && window.UserList_HistoryUserSelector.length == 0) {
            jq(".historyViewUserSelectorCont").hide();
        }

    };

    return {
        CallbackMethods: {
            get_events_by_filter: function(params, events) {

                if (typeof (params.__nextIndex) == "undefined") {
                    ASC.CRM.HistoryView.ShowMore = false;
                }
                if (params.startIndex === 0) {
                    ASC.CRM.HistoryView.EventsList = [];
                }

                for (var i = 0, n = events.length; i < n; i++) {
                    ASC.CRM.HistoryView.eventItemFactory(events[i]);
                    ASC.CRM.HistoryView.EventsList.push(events[i]);
                }

                if (params.startIndex === 0) {
                    jq("#eventsTable tbody tr").remove();

                    if (events.length == 0) {
                        if (typeof (ASC.CRM.HistoryView.advansedFilter) == "undefined"
                            || ASC.CRM.HistoryView.advansedFilter.advansedFilter().length == 1) {
                            jq("#eventsList").hide();
                            jq("#eventsFilterContainer").hide();
                            jq("#emptyContentForEventsFilter:not(.display-none)").addClass("display-none");
                            ASC.CRM.HistoryView.ShowMore = false;
                            LoadingBanner.hideLoading();
                            return false;
                        } else {
                            jq("#eventsList").hide();
                            jq("#eventsFilterContainer").show();
                            jq("#emptyContentForEventsFilter.display-none").removeClass("display-none");
                            LoadingBanner.hideLoading();
                            return false;
                        }
                    }

                    jq.tmpl("historyRowTmpl", events).appendTo("#eventsTable tbody");

                    jq("#emptyContentForEventsFilter:not(.display-none)").addClass("display-none");
                    jq("#eventsFilterContainer").show();
                    jq("#eventsList").show();
                    _resizeFilter();

                    LoadingBanner.hideLoading();
                } else {
                    if (events.length == 0 && jq("#eventsTable tbody tr").length == 0) {
                        jq("#eventsList").hide();
                        jq("#eventsFilterContainer").show();
                        jq("#emptyContentForEventsFilter.display-none)").removeClass("display-none");
                        LoadingBanner.hideLoading();
                        return false;
                    }
                    jq.tmpl("historyRowTmpl", events).appendTo("#eventsTable tbody");
                }
                for (var i = 0, n = events.length; i < n; i++) {
                    if (events[i].files != null) {
                        jq.dropdownToggle({
                            dropdownID: "eventAttach_" + events[i].id,
                            switcherSelector: "#eventAttachSwither_" + events[i].id,
                            addTop: 5,
                            rightPos: true
                        });
                    }
                }

                jq("#showMoreEventsButtons .loading-link").hide();
                if (ASC.CRM.HistoryView.ShowMore) {
                    jq("#showMoreEventsButtons .crm-showMoreLink").show();
                } else {
                    jq("#showMoreEventsButtons .crm-showMoreLink").hide();
                }
            },

            add_event: function(params, event) {
                ASC.CRM.HistoryView.addEventToHistoryLayout(params, event);
                ASC.CRM.HistoryView.EventsList.unshift(event);
                if (event.files != null) {
                    if (typeof (window.Attachments) != "undefined") {
                        window.Attachments.appendFilesToLayout(event.files);
                    }
                }
            },

            delete_event: function(params, event) {
                jq("#eventAttach_" + event.id).find("div[id^=fileContent_]").each(function() {
                    var fileID = jq(this).attr("id").split("_")[1];
                    if (typeof (window.Attachments) != "undefined") {
                        window.Attachments.deleteFileFromLayout(fileID);
                    }
                });
                _deleteHistoryEventByID(event.id);

                jq("#event_" + event.id).animate({ opacity: "hide" }, 500);
                setTimeout(function() {
                    jq("#event_" + event.id).remove();
                    if (jq("#eventsTable tr").length == 0) {
                        if (typeof (ASC.CRM.HistoryView.advansedFilter) != "undefined"
                            && ASC.CRM.HistoryView.advansedFilter.advansedFilter().length == 1) {
                            jq("#eventsFilterContainer").hide();
                            jq("#eventsList").hide();
                        } else {
                            _renderContent();
                        }
                    }
                }, 500);
            },

            delete_file_from_event: function(params, file) {
                if (typeof (window.Attachments) != "undefined") {
                    window.Attachments.deleteFileFromLayout(file.id);
                }
                ASC.CRM.HistoryView.deleteFileFromEventLayout(file.id, params.messageID);
            }
        },


        init: function(contactID, entityType, entityID) {
            ASC.CRM.HistoryView.ContactID = contactID;
            ASC.CRM.HistoryView.EntityID = entityID;
            ASC.CRM.HistoryView.EntityType = entityType;

            ASC.CRM.HistoryView.CountOfRows = ASC.CRM.Data.DefaultEntryCountOnPage;

            ASC.CRM.HistoryView.EventsList = [];
            ASC.CRM.HistoryView.ShowMore = true;
            ASC.CRM.HistoryView.isFilterVisible = false;
            ASC.CRM.HistoryView.isTabActive = false;
            ASC.CRM.HistoryView.isFirstTime = true;

            //init emptyScreen for filter
            _initEmptyScreen();

            _initUserSelectorForNotify();

            jq("#showMoreEventsButtons .crm-showMoreLink").bind("click", function() {
                _addRecordsToContent();
            });

            jq("#DepsAndUsersContainer").css("z-index", 999);

            jq("#historyBlock input.textEditCalendar").mask(ASC.Resources.Master.DatePatternJQ);
            jq("#historyBlock input.textEditCalendar").datepickerWithButton().val(window.historyView_dateTimeNowShortDateString);

            jq("#historyBlock table #selectedUsers").remove();

            if (!jq.browser.mobile) {
                _initFileUploader();
            }

            _initEventCategorySelector();

            setInterval(function() {
                var text = jq.trim(jq("#historyCKEditor").val()),
                    $input = jq("#contactProfileEdit .info_for_company input[name=baseInfo_companyName]");
                if(CKEDITOR.instances.historyCKEditor){
                    text = jq.trim(CKEDITOR.instances.historyCKEditor.getData());
                }
                if (text == "") {
                    jq("#historyBlock .middle-button-container a.button.blue.middle").addClass("disable");
                } else {
                    jq("#historyBlock .middle-button-container a.button.blue.middle.disable").removeClass("disable");
                }
            }, 500);

            _initFilter();
        },

        setFilter: function(evt, $container, filter, params, selectedfilters) { _renderContent(); },
        resetFilter: function(evt, $container, filter, selectedfilters) { _renderContent(); },

        activate: function() {
            if (ASC.CRM.HistoryView.isTabActive == false) {
                ASC.CRM.HistoryView.isTabActive = true;
                LoadingBanner.displayLoading();
                if (ASC.CRM.HistoryView.ContactID > 0) {
                    _fillEventToEntityItems();
                } else {
                    _fillEventToEntityContacts();
                }
                _renderContent();
            } else if (ASC.CRM.HistoryView.advansedFilter) {
                jq("#eventsAdvansedFilter").advansedFilter("resize");
            }
        },

        getFilterSettings: function() {
            var settings = {};

            if (ASC.CRM.HistoryView.advansedFilter.advansedFilter == null) return settings;

            var param = ASC.CRM.HistoryView.advansedFilter.advansedFilter();

            jq(param).each(function(i, item) {
                switch (item.id) {
                    case "sorter":
                        settings.sortBy = item.params.id;
                        settings.sortOrder = item.params.dsc == true ? 'descending' : 'ascending';
                        break;
                    case "text":
                        settings.filterValue = item.params.value;
                        break;
                    case "fromToDate":
                        settings.fromDate = new Date(item.params.from);
                        settings.toDate = new Date(item.params.to);
                        break;
                    default:
                        if (item.hasOwnProperty("apiparamname") && item.params.hasOwnProperty("value") && item.params.value != null) {
                            try {
                                var apiparamnames = jq.parseJSON(item.apiparamname),
                                    apiparamvalues = jq.parseJSON(item.params.value);
                                if (apiparamnames.length != apiparamvalues.length) {
                                    settings[item.apiparamname] = item.params.value;
                                }
                                for (var i = 0, len = apiparamnames.length; i < len; i++) {
                                    if (jq.trim(apiparamvalues[i]).length != 0) {
                                        settings[apiparamnames[i]] = apiparamvalues[i];
                                    }
                                }
                            } catch (err) {
                                settings[item.apiparamname] = item.params.value;
                            }
                        }
                        break;
                }
            });
            return settings;
        },

        eventItemFactory: function(eventItem) {
            if (eventItem.entity != null) {
                switch (eventItem.entity.entityType) {
                    case "opportunity":
                        eventItem.entityURL = "deals.aspx?id=" + eventItem.entity.entityId;
                        eventItem.entityType = ASC.CRM.Resources.CRMJSResource.Deal;
                        break;
                    case "case":
                        eventItem.entityURL = "cases.aspx?id=" + eventItem.entity.entityId;
                        eventItem.entityType = ASC.CRM.Resources.CRMJSResource.Case;
                        break;
                    default:
                        eventItem.entityURL = "";
                        eventItem.entityType = "";
                        break;
                }
            }

            eventItem.category.cssClass = eventItem.category.imagePath.split('/')[eventItem.category.imagePath.split('/').length - 1].split('.')[0];
            if (eventItem.category.cssClass == "32-attach") {
                eventItem.category.cssClass = "event_category_attach_file";
            }

            if (eventItem.category.id == -3) {// System event category for mails
                try {
                    ASC.CRM.Common.historyEventMsgObjFactory(eventItem);

                    eventItem.createdDate = eventItem.message.date_created;
                    eventItem.createdBy.displayName = eventItem.message.from;

                    eventItem.category.cssClass = eventItem.message.is_sended ? "event_category_system_email_out" : "event_category_system_email_in";
                }
                catch (e) {}
            }
        },

        deleteFile: function(fileID, messageID) {
            Teamlab.removeCrmFile({ messageID: messageID }, fileID, {
                success: ASC.CRM.HistoryView.CallbackMethods.delete_file_from_event
            });
        },

        deleteFileFromEventLayout: function(fileID, messageID) {
            jq("#fileContent_" + fileID).remove();
            if (jq("#eventAttach_" + messageID + ">ul.dropdown-content>li").length == 0) {
                jq(jq("#eventAttach_" + messageID).parent()).html("");
            }
        },

        showAttachmentPanel: function(showPanel) {
            if (showPanel) {
                if (jq("#attachShowButton").hasClass('disabledLink')) {
                    return false;
                }
                jq('#attachOptions').show();
                jq("#attachShowButton").hide();
                jq("#attachHideButton").show();
            } else {
                if (jq("#attachHideButton").hasClass('disabledLink')) {
                    return false;
                }
                jq('#attachOptions').hide();
                jq("#attachHideButton").hide();
                jq("#attachShowButton").show();
            }
        },

        addEvent: function() {
            var text = jq("#historyBlock textarea").val();
            if (CKEDITOR.instances.historyCKEditor) {
                text = CKEDITOR.instances.historyCKEditor.getData();
            }
            if (jq.trim(text) == "") return;

            if (!jq.browser.mobile && ASC.CRM.FileUploader.getUploadFileCount() > 0) {
                ASC.CRM.FileUploader.start();
            } else {
                var data = _readDataEvent();
                Teamlab.addCrmHistoryEvent({}, data,
                {
                    success: ASC.CRM.HistoryView.CallbackMethods.add_event,
                    before: function(params) {
                        LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.AddThisNoteProggress;
                        LoadingBanner.showLoaderBtn("#historyBlock");
                        jq("#historyBlock textarea, #historyBlock input, #historyBlock select").attr("disabled", true);
                    },
                    after: function (params) {
                        LoadingBanner.hideLoaderBtn("#historyBlock");
                        jq("#historyBlock .middle-button-container a.button.blue.middle").addClass("disable");
                        jq("#historyBlock textarea, #historyBlock input, #historyBlock select").attr("disabled", false);
                    }
                });
            }
        },

        addEventToHistoryLayout: function(params, event) {
            ASC.CRM.HistoryView.eventItemFactory(event);

            if (CKEDITOR.instances.historyCKEditor) {
                CKEDITOR.instances.historyCKEditor.setData("");
                CKEDITOR.instances.historyCKEditor.focus();
            } else {
                jq("#historyBlock textarea").val("");
                jq("#historyBlock textarea").focus();
            }

            jq("#contactSwitcherSelect").val(-2).change();
            jq("#typeSwitcherSelect").val(-2).change();
            ASC.CRM.HistoryView.changeType(-1, '');
            ASC.CRM.HistoryView.changeContact(-1);

            jq("#usrSrListViewAdvUsrSrContainer_HistoryUserSelector .addUserLink").useradvancedSelector("undisable", window.SelectedUsers_HistoryUserSelector.IDs);
            window.SelectedUsers_HistoryUserSelector.IDs.clear();
            window.SelectedUsers_HistoryUserSelector.Names.clear();
            jq("#selectedUsers_HistoryUserSelector div").remove();

            if (!jq.browser.mobile && !params.fromAttachmentsControl) {
                ASC.CRM.FileUploader.reinit();
                ASC.CRM.HistoryView.showAttachmentPanel(false);
            }

            if (jq("#eventsTable tbody tr").length == 0) {
                jq("#emptyContentForEventsFilter:not(.display-none)").addClass("display-none");
                jq("#eventsFilterContainer").show();
                jq("#eventsList").show();
                _resizeFilter();
            }
            jq.tmpl("historyRowTmpl", event).prependTo("#eventsTable tbody");

            if (event.files != null)
                jq.dropdownToggle({
                    dropdownID: "eventAttach_" + event.id,
                    switcherSelector: "#eventAttachSwither_" + event.id,
                    addTop: 5,
                    rightPos: true
                });
        },

        deleteEvent: function(id) {
            Teamlab.removeCrmHistoryEvent({}, id,
                {
                    success: ASC.CRM.HistoryView.CallbackMethods.delete_event,
                    before: function(params) { jq("#eventTrashImg_" + id).hide(); jq("#eventLoaderImg_" + id).show(); },
                    after: function (params) { jq("#eventLoaderImg_" + id).hide(); jq("#eventTrashImg_" + id).show(); }
                });
        },


        changeType: function(type, stringType) {
            jq("#typeID").val(stringType);
            jq("#itemID").val(-1);

            switch (type) {
                case 1: //opportunity
                    jq("#casesSwitcherSelect").tlcombobox('hide');
                    jq("#casesSwitcherSelect").val(-2).change();
                    jq("#dealsSwitcherSelect").tlcombobox('show');
                    jq("#typeSwitcherSelect>option[value='-1'].hidden").removeClass("hidden");
                    jq("#typeSwitcherSelect").tlCombobox();
                    break;
                case 7: //cases
                    jq("#dealsSwitcherSelect").tlcombobox('hide');
                    jq("#dealsSwitcherSelect").val(-2).change();
                    jq("#casesSwitcherSelect").tlcombobox('show');
                    jq("#typeSwitcherSelect>option[value='-1'].hidden").removeClass("hidden");
                    jq("#typeSwitcherSelect").tlCombobox();
                    break;
                default:
                    jq("#dealsSwitcherSelect").tlcombobox('hide');
                    jq("#casesSwitcherSelect").tlcombobox('hide');
                    jq("#dealsSwitcherSelect").val(-2).change();
                    jq("#casesSwitcherSelect").val(-2).change();
                    jq("#typeSwitcherSelect>option[value='-1']:not(.hidden)").addClass("hidden");
                    jq("#typeSwitcherSelect").tlCombobox();
                    break;
            }
        },

        changeContact: function(contactID) {
            jq("#contactID").val(contactID);

            if (contactID > 0) {
                jq("#contactSwitcherSelect>option[value='-1'].hidden").removeClass("hidden");
                jq("#contactSwitcherSelect").tlCombobox();
            } else {
                jq("#contactSwitcherSelect>option[value='-1']:not(.hidden)").addClass("hidden");
                jq("#contactSwitcherSelect").tlCombobox();
            }
        },

        appendOption: function($select, option) {
            var $tlcombobox = $select.parents('span.tl-combobox:first');
            $tlcombobox = $tlcombobox.length > 0 ? $tlcombobox : $select;
            $tlcombobox.parent().removeClass('empty-select');
            jq(document.createElement('option')).attr('value', option.value).text(option.title).appendTo($select);
            $select.tlCombobox();
            return $select;
        },

        removeOption: function($select, value) {
            $select.find('option[value="' + value + '"]').remove();
            $select.tlCombobox();
            var 
                value = -1,
                $options = $select.find('option'),
                optionsInd = $options.length;
            while (optionsInd--) {
                value = jq($options[optionsInd]).attr('value');
                value = isFinite(+value) ? +value : -1;
                if (value > 0) {
                    break;
                }
            }
            if (optionsInd === -1) {
                var $tlcombobox = $select.parents('span.tl-combobox:first');
                $tlcombobox = $tlcombobox.length > 0 ? $tlcombobox : $select;
                $tlcombobox.parent().addClass('empty-select');
            }
            return $select;
        },

        appendOptionToContact: function(option) {
            return ASC.CRM.HistoryView.appendOption(jq('#contactSwitcherSelect'), option);
        },

        removeOptionFromContact: function(value) {
            return ASC.CRM.HistoryView.removeOption(jq('#contactSwitcherSelect'), value);
        }
    };
})();


ASC.CRM.HistoryMailView = (function () {

    var createBodyIFrame = function (message) {
        var $frame = jq('<iframe id="message_body_frame_' + message.id +
        '" scrolling="auto" frameborder="0" width="100%" style="height:100%;">An iframe capable browser is required to view this web site.</iframe>');

        $frame.bind('load', function () {
            $frame.unbind('load');
            var $body = jq(this).contents().find('body');

            $body.css('cssText', 'height: 0;');
            message.html_body = '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">' + message.html_body;
            $body.html(message.html_body);
            $body.find("a").attr('target', '_blank');

            var $blockquote = jq($body.find('div.gmail_quote:first')[0] || $body.find('blockquote:first')[0]);

            if ($blockquote) {
                $blockquote.wrap('<div class="tmmail_quote"/>');

                var $wrap_block_quote = $body.find('div.tmmail_quote:first');

                if ($wrap_block_quote) {
                    $wrap_block_quote.before(
                        '<style>' +
                            '#controll-blockquote {width: 12px; display: inline-block; background-color: #F2F2F2; border: 1px solid #D7D8DC; cursor: pointer; padding: 0 5px; margin: 10px 0;}' +
                            '#controll-blockquote:hover {background-color: #D7D8DC; border: 1px solid #666;}' +
                            '#controll-blockquote div {margin-top: -8px; font-weight: bold; color: #D7D8DC; -moz-user-select: none; -webkit-user-select: none; -ms-user-select: none;}' +
                            '#controll-blockquote:hover div {color: #666;}' +
                            '</style>' +
                            '<div id="controll-blockquote"><div>...</div></div>');

                    $wrap_block_quote.hide();

                    var $btn_blockquote = $body.find('#controll-blockquote');
                    if ($btn_blockquote) {
                        $btn_blockquote.click(function () {
                            $wrap_block_quote.toggle();
                            var iframe = jq('#message_body_frame_' + message.id);
                            iframe.attr('scrolling', 'no');
                            setTimeout(function () {
                                iframe.attr('scrolling', 'auto');
                                _updateIframeSize(message.id);
                            }, 1);
                        });
                    }
                }
            }

            var index = $body.find("img").length;
            $body.find('img').each(function () {
                jq(this).bind("load", function () {
                    if (--index == 0) {
                        _updateIframeSize(message.id);
                    }
                });
            });

            _updateIframeSize(message.id);
        });

        return $frame;
    };

    var _getTextNodeParams = function (textNode) {
        var param = null;
        if (document.createRange) {
            var range = document.createRange();
            range.selectNodeContents(textNode);
            if (range.getBoundingClientRect) {
                var rect = range.getBoundingClientRect();
                if (rect) {
                    param = rect;
                }
            }
        }
        return param;
    };

    var _updateIframeSize = function (id) {
        var node_type = {
            element: 1,
            text: 3
        };

        /** IMPORTANT: All framed documents *must* have a DOCTYPE applied **/
        var iframe = jq('#message_body_frame_' + id),
            body = iframe.contents().find('body');

        body.css('cssText', 'height: auto; max-width: ' + iframe.width() + 'px !important;');

        var contents = body.contents(),
            new_height = 0,
            text_node_param;

        if (window.ActiveXObject || window.sidebar) { // IE, Firefox
            for (var i = 0; i < contents.length; i++) {
                var offset = 0;
                var item = contents[i];

                if (item.nodeType == node_type.element) {
                    offset = jq(item).offset().top + Math.max(item.scrollHeight, item.clientHeight, jq(item).outerHeight(true));
                } else if (item.nodeType == node_type.text) {
                    text_node_param = _getTextNodeParams(item);
                    offset = text_node_param == null ? 0 : text_node_param.bottom + 1;
                }

                if (new_height < offset) new_height = offset;
            }
            new_height += body.offset().top;
        } else {
            new_height = iframe.contents().outerHeight(true);
        }

        // for scroll
        if (window.ActiveXObject) new_height += 15;

        iframe.css('height', new_height + 'px');
    };


    var _displayIFrameMsgEvent = function (event) {
        jq("#mailHistoryEventContainer .messageHeader").html(jq.tmpl("historyMailTemplate", event.message));
        var bodyIFrameObj = createBodyIFrame(event.message);

        jq("#mailHistoryEventContainer .messageContent").html('').append(bodyIFrameObj);
    };

    return {
        init: function (event) {
            if (event == "" || event == null) return;
            try {
                event = jq.parseJSON(jq.base64.decode(event));
                ASC.CRM.Common.historyEventMsgObjFactory(event);
            } catch (e) {
                console.log(e);
                return;
            }

            try {
                if (event.message.hasOwnProperty("html_body")) {
                    _displayIFrameMsgEvent(event)
                } else {
                    jq.ajax({
                        type: "GET",
                        url: event.message.message_url,
                        success: function (response) {
                            try {
                                event.message.html_body = response;
                                _displayIFrameMsgEvent(event);
                            }
                            catch (e) { }
                        }
                    });
                }
            } catch (e) { }
        }
    };
})();

ASC.CRM.ContactSelector = new function() {
    var _hideAllAutocompleteBlocks = function() {
        jq("body>ul.ui-autocomplete").hide();
    };

    var _getSourceAutocompleteCallback = function(advancedInput, objName, obj, contacts) {
        var result = [];// contacts = null;
        if (contacts != null && contacts.length != 0) {
            for (var i = 0, n = contacts.length; i < n; i++) {
                if (jq.inArray(contacts[i].id, obj.SelectedContacts) === -1
                    && (typeof (obj.ExcludedArrayIDs) == "undefined" || jq.inArray(contacts[i].id, obj.ExcludedArrayIDs) === -1)) {
                    result.push(contacts[i]);
                }
            }
        }
        if (result == [] || result == null || result == 0) {
            var $trObj = jq(advancedInput).children('tbody').children('tr'),
                width = $trObj.width() - ($trObj.children('td').length - 3) * 18 - 2;
            jq("#noMatches_" + objName).css("width", width + "px");
            jq("#noMatches_" + objName).show();
        }
        return result;
    };

    var _initAutocomplete = function($input, advancedInput, objName, obj) {
        if ($input.length == 0) return;

        $input.autocomplete({
            //appendTo: "#selector_" + obj.ObjName,
            minLength: 0,
            delay: 300,
            focus: function(event, ui) {
                event.preventDefault ? event.preventDefault() : (event.returnValue = false);

                var autocomplete = jq(this).data("uiAutocomplete"),
                    menu = autocomplete.menu,

                    scroll = menu.element.scrollTop(),
                    offset = menu.active.offset().top - menu.element.offset().top,
                    elementHeight = menu.element.height();
                if (offset < 0) {
                    menu.element.scrollTop(scroll + offset);
                } else if (offset + menu.active.height() > elementHeight) {
                    menu.element.scrollTop(scroll + offset - elementHeight + menu.active.height());
                }
            },
            select: function (event, ui) {
                if (typeof (obj.SelectItemEvent) == "undefined") {
                    obj.setContact(this, ui.item.id, ui.item.displayName, ui.item.smallFotoUrl);
                    obj.showInfoContent(this);
                    return false;
                } else {
                    obj.SelectItemEvent(ui.item, { input: this, newContact: false });
                    jq(this).val("");
                    return false;
                }
            },
            selectFirst: false,
            search: function(event, ui) { return true; },
            source: function(request, response) {

                if (obj.IsInPopup) {
                    var ul = this.menu.element,
                        popupParent = jq(advancedInput).parents(".blockUI.blockPage:first");
                    if (popupParent.length != 0) {
                        if (popupParent.css("position") == "fixed") {
                            ul.css("position", "fixed");
                            jq("#noMatches_" + objName).css("position", "fixed");
                        } else {
                            ul.css("position", "absolute");
                            jq("#noMatches_" + objName).css("position", "absolute");
                        }
                    }
                }

                var term = request.term;
                if (term in obj.Cache) {
                    var contacts = obj.Cache[term];
                    response(_getSourceAutocompleteCallback(advancedInput, objName, obj, contacts));
                    return;
                }

                var data = { prefix: term, searchType: obj.SelectorType, entityType: obj.EntityType, entityID: obj.EntityID };

                Teamlab.getCrmContactsByPrefix({},
                {
                    filter: data,
                    success: function(par, contacts) {
                        jq("#loaderImg_" + objName).hide();
                        jq("#searchImg_" + objName).show();

                        obj.Cache[term] = contacts;
                        response(_getSourceAutocompleteCallback(advancedInput, objName, obj, contacts));
                    },
                    before: function(par) {
                        jq("#searchImg_" + objName).hide();
                        jq("#loaderImg_" + objName).show();
                    }
                });
            }
        });

        $input.data("uiAutocomplete")._renderMenu = function (ul, items) {
            var s = this;
            jq.each(items, function (jq, items) {
                s._renderItemData(ul, items);
            });

            if (obj.IsInPopup && jq.browser.mobile && ul.parents(".blockUI.blockPage:first").length == 0) {
                var popupParent = jq(advancedInput).parents(".blockUI.blockPage:first");
                if (popupParent.length != 0) {
                    ul.appendTo(popupParent.children());
                }
            }
        };

        $input.data("uiAutocomplete")._renderItem = function(ul, item) {
            jq("#noMatches_" + objName).hide();
            if (jq.inArray(item.id, obj.SelectedContacts) != -1) return;
            if (typeof (obj.ExcludedArrayIDs) != "undefined") {
                if (jq.inArray(item.id, obj.ExcludedArrayIDs) != -1) return;
            }
            var parent = item.company != null
                ? jq.format("<br/><span class='text-medium-describe'>{0}: {1}</span>", ASC.CRM.Resources.CRMJSResource.Company, item.company.displayName)
                : "";

            return jq("<li></li>").data("item.autocomplete", item)
                        .append(jq("<a href='javascript:void(0)'></a>").html(jq.htmlEncodeLight(item.displayName) + parent))
                        .appendTo(ul);
        };

        $input.data("uiAutocomplete")._resizeMenu = function() {
            var ul = this.menu.element,
                $trObj = jq(advancedInput).children("tbody").children("tr"),
                inputWidth = $trObj.width() - ($trObj.children("td").length - 3) * 18;
            ul.outerWidth(Math.max(inputWidth, this.element.outerWidth()));
        };

        $input.data("uiAutocomplete")._suggest = function(a) {
            var b = this.menu.element.empty().zIndex(this.element.zIndex() + 1);
            this._renderMenu(b, a);
            //this.menu.deactivate();
            this.menu.refresh();
            b.show();
            this._resizeMenu();
            b.position(jq.extend({ of: jq(this.element).parents("table:first") }, this.options.position));
        };
    };


    var _initClickEventHandler = function(input, objName) {
        var $noMatchesList = jq("div[id^='noMatches_" + objName + "']"),
            $noMatches = {};

        jq(input).bind("click", function (e) {
            $noMatchesList.hide();
            jq(this).autocomplete("search", jq.trim(jq(input).val()));
        });

        jq(document).click(function (event) {
            for (var i = 0, n = $noMatchesList.length; i < n; i++) {
                $noMatches = jq($noMatchesList[i]);

                if ($noMatches.is(":visible")) {
                    var $targetElement = jq((event.target) ? event.target : event.srcElement);
                    if (!$targetElement.parents().addBack().is("#" + $noMatches.attr("id").replace("noMatches_", "contactTitle_") + ", " + "#" + $noMatches.attr("id"))) {
                        $noMatches.hide();
                    }
                }
            }
        });
    };

    var _initKeyUpEventHandler = function(input) {
        jq(input).bind("keyup", function(e) {
            if (jq.trim(jq(this).val()) == "") {
                jq(this).parents("table:first").find("label.crossButton").hide();
            } else {
                jq(this).parents("table:first").find("label.crossButton").show();
            }
        });

    };


    this.ContactSelector = function(objName, params) {
        if (typeof (params) != "object") return;
        if (typeof (window[objName]) != "undefined") return window[objName];

        this.ObjName = objName;
        //default params
        this.SelectorType = -1; //Any
        this.ShowOnlySelectorContent = false;
        this.SelectedContacts = [];
        this.ExcludedArrayIDs = [];
        this.EntityType = -1; //Any
        this.EntityID = 0;
        this.DescriptionText = "";
        this.DeleteContactText = "";
        this.AddContactText = "";

        this.CrossButtonEventClick;
        this.SelectItemEvent;

        this.IsInPopup = false;
        this.AcceptNewContactShowOnly = false;

        this.ShowChangeButton = true;
        this.ShowAddButton = true;
        this.ShowDeleteButton = true;
        this.ShowContactImg = true;
        this.ShowNewCompanyContent = true;
        this.ShowNewContactContent = true;
        this.HTMLParent = "";

        var CurrentItem = this;
        //end default params


        if (params.hasOwnProperty("SelectorType")) {
            this.SelectorType = params.SelectorType;
        }
        if (params.hasOwnProperty("EntityType")) {
            this.EntityType = params.EntityType;
        }
        this.EntityID = params.EntityID || 0;

        if (params.hasOwnProperty("ShowOnlySelectorContent")) {
            this.ShowOnlySelectorContent = params.ShowOnlySelectorContent;
        }
        this.DescriptionText = params.DescriptionText || "";
        this.DeleteContactText = params.DeleteContactText || "";
        this.AddContactText = params.AddContactText || "";

        if (params.hasOwnProperty("IsInPopup")) {
            this.IsInPopup = params.IsInPopup;
        }
        if (params.hasOwnProperty("AcceptNewContactShowOnly")) {
            this.AcceptNewContactShowOnly = params.AcceptNewContactShowOnly;
        }

        if (params.hasOwnProperty("ShowChangeButton")) {
            this.ShowChangeButton = params.ShowChangeButton;
        }
        if (params.hasOwnProperty("ShowAddButton")) {
            this.ShowAddButton = params.ShowAddButton;
        }
        if (params.hasOwnProperty("ShowDeleteButton")) {
            this.ShowDeleteButton = params.ShowDeleteButton;
        }
        if (params.hasOwnProperty("ShowContactImg")) {
            this.ShowContactImg = params.ShowContactImg;
        }
        if (params.hasOwnProperty("ShowNewCompanyContent")) {
            this.ShowNewCompanyContent = params.ShowNewCompanyContent;
        }
        if (params.hasOwnProperty("ShowNewContactContent")) {
            this.ShowNewContactContent = params.ShowNewContactContent;
        }

        if (params.hasOwnProperty("presetSelectedContactsJson") && params.presetSelectedContactsJson != '') {
            if (typeof (params.presetSelectedContactsJson) == "object") {
                this.presetSelectedContacts = params.presetSelectedContactsJson;
            } else {
                this.presetSelectedContacts = jq.parseJSON(params.presetSelectedContactsJson);
            }
            if (this.presetSelectedContacts != null) {
                for (var i = 0, n = this.presetSelectedContacts.length; i < n; i++) {
                    this.SelectedContacts.push(this.presetSelectedContacts[i].id);
                }
            }
        }

        this.ExcludedArrayIDs = params.ExcludedArrayIDs || [];
        this.HTMLParent = params.HTMLParent || "";

        this.drawContactSelector = function() {
            if (CurrentItem.HTMLParent != "" && jq(CurrentItem.HTMLParent).length == 1 && jq("#selector_" + CurrentItem.ObjName).length == 0) {
                jq.tmpl("contactSelectorContainerTmpl", CurrentItem).appendTo(CurrentItem.HTMLParent);
            }

            CurrentItem.Cache = {};

            if (CurrentItem.SelectedContacts.length > 0 && CurrentItem.ShowOnlySelectorContent === false) {
                if (CurrentItem.presetSelectedContacts != null) {
                    for (var i = 0, n = CurrentItem.presetSelectedContacts.length; i < n; i++) {
                        CurrentItem.AddNewSelectorWithSelectedContact(CurrentItem.presetSelectedContacts[i]);
                    }
                }
            } else {
                CurrentItem.AddNewSelectorWithSelectedContact(null);
            }

            delete CurrentItem.presetSelectedContacts;
            var $selectorRows = jq("#selector_" + objName).children("div:first").children("div.contactSelector-item");
            jq($selectorRows[$selectorRows.length - 1]).addClass("withPlus");
        };

        this.changeContact = function(objID) {
            var id = parseInt(jq("#contactID_" + objID).val()),
                index = jq.inArray(id, CurrentItem.SelectedContacts);
            if (index != -1) {
                CurrentItem.SelectedContacts.splice(index, 1);
            }

            jq(window).trigger("editContactInSelector", [jq("#item_" + objID), this.ObjName]);
            //CurrentItem.setContact(jq("#contactTitle_" + objID), 0, "", "");
            jq("#contactID_" + objID).val(0);
            CurrentItem.showSelectorContent(jq("#contactTitle_" + objID));
        };

        this.deleteContact = function(objID) {
            var id = parseInt(jq("#contactID_" + objID).val()),
                index = jq.inArray(id, CurrentItem.SelectedContacts);
            if (index != -1) {
                CurrentItem.SelectedContacts.splice(index, 1);
            }

            jq(window).trigger("deleteContactFromSelector", [jq("#item_" + objID), this.ObjName]);

            jq("#item_" + objID).remove();
            var $selectorRows = jq("#selector_" + CurrentItem.ObjName).children("div:first").children("div.contactSelector-item");
            $selectorRows.filter(".withPlus").removeClass("withPlus");
            jq($selectorRows[$selectorRows.length - 1]).addClass("withPlus");
        };


        this.clearSelector = function () {
            CurrentItem.SelectedContacts = [];

            var $selectorRows = jq("#selector_" + CurrentItem.ObjName).children("div:first").children("div.contactSelector-item");
            if ($selectorRows.length > 1) {
                if (CurrentItem.ShowAddButton === true) {
                    jq($selectorRows[0]).addClass("withPlus");
                }
                $selectorRows.filter(":not(:first)").remove();
            }
        };


        this.crossButtonEventClick = function(objID) {
            jq("#selectorContent_" + objID).children(".contactSelector-inputContainer").find(".crossButton").hide();

            if (typeof (CurrentItem.CrossButtonEventClick) == "undefined") {
                var id = parseInt(jq("#contactID_" + objID).val()),
                    index = jq.inArray(id, CurrentItem.SelectedContacts);
                if (index != -1) {
                    CurrentItem.SelectedContacts.splice(index, 1);
                }
                jq("#contactTitle_" + objID).val("").blur();
                jq("#contactID_" + objID).val(0);
                jq("#noMatches_" + objID).hide();
            } else {
                CurrentItem.CrossButtonEventClick();
            }
        };

        this.setContact = function(obj, id, title, img) {
            if (jq(obj).length == 0) return;
            var objID = jq(obj).attr('id').replace("contactTitle_", ""),
                $avatarImg = jq("#infoContent_" + objID).find('img:first');

            jq(obj).val(title);

            if (img != "" && $avatarImg.length == 1) {
                if (img.indexOf("filehandler.ashx") != -1) {
                    jq.ajax({
                        type: "GET",
                        url: img,
                        success: function (response) {
                            try {
                                $avatarImg.attr("src", response);
                            }
                            catch (e) { }
                        }
                    });
                } else {
                    $avatarImg.attr("src", img);
                }
            }

            jq("#infoContent_" + objID).find('b:first').html(jq.htmlEncodeLight(title));
            jq("#contactID_" + objID).val(id);
            if (jq.inArray(id, CurrentItem.SelectedContacts) == -1 && id != 0) {
                CurrentItem.SelectedContacts.push(id);
            }
            jq(obj).val("");
            jq(window).trigger("setContactInSelector", [id, this.ObjName]);
        };

        this.showInfoContent = function(obj) {
            var objID = jq(obj).attr('id').replace("contactTitle_", "");
            jq('#selectorContent_' + objID).hide();
            jq("#newContactContent_" + objID).hide();
            jq('#infoContent_' + objID).show();
        };

        this.showSelectorContent = function(obj) {
            var objID = jq(obj).attr('id').replace("contactTitle_", "");

            if (jq.trim(jq(obj).val()) == "") {
                jq(obj).parents("table:first").find("label.crossButton").hide();
            } else {
                jq(obj).parents("table:first").find("label.crossButton").show();
            }

            jq('#infoContent_' + objID).hide();
            jq("#newContactContent_" + objID).hide();
            jq('#selectorContent_' + objID).show();
            jq(window).trigger("afterResetSelectedContact", [jq("#item_" + objID), this.ObjName]);
        };

        this.AddNewSelector = function(addSelectorLink) {
            if (addSelectorLink.hasClass("disabledLink")) return;
            addSelectorLink.addClass("disabledLink");
            CurrentItem.AddNewSelectorWithSelectedContact(null);
            addSelectorLink.removeClass("disabledLink");
        };

        this.AddNewSelectorWithSelectedContact = function(contact) {
            var index = 0,
                lastSelector = jq("#selector_" + CurrentItem.ObjName + " input[id^=contactTitle_]:last");
            if (jq(lastSelector).length > 0) {
                index = parseInt(jq(lastSelector).attr("id").replace("contactTitle_" + CurrentItem.ObjName + "_", "")) + 1;
            }

            var $newSelectorObj = jq.tmpl("contactSelectorRowTmpl",
                {
                    ObjID: CurrentItem.ObjName + "_" + index,
                    ObjName: CurrentItem.ObjName,
                    ShowOnlySelectorContent: CurrentItem.ShowOnlySelectorContent,
                    ShowNewCompanyContent: CurrentItem.ShowNewCompanyContent,
                    ShowNewContactContent: CurrentItem.ShowNewContactContent,
                    ShowContactImg: CurrentItem.ShowContactImg,
                    ShowDeleteButton: CurrentItem.ShowDeleteButton,
                    ShowAddButton: CurrentItem.ShowAddButton,
                    ShowChangeButton: CurrentItem.ShowChangeButton,

                    DeleteContactText: CurrentItem.DeleteContactText,
                    AddButtonText: CurrentItem.AddContactText,
                    WatermarkText: CurrentItem.DescriptionText,
                    selectedContact: contact
                });

            jq("#selector_" + CurrentItem.ObjName + " div:first").append($newSelectorObj);

            var $selectorRows = jq("#selector_" + CurrentItem.ObjName).children("div:first").children("div.contactSelector-item");

            $selectorRows.filter(".withPlus").removeClass("withPlus");
            jq($selectorRows[$selectorRows.length - 1]).addClass("withPlus");

            var $input = jq("#contactTitle_" + CurrentItem.ObjName + "_" + index);
            _initAutocomplete($input, jq($input).parents("table:first"), CurrentItem.ObjName + '_' + index, CurrentItem);
            _initClickEventHandler($input, CurrentItem.ObjName);
            _initKeyUpEventHandler($input);

            if (contact != null) {
                if (contact.smallFotoUrl.indexOf("filehandler.ashx") != -1) {
                    jq.ajax({
                        type: "GET",
                        url: contact.smallFotoUrl,
                        success: function (response) {
                            try {
                                jq("#infoContent_" + CurrentItem.ObjName + "_" + index).find("img:first").attr("src", response);
                            }
                            catch (e) { }
                        }
                    });
                }
            }
        };


        this.quickSearch = function(objID) {
            _hideAllAutocompleteBlocks();
            var $input = jq("#contactTitle_" + objID);
            $input.autocomplete("search", jq.trim($input.val()));
        };

        this.showNewCompany = function(objID) {
            var $selectorObj = this;
            jq("#newContactContent_" + objID + " input").val("");
            jq("#hiddenIsCompany_" + objID).val("true");
            jq("#noMatches_" + objID).hide();
            jq("#infoContent_" + objID).hide();
            jq("#selectorContent_" + objID).hide();
            jq("#newContactImg_" + objID).hide();
            jq("#newContactFirstName_" + objID).hide();
            jq("#newContactLastName_" + objID).hide();
            jq("#newContactContent_" + objID).show();
            jq("#newCompanyImg_" + objID).show();

            jq("#newCompanyTitle_" + objID).unbind("keyup").bind("keyup", function (e) {
                var code;
                if (e.keyCode) {
                    code = e.keyCode;
                } else if (e.which) {
                    code = e.which;
                }
                if (code == 13) {//Enter
                    $selectorObj.acceptNewContact(objID);
                }
                if (code == 27) {//ESC
                    $selectorObj.rejectNewContact(objID);
                }
            });

            var $input = jq("#contactTitle_" + objID);
            jq("#newCompanyTitle_" + objID).val($input.val()).show().focus();
        };

        this.showNewContact = function(objID) {
            var $selectorObj = this;
            jq("#newContactContent_" + objID + " input").val("");
            jq("#hiddenIsCompany_" + objID).val("false");
            jq("#noMatches_" + objID).hide();
            jq("#infoContent_" + objID).hide();
            jq("#selectorContent_" + objID).hide();
            jq("#newCompanyImg_" + objID).hide();
            jq("#newCompanyTitle_" + objID).hide();
            jq("#newContactContent_" + objID).show();
            jq("#newContactImg_" + objID).show();

            jq("#newContactFirstName_" + objID + ", #newContactLastName_" + objID).unbind("keyup").bind("keyup", function (e) {
                var code;
                if (e.keyCode) {
                    code = e.keyCode;
                } else if (e.which) {
                    code = e.which;
                }
                if (code == 13) {//Enter
                    $selectorObj.acceptNewContact(objID);
                }
                if (code == 27) {//ESC
                    $selectorObj.rejectNewContact(objID);
                }
            });

            var $input = jq("#contactTitle_" + objID);
            jq("#newContactLastName_" + objID).show();
            jq("#newContactFirstName_" + objID).val($input.val()).show().focus();
        };

        this.acceptNewContact = function(objID) {
            var data = {},
                isCompany = jq("#hiddenIsCompany_" + objID).val() == "true",

                $newCompanyTitleInput = jq("#newCompanyTitle_" + objID),
                $newContactFirstNameInput = jq("#newContactFirstName_" + objID),
                $newContactLastName = jq("#newContactLastName_" + objID),

                compName = $newCompanyTitleInput.length == 1 ? jq.trim($newCompanyTitleInput.val()) : "",
                firstName = $newContactFirstNameInput.length == 1 ? jq.trim($newContactFirstNameInput.val()) : "",
                lastName = $newContactLastName.length == 1 ? jq.trim($newContactLastName.val()) : "",

                obj = this,
                $input = jq("#contactTitle_" + objID);

            if (isCompany) {
                data.companyName = compName;
            } else {
                data.firstName = firstName;
                data.lastName = lastName;
            }
            data.managerList = new Array(Teamlab.profile.id);
            data.shareType = 0;

            var isValid = true;

            if (isCompany && compName == "") {
                jq("#newCompanyTitle_" + objID).addClass("requiredInputError");
                isValid = false;
            } else {
                jq("#newCompanyTitle_" + objID).removeClass("requiredInputError");
            }

            if (!isCompany && firstName == "") {
                jq("#newContactFirstName_" + objID).addClass("requiredInputError");
                isValid = false;
            } else {
                jq("#newContactFirstName_" + objID).removeClass("requiredInputError");
            }

            if (!isCompany && lastName == "") {
                jq("#newContactLastName_" + objID).addClass("requiredInputError");
                isValid = false;
            } else {
                jq("#newContactLastName_" + objID).removeClass("requiredInputError");
            }

            if (!isValid)
                return false;

            if (obj.AcceptNewContactShowOnly) {
                var imgSrc = isCompany ? jq("#newCompanyImg_" + objID).attr("src") : jq("#newContactImg_" + objID).attr('src'),
                    displayName = [data.firstName, ' ', data.lastName].join(''),
                    contactId = 0;

                if (typeof (obj.SelectItemEvent) == "undefined") {
                    jq("#infoContent_" + objID).show();
                    jq("#selectorContent_" + objID).hide();
                    obj.setContact($input, contactId, displayName, imgSrc);
                    obj.showInfoContent($input);
                    jq("#infoContent_" + objID + " img:first").attr("src", imgSrc);

                } else {
                    var contactInfo = { id: contactId, displayName: displayName, smallFotoUrl: imgSrc };
                    obj.showSelectorContent($input);
                    $input.val("");
                    obj.SelectItemEvent(contactInfo, { input: $input, newContact: true, data: data });
                }


            } else {
                Teamlab.addCrmContact({}, isCompany, data,
                {
                    before: function () {
                        jq("#newContactContent_" + objID).hide();
                    },
                    success: function (par, contact) {
                        if (typeof (obj.SelectItemEvent) == "undefined") {
                            jq("#infoContent_" + objID).show();
                            jq("#selectorContent_" + objID).hide();
                            obj.setContact($input, contact.id, contact.displayName, contact.smallFotoUrl);
                            obj.showInfoContent($input);

                            var imgSrc = isCompany ? jq("#newCompanyImg_" + objID).attr("src") : jq("#newContactImg_" + objID).attr('src');
                            jq("#infoContent_" + objID + " img:first").attr("src", imgSrc);

                        } else {
                            var contactInfo = { id: contact.id, displayName: contact.displayName, smallFotoUrl: contact.smallFotoUrl };
                            $input.val("");
                            obj.showSelectorContent($input);
                            obj.SelectItemEvent(contactInfo, { input: $input, newContact: true });
                        }
                    }
                });
            }
        };

        this.rejectNewContact = function(objID) {
            jq("#infoContent_" + objID).hide();
            jq("#selectorContent_" + objID).show();
            jq("#newContactContent_" + objID).hide();
            jq("#noMatches_" + objID).show();
            jq("#newContactContent_" + objID + " input").removeClass("requiredInputError");
        };

        jq(document).ready(function() {
            CurrentItem.drawContactSelector();
            jq(window).trigger("contactSelectorIsReady", [objName]);
        });
    };
};

ASC.CRM.CategorySelector = function (objName, currentCategory) {
    this.ObjName = objName;
    this.Me = function() { return jq("#" + this.ObjName); };
    this.CategoryTitle = currentCategory.title;
    this.CategoryID = currentCategory.id;

    if (jq.browser.mobile === false) {
        jq.dropdownToggle({
            dropdownID: this.ObjName + "_categoriesContainer",
            switcherSelector: ["#", this.ObjName, "_selectorContainer"].join(""),
            simpleToggle: true
        });
        ASC.CRM.Common.registerChangeHoverStateByParent("label.event_category", ["#", this.ObjName, "_categoriesContainer .categorySelector-category"].join(""));
    }

    this.changeContact = function(obj) {
        if (!jq.browser.mobile) {
            var id = parseInt(jq(obj).attr("id").split("_")[2]),
                title = jq("div", jq(obj)).text();
            this.setContact(id, title);
            this.hideSelectorContent();
        } else {
            var id = parseInt(jq(obj).val());
            var title = jq(obj).text();
            this.setContact(id, title);
        }
    };

    this.setContact = function(id, title) {
        this.CategoryID = id;
        this.CategoryTitle = title;

        if (!jq.browser.mobile) {
            jq('input[id=' + this.ObjName + '_categoryID]', this.Me()).val(id);
            jq('input[id=' + this.ObjName + '_categoryTitle]', this.Me()).val(title);
        } else {
            jq("option[id=" + this.ObjName + "_category_" + id + "]", this.Me()).attr("selected", true);
        }
    };

    this.hideSelectorContent = function() {
        jq('div[id=' + this.ObjName + '_categoriesContainer]', this.Me()).hide();
    };

    this.showSelectorContent = function() {
        jq('div[id=' + this.ObjName + '_categoriesContainer]', this.Me()).toggle();
    };

    this.getRowByContactID = function(id) {
        if (id > 0) {
            return jq.browser.mobile === true ?
                    jq("option[id=" + this.ObjName + "_category_" + id + "]", this.Me()) :
                    jq("div[id=" + this.ObjName + "_category_" + id + "]", this.Me());
        } else {
            return jq.browser.mobile === true ?
                    jq("option[id^=" + this.ObjName + "_category_]:first", this.Me()) :
                    jq("div[id^=" + this.ObjName + "_category_]:first", this.Me());
        }
    };

    this.renderControl = function(categories, selectedCategory, htmlParentSelector, maxWidth, helpInfoText) {
        if (typeof maxWidth == "undefined" || maxWidth == 0) {
            maxWidth = 230;
        }
        if (typeof helpInfoText != "undefined" && helpInfoText != "") {
            maxWidth -= 20;
        }
        jq.tmpl("categorySelectorTemplate",
            {
                jsObjName: this.ObjName,
                helpInfoText: helpInfoText,
                maxWidth: maxWidth,
                categories: categories,
                selectedCategory : selectedCategory
            }).appendTo(htmlParentSelector);
    };
};

ASC.CRM.TagView = (function() {
    var callback_delete_tag = function(params, tag) {
        params.element.remove();
        jq("#addTagDialog .h_line").show();
        jq.tmpl("tagInAllTagsTmpl", { "tagLabel": tag }).appendTo("#addTagDialog ul.dropdown-content");
        jq(window).trigger("deleteTagComplete", tag);
    };

    var _addTagToCurrentEntity = function (params, text) {
        Teamlab.addCrmTag(params, ASC.CRM.TagView.EntityType, ASC.CRM.TagView.EntityID, text,
                {
                    success: ASC.CRM.TagView.callback_add_tag,
                    before: function () {
                        jq("#tagContainer .adding_tag_loading").show();
                        jq("#addTagDialog").hide();
                    },
                    after: function () {
                        jq("#tagContainer .adding_tag_loading").hide();
                    }
                });
    };

    var _addTag = function(params, text) {
        if (text != null && text != "") {
            if (ASC.CRM.TagView.EntityID != 0 && !ASC.CRM.TagView.OnlyInterface) {
                _addTagToCurrentEntity(params, text);
            } else if (!params.hasOwnProperty("element") && !ASC.CRM.TagView.OnlyInterface) {
                Teamlab.addCrmEntityTag(params, ASC.CRM.TagView.EntityType, text,
                    {
                        success: ASC.CRM.TagView.callback_add_tag,
                        before: function () {
                            jq("#tagContainer .adding_tag_loading").show();
                            jq("#addTagDialog").hide();
                        },
                        after: function () {
                            jq("#tagContainer .adding_tag_loading").hide();
                        }
                    });
            } else {
                ASC.CRM.TagView.callback_add_tag(params, Teamlab.create('crm-tag', null, text));
                jq("#addTagDialog").hide();
            }
        }
    };

    return {
        init: function(entityType, onlyInterface, params) {
            ASC.CRM.TagView.EntityType = entityType;
            ASC.CRM.TagView.OnlyInterface = onlyInterface;

            if (typeof (params) != "undefined" && params.hasOwnProperty("addTag") && typeof (params.addTag) === "function") {
                ASC.CRM.TagView.addTagExtension = params.addTag;
            }

            var URLParamID = jq.getURLParam("id");
            if (isNaN(URLParamID)) {
                return;
            }

            ASC.CRM.TagView.EntityID = URLParamID * 1;

            if (jq("#addTagDialog a.dropdown-item").length == 0) {
                jq("#addTagDialog .h_line").hide();
            }

            jq.dropdownToggle({
                dropdownID: "addTagDialog",
                switcherSelector: "#addNewTag",
                addTop: 1,
                addLeft: 0,
                simpleToggle: true
            });

            jq("#addThisTag").click(function() {
                ASC.CRM.TagView.addTagExtension({}, jq("#addTagDialog input:first").val().trim());
            });

            jq("#addTagDialog input").focus().unbind("keyup").keyup(function(e) {
                var code;
                if (!e) {
                    e = event;
                }
                if (e.keyCode) {
                    code = e.keyCode;
                } else if (e.which) {
                    code = e.which;
                }

                if (code == 13) {
                    var text = jq("#addTagDialog input:first").val().trim();
                    if (text == "") {
                        return;
                    }
                    ASC.CRM.TagView.addTagExtension({}, text);
                }
            });

            if (jq.browser.mobile === true) {
                jq("#tagContainer").on("click", ".tag_item", function() {
                    jq(this).addClass("tag_hover");
                });
            }
        },

        deleteTag: function($element) {
            var text = jQuery.base64.decode($element.children(".tag_title:first").attr("data-value"));

            if (ASC.CRM.TagView.EntityID != 0 && !ASC.CRM.TagView.OnlyInterface) {
                Teamlab.removeCrmTag({ element: $element }, ASC.CRM.TagView.EntityType, ASC.CRM.TagView.EntityID, text,
                    {
                        success: callback_delete_tag
                    });
            } else {
                callback_delete_tag({ element: $element }, Teamlab.create('crm-tag', null, text));
            }
        },

        addTagExtension: function (params, text) {
            _addTag(params, text);
        },

        addExistingTag: function(element) {
            var tagTitle = jQuery.base64.decode(jq(element).attr("data-value"));

            ASC.CRM.TagView.addTagExtension({ element: element }, tagTitle);
        },

        callback_add_tag : function(params, tag) {
            jq.tmpl("taqTmpl", { "tagLabel": tag }).appendTo("#tagContainer>div:first");
            jq("#addTagDialog input:first").val("");
            if (params.hasOwnProperty("element")) {
                jq(params.element).remove();
                if (jq("#addTagDialog a.dropdown-item").length == 0) {
                    jq("#addTagDialog .h_line").hide();
                }
            }
            jq(window).trigger("addTagComplete", tag);
        },

        prepareTagDataForSubmitForm: function($input) {
            var tagData = {};
            tagData.tagListInfo = [];
            jq("#tagContainer .tag_item .tag_title").each(function() {
                var tagValue = jq(this).text().trim();
                if (tagValue) {
                    tagData.tagListInfo.push(tagValue);
                }
            });
            if (tagData.tagListInfo.length > 0) {
                $input.val(jq.toJSON(tagData));
            } else {
                $input.val();
            }
        }
    };
})();



ASC.CRM.ImportFromCSVView = (function() {

    var initOtherActionMenu = function() {
        ASC.CRM.Common.removeExportButtons();
        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };
    return {
        init: function(intEntityType) {
            jq("#delimiterCharacterSelect,#encodingSelect, #quoteCharacterSelect").tlcombobox();
            initOtherActionMenu();
            if (intEntityType == 0) {//contact

                ASC.CRM.UserSelectorListView.Init(
                   "_ImportContactsManager",
                   "UserSelectorListView_ImportContactsManager",
                   false,
                   ASC.CRM.Resources.CRMContactResource.NotifyContactManager,
                   new Array(ASC.CRM.Resources.CurrentUser),
                   null,
                   new Array(Teamlab.profile.id),
                   "#importContactsManager",
                   false);

                var $html = jq.tmpl("makePublicPanelTemplate",
                    {
                        Title: ASC.CRM.Resources.CRMContactResource.MakePublicPanelTitleForImportContacts,
                        Description: ASC.CRM.Resources.CRMContactResource.MakePublicPanelDescrForContact,
                        IsPublicItem: false,
                        CheckBoxLabel: ASC.CRM.Resources.CRMContactResource.MakePublicPanelCheckBoxLabelForImportContacts

                    });

                $html.find(".makePublicPanelSelector")
                    .append(jq("<option value='2'></option>").text(ASC.CRM.Resources.CRMCommonResource.AccessRightsForReading))
                    .append(jq("<option value='1'></option>").text(ASC.CRM.Resources.CRMCommonResource.AccessRightsForReadWriting))
                    .val("2")
                    .tlCombobox();

                $html.appendTo("#makePublicPanel");
            }

            AjaxPro.Utils.ImportFromCSV.GetStatus(intEntityType, function(res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    return;
                }
                if (res.value == null || res.value.IsCompleted) {
                    ASC.CRM.ImportEntities.init(intEntityType);
                } else {
                    jq("#importFromCSVSteps").hide();
                    jq("#importStartedFinalMessage").show();
                    ASC.CRM.ImportEntities.checkImportStatus(true);
                }
            });
        }
    };
})();

ASC.CRM.ImportEntities = (function ($) {
    var _CSVFileURI = "",
        _curIndexSampleRow = 0,
        _ajaxUploader,
        _entityType,
        _sampleRowCache = new Object(),
        _settingsBase = new Object();

    var _refreshSampleRowItems = function(data) {
        var resultData = jq.parseJSON(data),
            sampleRow = resultData.data,
            sampleRowColumnIndex = 0;

        jq("#columnMapping tbody tr td").each(
                function(index) {
                    if ((index + 1) % 3 == 0)
                        jq(this).text(sampleRow[sampleRowColumnIndex++]);
                });

        if (resultData.isMaxIndex) {
            jq("#nextSample").hide().prev().hide();
        }
    };

    var _getSampleRow = function() {
        AjaxPro.onLoading = function(b) {
            if (b) {
                jq("#columnMapping thead tr td:last span:last").block();
            } else {
                jq("#columnMapping thead tr td:last span:last").unblock();
            }
        };

        var sampleRowCacheKey = _CSVFileURI + '' + _curIndexSampleRow;

        if (_sampleRowCache[sampleRowCacheKey]) {
            _refreshSampleRowItems(_sampleRowCache[sampleRowCacheKey]);
            return;
        }

        AjaxPro.Utils.ImportFromCSV.GetSampleRow(_CSVFileURI, _curIndexSampleRow, jq.toJSON(_settingsBase),
            function(result) {
                if (result.error != null) {
                    _showErrorPanel(result.error.Message);
                    return;
                }
                _sampleRowCache[sampleRowCacheKey] = result.value;
                _refreshSampleRowItems(result.value);
            }
          );
    };

    var _nextStep = function(step) {
        jq(jq.format("#importFromCSVSteps dd:eq({0})", step - 1)).hide();
        jq(jq.format("#importFromCSVSteps dt:eq({0})", step - 1)).hide();
        jq(jq.format("#importFromCSVSteps dd:eq({0})", step)).show();
        jq(jq.format("#importFromCSVSteps dt:eq({0})", step)).show();
    };

    var _ajaxUploaderOnSubmitCallback = function(file, extension) {
        _settingsBase = {
            "has_header": jq("#ignoreFirstRow").is(":checked"),
            "delimiter_character": jq("#delimiterCharacterSelect option:selected").val(),
            "encoding": jq("#encodingSelect option:selected").val(),
            "quote_character": jq("#quoteCharacterSelect option:selected").val()
        };
        this._settings.data["importSettings"] = jq.toJSON(_settingsBase);
        LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.ImportFromCSVStepOneProgressLable
        LoadingBanner.showLoaderBtn("#importFromCSVSteps");
    };

    var _ajaxUploaderonCompleteCallback = function(file, response) {
        var responseObj = jq.parseJSON(response);
        if (!responseObj.Success) {
            _showErrorPanel(responseObj.Error || responseObj.Message);
            jq("#uploadCSVFile").removeClass("edit_button").addClass("import_button");
            jq("#uploadCSVFile").text(ASC.CRM.Resources.CRMJSResource.SelectCSVFileButton);
            jq("#uploadCSVFile").prev().hide();
            LoadingBanner.hideLoaderBtn("#importFromCSVSteps");
            //ASC.CRM.ImportEntities.prevStep(0);
            return false;
        }

        var responseData = jq.parseJSON(jQuery.base64.decode(responseObj.Data));
        _CSVFileURI = responseData.assignedPath;

        if (responseData.isMaxIndex) {
            jq("#prevSample").parent().hide();
        } else {
            jq("#prevSample").parent().show();
        }

        var stepTwoContent = jq("#columnMapping tbody");

        stepTwoContent.html("");
        jq.tmpl("columnMappingTemplate", responseData, {
            renderSelector: function(rowIndex) {
                var columnSelector = jq("#columnSelectorBase").clone();
                columnSelector.attr("id", jq.format("columnSelector_{0}", rowIndex));
                columnSelector.show();
                return jq("<div>").append(columnSelector).html();
            }
        }).appendTo(stepTwoContent);

        var selectItems = jq("#columnMapping select");

        selectItems.each(function() {
            try {
                var curSelect = jq(this),
                    columnHeader = jq.trim(curSelect.parent().prev().text());
                if (jq.browser.msie) {
                    var leftBracketColumnCount = jq(columnHeader.match(/\(/g)).length,
                        rightBracketColumnCount = jq(columnHeader.match(/\)/g)).length;

                    if (leftBracketColumnCount != rightBracketColumnCount) return;
                }
                var findedItem = curSelect.find(jq.format("option:contains('{0}'):first", columnHeader)).attr("selected", true);
            }
            catch (e) {
            }
        });


        selectItems.on("change", function() {
            var curSelector = jq(this);
            if (curSelector.find("option:first").is(":selected")) {
                curSelector.parents("td").addClass("missing");
            } else {
                curSelector.parents("td").removeClass("missing");
            }
        });

        selectItems.change();

        jq("#uploadCSVFile").removeClass("edit_button").addClass("import_button");

        jq("#uploadCSVFile").text(ASC.CRM.Resources.CRMJSResource.SelectCSVFileButton);
        jq("#uploadCSVFile").prev().hide();

        _nextStep(1);
        LoadingBanner.hideLoaderBtn("#importFromCSVSteps");
    };


    var _initErrorPanel = function () {
        jq.tmpl("blockUIPanelTemplate", {
            id: "importErrorPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Error,
            questionText: "",
            innerHtmlText: "<div class=\"errorNote\"></div>",
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK
        }).insertAfter("#columnMapping");

        jq("#importErrorPanel .button.blue").on("click", function () { jq.unblockUI(); });
    };

    var _showErrorPanel = function (msg) {
        jq("#importErrorPanel .errorNote").text(msg);
        //PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#importErrorPanel", 500, 200, 0);
    }

    return {
        init: function(entityType) {

            _entityType = entityType;

            if (_entityType == 0) {
                jq("#removingDuplicatesBehaviorPanel").show();
            } else {
                jq("#removingDuplicatesBehaviorPanel").hide();
            }

            jq.tmpl("columnSelectorTemplate", ASC.CRM.Data.columnSelectorData).appendTo("#columnSelectorBase");
            jq("#columnSelectorBase").find("option[name=is_header]").each(function() {
                var curItem = jq(this);
                curItem.nextUntil('option[name=is_header]').wrapAll(jq("<optgroup>").attr("label", curItem.val()));
                curItem.remove();
            });

            _ajaxUploader = new AjaxUpload('uploadCSVFile', {
                action: 'ajaxupload.ashx?type=ASC.Web.CRM.Controls.Common.ImportFileHandler,ASC.Web.CRM',
                autoSubmit: false,
                onChange: function(file, extension) {
                    if (extension == "") {
                        jq("#uploadCSVFile").removeClass("edit_button").addClass("import_button");
                        jq("#uploadCSVFile").text(ASC.CRM.Resources.CRMJSResource.SelectCSVFileButton);
                        jq("#uploadCSVFile").prev().hide();
                        ASC.CRM.ImportEntities.prevStep(0);
                        return false;
                    }

                    if (extension[0].toLowerCase() != "csv") {
                        _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorMessage_NotSupportedFileFormat);
                        return false;
                    }

                    var messageContainer = jq("#uploadCSVFile").prev();
                    messageContainer.show();
                    messageContainer.text(jq.format(ASC.CRM.Resources.CRMJSResource.SelectedCSVFileLabel, file));
                    jq("#uploadCSVFile").removeClass("import_button").addClass("edit_button").text(ASC.CRM.Resources.CRMJSResource.Change);
                    jq("#importFromCSVSteps dd:first .middle-button-container a.button.blue.middle:first").removeClass("disable");
                    return true;
                },
                onSubmit: _ajaxUploaderOnSubmitCallback,
                onComplete: _ajaxUploaderonCompleteCallback
            });

            _initErrorPanel();
        },
        prevStep: function(step) {
            jq(jq.format("#importFromCSVSteps dd:eq({0})", step + 1)).hide();
            jq(jq.format("#importFromCSVSteps dt:eq({0})", step + 1)).hide();

            jq(jq.format("#importFromCSVSteps dd:eq({0})", step)).show();
            jq(jq.format("#importFromCSVSteps dt:eq({0})", step)).show();

            if (step != 0) return;

            jq("#importFromCSVSteps dd:first .middle-button-container a.button.blue.middle:first").addClass("disable");
        },
        getPreviewImportData: function() {
            var columnMappingSelector = jq("#columnMapping select"),
                firstNameOptions = columnMappingSelector.find("option[name=firstName]:selected"),
                lastNameOptions = columnMappingSelector.find("option[name=lastName]:selected"),
                companyNameOptions = columnMappingSelector.find("option[name=companyName]:selected");

            if ((firstNameOptions.length == 0 || lastNameOptions.length == 0) && companyNameOptions.length == 0) {
                _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorNotMappingBasicColumn);
                return;
            }

            if (!((firstNameOptions.length == 1 && lastNameOptions.length == 1) || (companyNameOptions.length == 1))) {
                _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorMappingMoreBasicColumn);
                return;
            }

            var companyNameColumnIndex = 0,
                firstNameColumnIndex = 0,
                lastNameColumnIndex = 0;

            if (companyNameOptions.length == 1) {
                companyNameColumnIndex = columnMappingSelector.index(jq(companyNameOptions[0]).parents("select"));
            }

            if (firstNameOptions.length == 1 && lastNameOptions.length == 1) {
                firstNameColumnIndex = columnMappingSelector.index(jq(firstNameOptions[0]).parents("select"));
                lastNameColumnIndex = columnMappingSelector.index(jq(lastNameOptions[0]).parents("select"));
            }

            AjaxPro.onLoading = function(b) {
                if (b) {
                    LoadingBanner.showLoaderBtn("#importFromCSVSteps");
                } else {
                    LoadingBanner.hideLoaderBtn("#importFromCSVSteps");
                }
            };

            AjaxPro.Utils.ImportFromCSV.GetPreviewImportData(_CSVFileURI,
                    companyNameColumnIndex,
                    firstNameColumnIndex,
                    lastNameColumnIndex,
                        function(result) {
                            if (result.error != null) {
                                _showErrorPanel(result.error.Message);
                                return;
                            }
                            var resultData = jq.parseJSON(result.value);
                            jq("#previewImportData tbody tr").remove();
                            jq.tmpl("previewImportDataTemplate", resultData).appendTo("#previewImportData tbody");
                            jq("#importFromCSVSteps dd:eq(2) span:eq(1)").text(jq.format(ASC.CRM.Resources.CRMJSResource.ImportFromCSVStepThreeDescription, jq("#previewImportData tbody tr").length));
                            _nextStep(2);
                        });
        },

        getColumnMapping: function() {
            var result = {};
            jq("#columnMapping select").each(function() {
                var name = jq.trim(jq(this).find("option:selected").attr("name"));
                if (name == "" || name == -1) { return true; }
                if (typeof result[name] == "undefined") {
                    result[name] = new Array();
                }

                var idParts = this.id.split("_");
                if (idParts.length < 2) { return true; }

                result[name].push(idParts[1]);
            });
            return result;
        },

        startImport: function() {
            jq("select[id^=columnSelector].requiredInputError").removeClass("requiredInputError");
            var columnMapping = ASC.CRM.ImportEntities.getColumnMapping(),
                columnMappingSelector = jq("#columnMapping select"),
                isValid = true;

            for (var p in columnMapping) {
                if (columnMapping[p].length > 1 && p != "tag") {
                    for (var i = 0, n = columnMapping[p].length; i < n; i++) {
                        jq("#columnSelector_" + columnMapping[p][i]).addClass("requiredInputError");
                    }
                    isValid = false;
                }
            }
            if (!isValid) {
                _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorMappingMoreBasicColumn);
                return;
            }

            switch (_entityType) {
                case 0:  //  Contact
                    {
                        var firstNameOptions = columnMappingSelector.find("option[name=firstName]:selected"),
                            lastNameOptions = columnMappingSelector.find("option[name=lastName]:selected"),
                            companyNameOptions = columnMappingSelector.find("option[name=companyName]:selected");

                        if ((firstNameOptions.length == 0 || lastNameOptions.length == 0) && companyNameOptions.length == 0) {
                            _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorContactNotMappingBasicColumn);
                            return;
                        }
                    }
                    break;
                case 1:  //  Opportunity
                    {
                        var titleOptions = columnMappingSelector.find("option[name=title]:selected"),
                            responsibleOptions = columnMappingSelector.find("option[name=responsible]:selected");

                        if (titleOptions.length == 0 || responsibleOptions.length == 0) {
                            _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorOpportunityNotMappingBasicColumn);
                            return;
                        }
                    }
                    break;
                case 3:  //  Task
                    {
                        var titleOptions = columnMappingSelector.find("option[name=title]:selected"),
                            dueDateOptions = columnMappingSelector.find("option[name=due_date]:selected"),
                            responsibleOptions = columnMappingSelector.find("option[name=responsible]:selected");

                        if (titleOptions.length == 0 ||
                            dueDateOptions.length == 0 ||
                            responsibleOptions.length == 0) {
                            _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorTaskNotMappingBasicColumn);
                            return;
                        }
                    }
                    break;
                case 7:  //  Case
                    var titleOptions = columnMappingSelector.find("option[name=title]:selected");
                    if (titleOptions.length == 0) {
                        _showErrorPanel(ASC.CRM.Resources.CRMJSResource.ErrorCasesNotMappingBasicColumn);
                        return;
                    }
                    break;
                default:
                    break;
            }

            var importSetting = _settingsBase;

            importSetting["column_mapping"] = columnMapping;

            if (_entityType == 0) {//contact
                var contactManagers = new Array(Teamlab.profile.id);

                for (var i = 0, n = window.SelectedUsers_ImportContactsManager.IDs.length; i < n; i++) {
                    contactManagers.push(window.SelectedUsers_ImportContactsManager.IDs[i]);
                }
                importSetting["contact_managers"] = contactManagers;
                importSetting["share_type"] = jq("#isPublic").is(":checked") ? jq(".makePublicPanel select.makePublicPanelSelector").val() : 0;
            } else if (_entityType != 3) {
                var privateUsers = new Array(SelectedUsers.CurrentUserID);

                for (var i = 0, n = SelectedUsers.IDs.length; i < n; i++) {
                    privateUsers.push(SelectedUsers.IDs[i]);
                }
                importSetting["access_list"] = privateUsers;
                importSetting["is_private"] = jq("#isPrivate").is(":checked");
            }

            if (_entityType == 0) {
                importSetting.removing_duplicates_behavior = jq("input[name='removingDuplicatesBehavior']:checked").val();
            }

            AjaxPro.onLoading = function(b) {
                if (b) {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.ImportFromCSVStepTwoProgressLable;
                    LoadingBanner.showLoaderBtn("#importFromCSVSteps");
                } else {
                    LoadingBanner.hideLoaderBtn("#importFromCSVSteps");
                }
            };

            AjaxPro.Utils.ImportFromCSV.StartImport(_entityType, _CSVFileURI, jq.toJSON(importSetting),
                function(result) {
                    if (result.error != null) {
                        _showErrorPanel(result.error.Message);
                        return;
                    }
                    jq("#importFromCSVSteps").hide();
                    jq("#importStartedFinalMessage").show();
                    ASC.CRM.ImportEntities.checkImportStatus(true);
                });
        },
        startUploadCSVFile: function() {
            _ajaxUploader.submit();
        },
        getPrevSampleRow: function() {
            _curIndexSampleRow--;
            jq("#nextSample").show().prev().show();

            if (_curIndexSampleRow == 0) {
                jq("#prevSample").hide().next().hide();
            }
            _getSampleRow();
        },
        getNextSampleRow: function() {
            _curIndexSampleRow++;

            jq("#prevSample").show().next().show();
            _getSampleRow();
        },

        checkImportStatus: function(isFirstVisit) {

            AjaxPro.onLoading = function(b) { };
            if (isFirstVisit) {
                jq("#importStartedFinalMessage div.progress").css("width", "0%");
                jq("#importStartedFinalMessage div.percent").text("0%");
                jq("#importErrorBox").hide();
                jq("#importStartedFinalMessage div.progressErrorBox").html("");
            }

            AjaxPro.Utils.ImportFromCSV.GetStatus(_entityType, function(res) {
                if (res.error != null) {
                    _showErrorPanel(res.error.Message);
                    return false;
                }

                if (res.value == null) {
                    jq("#importStartedFinalMessage div.progress").css("width", "100%");
                    jq("#importStartedFinalMessage div.percent").text("100%");
                    jq("#importErrorBox").hide();
                    jq("#importStartedFinalMessage div.progressErrorBox").html("");
                    return false;
                }

                jq("#importStartedFinalMessage div.progress").css("width", parseInt(res.value.Percentage) + "%");
                jq("#importStartedFinalMessage div.percent").text(parseInt(res.value.Percentage) + "%");

                if (res.value.Error != null && res.value.Error != "") {
                    ASC.CRM.ImportEntities.buildErrorList(res);
                } else {
                    if (!res.value.IsCompleted) {
                        setTimeout("ASC.CRM.ImportEntities.checkImportStatus(false)", 3000);
                    }
                }
            });
        },

        buildErrorList: function(res) {
            var mess = "error";
            switch (typeof res.value.Error) {
                case "object":
                    mess = res.value.Error.Message + "<br/>";
                    break;
                case "string":
                    mess = res.value.Error;
                    break;
            }
            var link = jq("#importStartedFinalMessage #importLinkBox a").clone().removeClass("button.blue");
            jq("#importStartedFinalMessage div.progressErrorBox")
                .html(jq("<div></div>").addClass("red-text").html(mess))
                .append(link);
            jq("#importStartedFinalMessage #importErrorBox").show();
            jq("#importStartedFinalMessage #importLinkBox").hide();
        }
    };
})(jQuery);


ASC.CRM.UserSelectorListView = new function() {
    this.Init = function (objId, objName, showNotifyPanel, notifyLabel, usersWhoHasAccess, userIDs, disabledUserIDs, parentHtmlSelector, inPopup) {
        window[objName] = new ASC.CRM.UserSelectorListView.InitNewUserSelectorListView(
                                                                    objId,
                                                                    objName,
                                                                    usersWhoHasAccess,
                                                                    showNotifyPanel,
                                                                    notifyLabel,
                                                                    parentHtmlSelector);

        //ASC.Resources.Master.EmployeeAllDepartments,
        var $addUsrLink = jq("#usrSrListViewAdvUsrSrContainer" + objId + " .addUserLink");
        $addUsrLink.useradvancedSelector(
                {
                    showme: false,
                    inPopup: inPopup,
                    onechosen: false,
                    showGroups: true,
                    withGuests: false
                });

        var users = [],
            items = $addUsrLink.data("items");
        if (ASC.CRM.Data.isCrmAvailableForAllUsers == true) {
            users = jq.merge([], items);
        } else {
            for (var i = 0, n = ASC.CRM.Data.crmAvailableWithAdminList.length; i < n; i++){
                for (var j = 0, m = items.length; j < m; j++){
                    if (ASC.CRM.Data.crmAvailableWithAdminList[i].id == items[j].id) {
                        users.push(items[j]);
                        continue;
                    }
                }
            }
        }

        var disabledUserIDs = disabledUserIDs != null && disabledUserIDs != "" && disabledUserIDs.length != 0 ? disabledUserIDs : [],
            newUsrList = [];

        if (userIDs != null || disabledUserIDs.length != 0) {
            if (userIDs != null) {
                for (var i = 0, n = users.length; i < n; i++) {
                    if (userIDs.indexOf(users[i].id) != -1 && disabledUserIDs.indexOf(users[i].id) == -1) {
                        newUsrList.push(users[i]);
                    }
                }
            } else {
                for (var i = 0, n = users.length; i < n; i++) {
                    if (disabledUserIDs.indexOf(users[i].id) == -1) {
                        newUsrList.push(users[i]);
                    }
                }
            }
        } else {
            newUsrList = users;
        }

        $addUsrLink.useradvancedSelector("rewriteItemList", newUsrList, []);

        $addUsrLink.on("showList", function (event, items) {
            for (var i = 0, n = items.length; i < n; i++) {
                window[objName].PushUserIntoList(objId, objName, items[i].id, items[i].title);
            }
        });

        if (window["SelectedUsers" + objId].IDs.length == 0) {
            jq("#cbxNotify" + objId).removeAttr("checked");
            jq("#notifyPanel" + objId).hide();
        }

        if (window["SelectedUsers" + objId].IDs.length != 0) {
            $addUsrLink.useradvancedSelector("disable", window["SelectedUsers" + objId].IDs);
        }
    };

    this.InitNewUserSelectorListView = function (objId, objName, usersWhoHasAccess, showNotifyPanel, notifyLabel, parentHtmlSelector) {
        if (typeof (window[objName]) != "undefined") return window[objName];

        this.ObjId = objId;
        this.ObjName = objName;
        this.ShowNotifyPanel = showNotifyPanel;

        this.NotifyLabel = notifyLabel;
        this.DeleteImgSrc = window["SelectedUsers" + objId].DeleteImgSrc;

        this.PushUserIntoList = function (objId, objName, uID, uName) {
            var alreadyExist = false,
                windowSelectedUsersObj = window["SelectedUsers" + objId];

            for (var i = 0, n = windowSelectedUsersObj.IDs.length; i < n; i++)
                if (windowSelectedUsersObj.IDs[i] == uID) {
                    alreadyExist = true;
                    break;
                }

            if (alreadyExist) return false;

            windowSelectedUsersObj.IDs.push(uID);
            windowSelectedUsersObj.Names.push(uName);

            var item = jq("<div></div>")
                    .attr("id", "selectedUser_" + uID + objId)
                    .addClass("selectedUser"),

                deleteImg = jq("<img>")
                    .attr("src", windowSelectedUsersObj.DeleteImgSrc)
                    .css("margin", "0px 4px -2px 0px")
                    .css("display", "none")
                    .css("width", "12px")
                    .css("height", "12px")
                    .css("cursor", "pointer")
                    .attr("id", "deleteSelectedUserImg_" + uID + objId)
                    .attr("title", windowSelectedUsersObj.DeleteImgTitle);

            item.append(jq.htmlEncodeLight(uName)).append(deleteImg);

            jq("#selectedUsers" + objId).append(item);

            jq("#selectedUser_" + uID + objId).unbind("mouseover").bind("mouseover", function () {
                window[objName].SelectedItem_mouseOver(jq(this));
            });

            jq("#selectedUser_" + uID + objId).unbind("mouseout").bind("mouseout", function () {
                window[objName].SelectedItem_mouseOut(jq(this));
            });

            jq("#deleteSelectedUserImg_" + uID + objId).unbind("click").bind("click", function () {
                window[objName].SelectedUser_deleteItem(jq(this));
            });

            jq("#cbxNotify" + objId).prop("checked", true);
            jq("#notifyPanel" + objId).show();

            jq("#usrSrListViewAdvUsrSrContainer" + objId + " .addUserLink").useradvancedSelector("reset", true);
            jq("#usrSrListViewAdvUsrSrContainer" + objId + " .addUserLink").useradvancedSelector("disable", [uID])

            jq("#selectedUsers" + objId).parent().find("div[id=DepsAndUsersContainer]").hide();
            jq(window).trigger("advUserSelectorPushUserComplete", [objId, uID]);
        };

        this.SelectedItem_mouseOver = function (obj) {
            var iID = jq(obj).attr("id").split("_")[1];
            if (jq(obj).hasClass("selectedUser")) {
                jq("#selectedUser_" + iID + this.ObjId + " img:first").hide();
                jq("#selectedUser_" + iID + this.ObjId + " img:last").show();
            }
        };

        this.SelectedItem_mouseOut = function (obj) {
            var iID = jq(obj).attr("id").split("_")[1];
            if (jq(obj).hasClass("selectedUser")) {
                jq("#selectedUser_" + iID + this.ObjId + " img:first").show();
                jq("#selectedUser_" + iID + this.ObjId + " img:last").hide();
            }
        };

        this.SelectedUser_deleteItem = function (obj) {
            var uID = jq(obj).attr("id").split("_")[1];
            jq("#selectedUser_" + uID + this.ObjId).remove();

            var windowSelectedUsersObj = window["SelectedUsers" + this.ObjId];

            for (var i = 0; i < windowSelectedUsersObj.IDs.length; i++) {
                if (windowSelectedUsersObj.IDs[i] == uID) {
                    windowSelectedUsersObj.IDs.splice(i, 1);
                    windowSelectedUsersObj.Names.splice(i, 1);
                    break;
                }
            }

            if (windowSelectedUsersObj.IDs.length == 0) {
                jq("#cbxNotify" + this.ObjId).removeAttr("checked");
                jq("#notifyPanel" + this.ObjId).hide();
            }

            jq("#usrSrListViewAdvUsrSrContainer" + objId + " .addUserLink").useradvancedSelector("undisable", [uID])
            jq(window).trigger("advUserSelectorDeleteComplete", [this.ObjId, uID]);
        };

        var selectedUsers = [];

        for (var i = 0, n = window["SelectedUsers" + objId].IDs.length; i < n; i++) {
            selectedUsers.push({
                id: window["SelectedUsers" + objId].IDs[i],
                displayName: window["SelectedUsers" + objId].Names[i]
            });
        }

        jq.tmpl("userSelectorListViewTemplate", {
            objId : objId,
            objName: objName,
            usersWhoHasAccess: usersWhoHasAccess,
            deleteImgSrc: this.DeleteImgSrc,
            selectedUsers: selectedUsers,
            showNotifyPanel: showNotifyPanel,
            notifyLabel: notifyLabel,
            addEmployeeLabel: ASC.CRM.Resources.AddUser
        }).appendTo(parentHtmlSelector);

        return this;
    };
};

// Google Analytics const
var ga_Categories = {
    contacts: "crm_contacts",
    cases: "crm_cases",
    deals: "crm_deals",
    tasks: "crm_tasks",
    sender: "crm_sender"
};

var ga_Actions = {
    filterClick: "filter-click",
    createNew: "create-new",
    remove: "remove",
    edit: "edit",
    view: "view",
    changeStatus: "change-status",
    next: "next",
    userClick: "user-click",
    actionClick: "action-click",
    quickAction: "quick-action"
};
// end Google Analytics