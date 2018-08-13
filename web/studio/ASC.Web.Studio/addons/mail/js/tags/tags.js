/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


window.tagsManager = (function($) {
    var isInit = false,
        tags = [],
        events = $({});

    var init = function() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(Teamlab.events.removeMailTag, onDeleteMailTag);
            serviceManager.bind(Teamlab.events.getMailTags, onGetMailTags);

            tagsPanel.init();
            tagsColorsPopup.init();
            tagsDropdown.init();
            tagsPage.init();

            if (ASC.Mail.Presets.Tags) {
                var tagList = $.map(ASC.Mail.Presets.Tags, function (el) {
                    el.name = TMMail.htmlDecode(el.name);
                    return el;
                });

                onGetMailTags({}, tagList);
            }

        }
    };

    function convertServerTag(serverTag) {
        var tag = {};
        tag.id = serverTag.id;
        tag.name = serverTag.name;
        tag.short_name = cutTagName(tag.name);
        tag.style = serverTag.style;
        if (0 > tag.id) {
            tag.style = Math.abs(tag.id) % 16 + 1;
        }
        tag.addresses = serverTag.addresses;
        tag.lettersCount = serverTag.lettersCount;
        return tag;
    }

    function onGetMailTags(params, tagsArr) {
        tags = $.map(tagsArr, convertServerTag);
        events.trigger('refresh', [tags]);
    }

    var onUpdateMailTag = function(params, tag) {
        tag = convertServerTag(tag);
        $.each(tags, function(i) {
            if (tag.id == tags[i].id) {
                tags[i] = tag;
                events.trigger('update', tag);
                return false; //break
            }
        });
    };

    var onCreateMailTag = function(params, tag) {
        tag = convertServerTag(tag);
        var mailTags = $.grep(tags, function(item) { return item.id > 0 ? true : false; });
        tags.splice(mailTags.length, 0, tag);
        var prevTagId = mailTags.length > 0 ? mailTags[mailTags.length - 1].id : undefined;
        events.trigger('create', [tag, prevTagId]);
    };

    var onErrorCreateMailTag = function(params, errors) {
        events.trigger('error', { message: errors[0], comment: '' });
    };

    var onDeleteMailTag = function(params, id) {
        tags = $.grep(tags, function(tag) {
            return tag.id != id;
        });
        events.trigger('delete', id);
    };

    var getTag = function(tagid) {
        var res = undefined;
        $.each(tags, function(i, v) {
            if (v.id == tagid) {
                res = v;
                return false;
            }
        });
        return res;
    };

    var getTagByName = function(name) {
        for (var i = 0; i < tags.length; i++) {
            if (tags[i].name.toLowerCase() == name.toLowerCase()) {
                return tags[i];
            }
        }
        return undefined;
    };

    var getAllTags = function() {
        return tags;
    };

    function raiseAlreadyExistsError(tagname) {
        events.trigger('error', { message: MailScriptResource.ErrorTagNameAlreadyExists.replace('%1', '\"' + tagname + '\"'), comment: '' });
    }

    var createTag = function(tag) {
        if (getTagByName(tag.name)) {
            raiseAlreadyExistsError(tag.name);
            return;
        }

        if (!tag.addresses) {
            tag.addresses = [];
        }

        serviceManager.createTag(tag.name, tag.style, tag.addresses, {}, { success: onCreateMailTag, error: onErrorCreateMailTag });
    };

    var updateTag = function(tag) {
        var foundTag = getTagByName(tag.name);
        if (foundTag && tag.id != foundTag.id) {
            raiseAlreadyExistsError(tag.name);
            return;
        }

        serviceManager.updateTag(tag.id, tag.name, tag.style, tag.addresses, {}, { success: onUpdateMailTag, error: onErrorCreateMailTag });
    };

    var deleteTag = function(id) {
        serviceManager.deleteTag(id, {}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
    };

    var getVacantStyle = function() {
        var mailTags = $.grep(tags, function(item) { return item.id > 0 ? true : false; });
        if (mailTags.length > 0) {
            return (parseInt(mailTags[mailTags.length - 1].style)) % 16 + 1;
        }
        return 1;
    };

    var increment = function(id) {
        var tag = getTag(id);
        if (tag) {
            tag.lettersCount += 1;
            events.trigger('increment', [tag]);
        }
    };

    var decrement = function(id) {
        var tag = getTag(id);
        if (tag) {
            if (tag.lettersCount > 0) {
                tag.lettersCount -= 1;
            }
            events.trigger('decrement', [tag]);
        }
    };

    var cutTagName = function(tagName) {
        var hardcodedTagNameForViewLength = 25;
        var lastSlashIndex = tagName.lastIndexOf('/');
        var resultName;
        if (-1 == lastSlashIndex || tagName.length < hardcodedTagNameForViewLength) {
            resultName = tagName;
        } else {
            if ((tagName.length - lastSlashIndex) < hardcodedTagNameForViewLength) {
                var lengthOfBefin = hardcodedTagNameForViewLength - (tagName.length - lastSlashIndex) - 3;
                resultName = tagName.substr(0, lengthOfBefin) + '...' + tagName.substr(lastSlashIndex);
            } else {
                var lengthOfTagEnd = hardcodedTagNameForViewLength - 3;
                resultName = '...' + tagName.substr(tagName.length - lengthOfTagEnd, lengthOfTagEnd);
            }
        }
        return resultName;
    };

    return {
        init: init,
        getTag: getTag,
        getTagByName: getTagByName,
        getAllTags: getAllTags,

        createTag: createTag,
        updateTag: updateTag,
        deleteTag: deleteTag,
        getVacantStyle: getVacantStyle,

        increment: increment,
        decrement: decrement,

        events: events
    };
})(jQuery);