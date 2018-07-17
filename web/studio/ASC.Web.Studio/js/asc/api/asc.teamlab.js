/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


;
window.Teamlab = (function () {
    var ADD = 'post',
        UPDATE = 'put',
        REMOVE = 'delete',
        GET = 'get',
        helper = ServiceHelper,
        isInit = false,
        customEvents = {
        getException: 'ongetexception',
        getAuthentication: 'ongetauthentication',

        addPrjComment: 'onaddprjcomment',
        updatePrjTime: 'onupdateprjtime',
        removePrjTime: 'onremoveprjtime',

        getMailFilteredMessages: 'ongetmailmessages',
        getMailFolders: 'ongetmailfolders',
        getAccounts: 'ongetmailaccounts',
        getMailTags: 'ongetmailtags',
        getMailConversation: 'ongetmailconversation',
        getNextMailConversationId: 'ongetnextmailconversationid',
        getPrevMailConversationId: 'ongetprevmailconversationid',
        getMailMessage: 'ongetmailmessage',
        getNextMailMessageId: 'ongetnextmailmessageid',
        getPrevMailMessageId: 'ongetprevmailmessageid',
        removeMailFolderMessages: 'onremovemailfoldermessages',
        restoreMailMessages: 'onrestoremailmessages',
        moveMailMessages: 'onmovemailmessages',
        removeMailMessages: 'onremovemailmessages',
        markMailMessages: 'onmarkmailmessages',
        createMailTag: 'oncreatemailtag',
        updateMailTag: 'onupdatemailtag',
        removeMailTag: 'onremovemailtag',
        addMailDocument: 'onaddmaildocument',
        setMailTag: 'onsetmailtag',
        setMailConversationsTag: 'onsetmailconversationstag',
        unsetMailTag: 'unsetmailtag',
        unsetMailConversationsTag: 'unsetmailconversationstag',
        removeMailMailbox: 'onremovemailmailbox',
        getMailDefaultMailboxSettings: 'ongetmaildefaultmailboxsettings',
        getMailMailbox: 'ongetmailmailbox',
        setDefaultAccount: 'onsetdefaultaccount',
        createMailMailboxSimple: 'oncreatemailmailboxsimple',
        createMailMailboxOAuth: 'oncreateMailMailboxOAuth',
        updateMailMailboxOAuth: 'updateMailMailboxOAuth',
        createMailMailbox: 'oncreatemailmailbox',
        updateMailMailbox: 'onupdatemailmailbox',
        setMailMailboxState: 'onsetmailmailboxstate',
        removeMailMessageAttachment: 'onremovemailmessageattachment',
        sendMailMessage: 'onsendmailmessage',
        saveMailMessage: 'onsavemailmessage',
        searchEmails: 'ongetSearchEmails',
        getMailContacts: 'ongetMailContacts',
        getMailContactsByInfo: 'ongetMailContactsByInfo',
        createMailContact: 'oncreateMailContact',
        deleteMailContacts: 'ondeleteMailContacts',
        updateMailContact: 'onupdateMailContact',
        getMailAlerts: 'ongetmailalerts',
        deleteMailAlert: 'ondeletemailalert',
        moveMailConversations: 'onmovemailconversations',
        restoreMailConversations: 'onrestoremailconversations',
        removeMailConversations: 'onremovemailconversations',
        markMailConversations: 'onmarkmailconversations',
        getMailFilteredConversations: 'ongetmailfilteredconversations',
        getMailDisplayImagesAddresses: 'ongetmaildisplayimagesaddresses',
        createDisplayImagesAddress: 'oncreatedisplayimagesaddress',
        removeDisplayImagesAddress: 'onremovedisplayimagesaddress',
        getLinkedCrmEntitiesInfo: 'ongetlinkedcrmentitiesinfo',
        linkChainToCrm: 'onlinkchaintocrm',
        markChainAsCrmLinked: 'onmarkchainascrmlinked',
        unmarkChainAsCrmLinked: 'onunmarkchainascrmlinked',
        isConversationLinkedWithCrm: 'ongetcrmlinked',
        exportMessageToCrm: 'onexportmessagetocrm',
        getMailboxSignature: 'ongetmailboxsignature',
        updateMailboxSignature: 'onupdatemailboxsignature',
        updateMailboxAutoreply: 'onupdatemailboxautoreply',
        exportAllAttachmentsToMyDocuments: 'exportallattachmentstomydocuments',
        exportAllAttachmentsToDocuments: 'exportallattachmentstodocuments',
        exportAttachmentToMyDocuments: 'exportattachmenttomydocuments',
        exportAttachmentToDocuments: 'exportAttachmentToDocuments',
        setEMailInFolder: 'onsetemailinfolder',
        getMailServer: 'getmailserver',
        getMailServerFullInfo: 'getmailserverfullinfo',
        getMailServerFreeDns: 'getmailserverfreedns',
        getMailDomains: 'getmaildomains',
        addMailDomain: 'addmaildomain',
        removeMailDomain: 'removemaildomain',
        addMailbox: 'addmailbox',
        getMailboxes: 'getmailboxes',
        removeMailbox: 'removeMailbox',
        addMailBoxAlias: 'addmailboxalias',
        updateMailbox: 'updateMailbox',
        removeMailBoxAlias: 'removemailboxalias',
        addMailGroup: 'addmailgroup',
        addMailGroupAddress: 'addmailgroupaddress',
        getMailGroups: 'getmailgroups',
        removeMailGroup: 'removemailgroup',
        removeMailGroupAddress: 'removemailgroupaddress',
        isDomainExists: 'isdomainexists',
        checkDomainOwnership: 'checkdomainownership',
        getDomainDnsSettings: 'getdomaindnssettings',
        createNotificationAddress: 'createnotificationaddress',
        removeNotificationAddress: 'removenotificationaddress',
        addCalendarBody: 'addcalendarbody',
        setMailConversationEnabledFlag: 'setmailconversationenabledflag',
        setMailAlwaysDisplayImagesFlag: 'setmailalwaysdisplayimagesflag',
        setMailCacheUnreadMessagesFlag: 'setmailcacheunreadmessagesflag',
        setMailEnableGoNextAfterMove: 'setmailenablegonextaftermove',
        getMailOperationStatus: 'getmailoperationstatus',

        getTalkUnreadMessages: 'gettalkunreadmessages',

        addSubtask: 'addsubtask',
        removeSubtask: 'removesubtask',
        updateSubtask: 'updateSubtask',
        removePrjTask: 'removePrjTask',
        removePrjTasks: 'removePrjTasks',
        updatePrjTask: 'updatePrjTask',
        updatePrjTaskStatus: 'updatePrjTaskStatus',
        updatePrjTasksStatus: 'updatePrjTasksStatus',
        addPrjTask: 'addPrjTask',
        copyPrjTask: 'copyPrjTask',
        addPrjMilestone: 'addPrjMilestone',
        updatePrjMilestone: 'updatePrjMilestone',
        updatePrjMilestoneStatus: 'updatePrjMilestoneStatus',
        removePrjMilestone: 'removePrjMilestone',
        updatePrjTeam: 'updatePrjTeam',
        removePrjTeam: 'removePrjTeam',
        getPrjProject: 'getPrjProject',
        updatePrjProjectStatus: 'updatePrjProjectStatus',
        getCrmContactsForProject: 'getCrmContactsForProject',
        addCrmContactForProject: 'addCrmContactForProject',
        removeCrmContactFromProject: 'removeCrmContactFromProject',
        getDocFolder: 'getDocFolder'
    },
        customEventsHash = {},
        eventManager = new CustomEvent(customEvents);

    extendCustomEventsHash();

    function isArray(o) {
        return o ? o.constructor.toString().indexOf("Array") != -1 : false;
    }

    function getQuery(o) {
        return o && typeof o === 'object' && o.hasOwnProperty('query') ? o.query : null;
    }

    function extendCustomEventsHash() {
        for (var fld in customEvents) {
            customEventsHash[fld] = true;
            customEventsHash[customEvents[fld]] = true;
        }
    }

    function callMethodByName(handlername, container, self, args) {
        handlername = handlername.replace(/-/g, '_');
        if (container && typeof container === 'object' && typeof container[handlername] === 'function') {
            container[handlername].apply(self, args);
        }
    }

    function returnValue(value) {
        return value && isArray(value) ? window.Teamlab : value;
    }

    var init = function() {
        if (isInit === true) {
            return undefined;
        }
        isInit = true;

        helper.bind(null, onGetResponse);
        helper.bind('event', onGetEvent);
        helper.bind('extention', onGetExtention);
        //serviceManager.bind('me', onGetOwnProfile);
    };

    var bind = function(eventname, handler, params) {
        return eventManager.bind(eventname, handler, params);
    };

    var unbind = function(handlerid) {
        return eventManager.unbind(handlerid);
    };

    var call = function(eventname, self, args) {
        eventManager.call(eventname, self, args);
    };

    function onGetEvent(eventname, self, args) {
        if (customEventsHash.hasOwnProperty(eventname)) {
            call(eventname, self, args);
        }
    }

    function onGetExtention(eventname, params, errors) {
        eventManager.call(customEvents.getException, this, [params, errors]);
    }

    function onGetResponse(params, obj) {
        if (params.hasOwnProperty('___handler') && params.hasOwnProperty('___container')) {
            var args = [params];
            for (var i = 1, n = arguments.length; i < n; i++) {
                args.push(arguments[i]);
            }
            callMethodByName(params.___handler, params.___container, this, args);
        }
    }

    var joint = function() {
        helper.joint();
        return window.Teamlab;
    };

    var start = function(params, options) {
        return helper.start(params, options);
    };

    function addRequest(eventname, params, type, url, data, options) {
        return returnValue(helper.request(
            eventname,
            params,
            type,
            url,
            data,
            options
        ));
    }

    /* <common> */
    var getQuotas = function (params, options) {
        return addRequest(
            customEvents.getQuotas,
            params,
            GET,
            'settings/quota.json',
            null,
            options
        );
    };

    var recalculateQuota = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'settings/recalculatequota.json',
            null,
            options
        );
    };

    var checkRecalculateQuota = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'settings/checkrecalculatequota.json',
            null,
            options
        );
    };

    /* </common> */

    /* <people> */

    var remindPwd = function (params, email, options) {
        return addRequest(
            null,
            params,
            ADD,
            'people/password.json',
            { email: email },
            options
        );
    };

    var thirdPartyLinkAccount = function (params, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/thirdparty/linkaccount.json',
            data,
            options
        );
    };

    var thirdPartyUnLinkAccount = function (params, data, options) {
        return addRequest(
            customEvents.thirdPartyUnLinkAccount,
            params,
            REMOVE,
            'people/thirdparty/unlinkaccount.json',
            data,
            options
        );
    };

    var addProfile = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'people.json',
            data,
            options
        );
        return true;
    };

    var getProfile = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'people/' + id + '.json',
            null,
            options
        );
    };

    var getProfiles = function (params, options) {
        var status = null;
        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            status = options.filter.hasOwnProperty('status') ? options.filter.status : status;
        }

        var query = getQuery(options),
            url = 'people' + (status !== null ? '/status/' + status : '') + '.json';

        if (query) {
            if (options.filter) {
                options.filter.query = query;

            } else {
                options.filter = { query: query };
            }
            url = 'people' + (status !== null ? '/status/' + status : '') + '/search.json';
        }

        return addRequest(
            null,
            params,
            GET,
            url,
            null,
            options
        );
    };

    var getProfilesByFilter = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'people/filter.json',
            null,
            options
        );
    };

    var addGroup = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'group.json',
            data,
            options
        );
        return true;
    };

    var getGroup = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'group/' + id + '.json',
            null,
            options
        );
    };

    var getGroups = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'group.json',
            null,
            options
        );
    };

    var updateGroup = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'group/' + id + '.json',
            data,
            options
        );
    };

    var deleteGroup = function (params, id, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'group/' + id + '.json',
            null,
            options
        );
    };

    var updateProfile = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'people/' + id + '.json',
            data,
            options
        );
    };

    var updateUserType = function (params, type, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/type/' + type + '.json',
            data,
            options
        );
    };

    var updateUserStatus = function (params, status, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/status/' + status + '.json',
            data,
            options
        );
    };
    var updateUserPhoto = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/' + id + '/photo.json',
            data,
            options
        );
    };

    var removeUserPhoto = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'people/' + id + '/photo.json',
            data,
            options
        );
    };

    var sendInvite = function (params, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/invite.json',
            data,
            options
        );
    };

    var removeUser = function (params, id, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'people/' + id + '.json',
            null,
            options
        );
    };

    var removeUsers = function (params, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/delete.json',
            data,
            options
        );
    };

    var getUserGroups = function (id, options) {
        return addRequest(
            null,
            null,
            GET,
            'group/user/' + id + '.json',
            null,
            options
        );
    };

    var removeSelf = function (params, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/self/delete.json',
            null,
            options
        );
    };

    var joinAffiliate = function (params, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'people/self/joinaffiliate.json',
            null,
            options
        );
    };


    /* </people> */

    /* <community> */
    var addCmtBlog = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/blog.json',
            data,
            options
        );
        return true;
    };

    var getCmtBlog = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/blog/' + id + '.json',
            null,
            options
        );
    };

    var getCmtBlogs = function (params, options) {
        var query = getQuery(options);
        return addRequest(
            null,
            params,
            GET,
            'community/blog' + (query ? '/@search/' + query : '') + '.json',
            null,
            options
        );
    };

    var addCmtForumTopic = function (params, threadid, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/forum/' + threadid + '.json',
            data,
            options
        );
        return true;
    };

    var getCmtForumTopic = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/forum/topic/' + id + '.json',
            null,
            options
        );
    };

    var getCmtForumTopics = function (params, options) {
        var query = getQuery(options);
        return addRequest(
            null,
            params,
            GET,
            'community/forum' + (query ? '/@search/' + query : '/topic/recent') + '.json',
            null,
            options
        );
    };

    var getCmtForumCategories = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/forum.json',
            null,
            options
        );
    };

    var addCmtForumToCategory = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'community/forum.json',
            data,
            options
        );
    };

    var addCmtEvent = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/event.json',
            data,
            options
        );
        return true;
    };

    var getCmtEvent = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/event/' + id + '.json',
            null,
            options
        );
    };

    var getCmtEvents = function (params, options) {
        var query = getQuery(options);
        return addRequest(
            null,
            params,
            GET,
            'community/event' + (query ? '/@search/' + query : '') + '.json',
            null,
            options
        );
    };

    var addCmtBookmark = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/bookmark.json',
            data,
            options
        );
        return true;
    };

    var getCmtBookmark = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/bookmark/' + id + '.json',
            null,
            options
        );
    };

    var getCmtBookmarks = function (params, options) {
        var query = getQuery(options);
        return addRequest(
            null,
            params,
            GET,
            'community/bookmark' + (query ? '/@search/' + query : '/top/recent') + '.json',
            null,
            options
        );
    };

    var addCmtForumTopicPost = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/forum/topic/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var addCmtBlogComment = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/blog/' + id + '/comment.json',
            data,
            options
        );
        return true;
    };

    var getCmtBlogComments = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/blog/' + id + '/comment.json',
            null,
            options
        );
    };

    var addCmtEventComment = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/event/' + id + '/comment.json',
            data,
            options
        );
        return true;
    };

    var getCmtEventComments = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/event/' + id + '/comment.json',
            null,
            options
        );
    };

    var subscribeCmtEventComment = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/event/' + id + '/subscribe.json',
            data,
            options
        );
        return true;
    };

    var addCmtBookmarkComment = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/bookmark/' + id + '/comment.json',
            data,
            options
        );
        return true;
    };

    var getCmtBookmarkComments = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'community/bookmark/' + id + '/comment.json',
            null,
            options
        );
    };

    var subscribeCmtBirthday = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/birthday.json',
            data,
            options
        );
        return true;
    };

    var getCmtPreview = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'community/preview.json',
            data,
            options
        );
    };
    /* </community> */

    /* <projects> */

    var subscribeProject = function (params, id, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/' + id + '/follow.json',
            id,
            options
        );
        return true;
    };

    var getFeeds = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'feed/filter.json',
            null,
            options
        );
    };

    var getNewFeedsCount = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'feed/newfeedscount.json',
            null,
            options
        );
    };

    var readFeeds = function (params, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'feed/read.json',
            null,
            options
        );
    };

    var updatePrjSettings = function (data, options) {
        return addRequest(
            null,
            {},
            UPDATE,
            'project/settings.json',
            data,
            options
        );
    }

    var getPrjSettings = function (options) {
        return addRequest(
            null,
            {},
            GET,
            'project/settings.json',
            null,
            options
        );
    }

    var getPrjTags = function (params, options) {
        return addRequest(
            null,
            params,
            'get',
            'project/tag.json',
            null,
            options
        );
    };

    var getPrjSecurityinfo = function (params, options) {
        return addRequest(
            null,
            params,
            'get',
            'project/securityinfo.json',
            null,
            options
        );
    };

    var getPrjTagsByName = function (params, name, data, options) {
        return addRequest(
            null,
            params,
            'get',
            'project/tag/search.json',
            data,
            options
        );
    };

    var getPrjComments = function (params, type, id, options) {
        var fn = null;
        switch (type.toLowerCase()) {
            case 'discussion':
                fn = getPrjDiscussionComments;
                break;
        }
        if (typeof fn === 'function') {
            return returnValue(fn(params, id, options));
        }
        return false;
    };

    var addPrjSubtask = function (params, id, data, options) {
        addRequest(
            customEvents.addSubtask,
            params,
            ADD,
            'project/task/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var copyPrjSubtask = function (params, taskid, id, options) {
        addRequest(
            customEvents.addSubtask,
            params,
            ADD,
            'project/task/' + taskid + '/' + id + '/copy.json',
            null,
            options
        );
        return true;
    };

    var updatePrjSubtask = function (params, parentid, id, data, options) {
        var updateStatus = false;
        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                switch (fld) {
                    case 'status':
                        updateStatus = true;
                        break;
                }
            }
        }

        addRequest(
            customEvents.updateSubtask,
            params,
            UPDATE,
            'project/task/' + parentid + '/' + id + (updateStatus ? '/status' : '') + '.json',
            data,
            options
        );
        return true;
    };

    var removePrjSubtask = function (params, parentid, id, options) {
        addRequest(
            customEvents.removeSubtask,
            params,
            REMOVE,
            'project/task/' + parentid + '/' + id + '.json',
            id,
            options
        );
        return true;
    };

    var addPrjTask = function (params, id, data, options) {
        addRequest(
            customEvents.addPrjTask,
            params,
            ADD,
            'project/' + id + '/task.json',
            data,
            options
        );
        return true;
    };

    var copyPrjTask = function (params, id, data, options) {
        addRequest(
            customEvents.copyPrjTask,
            params,
            ADD,
            'project/task/' + data.copyFrom + '/copy.json',
            data,
            options
        );
        return true;
    };

    var addPrjTaskByMessage = function (params, prjId, messageId, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/' + prjId + '/task/' + messageId + '.json',
            null,
            options
        );
    };

    var updatePrjTask = function (params, id, data, options) {
        var updateStatus = false,
            updateMilestone = false,
            event = customEvents.updatePrjTask;

        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                switch (fld) {
                    case 'status':
                        updateStatus = true;
                        event = customEvents.updatePrjTaskStatus;
                        break;
                    case 'newMilestoneID':
                        updateMilestone = true;
                        data.milestoneid = data.newMilestoneID;
                        break;
                }
            }
        }

        addRequest(
            event,
            params,
            UPDATE,
            'project/task/' + id + (updateStatus ? '/status' : '') + (updateMilestone ? '/milestone' : '') + '.json',
            data,
            options
        );
        return true;
    };

    var updatePrjTasksMilestone = function (data, options) {
        addRequest(
            null,
            null,
            UPDATE,
            'project/task/milestone.json',
            data,
            options
        );
        return true;
    };

    var updatePrjTasksStatus = function(data, options) {
        addRequest(
            customEvents.updatePrjTasksStatus,
            null,
            UPDATE,
            'project/task/status.json',
            data,
            options
        );
        return true;
    }

    var removePrjTask = function (params, id, options) {
        addRequest(
            customEvents.removePrjTask,
            params,
            REMOVE,
            'project/task/' + id + '.json',
            id,
            options
        );
        return true;
    };

    var removePrjTasks = function (data, options) {
        addRequest(
            customEvents.removePrjTasks,
            null,
            REMOVE,
            'project/task.json',
            data,
            options
        );
        return true;
    };

    var getPrjTask = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/task/' + id + '.json',
            null,
            options
        );
    };

    var getPrjTasksById = function (params, ids, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/task.json',
            ids,
            options
        );
    };

    var getPrjTasks = function (params, options) {
        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            var filter = options.filter;
            filter.simple = true;
        }

        return addRequest(
            null,
            params,
            GET,
            'project/task/filter.json',
            null,
            options
        );
    };

    var getPrjTasksSimpleFilter = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/task/filter/simple.json',
            null,
            options
        );
    };

    var getPrjTaskFiles = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/task/' + id + '/files.json',
            null,
            options
        );
    };

    var subscribeToPrjTask = function (params, id, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'project/task/' + id + '/subscribe.json',
            id,
            options
        );
    };

    var notifyPrjTaskResponsible = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/task/' + id + '/notify.json',
            null,
            options
        );
    };

    var addPrjTaskLink = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/task/' + id + '/link.json',
            data,
            options
        );
        return true;
    };

    var removePrjTaskLink = function (params, id, data, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'project/task/' + id + '/link.json',
            data,
            options
        );
        return true;
    };

    var getPrjTeam = function (params, ids, options) {
        var isId = ids && (typeof ids === 'number' || typeof ids === 'string');
        return addRequest(
            null,
            params,
            isId ? GET : ADD,
            'project' + (isId ? '/' + ids : '') + '/team.json',
            isId ? null : { ids: ids },
            options
        );
    };

    var updatePrjTeam = function (params, id, data, options) {
        return addRequest(
            customEvents.updatePrjTeam,
            params,
            UPDATE,
            'project/' + id + '/team.json',
            data,
            options
        );
    };

    var setTeamSecurity = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'project/' + id + '/team/security.json',
            data,
            options
        );
    };

    var getPrjProjectFolder = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/' + id + '/files.json',
            null,
            options
        );
    };

    var addPrjEntityFiles = function (params, id, type, data, options) {
        options = options || {};
        if (typeof options === 'function') {
            options = { success: options };
        }
        if (!options.hasOwnProperty('filter')) {
            options.filter = {};
        }
        if (!options.filter.hasOwnProperty('entityType')) {
            options.filter.entityType = type;
        }

        addRequest(
            null,
            params,
            ADD,
            'project/' + id + '/entityfiles.json',
            isArray(data) ? { files: data } : data,
            options
        );
        return true;
    };

    var removePrjEntityFiles = function (params, id, type, data, options) {
        options = options || {};
        if (typeof options === 'function') {
            options = { success: options };
        }
        if (!options.hasOwnProperty('filter')) {
            options.filter = {};
        }
        if (!options.filter.hasOwnProperty('entityType')) {
            options.filter.entityType = type;
        }

        if (data && typeof data === 'object' && !data.hasOwnProperty('entityType')) {
            data.entityType = type;
        }

        addRequest(
            null,
            params,
            REMOVE,
            'project/' + id + '/entityfiles.json',
            typeof data === 'number' || typeof data === 'string' ? { entityType: type, fileid: data } : data,
            options
        );
        return true;
    };

    var getPrjEntityFiles = function (params, id, type, options) {
        options = options || {};
        if (typeof options === 'function') {
            options = { success: options };
        }
        if (!options.hasOwnProperty('filter')) {
            options.filter = {};
        }
        if (!options.filter.hasOwnProperty('entityType')) {
            options.filter.entityType = type;
        }

        return addRequest(
            null,
            params,
            GET,
            'project/' + id + '/entityfiles.json',
            null,
            options
        );
    };

    var addPrjMilestone = function (params, id, data, options) {
        addRequest(
            customEvents.addPrjMilestone,
            params,
            ADD,
            'project/' + id + '/milestone.json',
            data,
            options
        );
        return true;
    };

    var updatePrjMilestone = function (params, id, data, options) {
        var fldInd = 0,
            updateItem = null,
            event = customEvents.updatePrjMilestone;
        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                fldInd++;
                switch (fld) {
                    case 'status':
                        event = customEvents.updatePrjMilestoneStatus;
                        updateItem = 'status';
                        break;
                }
            }
        }
        if (fldInd > 1) {
            updateItem = null;
        }

        addRequest(
            event,
            params,
            UPDATE,
            'project/milestone/' + id + (updateItem ? '/' + updateItem : '') + '.json',
            data,
            options
        );
        return true;
    };

    var removePrjMilestone = function (id, options) {
        addRequest(
            customEvents.removePrjMilestone,
            null,
            REMOVE,
            'project/milestone/' + id + '.json',
            id,
            options
        );
        return true;
    };

    var removePrjMilestones = function (data, options) {
        addRequest(
            null,
            null,
            REMOVE,
            'project/milestone.json',
            data,
            options
        );
        return true;
    };

    var getPrjMilestone = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/milestone/' + id + '.json',
            null,
            options
        );
    };

    var getPrjMilestones = function (params, id, options) {
        if (arguments.length < 3) {
            options = arguments[1];
            id = null;
        }
        var type = null;

        var _id = id, _type = type;
        switch (id) {
            case 'late':
                _type = id;
                _id = null;
                break;
        }

        if (id instanceof Date) {
            _type = id.getFullYear() + '/' + (id.getMonth() + 1);
            _id = null;
        }

        id = _id;
        type = _type;

        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            var filter = options.filter;
            filter.simple = true;
            for (var fld in filter) {
                switch (fld) {
                    case 'participant':
                    case 'tag':
                    case 'projectId':
                    case 'status':
                    case 'deadlineStart':
                    case 'deadlineStop':
                    case 'sortBy':
                    case 'sortOrder':
                        type = type || 'filter';
                        break;
                }
            }
        }

        return addRequest(
            null,
            params,
            GET,
            'project' + (id ? '/' + id : '') + '/milestone' + (type ? '/' + type : '') + '.json',
            null,
            options
        );
    };

    var addPrjDiscussion = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/' + id + '/message.json',
            data,
            options
        );
        return true;
    };

    var updatePrjDiscussion = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/message/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var updatePrjDiscussionStatus = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/message/' + id + '/status.json',
            data,
            options
        );
        return true;
    };

    var removePrjDiscussion = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'project/message/' + id + '.json',
            id,
            options
        );
        return true;
    };

    var getPrjDiscussion = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/message/' + id + '.json',
            null,
            options
        );
    };

    var getPrjDiscussions = function (params, options) {
        var type = null;
        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            var filter = options.filter;
            filter.simple = true;
            for (var fld in filter) {
                switch (fld) {
                    case 'participant':
                    case 'tag':
                    case 'projectId':
                    case 'sortBy':
                    case 'sortOrder':
                        type = type || 'filter';
                        break;
                }
            }
        }

        return addRequest(
            null,
            params,
            GET,
            'project/message' + (type ? '/' + type : '') + '.json',
            null,
            options
        );
    };

    var subscribeToPrjDiscussion = function (params, id, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'project/message/' + id + '/subscribe.json',
            id,
            options
        );
    };

    var getSubscribesToPrjDiscussion = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/message/' + id + '/subscribes.json',
            id,
            options
        );
    };

    var addPrjProject = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/withSecurity.json',
            data,
            options
        );
        return true;
    };

    var updatePrjProject = function (params, id, data, options) {
        var fldInd = 0,
            updateItem = null;
        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                fldInd++;
                switch (fld) {
                    case 'tags':
                        updateItem = 'tag';
                        break;
                    case 'status':
                        updateItem = 'status';
                        break;
                }
            }
        }
        if (fldInd > 1) {
            updateItem = "withSecurityInfo";
        }

        addRequest(
            null,
            params,
            UPDATE,
            'project/' + id + (updateItem ? '/' + updateItem : '') + '.json',
            data,
            options
        );
        return true;
    };

    var updatePrjProjectStatus = function (params, id, data, options) {
        addRequest(
            customEvents.updatePrjProjectStatus,
            params,
            UPDATE,
            'project/' + id + '/status.json',
            data,
            options
        );
        return true;
    };

    var removePrjProject = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'project/' + id + '.json',
            id,
            options
        );
        return true;
    };

    var followingPrjProject = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'project/' + id + '/follow.json',
            data,
            options
        );
    };

    var getPrjProject = function (params, id, options) {
        return addRequest(
            customEvents.getPrjProject,
            params,
            GET,
            'project/' + id + '.json',
            null,
            options
        );
    };

    var getPrjProjects = function (params, type, options) {
        if (arguments.length < 4) {
            options = type;
            type = null;
        }

        var filter = null;
        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            filter = options.filter;
            filter.simple = true;
            for (var fld in filter) {
                switch (fld) {
                    case 'tag':
                    case 'follow':
                    case 'status':
                    case 'participant':
                    case 'sortBy':
                    case 'sortOrder':
                        type = type || 'filter';
                        break;
                }
            }
        }

        var query = getQuery(options);

        return addRequest(
            null,
            params,
            GET,
            'project' + (type ? '/' + type : '') + (query ? '/@search/' + query : '') + '.json',
            null,
            options
        );
    };

    var getPrjSelfProjects = function (params, options) {
        return getPrjProjects(params, '@self', options);
    };

    var getPrjFollowProjects = function (params, options) {
        return getPrjProjects(params, '@follow', options);
    };

    var getProjectsForCrmContact = function (params, contactid, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/contact/' + contactid + '.json',
            null,
            options
        );
    };

    var addProjectForCrmContact = function (params, projectid, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/' + projectid + '/contact.json',
            data,
            options
        );
    };

    var removeProjectFromCrmContact = function (params, projectid, data, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'project/' + projectid + '/contact.json',
            data,
            options
        );
    };

    var addPrjTaskComment = function (params, id, data, options) {
        if (!data.parentid) {
            data.parentid = '00000000-0000-0000-0000-000000000000';
        }

        addRequest(
            null,
            params,
            ADD,
            'project/task/' + id + '/comment.json',
            data,
            options
        );
        return true;
    };

    var getPrjTaskComments = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/task/' + id + '/comment.json',
            null,
            options
        );
    };

    var addPrjDiscussionComment = function (params, id, data, options) {
        if (!data.parentid) {
            data.parentid = '00000000-0000-0000-0000-000000000000';
        }

        addRequest(
            null,
            params,
            ADD,
            'project/message/' + id + '/comment.json',
            data,
            options
        );
        return true;
    };

    var getPrjDiscussionComments = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/message/' + id + '/comment.json',
            null,
            options
        );
    };

    var getPrjDiscussionPreview = function (params, htmltext, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/message/discussion/preview.json',
            { htmltext: htmltext },
            options
        );
    };

    var getPrjCommentPreview = function (params, commentid, htmltext, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/comment/preview.json',
            { commentid: commentid, htmltext: htmltext },
            options
        );
    };
    var getWikiCommentPreview = function (params, commentid, htmltext, options) {
        return addRequest(
            null,
            params,
            ADD,
            'community/wiki/comment/preview.json',
            { commentid: commentid, htmltext: htmltext },
            options
        );
    };

    var getBlogCommentPreview = function (params, commentid, htmltext, options) {
        return addRequest(
            null,
            params,
            ADD,
            'community/blog/comment/preview.json',
            { commentid: commentid, htmltext: htmltext },
            options
        );
    };
    var getNewsCommentPreview = function (params, commentid, htmltext, options) {
        return addRequest(
            null,
            params,
            ADD,
            'community/event/comment/preview.json',
            { commentid: commentid, htmltext: htmltext },
            options
        );
    };
    var getBookmarksCommentPreview = function (params, commentid, htmltext, options) {
        return addRequest(
            null,
            params,
            ADD,
            'community/bookmark/comment/preview.json',
            { commentid: commentid, htmltext: htmltext },
            options
        );
    };


    var removePrjComment = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'project/comment/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var removeWikiComment = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'community/wiki/comment/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var removeBlogComment = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'community/blog/comment/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var removeNewsComment = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'community/event/comment/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var removeBookmarksComment = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'community/bookmark/comment/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var addPrjComment = function (params, data, options) {
        addRequest(
            customEvents.addPrjComment,
            params,
            ADD,
            'project/comment.json',
            data,
            options
        );
        return true;
    };

    var addWikiComment = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/wiki/comment.json',
            data,
            options
        );
        return true;
    };

    var addBlogComment = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/blog/comment.json',
            data,
            options
        );
        return true;
    };

    var addNewsComment = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/event/comment.json',
            data,
            options
        );
        return true;
    };

    var addBookmarksComment = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'community/bookmark/comment.json',
            data,
            options
        );
        return true;
    };


    var updatePrjComment = function (params, commentid, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/comment/' + commentid + '.json',
            data,
            options
        );
        return true;
    };

    var updateWikiComment = function (params, commentid, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'community/wiki/comment/' + commentid + '.json',
            data,
            options
        );
        return true;
    };

    var updateBlogComment = function (params, commentid, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'community/blog/comment/' + commentid + '.json',
            data,
            options
        );
        return true;
    };

    var updateNewsComment = function (params, commentid, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'community/event/comment/' + commentid + '.json',
            data,
            options
        );
        return true;
    };

    var updateBookmarksComment = function (params, commentid, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'community/bookmark/comment/' + commentid + '.json',
            data,
            options
        );
        return true;
    };

    var fckeRemoveCommentComplete = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'portal/fcke/comment/removecomplete.json',
            data,
            options
        );
        return true;
    };

    var fckeCancelCommentComplete = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'portal/fcke/comment/cancelcomplete.json',
            data,
            options
        );
        return true;
    };

    var fckeEditCommentComplete = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'portal/fcke/comment/editcomplete.json',
            data,
            options
        );
        return true;
    };

    var getShortenLink = function (params, link, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'portal/getshortenlink.json',
            { link: link },
            options
        );
    };

    var updatePortalName = function (params, alias, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'portal/portalrename.json',
            { alias: alias },
            options
        );
        return true;
    };


    var addPrjProjectTeamPerson = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/' + id + '/team.json',
            data,
            options
        );
        return true;
    };

    var removePrjProjectTeamPerson = function (params, id, data, options) {
        addRequest(
            customEvents.removePrjTeam,
            params,
            REMOVE,
            'project/' + id + '/team.json',
            data,
            options
        );
        return true;
    };

    var getPrjProjectTeamPersons = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/' + id + '/team.json',
            null,
            options
        );
    };

    var getPrjProjectFiles = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/' + id + '/entityfiles.json',
            null,
            options
        );
    };

    // tasks index for gantt
    var getPrjGanttIndex = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/' + id + '/order.json',
            null,
            options
        );
    };

    var setPrjGanttIndex = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/' + id + '/order.json',
            data,
            options
        );
        return true;
    };

    // time-traking

    var addPrjTime = function (params, taskid, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/task/' + taskid + '/time.json',
            data,
            options
        );
        return true;
    };

    var getPrjTime = function (params, options) {
        return getPrjTaskTime(params, null, options);
    };

    var getPrjTimeById = function (params, projectId, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/' + projectId + '/time/total.json',
            null,
            options
        );
    };

    var getPrjTaskTime = function (params, taskid, options) {

        if (!taskid || !options) {
            var filter = null, _taskid = null, _options = null;
            for (var i = 2, n = arguments.length; i < n; i++) {
                _options = _options || (typeof arguments[i] === 'function' || typeof arguments[i] === 'object' ? arguments[i] : _options);
                _taskid = _taskid || (isFinite(+arguments[i]) ? +arguments[i] : _taskid);
            }

            options = _options;
            taskid = _taskid;
        }
        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            filter = options.filter;
        }

        return addRequest(
            null,
            params,
            GET,
            'project' + (taskid ? '/task/' + taskid : '') + '/time' + (filter ? '/filter' : '') + '.json',
            null,
            options
        );
    };
    
    var getTotalTimeByFilter = function (params, options) {
        var data = null;
        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            data = options.filter;
        }
        return addRequest(
            null,
            params,
            GET,
            'project/time/filter/total.json',
            null,
            options
        );
    };

    var updatePrjTime = function (params, id, data, options) {
        addRequest(
            customEvents.updatePrjTime,
            params,
            UPDATE,
            'project/time/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var changePaymentStatus = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/time/times/status.json',
            data,
            options
        );
        return true;
    };

    var removePrjTime = function (params, data, options) {
        addRequest(
            customEvents.removePrjTime,
            params,
            REMOVE,
            'project/time/times/remove.json',
            data,
            options
        );
        return true;
    };

    // project templates

    var getPrjTemplates = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'project/template.json',
            null,
            options
        );
        return true;
    };

    var getPrjTemplate = function (params, id, options) {
        addRequest(
            null,
            params,
            GET,
            'project/template/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var updatePrjTemplate = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'project/template/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var createPrjTemplate = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'project/template.json',
            data,
            options
        );
        return true;
    };

    var removePrjTemplate = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'project/template/' + id + '.json',
            null,
            options
        );
        return true;
    };

    //activities
    var getPrjActivities = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/activities/filter.json',
            null,
            options
        );
    };

    //import
    var checkPrjImportQuota = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/import/quota.json',
            data,
            options
        );
    };
    var addPrjImport = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/import.json',
            data,
            options
        );
    };
    var getPrjImport = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/import.json',
            null,
            options
        );
    };
    var getPrjImportProjects = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/import/projects.json',
            data,
            options
        );
    };
    //reports
    var getPrjReportTemplate = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'project/report/' + id + '.json',
            null,
            options
        );
    };
    var addPrjReportTemplate = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'project/report.json',
            data,
            options
        );
    };
    var updatePrjReportTemplate = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'project/report/' + id + '.json',
            data,
            options
        );
    };
    var deletePrjReportTemplate = function (params, id, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'project/report/' + id + '.json',
            null,
            options
        );
    };
    // upload files
    var uploadFilesToPrjEntity = function (params, entityId, data, options) {
        return helper.uploader(
            null,
            params,
            'file',
            'project/' + entityId + '/entityfiles/upload.tml',
            data,
            options
        );
    };

    /* </projects> */

    /* <documents> */
    var createDocUploadFile = function (params, id, data, options) {
        return helper.uploader(
            null,
            params,
            'file',
            'files/' + id + '/upload.tml',
            data,
            options
        );
    };

    var addDocFile = function (params, id, type, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'files/' + id + (type ? '/' + type : '') + '.json',
            data,
            options
        );
        return true;
    };

    var getDocFile = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'files/file/' + id + '.json',
            null,
            options
        );
    };

    var addDocFolder = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'files/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var getDocFolder = function (params, id, options) {
        return addRequest(
            customEvents.getDocFolder,
            params,
            GET,
            'files/' + id + '.json',
            null,
            options
        );
    };

    var removeDocFile = function (params, id, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'files/file/' + id + '.json',
            null,
            options
        );
    };

    var createDocUploadSession = function (params, id, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'files/' + id + '/upload/create_session.json',
            data,
            options
        );
    };

    var getFolderPath = function (id, options) {
        return addRequest(
            null,
            null,
            GET,
            'files/folder/' + id + '/path.json',
            null,
            options
        );
    };

    var getFileSecurityInfo = function (id, options) {
        return addRequest(
            null,
            null,
            GET,
            'files/file/' + id + '/share.json',
            null,
            options
        );
    };

    var generateSharedLink = function (id, data, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'files/' + id + '/sharedlink.json',
            data,
            options
        );
    };

    var copyBatchItems = function (data, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'files/fileops/copy.json',
            data,
            options
        );
    };

    var getOperationStatuses = function (options) {
        return addRequest(
            null,
            null,
            GET,
            'files/fileops.json',
            null,
            options
        );
    };

    var getPresignedUri = function (id, options) {
        return addRequest(
            null,
            null,
            GET,
            'files/file/' + id + '/presigned.json',
            null,
            options
        );
    };

    var saveDocServiceUrl = function (docServiceUrl, docServiceUrlInternal, docServiceUrlPortal, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'files/docservice.json',
            {
                docServiceUrl: docServiceUrl,
                docServiceUrlInternal: docServiceUrlInternal,
                docServiceUrlPortal: docServiceUrlPortal,
            },
            options
        );
    };
    /* </documents> */

    /* <crm> */
    var createCrmUploadFile = function (params, type, id, data, options) {
        return helper.uploader(
            null,
            params,
            'file',
            'crm/' + type + '/' + id + '/files/upload.tml',
            data,
            options
        );
    };

    var getCrmContactInfo = function (params, contactid, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/contact/' + contactid + '/data.json',
            null,
            options
        );
        return true;
    };

    var addCrmContactInfo = function (params, contactid, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/' + contactid + '/data.json',
            data,
            options
        );
        return true;
    };

    var updateCrmContactInfo = function (params, contactid, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/' + contactid + '/data/' + data.id + '.json',
            data,
            options
        );
        return true;
    };

    var deleteCrmContactInfo = function (params, contactid, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/contact/' + contactid + '/data/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var addCrmContactData = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/' + id + '/batch.json',
            isArray(data) ? { data: data } : data,
            options
        );
        return true;
    };

    var updateCrmContactData = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/' + id + '/batch.json',
            isArray(data) ? { data: data } : data,
            options
        );
        return true;
    };

    var addCrmContactTwitter = function (params, contactid, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/' + contactid + '/data.json',
            data,
            options
        );
        return true;
    };

    var addCrmEntityNote = function (params, type, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + type + '/' + id + '/files/text.json',
            data,
            options
        );
        return true;
    };

    var addCrmContact = function (params, isCompany, data, options) {
        if (isCompany === true) {
            return addCrmCompany(params, data, options);
        } else {
            return addCrmPerson(params, data, options);
        }
    };

    var addCrmCompany = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/company' + (isArray(data) ? '/quick' : '') + '.json',
            isArray(data) ? { data: data } : data,
            options
        );
        return true;
    };

    var updateCrmCompany = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/company/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var updateCrmCompanyContactStatus = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/company/' + id + '/status.json',
            data,
            options
        );
        return true;
    };

    var updateCrmPersonContactStatus = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/person/' + id + '/status.json',
            data,
            options
        );
        return true;
    };

    var updateCrmContactContactStatus = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/' + id + '/status.json',
            data,
            options
        );
        return true;
    };

    var addCrmPerson = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/person' + (isArray(data) ? '/quick' : '') + '.json',
            isArray(data) ? { data: data } : data,
            options
        );
        return true;
    };

    var updateCrmPerson = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/person/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmContact = function (params, ids, options) {
        var isNumberOrString = ids && (typeof ids === 'number' || typeof ids === 'string');
        var isObject = ids && typeof ids === 'object';
        addRequest(
            null,
            params,
            isNumberOrString ? REMOVE : UPDATE,
            'crm/contact' + (isNumberOrString ? '/' + ids : '') + '.json',
            isObject ? { contactids: ids } : null,
            options
        );
        return true;
    };

    var mergeCrmContacts = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/merge.json',
            data,
            options
        );
        return true;
    };

    var getCrmContactsForProject = function (params, id, options) {
        return addRequest(
            customEvents.getCrmContactsForProject,
            params,
            GET,
            'crm/contact/project/' + id + '.json',
            null,
            options
        );
    };

    var addCrmTag = function (params, type, ids, tagname, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + type + (typeof ids === 'object' ? '/taglist' : '/' + ids + '/tag') + '.json',
            { entityid: ids, tagName: tagname },
            options
        );
        return true;
    };

    var addCrmContactTagToGroup = function (params, type, id, tagname, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + type + '/' + id + '/tag/group.json',
            { entityid: id, entityType: type, tagName: tagname },
            options
        );
        return true;
    };

    var deleteCrmContactTagFromGroup = function (params, type, id, tagname, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/' + type + '/' + id + '/tag/group.json',
            { entityid: id, entityType: type, tagName: tagname },
            options
        );
        return true;
    };

    var removeCrmTag = function (params, type, id, tagname, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/' + type + '/' + id + '/tag.json',
            { tagName: tagname },
            options
        );
        return true;
    };

    var getCrmTags = function (params, type, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/' + type + '/' + id + '/tag.json',
            null,
            options
        );
    };

    var getCrmEntityTags = function (params, type, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/' + type + '/tag.json',
            null,
            options
        );
        return true;
    };

    var addCrmEntityTag = function (params, type, tagname, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + type + '/tag.json',
            { entityType: type, tagName: tagname },
            options
        );
        return true;
    };

    var removeCrmEntityTag = function (params, type, tagname, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/' + type + '/tag.json',
            { tagName: tagname },
            options
        );
        return true;
    };

    var removeCrmUnusedTag = function (params, type, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/' + type + '/tag/unused.json',
            null,
            options
        );
        return true;
    };

    var getCrmCustomFields = function (params, type, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/' + type + '/customfield/definitions.json',
            null,
            options
        );
        return true;
    };

    var addCrmCustomField = function (params, type, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + type + '/customfield.json',
            data,
            options
        );
        return true;
    };

    var updateCrmCustomField = function (params, type, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/' + type + '/customfield/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmCustomField = function (params, type, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/' + type + '/customfield/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var reorderCrmCustomFields = function (params, type, ids, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/' + type + '/customfield/reorder.json',
            { fieldids: ids, entityType: type },
            options
        );
        return true;
    };

    var getCrmDealMilestones = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/opportunity/stage.json',
            null,
            options
        );
        return true;
    };

    var addCrmDealMilestone = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/opportunity/stage.json',
            data,
            options
        );
        return true;
    };

    var updateCrmDealMilestone = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/opportunity/stage/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var updateCrmDealMilestoneColor = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/opportunity/stage/' + id + '/color.json',
            data,
            options
        );
        return true;
    };

    var removeCrmDealMilestone = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/opportunity/stage/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var reorderCrmDealMilestones = function (params, ids, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/opportunity/stage/reorder.json',
            { ids: ids },
            options
        );
        return true;
    };

    var addCrmContactStatus = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/status.json',
            data,
            options
        );
        return true;
    };

    var updateCrmContactStatus = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/status/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var updateCrmContactStatusColor = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/status/' + id + '/color.json',
            data,
            options
        );
        return true;
    };

    var removeCrmContactStatus = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/contact/status/' + id + '.json',
            null,
            options
        );
        return true;
    };


    var addCrmContactType = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/type.json',
            data,
            options
        );
        return true;
    };

    var updateCrmContactType = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/type/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmContactType = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/contact/type/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmListItem = function (params, type, options) {
        var path = "";
        switch (type) {
            case 1:
                //ContactStatus
                path = 'crm/contact/status.json';
                break;
            case 2:
                //TaskCategory
                path = 'crm/task/category.json';
                break;
            case 3:
                //HistoryCategory
                path = 'crm/history/category.json';
                break;
            case 4:
                //ContactType
                path = 'crm/contact/type.json';
                break;
            default:
                return false;
        }

        addRequest(
            null,
            params,
            GET,
            path,
            null,
            options
        );
        return true;
    };

    var addCrmListItem = function (params, type, data, options) {
        var path = "";
        switch (type) {
            case 1:
                //ContactStatus
                path = 'crm/contact/status.json';
                break;
            case 2:
                //TaskCategory
                path = 'crm/task/category.json';
                break;
            case 3:
                //HistoryCategory
                path = 'crm/history/category.json';
                break;
            case 4:
                //ContactType
                path = 'crm/contact/type.json';
                break;
            default:
                return false;
        }

        addRequest(
            null,
            params,
            ADD,
            path,
            data,
            options
        );
        return true;
    };

    var updateCrmListItem = function (params, type, id, data, options) {
        var path = "";
        switch (type) {
            case 1:
                //ContactStatus
                path = 'crm/contact/status/' + id + '.json';
                break;
            case 2:
                //TaskCategory
                path = 'crm/task/category/' + id + '.json';
                break;
            case 3:
                //HistoryCategory
                path = 'crm/history/category/' + id + '.json';
                break;
            case 4:
                //ContactType
                path = 'crm/contact/type/' + id + '.json';
                break;
            default:
                return false;
        }

        addRequest(
            null,
            params,
            UPDATE,
            path,
            data,
            options
        );
        return true;
    };

    var updateCrmListItemIcon = function (params, type, id, data, options) {
        var path = "";
        switch (type) {
            case 2:
                //TaskCategory
                path = 'crm/task/category/' + id + '/icon.json';
                break;
            case 3:
                //HistoryCategory
                path = 'crm/history/category/' + id + '/icon.json';
                break;
            default:
                return false;
        }

        addRequest(
            null,
            params,
            UPDATE,
            path,
            data,
            options
        );
        return true;
    };

    var removeCrmListItem = function (params, type, id, toid, options) {
        var path = "",
            data = {
                newcategoryid: toid
            };
        switch (type) {
            case 1:
                //ContactStatus
                path = 'crm/contact/status/' + id + '.json';
                break;
            case 2:
                //TaskCategory
                path = 'crm/task/category/' + id + '.json';
                break;
            case 3:
                //HistoryCategory
                path = 'crm/history/category/' + id + '.json';
                break;
            case 4:
                //ContactType
                path = 'crm/contact/type/' + id + '.json';
                break;
            default:
                return false;
        }

        addRequest(
            null,
            params,
            REMOVE,
            path,
            data,
            options
        );
        return true;
    };

    var reorderCrmListItems = function (params, type, titles, options) {
        var path = "";
        switch (type) {
            case 1:
                //ContactStatus
                path = 'crm/contact/status/reorder.json';
                break;
            case 2:
                //TaskCategory
                path = 'crm/task/category/reorder.json';
                break;
            case 3:
                //HistoryCategory
                path = 'crm/history/category/reorder.json';
                break;
            case 4:
                //ContactType
                path = 'crm/contact/type/reorder.json';
                break;
            default:
                return false;
        }

        addRequest(
            null,
            params,
            UPDATE,
            path,
            { titles: titles },
            options
        );
        return true;
    };

    var addCrmTask = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/task.json',
            data,
            options
        );
        return true;
    };

    var addCrmTaskGroup = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/task/group.json',
            data,
            options
        );
        return true;
    };

    var getCrmTask = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/task/' + id + '.json',
            null,
            options
        );
    };

    var updateCrmTask = function (params, id, data, options) {
        var isUpdateStatusAction = data.hasOwnProperty('isClosed');

        if (isUpdateStatusAction) {
            addRequest(
                null,
                params,
                UPDATE,
                !!data.isClosed ? 'crm/task/' + id + '/close.json' : 'crm/task/' + id + '/reopen.json',
                data,
                options
            );
        } else {
            addRequest(
                null,
                params,
                UPDATE,
                'crm/task/' + id + '.json',
                data,
                options
            );
        }

        return true;
    };

    var removeCrmTask = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/task/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var addCrmContactForProject = function (params, type, entityid, id, data, options) {
        addRequest(
            customEvents.addCrmContactForProject,
            params,
            ADD,
            'crm/contact/' + id + '/project/' + entityid + '.json',
            data,
            options
        );
    };

    var addCrmContactsForProject = function (params, projectid, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/project/' + projectid + '.json',
            data,
            options
        );
    };

    var removeCrmContactFromProject = function (params, type, entityid, id, options) {
        addRequest(
            customEvents.removeCrmContactFromProject,
            params,
            REMOVE,
            'crm/contact/' + id + '/project/' + entityid + '.json',
            null,
            options
        );
    };

    var addCrmDealForContact = function (params, contactid, opportunityid, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/' + contactid + '/opportunity/' + opportunityid + '.json',
            {
                contactid: contactid,
                opportunityid: opportunityid
            },
            options
        );
    };

    var removeCrmDealFromContact = function (params, contactid, opportunityid, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/contact/' + contactid + '/opportunity/' + opportunityid + '.json',
            {
                contactid: contactid,
                opportunityid: opportunityid
            },
            options
        );
    };

    var addCrmContactMember = function (params, type, entityid, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + type + '/' + entityid + '/contact' + (type === 'opportunity' ? '/' + id : '') + '.json',
            data,
            options
        );
    };

    var removeCrmContactMember = function (params, type, entityid, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/' + type + '/' + entityid + '/contact/' + id + '.json',
            null,
            options
        );
    };

    var getCrmContactMembers = function (params, type, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/' + type + '/' + id + '/contact.json',
            null,
            options
        );
    };

    var addCrmPersonMember = function (params, type, entityid, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/' + type + '/' + entityid + '/person.json',
            data,
            options
        );
    };

    var removeCrmPersonMember = function (params, type, entityid, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/contact/' + type + '/' + entityid + '/person.json',
            { personid: id },
            options
        );
    };

    var getCrmPersonMembers = function (params, type, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/' + type + '/' + id + '/person.json',
            null,
            options
        );
    };

    var getCrmEntityMembers = function (params, type, id, options) {
        var fn = null;
        switch (type) {
            case 'company':
                fn = getCrmPersonMembers;
                break;
            default:
                fn = getCrmContactMembers;
                break;
        }
        if (fn) {
            return returnValue(fn(params, type, id, options));
        }
        return false;
    };

    var addCrmEntityMember = function (params, type, entityid, id, data, options) {
        var fn = null;
        switch (type) {
            case 'company':
                fn = addCrmPersonMember;
                break;
            case 'project':
                fn = addCrmContactForProject;
                break;
            default:
                fn = addCrmContactMember;
                break;
        }
        if (fn) {
            return returnValue(fn(params, type, entityid, id, data, options));
        }
        return false;
    };

    var removeCrmEntityMember = function (params, type, entityid, id, options) {
        var fn = null;
        switch (type) {
            case 'company':
                fn = removeCrmPersonMember;
                break;
            case 'project':
                fn = removeCrmContactFromProject;
                break;
            default:
                fn = removeCrmContactMember;
                break;
        }
        if (fn) {
            return returnValue(fn(params, type, entityid, id, options));
        }
        return false;
    };

    var getCrmCases = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/case/filter.json',
            null,
            options
        );
    };

    var getCrmCasesByPrefix = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/case/byprefix.json',
            null,
            options
        );
    };

    var removeCrmCase = function (params, ids, options) {
        var isNumberOrString = ids && (typeof ids === 'number' || typeof ids === 'string'),
            isObject = ids && typeof ids === 'object';
        addRequest(
            null,
            params,
            isNumberOrString ? REMOVE : UPDATE,
            'crm/case' + (isNumberOrString ? '/' + ids : '') + '.json',
            isObject ? { casesids: ids } : null,
            options
        );
        return true;
    };

    var updateCrmCase = function (params, id, data, options) {
        var isUpdateStatusAction = data.hasOwnProperty('isClosed');

        if (isUpdateStatusAction) {
            addRequest(
                null,
                params,
                UPDATE,
                !!data.isClosed ? 'crm/case/' + id + '/close.json' : 'crm/case/' + id + '/reopen.json',
                data,
                options
            );
        } else {
            addRequest(
                null,
                params,
                UPDATE,
                'crm/case/' + id + '.json',
                data,
                options
            );
        }
        return true;
    };

    var getCrmContacts = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/filter.json',
            null,
            options
        );
    };

    var getCrmSimpleContacts = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/simple/filter.json',
            null,
            options
        );
    };

    var getCrmContactsForMail = function (params, data, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/mail.json',
            typeof data === 'number' || typeof data === 'string' ? { contactids: [data] } : data,
            options
        );
    };

    var getCrmContactsByPrefix = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/byprefix.json',
            null,
            options
        );
    };

    var getCrmContact = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/' + id + '.json',
            null,
            options
        );
    };

    var getCrmTasks = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/task/filter.json',
            null,
            options
        );
    };

    var getCrmOpportunity = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/opportunity/' + id + '.json',
            null,
            options
        );
    };

    var getCrmCase = function (params, id, options) {
        return addRequest(
          null,
          params,
          GET,
          'crm/case/' + id + '.json',
          null,
          options
        );
    };

    var getContactsByContactInfo = function (params, data, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/bycontactinfo.json',
            data,
            options
        );
    };

    var getCrmOpportunities = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/opportunity/filter.json',
            null,
            options
        );
    };

    var getCrmOpportunitiesByContact = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/opportunity/bycontact/' + id + '.json',
            null,
            options
        );
    };

    var getCrmOpportunitiesByPrefix = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/opportunity/byprefix.json',
            null,
            options
        );
    };

    var removeCrmOpportunity = function (params, ids, options) {
        var isNumberOrString = ids && (typeof ids === 'number' || typeof ids === 'string'),
            isObject = ids && typeof ids === 'object';
        addRequest(
            null,
            params,
            isNumberOrString ? REMOVE : UPDATE,
            'crm/opportunity' + (isNumberOrString ? '/' + ids : '') + '.json',
            isObject ? { opportunityids: ids } : null,
            options
        );
        return true;
    };

    var updateCrmOpportunityMilestone = function (params, opportunityid, stageid, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/opportunity/' + opportunityid + '/stage/' + stageid + '.json',
            { opportunityid: opportunityid, stageid: stageid },
            options
        );
    };

    var getCrmCurrencyConvertion = function (params, data, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/settings/currency/convert.json',
            data,
            options
        );
    };

    var getCrmCurrencySummaryTable = function (params, currency, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/settings/currency/summarytable.json',
            { currency: currency },
            options
        );
    };

    var updateCrmCurrency = function (params, currency, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/settings/currency.json',
            { currency: currency },
            options
        );
    };

    var setCrmCurrencyRates = function (params, currency, rates, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/currency/setrates.json',
            { currency: currency, rates: rates },
            options
        );
    };

    var addCrmCurrencyRates = function (params, rates, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/currency/addrates.json',
            { rates: rates },
            options
        );
    };

    var updateCRMContactStatusSettings = function (params, changeContactStatusGroupAuto, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/status/settings.json',
            { changeContactStatusGroupAuto: changeContactStatusGroupAuto },
            options
        );
    };

    var updateCRMContactTagSettings = function (params, addTagToContactGroupAuto, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/tag/settings.json',
            { addTagToContactGroupAuto: addTagToContactGroupAuto },
            options
        );
    };

    var updateCRMContactMailToHistorySettings = function (params, writeMailToHistoryAuto, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/mailtohistory/settings.json',
            { writeMailToHistoryAuto: writeMailToHistoryAuto },
            options
        );
    };

    var updateOrganisationSettingsCompanyName = function (params, companyName, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/settings/organisation/base.json',
            { companyName: companyName },
            options
        );
    };

    var updateOrganisationSettingsAddresses = function (params, addresses, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/settings/organisation/address.json',
            { companyAddress: addresses },
            options
        );
    };

    var updateOrganisationSettingsLogo = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/settings/organisation/logo.json',
            data,
            options
        );
    };

    var getOrganisationSettingsLogo = function (params, logoid, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/settings/organisation/logo.json',
            { id: logoid },
            options
        );
    };

    var updateWebToLeadFormKey = function (params, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'crm/settings/webformkey/change.json',
            null,
            options
        );
    };

    var updateCRMSMTPSettings = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/settings/smtp.json',
            data,
            options
        );
    };

    var sendSMTPTestMail = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/settings/testmail.json',
            data,
            options
        );
    };

    var sendSMTPMailToContacts = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/mailsmtp/send.json',
            data,
            options
        );
    };

    var getPreviewSMTPMailToContacts = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/contact/mailsmtp/preview.json',
            data,
            options
        );
    };

    var getStatusSMTPMailToContacts = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/contact/mailsmtp/status.json',
            null,
            options
        );
    };

    var cancelSMTPMailToContacts = function (params, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/mailsmtp/cancel.json',
            null,
            options
        );
    };

    var addCrmHistoryEvent = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/history.json',
            data,
            options
        );
    };

    var removeCrmHistoryEvent = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/history/' + id + '.json',
            null,
            options
        );
    };

    var getCrmHistoryEvents = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/history/filter.json',
            null,
            options
        );
    };

    var removeCrmFile = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/files/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmFolder = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/files/' + id + '.json',
            null,
            options
        );
    };

    var updateCrmContactRights = function (params, id, data, options) {
        if (!data || !options) {
            options = data;
            data = id;
            id = null;
        }

        addRequest(
            null,
            params,
            UPDATE,
            'crm/contact' + (id ? '/' + id : '') + '/access.json',
            data,
            options
        );
        return true;
    };

    var updateCrmCaseRights = function (params, id, data, options) {
        if (!data || !options) {
            options = data;
            data = id;
            id = null;
        }

        addRequest(
            null,
            params,
            UPDATE,
            'crm/case' + (id ? '/' + id : '') + '/access.json',
            data,
            options
        );
        return true;
    };

    var updateCrmOpportunityRights = function (params, id, data, options) {
        if (!data || !options) {
            options = data;
            data = id;
            id = null;
        }

        addRequest(
            null,
            params,
            UPDATE,
            'crm/opportunity' + (id ? '/' + id : '') + '/access.json',
            data,
            options
        );
        return true;
    };

    var addCrmEntityFiles = function (params, id, type, data, options) {
        if (data && typeof data === 'object' && !data.hasOwnProperty('entityType')) {
            data.entityType = type;
        }

        addRequest(
            null,
            params,
            ADD,
            'crm' + (type ? '/' + type : '') + '/' + id + '/files.json',
            isArray(data) ? { entityType: type, entityid: id, fileids: data } : data,
            options
        );
        return true;
    };

    var removeCrmEntityFiles = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/files/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmEntityFiles = function (params, id, type, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm' + (type ? '/' + type : '') + '/' + id + '/files.json',
            null,
            options
        );
    };

    var getCrmTaskCategories = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/task/category.json',
            null,
            options
        );
    };

    var addCrmEntityTaskTemplateContainer = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + data.entityType + '/tasktemplatecontainer.json',
            data,
            options
        );
        return true;
    };

    var updateCrmEntityTaskTemplateContainer = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/tasktemplatecontainer/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmEntityTaskTemplateContainer = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/tasktemplatecontainer/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmEntityTaskTemplateContainer = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/tasktemplatecontainer/' + id + '.json',
            null,
            options
        );
    };

    var getCrmEntityTaskTemplateContainers = function (params, type, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/' + type + '/tasktemplatecontainer.json',
            null,
            options
        );
    };

    var addCrmEntityTaskTemplate = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/tasktemplatecontainer/' + data.containerid + '/tasktemplate.json',
            data,
            options
        );
        return true;
    };

    var updateCrmEntityTaskTemplate = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/tasktemplatecontainer/' + data.containerid + '/tasktemplate.json',
            data,
            options
        );
        return true;
    };

    var removeCrmEntityTaskTemplate = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/tasktemplatecontainer/tasktemplate/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmEntityTaskTemplate = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/tasktemplatecontainer/tasktemplate/' + id + '.json',
            null,
            options
        );
    };

    var getCrmEntityTaskTemplates = function (params, containerid, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/tasktemplatecontainer/' + containerid + '/tasktemplate.json',
            null,
            options
        );
    };

    var getCrmInvoices = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/filter.json',
            null,
            options
        );
    };

    var getCrmEntityInvoices = function (params, type, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/' + type + '/invoicelist/' + id + '.json',
            null,
            options
        );
    };

    var updateCrmInvoicesStatusBatch = function (params, status, ids, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'crm/invoice/status/' + status + '.json',
            { invoiceids: ids },
            options
        );
    };

    var getCrmInvoiceByNumber = function (params, number, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/bynumber.json',
            { number: number },
            options
        );
    };

    var getCrmInvoiceByNumberExistence = function (params, number, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/bynumber/exist.json',
            { number: number },
            options
        );
    };

    var getCrmInvoiceItems = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoiceitem/filter.json',
            null,
            options
        );
    };

    var addCrmInvoiceItem = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/invoiceitem.json',
            data,
            options
        );
        return true;
    };

    var updateCrmInvoiceItem = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/invoiceitem/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmInvoiceItem = function (params, ids, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/invoiceitem' + (ids && (typeof ids === 'number' || typeof ids === 'string') ? '/' + ids : '') + '.json',
            ids && typeof ids === 'object' ? { ids: ids } : null,
            options
        );
        return true;
    };

    var getCrmInvoiceTaxes = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/tax.json',
            null,
            options
        );
    };

    var addCrmInvoiceTax = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/invoice/tax.json',
            data,
            options
        );
        return true;
    };

    var updateCrmInvoiceTax = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/invoice/tax/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmInvoiceTax = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/invoice/tax/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmInvoice = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/' + id + '.json',
            null,
            options
        );
    };

    var getCrmInvoiceSample = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/sample.json',
            null,
            options
        );
    };

    var getCrmInvoiceJsonData = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/jsondata/' + id + '.json',
            null,
            options
        );
    };

    var addCrmInvoice = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/invoice.json',
            data,
            options
        );
        return true;
    };

    var updateCrmInvoice = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/invoice/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmInvoice = function (params, ids, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/invoice' + (ids && (typeof ids === 'number' || typeof ids === 'string') ? '/' + ids : '') + '.json',
            ids && typeof ids === 'object' ? { invoiceids: ids } : null,
            options
        );
        return true;
    };

    var getInvoicePdfExistingOrCreate = function (params, id, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/invoice/' + id + '/pdf.json',
            null,
            options
        );
        return true;
    };

    var getInvoiceConverterData = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/invoice/converter/data.json',
            data,
            options
        );
        return true;
    };

    var addCrmInvoiceLine = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/invoiceline.json',
            data,
            options
        );
        return true;
    };

    var updateCrmInvoiceLine = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/invoiceline/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmInvoiceLine = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/invoiceline/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmInvoiceSettings = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/invoice/settings.json',
            null,
            options
        );
    };

    var updateCrmInvoiceSettingsName = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/invoice/settings/name.json',
            data,
            options
        );
        return true;
    };

    var updateCrmInvoiceSettingsTerms = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/invoice/settings/terms.json',
            data,
            options
        );
        return true;
    };

    var getCrmCurrencyRates = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/currency/rates.json',
            null,
            options
        );
    };

    var getCrmCurrencyRateById = function (params, id, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/currency/rates/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmCurrencyRateByCurrencies = function (params, from, to, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/currency/rates/' + from + '/' + to + '.json',
            null,
            options
        );
        return true;
    };

    var addCrmCurrencyRate = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/currency/rates.json',
            data,
            options
        );
        return true;
    };

    var updateCrmCurrencyRate = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/currency/rates/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmCurrencyRate = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/currency/rates/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmContactTweets = function (params, contactid, count, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/' + contactid + '/tweets.json',
            { contactid: contactid, count: count },
            options
        );
    };

    var getCrmContactTwitterProfiles = function (params, searchText, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/twitterprofile.json',
            { searchText: searchText },
            options
        );
    };

    var getCrmContactFacebookProfiles = function (params, searchText, isUser, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/contact/facebookprofile.json',
            { searchText: searchText, isUser: isUser },
            options
        );
    };

    var removeCrmContactAvatar = function (params, contactid, data, options) {
        return addRequest(
            null,
            params,
            REMOVE,
            'crm/contact/' + contactid + '/avatar.json',
            data,
            options
        );
    };

    var updateCrmContactAvatar = function (params, contactid, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'crm/contact/' + contactid + '/avatar.json',
            data,
            options
        );
    };

    var getCrmContactSocialMediaAvatar = function (params, data, options) {
        return addRequest(
            null,
            params,
            ADD,
            'crm/contact/socialmediaavatar.json',
            { socialNetworks: data },
            options
        );
    };

    var startCrmImportFromCSV = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/' + data.entityType + '/import/start.json',
            data,
            options
        );
        return true;
    };

    var getStatusCrmImportFromCSV = function (params, data, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/' + data.entityType + '/import/status.json',
            data,
            options
        );
        return true;
    };

    var getCrmImportFromCSVSampleRow = function (params, data, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/import/samplerow.json',
            data,
            options
        );
        return true;
    };

    var uploadFakeCrmImportFromCSV = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/import/uploadfake.json',
            data,
            options
        );
        return true;
    };

    var getStatusExportToCSV = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/export/status.json',
            null,
            options
        );
    };

    var cancelExportToCSV = function (params, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/export/cancel.json',
            null,
            options
        );
    };

    var startCrmExportToCSV = function (params, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/export/start.json',
            null,
            options
        );
        return true;
    };

    //#region VoIP

    var getCrmVoipAvailableNumbers = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/numbers/available.json',
            null,
            options
        );
        return true;
    };

    var getCrmVoipExistingNumbers = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/numbers/existing.json',
            null,
            options
        );
        return true;
    };

    var getCrmVoipUnlinkedNumbers = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/numbers/unlinked.json',
            null,
            options
        );
        return true;
    };

    var getCrmCurrentVoipNumber = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/numbers/current.json',
            null,
            options
        );
        return true;
    };

    var createCrmVoipNumber = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/numbers.json',
            data,
            options
        );
        return true;
    };

    var linkCrmVoipNumber = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/numbers/link.json',
            data,
            options
        );
        return true;
    };

    var removeCrmVoipNumber = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/voip/numbers/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var updateCrmVoipNumberSettings = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/voip/numbers/' + id + '/settings.json',
            data,
            options
        );
        return true;
    };

    var updateCrmVoipSettings = function (params, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/voip/numbers/settings.json',
            data,
            options
        );
        return true;
    };

    var getCrmVoipSettings = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/numbers/settings.json',
            null,
            options
        );
        return true;
    };

    var getCrmVoipNumberOperators = function (params, id, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/numbers/' + id + '/oper.json',
            null,
            options
        );
        return true;
    };

    var addCrmVoipNumberOperators = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/voip/numbers/' + id + '/oper.json',
            data,
            options
        );
        return true;
    };

    var updateCrmVoipOperator = function (params, id, data, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'crm/voip/opers/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var removeCrmVoipNumberOperators = function (params, id, data, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/voip/numbers/' + id + '/oper.json',
            data,
            options
        );
        return true;
    };

    var callVoipNumber = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/call.json',
            data,
            options
        );
        return true;
    };

    var answerVoipCall = function (params, id, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/call/' + id + '/answer.json',
            id,
            options
        );
        return true;
    };

    var rejectVoipCall = function (params, id, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/call/' + id + '/reject.json',
            id,
            options
        );
        return true;
    };

    var redirectVoipCall = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/call/' + id + '/redirect.json',
            data,
            options
        );
        return true;
    };

    var saveVoipCall = function (params, id, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/call/' + id + '.json',
            data,
            options
        );
        return true;
    };

    var saveVoipCallPrice = function (params, id, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/voip/price/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getVoipMissedCalls = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/call/missed.json',
            null,
            options
        );
    };

    var getVoipCalls = function (params, data, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/call.json',
            data,
            options
        );
    };

    var getVoipCall = function (params, id, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/call/' + id + '.json',
            null,
            options
        );
    };

    var getVoipToken = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/token.json',
            null,
            options
        );
        return true;
    };

    var getVoipUploads = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/voip/uploads.json',
            null,
            options
        );
        return true;
    };

    var deleteVoipUploads = function (params, data, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/voip/uploads.json',
            data,
            options
        );
        return true;
    };
    
    //#endregion

    //#region Reports

    var getCrmReportFiles = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'crm/report/files.json',
            null,
            options
        );
    };

    var removeCrmReportFile = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'crm/report/file/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var getCrmReportStatus = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/report/status.json',
            null,
            options
        );
    };

    var terminateCrmReport = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'crm/report/terminate.json',
            null,
            options
        );
    };

    var checkCrmReport = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/report/check.json',
            data,
            options
        );
    };

    var generateCrmReport = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'crm/report/generate.json',
            data,
            options
        );
    };

    //#endregion

    /* </crm> */

    /* <mail> */
    var getMailFilteredMessages = function (params, filter_data, options) {
        return addRequest(
            customEvents.getMailFilteredMessages,
            params,
            GET,
            'mail/messages.json',
            filter_data,
            options
        );
    };

    var getMailFolders = function (params, options) {
        return addRequest(
            customEvents.getMailFolders,
            params,
            GET,
            'mail/folders.json',
            null,
            options
        );
    };

    var getAccounts = function (params, options) {
        return addRequest(
            customEvents.getAccounts,
            params,
            GET,
            'mail/accounts.json',
            null,
            options
        );
    };

    var getMailTags = function (params, options) {
        return addRequest(
            customEvents.getMailTags,
            params,
            GET,
            'mail/tags.json',
            null,
            options
        );
    };

    var getMailMessage = function (params, id, data, options) {
        return addRequest(
            customEvents.getMailMessage,
            params,
            GET,
            'mail/messages/' + id + '.json',
            data,
            options
        );
    };

    var getMailboxSignature = function (params, id, data, options) {
        return addRequest(
            customEvents.getMailboxSignature,
            params,
            GET,
            'mail/signature/' + id + '.json',
            data,
            options
        );
    };

    var updateMailboxSignature = function (params, id, data, options) {
        return addRequest(
            customEvents.updateMailboxSignature,
            params,
            ADD,
            'mail/signature/update/' + id + '.json',
            data,
            options
        );
    };

    var updateMailboxAutoreply = function (params, id, data, options) {
        return addRequest(
            customEvents.updateMailboxAutoreply,
            params,
            ADD,
            'mail/autoreply/update/' + id + '.json',
            data,
            options
        );
    };

    var getLinkedCrmEntitiesInfo = function (params, data, options) {
        return addRequest(
            customEvents.getLinkedCrmEntitiesInfo,
            params,
            GET,
            'mail/crm/linked/entities.json',
            data,
            options
        );
    };

    var getNextMailMessageId = function (params, id, filter_data, options) {
        return addRequest(
            customEvents.getNextMailMessageId,
            params,
            GET,
            'mail/messages/' + id + '/next.json' + (filter_data ? "?" + jq.param(filter_data) : ""),
            null,
            options
        );
    };

    var getPrevMailMessageId = function (params, id, filter_data, options) {
        return addRequest(
            customEvents.getPrevMailMessageId,
            params,
            GET,
            'mail/messages/' + id + '/prev.json' + (filter_data ? "?" + jq.param(filter_data) : ""),
            filter_data,
            options
        );
    };

    var getMailConversation = function (params, id, data, options) {
        return addRequest(
            customEvents.getMailConversation,
            params,
            GET,
            'mail/conversation/' + id + '.json',
            data,
            options
        );
    };

    var getNextMailConversationId = function (params, id, filter_data, options) {
        return addRequest(
            customEvents.getNextMailConversationId,
            params,
            GET,
            'mail/conversation/' + id + '/next.json' + (filter_data ? "?" + jq.param(filter_data) : ""),
            filter_data,
            options
        );
    };

    var getPrevMailConversationId = function (params, id, filter_data, options) {
        return addRequest(
            customEvents.getPrevMailConversationId,
            params,
            GET,
            'mail/conversation/' + id + '/prev.json' + (filter_data ? "?" + jq.param(filter_data) : ""),
            filter_data,
            options
        );
    };

    var removeMailFolderMessages = function (params, id, options) {
        return addRequest(
            customEvents.removeMailFolderMessages,
            params,
            REMOVE,
            'mail/folders/' + id + '/messages.json',
            null,
            options
        );
    };

    var restoreMailMessages = function (params, data, options) {
        return addRequest(
            customEvents.restoreMailMessages,
            params,
            UPDATE,
            'mail/messages/restore.json',
            data,
            options
        );
    };

    var moveMailMessages = function (params, ids, folder, options) {
        return addRequest(
            customEvents.moveMailMessages,
            params,
            UPDATE,
            'mail/messages/move.json',
            { ids: ids, folder: folder },
            options
        );
    };

    var removeMailMessages = function (params, ids, options) {
        return addRequest(
            customEvents.removeMailMessages,
            params,
            UPDATE,
            'mail/messages/remove.json',
            { ids: ids },
            options
        );
    };

    var markMailMessages = function (params, ids, status, options) {
        return addRequest(
            customEvents.markMailMessages,
            params,
            UPDATE,
            'mail/messages/mark.json',
            { ids: ids, status: status },
            options
        );
    };

    var createMailTag = function (params, name, style, addresses, options) {
        return addRequest(
            customEvents.createMailTag,
            params,
            ADD,
            'mail/tags.json',
            { name: name, style: style, addresses: addresses },
            options
        );
    };

    var updateMailTag = function (params, id, name, style, addresses, options) {
        return addRequest(
            customEvents.updateMailTag,
            params,
            UPDATE,
            'mail/tags/' + id + '.json',
            { name: name, style: style, addresses: addresses },
            options
        );
    };

    var removeMailTag = function (params, id, options) {
        return addRequest(
            customEvents.removeMailTag,
            params,
            REMOVE,
            'mail/tags/' + id + '.json',
            null,
            options
        );
    };

    var setMailTag = function (params, messages_ids, tag_id, options) {
        return addRequest(
            customEvents.setMailTag,
            params,
            UPDATE,
            'mail/tags/' + tag_id + '/set.json',
            { messages: messages_ids },
            options
        );
    };

    var setMailConversationsTag = function (params, messages_ids, tag_id, options) {
        return addRequest(
            customEvents.setMailConversationsTag,
            params,
            UPDATE,
            'mail/conversations/tag/' + tag_id + '/set.json',
            { messages: messages_ids },
            options
        );
    };

    var unsetMailTag = function (params, messages_ids, tag_id, options) {
        return addRequest(
            customEvents.unsetMailTag,
            params,
            UPDATE,
            'mail/tags/' + tag_id + '/unset.json',
            { messages: messages_ids },
            options
        );
    };

    var unsetMailConversationsTag = function (params, messages_ids, tag_id, options) {
        return addRequest(
            customEvents.unsetMailConversationsTag,
            params,
            UPDATE,
            'mail/conversations/tag/' + tag_id + '/unset.json',
            { messages: messages_ids },
            options
        );
    };

    var addMailDocument = function (params, id, data, options) {
        return addRequest(
            customEvents.addMailDocument,
            params,
            ADD,
            'mail/messages/' + id + '/document.json',
            data,
            options
        );
    };

    var removeMailMailbox = function (params, email, options) {
        return addRequest(
            customEvents.removeMailMailbox,
            params,
            REMOVE,
            'mail/accounts.json',
            { email: email },
            options
        );
    };

    var getMailDefaultMailboxSettings = function (params, email, options) {
        return addRequest(
            customEvents.getMailDefaultMailboxSettings,
            params,
            GET,
            'mail/accounts/setups.json',
            { email: email, action: params.action },
            options
        );
    };

    var getMailMailbox = function (params, email, options) {
        return addRequest(
            customEvents.getMailMailbox,
            params,
            GET,
            'mail/accounts/single.json',
            { email: email },
            options
        );
    };

    var setDefaultAccount = function (params, isDefault, email, options) {
        return addRequest(
            customEvents.setDefaultAccount,
            params,
            UPDATE,
            'mail/accounts/default.json',
            { email: email, isDefault: isDefault },
            options
        );
    };

    var createMailMailboxSimple = function (params, email, password, options) {
        return addRequest(
            customEvents.createMailMailboxSimple,
            params,
            ADD,
            'mail/accounts/simple.json',
            { email: email, password: password },
            options
        );
    };

    var createMailMailboxOAuth = function (params, code, serviceType, options) {
        return addRequest(
            customEvents.createMailMailboxOAuth,
            params,
            ADD,
            'mail/accounts/oauth.json',
            { code: code, type: serviceType },
            options
        );
    };

    var updateMailMailboxOAuth = function (params, code, serviceType, mailboxId, options) {
        return addRequest(
            customEvents.updateMailMailboxOAuth,
            params,
            UPDATE,
            'mail/accounts/oauth.json',
            { code: code, type: serviceType, mailboxId: mailboxId },
            options
        );
    };
    var createMailMailbox = function (params, name, email, pop3_account, pop3_password, pop3_port, pop3_server,
                                     smtp_account, smtp_password, smtp_port, smtp_server, smtp_auth, imap, restrict, incoming_encryption_type,
                                     outcoming_encryption_type, auth_type_in, auth_type_smtp, options) {
        var data = {
            name: name,
            email: email,
            account: pop3_account,
            password: pop3_password,
            port: pop3_port,
            server: pop3_server,
            smtp_account: smtp_account,
            smtp_password: smtp_password,
            smtp_port: smtp_port,
            smtp_server: smtp_server,
            smtp_auth: smtp_auth,
            imap: imap,
            restrict: restrict,
            incoming_encryption_type: incoming_encryption_type,
            outcoming_encryption_type: outcoming_encryption_type,
            auth_type_in: auth_type_in,
            auth_type_smtp: auth_type_smtp
        };

        return addRequest(
            customEvents.createMailMailbox,
            params,
            ADD,
            'mail/accounts.json',
            data,
            options
        );
    };

    var updateMailMailbox = function (params, name, email, pop3_account, pop3_password, pop3_port, pop3_server,
                                     smtp_account, smtp_password, smtp_port, smtp_server, smtp_auth, restrict, incoming_encryption_type,
                                     outcoming_encryption_type, auth_type_in, auth_type_smtp, options) {
        var data = {
            name: name,
            email: email,
            account: pop3_account,
            password: pop3_password,
            port: pop3_port,
            server: pop3_server,
            smtp_account: smtp_account,
            smtp_password: smtp_password,
            smtp_port: smtp_port,
            smtp_server: smtp_server,
            smtp_auth: smtp_auth,
            restrict: restrict,
            incoming_encryption_type: incoming_encryption_type,
            outcoming_encryption_type: outcoming_encryption_type,
            auth_type_in: auth_type_in,
            auth_type_smtp: auth_type_smtp
        };

        return addRequest(
            customEvents.updateMailMailbox,
            params,
            UPDATE,
            'mail/accounts.json',
            data,
            options
        );
    };

    var setMailMailboxState = function (params, email, state, options) {
        return addRequest(
            customEvents.setMailMailboxState,
            params,
            UPDATE,
            'mail/accounts/state.json',
            { email: email, state: state },
            options
        );
    };

    var removeMailMessageAttachment = function (params, message_id, attachment_id, options) {
        return addRequest(
            customEvents.removeMailMessageAttachment,
            params,
            REMOVE,
            'mail/messages/' + message_id + '/attachments/' + attachment_id + '.json',
            null,
            options
        );
    };

    var sendMailMessage = function (params, message, options) {
        if (!(message instanceof ASC.Mail.Message)) {
            console.error("Unsupported message format");
            return null;
        }

        return addRequest(
            customEvents.sendMailMessage,
            params,
            UPDATE,
            'mail/messages/send.json',
            message.ToData(),
            options
        );
    };

    var saveMailMessage = function (params, message, options) {
        if (!(message instanceof ASC.Mail.Message)) {
            console.error("Unsupported message format");
            return null;
        }

        return addRequest(
            customEvents.saveMailMessage,
            params,
            UPDATE,
            'mail/messages/save.json',
            message.ToData(),
            options
        );
    };

    var searchEmails = function (params, data, options) {
        return addRequest(
            customEvents.searchEmails,
            params,
            GET,
            'mail/emails/search.json',
            data,
            options
        );
    };

    var getMailContacts = function (params, filterData, options) {
        return addRequest(
            customEvents.getMailContacts,
            params,
            GET,
            'mail/contacts.json',
            filterData,
            options
        );
    };

    var getMailContactsByInfo = function (params, data, options) {
        return addRequest(
            customEvents.getMailContactsByInfo,
            params,
            GET,
            'mail/contacts/bycontactinfo.json',
            data,
            options
        );
    };

    var createMailContact = function (params, name, description, emails, phoneNumbers, options) {
        return addRequest(
            customEvents.createMailContact,
            params,
            ADD,
            'mail/contact/add.json',
            { name: name, description: description, emails: emails, phoneNumbers: phoneNumbers },
            options
        );
    };

    var deleteMailContacts = function (params, ids, options) {
        return addRequest(
            customEvents.deleteMailContacts,
            params,
            UPDATE,
            'mail/contacts/remove.json',
            { ids: ids },
            options
        );
    };

    var updateMailContact = function (params, id, name, description, emails, phoneNumbers, options) {
        return addRequest(
            customEvents.updateMailContact,
            params,
            UPDATE,
            'mail/contact/update.json',
            { id: id, name: name, description: description, emails: emails, phoneNumbers: phoneNumbers },
            options
        );
    };

    var getMailAlerts = function (params, options) {
        return addRequest(
            customEvents.getMailAlerts,
            params,
            GET,
            'mail/alert.json',
            null,
            options
        );
    };

    var deleteMailAlert = function (params, id, options) {
        return addRequest(
            customEvents.deleteMailAlert,
            params,
            REMOVE,
            'mail/alert/' + id + '.json',
            null,
            options
        );
    };

    var getMailFilteredConversations = function (params, filter_data, options) {
        return addRequest(
            customEvents.getMailFilteredConversations,
            params,
            GET,
            'mail/conversations.json',
            filter_data,
            options
        );
    };

    var moveMailConversations = function (params, ids, folder, options) {
        return addRequest(
            customEvents.moveMailConversations,
            params,
            UPDATE,
            'mail/conversations/move.json',
            { ids: ids, folder: folder },
            options
        );
    };

    var restoreMailConversations = function (params, data, options) {
        return addRequest(
            customEvents.restoreMailConversations,
            params,
            UPDATE,
            'mail/conversations/restore.json',
            data,
            options
        );
    };

    var removeMailConversations = function (params, ids, options) {
        return addRequest(
            customEvents.removeMailConversations,
            params,
            UPDATE,
            'mail/conversations/remove.json',
            { ids: ids },
            options
        );
    };

    var markMailConversations = function (params, ids, status, options) {
        return addRequest(
            customEvents.markMailConversations,
            params,
            UPDATE,
            'mail/conversations/mark.json',
            { ids: ids, status: status },
            options
        );
    };

    var getMailDisplayImagesAddresses = function (params, options) {
        return addRequest(
            customEvents.getMailDisplayImagesAddresses,
            params,
            GET,
            'mail/display_images/addresses.json',
            null,
            options
        );
    };

    var createDisplayImagesAddress = function (params, email, options) {
        return addRequest(
            customEvents.createDisplayImagesAddress,
            params,
            ADD,
            'mail/display_images/address.json',
            { address: email },
            options
        );
    };

    var removeDisplayImagesAddress = function (params, email, options) {
        return addRequest(
            customEvents.removeDisplayImagesAddress,
            params,
            REMOVE,
            'mail/display_images/address.json',
            { address: email },
            options
        );
    };

    var linkChainToCrm = function (params, message_id, crm_contacts_id, options) {
        return addRequest(
            customEvents.linkChainToCrm,
            params,
            UPDATE,
            'mail/conversations/crm/link.json',
            {
                id_message: message_id,
                crm_contact_ids: crm_contacts_id
            },
            options
        );
    };

    var markChainAsCrmLinked = function (params, message_id, crm_contacts_id, options) {
        return addRequest(
            customEvents.markChainAsCrmLinked,
            params,
            UPDATE,
            'mail/conversations/crm/mark.json',
            {
                id_message: message_id,
                crm_contact_ids: crm_contacts_id
            },
            options
        );
    };

    var unmarkChainAsCrmLinked = function (params, message_id, crm_contacts_id, options) {
        return addRequest(
            customEvents.unmarkChainAsCrmLinked,
            params,
            UPDATE,
            'mail/conversations/crm/unmark.json',
            {
                id_message: message_id,
                crm_contact_ids: crm_contacts_id
            },
            options
        );
    };

    var exportMessageToCrm = function (params, message_id, crm_contacts_id, options) {
        return addRequest(
            customEvents.exportMessageToCrm,
            params,
            UPDATE,
            'mail/messages/crm/export.json',
            {
                id_message: message_id,
                crm_contact_ids: crm_contacts_id
            },
            options
        );
    };

    var isConversationLinkedWithCrm = function (params, messageId, options) {
        return addRequest(
            customEvents.isConversationLinkedWithCrm,
            params,
            GET,
            'mail/conversations/link/crm/status.json',
            {
                message_id: messageId
            },
            options
        );
    };

    var getMailHelpCenterHtml = function (params, options) {
        return addRequest(
            customEvents.getMailHelpCenterHtml,
            params,
            GET,
            'mail/helpcenter.json',
            null,
            options
        );
    };

    var exportAllAttachmentsToMyDocuments = function (params, message_id, options) {
        return addRequest(
            customEvents.exportAllAttachmentsToMyDocuments,
            params,
            UPDATE,
            'mail/messages/attachments/export.json',
            {
                id_message: message_id
            },
            options
        );
    };

    var exportAllAttachmentsToDocuments = function (params, message_id, folder_id, options) {
        return addRequest(
            customEvents.exportAllAttachmentsToDocuments,
            params,
            UPDATE,
            'mail/messages/attachments/export.json',
            {
                id_message: message_id,
                id_folder: folder_id
            },
            options
        );
    };

    var exportAttachmentToMyDocuments = function (params, attachment_id, options) {
        return addRequest(
            customEvents.exportAttachmentToMyDocuments,
            params,
            UPDATE,
            'mail/messages/attachment/export.json',
            {
                id_attachment: attachment_id
            },
            options
        );
    };

    var exportAttachmentToDocuments = function (params, attachment_id, folder_id, options) {
        return addRequest(
            customEvents.exportAttachmentToDocuments,
            params,
            UPDATE,
            'mail/messages/attachment/export.json',
            {
                id_attachment: attachment_id,
                id_folder: folder_id
            },
            options
        );
    };

    var setEMailInFolder = function (params, mailbox_id, email_in_folder, options) {
        return addRequest(
            customEvents.setEMailInFolder,
            params,
            UPDATE,
            'mail/accounts/emailinfolder.json',
            {
                mailbox_id: mailbox_id,
                email_in_folder: email_in_folder
            },
            options
        );
    };

    var getMailServer = function (params, options) {
        return addRequest(
            customEvents.getMailServer,
            params,
            GET,
            'mailserver/server.json',
            null,
            options
        );
    };

    var getMailServerFullInfo = function (params, options) {
        return addRequest(
            customEvents.getMailServerFullInfo,
            params,
            GET,
            'mailserver/serverinfo/get.json',
            null,
            options
        );
    };

    var getMailServerFreeDns = function (params, options) {
        return addRequest(
            customEvents.getMailServerFreeDns,
            params,
            GET,
            'mailserver/freedns/get.json',
            null,
            options
        );
    };

    var getMailDomains = function (params, options) {
        return addRequest(
            customEvents.getMailDomains,
            params,
            GET,
            'mailserver/domains/get.json',
            null,
            options
        );
    };

    var getCommonMailDomain = function (params, options) {
        return addRequest(
            customEvents.getCommonMailDomain,
            params,
            GET,
            'mailserver/domains/common.json',
            null,
            options
        );
    };

    var addMailDomain = function (params, domain_name, dns_id, options) {
        return addRequest(
            customEvents.addMailDomain,
            params,
            ADD,
            'mailserver/domains/add.json',
            { name: domain_name, id_dns: dns_id },
            options
        );
    };

    var removeMailDomain = function (params, id_domain, options) {
        return addRequest(
            customEvents.removeMailDomain,
            params,
            REMOVE,
            'mailserver/domains/remove/' + id_domain + '.json',
            null,
            options
        );
    };

    var addMailbox = function (params, name, local_part, domain_id, user_id, options) {
        return addRequest(
            customEvents.addMailbox,
            params,
            ADD,
            'mailserver/mailboxes/add.json',
            { name: name, local_part: local_part, domain_id: domain_id, user_id: user_id },
            options
        );
    };

    var addMyMailbox = function (params, mailbox_name, options) {
        return addRequest(
            customEvents.addMyMailbox,
            params,
            ADD,
            'mailserver/mailboxes/addmy.json',
            { name: mailbox_name },
            options
        );
    };

    var getMailboxes = function (params, options) {
        return addRequest(
            customEvents.getMailboxes,
            params,
            GET,
            'mailserver/mailboxes/get.json',
            null,
            options
        );
    };

    var removeMailbox = function (params, id_mailbox, options) {
        return addRequest(
            customEvents.removeMailbox,
            params,
            REMOVE,
            'mailserver/mailboxes/remove/' + id_mailbox + '.json',
            null,
            options
        );
    };

    var updateMailbox = function (params, mailbox_id, name, options) {
        return addRequest(
            customEvents.updateMailbox,
            params,
            UPDATE,
            'mailserver/mailboxes/update.json',
            { mailbox_id: mailbox_id, name: name },
            options
        );
    };

    var addMailBoxAlias = function (params, mailbox_id, address_name, options) {
        return addRequest(
            customEvents.addMailBoxAlias,
            params,
            UPDATE,
            'mailserver/mailboxes/alias/add.json',
            { mailbox_id: mailbox_id, alias_name: address_name },
            options
        );
    };

    var removeMailBoxAlias = function (params, mailbox_id, address_id, options) {
        return addRequest(
            customEvents.removeMailBoxAlias,
            params,
            UPDATE,
            'mailserver/mailboxes/alias/remove.json',
            { mailbox_id: mailbox_id, address_id: address_id },
            options
        );
    };

    var addMailGroup = function (params, group_name, domain_id, address_ids, options) {
        return addRequest(
            customEvents.addMailGroup,
            params,
            ADD,
            'mailserver/groupaddress/add.json',
            { name: group_name, domain_id: domain_id, address_ids: address_ids },
            options
        );
    };

    var addMailGroupAddress = function (params, group_id, address_id, options) {
        return addRequest(
            customEvents.addMailGroupAddress,
            params,
            UPDATE,
            'mailserver/groupaddress/address/add.json',
            { mailgroup_id: group_id, address_id: address_id },
            options
        );
    };

    var removeMailGroupAddress = function (params, group_id, address_id, options) {
        return addRequest(
            customEvents.removeMailGroupAddress,
            params,
            REMOVE,
            'mailserver/groupaddress/addresses/remove.json',
            { mailgroup_id: group_id, address_id: address_id },
            options
        );
    };

    var getMailGroups = function (params, options) {
        return addRequest(
            customEvents.getMailGroups,
            params,
            GET,
            'mailserver/groupaddress/get.json',
            null,
            options
        );
    };

    var removeMailGroup = function (params, id_group, options) {
        return addRequest(
            customEvents.removeMailGroup,
            params,
            REMOVE,
            'mailserver/groupaddress/remove/' + id_group + '.json',
            null,
            options
        );
    };

    var isDomainExists = function (params, domain_name, options) {
        return addRequest(
            customEvents.isDomainExists,
            params,
            GET,
            'mailserver/domains/exists.json',
            { name: domain_name },
            options
        );
    };

    var checkDomainOwnership = function (params, domain_name, options) {
        return addRequest(
            customEvents.checkDomainOwnership,
            params,
            GET,
            'mailserver/domains/ownership/check.json',
            { name: domain_name },
            options
        );
    };

    var getDomainDnsSettings = function (params, domain_id, options) {
        return addRequest(
            customEvents.getDomainDnsSettings,
            params,
            GET,
            'mailserver/domains/dns/get.json',
            { id: domain_id },
            options
        );
    };

    var createNotificationAddress = function (params, address_username, password, domain_id, options) {
        return addRequest(
            customEvents.createNotificationAddress,
            params,
            ADD,
            'mailserver/notification/address/add.json',
            { name: address_username, password: password, domain_id: domain_id },
            options
        );
    };

    var removeNotificationAddress = function (params, address, options) {
        return addRequest(
            customEvents.removeNotificationAddress,
            params,
            REMOVE,
            'mailserver/notification/address/remove.json',
            { address: address },
            options
        );
    };

    var addCalendarBody = function (params, id_message, ical_body, options) {
        return addRequest(
            customEvents.addCalendarBody,
            params,
            ADD,
            'mail/messages/calendarbody/add',
            { id_message: id_message, ical_body: ical_body },
            options
        );
    };

    var setMailConversationEnabledFlag = function (params, enabled, options) {
        return addRequest(
            customEvents.setMailConversationEnabledFlag,
            params,
            UPDATE,
            'mail/settings/conversationsEnabled',
            { enabled: enabled },
            options
        );
    };

    var setMailAlwaysDisplayImagesFlag = function (params, enabled, options) {
        return addRequest(
            customEvents.setMailAlwaysDisplayImagesFlag,
            params,
            UPDATE,
            'mail/settings/alwaysDisplayImages',
            { enabled: enabled },
            options
        );
    };

    var setMailCacheUnreadMessagesFlag = function (params, enabled, options) {
        return addRequest(
            customEvents.setMailCacheUnreadMessagesFlag,
            params,
            UPDATE,
            'mail/settings/cacheMessagesEnabled',
            { enabled: enabled },
            options
        );
    };

    var setMailEnableGoNextAfterMove = function (params, enabled, options) {
        return addRequest(
            customEvents.setMailEnableGoNextAfterMove,
            params,
            UPDATE,
            'mail/settings/goNextAfterMoveEnabled',
            { enabled: enabled },
            options
        );
    };
    

    var getMailServerInfo = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'mail/mailservice/get.json',
            null,
            options
        );
    };

    var connectMailServerInfo = function (params, ip, sqlip, user, password, options) {
        return addRequest(
            null,
            params,
            ADD,
            'mail/mailservice/connect.json',
            {
                ip: ip,
                sqlip: sqlip,
                user: user,
                password: password
            },
            options
        );
    };

    var saveMailServerInfo = function (params, ip, sqlip, user, password, token, host, options) {
        return addRequest(
            null,
            params,
            ADD,
            'mail/mailservice/save.json',
            {
                ip: ip,
                sqlip: sqlip,
                user: user,
                password: password,
                token: token,
                host: host
            },
            options
        );
    };

    var getMailOperationStatus = function (params, id, options) {
        return addRequest(
            customEvents.getMailOperationStatus,
            null,
            GET,
            'mail/operations/' + id + '.json',
            null,
            options
        );
    };

    /* </mail> */

    /* <settings> */
    var getWebItemSecurityInfo = function (params, data, options) {
        return addRequest(
            null,
            params,
            GET,
            'settings/security.json',
            typeof data === 'number' || typeof data === 'string' ? { ids: [data] } : data,
            options
        );
    };

    var setWebItemSecurity = function (params, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'settings/security.json',
            data,
            options
        );
    };

    var setAccessToWebItems = function (params, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'settings/security/access.json',
            data,
            options
        );
    };

    var setProductAdministrator = function (params, data, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'settings/security/administrator.json',
            data,
            options
        );
    };

    var isProductAdministrator = function (params, data, options) {
        return addRequest(
            null,
            params,
            GET,
            'settings/security/administrator.json',
            data,
            options
        );
    };
    var getPortalSettings = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'settings.json',
            null,
            options
        );
    };

    var getPortalLogo = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'settings/logo.json',
            null,
            options
        );
    };

    var getIpRestrictions = function (options) {
        return addRequest(
            null,
            null,
            GET,
            'settings/iprestrictions.json',
            null,
            options
        );
    };

    var saveIpRestrictions = function (data, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/iprestrictions.json',
            data,
            options
        );
    };

    var updateIpRestrictionsSettings = function (data, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/iprestrictions/settings.json',
            data,
            options
        );
    };

    var updateTipsSettings = function (data, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/tips.json',
            data,
            options
        );
    };

    var updateTipsSubscription = function (options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/tips/change/subscription.json',
            null,
            options
        );
    };

    var smsValidationSettings = function (enable, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/sms.json',
            { enable: enable },
            options
        );
    };

    var closeWelcomePopup = function () {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/welcome/close.json',
            null,
            null
        );
    };

    var setColorTheme = function (params, theme, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'settings/colortheme.json',
            { theme: theme },
            options
        );
    };

    var setTimaAndLanguage = function (lng, timeZoneID, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            "settings/timeandlanguage.json",
            { lng: lng, timeZoneID: timeZoneID },
            options
        );
    };

    var setDefaultpage = function (params, defaultProductID, options) {
        return addRequest(
            null,
            params,
            UPDATE,
            'settings/defaultpage.json',
            { defaultProductID: defaultProductID },
            options
        );

    };

    // LDAP

    var saveLdapSettings = function (params, settings, options) {
        addRequest(
            null,
            params,
            ADD,
            'settings/ldap.json',
            {settings: settings},
            options
        );
        return true;
    };

    var getLdapSettings = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/ldap.json',
            null,
            options
        );
        return true;
    };

    var getLdapDefaultSettings = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/ldap/default.json',
            null,
            options
        );
        return true;
    };

    var getLdapStatus = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/ldap/status.json',
            null,
            options
        );
        return true;
    };

    var syncLdap = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/ldap/sync.json',
            null,
            options
        );
        return true;
    };

    var updateEmailActivationSettings = function (data, options) {
        return addRequest(
            null,
            null,
            UPDATE,
            'settings/emailactivation.json',
            data,
            options
        );
    };

    /* </settings> */

    //#region Security

    var getLoginEvents = function (options) {
        return addRequest(
            null,
            null,
            GET,
            'security/audit/login/last.json',
            null,
            options
        );
    };

    var getAuditEvents = function (params, id, options) {
        return addRequest(
            null,
            params,
            GET,
            'security/audit/events/last.json',
            null,
            options
        );
    };

    var createLoginHistoryReport = function (params, options) {
        return addRequest(
            null,
            params,
            ADD,
            'security/audit/login/report.json',
            null,
            options
        );
    };

    var createAuditTrailReport = function (params, options) {
        return addRequest(
            null,
            params,
            ADD,
            'security/audit/events/report.json',
            null,
            options
        );
    };

    var getAuditSettings = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'security/audit/settings/lifetime.json',
            null,
            options
        );
    };

    var setAuditSettings = function (params, loginLifeTime, auditLifeTime, options) {
        return addRequest(
            null,
            params,
            ADD,
            'security/audit/settings/lifetime.json',
            {
                settings: {
                    LoginHistoryLifeTime: loginLifeTime,
                    AuditTrailLifeTime: auditLifeTime
                }
            },
            options
        );
    };

    //#endregion

    var getTalkUnreadMessages = function (params, options) {
        addRequest(
            customEvents.getTalkUnreadMessages,
            params,
            GET,
            'portal/talk/unreadmessages.json',
            null,
            options
        );
        return true;
    };

    var registerUserOnPersonal = function (data, options) {
        addRequest(
            null,
            null,
            ADD,
            'authentication/register.json',
            data,
            options
        );
        return true;
    };

    var saveWhiteLabelSettings = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'settings/whitelabel/save.json',
            data,
            options
        );
        return true;
    };

    var restoreWhiteLabelSettings = function (params, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'settings/whitelabel/restore.json',
            null,
            options
        );
        return true;
    };

    //#region CustomNavigation

    var getCustomNavigationItems = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/customnavigation/getall.json',
            null,
            options
        );
        return true;
    };

    var getCustomNavigationItemSample = function (params, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/customnavigation/getsample.json',
            null,
            options
        );
        return true;
    };

    var getCustomNavigationItem = function(params, id, options) {
        addRequest(
            null,
            params,
            GET,
            'settings/customnavigation/get/' + id + '.json',
            null,
            options
        );
        return true;
    };

    var createCustomNavigationItem = function (params, data, options) {
        addRequest(
            null,
            params,
            ADD,
            'settings/customnavigation/create.json',
            data,
            options
        );
        return true;
    };

    var deleteCustomNavigationItem = function (params, id, options) {
        addRequest(
            null,
            params,
            REMOVE,
            'settings/customnavigation/delete/' + id + '.json',
            null,
            options
        );
        return true;
    };

    //#endregion

    var getCalendars = function (params, dateStart, dateEnd, options) {
        var start = dateStart instanceof Date ? Teamlab.serializeTimestamp(dateStart, true) : dateStart;
        var end = dateEnd instanceof Date ? Teamlab.serializeTimestamp(dateEnd, true) : dateEnd;

        addRequest(
            null,
            params,
            GET,
            "calendar/calendars/" + start + "/" + end + ".json",
            null,
            options
        );
        return true;
    };

    var getCalendarEventByUid = function (params, eventUid, options) {
        addRequest(
            null,
            params,
            GET,
            "calendar/events/{0}/historybyuid.json".format(eventUid),
            null,
            options
        );
        return true;
    };

    var getCalendarEventById = function (params, eventId, options) {
        addRequest(
            null,
            params,
            GET,
            "calendar/events/{0}/historybyid.json".format(eventId),
            null,
            options
        );
        return true;
    };

    var importCalendarEventIcs = function (params, calendarId, ics, options) {
        addRequest(
            null,
            params,
            ADD,
            "calendar/importIcs.json",
            { iCalString: ics, calendarId: calendarId },
            options
        );
        return true;
    };

    //#region Bar

    var getBarPromotions = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'portal/bar/promotions.json',
            {
                domain: window.location.hostname,
                page: window.location.pathname + window.location.search
            },
            options
        );
    };

    var markBarPromotion = function (params, id, options) {
        return addRequest(
            null,
            params,
            ADD,
            'portal/bar/promotions/mark/{0}.json'.format(id),
            { id: id },
            options
        );
    };

    var getBarTips = function (params, options) {
        return addRequest(
            null,
            params,
            GET,
            'portal/bar/tips.json',
            {
                page: window.location.pathname + window.location.search + window.location.hash,
                productAdmin: ASC.Resources.Master.IsProductAdmin
            },
            options
        );
    };

    var markBarTip = function (params, id, options) {
        return addRequest(
            null,
            params,
            ADD,
            'portal/bar/tips/mark/{0}.json'.format(id),
            { id: id },
            options
        );
    };

    var deleteBarTips = function (params, options) {
        return addRequest(
            customEvents.removeNotificationAddress,
            params,
            REMOVE,
            'portal/bar/tips.json',
            null,
            options
        );
    };

    //#endregion

    //#region Reassign user data

    var getReassignProgress = function (params, userId, options) {
        addRequest(
            null,
            params,
            GET,
            "people/reassign/progress.json",
            { userId: userId },
            options
        );
        return true;
    };

    var terminateReassign = function (params, userId, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'people/reassign/terminate.json',
            { userId: userId },
            options
        );
        return true;
    };

    var startReassign = function (params, fromUserId, toUserId, options) {
        addRequest(
            null,
            params,
            ADD,
            "people/reassign/start.json",
            { fromUserId: fromUserId, toUserId: toUserId },
            options
        );
        return true;
    };

    //#endregion

    //#region Remove user data

    var getRemoveProgress = function (params, userId, options) {
        addRequest(
            null,
            params,
            GET,
            "people/remove/progress.json",
            { userId: userId },
            options
        );
        return true;
    };

    var terminateRemove = function (params, userId, options) {
        addRequest(
            null,
            params,
            UPDATE,
            'people/remove/terminate.json',
            { userId: userId },
            options
        );
        return true;
    };

    var startRemove = function (params, userId, options) {
        addRequest(
            null,
            params,
            ADD,
            "people/remove/start.json",
            { userId: userId },
            options
        );
        return true;
    };

    //#endregion

    return {
        events: customEvents,

        profile: ServiceFactory.profile,

        constants: {
            dateFormats: ServiceFactory.dateFormats,
            contactTypes: ServiceFactory.contactTypes,
            nameCollections: ServiceFactory.nameCollections
        },

        create: ServiceFactory.create,
        formattingDate: ServiceFactory.formattingDate,
        serializeDate: ServiceFactory.serializeDate,
        serializeTimestamp: ServiceFactory.serializeTimestamp,
        getDisplayTime: ServiceFactory.getDisplayTime,
        getDisplayDate: ServiceFactory.getDisplayDate,
        getDisplayDatetime: ServiceFactory.getDisplayDatetime,
        sortCommentsByTree: ServiceFactory.sortCommentsByTree,

        joint: joint,
        start: start,

        init: init,
        bind: bind,
        unbind: unbind,
        call: call,

        getQuotas: getQuotas,
        recalculateQuota: recalculateQuota,
        checkRecalculateQuota: checkRecalculateQuota,

        remindPwd: remindPwd,
        thirdPartyLinkAccount: thirdPartyLinkAccount,
        thirdPartyUnLinkAccount: thirdPartyUnLinkAccount,

        addProfile: addProfile,
        getProfile: getProfile,
        getProfiles: getProfiles,
        getProfilesByFilter: getProfilesByFilter,
        addGroup: addGroup,
        updateGroup: updateGroup,
        getGroup: getGroup,
        getGroups: getGroups,
        deleteGroup: deleteGroup,
        updateProfile: updateProfile,
        updateUserType: updateUserType,
        updateUserStatus: updateUserStatus,
        updateUserPhoto: updateUserPhoto,
        removeUserPhoto: removeUserPhoto,
        sendInvite: sendInvite,
        removeUser: removeUser,
        removeUsers: removeUsers,
        getUserGroups: getUserGroups,
        removeSelf: removeSelf,
        joinAffiliate: joinAffiliate,

        addCmtBlog: addCmtBlog,
        getCmtBlog: getCmtBlog,
        getCmtBlogs: getCmtBlogs,
        addCmtForumTopic: addCmtForumTopic,
        getCmtForumTopic: getCmtForumTopic,
        getCmtForumTopics: getCmtForumTopics,
        getCmtForumCategories: getCmtForumCategories,
        addCmtForumToCategory: addCmtForumToCategory,
        addCmtEvent: addCmtEvent,
        getCmtEvent: getCmtEvent,
        getCmtEvents: getCmtEvents,
        addCmtBookmark: addCmtBookmark,
        getCmtBookmark: getCmtBookmark,
        getCmtBookmarks: getCmtBookmarks,

        addCmtForumTopicPost: addCmtForumTopicPost,
        addCmtBlogComment: addCmtBlogComment,
        getCmtBlogComments: getCmtBlogComments,
        addCmtEventComment: addCmtEventComment,
        getCmtEventComments: getCmtEventComments,
        subscribeCmtEventComment: subscribeCmtEventComment,
        addCmtBookmarkComment: addCmtBookmarkComment,
        getCmtBookmarkComments: getCmtBookmarkComments,
        subscribeCmtBirthday: subscribeCmtBirthday,
        getCmtPreview: getCmtPreview,

        subscribeProject: subscribeProject,
        getFeeds: getFeeds,
        getNewFeedsCount: getNewFeedsCount,
        readFeeds: readFeeds,
        getPrjTags: getPrjTags,
        getPrjTagsByName: getPrjTagsByName,

        getPrjComments: getPrjComments,
        addPrjTaskComment: addPrjTaskComment,
        getPrjTaskComments: getPrjTaskComments,
        addPrjDiscussionComment: addPrjDiscussionComment,
        getPrjDiscussionComments: getPrjDiscussionComments,

        getPrjDiscussionPreview: getPrjDiscussionPreview,
        getPrjCommentPreview: getPrjCommentPreview,
        getWikiCommentPreview: getWikiCommentPreview,
        getBlogCommentPreview: getBlogCommentPreview,
        getNewsCommentPreview: getNewsCommentPreview,
        getBookmarksCommentPreview:getBookmarksCommentPreview,

        removePrjComment: removePrjComment,
        removeWikiComment:removeWikiComment,
        removeBlogComment: removeBlogComment,
        removeNewsComment:removeNewsComment,
        removeBookmarksComment: removeBookmarksComment,

        addPrjComment: addPrjComment,
        addWikiComment: addWikiComment,
        addBlogComment: addBlogComment,
        addNewsComment: addNewsComment,
        addBookmarksComment: addBookmarksComment,

        updatePrjComment: updatePrjComment,
        updateWikiComment: updateWikiComment,
        updateBlogComment: updateBlogComment,
        updateNewsComment: updateNewsComment,
        updateBookmarksComment: updateBookmarksComment,

        fckeRemoveCommentComplete: fckeRemoveCommentComplete,
        fckeCancelCommentComplete: fckeCancelCommentComplete,
        fckeEditCommentComplete: fckeEditCommentComplete,
        getShortenLink: getShortenLink,
        updatePortalName: updatePortalName,

        updatePrjSettings: updatePrjSettings,
        getPrjSettings: getPrjSettings,
        getPrjSecurityinfo: getPrjSecurityinfo,
        addPrjEntityFiles: addPrjEntityFiles,
        uploadFilesToPrjEntity: uploadFilesToPrjEntity,
        removePrjEntityFiles: removePrjEntityFiles,
        getPrjEntityFiles: getPrjEntityFiles,
        addPrjSubtask: addPrjSubtask,
        copyPrjSubtask: copyPrjSubtask,
        updatePrjSubtask: updatePrjSubtask,
        updatePrjTask: updatePrjTask,
        updatePrjTasksStatus: updatePrjTasksStatus,
        updatePrjTasksMilestone: updatePrjTasksMilestone,
        removePrjSubtask: removePrjSubtask,
        addPrjTask: addPrjTask,
        copyPrjTask: copyPrjTask,
        getPrjTask: getPrjTask,
        addPrjTaskByMessage: addPrjTaskByMessage,
        getPrjTasks: getPrjTasks,
        getPrjTasksSimpleFilter: getPrjTasksSimpleFilter,
        getPrjTasksById: getPrjTasksById,
        addPrjMilestone: addPrjMilestone,
        updatePrjMilestone: updatePrjMilestone,
        removePrjMilestone: removePrjMilestone,
        removePrjMilestones: removePrjMilestones,
        getPrjMilestone: getPrjMilestone,
        getPrjMilestones: getPrjMilestones,
        addPrjDiscussion: addPrjDiscussion,
        updatePrjDiscussion: updatePrjDiscussion,
        updatePrjDiscussionStatus: updatePrjDiscussionStatus,
        removePrjDiscussion: removePrjDiscussion,
        getPrjDiscussion: getPrjDiscussion,
        getPrjDiscussions: getPrjDiscussions,
        subscribeToPrjDiscussion: subscribeToPrjDiscussion,
        getSubscribesToPrjDiscussion: getSubscribesToPrjDiscussion,

        addPrjProject: addPrjProject,
        updatePrjProject: updatePrjProject,
        updatePrjProjectStatus: updatePrjProjectStatus,
        removePrjProject: removePrjProject,
        followingPrjProject: followingPrjProject,
        getPrjProject: getPrjProject,
        getPrjProjects: getPrjProjects,
        getPrjTaskFiles: getPrjTaskFiles,
        subscribeToPrjTask: subscribeToPrjTask,
        notifyPrjTaskResponsible: notifyPrjTaskResponsible,

        addPrjTaskLink: addPrjTaskLink,
        removePrjTaskLink: removePrjTaskLink,

        getPrjGanttIndex: getPrjGanttIndex,
        setPrjGanttIndex: setPrjGanttIndex,

        getPrjProjectFolder: getPrjProjectFolder,
        getPrjSelfProjects: getPrjSelfProjects,
        getPrjFollowProjects: getPrjFollowProjects,

        getProjectsForCrmContact: getProjectsForCrmContact,
        addProjectForCrmContact: addProjectForCrmContact,
        removeProjectFromCrmContact: removeProjectFromCrmContact,
        getPrjTeam: getPrjTeam,
        updatePrjTeam: updatePrjTeam,
        addPrjProjectTeamPerson: addPrjProjectTeamPerson,
        removePrjProjectTeamPerson: removePrjProjectTeamPerson,
        getPrjProjectTeamPersons: getPrjProjectTeamPersons,
        setTeamSecurity: setTeamSecurity,
        getPrjProjectFiles: getPrjProjectFiles,
        addPrjTime: addPrjTime,
        getPrjTime: getPrjTime,
        getPrjTaskTime: getPrjTaskTime,
        getPrjTimeById: getPrjTimeById,
        getTotalTimeByFilter: getTotalTimeByFilter,
        updatePrjTime: updatePrjTime,
        removePrjTime: removePrjTime,
        changePaymentStatus: changePaymentStatus,
        getPrjTemplates: getPrjTemplates,
        getPrjTemplate: getPrjTemplate,
        updatePrjTemplate: updatePrjTemplate,
        createPrjTemplate: createPrjTemplate,
        removePrjTemplate: removePrjTemplate,
        getPrjActivities: getPrjActivities,
        addPrjImport: addPrjImport,
        getPrjImport: getPrjImport,
        getPrjImportProjects: getPrjImportProjects,
        checkPrjImportQuota: checkPrjImportQuota,
        getPrjReportTemplate: getPrjReportTemplate,
        addPrjReportTemplate: addPrjReportTemplate,
        updatePrjReportTemplate: updatePrjReportTemplate,
        deletePrjReportTemplate: deletePrjReportTemplate,

        createDocUploadFile: createDocUploadFile,
        addDocFile: addDocFile,
        removeDocFile: removeDocFile,
        getDocFile: getDocFile,
        addDocFolder: addDocFolder,
        getDocFolder: getDocFolder,
        createDocUploadSession: createDocUploadSession,
        getFolderPath: getFolderPath,
        getFileSecurityInfo: getFileSecurityInfo,
        generateSharedLink: generateSharedLink,
        copyBatchItems: copyBatchItems,
        getOperationStatuses: getOperationStatuses,
        getPresignedUri: getPresignedUri,
        saveDocServiceUrl: saveDocServiceUrl,

        createCrmUploadFile: createCrmUploadFile,

        getCrmContactInfo: getCrmContactInfo,
        addCrmContactInfo: addCrmContactInfo,
        updateCrmContactInfo: updateCrmContactInfo,
        deleteCrmContactInfo: deleteCrmContactInfo,
        addCrmContactData: addCrmContactData,
        updateCrmContactData: updateCrmContactData,
        addCrmContactTwitter: addCrmContactTwitter,
        addCrmEntityNote: addCrmEntityNote,

        addCrmContact: addCrmContact,
        addCrmCompany: addCrmCompany,
        updateCrmCompany: updateCrmCompany,
        updateCrmContactContactStatus: updateCrmContactContactStatus,
        updateCrmCompanyContactStatus: updateCrmCompanyContactStatus,
        updateCrmPersonContactStatus: updateCrmPersonContactStatus,
        addCrmPerson: addCrmPerson,
        updateCrmPerson: updateCrmPerson,
        removeCrmContact: removeCrmContact,
        mergeCrmContacts: mergeCrmContacts,
        addCrmTag: addCrmTag,
        addCrmContactTagToGroup: addCrmContactTagToGroup,
        deleteCrmContactTagFromGroup: deleteCrmContactTagFromGroup,
        addCrmEntityTag: addCrmEntityTag,
        removeCrmTag: removeCrmTag,
        removeCrmEntityTag: removeCrmEntityTag,
        removeCrmUnusedTag: removeCrmUnusedTag,
        getCrmCustomFields: getCrmCustomFields,
        addCrmCustomField: addCrmCustomField,
        updateCrmCustomField: updateCrmCustomField,
        removeCrmCustomField: removeCrmCustomField,
        reorderCrmCustomFields: reorderCrmCustomFields,
        getCrmDealMilestones: getCrmDealMilestones,
        addCrmDealMilestone: addCrmDealMilestone,
        updateCrmDealMilestone: updateCrmDealMilestone,
        updateCrmDealMilestoneColor: updateCrmDealMilestoneColor,
        removeCrmDealMilestone: removeCrmDealMilestone,
        reorderCrmDealMilestones: reorderCrmDealMilestones,
        addCrmContactStatus: addCrmContactStatus,
        updateCrmContactStatus: updateCrmContactStatus,
        updateCrmContactStatusColor: updateCrmContactStatusColor,
        removeCrmContactStatus: removeCrmContactStatus,

        addCrmContactType: addCrmContactType,
        updateCrmContactType: updateCrmContactType,
        removeCrmContactType: removeCrmContactType,

        getCrmListItem: getCrmListItem,
        addCrmListItem: addCrmListItem,
        updateCrmListItem: updateCrmListItem,
        updateCrmListItemIcon: updateCrmListItemIcon,
        removeCrmListItem: removeCrmListItem,
        reorderCrmListItems: reorderCrmListItems,
        removePrjTask: removePrjTask,
        removePrjTasks: removePrjTasks,
        addCrmTask: addCrmTask,
        addCrmTaskGroup: addCrmTaskGroup,
        getCrmTask: getCrmTask,
        addCrmEntityMember: addCrmEntityMember,
        addCrmContactsForProject: addCrmContactsForProject,
        removeCrmEntityMember: removeCrmEntityMember,
        addCrmDealForContact: addCrmDealForContact,
        removeCrmDealFromContact: removeCrmDealFromContact,
        updateCrmTask: updateCrmTask,
        removeCrmTask: removeCrmTask,

        getCrmCases: getCrmCases,
        getCrmCasesByPrefix: getCrmCasesByPrefix,
        removeCrmCase: removeCrmCase,
        updateCrmCase: updateCrmCase,
        getCrmContacts: getCrmContacts,
        getCrmSimpleContacts: getCrmSimpleContacts,
        getCrmContactsForMail: getCrmContactsForMail,
        getCrmContactsByPrefix: getCrmContactsByPrefix,
        getCrmContact: getCrmContact,
        getCrmTags: getCrmTags,
        getCrmEntityTags: getCrmEntityTags,
        getCrmEntityMembers: getCrmEntityMembers,
        getCrmTasks: getCrmTasks,
        getCrmOpportunity: getCrmOpportunity,
        getCrmOpportunities: getCrmOpportunities,
        getCrmOpportunitiesByContact: getCrmOpportunitiesByContact,
        getCrmOpportunitiesByPrefix: getCrmOpportunitiesByPrefix,
        removeCrmOpportunity: removeCrmOpportunity,
        updateCrmOpportunityMilestone: updateCrmOpportunityMilestone,
        getCrmCurrencyConvertion: getCrmCurrencyConvertion,
        getCrmCurrencySummaryTable: getCrmCurrencySummaryTable,
        updateCrmCurrency: updateCrmCurrency,
        setCrmCurrencyRates: setCrmCurrencyRates,
        addCrmCurrencyRates: addCrmCurrencyRates,
        updateCRMContactStatusSettings: updateCRMContactStatusSettings,
        updateCRMContactTagSettings: updateCRMContactTagSettings,
        updateCRMContactMailToHistorySettings: updateCRMContactMailToHistorySettings,
        updateOrganisationSettingsCompanyName: updateOrganisationSettingsCompanyName,
        updateOrganisationSettingsAddresses: updateOrganisationSettingsAddresses,
        updateOrganisationSettingsLogo: updateOrganisationSettingsLogo,
        getOrganisationSettingsLogo: getOrganisationSettingsLogo,
        updateWebToLeadFormKey: updateWebToLeadFormKey,
        updateCRMSMTPSettings: updateCRMSMTPSettings,
        sendSMTPTestMail: sendSMTPTestMail,
        sendSMTPMailToContacts: sendSMTPMailToContacts,
        getPreviewSMTPMailToContacts: getPreviewSMTPMailToContacts,
        getStatusSMTPMailToContacts: getStatusSMTPMailToContacts,
        cancelSMTPMailToContacts: cancelSMTPMailToContacts,
        addCrmHistoryEvent: addCrmHistoryEvent,
        removeCrmHistoryEvent: removeCrmHistoryEvent,
        getCrmHistoryEvents: getCrmHistoryEvents,
        removeCrmFile: removeCrmFile,
        getCrmFolder: getCrmFolder,
        updateCrmContactRights: updateCrmContactRights,
        updateCrmCaseRights: updateCrmCaseRights,
        updateCrmOpportunityRights: updateCrmOpportunityRights,
        addCrmEntityFiles: addCrmEntityFiles,
        removeCrmEntityFiles: removeCrmEntityFiles,
        getCrmEntityFiles: getCrmEntityFiles,
        getCrmTaskCategories: getCrmTaskCategories,
        getCrmCase: getCrmCase,
        getContactsByContactInfo: getContactsByContactInfo,

        getCrmInvoices: getCrmInvoices,
        getCrmEntityInvoices: getCrmEntityInvoices,
        updateCrmInvoicesStatusBatch: updateCrmInvoicesStatusBatch,
        getCrmInvoiceByNumber: getCrmInvoiceByNumber,
        getCrmInvoiceByNumberExistence: getCrmInvoiceByNumberExistence,

        getCrmInvoiceItems: getCrmInvoiceItems,
        addCrmInvoiceItem: addCrmInvoiceItem,
        updateCrmInvoiceItem: updateCrmInvoiceItem,
        removeCrmInvoiceItem: removeCrmInvoiceItem,

        getCrmInvoiceTaxes: getCrmInvoiceTaxes,
        addCrmInvoiceTax: addCrmInvoiceTax,
        updateCrmInvoiceTax: updateCrmInvoiceTax,
        removeCrmInvoiceTax: removeCrmInvoiceTax,

        getCrmInvoice: getCrmInvoice,
        getCrmInvoiceSample: getCrmInvoiceSample,
        getCrmInvoiceJsonData: getCrmInvoiceJsonData,
        addCrmInvoice: addCrmInvoice,
        updateCrmInvoice: updateCrmInvoice,
        removeCrmInvoice: removeCrmInvoice,
        getInvoicePdfExistingOrCreate: getInvoicePdfExistingOrCreate,
        getInvoiceConverterData: getInvoiceConverterData,

        addCrmInvoiceLine: addCrmInvoiceLine,
        updateCrmInvoiceLine: updateCrmInvoiceLine,
        removeCrmInvoiceLine: removeCrmInvoiceLine,

        getCrmInvoiceSettings: getCrmInvoiceSettings,
        updateCrmInvoiceSettingsName: updateCrmInvoiceSettingsName,
        updateCrmInvoiceSettingsTerms: updateCrmInvoiceSettingsTerms,

        getCrmCurrencyRates: getCrmCurrencyRates,
        getCrmCurrencyRateById: getCrmCurrencyRateById,
        getCrmCurrencyRateByCurrencies: getCrmCurrencyRateByCurrencies,
        addCrmCurrencyRate: addCrmCurrencyRate,
        updateCrmCurrencyRate: updateCrmCurrencyRate,
        removeCrmCurrencyRate: removeCrmCurrencyRate,
        getCrmContactTweets: getCrmContactTweets,
        getCrmContactTwitterProfiles: getCrmContactTwitterProfiles,
        getCrmContactFacebookProfiles: getCrmContactFacebookProfiles,
        removeCrmContactAvatar: removeCrmContactAvatar,
        updateCrmContactAvatar: updateCrmContactAvatar,
        getCrmContactSocialMediaAvatar: getCrmContactSocialMediaAvatar,
        startCrmImportFromCSV: startCrmImportFromCSV,
        getStatusCrmImportFromCSV: getStatusCrmImportFromCSV,
        getCrmImportFromCSVSampleRow: getCrmImportFromCSVSampleRow,
        uploadFakeCrmImportFromCSV: uploadFakeCrmImportFromCSV,
        getStatusExportToCSV: getStatusExportToCSV,
        cancelExportToCSV: cancelExportToCSV,
        startCrmExportToCSV: startCrmExportToCSV,

        addCrmEntityTaskTemplateContainer: addCrmEntityTaskTemplateContainer,
        updateCrmEntityTaskTemplateContainer: updateCrmEntityTaskTemplateContainer,
        removeCrmEntityTaskTemplateContainer: removeCrmEntityTaskTemplateContainer,
        getCrmEntityTaskTemplateContainer: getCrmEntityTaskTemplateContainer,
        getCrmEntityTaskTemplateContainers: getCrmEntityTaskTemplateContainers,
        addCrmEntityTaskTemplate: addCrmEntityTaskTemplate,
        updateCrmEntityTaskTemplate: updateCrmEntityTaskTemplate,
        removeCrmEntityTaskTemplate: removeCrmEntityTaskTemplate,
        getCrmEntityTaskTemplate: getCrmEntityTaskTemplate,
        getCrmEntityTaskTemplates: getCrmEntityTaskTemplates,
        getCrmContactsForProject: getCrmContactsForProject,

        getCrmVoipAvailableNumbers: getCrmVoipAvailableNumbers,
        getCrmVoipExistingNumbers: getCrmVoipExistingNumbers,
        getCrmVoipUnlinkedNumbers: getCrmVoipUnlinkedNumbers,
        getCrmCurrentVoipNumber: getCrmCurrentVoipNumber,
        createCrmVoipNumber: createCrmVoipNumber,
        linkCrmVoipNumber: linkCrmVoipNumber,
        removeCrmVoipNumber: removeCrmVoipNumber,
        updateCrmVoipNumberSettings: updateCrmVoipNumberSettings,
        updateCrmVoipSettings: updateCrmVoipSettings,
        getCrmVoipSettings: getCrmVoipSettings,
        addCrmVoipNumberOperators: addCrmVoipNumberOperators,
        updateCrmVoipOperator: updateCrmVoipOperator,
        removeCrmVoipNumberOperators: removeCrmVoipNumberOperators,
        getCrmVoipNumberOperators: getCrmVoipNumberOperators,
        callVoipNumber: callVoipNumber,
        answerVoipCall: answerVoipCall,
        rejectVoipCall: rejectVoipCall,
        redirectVoipCall: redirectVoipCall,
        saveVoipCall: saveVoipCall,
        saveVoipCallPrice: saveVoipCallPrice,
        getVoipCalls: getVoipCalls,
        getVoipMissedCalls: getVoipMissedCalls,
        getVoipCall: getVoipCall,
        getVoipToken: getVoipToken,
        getVoipUploads: getVoipUploads,
        deleteVoipUploads: deleteVoipUploads,

        getCrmReportFiles: getCrmReportFiles,
        removeCrmReportFile: removeCrmReportFile,
        getCrmReportStatus: getCrmReportStatus,
        terminateCrmReport: terminateCrmReport,
        checkCrmReport: checkCrmReport,
        generateCrmReport: generateCrmReport,

        getMailFilteredMessages: getMailFilteredMessages,
        getMailFolders: getMailFolders,
        getAccounts: getAccounts,
        getMailTags: getMailTags,
        getMailMessage: getMailMessage,
        getNextMailMessageId: getNextMailMessageId,
        getPrevMailMessageId: getPrevMailMessageId,
        getMailConversation: getMailConversation,
        getNextMailConversationId: getNextMailConversationId,
        getPrevMailConversationId: getPrevMailConversationId,
        removeMailFolderMessages: removeMailFolderMessages,
        restoreMailMessages: restoreMailMessages,
        moveMailMessages: moveMailMessages,
        removeMailMessages: removeMailMessages,
        markMailMessages: markMailMessages,
        createMailTag: createMailTag,
        updateMailTag: updateMailTag,
        removeMailTag: removeMailTag,
        setMailTag: setMailTag,
        setMailConversationsTag: setMailConversationsTag,
        unsetMailTag: unsetMailTag,
        unsetMailConversationsTag: unsetMailConversationsTag,
        addMailDocument: addMailDocument,
        removeMailMailbox: removeMailMailbox,
        getMailDefaultMailboxSettings: getMailDefaultMailboxSettings,
        getMailMailbox: getMailMailbox,
        setDefaultAccount: setDefaultAccount,
        createMailMailboxSimple: createMailMailboxSimple,
        createMailMailboxOAuth: createMailMailboxOAuth,
        updateMailMailboxOAuth: updateMailMailboxOAuth,
        createMailMailbox: createMailMailbox,
        updateMailMailbox: updateMailMailbox,
        setMailMailboxState: setMailMailboxState,
        removeMailMessageAttachment: removeMailMessageAttachment,
        sendMailMessage: sendMailMessage,
        saveMailMessage: saveMailMessage,
        searchEmails: searchEmails,
        getMailContacts: getMailContacts,
        getMailContactsByInfo: getMailContactsByInfo,
        createMailContact: createMailContact,
        deleteMailContacts: deleteMailContacts,
        updateMailContact: updateMailContact,
        getMailAlerts: getMailAlerts,
        deleteMailAlert: deleteMailAlert,
        getMailFilteredConversations: getMailFilteredConversations,
        moveMailConversations: moveMailConversations,
        restoreMailConversations: restoreMailConversations,
        removeMailConversations: removeMailConversations,
        markMailConversations: markMailConversations,
        getMailDisplayImagesAddresses: getMailDisplayImagesAddresses,
        createDisplayImagesAddress: createDisplayImagesAddress,
        removeDisplayImagesAddress: removeDisplayImagesAddress,
        linkChainToCrm: linkChainToCrm,
        markChainAsCrmLinked: markChainAsCrmLinked,
        unmarkChainAsCrmLinked: unmarkChainAsCrmLinked,
        exportMessageToCrm: exportMessageToCrm,
        getLinkedCrmEntitiesInfo: getLinkedCrmEntitiesInfo,
        isConversationLinkedWithCrm: isConversationLinkedWithCrm,
        getMailHelpCenterHtml: getMailHelpCenterHtml,
        getMailboxSignature: getMailboxSignature,
        updateMailboxSignature: updateMailboxSignature,
        updateMailboxAutoreply: updateMailboxAutoreply,
        exportAllAttachmentsToMyDocuments: exportAllAttachmentsToMyDocuments,
        exportAllAttachmentsToDocuments: exportAllAttachmentsToDocuments,
        exportAttachmentToMyDocuments: exportAttachmentToMyDocuments,
        exportAttachmentToDocuments: exportAttachmentToDocuments,
        setEMailInFolder: setEMailInFolder,
        getMailServer: getMailServer,
        getMailServerFullInfo: getMailServerFullInfo,
        getMailServerFreeDns: getMailServerFreeDns,
        getMailDomains: getMailDomains,
        getCommonMailDomain: getCommonMailDomain,
        addMailDomain: addMailDomain,
        removeMailDomain: removeMailDomain,
        addMailbox: addMailbox,
        addMyMailbox: addMyMailbox,
        getMailboxes: getMailboxes,
        removeMailbox: removeMailbox,
        addMailBoxAlias: addMailBoxAlias,
        updateMailbox: updateMailbox,
        removeMailBoxAlias: removeMailBoxAlias,
        addMailGroup: addMailGroup,
        addMailGroupAddress: addMailGroupAddress,
        removeMailGroupAddress: removeMailGroupAddress,
        getMailGroups: getMailGroups,
        removeMailGroup: removeMailGroup,
        isDomainExists: isDomainExists,
        checkDomainOwnership: checkDomainOwnership,
        getDomainDnsSettings: getDomainDnsSettings,
        createNotificationAddress: createNotificationAddress,
        removeNotificationAddress: removeNotificationAddress,
        addCalendarBody: addCalendarBody,
        setMailConversationEnabledFlag: setMailConversationEnabledFlag,
        setMailAlwaysDisplayImagesFlag: setMailAlwaysDisplayImagesFlag,
        setMailCacheUnreadMessagesFlag: setMailCacheUnreadMessagesFlag,
        setMailEnableGoNextAfterMove: setMailEnableGoNextAfterMove,
        getMailServerInfo: getMailServerInfo,
        connectMailServerInfo: connectMailServerInfo,
        saveMailServerInfo: saveMailServerInfo,
        getMailOperationStatus: getMailOperationStatus,

        getWebItemSecurityInfo: getWebItemSecurityInfo,
        setWebItemSecurity: setWebItemSecurity,
        setAccessToWebItems: setAccessToWebItems,
        setProductAdministrator: setProductAdministrator,
        isProductAdministrator: isProductAdministrator,
        getPortalSettings: getPortalSettings,
        getPortalLogo: getPortalLogo,

        getAuditEvents: getAuditEvents,
        getLoginEvents: getLoginEvents,
        createLoginHistoryReport: createLoginHistoryReport,
        createAuditTrailReport: createAuditTrailReport,

        getAuditSettings: getAuditSettings,
        setAuditSettings: setAuditSettings,

        getIpRestrictions: getIpRestrictions,
        saveIpRestrictions: saveIpRestrictions,
        updateIpRestrictionsSettings: updateIpRestrictionsSettings,
        updateTipsSettings: updateTipsSettings,
        updateTipsSubscription: updateTipsSubscription,
        smsValidationSettings: smsValidationSettings,
        closeWelcomePopup: closeWelcomePopup,
        setColorTheme: setColorTheme,
        setTimaAndLanguage: setTimaAndLanguage,
        setDefaultpage: setDefaultpage,

        getTalkUnreadMessages: getTalkUnreadMessages,

        registerUserOnPersonal: registerUserOnPersonal,

        saveWhiteLabelSettings: saveWhiteLabelSettings,
        restoreWhiteLabelSettings: restoreWhiteLabelSettings,

        getCustomNavigationItems: getCustomNavigationItems,
        getCustomNavigationItemSample: getCustomNavigationItemSample,
        getCustomNavigationItem: getCustomNavigationItem,
        createCustomNavigationItem: createCustomNavigationItem,
        deleteCustomNavigationItem: deleteCustomNavigationItem,

        getCalendars: getCalendars,
        getCalendarEventByUid: getCalendarEventByUid,
        getCalendarEventById: getCalendarEventById,
        importCalendarEventIcs: importCalendarEventIcs,

        saveLdapSettings: saveLdapSettings,
        getLdapSettings: getLdapSettings,
        getLdapDefaultSettings: getLdapDefaultSettings,
        getLdapStatus: getLdapStatus,
        syncLdap: syncLdap,
        updateEmailActivationSettings: updateEmailActivationSettings,

        getBarPromotions: getBarPromotions,
        markBarPromotion: markBarPromotion,
        getBarTips: getBarTips,
        markBarTip: markBarTip,
        deleteBarTips: deleteBarTips,

        getReassignProgress: getReassignProgress,
        terminateReassign: terminateReassign,
        startReassign: startReassign,

        getRemoveProgress: getRemoveProgress,
        terminateRemove: terminateRemove,
        startRemove: startRemove
    };
})();