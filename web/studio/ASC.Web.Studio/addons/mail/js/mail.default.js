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


window.TMMail = (function($) {
    var isInit = false,
        requiredFieldErrorCss = "requiredFieldError",
        options = {
            MessagesPageSize: 25,
            ContactsPageSize: 25,
            ConversationSortAsc: "true"
        },
        saveMessageInterval = 5000, // 5 seconds for autosave
        showNextAlertTimeout = 60000,
        constants = {
            pageTitle: '',
            pageHeader: ''
        },

        optionCookieName = 'tmmail',
        optionSeparator = '&',
        maxWordLength = 10, //like google
        systemFolders = {
            inbox: { id: 1, name: 'inbox', displayName: ASC.Mail.Resources.MailResource.FolderNameInbox },
            sent: { id: 2, name: 'sent', displayName: ASC.Mail.Resources.MailResource.FolderNameSent },
            drafts: { id: 3, name: 'drafts', displayName: ASC.Mail.Resources.MailResource.FolderNameDrafts },
            trash: { id: 4, name: 'trash', displayName: ASC.Mail.Resources.MailResource.FolderNameTrash },
            spam: { id: 5, name: 'spam', displayName: ASC.Mail.Resources.MailResource.FolderNameSpam },
            userfolder: { id: 6, name: 'userfolder', displayName: '' },
            templates: { id: 7, name: 'templates', displayName: ASC.Mail.Resources.MailResource.FolderNameTemplates }
        },
        actionTypes = {
            'move': 1,
            'restore': 2,
            'delete_messages': 3,
            'clear_folder': 4,
            'move_filtered': 5,
            'restore_filtered': 6,
            'delete_filtered': 7
        },
        anchorRegExp = {
            sysfolders: /^$|^(inbox|sent|drafts|trash|spam|userfolder|templates)\/?(.+)*/,

            inbox: /^$|^inbox\/?(.+)*/,
            sent: /^sent\/?(.+)*/,
            drafts: /^drafts\/?(.+)*/,
            templates: /^templates\/?(.+)*/,
            trash: /^trash\/?(.+)*/,
            spam: /^spam\/?(.+)*/,
            userfolder: /^userfolder(?:=(\d+))?/,

            reply: /^reply\/(\d+)\/?$/,
            forward: /^forward\/(\d+)\/?$/,
            compose: /^compose\/?(.+)*/,
            composeto: /^composeto\/?(.+)*/,
            replyAll: /^replyAll\/(\d+)$/,
            writemessage: /^(compose|composeto|draftitem|templateitem|forward|reply|replyAll)\/?(.+)*/,
            composenewmessage: /^(compose|composeto|forward|reply|replyAll)\/?(.+)*/,
            viewmessage: /^(message|conversation)\/?(.+)*/,

            message: /^message\/(\d+)\/?$/,
            next_message: /^message\/(\d+)\/next\/?$/,
            prev_message: /^message\/(\d+)\/prev\/?$/,

            conversation: /^conversation\/(\d+)\/?$/,
            next_conversation: /^conversation\/(\d+)\/next\/?$/,
            prev_conversation: /^conversation\/(\d+)\/prev\/?$/,

            draftitem: /^draftitem\/(\d+)\/?$/,
            templateitem: /^templateitem\/(\d+)\/?$/,

            accounts: /^accounts\/?(.+)*/,

            tags: /^tags\/?$/,
            foldersettings: /^foldersettings\/?$/,
            filtersettings: /^filtersettings\/?$/,
            administration: /^administration\/?$/,
            commonSettings: /^common\/?$/,

            createfilter: /^createfilter\/?$/,
            editfilter: /^editfilter\/(\d+)\/?$/,

            teamlab: /^tlcontact\/?(.+)*/,
            crm: /^crmcontact\/?(.+)*/,
            personal_contact: /^customcontact\/?(.+)*/,
            helpcenter: /^help(?:=(\d+))?/,

            common: /(.+)*/,

            messagePrint: /^message\/print\/(\d+)\/?(.+)*/,
            conversationPrint: /^conversation\/print\/(\d+)\/?(.+)*/,

            contacts: /^contacts\/?$/,
            settings: /^settings\/?$/
        },
        providerRegExp = {
            gmail: /@(gmail\.com|google\.com|googlemail\.com)/,
            outlook: /@(hotmail\.com|hotmail\.co\.jp|hotmail\.co\.uk|hotmail\.com\.br|hotmail\.de|hotmail\.es|hotmail\.fr|hotmail\.it|live\.com|live\.co\.jp|live\.de|live\.fr|live\.it|live\.jp|live\.ru|live\.co\.uk|msn\.com|outlook\.com)/,
            yahoo: /@(yahoo\.com|yahoo\.com\.ar|yahoo\.com\.au|yahoo\.com\.br|yahoo\.com\.mx|yahoo\.co\.nz|yahoo\.co\.uk|yahoo\.de|yahoo\.es|yahoo\.fr|yahoo\.it|ymail\.com)/,
            mailru: /@(mail\.ru|bk\.ru|inbox\.ru|list\.ru)/,
            yandex: /@(yandex\.ru|yandex\.com|yandex\.ua|yandex\.com\.tr|yandex\.kz|yandex\.by|yandex\.net|ya\.ru|narod\.ru)/,
            rambler: /@(rambler\.ru|lenta\.ru|autorambler\.ru|myrambler\.ru|r0\.ru|ro\.ru)/,
            qip: /@(qip\.ru|5ballov\.ru|aeterna\.ru|fotoplenka\.ru|fromru\.com|front\.ru|hotbox\.ru|hotmail\.ru|krovatka\.su|land\.ru|mail15\.com|mail333\.com|memori\.ru|newmail\.ru|nightmail\.ru|nm\.ru|photofile\.ru|pisem\.net|pochta\.ru|pochtamt\.ru|pop3\.ru|rbcmail\.ru|smtp\.ru|ziza\.ru)/,
        },
        resizeUFhandle,
        generator = new IDGenerator();

    function init() {
        if (isInit === true) {
            return;
        }

        isInit = true;
        loadOptions();

        serviceManager.init();

        constants.pageTitle = document.title;
        constants.pageHeader = ASC.Mail.Resources.MailResource.MailTitle || 'Mail';

        ASC.Controls.AnchorController.bind(TMMail.anchors.common, checkAnchor);
    }

    function setPageHeaderFolderName(folderId) {
        // ToDo: fix this workaround for 'undefined' in title
        // case: open conversation by direct link, and 'undefined' word will appear in page title
        if (0 == folderId) {
            return;
        }

        var title, unread;

        if (folderId !== TMMail.sysfolders.userfolder.id) {
            unread = $('#foldersContainer').children('[folderid=' + folderId + ']').attr('unread');
            title = (unread == 0 || unread == undefined ? "" : ('(' + unread + ') ')) +
                getSysFolderDisplayNameById(folderId);
        } else {

            var node = userFoldersPanel.getSelected();

            if (!node) {
                setTimeout(function () { setPageHeaderFolderName(folderId); }, 100);
                return;
            }

            unread = null;


            if (node.li_attr) {

                if (commonSettingsPage.isConversationsEnabled()) {
                    unread = node.li_attr.unread_chains;
                } else {
                    unread = node.li_attr.unread_messages;
                }

            }

            title = (!unread ? "" : ('(' + unread + ') ')) + node.text;
        }

        setPageHeaderTitle(title);
    }

    function setPageHeaderTitle(title) {
        title = "{0} - {1}".format(title, constants.pageTitle);

        if ($.browser.msie) {
            setImmediate(function() {
                document.title = title;
            });
        } else {
            document.title = title;
        }
    }

    function option(name, value) {
        if (typeof name !== 'string') {
            return undefined;
        }
        if (typeof value === 'undefined') {
            return options[name];
        }
        options[name] = value;
        saveOptions();
        return value;
    }

    function loadOptions() {
        var fieldSeparator = ':',
            pos,
            name,
            value,
            collect = ASC.Mail.cookies.get(optionCookieName).split(optionSeparator);

        for (var i = 0, n = collect.length; i < n; i++) {
            if ((pos = collect[i].indexOf(fieldSeparator)) === -1) {
                continue;
            }
            name = collect[i].substring(0, pos);
            value = collect[i].substring(pos + 1);
            if (!name.length) {
                continue;
            }
            options[name] = value;
        }
    }

    function saveOptions() {
        var fieldSeparator = ':',
            collect = [];
        for (var name in options) {
            if (options.hasOwnProperty(name)) {
                collect.push(name + fieldSeparator + options[name]);
            }
        }
        ASC.Mail.cookies.set(optionCookieName, collect.join(optionSeparator));
    }

    function ltgt(str) {
        if (str.indexOf('<') !== false || str.indexOf('>') !== false) {
            str = str.replace(/</g, '&lt;').replace(/>/g, '&gt;');
        }
        return str;
    }

    function inArray(needle, haystack, strict) {
        strict = !!strict;
        var found = false, key;
        for (key in haystack) {
            if ((strict && haystack[key] === needle) || (!strict && haystack[key] == needle)) {
                found = true;
                break;
            }
        }
        return found;
    }

    function getErrorMessage(errors) {
        if ($.isArray(errors) && errors.length === 1) {
            return errors[0];
        }

        var mes = [];
        mes.push('<ul class="errors">');
        for (var i = 0, n = errors.length; i < n; i++) {
            mes.push('<li class="error-message">' + errors[i] + '</li>');
        }
        mes.push('</ul>');
        return mes.join('');
    }

    function isInvalidPage() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        var invalid = !(anchorRegExp.message.test(anchor) ||
            anchorRegExp.accounts.test(anchor) ||
            anchorRegExp.teamlab.test(anchor) ||
            anchorRegExp.crm.test(anchor) ||
            anchorRegExp.sysfolders.test(anchor) ||
            anchorRegExp.writemessage.test(anchor) ||
            anchorRegExp.tags.test(anchor) ||
            anchorRegExp.conversation.test(anchor) ||
            anchorRegExp.helpcenter.test(anchor) ||
            anchorRegExp.next_message.test(anchor) ||
            anchorRegExp.prev_message.test(anchor) ||
            anchorRegExp.next_conversation.test(anchor) ||
            anchorRegExp.prev_conversation.test(anchor) ||
            anchorRegExp.administration.test(anchor) ||
            anchorRegExp.commonSettings.test(anchor) ||
            anchorRegExp.messagePrint.test(anchor) ||
            anchorRegExp.conversationPrint.test(anchor) ||
            anchorRegExp.personal_contact.test(anchor) ||
            anchorRegExp.userfolder.test(anchor) ||
            anchorRegExp.foldersettings.test(anchor) ||
            anchorRegExp.filtersettings.test(anchor) ||
            anchorRegExp.createfilter.test(anchor) ||
            anchorRegExp.editfilter.test(anchor) ||
            anchorRegExp.contacts.test(anchor) ||
            anchorRegExp.settings.test(anchor));

        return invalid;
    }

    function pageIs(pageType) {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        switch (pageType) {
            case 'message':
                if (anchorRegExp.message.test(anchor)) {
                    return true;
                }
                break;
            case 'inbox':
                if (anchorRegExp.inbox.test(anchor) || anchor == '') {
                    return true;
                }
                break;
            case 'sent':
                if (anchorRegExp.sent.test(anchor)) {
                    return true;
                }
                break;
            case 'drafts':
                if (anchorRegExp.drafts.test(anchor)) {
                    return true;
                }
                break;
            case 'templates':
                if (anchorRegExp.templates.test(anchor)) {
                    return true;
                }
                break;
            case 'trash':
                if (anchorRegExp.trash.test(anchor)) {
                    return true;
                }
                break;
            case 'spam':
                if (anchorRegExp.spam.test(anchor)) {
                    return true;
                }
                break;
            case 'compose':
                if (anchorRegExp.compose.test(anchor)) {
                    return true;
                }
                break;
            case 'composeto':
                if (anchorRegExp.composeto.test(anchor)) {
                    return true;
                }
                break;
            case 'draftitem':
                if (anchorRegExp.draftitem.test(anchor)) {
                    return true;
                }
                break;
            case 'templateitem':
                if (anchorRegExp.templateitem.test(anchor)) {
                    return true;
                }
                break;
            case 'reply':
                if (anchorRegExp.reply.test(anchor)) {
                    return true;
                }
                break;
            case 'forward':
                if (anchorRegExp.forward.test(anchor)) {
                    return true;
                }
                break;
            case 'accounts':
                if (anchorRegExp.accounts.test(anchor)) {
                    return true;
                }
                break;
            case 'tlContact':
                if (anchorRegExp.teamlab.test(anchor)) {
                    return true;
                }
                break;
            case 'crmContact':
                if (anchorRegExp.crm.test(anchor)) {
                    return true;
                }
                break;
            case 'personalContact':
                if (anchorRegExp.personal_contact.test(anchor)) {
                    return true;
                }
                break;
            case 'sysfolders':
                if (anchorRegExp.sysfolders.test(anchor) || anchor == '') {
                    return true;
                }
                break;
            case 'userfolder':
                if (anchorRegExp.userfolder.test(anchor)) {
                    return true;
                }
                break;
            case 'composenewmessage':
                if (anchorRegExp.composenewmessage.test(anchor)) {
                    return true;
                }
                break;
            case 'writemessage':
                if (anchorRegExp.writemessage.test(anchor)) {
                    return true;
                }
                break;
            case 'viewmessage':
                if (anchorRegExp.viewmessage.test(anchor)) {
                    return true;
                }
                break;
            case 'tags':
                if (anchorRegExp.tags.test(anchor)) {
                    return true;
                }
                break;
            case 'foldersettings':
                if (anchorRegExp.foldersettings.test(anchor)) {
                    return true;
                }
                break;
            case 'filtersettings':
                if (anchorRegExp.filtersettings.test(anchor)) {
                    return true;
                }
                break;
            case 'createfilter':
                if (anchorRegExp.createfilter.test(anchor)) {
                    return true;
                }
                break;
            case 'editfilter':
                if (anchorRegExp.editfilter.test(anchor)) {
                    return true;
                }
                break;
            case 'administration':
                if (anchorRegExp.administration.test(anchor)) {
                    return true;
                }
                break;
            case 'common':
                if (anchorRegExp.commonSettings.test(anchor)) {
                    return true;
                }
                break;
            case 'conversation':
                if (anchorRegExp.conversation.test(anchor)) {
                    return true;
                }
                break;
            case 'helpcenter':
                if (anchorRegExp.helpcenter.test(anchor)) {
                    return true;
                }
                break;
            case 'messagePrint':
                if (anchorRegExp.messagePrint.test(anchor)) {
                    return true;
                }
                break;
            case 'conversationPrint':
                if (anchorRegExp.conversationPrint.test(anchor)) {
                    return true;
                }
                break;
            case 'print':
                if (anchorRegExp.messagePrint.test(anchor) || anchorRegExp.conversationPrint.test(anchor)) {
                    return true;
                }
                break;
            case 'contacts':
                if (anchorRegExp.contacts.test(anchor)) {
                    return true;
                }
                break;
            case 'settings':
                if (anchorRegExp.settings.test(anchor)) {
                    return true;
                }
                break;
        }
        return false;
    }

    function getSysFolderNameById(sysfolderId, defaultValue) {
        var sysfolder = getSysFolderById(sysfolderId);
        return typeof sysfolder != 'undefined' && sysfolder ? sysfolder.name : defaultValue;
    }

    function getSysFolderDisplayNameById(sysfolderId, defaultValue) {
        var sysfolder = getSysFolderById(sysfolderId);
        return typeof sysfolder != 'undefined' && sysfolder ? sysfolder.displayName : defaultValue;
    }

    function getSysFolderIdByName(sysfolderName, defaultValue) {
        for (var sysfolderNameIn in systemFolders) {
            var sysfolder = systemFolders[sysfolderNameIn];
            if (sysfolder.name == sysfolderName) {
                return sysfolder.id;
            }
        }
        return defaultValue;
    }

    function getSysFolderById(sysfolderId) {
        for (var sysfolderName in systemFolders) {
            var sysfolder = systemFolders[sysfolderName];
            if (sysfolder.id == +sysfolderId) {
                return sysfolder;
            }
        }
        return undefined;
    }

    function extractFolderIdFromAnchor() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        if (anchor === "") {
            return systemFolders.inbox.id;
        }
        var sysfolderRes = anchorRegExp.sysfolders.exec(anchor);
        if (sysfolderRes != null) {
            return getSysFolderIdByName(sysfolderRes[1]);
        }
        return 0;
    }

    function extractUserFolderIdFromAnchor() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        if (anchor === "") {
            return null;
        }
        var userFolderId = anchorRegExp.userfolder.exec(anchor);
        if (userFolderId != null) {
            return userFolderId[1];
        }
        return 0;
    }

    function extractConversationIdFromAnchor() {
        var anchor = ASC.Controls.AnchorController.getAnchor();

        if (anchor !== "") {
            var conversationId = anchorRegExp.conversation.exec(anchor);
            if (conversationId != null) {
                return conversationId[1];
            }
        }

        return 0;
    }

    function extractFileIdsFromAnchor() {
        var anchor = ASC.Controls.AnchorController.getAnchor();

        if (anchor !== "") {
            try {
                var files = getParamsValue(anchor, /files=([^\/]+)/);

                if (!files) return undefined;

                var fileIds = JSON.parse(decodeURIComponent(files));

                return jq.isArray(fileIds) ? fileIds : undefined;
            } catch (e) {
                console.warn(e);
            }
        }

        return undefined;
    }

    function getAccountErrorFooter(address) {
        return window.MailScriptResource.AccountCreationErrorGmailFooter
            .format('<a target="blank" class="linkDescribe" href="' + getFaqLink(address) + '">', '</a>');
    }

    function getSupportLink() {
        return ASC.Mail.Constants.SUPPORT_URL;
    }

    function getFaqLink(address) {
        address = (address || "").toLowerCase();

        var anchor = "";

        if (providerRegExp.gmail.test(address)) {
            anchor = "#IssueswithGmailcomService_block";
        }
        else if (providerRegExp.outlook.test(address) || providerRegExp.yahoo.test(address)) {
            anchor = "#IssueswithHotmailcomandYahoocomServices_block";
        }
        else if (providerRegExp.mailru.test(address)) {
            anchor = "#IssueswithMailruService_block";
        }
        else if (providerRegExp.yandex.test(address)) {
            anchor = "#IssueswithYandexruService_block";
        }

        return ASC.Mail.Constants.FAQ_URL + anchor;
    }

    function getOAuthFaqLink() {
        return ASC.Mail.Constants.FAQ_URL + "#GmailcomService_12";
    }

    function moveToReply(msgid) {
        ASC.Controls.AnchorController.move('#reply/' + msgid);
    }

    function moveToReplyAll(msgid) {
        ASC.Controls.AnchorController.move('#replyAll/' + msgid);
    }

    function moveToForward(msgid) {
        ASC.Controls.AnchorController.move('#forward/' + msgid);
    }

    function moveToMessagePrint(messageId, showImages, showQuotes) {
        var href = window.location.href.split('#')[0] + '?blankpage=true#message/print/' + messageId;

        if (showImages) {
            href += '?sim=' + messageId;
        }
        if (showQuotes) {
            href += showImages ? '&' : '?';
            href += 'squ=' + messageId;
        }
        
        window.open(href);
    }

    function moveToConversationPrint(conversationId, simIds, squIds, sortAsc, showAll) {
        var href = window.location.href.split('#')[0] + '?blankpage=true#conversation/print/' + conversationId;

        if (simIds && simIds.length) {
            href += '?sim=' + simIds.join(',');
        }
        if (squIds && squIds.length) {
            href += simIds && simIds.length ? '&' : '?';
            href += 'squ=' + squIds.join(',');
        }

        href += (simIds && simIds.length) || (squIds && squIds.length) ? '&' : '?';
        href += 'sortAsc=' + (sortAsc === undefined || sortAsc === true ? '1' : '0');

        if(showAll)
            href += '&showAll=' + (showAll ? '1' : '0');

        window.open(href);
    }

    function moveToInbox() {
        ASC.Controls.AnchorController.move(systemFolders.inbox.name);
    }

    function openMessage(id) {
        window.open('#message/' + id, '_blank');
    }

    function openConversation(id) {
        window.open('#conversation/' + id, '_blank');
    }

    function openDraftItem(id) {
        window.open('#draftitem/' + id, '_blank');
    }

    function openTemplateItem(id) {
        window.open('#templateitem/' + id, '_blank');
    }

    function moveToConversation(id, safe) {
        var anchor = '#conversation/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    }

    function moveToMessage(id, safe) {
        var anchor = '#message/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    }

    function moveToDraftItem(id, safe) {
        var anchor = '#draftitem/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    }

    function moveToTemplateItem(id, safe) {
        var anchor = '#templateitem/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    }

    function wordWrap(string) {
        var words = string.split(' ');
        for (var i = 0; i < words.length; i++) {
            var newWord = '';
            var lastIndex = 0;
            for (var j = maxWordLength; j < words[i].length; j += maxWordLength) {
                newWord += htmlEncode(words[i].slice(j - maxWordLength, j)) + '<wbr/>';
                lastIndex = j;
            }
            if (lastIndex > 0) {
                newWord += htmlEncode(words[i].slice(lastIndex));
                words[i] = newWord;
            } else {
                words[i] = htmlEncode(words[i]);
            }
        }
        string = words.join(' ');
        return string;
    }

    function getParamsValue(params, reg) {
        var myArray = reg.exec(params);
        if (myArray === null) {
            return undefined;
        }
        return myArray[1];
    }

    function showCompleteActionHint(actionType, isConversation, count, dstFolderId, userFolderId) {
        var hintText, folderName;

        if (dstFolderId == TMMail.sysfolders.userfolder.id) {
            folderName = TMMail.htmlEncode(userFoldersManager.get(userFolderId).name);
        } else {
            folderName = TMMail.getSysFolderDisplayNameById(dstFolderId, '');
        }

        switch (actionType) {
            case TMMail.action_types.move:
                hintText =
                    count == 1 ?
                        (isConversation ?
                            window.MailActionCompleteResource.moveOneConversationTo :
                            window.MailActionCompleteResource.moveOneMessageTo)
                            .replace('%folder%', folderName) :
                        (isConversation ?
                            window.MailActionCompleteResource.moveManyConversationsTo :
                            window.MailActionCompleteResource.moveManyMessagesTo)
                            .replace('%folder%', folderName)
                            .replace('%count%', count);
                break;
            case TMMail.action_types.restore:
                hintText =
                    count == 1 ?
                        (isConversation ?
                            window.MailActionCompleteResource.restoreOneConversationTo :
                            window.MailActionCompleteResource.restoreOneMessageTo)
                            .replace('%folder%', folderName) :
                        (isConversation ?
                            window.MailActionCompleteResource.restoreManyConversations :
                            window.MailActionCompleteResource.restoreManyMessages)
                            .replace('%count%', count);
                break;
            case TMMail.action_types.delete_messages:
                hintText =
                    count == 1 ?
                        (isConversation ?
                            window.MailActionCompleteResource.deleteOneConversation :
                            window.MailActionCompleteResource.deleteOneMessage)
                        :
                        (isConversation ?
                            window.MailActionCompleteResource.deleteManyConversations :
                            window.MailActionCompleteResource.deleteManyMessages)
                            .replace('%count%', count);
                break;
            case TMMail.action_types.clear_folder:
                hintText = window.MailActionCompleteResource.clearFolder.replace('%folder%', folderName);
                break;
            case TMMail.action_types.move_filtered:
                hintText = isConversation ?
                    window.MailActionCompleteResource.moveFilteredConversationsTo.replace('%folder%', folderName) :
                    window.MailActionCompleteResource.moveFilteredMessagesTo.replace('%folder%', folderName);
                break;
            case TMMail.action_types.restore_filtered:
                hintText = isConversation ?
                    window.MailActionCompleteResource.restoreFilteredConversations :
                    window.MailActionCompleteResource.restoreFilteredMessages;
                break;
            case TMMail.action_types.delete_filtered:
                hintText = isConversation ?
                    window.MailActionCompleteResource.deleteFilteredConversations :
                    window.MailActionCompleteResource.deleteFilteredMessages;
                break;
            default:
                return;
        }

        setTimeout(function() {
            window.LoadingBanner.hideLoading();
            window.toastr.success(hintText);
        }, 1000);
    }

    function strHash(str) {
        var hash = 0, i, l;
        if (str.length == 0) {
            return hash;
        }
        for (i = 0, l = str.length; i < l; i++) {
            hash = ((hash << 5) - hash) + str.charCodeAt(i);
            hash |= 0; // Convert to 32bit integer
        }
        return hash;
    }

    function canViewInDocuments(url) {
        return ASC.Files.Utility.CanWebView(url);
    }

    function canEditInDocuments(url) {
        return ASC.Files.Utility.CanWebEdit(url);
    }

    function canViewAsCalendar(filename) {
        return filename.match(/.ics$/i) !== null ||
            filename.match(/.ical$/i) !== null ||
            filename.match(/.ifb$/i) !== null ||
            filename.match(/.icalendar$/i) !== null;
    }

    function fixMailtoLinks(element) {
        element.find("a[href*='mailto:']").click(function() {
            messagePage.setToEmailAddresses([$(this).attr('href').substr(7, $(this).attr('href').length - 1)]);
            window.location.href = "#composeto";
            return false;
        });
    }

    function isIe() {
        var ua = navigator.userAgent;
        return ua.match(/Trident\/7\./) || ua.match(/MSIE *\d+\.\w+/i);
    }

    function getAttachmentDownloadUrl(attachmentId) {
        return ASC.Mail.Constants.DOWNLOAD_HANDLER_URL.format(attachmentId);
    }

    function getViewDocumentUrl(attachmentId) {
        return ASC.Mail.Constants.VIEW_DOCUMENT_HANDLER_URL.format(attachmentId);
    }

    function getEditDocumentUrl(attachmentId) {
        return ASC.Mail.Constants.EDIT_DOCUMENT_HANDLER_URL.format(attachmentId);
    }

    function getContactPhototUrl(contactId, photoSize) {
        return ASC.Mail.Constants.CONTACT_PHOTO_HANDLER_URL.format(contactId, photoSize);
    }

    function htmlEncode(value) {
        var entities = {
            '&nbsp;': ' ',
            '&': '&amp;',
            '>': '&gt;',
            '<': '&lt;',
            '"': '&quot;'
        }, keys = [], p, regex;

        for (p in entities) {
            keys.push(p);
        }

        regex = new RegExp('(' + keys.join('|') + ')', 'g');

        var result = (!value) ? value : String(value).replace(regex, function(match, capture) {
            return entities[capture];
        }).replace(/^\s+|\s+$/g, '');

        return result;
    }

    function htmlDecode(value) {
        var result = window.Encoder.htmlDecode(value);
        return result;
    }

    // checks current page anchor and change it to inbox folder if required
    // close viewers on page change

    function checkAnchor() {
        if (ASC.Files && ASC.Files.MediaPlayer && ASC.Files.MediaPlayer.isView) {
            ASC.Files.MediaPlayer.closePlayer();
        }

        if (isInvalidPage()) {
            moveToInbox();
        }
    }

    function disableButton(button, disable) {
        button.toggleClass("disable", disable);
        if (disable) {
            button.attr("disabled", "disabled");
        } else {
            button.removeAttr("disabled");
        }
    }

    function disableInput(input, disable) {
        if (disable) {
            input.attr('disabled', 'true');
        } else {
            input.removeAttr('disabled');
        }
    }

    function setRequiredHint(containerId, text) {
        var hint = $("#" + containerId + ".requiredField span.requiredErrorText");
        hint.text(text);
        hint.attr('title', text);
    }

    function setRequiredError(containerId, needShow) {
        $("#" + containerId + ".requiredField").toggleClass(requiredFieldErrorCss, needShow);
    }

    function isRequiredErrorVisible(containerId) {
        return $("#" + containerId + ".requiredField").hasClass(requiredFieldErrorCss);
    }

    function isPopupVisible() {
        return $('#manageWindow').is(':visible') || $('#commonPopup').is(':visible') || $("#popupDocumentUploader").is(':visible') || $('#tagWnd').is(':visible');
    }

    function getDateFormated(date, format) {
        return window.ServiceFactory.formattingDate(date, format,
                                    ASC.Resources.Master.DayNames,
                                    ASC.Resources.Master.DayNamesFull,
                                    ASC.Resources.Master.MonthNames,
                                    ASC.Resources.Master.MonthNamesFull);
    }

    function getMapUrl(location) {
        if (!location)
            return '';

        return "https://maps.google.com/maps?q={0}".format(location.split(/[,\s]/).join('+'));
    }

    function getFileIconByExt(fileExt) {
        var utility = ASC.Files.Utility,
            split = utility.getCssClassByFileTitle(fileExt).split('_'),
            iconSrc = '';

        iconSrc = split[split.length - 1].toLowerCase();

        return ASC.Mail.Master["file_" + iconSrc + "_21"] || ASC.Mail.Master.file_21;
    }

    function resizeContent() {
        if (TMMail.pageIs('sysfolders') || TMMail.pageIs('crmContact')) {
            mailBox.changeTagStyle();
        }
        mailBox.resizeActionMenuWidth();

        var mainPageEl = jq(".mainPageLayout"),
            mainPageContentEl = mainPageEl.find(".mainPageContent"),
            sidePanelEl = mainPageEl.find(".mainPageTableSidePanel"),
            mainPageWidth = parseInt(mainPageEl.width()),
            mainPageMinWidth = parseInt(mainPageEl.css("min-width")),
            sidePanelWidth = parseInt(sidePanelEl.width()),
            k = parseInt(mainPageEl.css("padding-left")) +
                parseInt(mainPageEl.css("padding-right")) +
                parseInt(mainPageContentEl.css("padding-left")) -
                parseInt(sidePanelEl.find(".ui-resizable-handle").width());

        var newWidth = (mainPageWidth > mainPageMinWidth ? mainPageWidth : mainPageMinWidth) - sidePanelWidth - k + "px";

        jq(".body").css("max-width", newWidth);

        resizeUserFolders();
    }

    function resizeUserFolders() {
        if (resizeUFhandle)
            clearTimeout(resizeUFhandle);

        var resize = function() {
            //console.log("resizeUserFolders()");
            var sidePanelEl = jq("#studio_sidePanel");

            if (!sidePanelEl) {
                //console.log("resizeUserFolders() failed");
                return;
            }

            var sidePanelWidth = parseInt(sidePanelEl.width());

            var userFolders = sidePanelEl.find("#userFolderContainer .userFolders .jstree-node");
            if (!userFolders.length) {
                //console.log("resizeUserFolders() no folders");
                return;
            }

            var i, n = userFolders.length;
            for (i = 0; i < n; i++) {
                var el = jq(userFolders[i]),
                    level = +el.attr("aria-level"),
                    counter = el.find("> .lattersCount"),
                    k = (counter.length > 0 ? jq(counter[0]).width() : 0),
                    newMaxWidth = sidePanelWidth - k - (24 * level);

                el.find("> .jstree-anchor").css("max-width", newMaxWidth);
                //console.log("id: #{0} level: {1} sidePanelWidth: {2} k: {3}  max-width: {4}".format(el.attr("id"), level, sidePanelWidth, k, newMaxWidth));
            }
        };

        resizeUFhandle = setTimeout(resize, 25);
        resize();
    }

    function iterationCopy(src) {
        if (!src) return src;

        var target = {};
        for (var prop in src) {
            if (src.hasOwnProperty(prop)) {
                target[prop] = src[prop];
            }
        }
        return target;
    }

    function hideAllActionPanels() {
        $.each($('.actionPanel:visible'), function (index, value) {
            var popup = $(value);
            if (popup != undefined) {
                popup.hide();
            }
        });
    }

    function IDGenerator() {

        this.length = 8;
        this.timestamp = +new Date;

        var getRandomInt = function (min, max) {
            return Math.floor(Math.random() * (max - min + 1)) + min;
        }

        this.generate = function () {
            var ts = this.timestamp.toString();
            var parts = ts.split("").reverse();
            var id = "";

            for (var i = 0; i < this.length; ++i) {
                var index = getRandomInt(0, parts.length - 1);
                id += parts[index];
            }

            return id;
        }
    }

    function getRandomId() {
        return generator.generate();
    }

    function scrollTop() {
        var windowWidth = $(window).width();
        window.scrollTo(0, 0);

        if (windowWidth > 1200) {
            var pageContent = $(".page-content");
            pageContent.scrollLeft(0);
            pageContent.scrollTop(0);
        }
    }

    function isTemplate() {
        return pageIs('templateitem');
    }

    return {
        sysfolders: systemFolders,
        action_types: actionTypes,
        anchors: anchorRegExp,

        init: init,
        option: option,

        saveMessageInterval: saveMessageInterval,
        showNextAlertTimeout: showNextAlertTimeout,

        ltgt: ltgt,
        inArray: inArray,

        setPageHeaderFolderName: setPageHeaderFolderName,
        setPageHeaderTitle: setPageHeaderTitle,
        getErrorMessage: getErrorMessage,
        pageIs: pageIs,

        getSysFolderById: getSysFolderById,
        getSysFolderNameById: getSysFolderNameById,
        getSysFolderIdByName: getSysFolderIdByName,
        getSysFolderDisplayNameById: getSysFolderDisplayNameById,
        extractFolderIdFromAnchor: extractFolderIdFromAnchor,
        extractConversationIdFromAnchor: extractConversationIdFromAnchor,
        extractUserFolderIdFromAnchor: extractUserFolderIdFromAnchor,
        extractFileIdsFromAnchor: extractFileIdsFromAnchor,

        getFaqLink: getFaqLink,
        getSupportLink: getSupportLink,

        moveToReply: moveToReply,
        moveToReplyAll: moveToReplyAll,
        moveToForward: moveToForward,
        moveToMessagePrint: moveToMessagePrint,
        moveToConversationPrint: moveToConversationPrint,
        openMessage: openMessage,
        openConversation: openConversation,
        openDraftItem: openDraftItem,
        openTemplateItem: openTemplateItem,
        moveToConversation: moveToConversation,
        moveToMessage: moveToMessage,
        moveToDraftItem: moveToDraftItem,
        moveToTemplateItem: moveToTemplateItem,
        moveToInbox: moveToInbox,

        getParamsValue: getParamsValue,
        showCompleteActionHint: showCompleteActionHint,

        strHash: strHash,

        canViewInDocuments: canViewInDocuments,
        canEditInDocuments: canEditInDocuments,
        canViewAsCalendar: canViewAsCalendar,
        fixMailtoLinks: fixMailtoLinks,
        isIe: isIe,
        getAttachmentDownloadUrl: getAttachmentDownloadUrl,
        getViewDocumentUrl: getViewDocumentUrl,
        getEditDocumentUrl: getEditDocumentUrl,
        getContactPhototUrl: getContactPhototUrl,

        wordWrap: wordWrap,
        htmlEncode: htmlEncode,
        htmlDecode: htmlDecode,
        copy: iterationCopy,

        checkAnchor: checkAnchor,
        disableButton: disableButton,
        disableInput: disableInput,
        setRequiredHint: setRequiredHint,
        setRequiredError: setRequiredError,
        isRequiredErrorVisible: isRequiredErrorVisible,
        isPopupVisible: isPopupVisible,
        getDateFormated: getDateFormated,
        getMapUrl: getMapUrl,
        getAccountErrorFooter: getAccountErrorFooter,
        getFileIconByExt: getFileIconByExt,
        getOAuthFaqLink: getOAuthFaqLink,

        resizeContent: resizeContent,
        resizeUserFolders: resizeUserFolders,

        hideAllActionPanels: hideAllActionPanels,

        getRandomId: getRandomId,
        scrollTop: scrollTop,

        isTemplate: isTemplate
    };
})(jQuery);