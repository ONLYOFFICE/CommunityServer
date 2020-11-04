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


window.tagsPage = (function($) {
    var isInit = false,
        page,
        header,
        buttons = [];

    function init() {
        if (isInit === false) {
            isInit = true;

            page = $('#id_tags_page');
            
            header = $('#pageActionContainer');

            header.on('click', '#createNewTag', function () {
                tagsModal.showCreate();
            });
            
            header.on('click', '#tagHelpCenterSwitcher', function (e) {
                jq('#tagHelpCenterSwitcher').helper({ BlockHelperID: 'TagsHelperBlock' });
                e.preventDefault();
                e.stopPropagation();
            });

            tagsManager.bind(tagsManager.events.OnRefresh, onRefreshTags);
            tagsManager.bind(tagsManager.events.OnDelete, onDeleteTag);
            tagsManager.bind(tagsManager.events.OnCreate, onCreateTag);
            tagsManager.bind(tagsManager.events.OnUpdate, onUpdateTag);

            buttons = [
                { selector: "#tagActionMenu .editAccount", handler: editTag },
                { selector: "#tagActionMenu .deleteAccount", handler: deleteTag }];
        }
    }

    function show() {
        page.find('.tag_list').remove();

        var html = $.tmpl('tagsTmpl', { tags: tagsManager.getAllTags() }, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);
        $('#id_tags_page .containerBodyBlock').append($html);

        $('#id_tags_page').actionMenu('tagActionMenu', buttons);

        if (!checkEmptyShowBlankPage()) {
            header.html($.tmpl('tagsPageHeaderTmpl'));
            page.show(0, updateTagsAddresses);
        } else {
            hide();
        }
    }

    function updateTagsAddresses() {
        page.find('.row').each(function(index, value) {
            var $value = $(value);
            var id = $value.attr('data_id');
            var tag = tagsManager.getTag(id);
            $value.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': addressToHtml });
        });
    }

    function hide() {
        page.hide();
        header.empty();
    }

    function addressToHtml(address, separator) {
        var html = $.tmpl("addressTagTmpl", { address: address, separator: separator }, { htmlEncode: TMMail.htmlEncode });
        return html;
    }

    function onRefreshTags(e, tags) {
        page.find('.row[data_id]').remove();
        $.each(tags, function(index, value) { onCreateTag(undefined, value); });
    }

    function onCreateTag(e, tag) {
        addTag(tag);
        page.find('.tag_list').show();
        page.find('.emptyScrCtrl').hide();
    }

    function addTag(tag) {
        var html = $.tmpl('tagItemTmpl', tag, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);
        $html.actionMenu('tagActionMenu', buttons);
        if (0 <= tag.id) { // mail tag
            $html.find('.tag').click(function() {
                editTag(tag.id);
            });
            // get last mail tag in list
            var rows = page.find('.row');
            if (0 === rows.length) {
                page.find('.tag_list').append($html);
            } else if (0 > parseInt($(rows[0]).attr('data_id'))) {
                page.find('.tag_list').prepend($html);
            } else {
                rows.each(function(index, value) {
                    if (rows.length === index + 1 || 0 > parseInt($(rows[index + 1]).attr('data_id'))) {
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
                header.html($.tmpl('tagsPageHeaderTmpl'));
                page.show();
            }
        }
    }

    function onDeleteTag(e, id) {
        page.find('.row[data_id="' + id + '"]').remove();
        if (TMMail.pageIs('tags')) {
            if (checkEmptyShowBlankPage()) {
                hide();
            }
        }
    }

    function onUpdateTag(e, tag) {
        var tagDiv = page.find('.row[data_id="' + tag.id + '"]');
        tagDiv.find('span.tag').removeClass().addClass('tag tagArrow tag' + tag.style).html(TMMail.htmlEncode(tag.name));
        tagDiv.find('.addresses').hidePanel({ 'items': tag.addresses, 'item_to_html': addressToHtml });
    }

    function editTag(id) {
        var tag = tagsManager.getTag(id);
        tagsModal.showEdit(tag);
    }

    function deleteTag(id) {
        var tag = tagsManager.getTag(id);
        tagsModal.showDelete(tag);
    }

    function checkEmptyShowBlankPage() {
        if (page.find('.tag_item').length) {
            page.find('.tag_list').show();
            blankPages.hide();
            return false;
        } else {
            page.find('.tag_list').hide();
            blankPages.showEmptyTags();
            return true;
        }
    }

    return {
        init: init,

        show: show,
        hide: hide
    };
})(jQuery);