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

(function($) {

    var _initAutocomplete = function(input) {
        if (input.length == 0) return;

        $(input).autocomplete({
            minLength: 1,
            delay: 500,
            autoFocus: true,
            appendTo: $(input).parent(),
            select: function(event, ui) {
                var result = _fullSearchString + ui.item.value;
                if ($(this).hasClass('multipleAutocomplete')) result = result + ', ';
                if ($(this).hasClass('emailOnly')) result = TMMail.parseEmailFromFullAddress(result);
                $(this).val(result);
                $(this).trigger('input');
                return false;
            },
            create: function(event, ui) {
                $(window).resize(function () {
                    if ($(input).data("uiAutocomplete") != undefined) $(input).data("uiAutocomplete").close();
                });
            },
            focus: function(event, ui) {
                return false;
            },
            search: function(event, ui) {
                return true;
            },
            source: function(request, response) {
                var term = request.term;

                if (input.hasClass('multipleAutocomplete')) {
                    var stringList = term.split(',');
                    term = stringList[stringList.length - 1].trim();
                    _fullSearchString = request.term;
                    _fullSearchString = _fullSearchString.slice(0, _fullSearchString.lastIndexOf(term));
                }

                if (term in _emailAutocompleteCache) {
                    var resp = '';
                    if (input.hasClass('emailOnly')) {
                        var result;
                        for (var i = 0; i < _emailAutocompleteCache[term].length; i++) {
                            result = (_emailAutocompleteCache[term])[i];
                            result = TMMail.parseEmailFromFullAddress(result);
                            if (term != result && input[0].id == document.activeElement.id) {
                                resp = _emailAutocompleteCache[term];
                                break;
                            }
                        }
                    }
                    else if (document.activeElement && input[0].id == document.activeElement.id) resp = _emailAutocompleteCache[term];
                    response(resp);
                    return;
                }
                serviceManager.getMailContacts({ searchText: term, responseFunction: response, input: input }, { term: term }, { success: _onGetContacts });
            }
        });

        $(input).data("uiAutocomplete")._resizeMenu = function() {
            var ul = this.menu.element;
            ul.outerWidth(Math.max($(input).width(), this.element.outerWidth()));
        };
    };

    var _onGetContacts = function(params, contacts) {
        var current_value = params.input[0].value;
        _emailAutocompleteCache[params.searchText] = contacts;
        var resp = '';
        if ($(params.input).hasClass('emailOnly')) {
            var result;
            for (var i = 0; i < contacts.length; i++) {
                result = contacts[i];
                result = TMMail.parseEmailFromFullAddress(result);
                if (params.searchText != result && document.activeElement && (params.input)[0].id == document.activeElement.id) {
                    resp = contacts;
                    break;
                }
            }
        }
        else if (document.activeElement && (params.input)[0].id == document.activeElement.id) resp = contacts;
        params.responseFunction(resp);
        params.input[0].value = current_value;
    };

    var _fullSearchString = '';
    var _emailAutocompleteCache = {};


    $.fn.emailAutocomplete = function(params) {
        var input = $(this);
        if (params.multiple) input.addClass('multipleAutocomplete');
        if (params.emailOnly) input.addClass('emailOnly');
        _initAutocomplete(input);
    };
})(jQuery);
