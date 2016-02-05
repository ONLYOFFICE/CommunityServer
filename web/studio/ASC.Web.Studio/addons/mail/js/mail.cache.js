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


window.mailCache = (function($) {

    function getContentEl(id) {
        id = parseInt(id);
        var $messageRow = $('#itemContainer .messages tr[data_id=' + id + ']');
        return $messageRow;
    }

    function set(id, content) {
        var $messageRow = getContentEl(id);
        if ($messageRow) {
            $messageRow.data('content', content);
        }
    }

    function get(id) {
        var $messageRow = getContentEl(id);
        if ($messageRow && $messageRow.data('content')) {
            return $messageRow.data('content');
        }

        return undefined;
    }

    function setHasLinked(id, hasLinked) {
        var content = get(id);
        if (content) {
            content.hasLinked = hasLinked;
            set(id, content);
        }
    }

    function setImportant(id, important) {
        var content = get(id);
        if (content) {
            var needUpdate = false;
            for (var i = 0, len = content.messages.length; i < len; i++) {
                var msg = content.messages[i];
                if (msg.important !== important) {
                    msg.important = important;
                    needUpdate = true;
                }
            }
            if (needUpdate)
                set(id, content);
        }
    }

    function setTag(id, tagId, remove) {
        var content = get(id);
        if (content) {
            tagId = parseInt(tagId);
            var needUpdate = false;
            for (var i = 0, len = content.messages.length; i < len; i++) {
                var msg = content.messages[i];
                if (msg.tagIds && msg.tagIds.length > 0) {
                    var index = $.inArray(tagId, msg.tagIds);
                    if (index > -1) {
                        if (remove) {
                            msg.tagIds.splice(index, 1);
                            needUpdate = true;
                        }
                    } else {
                        if (!remove) {
                            msg.tagIds.push(tagId);
                            needUpdate = true;
                        }
                    }
                    
                } else {
                    if (!remove) {
                        msg.tagIds = [tagId];
                        needUpdate = true;
                    }
                }
            }
            if (needUpdate)
                set(id, content);
        }
    }

    return {
        get: get,
        set: set,
        setHasLinked: setHasLinked,
        setImportant: setImportant,
        setTag: setTag
    };

})(jQuery);