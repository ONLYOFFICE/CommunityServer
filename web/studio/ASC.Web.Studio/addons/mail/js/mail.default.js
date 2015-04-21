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


window.TMMail = (function($) {
    var isInit = false,
        lastItems = 29,
        plusItems = 29,
        requiredFieldErrorCss = "requiredFieldError",
        options = {
            MessagesPageSize: 25,
            ContactsPageSize: 25,
            ConversationSortAsc: "true"
        },
        availability = {
            CRM: true,
            People: true
        },
        saveMessageInterval = 5000, // 5 seconds for autosave
        showNextAlertTimeout = 60000,
        serviceCheckInterval = 30000,
        constants = {
            pageTitle: '',
            pageHeader: ''
        },
        reEmail = /(([\w-\s]+)|([\w-]+(?:\.[\w-]+)*)|([\w-\s]+)([\w-]+(?:\.[\w-]+)*))(@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-zA-Z]{2,7}(?:\.[a-zA-Z]{2})?))|(@\[?((25[0-5]\.|2[0-4][0-9]\.|1[0-9]{2}\.|[0-9]{1,2}\.))((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\.){2}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\]?)/,
        reEmailStrict = /^([\w-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,7}|[0-9]{1,3})(\]?)$/,
        reMailServerEmailStrict = /^([a-zA-Z0-9]+)([-\.\_][a-zA-Z0-9]+)*@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,7}|[0-9]{1,3})(\]?)$/,
        reDomainStrict = /(?=^.{5,254}$)(^(?:(?!\d+\.)[a-zA-Z0-9_\-]{1,63}\.?)+\.(?:[a-zA-Z]{2,})$)/,
        optionCookieName = 'tmmail',
        headerSeparator = ' - ',
        optionSeparator = '&',
        lastTimeModifiedAll = 0, // Means the date and time the message list on server modified. This value comes from server and independent on folder.

        maxWordLength = 10, //like google

        messagesModifyDate = new Date(0),
        systemFolders = {
            inbox: { id: 1, name: 'inbox', displayName: ASC.Mail.Resources.MailResource.FolderNameInbox, last_time_modified: 0 },
            sent: { id: 2, name: 'sent', displayName: ASC.Mail.Resources.MailResource.FolderNameSent, last_time_modified: 0 },
            drafts: { id: 3, name: 'drafts', displayName: ASC.Mail.Resources.MailResource.FolderNameDrafts, last_time_modified: 0 },
            trash: { id: 4, name: 'trash', displayName: ASC.Mail.Resources.MailResource.FolderNameTrash, last_time_modified: 0 },
            spam: { id: 5, name: 'spam', displayName: ASC.Mail.Resources.MailResource.FolderNameSpam, last_time_modified: 0 }
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
            sysfolders: /^$|^(inbox|sent|drafts|trash|spam)\/?(.+)*/,

            inbox: /^$|^inbox\/?(.+)*/,
            sent: /^sent\/?(.+)*/,
            drafts: /^drafts\/?(.+)*/,
            trash: /^trash\/?(.+)*/,
            spam: /^spam\/?(.+)*/,

            reply: /^reply\/(\d+)\/?$/,
            forward: /^forward\/(\d+)\/?$/,
            compose: /^compose\/?$/,
            composeto: /^composeto\/?(.+)*/,
            replyAll: /^replyAll\/(\d+)$/,
            writemessage: /^(compose|composeto|draftitem|forward|reply|replyAll)\/?(.+)*/,

            message: /^message\/(\d+)\/?$/,
            next_message: /^message\/(\d+)\/next\/?$/,
            prev_message: /^message\/(\d+)\/prev\/?$/,

            conversation: /^conversation\/(\d+)\/?$/,
            next_conversation: /^conversation\/(\d+)\/next\/?$/,
            prev_conversation: /^conversation\/(\d+)\/prev\/?$/,

            draftitem: /^draftitem\/(\d+)\/?$/,

            accounts: /^accounts\/?$/,
            tags: /^tags\/?$/,
            administration: /^administration\/?$/,

            teamlab: /^tlcontact\/?(.+)*/,
            crm: /^crmcontact\/?(.+)*/,
            helpcenter: /^help(?:=(\d+))?/,

            common: /(.+)*/,

            messagePrint: /^message\/print\/(\d+)\/?(.+)*/,
            conversationPrint: /^conversation\/print\/(\d+)\/?(.+)*/
        };

    var init = function(crmAvailable, tlAvailable) {
        if (isInit === true) {
            return;
        }

        isInit = true;
        loadOptions();

        serviceManager.init(serviceCheckInterval);

        constants.pageTitle = document.title;
        constants.pageHeader = ASC.Mail.Resources.MailResource.MailTitle || 'Mail';

        availability.CRM = crmAvailable;
        availability.People = tlAvailable;

        serviceManager.bind(window.Teamlab.events.getMailFolderModifyDate, onGetMailFolderModifyDate);
        ASC.Controls.AnchorController.bind(TMMail.anchors.common, checkAnchor);
    };

    function onGetMailFolderModifyDate(params, date) {
        var folder = getSysFolderById(params.folder_id);
        folder.modified_date = date;
    }

    // Get current page last time items list modified on the server (the value that was last get from the server for this folder)

    function getFolderModifyDate(folderId) {
        var folder = getSysFolderById(folderId);
        return folder.modified_date;
    }


    var setPageHeaderFolderName = function(folderId) {
        // ToDo: fix this workaround for 'undefined' in title
        // case: open conversation by direct link, and 'undefined' word will appear in page title
        if (0 == folderId) {
            return;
        }

        var unread = $('#foldersContainer').children('[folderid=' + folderId + ']').attr('unread');

        var title = (unread == 0 || unread == undefined ? "" : ('(' + unread + ') ')) + getSysFolderDisplayNameById(folderId);

        setPageHeaderTitle(title);
    };

    var setPageHeaderTitle = function(title) {
        title = translateSymbols(title) + headerSeparator + constants.pageTitle;

        if ($.browser.msie) {
            setImmediate(function() {
                document.title = title;
            });
        } else {
            document.title = title;
        }
    };

    var constant = function(name) {
        return constants[name];
    };

    var option = function(name, value) {
        if (typeof name !== 'string') {
            return undefined;
        }
        if (typeof value === 'undefined') {
            return options[name];
        }
        options[name] = value;
        saveOptions();
        return value;
    };

    var loadOptions = function() {
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
    };

    var saveOptions = function() {
        var fieldSeparator = ':',
            collect = [];
        for (var name in options) {
            if (options.hasOwnProperty(name)) {
                collect.push(name + fieldSeparator + options[name]);
            }
        }
        ASC.Mail.cookies.set(optionCookieName, collect.join(optionSeparator));
    };

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


    function translateSymbols(str, toText) {
        var symbols = [
            ['&lt;', '<'],
            ['&gt;', '>']
        ];

        if (typeof str !== 'string') {
            return '';
        }
        var symInd;
        if (typeof toText === 'undefined' || toText) {
            symInd = symbols.length;
            while (symInd--) {
                str = str.replace(new RegExp(symbols[symInd][0], 'g'), symbols[symInd][1]);
            }
        } else {
            symInd = symbols.length;
            while (symInd--) {
                str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
            }
        }
        return str;
    }

    var getErrorMessage = function(errors) {
        var mes = [];
        mes.push('<ul class="errors">');
        for (var i = 0, n = errors.length; i < n; i++) {
            mes.push('<li class="error-message">' + errors[i] + '</li>');
        }
        mes.push('</ul>');
        return mes.join('');
    };

    var isInvalidPage = function() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        return !(anchorRegExp.message.test(anchor) || anchorRegExp.accounts.test(anchor) || anchorRegExp.teamlab.test(anchor) ||
            anchorRegExp.crm.test(anchor) || anchorRegExp.sysfolders.test(anchor) || anchorRegExp.writemessage.test(anchor) ||
            anchorRegExp.tags.test(anchor) || anchorRegExp.conversation.test(anchor) || anchorRegExp.helpcenter.test(anchor) ||
            anchorRegExp.next_message.test(anchor) || anchorRegExp.prev_message.test(anchor) || anchorRegExp.next_conversation.test(anchor) ||
            anchorRegExp.prev_conversation.test(anchor) || anchorRegExp.administration.test(anchor) ||
            anchorRegExp.messagePrint.test(anchor) || anchorRegExp.conversationPrint.test(anchor));
    };

    var pageIs = function(pageType) {
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
            case 'teamlab':
                if (anchorRegExp.teamlab.test(anchor)) {
                    return true;
                }
                break;
            case 'crm':
                if (anchorRegExp.crm.test(anchor)) {
                    return true;
                }
                break;
            case 'sysfolders':
                if (anchorRegExp.sysfolders.test(anchor) || anchor == '') {
                    return true;
                }
                break;
            case 'writemessage':
                if (anchorRegExp.writemessage.test(anchor)) {
                    return true;
                }
                break;
            case 'tags':
                if (anchorRegExp.tags.test(anchor)) {
                    return true;
                }
                break;
            case 'administration':
                if (anchorRegExp.administration.test(anchor)) {
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
            case 'messagePrint':
                if (anchorRegExp.messagePrint.test(anchor)) {
                    return true;
                }
            case 'conversationPrint':
                if (anchorRegExp.conversationPrint.test(anchor)) {
                    return true;
                }
                break;
        }
        return false;
    };

    var getSysFolderNameById = function(sysfolderId, defaultValue) {
        var sysfolder = getSysFolderById(sysfolderId);
        return typeof sysfolder != 'undefined' && sysfolder ? sysfolder.name : defaultValue;
    };

    var getSysFolderDisplayNameById = function(sysfolderId, defaultValue) {
        var sysfolder = getSysFolderById(sysfolderId);
        return typeof sysfolder != 'undefined' && sysfolder ? sysfolder.displayName : defaultValue;
    };

    var getSysFolderIdByName = function(sysfolderName, defaultValue) {
        for (var sysfolderNameIn in systemFolders) {
            var sysfolder = systemFolders[sysfolderNameIn];
            if (sysfolder.name == sysfolderName) {
                return sysfolder.id;
            }
        }
        return defaultValue;
    };

    // private
    var getSysFolderById = function(sysfolderId) {
        for (var sysfolderName in systemFolders) {
            var sysfolder = systemFolders[sysfolderName];
            if (sysfolder.id == +sysfolderId) {
                return sysfolder;
            }
        }
        return undefined;
    };

    // Get current page last time items list modified on the server (the value that was last get from the server for this folder)
    var getLastTimeServerListModifiedForFolder = function(folderId) {
        var sysfolder = getSysFolderById(folderId);
        if (typeof sysfolder != 'undefined' && sysfolder) {
            return sysfolder.last_time_modified;
        }
        return null;
    };

    // Set current page last time items list modified on the server (the value that was last get from the server for this folder)
    var setLastTimeServerListModifiedForFolder = function(timeValue, folderId) {
        var sysfolder = getSysFolderById(folderId);
        if (typeof sysfolder != 'undefined' && sysfolder) {
            sysfolder.last_time_modified = timeValue;
        }
    };

    var extractFolderIdFromAnchor = function() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        if (anchor === "") {
            return systemFolders.inbox.id;
        }
        var sysfolderRes = anchorRegExp.sysfolders.exec(anchor);
        if (sysfolderRes != null) {
            return getSysFolderIdByName(sysfolderRes[1]);
        }
        return 0;
    };

    var extractConversationIdFromAnchor = function() {
        var anchor = ASC.Controls.AnchorController.getAnchor();

        if (anchor !== "") {
            var conversationId = anchorRegExp.conversation.exec(anchor);
            if (conversationId != null) {
                return conversationId[1];
            }
        }

        return 0;
    };

    var getSupportLink = function() {
        return window.MailSupportUrl;
    };

    var getFaqLink = function(address) {
        address = address || "";
        var anchor = "";
        if (/@gmail\./.test(address.toLowerCase()) || /@googlemail\./.test(address.toLowerCase())) {
            anchor = "#IssueswithGmailcomService_block";
        }
        if (/@hotmail\./.test(address.toLowerCase()) || /@live\./.test(address.toLowerCase())
            || /@msn\./.test(address.toLowerCase()) || /@outlook\./.test(address.toLowerCase())
            || /@yahoo\./.test(address.toLowerCase())) {
            anchor = "#IssueswithHotmailcomandYahoocomServices_block";
        }
        if (/@mail\.ru/.test(address.toLowerCase())) {
            anchor = "#IssueswithMailruService_block";
        }
        return window.MailFaqUri + anchor;
    };

    var moveToReply = function(msgid) {
        ASC.Controls.AnchorController.move('#reply/' + msgid);
    };

    var moveToReplyAll = function(msgid) {
        ASC.Controls.AnchorController.move('#replyAll/' + msgid);
    };

    var moveToForward = function(msgid) {
        ASC.Controls.AnchorController.move('#forward/' + msgid);
    };

    var moveToMessagePrint = function(messageId, showImages) {
        if (showImages) {
            window.open(window.location.href.split('#')[0] + '?blankpage=true#message/print/' + messageId + '?sim=' + messageId);
        }
        else {
            window.open(window.location.href.split('#')[0] + '?blankpage=true#message/print/' + messageId);
        }
    };

    var moveToConversationPrint = function (conversationId, simIds) {
        var href = window.location.href.split('#')[0] + '?blankpage=true#conversation/print/' + conversationId;
        if (simIds && simIds.length)
            href += '?sim=' + simIds.join(",");
        window.open(href);
    };

    var moveToInbox = function() {
        ASC.Controls.AnchorController.move(systemFolders.inbox.name);
    };

    var openMessage = function(id) {
        window.open('#message/' + id, '_blank');
    };

    var openConversation = function(id) {
        window.open('#conversation/' + id, '_blank');
    };

    function openDraftItem(id) {
        window.open('#draftitem/' + id, '_blank');
    }

    function moveToConversation(id, safe) {
        var anchor = '#conversation/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    };

    function moveToMessage(id, safe) {
        var anchor = '#message/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    }

    ;

    function moveToDraftItem(id, safe) {
        var anchor = '#draftitem/' + id;
        if (safe) {
            ASC.Controls.AnchorController.safemove(anchor);
        } else {
            ASC.Controls.AnchorController.move(anchor);
        }
    }

    ;

    var parseEmailFromFullAddress = function(from) {
        var res = (/^.*<([^<^>]+)>$/).exec(from);
        return (res != null) && (res.length == 2) ? res[1] : from;
    };

    var parseFullNameFromFullAddress = function(from) {
        var res = (/^"(.+)" <[^<^>]+>$/).exec(from);
        return (res != null) && (res.length == 2) ? res[1] : "";
    };

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

    var getParamsValue = function(params, reg) {
        var myArray = reg.exec(params);
        if (myArray === null) {
            return undefined;
        }
        return myArray[1];
    };

    function showCompleteActionHint(actionType, isConversation, count, dstFolderId) {
        var hintText;
        var folderName = TMMail.GetSysFolderDisplayNameById(dstFolderId, '');
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

    var strHash = function(str) {
        var hash = 0, i, l;
        if (str.length == 0) {
            return hash;
        }
        for (i = 0, l = str.length; i < l; i++) {
            hash = ((hash << 5) - hash) + str.charCodeAt(i);
            hash |= 0; // Convert to 32bit integer
        }
        return hash;
    };

    function prepareUrlToDocument(url) {
        return decodeURIComponent(url).replace('+', '%2b').replace('#', '%23');
    }

    function canViewInDocuments(url) {
        return ASC.Files.Utility.CanWebView(url) && ASC.Resources.Master.TenantTariffDocsEdition; // && url.match(/.ods$/) == null;
    }

    function canEditInDocuments(url) {
        return ASC.Files.Utility.CanWebEdit(url) && ASC.Resources.Master.TenantTariffDocsEdition; // && url.match(/.ods$/) == null;
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
        return window.MailDownloadHandlerUri.format(attachmentId);
    }

    function getAttachmentsDownloadAllUrl(messageId) {
        return window.MailDownloadAllHandlerUri.format(messageId);
    }

    function getViewDocumentUrl(attachmentId) {
        return window.MailViewDocumentHandlerUri.format(attachmentId);
    }

    function getEditDocumentUrl(attachmentId) {
        return window.MailEditDocumentHandlerUri.format(attachmentId);
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

    function checkAnchor() {
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

    return {
        reEmail: reEmail,
        reEmailStrict: reEmailStrict,
        reMailServerEmailStrict: reMailServerEmailStrict,
        reDomainStrict: reDomainStrict,
        sysfolders: systemFolders,
        action_types: actionTypes,
        anchors: anchorRegExp,

        init: init,
        option: option,
        availability: availability,
        lastItems: lastItems,
        plusItems: plusItems,
        constant: constant,
        saveMessageInterval: saveMessageInterval,
        showNextAlertTimeout: showNextAlertTimeout,
        serviceCheckInterval: serviceCheckInterval,

        last_time_modified_all: lastTimeModifiedAll,
        GetLastTimeServerListModifiedForFolder: getLastTimeServerListModifiedForFolder,
        SetLastTimeServerListModifiedForFolder: setLastTimeServerListModifiedForFolder,

        ltgt: ltgt,
        in_array: inArray,
        translateSymbols: translateSymbols,
        setPageHeaderFolderName: setPageHeaderFolderName,
        setPageHeaderTitle: setPageHeaderTitle,
        getErrorMessage: getErrorMessage,
        pageIs: pageIs,
        isInvalidPage: isInvalidPage,
        GetSysFolderNameById: getSysFolderNameById,
        GetSysFolderIdByName: getSysFolderIdByName,
        GetSysFolderDisplayNameById: getSysFolderDisplayNameById,
        ExtractFolderIdFromAnchor: extractFolderIdFromAnchor,
        ExtractConversationIdFromAnchor: extractConversationIdFromAnchor,

        GetFolderModifyDate: getFolderModifyDate,
        messages_modify_date: messagesModifyDate,

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
        moveToConversation: moveToConversation,
        moveToMessage: moveToMessage,
        moveToDraftItem: moveToDraftItem,
        moveToInbox: moveToInbox,

        parseEmailFromFullAddress: parseEmailFromFullAddress,
        parseFullNameFromFullAddress: parseFullNameFromFullAddress,
        getParamsValue: getParamsValue,
        showCompleteActionHint: showCompleteActionHint,

        strHash: strHash,
        prepareUrlToDocument: prepareUrlToDocument,
        canViewInDocuments: canViewInDocuments,
        canEditInDocuments: canEditInDocuments,
        fixMailtoLinks: fixMailtoLinks,
        isIe: isIe,
        getAttachmentDownloadUrl: getAttachmentDownloadUrl,
        getAttachmentsDownloadAllUrl: getAttachmentsDownloadAllUrl,
        getViewDocumentUrl: getViewDocumentUrl,
        getEditDocumentUrl: getEditDocumentUrl,

        wordWrap: wordWrap,
        htmlEncode: htmlEncode,
        htmlDecode: htmlDecode,

        checkAnchor: checkAnchor,
        disableButton: disableButton,
        disableInput: disableInput,
        setRequiredHint: setRequiredHint,
        setRequiredError: setRequiredError,
        isRequiredErrorVisible: isRequiredErrorVisible,
        isPopupVisible: isPopupVisible
    };
})(jQuery);