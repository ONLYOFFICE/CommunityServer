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

window.tagsPanel = (function($) {
    var 
        isInit = false,
        firstLoad = true,
        _panel_content,
        _panel_max_h;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            _panel_content = $('#id_tags_panel_content');

            _panel_max_h = _panel_content.parent().css("max-height").replace(/[^-\d\.]/g, '');
            $('#tags_panel').hover(_expand_tags_panel, _collapse_tags_panel);

            tagsManager.events.bind('refresh', _onRefreshTags);
            tagsManager.events.bind('delete', _onDeleteTag);
            tagsManager.events.bind('update', _onUpdateTag);
            tagsManager.events.bind('increment', _onIncrement);
            tagsManager.events.bind('decrement', _onDecrement);
        }
    };

    var _expand_tags_panel = function () {
       _panel_content.parent().stop().animate({ "max-height": _panel_content.height() }, 200, function() {
            $('#tags_panel .more').css({ 'visibility': 'hidden' });
       });
    };

    var _collapse_tags_panel = function() {
        _panel_content.parent().stop().animate({ "max-height": _panel_max_h }, 200, function() {
            $('#tags_panel .more').css({ 'visibility': 'visible' });
        });
    };

    var _getTag$html = function(tag) {

        tag.used = isTagInFilter(tag);

        var html = $.tmpl('tagInLeftPanelTmpl', tag, { htmlEncode: TMMail.htmlEncode });

        var $html = $(html);

        $html.click(function(e) {
            if (e.isPropagationStopped())
                return;

            // google analytics
            window.ASC.Mail.ga_track(ga_Categories.leftPanel, ga_Actions.filterClick, 'tag');

            var tagid = $(this).attr('labelid');

            if (MailFilter.toggleTag(tagid))
                markTag(tagid);
            else
                unmarkTag(tagid);

            mailBox.updateAnchor();
        });

        return $html;
    };

    function isTagInFilter(tag) {
        return $.inArray(tag.id.toString(), MailFilter.getTags()) >= 0;
    }

    var _onRefreshTags = function(e, tags) {
        _panel_content.find('.tag[labelid]').remove();
        $.each(tags, function(index, tag) {
            if (0 >= tag.lettersCount)
                return;
            var $html = _getTag$html(tag);
            _panel_content.append($html);
        });
        _updatePanel();
    };

    var unmarkAllTags = function() {
        _panel_content.find('.tag').removeClass().addClass('tag inactive');
    };

    var unmarkTag = function (tagid) {
        try {
            _panel_content.find('.tag[labelid="' + tagid + '"]').removeClass().addClass('tag inactive');
        }
        catch (err) { }
    };

    var markTag = function (tagid) {
        try {
            var tag = tagsManager.getTag(tagid);
            var css = 'tagArrow tag' + tag.style;
            _panel_content.find('.tag[labelid="' + tagid + '"]').removeClass('inactive').addClass(css);
        }
        catch (err) { }
    };

    var _onUpdateTag = function(e, tag) {
        /*if (0 >= tag.lettersCount) {
            _deleteTag(tag.id);
            return;
        }
        if (0 == _panel_content.find('.tag[labelid="' + tag.id + '"]').length) {
            _insertTag(tag);
            return;
        }*/
        var tag_div = _panel_content.find('.tag[labelid="' + tag.id + '"]');
        tag_div.find('.square').removeClass().addClass('square tag' + tag.style);
        tag_div.find('.name').html(TMMail.ltgt(tag.name));
        _updatePanel();
    };

    var _deleteTag = function(id) {
        _panel_content.find('.tag[labelid="' + id + '"]').remove();
        _updatePanel();
    };
    var _onDeleteTag = function(e, id) {
        _deleteTag(id);
    };

    var _insertTag = function(tag) {
        var $html = _getTag$html(tag);
        var tags = _panel_content.find('.tag[labelid]');
        var insert_flag = false;
        $.each(tags, function(index, value) {
            var id = parseInt($(value).attr('labelid'));
            if ((tag.id > 0 && (id > tag.id || id < 0)) || (tag.id < 0 && id < tag.id)) {
                $(value).before($html);
                insert_flag = true;
                return false;
            }
        });
        if (!insert_flag)
            _panel_content.append($html);
        _updatePanel();
    };
    var _onIncrement = function(e, tag) {
        if (0 == _panel_content.find('.tag[labelid="' + tag.id + '"]').length)
            _insertTag(tag);
    };

    var _onDecrement = function(e, tag) {
        if (0 >= tag.lettersCount)
            _onDeleteTag(e, tag.id);
    };

    var _updatePanel = function() {
        if (0 == $('#tags_panel .tag').length) {
            $('#tags_panel').hide();
            return;
        }
        $('#tags_panel').show();
        if (_panel_max_h < _panel_content.height())
            $('#tags_panel .more').show();
        else
            $('#tags_panel .more').hide();
    };

    return {
        init: init,
        unmarkAllTags: unmarkAllTags,
        unmarkTag: unmarkTag,
        markTag: markTag
    };
})(jQuery);