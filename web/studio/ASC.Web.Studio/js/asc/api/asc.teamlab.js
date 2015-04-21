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


;
window.Teamlab = (function() {
    var isInit = false,
        eventManager = null;

    var customEvents = {
        getException: 'ongetexception',
        getAuthentication: 'ongetauthentication',

        addComment: 'onaddcomment',
        updateComment: 'onupdatecomment',
        removeComment: 'onremovecomment',

        getCmtBlog: 'ongetcmtyblog',

        subscribeProject: 'subscribeproject',
        addPrjComment: 'onaddprjcomment',
        updatePrjComment: 'onupdateprjcomment',
        removePrjComment: 'onremoveprjcomment',
        updatePrjTask: 'onupdateprjtask',
        addPrjTask: 'onaddprjtask',
        getPrjTask: 'ongetprjtask',
        getPrjTasks: 'ongetprjtasks',
        addPrjSubtask: 'onaddprjsubtask',
        removePrjSubtask: 'onremoveprjsubtask',
        updatePrjSubtask: 'onupdateprjsubtask',
        removePrjTask: 'onremoveprjtask',
        getPrjDiscussion: 'ongetprjdiscussion',
        getPrjDiscussions: 'ongetprjdiscussions',
        getPrjProjects: 'ongetprjprojects',
        getPrjTeam: 'ongetprjteam',
        setTeamSecurity: 'setteamsecurity',
        getPrjTemplates: 'getprjtemplates',
        getPrjTemplate: 'getprjtemplate',
        updatePrjTemplate: 'updateprjtemplate',
        createPrjTemplate: 'createprjtemplate',
        removePrjTemplate: 'removeprjtemplate',
        getPrjMilestones: 'ongetprjmilestones',
        updatePrjProjectStatus: 'onupdateprjprojectstatus',
        addPrjTime: 'onaddprjtime',
        getPrjTime: 'ongetprjtime',
        updatePrjTime: 'onupdateprjtime',
        removePrjTime: 'onremoveprjtime',

        getCrmContact: 'ongetcrmcontact',
        getCrmOpportunity: 'ongetcrmopportunity',
        getCrmCase: 'ongetcrmcase',
        getCrmContactsByPrefix: 'ongetcrmcontactbyprefix',
        getCrmOpportunitiesByPrefix: 'ongetcrmopportunitybyprefix',
        getCrmCasesByPrefix: 'ongetcrmcasebyprefix',
        getContactsByContactInfo: 'ongetcontactsbycontactinfo',
        getCrmContactTweets: 'ongetcrmcontacttweets',
        updateWebToLeadFormKey: 'onupdatewebtoleadformkey',
        updateCRMSMTPSettings: 'onupdatesmtpsettings',
        sendSMTPTestMail: 'onsendsmtptestmail',
        sendSMTPMailToContacts: 'onsendsmtpmailtocontacts',
        getPreviewSMTPMailToContacts: 'ongetpreviewsmtpmailtocontacts',
        getStatusSMTPMailToContacts: 'ongetstatussmtpmailtocontacts',
        cancelSMTPMailToContacts: 'oncancelsmtpmailtocontacts',
        getStatusExportToCSV: 'ongetstatusexporttocsv',
        cancelExportToCSV: 'oncancelexporttocsv',
        startCrmExportToCSV: 'onstartcrmexporttocsv',
        getCrmContactInCruchBase: 'ongetcrmcontactincruchbase',
        startCrmImportFromCSV: 'onstartcrmimportfromcsv',
        getStatusCrmImportFromCSV: 'ongetstatuscrmimportfromcsv',
        getCrmImportFromCSVSampleRow: 'ongetcrmimportfromcsvsamplerow',
        uploadFakeCrmImportFromCSV: 'onuploadfakecrmimportfromcsv',

        getMailFilteredMessages: 'ongetmailmessages',
        getMailFolders: 'ongetmailfolders',
        getMailMessagesModifyDate: 'ongetmailmessagesmodifydate',
        getMailFolderModifyDate: 'ongetmailfoldermodifydate',
        getAccounts: 'ongetmailaccounts',
        getMailTags: 'ongetmailtags',
        getMailConversation: 'ongetmailconversation',
        getNextMailConversationId: 'ongetnextmailconversationid',
        getPrevMailConversationId: 'ongetprevmailconversationid',
        getMailMessage: 'ongetmailmessage',
        getNextMailMessageId: 'ongetnextmailmessageid',
        getPrevMailMessageId: 'ongetprevmailmessageid',
        getMailMessageTemplate: 'ongetmailmessagetemplate',
        getMailRandomGuid: 'ongetmailrandomguid',
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
        createMailMailbox: 'oncreatemailmailbox',
        updateMailMailbox: 'onupdatemailmailbox',
        setMailMailboxState: 'onsetmailmailboxstate',
        removeMailMessageAttachment: 'onremovemailmessageattachment',
        sendMailMessage: 'onsendmailmessage',
        saveMailMessage: 'onsavemailmessage',
        getMailContacts: 'ongetmailcontacts',
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
        isConversationLinkedWithCrm: "oncompleteconversationcheckforlinkingwithcrm",
        exportMessageToCrm: "onexportmessagetocrm",
        getMailboxSignature: "ongetmailboxsignature",
        updateMailboxSignature: "onupdatemailboxsignature",
        exportAllAttachmentsToMyDocuments: 'exportallattachmentstomydocuments',
        exportAttachmentToMyDocuments: 'exportattachmenttomydocuments',
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
        removeMailBoxAlias: 'removemailboxalias',
        addMailGroup: 'addmailgroup',
        addMailGroupAddress: 'addmailgroupaddress',
        getMailGroups: 'getmailgroups',
        removeMailGroup: 'removemailgroup',
        removeMailGroupAddress: 'removemailgroupaddress',
        isDomainExists: 'isdomainexists',
        checkDomainOwnership: 'checkdomainownership',
        getDomainDnsSettings: 'getdomaindnssettings',

        getFolderPath: 'ongetfolderpath',

        getTalkUnreadMessages: "gettalkunreadmessages",
        registerUserOnPersonal: "registeruseronpersonal",
    },
        customEventsHash = {},
        eventManager = new CustomEvent(customEvents);
    extendCustomEventsHash();

    function isArray(o) {
        return o ? o.constructor.toString().indexOf("Array") != -1 : false;
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

        ServiceManager.bind(null, onGetResponse);
        ServiceManager.bind('event', onGetEvent);
        ServiceManager.bind('extention', onGetExtention);
        //ServiceManager.bind('me', onGetOwnProfile);
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

    var extendEventManager = function(events) {
        for (var fld in events) {
            if (events.hasOwnProperty(fld)) {
                customEvents[fld] = events[fld];
            }
        }
        eventManager.extend(customEvents);
        extendCustomEventsHash();
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

    function onGetOwnProfile(params, profile) {
        //console.log('me: ', profile);
    }

    var joint = function() {
        ServiceManager.joint();
        return window.Teamlab;
    };

    var start = function(params, options) {
        return ServiceManager.start(params, options);
    };

    /* <common> */
    var getQuotas = function(params, options) {
        return returnValue(ServiceManager.getQuotas(customEvents.getQuotas, params, options));
    };
    /* </Common> */

    /* <people> */
    var addProfile = function(params, data, options) {
        return returnValue(ServiceManager.addProfile(customEvents.addProfile, params, data, options));
    };

    var getProfile = function(params, id, options) {
        return returnValue(ServiceManager.getProfile(customEvents.getProfile, params, id, options));
    };

    var getProfiles = function(params, options) {
        return returnValue(ServiceManager.getProfiles(customEvents.getProfiles, params, options));
    };

    var getProfilesByFilter = function(params, options) {
        return returnValue(ServiceManager.getProfilesByFilter(customEvents.getProfilesByFilter, params, options));
    };

    var addGroup = function(params, data, options) {
        return returnValue(ServiceManager.addGroup(customEvents.addGroup, params, data, options));
    };

    var updateGroup = function(params, id, data, options) {
        return returnValue(ServiceManager.updateGroup(customEvents.updateGroup, params, id, data, options));
    };

    var getGroup = function(id, options) {
        return returnValue(ServiceManager.getGroup(customEvents.getGroup, null, id, options));
    };

    var getGroups = function(params, options) {
        return returnValue(ServiceManager.getGroups(customEvents.getGroups, params, options));
    };

    var deleteGroup = function(id, options) {
        return returnValue(ServiceManager.deleteGroup(customEvents.deleteGroup, null, id, options));
    };

    var updateProfile = function(params, id, data, options) {
        return returnValue(ServiceManager.updateProfile(customEvents.updateProfile, params, id, data, options));
    };

    var updateUserType = function(params, type, data, options) {
        return returnValue(ServiceManager.updateUserType(customEvents.updateUserType, params, type, data, options));
    };

    var updateUserStatus = function(params, status, data, options) {
        return returnValue(ServiceManager.updateUserStatus(customEvents.updateUserStatus, params, status, data, options));
    };
    var updateUserPhoto = function(params, id, data, options) {
        return returnValue(ServiceManager.updateUserPhoto(customEvents.updateUserPhoto, params, id, data, options));
    };

    var removeUserPhoto = function(params, id, data, options) {
        return returnValue(ServiceManager.removeUserPhoto(customEvents.removeUserPhoto, params, id, data, options));
    };

    var sendInvite = function(params, data, options) {
        return returnValue(ServiceManager.sendInvite(customEvents.sendInvite, params, data, options));
    };

    var removeUsers = function(params, data, options) {
        return returnValue(ServiceManager.removeUsers(customEvents.removeUsers, params, data, options));
    };

    var getUserGroups = function (id, options) {
        return returnValue(ServiceManager.getUserGroups(customEvents.getUserGroups, null, id, options));
    };
    /* </people> */

    /* <community> */
    var addCmtBlog = function(params, data, options) {
        return returnValue(ServiceManager.addCmtBlog(customEvents.addCmtBlog, params, data, options));
    };

    var getCmtBlog = function(params, id, options) {
        return returnValue(ServiceManager.getCmtBlog(customEvents.getCmtBlog, params, id, options));
    };

    var getCmtBlogs = function(params, options) {
        return returnValue(ServiceManager.getCmtBlogs(customEvents.getCmtBlogs, params, options));
    };

    var addCmtForumTopic = function(params, threadid, data, options) {
        if (arguments.length === 3) {
            options = arguments[2];
            data = arguments[1];
            threadid = data && typeof data === 'object' && data.hasOwnProperty('threadid') ? data.threadid : null;
        }

        return returnValue(ServiceManager.addCmtForumTopic(customEvents.addCmtForumTopic, params, threadid, data, options));
    };

    var getCmtForumTopic = function(params, id, options) {
        return returnValue(ServiceManager.getCmtForumTopic(customEvents.getCmtForumTopic, params, id, options));
    };

    var getCmtForumTopics = function(params, options) {
        return returnValue(ServiceManager.getCmtForumTopics(customEvents.getCmtForumTopics, params, options));
    };

    var getCmtForumCategories = function(params, options) {
        return returnValue(ServiceManager.getCmtForumCategories(customEvents.getCmtForumCategories, params, options));
    };

    var addCmtForumToCategory = function(params, data, options) {
        return returnValue(ServiceManager.addCmtForumToCategory(customEvents.addCmtForumToCategory, params, data, options));
    };

    var addCmtEvent = function(params, data, options) {
        return returnValue(ServiceManager.addCmtEvent(customEvents.addCmtEvent, params, data, options));
    };

    var getCmtEvent = function(params, id, options) {
        return returnValue(ServiceManager.getCmtEvent(customEvents.getCmtEvent, params, id, options));
    };

    var getCmtEvents = function(params, options) {
        return returnValue(ServiceManager.getCmtEvents(customEvents.getCmtEvents, params, options));
    };

    var addCmtBookmark = function(params, data, options) {
        return returnValue(ServiceManager.addCmtBookmark(customEvents.addCmtBookmark, params, data, options));
    };

    var getCmtBookmark = function(params, id, options) {
        return returnValue(ServiceManager.getCmtBookmark(customEvents.getCmtBookmark, params, id, options));
    };

    var getCmtBookmarks = function(params, options) {
        return returnValue(ServiceManager.getCmtBookmarks(customEvents.getCmtBookmarks, params, options));
    };

    var addCmtForumTopicPost = function(params, id, data, options) {
        return returnValue(ServiceManager.addCmtForumTopicPost(customEvents.addCmtForumTopicPost, params, id, data, options));
    };

    var addCmtBlogComment = function(params, id, data, options) {
        return returnValue(ServiceManager.addCmtBlogComment(customEvents.addCmtBlogComment, params, id, data, options));
    };

    var getCmtBlogComments = function(params, id, options) {
        return returnValue(ServiceManager.getCmtBlogComments(customEvents.getCmtBlogComments, params, id, options));
    };

    var addCmtEventComment = function(params, id, data, options) {
        return returnValue(ServiceManager.addCmtEventComment(customEvents.addCmtEventComment, params, id, data, options));
    };

    var getCmtEventComments = function(params, id, options) {
        return returnValue(ServiceManager.getCmtEventComments(customEvents.getCmtEventComments, params, id, options));
    };

    var addCmtBookmarkComment = function(params, id, data, options) {
        return returnValue(ServiceManager.addCmtBookmarkComment(customEvents.addCmtBookmarkComment, params, id, data, options));
    };

    var getCmtBookmarkComments = function(params, id, options) {
        return returnValue(ServiceManager.getCmtBookmarkComments(customEvents.getCmtBookmarkComments, params, id, options));
    };
    /* </community> */

    /* <projects> */

    var subscribeProject = function(params, id, options) {
        return returnValue(ServiceManager.subscribeProject(customEvents.subscribeProject, params, id, options));
    };

    var getFeeds = function(params, options) {
        return returnValue(ServiceManager.getFeeds(customEvents.getFeeds, params, options));
    };

    var getNewFeedsCount = function(params, options) {
        return returnValue(ServiceManager.getNewFeedsCount(customEvents.getNewFeedsCount, params, options));
    };

    var readFeeds = function(params, options) {
        return returnValue(ServiceManager.readFeeds(customEvents.readFeeds, params, options));
    };

    var getPrjTags = function(params, options) {
        return returnValue(ServiceManager.getPrjTags(customEvents.getPrjTags, params, options));
    };

    var getPrjTagsByName = function(params, name, data, options) {
        return returnValue(ServiceManager.getPrjTagsByName(customEvents.getPrjTagsByName, params, name, data, options));
    };

    var addPrjComment = function(params, type, id, data, options) {
        var fn = null;
        switch (type.toLowerCase()) {
            case 'discussion':
                fn = ServiceManager.addPrjDiscussionComment;
                break;
        }
        if (typeof fn === 'function') {
            return returnValue(fn(customEvents.addPrjComment, params, id, data, options));
        }
        return false;
    };

    var updatePrjComment = function(params, type, id, data, options) {
        var fn = null;
        switch (type.toLowerCase()) {
            case 'discussion':
                fn = ServiceManager.updatePrjDiscussionComment;
                break;
        }
        if (typeof fn === 'function') {
            return returnValue(fn(customEvents.updatePrjComment, params, id, data, options));
        }
        return false;
    };

    var removePrjComment = function(params, type, id, options) {
        var fn = null;
        switch (type.toLowerCase()) {
            case 'discussion':
                fn = ServiceManager.removePrjDiscussionComment;
                break;
        }
        if (typeof fn === 'function') {
            return returnValue(fn(customEvents.removePrjComment, params, id, options));
        }
        return false;
    };

    var getPrjComments = function(params, type, id, options) {
        var fn = null;
        switch (type.toLowerCase()) {
            case 'discussion':
                fn = ServiceManager.getPrjDiscussionComments;
                break;
        }
        if (typeof fn === 'function') {
            return returnValue(fn(customEvents.getPrjComments, params, id, options));
        }
        return false;
    };

    var addPrjTaskComment = function(params, id, data, options) {
        return returnValue(ServiceManager.addPrjTaskComment(customEvents.addPrjTaskComment, params, id, data, options));
    };

    var updatePrjTaskComment = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjTaskComment(customEvents.updatePrjTaskComment, params, id, data, options));
    };

    var removePrjTaskComment = function(params, id, options) {
        return returnValue(ServiceManager.removePrjTaskComment(customEvents.removePrjTaskComment, params, id, options));
    };

    var getPrjTaskComments = function(params, id, options) {
        return returnValue(ServiceManager.getPrjTaskComments(customEvents.getPrjTaskComments, params, id, options));
    };

    var addPrjDiscussionComment = function(params, id, data, options) {
        return returnValue(ServiceManager.addPrjDiscussionComment(customEvents.addPrjDiscussionComment, params, id, data, options));
    };

    var updatePrjDiscussionComment = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjDiscussionComment(customEvents.updatePrjDiscussionComment, params, id, data, options));
    };

    var removePrjDiscussionComment = function(params, id, options) {
        return returnValue(ServiceManager.removePrjDiscussionComment(customEvents.removePrjDiscussionComment, params, id, options));
    };

    var getPrjDiscussionComments = function(params, id, options) {
        return returnValue(ServiceManager.getPrjDiscussionComments(customEvents.getPrjDiscussionComments, params, id, options));
    };

    var addPrjSubtask = function(params, taskid, data, options) {
        return returnValue(ServiceManager.addPrjSubtask(customEvents.addPrjSubtask, params, taskid, data, options));
    };

    var updatePrjSubtask = function(params, taskid, subtaskid, data, options) {
        return returnValue(ServiceManager.updatePrjSubtask(customEvents.updatePrjSubtask, params, taskid, subtaskid, data, options));
    };

    var removePrjSubtask = function(params, taskid, subtaskid, options) {
        return returnValue(ServiceManager.removePrjSubtask(customEvents.removePrjSubtask, params, taskid, subtaskid, options));
    };

    var addPrjTask = function(params, projectid, data, options) {
        return returnValue(ServiceManager.addPrjTask(customEvents.addPrjTask, params, projectid, data, options));
    };

    var addPrjTaskByMessage = function(params, projectId, massegeId, options) {
        return returnValue(ServiceManager.addPrjTaskByMessage(customEvents.addPrjTaskByMessage, params, projectId, massegeId, options));
    };

    var updatePrjTask = function(params, taskid, data, options) {
        return returnValue(ServiceManager.updatePrjTask(customEvents.updatePrjTask, params, taskid, data, options));
    };

    var removePrjTask = function(params, id, options) {
        return returnValue(ServiceManager.removePrjTask(customEvents.removePrjTask, params, id, options));
    };

    var getPrjTask = function(params, id, options) {
        return returnValue(ServiceManager.getPrjTask(customEvents.getPrjTask, params, id, options));
    };

    var getPrjTasksById = function(params, ids, options) {
        return returnValue(ServiceManager.getPrjTasksById(customEvents.getPrjTasksById, params, ids, options));
    };

    var getPrjTasks = function(params, projectid, type, status, options) {
        return returnValue(ServiceManager.getPrjTasks(customEvents.getPrjTasks, params, projectid, type, status, options));
    };

    var getPrjTasksSimpleFilter = function(params, options) {
        return returnValue(ServiceManager.getPrjTasksSimpleFilter(customEvents.getPrjTasksSimpleFilter, params, options));
    };

    var getPrjTeam = function(params, ids, options) {
        return returnValue(ServiceManager.getPrjTeam(customEvents.getPrjTeam, params, ids, options));
    };

    var updatePrjTeam = function(params, projectid, data, options) {
        return returnValue(ServiceManager.updatePrjTeam(customEvents.updatePrjTeam, params, projectid, data, options));
    };

    var setTeamSecurity = function(params, projectid, data, options) {
        return returnValue(ServiceManager.setTeamSecurity(customEvents.setTeamSecurity, params, projectid, data, options));
    };

    var getPrjTaskFiles = function(params, taskid, options) {
        return returnValue(ServiceManager.getPrjTaskFiles(customEvents.getPrjTaskFiles, params, taskid, options));
    };

    var subscribeToPrjTask = function(params, taskid, options) {
        return returnValue(ServiceManager.subscribeToPrjTask(customEvents.subscribeToPrjTask, params, taskid, options));
    };

    var notifyPrjTaskResponsible = function(params, taskid, options) {
        return returnValue(ServiceManager.notifyPrjTaskResponsible(customEvents.notifyPrjTaskResponsible, params, taskid, options));
    };

    var addPrjTaskLink = function(params, taskid, data, options) {
        return returnValue(ServiceManager.addPrjTaskLink(customEvents.addPrjTaskLink, params, taskid, data, options));
    };

    var removePrjTaskLink = function(params, taskid, data, options) {
        return returnValue(ServiceManager.removePrjTaskLink(customEvents.removePrjTaskLink, params, taskid, data, options));
    };

    var getPrjProjectFolder = function(params, taskid, options) {
        return returnValue(ServiceManager.getPrjProjectFolder(customEvents.getPrjProjectFolder, params, taskid, options));
    };

    var addPrjEntityFiles = function(params, entityid, entitytype, data, options) {
        return returnValue(ServiceManager.addPrjEntityFiles(customEvents.addPrjEntityFiles, params, entityid, entitytype, data, options));
    };

    var removePrjEntityFiles = function(params, entityid, entitytype, data, options) {
        return returnValue(ServiceManager.removePrjEntityFiles(customEvents.removePrjEntityFiles, params, entityid, entitytype, data, options));
    };

    var getPrjEntityFiles = function(params, entityid, entitytype, options) {
        return returnValue(ServiceManager.getPrjEntityFiles(customEvents.getPrjEntityFiles, params, entityid, entitytype, options));
    };

    var addPrjMilestone = function(params, projectid, data, options) {
        return returnValue(ServiceManager.addPrjMilestone(customEvents.addPrjMilestone, params, projectid, data, options));
    };

    var updatePrjMilestone = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjMilestone(customEvents.updatePrjMilestone, params, id, data, options));
    };

    var removePrjMilestone = function(params, id, options) {
        return returnValue(ServiceManager.removePrjMilestone(customEvents.removePrjMilestone, params, id, options));
    };

    var getPrjMilestone = function(params, id, options) {
        return returnValue(ServiceManager.getPrjMilestone(customEvents.getPrjMilestone, params, id, options));
    };

    var getPrjMilestones = function(params, projectid, options) {
        if (arguments.length < 3) {
            options = arguments[1];
            projectid = null;
        }

        return returnValue(ServiceManager.getPrjMilestones(customEvents.getPrjMilestones, params, projectid, options));
    };

    var addPrjDiscussion = function(params, projectid, data, options) {
        return returnValue(ServiceManager.addPrjDiscussion(customEvents.addPrjDiscussion, params, projectid, data, options));
    };

    var updatePrjDiscussion = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjDiscussion(customEvents.updatePrjDiscussion, params, id, data, options));
    };
    
    var updatePrjDiscussionStatus = function (params, id, data, options) {
        return returnValue(ServiceManager.updatePrjDiscussionStatus(customEvents.updatePrjDiscussionStatus, params, id, data, options));
    };

    var removePrjDiscussion = function(params, id, options) {
        return returnValue(ServiceManager.removePrjDiscussion(customEvents.removePrjDiscussion, params, id, options));
    };

    var getPrjDiscussion = function(params, id, options) {
        return returnValue(ServiceManager.getPrjDiscussion(customEvents.getPrjDiscussion, params, id, options));
    };

    var getPrjDiscussions = function(params, projectid, options) {
        if (arguments.length < 3) {
            options = arguments[1];
            projectid = null;
        }

        return returnValue(ServiceManager.getPrjDiscussions(customEvents.getPrjDiscussions, params, projectid, options));
    };

    var subscribeToPrjDiscussion = function(params, taskid, options) {
        return returnValue(ServiceManager.subscribeToPrjDiscussion(customEvents.subscribeToPrjDiscussion, params, taskid, options));
    };
    
    var getSubscribesToPrjDiscussion = function (params, messageid, options) {
        return returnValue(ServiceManager.getSubscribesToPrjDiscussion(customEvents.getSubscribesToPrjDiscussion, params, messageid, options));
    };

    var addPrjProject = function(params, data, options) {
        return returnValue(ServiceManager.addPrjProject(customEvents.addPrjProject, params, data, options));
    };

    var updatePrjProject = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjProject(customEvents.updatePrjProject, params, id, data, options));
    };

    var updatePrjProjectStatus = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjProjectStatus(customEvents.updatePrjProjectStatus, params, id, data, options));
    };

    var removePrjProject = function(params, id, options) {
        return returnValue(ServiceManager.removePrjProject(customEvents.removePrjProject, params, id, options));
    };

    var followingPrjProject = function(params, projectid, data, options) {
        return returnValue(ServiceManager.followingPrjProject(customEvents.followingPrjProject, params, projectid, data, options));
    };

    var getPrjProject = function(params, id, options) {
        return returnValue(ServiceManager.getPrjProject(customEvents.getPrjProject, params, id, options));
    };

    var getPrjProjects = function(params, options) {
        return returnValue(ServiceManager.getPrjProjects(customEvents.getPrjProjects, params, options));
    };

    var getPrjSelfProjects = function(params, options) {
        return returnValue(ServiceManager.getPrjSelfProjects(customEvents.getPrjProjects, params, options));
    };

    var getPrjFollowProjects = function(params, options) {
        return returnValue(ServiceManager.getPrjFollowProjects(customEvents.getPrjProjects, params, options));
    };

    var getProjectsForCrmContact = function(params, contactid, options) {
        return returnValue(ServiceManager.getProjectsForCrmContact(customEvents.getProjectsForCrmContact, params, contactid, options));
    };

    var addProjectForCrmContact = function(params, projectid, data, options) {
        return returnValue(ServiceManager.addProjectForCrmContact(customEvents.addProjectForCrmContact, params, projectid, data, options));
    };

    var removeProjectFromCrmContact = function(params, projectid, data, options) {
        return returnValue(ServiceManager.removeProjectFromCrmContact(customEvents.removeProjectFromCrmContact, params, projectid, data, options));
    };

    var addPrjProjectTeamPerson = function(params, projectid, data, options) {
        return returnValue(ServiceManager.addPrjProjectTeamPerson(customEvents.addPrjProjectTeamPerson, params, projectid, data, options));
    };

    var removePrjProjectTeamPerson = function(params, projectid, data, options) {
        return returnValue(ServiceManager.removePrjProjectTeamPerson(customEvents.removePrjProjectTeamPerson, params, projectid, data, options));
    };

    var getPrjProjectTeamPersons = function(params, projectid, options) {
        return returnValue(ServiceManager.getPrjProjectTeamPersons(customEvents.getPrjProjectTeamPersons, params, projectid, options));
    };

    var getPrjProjectFiles = function(params, projectid, options) {
        return returnValue(ServiceManager.getPrjProjectFiles(customEvents.getPrjProjectFiles, params, projectid, options));
    };

    var addPrjTime = function(params, taskid, data, options) {
        return returnValue(ServiceManager.addPrjTime(customEvents.addPrjTime, params, taskid, data, options));
    };

    var getPrjTime = function(params, taskid, options) {
        return returnValue(ServiceManager.getPrjTime(customEvents.getPrjTime, params, taskid, options));
    };

    var getTotalTimeByFilter = function(params, options) {
        return returnValue(ServiceManager.getTotalTaskTimeByFilter(customEvents.getTotalTaskTimeByFilter, params, options));
    };

    var updatePrjTime = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjTime(customEvents.updatePrjTime, params, id, data, options));
    };

    var changePaymentStatus = function(params, id, data, options) {
        return returnValue(ServiceManager.changePaymentStatus(customEvents.changePaymentStatus, params, id, data, options));
    };

    var removePrjTime = function(params, id, options) {
        return returnValue(ServiceManager.removePrjTime(customEvents.removePrjTime, params, id, options));
    };

    var getPrjTemplates = function(params, options) {
        return returnValue(ServiceManager.getPrjTemplates(customEvents.getPrjTemplates, params, options));
    };

    var getPrjTemplate = function(params, id, options) {
        return returnValue(ServiceManager.getPrjTemplate(customEvents.getPrjTemplate, params, id, options));
    };

    var updatePrjTemplate = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjTemplate(customEvents.updatePrjTemplate, params, id, data, options));
    };

    var createPrjTemplate = function(params, data, options) {
        return returnValue(ServiceManager.createPrjTemplate(customEvents.createPrjTemplate, params, data, options));
    };

    var removePrjTemplate = function(params, id, options) {
        return returnValue(ServiceManager.removePrjTemplate(customEvents.removePrjTemplate, params, id, options));
    };

    var getPrjActivities = function(params, options) {
        return returnValue(ServiceManager.getPrjActivities(customEvents.getPrjActivities, params, options));
    };

    var addPrjImport = function(params, data, options) {
        return returnValue(ServiceManager.addPrjImport(customEvents.addPrjImport, params, data, options));
    };

    var getPrjImport = function(params, options) {
        return returnValue(ServiceManager.getPrjImport(customEvents.getPrjImport, params, options));
    };

    var getPrjImportProjects = function(params, data, options) {
        return returnValue(ServiceManager.getPrjImportProjects(customEvents.getPrjImportProjects, params, data, options));
    };

    var checkPrjImportQuota = function(params, data, options) {
        return returnValue(ServiceManager.checkPrjImportQuota(customEvents.checkPrjImportQuota, params, data, options));
    };

    var addPrjReportTemplate = function(params, data, options) {
        return returnValue(ServiceManager.addPrjReportTemplate(customEvents.addPrjReportTemplate, params, data, options));
    };

    var updatePrjReportTemplate = function(params, id, data, options) {
        return returnValue(ServiceManager.updatePrjReportTemplate(customEvents.updatePrjReportTemplate, params, id, data, options));
    };

    var deletePrjReportTemplate = function(params, id, options) {
        return returnValue(ServiceManager.deletePrjReportTemplate(customEvents.deletePrjReportTemplate, params, id, options));
    };

    var uploadFilesToPrjEntity = function(params, entityId, data, options) {
        return returnValue(ServiceManager.uploadFilesToPrjEntity(customEvents.uploadFilesToPrjEntity, params, entityId, data, options));
    };

    var getPrjGanttIndex = function(params, id, options) {
        return returnValue(ServiceManager.getPrjGanttIndex(customEvents.getPrjGanttIndex, params, id, options));
    };

    var setPrjGanttIndex = function(params, id, data, options) {
        return returnValue(ServiceManager.setPrjGanttIndex(customEvents.setPrjGanttIndex, params, id, data, options));
    };
    /* </projects> */

    /* <documents> */
    var createDocUploadFile = function(params, id, data, options) {
        return returnValue(ServiceManager.createDocUploadFile(customEvents.uploadDocFile, params, id, data, options));
    };

    var addDocFile = function(params, id, type, data, options) {
        if (arguments.length < 5) {
            options = arguments[3];
            data = arguments[3];
            type = null;
        }

        return returnValue(ServiceManager.addDocFile(customEvents.addDocFile, params, id, type, data, options));
    };

    var getDocFile = function(params, id, options) {
        return returnValue(ServiceManager.getDocFile(customEvents.getDocFile, params, id, options));
    };

    var addDocFolder = function(params, id, data, options) {
        return returnValue(ServiceManager.addDocFolder(customEvents.addDocFolder, params, id, data, options));
    };

    var getDocFolder = function(params, folderid, options) {
        return returnValue(ServiceManager.getDocFolder(customEvents.getDocFolder, params, folderid, options));
    };

    var removeDocFile = function(params, fileId, options) {
        return returnValue(ServiceManager.removeDocFile(customEvents.removeDocFile, params, fileId, options));
    };

    var createDocUploadSession = function(params, folderId, data, options) {
        return returnValue(ServiceManager.createDocUploadSession(customEvents.createDocUploadSession, params, folderId, data, options));
    };

    var getFolderPath = function (folderId, options) {
        return returnValue(ServiceManager.getFolderPath(customEvents.getFolderPath, folderId, options));
    };

    var getFileSecurityInfo = function(fileId, options) {
        return returnValue(ServiceManager.getFileSecurityInfo(customEvents.getFileSecurityInfo, fileId, options));
    };

    var generateSharedLink = function(fileId, data, options) {
        return returnValue(ServiceManager.generateSharedLink(customEvents.generateSharedLink, fileId, data, options));
    };

    var copyBatchItems = function(data, options) {
        return returnValue(ServiceManager.copyBatchItems(customEvents.copyBatchItems, data, options));
    };

    var getOperationStatuses = function(options) {
        return returnValue(ServiceManager.getOperationStatuses(customEvents.getOperationStatuses, options));
    };

    var getPresignedUri = function (fileId, options) {
        return returnValue(ServiceManager.getPresignedUri(customEvents.getPresignedUri, fileId, options));
    };

    /* </documents> */

    /* <crm> */
    var createCrmUploadFile = function(params, type, id, data, options) {
        return returnValue(ServiceManager.createCrmUploadFile(customEvents.uploadCrmFile, params, type, id, data, options));
    };

    var getCrmContactInfo = function (params, contactid, options) {
        return returnValue(ServiceManager.getCrmContactInfo(customEvents.getCrmContactInfo, params, contactid, options));
    };

    var addCrmContactInfo = function(params, contactid, data, options) {
        return returnValue(ServiceManager.addCrmContactInfo(customEvents.addCrmContactInfo, params, contactid, data, options));
    };

    var updateCrmContactInfo = function(params, contactid, data, options) {
        return returnValue(ServiceManager.updateCrmContactInfo(customEvents.updateCrmContactInfo, params, contactid, data, options));
    };

    var deleteCrmContactInfo = function(params, contactid, id, options) {
        return returnValue(ServiceManager.deleteCrmContactInfo(customEvents.deleteCrmContactInfo, params, contactid, id, options));
    };

    var addCrmContactData = function(params, id, data, options) {
        return returnValue(ServiceManager.addCrmContactData(customEvents.addCrmContactData, params, id, data, options));
    };

    var updateCrmContactData = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactData(customEvents.updateCrmContactData, params, id, data, options));
    };

    var addCrmContactTwitter = function(params, contactid, data, options) {
        return returnValue(ServiceManager.addCrmContactTwitter(customEvents.addCrmContactTwitter, params, contactid, data, options));
    };

    var addCrmEntityNote = function(params, type, id, data, options) {
        return returnValue(ServiceManager.addCrmEntityNote(customEvents.addCrmEntityNote, params, type, id, data, options));
    };

    var addCrmContact = function(params, isCompany, data, options) {
        if (isCompany === true) {
            return returnValue(ServiceManager.addCrmCompany(customEvents.addCrmCompany, params, data, options));
        } else {
            return returnValue(ServiceManager.addCrmPerson(customEvents.addCrmContact, params, data, options));
        }
    };

    var addCrmCompany = function(params, data, options) {
        return returnValue(ServiceManager.addCrmCompany(customEvents.addCrmCompany, params, data, options));
    };

    var updateCrmCompany = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmCompany(customEvents.updateCrmCompany, params, id, data, options));
    };

    var updateCrmContactContactStatus = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactContactStatus(customEvents.updateCrmContactContactStatus, params, id, data, options));
    };

    var updateCrmCompanyContactStatus = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmCompanyContactStatus(customEvents.updateCrmCompanyContactStatus, params, id, data, options));
    };

    var updateCrmPersonContactStatus = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmPersonContactStatus(customEvents.updateCrmPersonContactStatus, params, id, data, options));
    };

    var addCrmPerson = function(params, data, options) {
        return returnValue(ServiceManager.addCrmPerson(customEvents.addCrmPerson, params, data, options));
    };

    var updateCrmPerson = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmPerson(customEvents.updateCrmPerson, params, id, data, options));
    };

    var removeCrmContact = function(params, ids, options) {
        if (arguments.length === 2) {
            options = arguments[1];
            ids = null;
        }

        return returnValue(ServiceManager.removeCrmContact(customEvents.removeCrmContact, params, ids, options));
    };

    var mergeCrmContacts = function(params, data, options) {
        return returnValue(ServiceManager.mergeCrmContacts(customEvents.mergeCrmContacts, params, data, options));
    };

    var addCrmTag = function(params, type, ids, tagname, options) {
        if (arguments.length === 4) {
            options = arguments[3];
            tagname = arguments[2];
            ids = null;
        }

        return returnValue(ServiceManager.addCrmTag(customEvents.addCrmTag, params, type, ids, tagname, options));
    };

    var addCrmContactTagToGroup = function(params, type, id, tagname, options) {
        return returnValue(ServiceManager.addCrmContactTagToGroup(customEvents.addCrmContactTagToGroup, params, type, id, tagname, options));
    };

    var deleteCrmContactTagFromGroup = function(params, type, id, tagname, options) {
        return returnValue(ServiceManager.deleteCrmContactTagFromGroup(customEvents.deleteCrmContactTagFromGroup, params, type, id, tagname, options));
    };

    var addCrmEntityTag = function(params, type, tagname, options) {
        return returnValue(ServiceManager.addCrmEntityTag(customEvents.addCrmEntityTag, params, type, tagname, options));
    };

    var removeCrmTag = function(params, type, id, tagname, options) {
        return returnValue(ServiceManager.removeCrmTag(customEvents.removeCrmTag, params, type, id, tagname, options));
    };

    var removeCrmEntityTag = function(params, type, tagname, options) {
        return returnValue(ServiceManager.removeCrmEntityTag(customEvents.removeCrmEntityTag, params, type, tagname, options));
    };

    var removeCrmUnusedTag = function(params, type, options) {
        return returnValue(ServiceManager.removeCrmUnusedTag(customEvents.removeCrmUnusedTag, params, type, options));
    };

    var getCrmCustomFields = function(params, type, options) {
        return returnValue(ServiceManager.getCrmCustomFields(customEvents.getCrmCustomFields, params, type, options));
    };

    var addCrmCustomField = function(params, type, data, options) {
        return returnValue(ServiceManager.addCrmCustomField(customEvents.addCrmCustomField, params, type, data, options));
    };

    var updateCrmCustomField = function(params, type, id, data, options) {
        return returnValue(ServiceManager.updateCrmCustomField(customEvents.updateCrmCustomField, params, type, id, data, options));
    };

    var removeCrmCustomField = function(params, type, id, options) {
        return returnValue(ServiceManager.removeCrmCustomField(customEvents.removeCrmCustomField, params, type, id, options));
    };

    var reorderCrmCustomFields = function(params, type, ids, options) {
        return returnValue(ServiceManager.reorderCrmCustomFields(customEvents.reorderCrmCustomFields, params, type, ids, options));
    };

    var getCrmDealMilestones = function(params, options) {
        return returnValue(ServiceManager.getCrmDealMilestones(customEvents.getCrmDealMilestones, params, options));
    };

    var addCrmDealMilestone = function(params, data, options) {
        return returnValue(ServiceManager.addCrmDealMilestone(customEvents.addCrmDealMilestone, params, data, options));
    };

    var updateCrmDealMilestone = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmDealMilestone(customEvents.updateCrmDealMilestone, params, id, data, options));
    };

    var updateCrmDealMilestoneColor = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmDealMilestoneColor(customEvents.updateCrmDealMilestoneColor, params, id, data, options));
    };

    var removeCrmDealMilestone = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmDealMilestone(customEvents.removeCrmDealMilestone, params, id, options));
    };

    var reorderCrmDealMilestones = function(params, ids, options) {
        return returnValue(ServiceManager.reorderCrmDealMilestones(customEvents.reorderCrmDealMilestones, params, ids, options));
    };

    var addCrmContactStatus = function(params, data, options) {
        return returnValue(ServiceManager.addCrmContactStatus(customEvents.addCrmContactStatus, params, data, options));
    };

    var updateCrmContactStatus = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactStatus(customEvents.updateCrmContactStatus, params, id, data, options));
    };

    var updateCrmContactStatusColor = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactStatusColor(customEvents.updateCrmContactStatusColor, params, id, data, options));
    };

    var removeCrmContactStatus = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmContactStatus(customEvents.removeCrmContactStatus, params, id, options));
    };

    var addCrmContactType = function(params, data, options) {
        return returnValue(ServiceManager.addCrmContactType(customEvents.addCrmContactType, params, data, options));
    };

    var updateCrmContactType = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactType(customEvents.updateCrmContactType, params, id, data, options));
    };

    var removeCrmContactType = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmContactType(customEvents.removeCrmContactType, params, id, options));
    };

    var getCrmListItem = function(params, type, options) {
        return returnValue(ServiceManager.getCrmListItem(customEvents.getCrmListItem, params, type, options));
    };

    var addCrmListItem = function(params, type, data, options) {
        return returnValue(ServiceManager.addCrmListItem(customEvents.addCrmListItem, params, type, data, options));
    };

    var updateCrmListItem = function(params, type, id, data, options) {
        return returnValue(ServiceManager.updateCrmListItem(customEvents.updateCrmListItem, params, type, id, data, options));
    };

    var updateCrmListItemIcon = function(params, type, id, data, options) {
        return returnValue(ServiceManager.updateCrmListItemIcon(customEvents.updateCrmListItemIcon, params, type, id, data, options));
    };

    var removeCrmListItem = function(params, type, id, options) {
        return returnValue(ServiceManager.removeCrmListItem(customEvents.removeCrmListItem, params, type, id, options));
    };

    var reorderCrmListItems = function(params, type, titles, options) {
        return returnValue(ServiceManager.reorderCrmListItems(customEvents.reorderCrmListItems, params, type, titles, options));
    };

    var addCrmTask = function(params, data, options) {
        return returnValue(ServiceManager.addCrmTask(customEvents.addCrmTask, params, data, options));
    };

    var addCrmTaskGroup = function(params, data, options) {
        return returnValue(ServiceManager.addCrmTaskGroup(customEvents.addCrmTaskGroup, params, data, options));
    };

    var getCrmTask = function(params, id, options) {
        return returnValue(ServiceManager.getCrmTask(customEvents.getCrmTask, params, id, options));
    };

    var updateCrmTask = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmTask(customEvents.updateCrmTask, params, id, data, options));
    };

    var removeCrmTask = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmTask(customEvents.removeCrmTask, params, id, options));
    };

    var addCrmEntityMember = function(params, type, entityid, id, data, options) {
        var fn = null;
        switch (type) {
            case 'company':
                fn = ServiceManager.addCrmPersonMember;
                break;
            case 'project':
                fn = ServiceManager.addCrmContactForProject;
                break;
            default:
                fn = ServiceManager.addCrmContactMember;
                break;
        }
        if (fn) {
            return returnValue(fn(customEvents.addCrmEntityMember, params, type, entityid, id, data, options));
        }
        return false;
    };

    var addCrmContactsForProject = function(params, projectid, data, options) {
        return returnValue(ServiceManager.addCrmContactsForProject(customEvents.addCrmContactsForProject, params, projectid, data, options));
    };

    var addCrmDealForContact = function(params, contactid, opportunityid, options) {
        return returnValue(ServiceManager.addCrmDealForContact(customEvents.addCrmDealForContact, params, contactid, opportunityid, options));
    };

    var removeCrmDealFromContact = function(params, contactid, opportunityid, options) {
        return returnValue(ServiceManager.removeCrmDealFromContact(customEvents.removeCrmDealFromContact, params, contactid, opportunityid, options));
    };

    var removeCrmEntityMember = function(params, type, entityid, id, options) {
        var fn = null;
        switch (type) {
            case 'company':
                fn = ServiceManager.removeCrmPersonMember;
                break;
            case 'project':
                fn = ServiceManager.removeCrmContactFromProject;
                break;
            default:
                fn = ServiceManager.removeCrmContactMember;
                break;
        }
        if (fn) {
            return returnValue(fn(customEvents.removeCrmEntityMember, params, type, entityid, id, options));
        }
        return false;
    };

    var getCrmCases = function(params, options) {
        return returnValue(ServiceManager.getCrmCases(customEvents.getCrmCases, params, options));
    };

    var getCrmCasesByPrefix = function(params, options) {
        return returnValue(ServiceManager.getCrmCasesByPrefix(customEvents.getCrmCasesByPrefix, params, options));
    };

    var removeCrmCase = function(params, ids, options) {
        if (arguments.length === 2) {
            options = arguments[1];
            ids = null;
        }
        return returnValue(ServiceManager.removeCrmCase(customEvents.removeCrmCase, params, ids, options));
    };

    var updateCrmCase = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmCase(customEvents.updateCrmCase, params, id, data, options));
    };

    var getCrmContacts = function(params, options) {
        return returnValue(ServiceManager.getCrmContacts(customEvents.getCrmContacts, params, options));
    };

    var getCrmSimpleContacts = function(params, options) {
        return returnValue(ServiceManager.getCrmSimpleContacts(customEvents.getCrmSimpleContacts, params, options));
    };

    var getCrmContactsForMail = function(params, data, options) {
        return returnValue(ServiceManager.getCrmContactsForMail(customEvents.getCrmContactsForMail, params, data, options));
    };

    var getCrmContactsByPrefix = function(params, options) {
        return returnValue(ServiceManager.getCrmContactsByPrefix(customEvents.getCrmContactsByPrefix, params, options));
    };

    var getCrmContact = function(params, id, options) {
        return returnValue(ServiceManager.getCrmContact(customEvents.getCrmContact, params, id, options));
    };

    var getCrmTags = function(params, type, id, options) {
        return returnValue(ServiceManager.getCrmTags(customEvents.getCrmTags, params, type, id, options));
    };

    var getCrmEntityTags = function(params, type, options) {
        return returnValue(ServiceManager.getCrmEntityTags(customEvents.getCrmEntityTags, params, type, options));
    };

    var getCrmContactsForProject = function(params, id, options) {
        return returnValue(ServiceManager.getCrmContactsForProject(customEvents.getCrmContactsForProject, params, id, options));
    };

    var getCrmEntityMembers = function(params, type, id, options) {
        var fn = null;
        switch (type) {
            case 'company':
                fn = ServiceManager.getCrmPersonMembers;
                break;
            default:
                fn = ServiceManager.getCrmContactMembers;
                break;
        }
        if (fn) {
            return returnValue(fn(customEvents.getCrmEntityMembers, params, type, id, options));
        }
        return false;
    };

    var getCrmTasks = function(params, options) {
        return returnValue(ServiceManager.getCrmTasks(customEvents.getCrmTasks, params, options));
    };

    var getCrmOpportunity = function(params, id, options) {
        return returnValue(ServiceManager.getCrmOpportunity(customEvents.getCrmOpportunity, params, id, options));
    };

    var getCrmCase = function(params, id, options) {
        return returnValue(ServiceManager.getCrmCase(customEvents.getCrmCase, params, id, options));
    };

    var getContactsByContactInfo = function(params, data, options) {
        return returnValue(ServiceManager.getContactsByContactInfo(customEvents.getContactsByContactInfo, params, data, options));
    };

    var getCrmOpportunities = function(params, options) {
        return returnValue(ServiceManager.getCrmOpportunities(customEvents.getCrmOpportunities, params, options));
    };
    
    var getCrmOpportunitiesByContact = function (params, id, options) {
        return returnValue(ServiceManager.getCrmOpportunitiesByContact(customEvents.getCrmOpportunitiesByContact, params, id, options));
    };

    var getCrmInvoices = function(params, options) {
        return returnValue(ServiceManager.getCrmInvoices(customEvents.getCrmInvoices, params, options));
    };
    
    var getCrmEntityInvoices = function (params, type, id, options) {
        return returnValue(ServiceManager.getCrmEntityInvoices(customEvents.getCrmEntityInvoices, params, type, id, options));
    };

    var updateCrmInvoicesStatusBatch = function(params, status, ids, options) {
        return returnValue(ServiceManager.updateCrmInvoicesStatusBatch(customEvents.updateCrmInvoicesStatusBatch, params, status, ids, options));
    };

    var getCrmInvoiceByNumber = function(params, number, options) {
        return returnValue(ServiceManager.getCrmInvoiceByNumber(customEvents.getCrmInvoiceByNumber, params, number, options));
    };

    var getCrmInvoiceByNumberExistence = function(params, number, options) {
        return returnValue(ServiceManager.getCrmInvoiceByNumberExistence(customEvents.getCrmInvoiceByNumberExistence, params, number, options));
    };

    var getCrmInvoiceItems = function(params, options) {
        return returnValue(ServiceManager.getCrmInvoiceItems(customEvents.getCrmInvoiceItems, params, options));
    };

    var addCrmInvoiceItem = function (params, data, options) {
        return returnValue(ServiceManager.addCrmInvoiceItem(customEvents.addCrmInvoiceItem, params, data, options));
    };

    var updateCrmInvoiceItem = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmInvoiceItem(customEvents.updateCrmInvoiceItem, params, id, data, options));
    };

    var removeCrmInvoiceItem = function (params, ids, options) {
        if (arguments.length === 2) {
            options = arguments[1];
            ids = null;
        }
        return returnValue(ServiceManager.removeCrmInvoiceItem(customEvents.removeCrmInvoiceItem, params, ids, options));
    };

    var getCrmInvoiceTaxes = function(params, options) {
        return returnValue(ServiceManager.getCrmInvoiceTaxes(customEvents.getCrmInvoiceTaxes, params, options));
    };
    
    var addCrmInvoiceTax = function (params, data, options) {
        return returnValue(ServiceManager.addCrmInvoiceTax(customEvents.addCrmInvoiceTax, params, data, options));
    };

    var updateCrmInvoiceTax = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmInvoiceTax(customEvents.updateCrmInvoiceTax, params, id, data, options));
    };

    var removeCrmInvoiceTax = function (params, id, options) {
        return returnValue(ServiceManager.removeCrmInvoiceTax(customEvents.removeCrmInvoiceTax, params, id, options));
    };

    var getCrmInvoice = function (params, id, options) {
        return returnValue(ServiceManager.getCrmInvoice(customEvents.getCrmInvoice, params, id, options));
    };
    
    var getCrmInvoiceSample = function (params, options) {
        return returnValue(ServiceManager.getCrmInvoiceSample(customEvents.getCrmInvoiceSample, params, options));
    };
    
    var getCrmInvoiceJsonData = function (params, id, options) {
        return returnValue(ServiceManager.getCrmInvoiceJsonData(customEvents.getCrmInvoiceJsonData, params, id, options));
    };

    var addCrmInvoice = function (params, data, options) {
        return returnValue(ServiceManager.addCrmInvoice(customEvents.addCrmInvoice, params, data, options));
    };
    
    var updateCrmInvoice = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmInvoice(customEvents.updateCrmInvoice, params, id, data, options));
    };

    var removeCrmInvoice = function (params, ids, options) {
        return returnValue(ServiceManager.removeCrmInvoice(customEvents.removeCrmInvoice, params, ids, options));
    };

    var getInvoicePdfExistingOrCreate = function (params, id, options) {
        return returnValue(ServiceManager.getInvoicePdfExistingOrCreate(customEvents.getInvoicePdfExistingOrCreate, params, id, options));
    };

    var getInvoiceConverterData = function (params, data, options) {
        return returnValue(ServiceManager.getInvoiceConverterData(customEvents.getInvoiceConverterData, params, data, options));
    };

    var addCrmInvoiceLine = function (params, data, options) {
        return returnValue(ServiceManager.addCrmInvoiceLine(customEvents.addCrmInvoiceLine, params, data, options));
    };
    
    var updateCrmInvoiceLine = function (params, data, options) {
        return returnValue(ServiceManager.updateCrmInvoiceLine(customEvents.updateCrmInvoiceLine, params, id, data, options));
    };

    var removeCrmInvoiceLine = function (params, id, options) {
        return returnValue(ServiceManager.removeCrmInvoiceLine(customEvents.removeCrmInvoiceLine, params, id, options));
    };

    var getCrmInvoiceSettings = function (params, options) {
        return returnValue(ServiceManager.getCrmInvoiceSettings(customEvents.getCrmInvoiceSettings, params, options));
    };
    
    var updateCrmInvoiceSettingsName = function (params, data, options) {
        return returnValue(ServiceManager.updateCrmInvoiceSettingsName(customEvents.updateCrmInvoiceSettingsName, params, data, options));
    };
    
    var updateCrmInvoiceSettingsTerms = function (params, data, options) {
        return returnValue(ServiceManager.updateCrmInvoiceSettingsTerms(customEvents.updateCrmInvoiceSettingsTerms, params, data, options));
    };

    var getCrmCurrencyRates = function (params, options) {
        return returnValue(ServiceManager.getCrmCurrencyRates(customEvents.getCrmCurrencyRates, params, options));
    };
    
    var getCrmCurrencyRateById = function (params, id, options) {
        return returnValue(ServiceManager.getCrmCurrencyRateById(customEvents.getCrmCurrencyRateById, params, id, options));
    };
    
    var getCrmCurrencyRateByCurrencies = function (params, from, to, options) {
        return returnValue(ServiceManager.getCrmCurrencyRateByCurrencies(customEvents.getCrmCurrencyRateByCurrencies, params, from, to, options));
    };

    var addCrmCurrencyRate = function (params, data, options) {
        return returnValue(ServiceManager.addCrmCurrencyRate(customEvents.addCrmCurrencyRate, params, data, options));
    };

    var updateCrmCurrencyRate = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmCurrencyRate(customEvents.updateCrmCurrencyRate, params, id, data, options));
    };

    var removeCrmCurrencyRate = function (params, id, options) {
        return returnValue(ServiceManager.removeCrmCurrencyRate(customEvents.removeCrmCurrencyRatex, params, id, options));
    };

    var getCrmContactTweets = function (params, id, count, options) {
        return returnValue(ServiceManager.getCrmContactTweets(customEvents.getCrmContactTweets, params, id, count, options));
    };

    var getCrmContactTwitterProfiles = function (params, searchText, options) {
        return returnValue(ServiceManager.getCrmContactTwitterProfiles(customEvents.getCrmContactTwitterProfiles, params, searchText, options));
    };

    var getCrmContactFacebookProfiles = function (params, searchText, isUser, options) {
        return returnValue(ServiceManager.getCrmContactFacebookProfiles(customEvents.getCrmContactFacebookProfiles, params, searchText, isUser, options));
    };

    var getCrmContactLinkedinProfiles = function (params, firstName, lastName, options) {
        return returnValue(ServiceManager.getCrmContactLinkedinProfiles(customEvents.getCrmContactLinkedinProfiles, params, firstName, lastName, options));
    };

    var removeCrmContactAvatar = function (params, id, data, options) {
        return returnValue(ServiceManager.removeCrmContactAvatar(customEvents.removeCrmContactAvatar, params, id, data, options));
    };

    var updateCrmContactAvatar = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactAvatar(customEvents.updateCrmContactAvatar, params, id, data, options));
    };

    var getCrmContactSocialMediaAvatar = function (params, data, options) {
        return returnValue(ServiceManager.getCrmContactSocialMediaAvatar(customEvents.getCrmContactSocialMediaAvatar, params, data, options));
    };

    var getCrmContactInCruchBase = function (params, data, options) {
        return returnValue(ServiceManager.getCrmContactInCruchBase(customEvents.getCrmContactInCruchBase, params, data, options));
    };

    var startCrmImportFromCSV = function (params, data, options) {
        return returnValue(ServiceManager.startCrmImportFromCSV(customEvents.startCrmImportFromCSV, params, data, options));
    };

    var getStatusCrmImportFromCSV = function (params, data, options) {
        return returnValue(ServiceManager.getStatusCrmImportFromCSV(customEvents.getStatusCrmImportFromCSV, params, data, options));
    };

    var getCrmImportFromCSVSampleRow = function (params, data, options) {
        return returnValue(ServiceManager.getCrmImportFromCSVSampleRow(customEvents.getCrmImportFromCSVSampleRow, params, data, options));
    };

    var uploadFakeCrmImportFromCSV = function (params, data, options) {
        return returnValue(ServiceManager.uploadFakeCrmImportFromCSV(customEvents.uploadFakeCrmImportFromCSV, params, data, options));
    };

    var getStatusExportToCSV = function(params, options) {
        return returnValue(ServiceManager.getStatusExportToCSV(customEvents.getStatusExportToCSV, params, options));
    };

    var cancelExportToCSV = function(params, options) {
        return returnValue(ServiceManager.cancelExportToCSV(customEvents.cancelExportToCSV, params, options));
    };

    var startCrmExportToCSV = function(params, options) {
        return returnValue(ServiceManager.startCrmExportToCSV(customEvents.startCrmExportToCSV, params, options));
    };

    var getCrmOpportunitiesByPrefix = function(params, options) {
        return returnValue(ServiceManager.getCrmOpportunitiesByPrefix(customEvents.getCrmOpportunitiesByPrefix, params, options));
    };

    var removeCrmOpportunity = function(params, ids, options) {
        if (arguments.length === 2) {
            options = arguments[1];
            ids = null;
        }
        return returnValue(ServiceManager.removeCrmOpportunity(customEvents.removeCrmOpportunity, params, ids, options));
    };

    var updateCrmOpportunityMilestone = function(params, opportunityid, stageid, options) {
        return returnValue(ServiceManager.updateCrmOpportunityMilestone(customEvents.updateCrmOpportunityMilestone, params, opportunityid, stageid, options));
    };

    var getCrmCurrencyConvertion = function(params, data, options) {
        return returnValue(ServiceManager.getCrmCurrencyConvertion(customEvents.getCrmCurrencyConvertion, params, data, options));
    };

    var getCrmCurrencySummaryTable = function(params, currency, options) {
        return returnValue(ServiceManager.getCrmCurrencySummaryTable(customEvents.getCrmCurrencySummaryTable, params, currency, options));
    };

    var updateCrmCurrency = function(params, currency, options) {
        return returnValue(ServiceManager.updateCrmCurrency(customEvents.updateCrmCurrency, params, currency, options));
    };

    var updateCRMContactStatusSettings = function(params, changeContactStatusGroupAuto, options) {
        return returnValue(ServiceManager.updateCRMContactStatusSettings(customEvents.updateCRMContactStatusSettings, params, changeContactStatusGroupAuto, options));
    };

    var updateCRMContactTagSettings = function(params, addTagToContactGroupAuto, options) {
        return returnValue(ServiceManager.updateCRMContactTagSettings(customEvents.updateCRMContactTagSettings, params, addTagToContactGroupAuto, options));
    };

    var updateCRMContactMailToHistorySettings = function(params, writeMailToHistoryAuto, options) {
        return returnValue(ServiceManager.updateCRMContactMailToHistorySettings(customEvents.updateCRMContactMailToHistorySettings, params, writeMailToHistoryAuto, options));
    };

    var updateOrganisationSettingsCompanyName = function(params, companyName, options) {
        return returnValue(ServiceManager.updateOrganisationSettingsCompanyName(customEvents.updateOrganisationSettingsCompanyName, params, companyName, options));
    };

    var updateOrganisationSettingsAddresses = function(params, addresses, options) {
        return returnValue(ServiceManager.updateOrganisationSettingsAddresses(customEvents.updateOrganisationSettingsAddresses, params, addresses, options));
    };

    var updateOrganisationSettingsLogo = function(params, data, options) {
        return returnValue(ServiceManager.updateOrganisationSettingsLogo(customEvents.updateOrganisationSettingsLogo, params, data, options));
    };

    var getOrganisationSettingsLogo = function(params, id, options) {
        return returnValue(ServiceManager.getOrganisationSettingsLogo(customEvents.getOrganisationSettingsLogo, params, id, options));
    };

    var updateWebToLeadFormKey = function(params, options) {
        return returnValue(ServiceManager.updateWebToLeadFormKey(customEvents.updateWebToLeadFormKey, params, options));
    };

    var updateCRMSMTPSettings = function(params, data, options) {
        return returnValue(ServiceManager.updateCRMSMTPSettings(customEvents.updateCRMSMTPSettings, params, data, options));
    };

    var sendSMTPTestMail = function(params, data, options) {
        return returnValue(ServiceManager.sendSMTPTestMail(customEvents.sendSMTPTestMail, params, data, options));
    };

    var sendSMTPMailToContacts = function(params, data, options) {
        return returnValue(ServiceManager.sendSMTPMailToContacts(customEvents.sendSMTPMailToContacts, params, data, options));
    };

    var getPreviewSMTPMailToContacts = function(params, data, options) {
        return returnValue(ServiceManager.getPreviewSMTPMailToContacts(customEvents.getPreviewSMTPMailToContacts, params, data, options));
    };

    var getStatusSMTPMailToContacts = function(params, options) {
        return returnValue(ServiceManager.getStatusSMTPMailToContacts(customEvents.getStatusSMTPMailToContacts, params, options));
    };

    var cancelSMTPMailToContacts = function(params, options) {
        return returnValue(ServiceManager.cancelSMTPMailToContacts(customEvents.cancelSMTPMailToContacts, params, options));
    };

    var addCrmHistoryEvent = function(params, data, options) {
        return returnValue(ServiceManager.addCrmHistoryEvent(customEvents.addCrmHistoryEvent, params, data, options));
    };

    var removeCrmHistoryEvent = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmHistoryEvent(customEvents.removeCrmHistoryEvent, params, id, options));
    };

    var getCrmHistoryEvents = function(params, options) {
        return returnValue(ServiceManager.getCrmHistoryEvents(customEvents.getCrmHistoryEvents, params, options));
    };

    var removeCrmFile = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmFile(customEvents.removeCrmFile, params, id, options));
    };

    var getCrmFolder = function(params, id, options) {
        return returnValue(ServiceManager.getCrmFolder(customEvents.getCrmFolder, params, id, options));
    };

    var updateCrmContactRights = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmContactRights(customEvents.updateCrmContactRights, params, id, data, options));
    };

    var updateCrmCaseRights = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmCaseRights(customEvents.updateCrmCaseRights, params, id, data, options));
    };

    var updateCrmOpportunityRights = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmOpportunityRights(customEvents.updateCrmOpportunityRights, params, id, data, options));
    };

    var addCrmEntityFiles = function(params, id, type, data, options) {
        return returnValue(ServiceManager.addCrmEntityFiles(customEvents.addCrmEntityFiles, params, id, type, data, options));
    };

    var removeCrmEntityFiles = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmEntityFiles(customEvents.removeCrmEntityFiles, params, id, options));
    };

    var getCrmEntityFiles = function(params, id, type, options) {
        return returnValue(ServiceManager.getCrmEntityFiles(customEvents.getCrmEntityFiles, params, id, type, options));
    };

    var getCrmTaskCategories = function(params, options) {
        return returnValue(ServiceManager.getCrmTaskCategories(customEvents.getCrmTaskCategories, params, options));
    };

    var addCrmEntityTaskTemplateContainer = function(params, data, options) {
        return returnValue(ServiceManager.addCrmEntityTaskTemplateContainer(customEvents.addCrmEntityTaskTemplateContainer, params, data, options));
    };

    var updateCrmEntityTaskTemplateContainer = function(params, id, data, options) {
        return returnValue(ServiceManager.updateCrmEntityTaskTemplateContainer(customEvents.updateCrmEntityTaskTemplateContainer, params, id, data, options));
    };

    var removeCrmEntityTaskTemplateContainer = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmEntityTaskTemplateContainer(customEvents.removeCrmEntityTaskTemplateContainer, params, id, options));
    };

    var getCrmEntityTaskTemplateContainer = function(params, id, options) {
        return returnValue(ServiceManager.getCrmEntityTaskTemplateContainer(customEvents.getCrmEntityTaskTemplateContainer, params, id, options));
    };

    var getCrmEntityTaskTemplateContainers = function(params, type, options) {
        return returnValue(ServiceManager.getCrmEntityTaskTemplateContainers(customEvents.getCrmEntityTaskTemplateContainers, params, type, options));
    };

    var addCrmEntityTaskTemplate = function(params, data, options) {
        return returnValue(ServiceManager.addCrmEntityTaskTemplate(customEvents.addCrmEntityTaskTemplate, params, data, options));
    };

    var updateCrmEntityTaskTemplate = function(params, data, options) {
        return returnValue(ServiceManager.updateCrmEntityTaskTemplate(customEvents.updateCrmEntityTaskTemplate, params, data, options));
    };

    var removeCrmEntityTaskTemplate = function(params, id, options) {
        return returnValue(ServiceManager.removeCrmEntityTaskTemplate(customEvents.removeCrmEntityTaskTemplate, params, id, options));
    };

    var getCrmEntityTaskTemplate = function(params, id, options) {
        return returnValue(ServiceManager.getCrmEntityTaskTemplate(customEvents.getCrmEntityTaskTemplate, params, id, options));
    };

    var getCrmEntityTaskTemplates = function(params, containerid, options) {
        return returnValue(ServiceManager.getCrmEntityTaskTemplates(customEvents.getCrmEntityTaskTemplates, params, containerid, options));
    };
    
    //#region VoIP
    
    var getCrmVoipAvailableNumbers = function (params, options) {
        return returnValue(ServiceManager.getCrmVoipAvailableNumbers(customEvents.getCrmVoipAvailableNumbers, params, options));
    };
    
    var getCrmVoipExistingNumbers = function (params, options) {
        return returnValue(ServiceManager.getCrmVoipExistingNumbers(customEvents.getCrmVoipExistingNumbers, params, options));
    };
    
    var getCrmCurrentVoipNumber = function (params, options) {
        return returnValue(ServiceManager.getCrmCurrentVoipNumber(customEvents.getCrmCurrentVoipNumber, params, options));
    };
    
    var createCrmVoipNumber = function (params, data, options) {
        return returnValue(ServiceManager.createCrmVoipNumber(customEvents.createCrmVoipNumber, params, data, options));
    };
    
    var removeCrmVoipNumber = function (params, id, options) {
        return returnValue(ServiceManager.removeCrmVoipNumber(customEvents.removeCrmVoipNumber, params, id, options));
    };
    
    var updateCrmVoipNumberSettings = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmVoipNumberSettings(customEvents.updateCrmVoipNumberSettings, params, id, data, options));
    };
    
    var getCrmVoipSettings = function (params, options) {
        return returnValue(ServiceManager.getCrmVoipSettings(customEvents.getCrmVoipSettings, params, options));
    };
    
    var updateCrmVoipSettings = function (params, data, options) {
        return returnValue(ServiceManager.updateCrmVoipSettings(customEvents.updateCrmVoipSettings, params, data, options));
    };
    
    var addCrmVoipNumberOperators = function (params, id, data, options) {
        return returnValue(ServiceManager.addCrmVoipNumberOperators(customEvents.addCrmVoipNumberOperators, params, id, data, options));
    };
    
    var updateCrmVoipOperator = function (params, id, data, options) {
        return returnValue(ServiceManager.updateCrmVoipOperator(customEvents.updateCrmVoipOperator, params, id, data, options));
    };
    
    var removeCrmVoipNumberOperators = function (params, id, data, options) {
        return returnValue(ServiceManager.removeCrmVoipNumberOperators(customEvents.removeCrmVoipNumberOperators, params, id, data, options));
    };
    
    var getCrmVoipNumberOperators = function (params, id, options) {
        return returnValue(ServiceManager.getCrmVoipNumberOperators(customEvents.getCrmVoipNumberOperators, params, id, options));
    };
    
    var callVoipNumber = function (params, data, options) {
        return returnValue(ServiceManager.callVoipNumber(customEvents.callVoipNumber, params, data, options));
    };
    
    var answerVoipCall = function (params, id, options) {
        return returnValue(ServiceManager.answerVoipCall(customEvents.answerVoipCall, params, id, options));
    };
    
    var rejectVoipCall = function (params, id, options) {
        return returnValue(ServiceManager.rejectVoipCall(customEvents.rejectVoipCall, params, id, options));
    };
    
    var redirectVoipCall = function (params, id, data, options) {
        return returnValue(ServiceManager.redirectVoipCall(customEvents.redirectVoipCall, params, id, data, options));
    };
    
    var saveVoipCall = function (params, id, data, options) {
        return returnValue(ServiceManager.saveVoipCall(customEvents.saveVoipCall, params, id, data, options));
    };
    
    var getVoipCalls = function (params, data, options) {
        return returnValue(ServiceManager.getVoipCalls(customEvents.getVoipCalls, params, data, options));
    };
    
    var getVoipMissedCalls = function (params, options) {
        return returnValue(ServiceManager.getVoipMissedCalls(customEvents.getVoipMissedCalls, params, options));
    };
    
    var getVoipCall = function(params, id, options) {
        return returnValue(ServiceManager.getVoipCall(customEvents.getVoipCall, params, id, options));
    };

    var getVoipToken = function(params, options) {
        return returnValue(ServiceManager.getVoipToken(customEvents.getVoipToken, params, options));
    };
    
    var getVoipUploads = function (params, options) {
        return returnValue(ServiceManager.getVoipUploads(customEvents.getVoipUploads, params, options));
    };
    
    var deleteVoipUploads = function (params, data, options) {
        return returnValue(ServiceManager.deleteVoipUploads(customEvents.deleteVoipUploads, params, data, options));
    };
    
    //#endregion

    /* </crm> */
    /* <mail> */
    var _single_sm_request = function() {
        // sm methods and event names are equal
        var method = ServiceManager[arguments[0]];
        // get event string value by its name
        arguments[0] = customEvents[arguments[0]];
        // just 1 request
        arguments[arguments.length - 1] = arguments[arguments.length - 1] || {};
        arguments[arguments.length - 1].max_request_attempts = 1;
        return returnValue(method.apply(this, arguments));
    };

    var getMailFilteredMessages = function(params, filter_data, options) {
        return _single_sm_request('getMailFilteredMessages', params, filter_data, options);
    };

    var getMailFolders = function(params, last_check_time, options) {
        return _single_sm_request('getMailFolders', params, last_check_time, options);
    };

    var getMailMessagesModifyDate = function(params, options) {
        return _single_sm_request('getMailMessagesModifyDate', params, options);
    };

    var getMailFolderModifyDate = function(params, folder_id, options) {
        return _single_sm_request('getMailFolderModifyDate', params, folder_id, options);
    };

    var getAccounts = function (params, options) {
        return _single_sm_request('getAccounts', params, options);
    };

    var getMailTags = function(params, options) {
        return _single_sm_request('getMailTags', params, options);
    };

    var getMailMessage = function(params, id, data, options) {
        return _single_sm_request('getMailMessage', params, id, data, options);
    };
    
    var getMailboxSignature = function (params, id, data, options) {
        return _single_sm_request('getMailboxSignature', params, id, data, options);
    };
    
    var updateMailboxSignature = function(params, id, data, options) {
        return _single_sm_request('updateMailboxSignature', params, id, data, options)
    }

    var getLinkedCrmEntitiesInfo = function(params, data, options) {
        return _single_sm_request('getLinkedCrmEntitiesInfo', params, data, options);
    };

    var getNextMailMessageId = function(params, id, filter_data, options) {
        return _single_sm_request('getNextMailMessageId', params, id, filter_data, options);
    };

    var getPrevMailMessageId = function(params, id, filter_data, options) {
        return _single_sm_request('getPrevMailMessageId', params, id, filter_data, options);
    };

    var getMailConversation = function(params, id, data, options) {
        return _single_sm_request('getMailConversation', params, id, data, options);
    };

    var getNextMailConversationId = function(params, id, filter_data, options) {
        return _single_sm_request('getNextMailConversationId', params, id, filter_data, options);
    };

    var getPrevMailConversationId = function(params, id, filter_data, options) {
        return _single_sm_request('getPrevMailConversationId', params, id, filter_data, options);
    };

    var getMailMessageTemplate = function(params, options) {
        return _single_sm_request('getMailMessageTemplate', params, options);
    };

    var getMailRandomGuid = function(params, options) {
        return _single_sm_request('getMailRandomGuid', params, options);
    };

    var removeMailFolderMessages = function(params, folder_id, options) {
        return _single_sm_request('removeMailFolderMessages', params, folder_id, options);
    };

    var restoreMailMessages = function (params, data, options) {
        return _single_sm_request('restoreMailMessages', params, data, options);
    };

    var moveMailMessages = function(params, ids, folder, options) {
        return _single_sm_request('moveMailMessages', params, ids, folder, options);
    };

    var removeMailMessages = function(params, ids, options) {
        return _single_sm_request('removeMailMessages', params, ids, options);
    };

    var markMailMessages = function(params, ids, status, options) {
        return _single_sm_request('markMailMessages', params, ids, status, options);
    };

    var createMailTag = function(params, name, style, addresses, options) {
        return _single_sm_request('createMailTag', params, name, style, addresses, options);
    };

    var updateMailTag = function(params, id, name, style, addresses, options) {
        return _single_sm_request('updateMailTag', params, id, name, style, addresses, options);
    };

    var removeMailTag = function(params, id, options) {
        return _single_sm_request('removeMailTag', params, id, options);
    };

    var setMailTag = function(params, messages_ids, tag_id, options) {
        return _single_sm_request('setMailTag', params, messages_ids, tag_id, options);
    };

    var setMailConversationsTag = function(params, messages_ids, tag_id, options) {
        return _single_sm_request('setMailConversationsTag', params, messages_ids, tag_id, options);
    };

    var unsetMailTag = function(params, messages_ids, tag_id, options) {
        return _single_sm_request('unsetMailTag', params, messages_ids, tag_id, options);
    };

    var unsetMailConversationsTag = function(params, messages_ids, tag_id, options) {
        return _single_sm_request('unsetMailConversationsTag', params, messages_ids, tag_id, options);
    };

    var addMailDocument = function(params, id, data, options) {
        return _single_sm_request('addMailDocument', params, id, data, options);
    };

    var removeMailMailbox = function(params, email, options) {
        return _single_sm_request('removeMailMailbox', params, email, options);
    };

    var getMailDefaultMailboxSettings = function(params, email, options) {
        return _single_sm_request('getMailDefaultMailboxSettings', params, email, options);
    };

    var getMailMailbox = function(params, email, options) {
        return _single_sm_request('getMailMailbox', params, email, options);
    };

    var setDefaultAccount = function (params, setDefault, email) {
        return _single_sm_request('setDefaultAccount', params, setDefault, email);
    };

    var createMailMailboxSimple = function(params, email, password, options) {
        return _single_sm_request('createMailMailboxSimple', params, email, password, options);
    };

    var createMailMailboxOAuth = function(params, email, refreshToken, serviceType, options) {
        return _single_sm_request('createMailMailboxOAuth', params, email, refreshToken, serviceType, options);
    };

    var createMailMailbox = function(params, name, email, ssl, pop3_account, pop3_password, pop3_port, pop3_server,
                                     smtp_account, smtp_password, smtp_port, smtp_server, smtp_auth, imap, restrict, ssl_outgoing, incoming_encryption_type, outcoming_encryption_type, auth_type_in, auth_type_smtp, options) {
        return _single_sm_request('createMailMailbox', params, name, email, ssl, pop3_account, pop3_password, pop3_port, pop3_server,
            smtp_account, smtp_password, smtp_port, smtp_server, smtp_auth, imap, restrict, ssl_outgoing, incoming_encryption_type, outcoming_encryption_type, auth_type_in, auth_type_smtp, options);
    };

    var updateMailMailbox = function(params, name, email, ssl, pop3_account, pop3_password, pop3_port, pop3_server,
                                     smtp_account, smtp_password, smtp_port, smtp_server, smtp_auth, restrict, ssl_outgoing, incoming_encryption_type, outcoming_encryption_type, auth_type_in, auth_type_smtp, options) {
        return _single_sm_request('updateMailMailbox', params, name, email, ssl, pop3_account, pop3_password, pop3_port, pop3_server,
            smtp_account, smtp_password, smtp_port, smtp_server, smtp_auth, restrict, ssl_outgoing, incoming_encryption_type, outcoming_encryption_type, auth_type_in, auth_type_smtp, options);
    };

    var setMailMailboxState = function(params, email, state, options) {
        return _single_sm_request('setMailMailboxState', params, email, state, options);
    };

    var removeMailMessageAttachment = function(params, message_id, attachment_id, options) {
        return _single_sm_request('removeMailMessageAttachment', params, message_id, attachment_id, options);
    };

    var sendMailMessage = function (params, id, from, subject, to, cc, bcc, body, attachments, streamId, mimeMessageId, mimeReplyToId, importance, tags, fileLinksShareMode, options) {
        return _single_sm_request('sendMailMessage', params, id, from, subject, to, cc, bcc, body, attachments, streamId, mimeMessageId, mimeReplyToId, importance, tags, fileLinksShareMode, options);
    };

    var saveMailMessage = function(params, id, from, subject, to, cc, bcc, body, attachments, streamId, mimeMessageId, mimeReplyToId, importance, tags, options) {
        return _single_sm_request('saveMailMessage', params, id, from, subject, to, cc, bcc, body, attachments, streamId, mimeMessageId, mimeReplyToId, importance, tags, options);
    };

    var getMailContacts = function(params, term, options) {
        return _single_sm_request('getMailContacts', params, term, options);
    };


    var getMailAlerts = function(params, options) {
        return _single_sm_request('getMailAlerts', params, options);
    };

    var deleteMailAlert = function(params, id, options) {
        return _single_sm_request('deleteMailAlert', params, id, options);
    };

    var getMailFilteredConversations = function(params, filter_data, options) {
        return _single_sm_request('getMailFilteredConversations', params, filter_data, options);
    };

    var moveMailConversations = function(params, ids, folder, options) {
        return _single_sm_request('moveMailConversations', params, ids, folder, options);
    };

    var restoreMailConversations = function(params, data, options) {
        return _single_sm_request('restoreMailConversations', params, data, options);
    };

    var removeMailConversations = function(params, ids, options) {
        return _single_sm_request('removeMailConversations', params, ids, options);
    };

    var markMailConversations = function(params, ids, status, options) {
        return _single_sm_request('markMailConversations', params, ids, status, options);
    };

    var getMailDisplayImagesAddresses = function(params, options) {
        return _single_sm_request('getMailDisplayImagesAddresses', params, options);
    };

    var createDisplayImagesAddress = function(params, email, options) {
        return _single_sm_request('createDisplayImagesAddress', params, email, options);
    };

    var removeDisplayImagesAddress = function(params, email, options) {
        return _single_sm_request('removeDisplayImagesAddress', params, email, options);
    };

    var linkChainToCrm = function (params, id_message, crm_contact_ids, options) {
        return _single_sm_request('linkChainToCrm', params, id_message, crm_contact_ids, options);
    };
    
    var markChainAsCrmLinked = function(params, id_message, crm_contact_ids, options) {
        return _single_sm_request('markChainAsCrmLinked', params, id_message, crm_contact_ids, options);
    };

    var unmarkChainAsCrmLinked = function(params, id_message, crm_contact_ids, options) {
        return _single_sm_request('unmarkChainAsCrmLinked', params, id_message, crm_contact_ids, options);
    };

    var exportMessageToCrm = function(params, id_message, crm_contact_ids, options) {
        return _single_sm_request('exportMessageToCrm', params, id_message, crm_contact_ids, options);
    };

    var isConversationLinkedWithCrm = function(params, message_id, options) {
        return _single_sm_request('isConversationLinkedWithCrm', params, message_id, options);
    };

    var getMailHelpCenterHtml = function(params, options) {
        return _single_sm_request('getMailHelpCenterHtml', params, options);
    };

    var exportAllAttachmentsToMyDocuments = function (params, id_message, options) {
        return _single_sm_request('exportAllAttachmentsToMyDocuments', params, id_message, options);
    };

    var exportAttachmentToMyDocuments = function (params, id_attachment, options) {
        return _single_sm_request('exportAttachmentToMyDocuments', params, id_attachment, options);
    };

    var setEMailInFolder = function (params, id_account, email_in_folder, options) {
        return _single_sm_request('setEMailInFolder', params, id_account, email_in_folder, options);
    };
    var getMailServer = function(params, options) {
        return _single_sm_request('getMailServer', params, options);
    };
    
    var getMailServerFullInfo = function (params, options) {
        return _single_sm_request('getMailServerFullInfo', params, options);
    };
    
    var getMailServerFreeDns = function (params, options) {
        return _single_sm_request('getMailServerFreeDns', params, options);
    };

    var getMailDomains = function (params, options) {
        return _single_sm_request('getMailDomains', params, options);
    };
    
    var getCommonMailDomain = function (params, options) {
        return _single_sm_request('getCommonMailDomain', params, options);
    };

    var addMailDomain = function (params, domain_name, dns_id, options) {
        return _single_sm_request('addMailDomain', params, domain_name, dns_id, options);
    };

    var removeMailDomain = function (params, id_domain, options) {
        return _single_sm_request('removeMailDomain', params, id_domain, options);
    };

    var addMailbox = function (params, mailbox_name, domain_id, user_id, options) {
        return _single_sm_request('addMailbox', params, mailbox_name, domain_id, user_id, options);
    };

    var addMyMailbox = function (params, mailbox_name, options) {
        return _single_sm_request('addMyMailbox', params, mailbox_name, options);
    };

    var getMailboxes = function (params, options) {
        return _single_sm_request('getMailboxes', params, options);
    };

    var removeMailbox = function (params, id_mailbox, options) {
        return _single_sm_request('removeMailbox', params, id_mailbox, options);
    };

    var addMailBoxAlias = function (params, mailbox_id, alias_name, options) {
        return _single_sm_request('addMailBoxAlias', params, mailbox_id, alias_name, options);
    };

    var removeMailBoxAlias = function (params, mailbox_id, address_id, options) {
        return _single_sm_request('removeMailBoxAlias', params, mailbox_id, address_id, options);
    };

    var addMailGroup = function (params, group_name, domain_id, address_ids, options) {
        return _single_sm_request('addMailGroup', params, group_name, domain_id, address_ids, options);
    };

    var addMailGroupAddress = function (params, group_id, address_id, options) {
        return _single_sm_request('addMailGroupAddress', params, group_id, address_id, options);
    };

    var removeMailGroupAddress = function (params, group_id, address_id, options) {
        return _single_sm_request('removeMailGroupAddress', params, group_id, address_id, options);
    };

    var getMailGroups = function (params, options) {
        return _single_sm_request('getMailGroups', params, options);
    };

    var removeMailGroup = function (params, id_group, options) {
        return _single_sm_request('removeMailGroup', params, id_group, options);
    };

    var isDomainExists = function (params, domain_name, options) {
        return _single_sm_request('isDomainExists', params, domain_name, options);
    };

    var checkDomainOwnership = function (params, domain_name, options) {
        return _single_sm_request('checkDomainOwnership', params, domain_name, options);
    };

    var getDomainDnsSettings = function (params, domain_id, options) {
        return _single_sm_request('getDomainDnsSettings', params, domain_id, options);
    };

    var checkDomainDnsStatus = function (params, domain_id, options) {
        return _single_sm_request('checkDomainDnsStatus', params, domain_id, options);
    };
    /* </mail> */

    /* <settings> */
    var getWebItemSecurityInfo = function(params, data, options) {
        return returnValue(ServiceManager.getWebItemSecurityInfo(customEvents.getWebItemSecurityInfo, params, data, options));
    };

    var setWebItemSecurity = function(params, data, options) {
        return returnValue(ServiceManager.setWebItemSecurity(customEvents.setWebItemSecurity, params, data, options));
    };

    var setAccessToWebItems = function(params, data, options) {
        return returnValue(ServiceManager.setAccessToWebItems(customEvents.setAccessToWebItems, params, data, options));
    };

    var setProductAdministrator = function(params, data, options) {
        return returnValue(ServiceManager.setProductAdministrator(customEvents.setProductAdministrator, params, data, options));
    };

    var isProductAdministrator = function(params, data, options) {
        return returnValue(ServiceManager.isProductAdministrator(customEvents.isProductAdministrator, params, data, options));
    };
    var getPortalSettings = function(params, options) {
        return returnValue(ServiceManager.getPortalSettings(customEvents.getPortalSettings, params, options));
    };

    var getPortalLogo = function(params, options) {
        return returnValue(ServiceManager.getPortalLogo(customEvents.getPortalLogo, params, options));
    };

    /* </settings> */

    //#region security

    var getLoginEvents = function(options) {
        return returnValue(ServiceManager.getLoginEvents(customEvents.getLoginEvents, options));
    };
    
    var getAuditEvents = function(params, id, options) {
        return returnValue(ServiceManager.getAuditEvents(customEvents.getAuditEvents, params, id, options));
    };

    var createLoginHistoryReport = function(params, id, options) {
        return returnValue(ServiceManager.createLoginHistoryReport(customEvents.createLoginHistoryReport, params, id, options));
    };
    
    var createAuditTrailReport = function(params, id, options) {
        return returnValue(ServiceManager.createAuditTrailReport(customEvents.createAuditTrailReport, params, id, options));
    };
    
    var getIpRestrictions = function(options) {
        return returnValue(ServiceManager.getIpRestrictions(options));
    };
    
    var saveIpRestrictions = function(data, options) {
        return returnValue(ServiceManager.saveIpRestrictions(data, options));
    };

    var updateIpRestrictionsSettings = function(data, options) {
        return returnValue(ServiceManager.updateIpRestrictionsSettings(data, options));
    };

    var updateTipsSettings = function(data, options) {
        return returnValue(ServiceManager.updateTipsSettings(data, options));
    };

    var smsValidationSettings = function (enable, options) {
        return returnValue(ServiceManager.smsValidationSettings(enable, options));
    };

    //#endregion

    var getTalkUnreadMessages = function(params, options) {
        return returnValue(ServiceManager.getTalkUnreadMessages(customEvents.getTalkUnreadMessages, params, options));
    };

    var registerUserOnPersonal = function (data, options) {
        return returnValue(ServiceManager.registerUserOnPersonal(customEvents.registerUserOnPersonal, data, options));
    };

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
        extendEventManager: extendEventManager,

        getQuotas: getQuotas,

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
        removeUsers: removeUsers,
        getUserGroups: getUserGroups,

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
        addCmtBookmarkComment: addCmtBookmarkComment,
        getCmtBookmarkComments: getCmtBookmarkComments,

        subscribeProject: subscribeProject,
        getFeeds: getFeeds,
        getNewFeedsCount: getNewFeedsCount,
        readFeeds: readFeeds,
        getPrjTags: getPrjTags,
        getPrjTagsByName: getPrjTagsByName,
        addPrjComment: addPrjComment,
        updatePrjComment: updatePrjComment,
        removePrjComment: removePrjComment,
        getPrjComments: getPrjComments,
        addPrjTaskComment: addPrjTaskComment,
        updatePrjTaskComment: updatePrjTaskComment,
        removePrjTaskComment: removePrjTaskComment,
        getPrjTaskComments: getPrjTaskComments,
        addPrjDiscussionComment: addPrjDiscussionComment,
        updatePrjDiscussionComment: updatePrjDiscussionComment,
        removePrjDiscussionComment: removePrjDiscussionComment,
        getPrjDiscussionComments: getPrjDiscussionComments,

        addPrjEntityFiles: addPrjEntityFiles,
        uploadFilesToPrjEntity: uploadFilesToPrjEntity,
        removePrjEntityFiles: removePrjEntityFiles,
        getPrjEntityFiles: getPrjEntityFiles,
        addPrjSubtask: addPrjSubtask,
        updatePrjSubtask: updatePrjSubtask,
        updatePrjTask: updatePrjTask,
        removePrjSubtask: removePrjSubtask,
        addPrjTask: addPrjTask,
        getPrjTask: getPrjTask,
        addPrjTaskByMessage: addPrjTaskByMessage,
        getPrjTasks: getPrjTasks,
        getPrjTasksSimpleFilter: getPrjTasksSimpleFilter,
        getPrjTasksById: getPrjTasksById,
        addPrjMilestone: addPrjMilestone,
        updatePrjMilestone: updatePrjMilestone,
        removePrjMilestone: removePrjMilestone,
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
        addPrjReportTemplate: addPrjReportTemplate,
        updatePrjReportTemplate: updatePrjReportTemplate,
        deletePrjReportTemplate: deletePrjReportTemplate,

        createDocUploadFile: createDocUploadFile,
        addDocFile: addDocFile,
        removeDocFile: removeDocFile,
        getDocFile: getDocFile,
        addDocFolder: addDocFolder,
        addDocFile: addDocFile,
        getDocFile: getDocFile,
        addDocFolder: addDocFolder,
        addDocFile: addDocFile,
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
        getCrmContactLinkedinProfiles: getCrmContactLinkedinProfiles,
        removeCrmContactAvatar: removeCrmContactAvatar,
        updateCrmContactAvatar: updateCrmContactAvatar,
        getCrmContactSocialMediaAvatar: getCrmContactSocialMediaAvatar,
        getCrmContactInCruchBase: getCrmContactInCruchBase,
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
        getCrmCurrentVoipNumber: getCrmCurrentVoipNumber,
        createCrmVoipNumber: createCrmVoipNumber,
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
        getVoipCalls: getVoipCalls,
        getVoipMissedCalls: getVoipMissedCalls,
        getVoipCall: getVoipCall,
        getVoipToken: getVoipToken,
        getVoipUploads: getVoipUploads,
        deleteVoipUploads: deleteVoipUploads,

        getMailFilteredMessages: getMailFilteredMessages,
        getMailFolders: getMailFolders,
        getMailMessagesModifyDate: getMailMessagesModifyDate,
        getMailFolderModifyDate: getMailFolderModifyDate,
        getAccounts: getAccounts,
        getMailTags: getMailTags,
        getMailMessage: getMailMessage,
        getNextMailMessageId: getNextMailMessageId,
        getPrevMailMessageId: getPrevMailMessageId,
        getMailConversation: getMailConversation,
        getNextMailConversationId: getNextMailConversationId,
        getPrevMailConversationId: getPrevMailConversationId,
        getMailMessageTemplate: getMailMessageTemplate,
        getMailRandomGuid: getMailRandomGuid,
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
        createMailMailbox: createMailMailbox,
        updateMailMailbox: updateMailMailbox,
        setMailMailboxState: setMailMailboxState,
        removeMailMessageAttachment: removeMailMessageAttachment,
        sendMailMessage: sendMailMessage,
        saveMailMessage: saveMailMessage,
        getMailContacts: getMailContacts,
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
        exportAllAttachmentsToMyDocuments: exportAllAttachmentsToMyDocuments,
        exportAttachmentToMyDocuments: exportAttachmentToMyDocuments,
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
        removeMailBoxAlias: removeMailBoxAlias,
        addMailGroup: addMailGroup,
        addMailGroupAddress: addMailGroupAddress,
        removeMailGroupAddress: removeMailGroupAddress,
        getMailGroups: getMailGroups,
        removeMailGroup: removeMailGroup,
        isDomainExists: isDomainExists,
        checkDomainOwnership: checkDomainOwnership,
        getDomainDnsSettings: getDomainDnsSettings,

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
        getIpRestrictions: getIpRestrictions,
        saveIpRestrictions: saveIpRestrictions,
        updateIpRestrictionsSettings: updateIpRestrictionsSettings,
        updateTipsSettings: updateTipsSettings,
        smsValidationSettings: smsValidationSettings,

        getTalkUnreadMessages: getTalkUnreadMessages,

        registerUserOnPersonal: registerUserOnPersonal,
    };
})();