/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.TMMail = (function($) {
    var 
    isInit = false,
    lastItems = 29,
    plusItems = 29,
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
    constants = {
        pageTitle: '',
        pageHeader: ''
    },
    reEmail = /(([\w-\s]+)|([\w-]+(?:\.[\w-]+)*)|([\w-\s]+)([\w-]+(?:\.[\w-]+)*))(@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-zA-Z]{2,7}(?:\.[a-zA-Z]{2})?))|(@\[?((25[0-5]\.|2[0-4][0-9]\.|1[0-9]{2}\.|[0-9]{1,2}\.))((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\.){2}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\]?)/,
    reEmailStrict = /^([\w-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,7}|[0-9]{1,3})(\]?)$/,
    optionCookieName = 'tmmail',
    headerSeparator = ' - ',
    optionSeparator = '&',
    last_time_modified_all = 0, // Means the date and time the message list on server modified. This value comes from server and independent on folder.

    max_word_length = 10, //like google

    messages_modify_date = new Date(0),

    systemFolders = {
        inbox: { id: 1, name: 'inbox', displayName: ASC.Mail.Resources.MailResource.FolderNameInbox, last_time_modified: 0 },
        sent: { id: 2, name: 'sent', displayName: ASC.Mail.Resources.MailResource.FolderNameSent, last_time_modified: 0 },
        drafts: { id: 3, name: 'drafts', displayName: ASC.Mail.Resources.MailResource.FolderNameDrafts, last_time_modified: 0 },
        trash: { id: 4, name: 'trash', displayName: ASC.Mail.Resources.MailResource.FolderNameTrash, last_time_modified: 0 },
        spam: { id: 5, name: 'spam', displayName: ASC.Mail.Resources.MailResource.FolderNameSpam, last_time_modified: 0 }
    },
    action_types = {
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

        teamlab: /^tlcontact\/?(.+)*/,
        crm: /^crmcontact\/?(.+)*/,
        helpcenter: /^help(?:=(\d+))?/,

        common: /(.+)*/
    };

    var init = function(serviceCheckTimeout, crm_available, tl_available) {
        if (isInit === true) return;

        isInit = true;
        loadOptions();

        var timeout_ms = (+serviceCheckTimeout || 30)*1000;
        watchdog.init(timeout_ms);
        serviceManager.init(timeout_ms);

        constants.pageTitle = document.title;
        constants.pageHeader = ASC.Mail.Resources.MailResource.MailTitle || 'Mail';

        availability.CRM = crm_available;
        availability.People = tl_available;

        serviceManager.bind(window.Teamlab.events.getMailFolderModifyDate, onGetMailFolderModifyDate);
        ASC.Controls.AnchorController.bind(TMMail.anchors.common, checkAnchor);
    };

    function isLocalStorageAvailable() {
        try {
            return 'localStorage' in window && window['localStorage'] !== null;
        } catch (e) {
            return false;
        }
    }

    function onGetMailFolderModifyDate(params, date, errors) {
        var folder = GetSysFolderById(params.folder_id);
        folder.modified_date = date;
    }

    // Get current page last time items list modified on the server (the value that was last get from the server for this folder)
    function GetFolderModifyDate(folder_id) {
        var folder = GetSysFolderById(folder_id);
        return folder.modified_date;
    }


    var setPageHeaderFolderName = function(folder_id) {
        // ToDo: fix this workaround for 'undefined' in title
        // case: open conversation by direct link, and 'undefined' word will appear in page title
        if (0 == folder_id)
            return;

        var unread = $('#foldersContainer').children('[folderid=' + folder_id + ']').attr('unread');

        var title = (unread == 0 || unread == undefined ? "" : ('(' + unread + ') ')) + GetSysFolderDisplayNameById(folder_id);

        setPageHeaderTitle(title);
    };

    var setPageHeaderTitle = function(title) {
         title = translateSymbols(title) + headerSeparator + constants.pageTitle;

        if ($.browser.msie) {
            setImmediate(function () {
                document.title = title;
            });
        } else {
            document.title = title;
        }
    };

    var constant = function(name) {
        return constants[name];
    };

    var option = function(name, value, needExecEvent) {
        if (typeof name !== 'string') {
            return undefined;
        }
        if (typeof value === 'undefined') {
            return options[name];
        }
        var oldValue = options[name];
        options[name] = value;
        saveOptions();
        return value;
    };

    var loadOptions = function() {
        var 
      fieldSeparator = ':',
      pos = -1,
      name = '',
      value = '',
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
        var 
      fieldSeparator = ':',
      collect = [];
        for (var name in options) {
            if (options.hasOwnProperty(name)) {
                collect.push(name + fieldSeparator + options[name]);
            }
        }
        ASC.Mail.cookies.set(optionCookieName, collect.join(optionSeparator));
    };

    function trimString(str) {
        return str.replace(/^\s+|\s+$/g, '');
    }

    function trimArray(arr) {
        var 
      newArray = [];
        for (i = 0, n = arr.length; i < n; i++) {
            if (arr[i]) {
                newArray.push(arr[i]);
            }
        }
        return newArray;
    }

    function trim(o) {
        if (o instanceof Array) {
            return trimArray(o);
        } else {
            return trimString(o.toString());
        }
    }

    function ltgt(str) {
        if (str.indexOf('<') !== false || str.indexOf('>') !== false) str = str.replace(/</g, '&lt;').replace(/>/g, '&gt;');
        return str;
    }

    function in_array(needle, haystack, strict) {
        var found = false, key, strict = !!strict;
        for (key in haystack) {
            if ((strict && haystack[key] === needle) || (!strict && haystack[key] == needle)) {
                found = true;
                break;
            }
        }
        return found;
    }


    function translateSymbols(str, toText) {
        var 
      symbols = [
        ['&lt;', '<'],
        ['&gt;', '>']
      ];

        if (typeof str !== 'string') {
            return '';
        }

        if (typeof toText === 'undefined' || toText) {
            var symInd = symbols.length;
            while (symInd--) {
                str = str.replace(new RegExp(symbols[symInd][0], 'g'), symbols[symInd][1]);
            }
        } else {
            var symInd = symbols.length;
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
            anchorRegExp.prev_conversation.test(anchor));
    };

    var pageIs = function(pageType) {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        switch (pageType) {
            case 'message':
                if (anchorRegExp.message.test(anchor))
                    return true;
                break;
            case 'inbox':
                if (anchorRegExp.inbox.test(anchor) || anchor == '')
                    return true;
                break;
            case 'sent':
                if (anchorRegExp.sent.test(anchor))
                    return true;
                break;
            case 'drafts':
                if (anchorRegExp.drafts.test(anchor))
                    return true;
                break;
            case 'trash':
                if (anchorRegExp.trash.test(anchor))
                    return true;
                break;
            case 'spam':
                if (anchorRegExp.spam.test(anchor))
                    return true;
                break;
            case 'compose':
                if (anchorRegExp.compose.test(anchor))
                    return true;
                break;
            case 'composeto':
                if (anchorRegExp.composeto.test(anchor))
                    return true;
                break;
            case 'draftitem':
                if (anchorRegExp.draftitem.test(anchor))
                    return true;
                break;
            case 'reply':
                if (anchorRegExp.reply.test(anchor))
                    return true;
                break;
            case 'forward':
                if (anchorRegExp.forward.test(anchor))
                    return true;
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
            case 'conversation':
                if (anchorRegExp.conversation.test(anchor))
                    return true;
                break;
            case 'helpcenter':
                if (anchorRegExp.helpcenter.test(anchor)) {
                    return true;
                }
                break;
        }
        return false;
    };

    var GetSysFolderNameById = function(sysfolder_id, default_value) {
        var sysfolder = GetSysFolderById(sysfolder_id);
        return typeof sysfolder != 'undefined' ? sysfolder.name : default_value;
    };

    var GetSysFolderDisplayNameById = function(sysfolder_id, default_value) {
        var sysfolder = GetSysFolderById(sysfolder_id);
        return typeof sysfolder != 'undefined' ? sysfolder.displayName : default_value;
    };

    var GetSysFolderIdByName = function(sysfolder_name, default_value) {
        for (var sysfolder_name_in in systemFolders) {
            var sysfolder = systemFolders[sysfolder_name_in];
            if (sysfolder.name == sysfolder_name) {
                return sysfolder.id;
            }
        }
        return default_value;
    };

    // private
    var GetSysFolderById = function(sysfolder_id) {
        for (var sysfolder_name in systemFolders) {
            var sysfolder = systemFolders[sysfolder_name];
            if (sysfolder.id == +sysfolder_id) {
                return sysfolder;
            }
        }
        return undefined;
    };

    // Get current page last time items list modified on the server (the value that was last get from the server for this folder)
    var GetLastTimeServerListModifiedForFolder = function(folder_id) {
        var sysfolder = GetSysFolderById(folder_id);
        if (typeof sysfolder != 'undefined') {
            return sysfolder.last_time_modified;
        }
        return null;
    };

    // Set current page last time items list modified on the server (the value that was last get from the server for this folder)
    var SetLastTimeServerListModifiedForFolder = function(time_value, folder_id) {
        var sysfolder = GetSysFolderById(folder_id);
        if (typeof sysfolder != 'undefined') {
            sysfolder.last_time_modified = time_value;
        }
    };

    var ExtractFolderIdFromAnchor = function() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        if (anchor === "") {
            return systemFolders.inbox.id;
        }
        var sysfolder_res = anchorRegExp.sysfolders.exec(anchor);
        if (sysfolder_res != null) {
            return GetSysFolderIdByName(sysfolder_res[1]);
        }
        return 0;
    };

    var ExtractConversationIdFromAnchor = function() {
        var anchor = ASC.Controls.AnchorController.getAnchor();

        if (anchor !== "") {
            var conversationId = anchorRegExp.conversation.exec(anchor);
            if (conversationId != null)
                return conversationId[1];
        }

        return 0;
    };

    var getSupportLink = function() {
        return window.MailSupportUrl;
    };

    var getFaqLink = function(address) {
        address = address || "";
        var anchor = "";
        if (/@gmail\./.test(address.toLowerCase()) || /@googlemail\./.test(address.toLowerCase()))
            anchor = "#IssueswithGmailcomService_block";
        if (/@hotmail\./.test(address.toLowerCase()) || /@live\./.test(address.toLowerCase())
         || /@msn\./.test(address.toLowerCase()) || /@outlook\./.test(address.toLowerCase())
         || /@yahoo\./.test(address.toLowerCase()))
            anchor = "#IssueswithHotmailcomandYahoocomServices_block";
        if (/@mail\.ru/.test(address.toLowerCase()))
            anchor = "#IssueswithMailruService_block";
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

    var moveToInbox = function() {
        ASC.Controls.AnchorController.move(systemFolders.inbox.name);
    };

    var openMessage = function(id) {
        window.open('#message/' + id, '_blank');
    };

    function moveToConversation (id, safe) {
        var anchor = '#conversation/' + id;
        if (safe)
            ASC.Controls.AnchorController.safemove(anchor);
        else
            ASC.Controls.AnchorController.move(anchor);
    };

    function moveToMessage (id, safe) {
        var anchor = '#message/' + id;
        if (safe)
            ASC.Controls.AnchorController.safemove(anchor);
        else
            ASC.Controls.AnchorController.move(anchor);
    };

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
            var new_word = '';
            var last_index = 0;
            for (var j = max_word_length; j < words[i].length; j += max_word_length) {
                new_word += htmlEncode(words[i].slice(j - max_word_length, j)) + '<wbr/>';
                last_index = j;
            }
            if (last_index > 0) {
                new_word += htmlEncode(words[i].slice(last_index));
                words[i] = new_word;
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

    function showCompleteActionHint(action_type, is_conversation, count, dst_folder_id) {
        var hint_text = '';
        var folder_name = TMMail.GetSysFolderDisplayNameById(dst_folder_id, '');
        switch (action_type) {
        case TMMail.action_types.move:
            hint_text =
                count == 1 ?
                    (is_conversation ?
                        window.MailActionCompleteResource.moveOneConversationTo :
                        window.MailActionCompleteResource.moveOneMessageTo)
                        .replace('%folder%', folder_name) :
                    (is_conversation ?
                        window.MailActionCompleteResource.moveManyConversationsTo :
                        window.MailActionCompleteResource.moveManyMessagesTo)
                        .replace('%folder%', folder_name)
                        .replace('%count%', count);
            break;
        case TMMail.action_types.restore:
            hint_text =
                count == 1 ?
                    (is_conversation ?
                        window.MailActionCompleteResource.restoreOneConversationTo :
                        window.MailActionCompleteResource.restoreOneMessageTo)
                        .replace('%folder%', folder_name) :
                    (is_conversation ?
                        window.MailActionCompleteResource.restoreManyConversations :
                        window.MailActionCompleteResource.restoreManyMessages)
                        .replace('%count%', count);
            break;
        case TMMail.action_types.delete_messages:
            hint_text =
                count == 1 ?
                    (is_conversation ?
                        window.MailActionCompleteResource.deleteOneConversation :
                        window.MailActionCompleteResource.deleteOneMessage)
                    :
                    (is_conversation ?
                        window.MailActionCompleteResource.deleteManyConversations :
                        window.MailActionCompleteResource.deleteManyMessages)
                        .replace('%count%', count);
            break;
        case TMMail.action_types.clear_folder:
            hint_text = window.MailActionCompleteResource.clearFolder.replace('%folder%', folder_name);
            break;
        case TMMail.action_types.move_filtered:
            hint_text = is_conversation ?
                window.MailActionCompleteResource.moveFilteredConversationsTo.replace('%folder%', folder_name) :
                window.MailActionCompleteResource.moveFilteredMessagesTo.replace('%folder%', folder_name);
            break;
        case TMMail.action_types.restore_filtered:
            hint_text = is_conversation ?
                window.MailActionCompleteResource.restoreFilteredConversations :
                window.MailActionCompleteResource.restoreFilteredMessages;
            break;
        case TMMail.action_types.delete_filtered:
            hint_text = is_conversation ?
                window.MailActionCompleteResource.deleteFilteredConversations :
                window.MailActionCompleteResource.deleteFilteredMessages;
            break;
        default:
            return;
        }

        setTimeout(function() {
            window.LoadingBanner.hideLoading();
            window.toastr.success(hint_text);
        }, 1000);
    }
    
    var str_hash = function(str) {
        var hash = 0, i, l;
        if (str.length == 0) return hash;
        for (i = 0, l = str.length; i < l; i++) {
            hash  = ((hash<<5)-hash)+str.charCodeAt(i);
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
        element.find("a[href*='mailto:']").click(function () {
            messagePage.setToEmailAddresses([$(this).attr('href').substr(7, $(this).attr('href').length - 1)]);
            window.location.href = "#composeto";
            return false;
        });
    }

    function isIe() {
        var ua = navigator.userAgent;
        return ua.match(/Trident\/7\./) || ua.match(/MSIE *\d+\.\w+/i);
    }

    function getAttachmentDownloadUrl(attachment_id) {
      return window.MailDownloadHandlerUri.format(attachment_id);
    }
    
    function getAttachmentsDownloadAllUrl(message_id) {
        return window.MailDownloadAllHandlerUri.format(message_id);
    }
    
    function getViewDocumentUrl(attachment_id) {
        return window.MailViewDocumentHandlerUri.format(attachment_id);
    }
    
    function getEditDocumentUrl(attachment_id) {
        return window.MailEditDocumentHandlerUri.format(attachment_id);
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

        var result = (!value) ? value : String(value).replace(regex, function (match, capture) {
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

    return {
        reEmail: reEmail,
        reEmailStrict: reEmailStrict,
        sysfolders: systemFolders,
        action_types: action_types,
        anchors: anchorRegExp,

        init: init,
        option: option,
        availability: availability,
        lastItems: lastItems,
        plusItems: plusItems,
        constant: constant,
        saveMessageInterval: saveMessageInterval,

        last_time_modified_all: last_time_modified_all,
        GetLastTimeServerListModifiedForFolder: GetLastTimeServerListModifiedForFolder,
        SetLastTimeServerListModifiedForFolder: SetLastTimeServerListModifiedForFolder,

        ltgt: ltgt,
        in_array: in_array,
        translateSymbols: translateSymbols,
        setPageHeaderFolderName: setPageHeaderFolderName,
        setPageHeaderTitle: setPageHeaderTitle,
        getErrorMessage: getErrorMessage,
        pageIs: pageIs,
        isInvalidPage: isInvalidPage,
        GetSysFolderNameById: GetSysFolderNameById,
        GetSysFolderIdByName: GetSysFolderIdByName,
        GetSysFolderDisplayNameById: GetSysFolderDisplayNameById,
        ExtractFolderIdFromAnchor: ExtractFolderIdFromAnchor,
        ExtractConversationIdFromAnchor: ExtractConversationIdFromAnchor,

        GetFolderModifyDate: GetFolderModifyDate,
        messages_modify_date: messages_modify_date,

        getFaqLink: getFaqLink,
        getSupportLink: getSupportLink,

        moveToReply: moveToReply,
        moveToReplyAll: moveToReplyAll,
        moveToForward: moveToForward,
        openMessage: openMessage,
        moveToConversation: moveToConversation,
        moveToMessage: moveToMessage,
        moveToInbox: moveToInbox,

        parseEmailFromFullAddress: parseEmailFromFullAddress,
        parseFullNameFromFullAddress: parseFullNameFromFullAddress,
        getParamsValue: getParamsValue,
        showCompleteActionHint: showCompleteActionHint,

        strHash: str_hash,
        prepareUrlToDocument: prepareUrlToDocument,
        canViewInDocuments: canViewInDocuments,
        canEditInDocuments: canEditInDocuments,
        fixMailtoLinks: fixMailtoLinks,
        isIe: isIe,
        isLocalStorageAvailable: isLocalStorageAvailable,
        getAttachmentDownloadUrl: getAttachmentDownloadUrl,
        getAttachmentsDownloadAllUrl: getAttachmentsDownloadAllUrl,
        getViewDocumentUrl: getViewDocumentUrl,
        getEditDocumentUrl: getEditDocumentUrl,

        wordWrap: wordWrap,
        htmlEncode: htmlEncode,
        htmlDecode: htmlDecode,

        checkAnchor: checkAnchor
    };
})(jQuery);