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


window.tagsPanel = (function($) {
    var isInit = false,
        tagsPanelContent,
        tagsMore,
        tagsPanel,
        listMaxHeight;

    function init() {
        if (isInit)
            return;

        isInit = true;

        tagsPanel = $('#tags_panel');

        tagsPanelContent = tagsPanel.find('#id_tags_panel_content');

        tagsMore = tagsPanel.find(".more");

        listMaxHeight = parseInt(tagsPanelContent.css("max-height").replace(/[^-\d\.]/g, ''));

        tagsManager.bind(tagsManager.events.OnRefresh, onRefreshTags);
        tagsManager.bind(tagsManager.events.OnDelete, onDeleteTag);
        tagsManager.bind(tagsManager.events.OnUpdate, onUpdateTag);
        tagsManager.bind(tagsManager.events.OnIncrement, onIncrement);
        tagsManager.bind(tagsManager.events.OnDecrement, onDecrement);

            //jq(window).on("resizeWinTimer", function () {
            //    updatePanel();
            //});
    }

    function expandTagsPanel() {
        tagsPanelContent.animate({ "max-height": tagsPanelContent[0].scrollHeight }, 200, function() {
            tagsMore.hide();
        });
    }

    function getTag$Html(tag) {

        tag.used = isTagInFilter(tag);

        var html = $.tmpl('tagInLeftPanelTmpl', tag, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);

        $html.click(function(e) {
            if (e.isPropagationStopped()) {
                return;
            }

            // google analytics
            window.ASC.Mail.ga_track(ga_Categories.leftPanel, ga_Actions.filterClick, 'tag');

            var tagid = $(this).attr('labelid');

            if (MailFilter.toggleTag(tagid)) {
                markTag(tagid);
            } else {
                unmarkTag(tagid);
            }

            mailBox.updateAnchor();

            TMMail.scrollTop();
        });

        return $html;
    }

    function isTagInFilter(tag) {
        return $.inArray(tag.id.toString(), MailFilter.getTags()) >= 0;
    }

    function onRefreshTags(e, tags) {
        tagsPanelContent.find('.tag[labelid]').remove();
        $.each(tags, function(index, tag) {
            if (0 >= tag.lettersCount) {
                return;
            }
            var $html = getTag$Html(tag);
            tagsPanelContent.append($html);
        });
        updatePanel();
    }

    function unmarkAllTags() {
        tagsPanelContent.find('.tag').removeClass().addClass('tag inactive');
    }

    function unmarkTag(tagid) {
        try {
            tagsPanelContent.find('.tag[labelid="' + tagid + '"]').removeClass().addClass('tag inactive');
        } catch (err) {
            console.error(err);
        }
    }

    function markTag(tagid) {
        try {
            var tag = tagsManager.getTag(tagid);
            var css = 'tagArrow tag' + tag.style;
            tagsPanelContent.find('.tag[labelid="' + tagid + '"]').removeClass('inactive').addClass(css);
        } catch (err) {
            console.error(err);
        }
    }

    function onUpdateTag(e, tag) {
        var tagDiv = tagsPanelContent.find('.tag[labelid="' + tag.id + '"]');
        tagDiv.find('.square').removeClass().addClass('square tag' + tag.style);
        tagDiv.find('.name').html(TMMail.ltgt(tag.name));
        updatePanel();
    }

    function deleteTag(id) {
        tagsPanelContent.find('.tag[labelid="' + id + '"]').remove();
        updatePanel();
    }

    function onDeleteTag(e, id) {
        deleteTag(id);
    }

    function insertTag(tag) {
        var $html = getTag$Html(tag);
        var tags = tagsPanelContent.find('.tag[labelid]');
        var insertFlag = false;
        $.each(tags, function (index, value) {
            var id = parseInt($(value).attr('labelid'));
            if ((tag.id > 0 && (id > tag.id || id < 0)) || (tag.id < 0 && id < tag.id)) {
                $(value).before($html);
                insertFlag = true;
                return false;
            }
        });

        if (!insertFlag) {
            tagsPanelContent.append($html);
        }
        updatePanel();
    }

    function onIncrement(e, tag) {
        if (0 === tagsPanelContent.find('.tag[labelid="' + tag.id + '"]').length) {
            insertTag(tag);
        }
    }

    function onDecrement(e, tag) {
        if (0 >= tag.lettersCount) {
            onDeleteTag(e, tag.id);
        }
    }

    function updatePanel() {
        if (0 === tagsPanelContent.find('.tag').length) {
            tagsPanel.hide();
            return;
        }
        tagsPanel.show();
        if (listMaxHeight < tagsPanelContent[0].scrollHeight) {
            tagsMore.show();
            tagsMore.find(".more_link").on("click", expandTagsPanel);
        } else {
            tagsMore.find(".more_link").off("click", expandTagsPanel);
            tagsMore.hide();
        }
    }

    return {
        init: init,
        unmarkAllTags: unmarkAllTags,
        unmarkTag: unmarkTag,
        markTag: markTag
    };
})(jQuery);