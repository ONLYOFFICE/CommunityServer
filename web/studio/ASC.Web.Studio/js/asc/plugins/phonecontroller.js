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

/*
Copyright (c) Ascensio System SIA 2013. All rights reserved.
http://www.teamlab.com
*/
;
var PhoneController = new function() {

    _renderControl = function($input) {
        $input.addClass("phoneControlInput");
        var innerHtml = [
            "<table cellpadding='0' cellspacing='0' class='styled-select-container'>",
                "<colgroup>",
                    "<col style='width: 50px;' />",
                    "<col />",
                "</colgroup>",
                "<tbody>",
                    "<tr>",
                        "<td>",
                            "<span class='phoneControlSwitherWrapper'>",
                                "<div class='phoneControlSwither tl-combobox tl-combobox-container'>",
                                    "<div class='selectedPhoneCountry'></div>",
                                "</div>",
                            "</span>",
                        "</td>",
                        "<td>",
                            "<div class='phoneControlInputContainer'>",
                                $input[0].outerHTML,
                            "</div>",
                        "</td>",
                    "</tr>",
                "</tbody>",
            "</table>",
            "<div class='studio-action-panel' id='phoneControlDropDown'>",
                "<ul class='dropdown-content'></ul>",
            "</div>"
            ].join('');

        var o = document.createElement('span');
        o.className = 'phoneControlContainer';
        o.innerHTML = innerHtml;
        jq($input).replaceWith(o);
        PhoneController.phoneControlContainer = jq(o);
    };

    _getCountryByKey = function(key) {
        for (var i = 0, n = PhoneController.countryList.length; i < n; i++) {
            if (PhoneController.countryList[i].key == key) {
                return PhoneController.countryList[i];
            }
        }
        return null;
    };

    _sortCountriesByCode = function(a, b) {
        var aInt = a.country_code * 1,
            bInt = b.country_code * 1;
        if (aInt > bInt) {
            return -1;
        }
        if (aInt < bInt) {
            return 1;
        }
        return typeof (a.def) != "undefined"
                ? -1
                : (typeof (b.def) != "undefined" ? 1 : 0);
    };

    _initCountryPhonesDropDown = function() {
        var html = "",
            tmp = null,
            country = null;

        for (var i = 0, n = PhoneController.countryList.length; i < n; i++) {
            country = PhoneController.countryList[i];
            if (PhoneController.defaultCountryCallingCode == country.key) {
                PhoneController.selectedCountryPhone = country;
                PhoneController.selectedCountryPhone["def"] = true;
            }
            html += ["<li class='li_",
                    country.key,
                    PhoneController.defaultCountryCallingCode == country.key ? " default-item selected'" : "'",
                    ">",
                "<table><tbody>",
                    "<tr>",
                        "<td>",
                            "<div class='fg-item fg_",
                                country.key,
                                "'",
                                " title='",
                                country.title,
                                    "'>",
                            "</div>",
                        "</td>",
                        "<td>",
                            "<a class='dropdown-item' data-key='",
                            country.key,
                            "'>",
                            country.title,
                            " ",
                            country.country_code,
                            "</a>",
                        "</td>",
                    "</tr>",
                "</tbody></table>",
                "</li>"
                ].join('');
        }

        PhoneController.phoneControlContainer.find("#phoneControlDropDown ul.dropdown-content").html(html);

        PhoneController.countryListSortedByCode = jq.extend([], PhoneController.countryList);
        PhoneController.countryListSortedByCode.sort(_sortCountriesByCode);

        PhoneController.phoneControlContainer.find(".phoneControlSwither .selectedPhoneCountry:first")
            .attr("class", "selectedPhoneCountry fg_" + PhoneController.selectedCountryPhone.key)
            .attr("title", PhoneController.selectedCountryPhone.title);

        PhoneController.phoneControlContainer.find("input.phoneControlInput:first").val(PhoneController.selectedCountryPhone.country_code + " ");
        PhoneController.phoneControlContainer.find("input.phoneControlInput:first").on("keyup", function(event) {
            _enterPhone();
        });

        PhoneController.phoneControlContainer.find("input.phoneControlInput:first").unbind('paste').bind('paste', function(e) {
            setTimeout(
                function() {
                    _enterPhone();
                }, 0);
            return true;
        });
        PhoneController.phoneControlContainer.find("#phoneControlDropDown ul.dropdown-content").on("click", "a.dropdown-item", function() {
            _selectCountryPhoneComplete(jq(this), jq(this).attr("data-key"));
            jq("#phoneControlDropDown").hide();
        });
        jq.dropdownToggle({
            dropdownID: "phoneControlDropDown",
            switcherSelector: ".phoneControlContainer .phoneControlSwither",
            simpleToggle: true
        });
    };
    
    _enterPhone = function () {
        var phone_text = _purePhone();
        var country = _findCountryByPhone(phone_text);
        if (country != PhoneController.selectedCountryPhone
            && !(country == null && PhoneController.selectedCountryPhone.country_code == PhoneController.defaultCountryCallingCode)) {
            _selectCountryPhoneComplete(null, country != null ? country.key : null);
        }
    };

    _selectCountryPhoneComplete = function($opt, key) {
        var phone_text = _purePhone();

        delete PhoneController.selectedCountryPhone["def"];
        PhoneController.countryListSortedByCode.sort(_sortCountriesByCode);

        if ($opt == null || $opt == {}) {
            if (typeof (key) != "string" || key == "") {
                key = PhoneController.defaultCountryCallingCode;
                PhoneController.selectedCountryPhone = _getCountryByKey(key);
            } else {
                PhoneController.selectedCountryPhone = _getCountryByKey(key);
                phone_text = jq.trim(phone_text.replace(PhoneController.GetCountryPhoneReg(PhoneController.selectedCountryPhone.country_code), ""));
                phone_text = [PhoneController.selectedCountryPhone.country_code, phone_text].join("");
                PhoneController.phoneControlContainer.find("input.phoneControlInput:first").val(phone_text);
            }
        } else {
            phone_text = jq.trim(phone_text.replace(PhoneController.GetCountryPhoneReg(null), ""));
            PhoneController.selectedCountryPhone = _getCountryByKey(key);

            phone_text = [PhoneController.selectedCountryPhone.country_code, phone_text].join("");
            PhoneController.phoneControlContainer.find("input.phoneControlInput:first").val(phone_text);
        }
        PhoneController.selectedCountryPhone["def"] = true;
        PhoneController.countryListSortedByCode.sort(_sortCountriesByCode);
        jq("#phoneControlDropDown ul.dropdown-content li.selected").removeClass("selected");
        jq("#phoneControlDropDown ul.dropdown-content li.li_" + key).addClass("selected");
        PhoneController.phoneControlContainer.find(".phoneControlSwither .selectedPhoneCountry")
            .attr("class", "selectedPhoneCountry fg_" + key)
            .attr("title", PhoneController.selectedCountryPhone.title);
    };

    _findCountryByPhone = function(phone) {
        for (var i = 0, n = PhoneController.countryListSortedByCode.length; i < n; i++) {
            country = PhoneController.countryListSortedByCode[i];
            if (PhoneController.GetCountryPhoneReg(country.country_code).test(phone)) {
                return country;
            }
        }
        return null;
    };

    _purePhone = function () {
        var phone_text = jq.trim(PhoneController.phoneControlContainer.find("input.phoneControlInput:first").val());
        if (phone_text.length && phone_text[0] != '+') {
            phone_text = "+" + phone_text;
        }
        return phone_text;
    };

    return {

        isInit: false,
        phoneControlContainer: null,
        selectedCountryPhone: null,
        defaultCountryCallingCode: "",
        countryList: [],
        countryListSortedByCode: [],

        Init: function($input, countryList, testDefaultCountryCallingCodeList) {
            if (this.isInit === false) {
                this.countryList = countryList;

                this.defaultCountryCallingCode = "";
                var tmp = null;

                if (typeof (testDefaultCountryCallingCodeList) !== "undefined" && testDefaultCountryCallingCodeList.length > 0) {
                    for (var i = 0, n = testDefaultCountryCallingCodeList.length; i < n; i++) {
                        tmp = _getCountryByKey(testDefaultCountryCallingCodeList[i]);
                        if (tmp != null) {
                            this.defaultCountryCallingCode = tmp.key;
                            break;
                        }
                    }
                }

                if (this.defaultCountryCallingCode == "") {
                    return;
                }
                _renderControl($input);
                var startPhone = _purePhone();
                _initCountryPhonesDropDown();
                PhoneController.phoneControlContainer.find("input.phoneControlInput:first").val(startPhone);
                _enterPhone(startPhone);

                this.isInit = true;
            }
        },

        GetCountryPhoneReg: function(country_code) {
            if (typeof (country_code) == "undefined" || country_code == null || country_code == "") {
                country_code = PhoneController.selectedCountryPhone.country_code;
            }
            return new RegExp("^\s*" + country_code.replace("+", "\\+"));
        },

        ClearDataAndErrors: function() {
            PhoneController.selectedCountryPhone = _getCountryByKey(PhoneController.defaultCountryCallingCode);
            PhoneController.phoneControlContainer.find("input.phoneControlInput:first").val(PhoneController.selectedCountryPhone.country_code + " ");
            PhoneController.ClearErrors();
        },

        ClearErrors: function() {
            PhoneController.DeleteErrorClass(PhoneController.phoneControlContainer);
        },

        ShowErrors: function() {
            PhoneController.phoneControlContainer.addClass('error');
        },

        DeleteErrorClass: function($o) {
            $o.attr("class", $o.attr("class").replace(/\s*error/gi, ''));
        },

        GetPhone: function() {
            var phone = _purePhone();
            if (!PhoneController.GetCountryPhoneReg(null).test(phone)) {
                phone = [PhoneController.selectedCountryPhone.country_code, phone].join('');
            }
            return phone;
        }
    };
};