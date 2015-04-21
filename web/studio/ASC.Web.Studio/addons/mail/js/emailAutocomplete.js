/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


(function($) {

    var initAutocomplete = function(input) {
        if (input.length == 0) {
            return;
        }

        $(input).autocomplete({
            minLength: 1,
            delay: 500,
            autoFocus: true,
            appendTo: $(input).parent(),
            select: function(event, ui) {
                var result = fullSearchString + ui.item.value;
                if ($(this).hasClass('multipleAutocomplete')) {
                    result = result + ', ';
                }
                if ($(this).hasClass('emailOnly')) {
                    result = TMMail.parseEmailFromFullAddress(result);
                }
                $(this).val(result);
                $(this).trigger('input');
                return false;
            },
            create: function() {
                $(window).resize(function() {
                    if ($(input).data("ui-autocomplete") != undefined) {
                        $(input).data("ui-autocomplete").close();
                    }
                });
            },
            focus: function() {
                return false;
            },
            search: function() {
                return true;
            },
            source: function(request, response) {
                var term = request.term;

                if (input.hasClass('multipleAutocomplete')) {
                    var stringList = term.split(',');
                    term = stringList[stringList.length - 1].trim();
                    fullSearchString = request.term;
                    fullSearchString = fullSearchString.slice(0, fullSearchString.lastIndexOf(term));
                }

                if (term in emailAutocompleteCache) {
                    var resp = '';
                    if (input.hasClass('emailOnly')) {
                        var result;
                        for (var i = 0; i < emailAutocompleteCache[term].length; i++) {
                            result = (emailAutocompleteCache[term])[i];
                            result = TMMail.parseEmailFromFullAddress(result);
                            if (term != result && input[0].id == document.activeElement.id) {
                                resp = emailAutocompleteCache[term];
                                break;
                            }
                        }
                    } else if (document.activeElement && input[0].id == document.activeElement.id) {
                        resp = emailAutocompleteCache[term];
                    }
                    response(resp);
                    return;
                }
                serviceManager.getMailContacts({ searchText: term, responseFunction: response, input: input }, { term: term }, { success: onGetContacts });
            }
        });

        $(input).data("ui-autocomplete")._resizeMenu = function() {
            var ul = this.menu.element;
            ul.outerWidth(Math.max($(input).width(), this.element.outerWidth()));
        };
    };

    var onGetContacts = function(params, contacts) {
        var currentValue = params.input[0].value;
        emailAutocompleteCache[params.searchText] = contacts;
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
        } else if (document.activeElement && (params.input)[0].id == document.activeElement.id) {
            resp = contacts;
        }
        params.responseFunction(resp);
        params.input[0].value = currentValue;
    };

    var fullSearchString = '';
    var emailAutocompleteCache = {};


    $.fn.emailAutocomplete = function(params) {
        var input = $(this);
        if (params.multiple) {
            input.addClass('multipleAutocomplete');
        }
        if (params.emailOnly) {
            input.addClass('emailOnly');
        }
        initAutocomplete(input);
    };
})(jQuery);