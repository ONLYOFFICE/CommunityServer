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
window.tagsPage = (function($) {
    var 
        isInit = false,
        _page,
        buttons = [];

    var init = function() {
        if (isInit === false) {
            isInit = true;

            _page = $('#id_tags_page');

            _page.find('#createNewTag').click(function() {
                tagsModal.showCreate();
                return false;
            });

            tagsManager.events.bind('refresh', _onRefreshTags);
            tagsManager.events.bind('delete', _onDeleteTag);
            tagsManager.events.bind('create', _onCreateTag);
            tagsManager.events.bind('update', _onUpdateTag);

            buttons = [
                { selector: "#tagActionMenu .editAccount", handler: _edit_tag },
                { selector: "#tagActionMenu .deleteAccount", handler: _delete_tag}];
        }
    };

    var show = function() {
        _page.find('.tag_list').remove();

        var html = $.tmpl('tagsTmpl', { tags: tagsManager.getAllTags() }, { htmlEncode: TMMail.htmlEncode });
        
        var $html = $(html);
        $('#id_tags_page .containerBodyBlock .content-header').after($html);

        $('#id_tags_page').actionMenu('tagActionMenu', buttons);

        if (!_checkEmptyShowBlankPage()) _page.show(0, _updateTagsAddresses);
        else _page.hide();
    };

    var _updateTagsAddresses = function() {
        _page.find('.row').each(function(index, value) {
            var $value = $(value);
            var id = $value.attr('data_id');
            var tag = tagsManager.getTag(id);
            $value.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': _address_to_html });
        });
    };

    var hide = function() {
        _page.hide();
    };

    var _address_to_html = function(address, separator) {
        if (separator)
            return '<span>' + ', ' + address + '</span>';
        else
            return '<span>' + address + '</span>';
    };

    var _onRefreshTags = function(e, tags) {
        _page.find('.row[data_id]').remove();
        $.each(tags, function(index, value) { _onCreateTag(undefined, value); });
    };

    var _onCreateTag = function(e, tag) {
        _addTag(tag);
        _page.find('.tag_list').show();
        _page.find('.emptyScrCtrl').hide();
    };

    var _addTag = function(tag) {
        var html = $.tmpl('tagItemTmpl', tag, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);
        $html.actionMenu('tagActionMenu', buttons);
        if (0 <= tag.id) {// mail tag
            $html.find('.tag').click(function(e) {
                _edit_tag(tag.id);
            });
            // get last mail tag in list
            var rows = _page.find('.row');
            if (0 == rows.length)
                _page.find('.tag_list').append($html);
            else if (0 > parseInt($(rows[0]).attr('data_id')))
                _page.find('.tag_list').prepend($html);
            else
                rows.each(function(index, value) {
                    if (rows.length == index + 1 || 0 > parseInt($(rows[index + 1]).attr('data_id'))) {
                        $(value).after($html);
                        return false;
                    }
                });
        } else {// crm tag
            _page.find('.tag_list').append($html);
        }

        setImmediate(function () { $html.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': _address_to_html }); });

        if (TMMail.pageIs('tags')) {
            if (!_checkEmptyShowBlankPage())
                _page.show();
        }
    };

    var _onDeleteTag = function(e, id) {
        _page.find('.row[data_id="' + id + '"]').remove();
        if (TMMail.pageIs('tags')) {
            if (_checkEmptyShowBlankPage())
                _page.hide();
        }
    };

    var _onUpdateTag = function(e, tag) {
        var tag_div = _page.find('.row[data_id="' + tag.id + '"]');
        tag_div.find('span.tag').removeClass().addClass('tag tagArrow tag' + tag.style).html(TMMail.htmlEncode(tag.name));
        tag_div.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': _address_to_html });
    };

    var _edit_tag = function(id) {
        var tag = tagsManager.getTag(id);
        tagsModal.showEdit(tag);
    };

    var _delete_tag = function(id) {
        var tag = tagsManager.getTag(id);
        tagsModal.showDelete(tag);
    };

    var _checkEmptyShowBlankPage = function() {
        if (_page.find('.tag_item').length) {
            _page.find('.tag_list').show();
            blankPages.hide();
            return false;
        }
        else {
            _page.find('.tag_list').hide();
            blankPages.showEmptyTags();
            return true;
        }
    };
    return {
        init: init,

        show: show,
        hide: hide
    };
})(jQuery);