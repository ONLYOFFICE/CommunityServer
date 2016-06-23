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


window.fromSenderFilter = (function($) {
    var type = 'from-sender-filter';

    function init() {
        jQuery(document.body).bind('click', onBodyClick);
    }

    function onBodyClick(e) {
        if (!$(e.target).is('#mailsender-selector-wrapper')
            && !$(e.target).is('#search-mailsender-panel')
            && $(e.target).closest('#mailsender-selector-wrapper').length == 0
            && $(e.target).closest('#search-mailsender-panel').length == 0) {
            $('#search-mailsender-panel').hide();
        }
    }

    function create() {
        var o = document.createElement('div');
        o.innerHTML = [
            '<div class="default-value">',
            '<span class="title">',
            (TMMail.pageIs('sent') || TMMail.pageIs('drafts')) ? MailScriptResource.FilterToMailAddress : MailScriptResource.FilterFromSender,
            '</span>',
            '<span id="mailsender-selector-wrapper" class="selector-wrapper">',
            '<span class="combobox-selector">',
            '<span class="custom-combobox">',
            '<span class="combobox-title">',
            '<span class="inner-text">' + MailScriptResource.Select + '</span>',
            '</span>',
            '</span>',
            '</span>',
            '</span>',
            '<span class="btn-delete">&times;</span>',
            '</div>'
        ].join('');
        return o;
    }

    function customize($container, $filteritem) {
        var $html = $([
            '<div id="search-mailsender-panel" class="studio-action-panel">',
            '<div class="advanced-selector-search">',
            '<input class="advanced-selector-search-field" type="text"/>',
            '<div class="advanced-selector-search-btn"></div>',
            '</div>',
            '</div>'].join(''));

        $filteritem.find('#mailsender-selector-wrapper').append($html);

        $filteritem.on('keydown', '#mailsender-selector-wrapper .advanced-selector-search-field', function(e) {
            if (e.keyCode != 13) {
                return;
            }

            var val = $(this).val().trim();
            if (!val) {
                return;
            }

            MailFilter.setFrom($(this).val());
            mailBox.updateAnchor();
        });

        $filteritem.on('click', '#mailsender-selector-wrapper .advanced-selector-search-btn', function() {
            var val = $filteritem.find('#mailsender-selector-wrapper .advanced-selector-search-field').val().trim();
            if (!val) {
                return;
            }

            MailFilter.setFrom(val);
            mailBox.updateAnchor();
        });

        $filteritem.on('click', '#mailsender-selector-wrapper .combobox-selector', function() {
            $filteritem.find('#search-mailsender-panel').toggle();
            setFocusToInput($filteritem.find('#mailsender-selector-wrapper .advanced-selector-search-field'));
        });
    }

    function destroy() {
    }

    function process($container, $filteritem, filtervalue, params) {
        $filteritem.find('#mailsender-selector-wrapper .inner-text').text(params.value);
        $filteritem.find('#mailsender-selector-wrapper .advanced-selector-search-field').val(params.value);
    }

    function setFocusToInput($el) {
        var length = $el.val().length;

        $el.focus();
        $el[0].setSelectionRange(length.length, length.length);
    }

    init();

    return {
        type: type,
        create: create,
        customize: customize,
        destroy: destroy,
        process: process
    };
})(jQuery);