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


window.tagsPage = (function($) {
    var isInit = false,
        page,
        buttons = [];

    var init = function() {
        if (isInit === false) {
            isInit = true;

            page = $('#id_tags_page');

            page.find('#createNewTag').click(function() {
                tagsModal.showCreate();
                return false;
            });

            tagsManager.events.bind('refresh', onRefreshTags);
            tagsManager.events.bind('delete', onDeleteTag);
            tagsManager.events.bind('create', onCreateTag);
            tagsManager.events.bind('update', onUpdateTag);

            buttons = [
                { selector: "#tagActionMenu .editAccount", handler: editTag },
                { selector: "#tagActionMenu .deleteAccount", handler: deleteTag }];
        }
    };

    var show = function() {
        page.find('.tag_list').remove();

        var html = $.tmpl('tagsTmpl', { tags: tagsManager.getAllTags() }, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);
        $('#id_tags_page .containerBodyBlock .content-header').after($html);

        $('#id_tags_page').actionMenu('tagActionMenu', buttons);

        if (!checkEmptyShowBlankPage()) {
            page.show(0, updateTagsAddresses);
        } else {
            page.hide();
        }
    };

    var updateTagsAddresses = function() {
        page.find('.row').each(function(index, value) {
            var $value = $(value);
            var id = $value.attr('data_id');
            var tag = tagsManager.getTag(id);
            $value.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': addressToHtml });
        });
    };

    var hide = function() {
        page.hide();
    };

    var addressToHtml = function(address, separator) {
        if (separator) {
            return '<span>' + ', ' + address + '</span>';
        } else {
            return '<span>' + address + '</span>';
        }
    };

    var onRefreshTags = function(e, tags) {
        page.find('.row[data_id]').remove();
        $.each(tags, function(index, value) { onCreateTag(undefined, value); });
    };

    var onCreateTag = function(e, tag) {
        addTag(tag);
        page.find('.tag_list').show();
        page.find('.emptyScrCtrl').hide();
    };

    var addTag = function(tag) {
        var html = $.tmpl('tagItemTmpl', tag, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);
        $html.actionMenu('tagActionMenu', buttons);
        if (0 <= tag.id) { // mail tag
            $html.find('.tag').click(function() {
                editTag(tag.id);
            });
            // get last mail tag in list
            var rows = page.find('.row');
            if (0 == rows.length) {
                page.find('.tag_list').append($html);
            } else if (0 > parseInt($(rows[0]).attr('data_id'))) {
                page.find('.tag_list').prepend($html);
            } else {
                rows.each(function(index, value) {
                    if (rows.length == index + 1 || 0 > parseInt($(rows[index + 1]).attr('data_id'))) {
                        $(value).after($html);
                        return false;
                    }
                });
            }
        } else { // crm tag
            page.find('.tag_list').append($html);
        }

        setImmediate(function() { $html.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': addressToHtml }); });

        if (TMMail.pageIs('tags')) {
            if (!checkEmptyShowBlankPage()) {
                page.show();
            }
        }
    };

    var onDeleteTag = function(e, id) {
        page.find('.row[data_id="' + id + '"]').remove();
        if (TMMail.pageIs('tags')) {
            if (checkEmptyShowBlankPage()) {
                page.hide();
            }
        }
    };

    var onUpdateTag = function(e, tag) {
        var tagDiv = page.find('.row[data_id="' + tag.id + '"]');
        tagDiv.find('span.tag').removeClass().addClass('tag tagArrow tag' + tag.style).html(TMMail.htmlEncode(tag.name));
        tagDiv.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': addressToHtml });
    };

    var editTag = function(id) {
        var tag = tagsManager.getTag(id);
        tagsModal.showEdit(tag);
    };

    var deleteTag = function(id) {
        var tag = tagsManager.getTag(id);
        tagsModal.showDelete(tag);
    };

    var checkEmptyShowBlankPage = function() {
        if (page.find('.tag_item').length) {
            page.find('.tag_list').show();
            blankPages.hide();
            return false;
        } else {
            page.find('.tag_list').hide();
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