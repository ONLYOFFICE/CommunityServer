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