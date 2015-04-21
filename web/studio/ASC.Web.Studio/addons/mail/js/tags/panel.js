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


window.tagsPanel = (function($) {
    var isInit = false,
        panelContent,
        panelMaxH;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            panelContent = $('#id_tags_panel_content');

            panelMaxH = panelContent.parent().css("max-height").replace(/[^-\d\.]/g, '');
            $('#tags_panel').hover(expandTagsPanel, collapseTagsPanel);

            tagsManager.events.bind('refresh', onRefreshTags);
            tagsManager.events.bind('delete', onDeleteTag);
            tagsManager.events.bind('update', onUpdateTag);
            tagsManager.events.bind('increment', onIncrement);
            tagsManager.events.bind('decrement', onDecrement);
        }
    };

    var expandTagsPanel = function() {
        panelContent.parent().stop().animate({ "max-height": panelContent.height() }, 200, function() {
            $('#tags_panel .more').css({ 'visibility': 'hidden' });
        });
    };

    var collapseTagsPanel = function() {
        panelContent.parent().stop().animate({ "max-height": panelMaxH }, 200, function() {
            $('#tags_panel .more').css({ 'visibility': 'visible' });
        });
    };

    var getTag$Html = function(tag) {

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
        });

        return $html;
    };

    function isTagInFilter(tag) {
        return $.inArray(tag.id.toString(), MailFilter.getTags()) >= 0;
    }

    var onRefreshTags = function(e, tags) {
        panelContent.find('.tag[labelid]').remove();
        $.each(tags, function(index, tag) {
            if (0 >= tag.lettersCount) {
                return;
            }
            var $html = getTag$Html(tag);
            panelContent.append($html);
        });
        updatePanel();
    };

    var unmarkAllTags = function() {
        panelContent.find('.tag').removeClass().addClass('tag inactive');
    };

    var unmarkTag = function(tagid) {
        try {
            panelContent.find('.tag[labelid="' + tagid + '"]').removeClass().addClass('tag inactive');
        } catch(err) {
        }
    };

    var markTag = function(tagid) {
        try {
            var tag = tagsManager.getTag(tagid);
            var css = 'tagArrow tag' + tag.style;
            panelContent.find('.tag[labelid="' + tagid + '"]').removeClass('inactive').addClass(css);
        } catch(err) {
        }
    };

    var onUpdateTag = function(e, tag) {
        var tagDiv = panelContent.find('.tag[labelid="' + tag.id + '"]');
        tagDiv.find('.square').removeClass().addClass('square tag' + tag.style);
        tagDiv.find('.name').html(TMMail.ltgt(tag.name));
        updatePanel();
    };

    var deleteTag = function(id) {
        panelContent.find('.tag[labelid="' + id + '"]').remove();
        updatePanel();
    };

    var onDeleteTag = function(e, id) {
        deleteTag(id);
    };

    var insertTag = function(tag) {
        var $html = getTag$Html(tag);
        var tags = panelContent.find('.tag[labelid]');
        var insertFlag = false;
        $.each(tags, function(index, value) {
            var id = parseInt($(value).attr('labelid'));
            if ((tag.id > 0 && (id > tag.id || id < 0)) || (tag.id < 0 && id < tag.id)) {
                $(value).before($html);
                insertFlag = true;
                return false;
            }
        });

        if (!insertFlag) {
            panelContent.append($html);
        }
        updatePanel();
    };

    var onIncrement = function(e, tag) {
        if (0 == panelContent.find('.tag[labelid="' + tag.id + '"]').length) {
            insertTag(tag);
        }
    };

    var onDecrement = function(e, tag) {
        if (0 >= tag.lettersCount) {
            onDeleteTag(e, tag.id);
        }
    };

    var updatePanel = function() {
        if (0 == $('#tags_panel .tag').length) {
            $('#tags_panel').hide();
            return;
        }
        $('#tags_panel').show();
        if (panelMaxH < panelContent.height()) {
            $('#tags_panel .more').show();
        } else {
            $('#tags_panel .more').hide();
        }
    };

    return {
        init: init,
        unmarkAllTags: unmarkAllTags,
        unmarkTag: unmarkTag,
        markTag: markTag
    };
})(jQuery);