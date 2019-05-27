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
        eventsHandler = $({}),
        supportedCustomEvents = {
            OnCreate: "tags.create",
            OnUpdate: "tags.update",
            OnDelete: "tags.delete",
            OnIncrement: "tags.increment",
            OnDecrement: "tags.decrement",
            OnError: "tags.error",
            OnRefresh: "tags.refresh"
        };

    function init() {
        if (isInit === false) {
            isInit = true;

            window.Teamlab.bind(Teamlab.events.removeMailTag, onDeleteMailTag);
            window.Teamlab.bind(Teamlab.events.getMailTags, onGetMailTags);

            tagsPanel.init();
            tagsColorsPopup.init();
            tagsDropdown.init();
            tagsPage.init();

            if (ASC.Mail.Presets.Tags) {
                var tagList = $.map(ASC.Mail.Presets.Tags, function (tag) {
                    tag.id = +tag.id;
                    tag.name = TMMail.htmlDecode(tag.name);
                    return tag;
                });

                onGetMailTags({}, tagList);
            }

        }
    }

    function convertServerTag(serverTag) {
        var tag = {};
        tag.id = +serverTag.id;
        tag.name = serverTag.name;
        tag.short_name = cutTagName(tag.name);
        tag.style = serverTag.style;
        if (0 > tag.id) {
            tag.style = Math.abs(tag.id) % 16 + 1;
        }
        tag.addresses = serverTag.addresses;
        tag.lettersCount = serverTag.lettersCount;
        tag.used = false;
        return tag;
    }

    function onGetMailTags(params, tagList) {
        tags = tagList.map(convertServerTag);
        eventsHandler.trigger(supportedCustomEvents.OnRefresh, [tags]);
    }

    function onUpdateMailTag(params, tag) {
        var newTag = convertServerTag(tag);

        for (var i = 0; i < tags.length; i++) {
            if (tags[i].id === newTag.id) {
                tags[i] = newTag;
                eventsHandler.trigger(supportedCustomEvents.OnUpdate, newTag);
                break;
            }
        }

        return newTag;
    }

    function onCreateMailTag(params, tag) {
        var newTag = convertServerTag(tag);

        var mailTags = $.grep(tags,
            function(t) {
                return t.id > 0 ? true : false;
            });

        tags.splice(mailTags.length, 0, newTag);

        var prevTagId = mailTags.length > 0
            ? mailTags[mailTags.length - 1].id
            : undefined;

        eventsHandler.trigger(supportedCustomEvents.OnCreate, [newTag, prevTagId]);

        return newTag;
    }

    function onError(params, errors) {
        eventsHandler.trigger(supportedCustomEvents.OnError, { message: errors[0], comment: '' });
        return errors;
    }

    function onDeleteMailTag(params, id) {
        id = +id;
        tags = $.grep(tags, function(tag) {
            return tag.id !== id;
        });
        eventsHandler.trigger(supportedCustomEvents.OnDelete, id);
    }

    function getTag(tagid) {
        tagid = +tagid;

        for (var i = 0; i < tags.length; i++) {
            var tag = tags[i];
            if (tag.id === tagid) {
                return tags[i];
            }
        }

        return undefined;
    }

    function getTagByName(name) {
        for (var i = 0; i < tags.length; i++) {
            if (tags[i].name.toLowerCase() === name.toLowerCase()) {
                tags[i].id = +tags[i].id;
                return tags[i];
            }
        }
        return undefined;
    }

    function getAllTags() {
        return tags;
    }

    function createTag(tag) {
        var d = $.Deferred();

        var newTag = convertServerTag(tag);

        if (getTagByName(newTag.name)) {
            var error = MailScriptResource.ErrorTagNameAlreadyExists.replace('%1', '\"' + newTag.name + '\"');
            return d.reject(onError({}, [error]))
        }

        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.tagsManagement, ga_Actions.createNew, "create_new_tag");

        serviceManager.createTag(newTag.name,
            newTag.style,
            newTag.addresses,
            {},
            {
                success: function(params, serverTag) {
                    d.resolve(onCreateMailTag(params, serverTag));
                },
                error: function(params, errors) {
                    d.reject(onError(params, errors));
                }
            });

        return d.promise();
    }

    function updateTag(tag) {
        var d = $.Deferred();

        var newTag = convertServerTag(tag);

        var foundTag = getTagByName(newTag.name);
        if (foundTag && newTag.id !== foundTag.id) {
            var error = MailScriptResource.ErrorTagNameAlreadyExists.replace('%1', '\"' + newTag.name + '\"');
            return d.reject(onError({}, [error]));
        }

        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.tagsManagement, ga_Actions.update, "update_tag");

        serviceManager.updateTag(newTag.id,
            newTag.name,
            newTag.style,
            newTag.addresses,
            {},
            {
                success: function(params, serverTag) {
                    d.resolve(onUpdateMailTag(params, serverTag));
                },
                error: function(params, errors) {
                    d.reject(onError(params, errors));
                }
            });

        return d.promise();
    }

    function deleteTag(id) {
        serviceManager.deleteTag(id, {}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function getVacantStyle() {
        var mailTags = $.grep(tags, function(item) { return item.id > 0 ? true : false; });
        if (mailTags.length > 0) {
            return (parseInt(mailTags[mailTags.length - 1].style)) % 16 + 1;
        }
        return 1;
    }

    function increment(id) {
        var tag = getTag(id);
        if (tag) {
            tag.lettersCount += 1;
            eventsHandler.trigger(supportedCustomEvents.OnIncrement, [tag]);
        }
    }

    function decrement(id) {
        var tag = getTag(id);
        if (tag) {
            if (tag.lettersCount > 0) {
                tag.lettersCount -= 1;
            }
            eventsHandler.trigger(supportedCustomEvents.OnDecrement, [tag]);
        }
    }

    function cutTagName(tagName) {
        var hardcodedTagNameForViewLength = 25;
        var lastSlashIndex = tagName.lastIndexOf('/');
        var resultName;
        if (-1 === lastSlashIndex || tagName.length < hardcodedTagNameForViewLength) {
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
    }

    function bind(eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        eventsHandler.unbind(eventName);
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,

        getTag: getTag,
        getTagByName: getTagByName,
        getAllTags: getAllTags,

        createTag: createTag,
        updateTag: updateTag,
        deleteTag: deleteTag,
        getVacantStyle: getVacantStyle,

        increment: increment,
        decrement: decrement
    };
})(jQuery);