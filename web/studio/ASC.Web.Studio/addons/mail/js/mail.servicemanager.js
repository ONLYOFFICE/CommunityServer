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


if (typeof window.serviceManager === 'undefined') {
    var xmlEncode = function(s) {
        return s.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    };

    window.serviceManager = (function($) {
        var isInit = false;

        var getNodeContent = function(o) {
            if (!o || typeof o !== 'object') {
                return '';
            }
            return o.text || o.textContent || (function() {
                var result = '',
                    childrens = o.childNodes;
                if (!childrens) {
                    return result;
                }
                for (var i = 0, n = childrens.length; i < n; i++) {
                    var child = childrens.item(i);
                    switch (child.nodeType) {
                        case 1:
                        case 5:
                            result += arguments.callee(child);
                            break;
                        case 3:
                        case 2:
                        case 4:
                            result += child.nodeValue;
                            break;
                        default:
                            break;
                    }
                }
                return result;
            })(o);
        };

        var init = function(timeoutMs) {
            if (true === isInit) {
                return;
            }

            isInit = true;
            if (!ASC.Resources.Master.Hub.Url) {
                setInterval(function() {
                    updateFolders(undefined, TMMail.messages_modify_date, TMMail.GetFolderModifyDate(MailFilter.getFolder()));
                }, timeoutMs);
                setInterval(function () {
                    getAccounts();
                }, timeoutMs * 10);
            }
        };

        var bind = function(event, handler) {
            return window.Teamlab.bind(event, handler);
        };

        var unbind = function() {
            return window.Teamlab.unbind.apply(this, arguments);
        };

        var hideCallback = function() {
            window.LoadingBanner.hideLoading();
        };

        var wrapHideCallback = function(orig) {
            return function() {
                window.LoadingBanner.hideLoading();
                orig.apply(this, arguments);
            };
        };

        var wrapper = function(paramsCount, orig) {
            return function() {
                var loadingMessage = arguments.length > paramsCount ? arguments[arguments.length - 1] : undefined;

                if (loadingMessage) {
                    window.LoadingBanner.strLoading = loadingMessage;
                    window.LoadingBanner.displayMailLoading(true, true);

                    var options = arguments[arguments.length - 2] || {};
                    options.success = options.hasOwnProperty('success') && typeof options.success === 'function' ? wrapHideCallback(options.success) : hideCallback;
                    options.error = options.hasOwnProperty('error') && typeof options.error === 'function' ? wrapHideCallback(options.error) : hideCallback;
                }
                return orig.apply(this, arguments);
            };
        };

        var updateFolders = function(loadingMessage, messagesModifyDate, folderModifyDate) {
            var options = {};
            if (loadingMessage) {
                window.LoadingBanner.strLoading = loadingMessage;
                window.LoadingBanner.displayMailLoading(true, true);

                options.success = hideCallback;
                options.error = hideCallback;
            }

            if (!messagesModifyDate) {
                messagesModifyDate = new Date(0);
            }

            if (!folderModifyDate) {
                folderModifyDate = new Date(0);
            }

            window.Teamlab.getMailFilteredConversations({ folder_id: MailFilter.getFolder() },
                $.extend({}, MailFilter.toData(), { last_check_date: folderModifyDate }),
                {});
            window.Teamlab.getMailFolders({}, messagesModifyDate, {});
            window.Teamlab.getMailMessagesModifyDate({}, {});
            window.Teamlab.getMailFolderModifyDate({ folder_id: MailFilter.getFolder() }, MailFilter.getFolder(), options);
        };

        var getTags = wrapper(2, function(params, options) {
            window.Teamlab.getMailTags(params, options);
        });

        var createTag = wrapper(5, function(name, style, addresses, params, options) {
            window.Teamlab.createMailTag(params, name, style, addresses, options);
        });

        var updateTag = wrapper(6, function(id, name, style, addresses, params, options) {
            window.Teamlab.updateMailTag(params, id, name, style, addresses, options);
        });

        var deleteTag = wrapper(3, function(id, params, options) {
            window.Teamlab.removeMailTag(params, id, options);
        });

        var setTag = wrapper(4, function(messageIds, tagId, params, options) {
            window.Teamlab.setMailTag(params, messageIds, tagId, options);
        });

        var setConverstationsTag = wrapper(4, function(messageIds, tagId, params, options) {
            window.Teamlab.setMailConversationsTag(params, messageIds, tagId, options);
        });

        var unsetTag = wrapper(4, function(messageIds, tagId, params, options) {
            window.Teamlab.unsetMailTag(params, messageIds, tagId, options);
        });

        var unsetConverstationsTag = wrapper(4, function(messageIds, tagId, params, options) {
            window.Teamlab.unsetMailConversationsTag(params, messageIds, tagId, options);
        });

        var getAccounts = wrapper(2, function(params, options) {
            window.Teamlab.getAccounts(params, options);
        });

        var createBox = wrapper(19, function(name, email, pop3Account, pop3Password, pop3Port, pop3Server,
                                             smtpAccount, smtpPassword, smtpPort, smtpServer, smtpAuth, imap, restrict, incomingEncyptionType,
                                             outcomingEncryptionType, authTypeIn, authTypeSmtp, params, options) {
            window.Teamlab.createMailMailbox(params, name, email, pop3Account, pop3Password, pop3Port, pop3Server,
                smtpAccount, smtpPassword, smtpPort, smtpServer, smtpAuth, imap, restrict, incomingEncyptionType,
                outcomingEncryptionType, authTypeIn, authTypeSmtp, options);
        });

        var createMinBox = wrapper(4, function(email, password, params, options) {
            window.Teamlab.createMailMailboxSimple(params, email, password, options);
        });

        var createOAuthBox = wrapper(5, function(email, refreshToken, serviceType, params, options) {
            window.Teamlab.createMailMailboxOAuth(params, email, refreshToken, serviceType, options);
        });

        var getDefaultMailboxSettings = wrapper(3, function(email, params, options) {
            window.Teamlab.getMailDefaultMailboxSettings(params, email, options);
        });

        var updateBox = wrapper(19, function(name, email, pop3Account, pop3Password, pop3Port, pop3Server,
                                             smtpAccount, smtpPassword, smtpPort, smtpServer, smtpAuth, beginDate, incomingEncyptionType,
                                             outcomingEncryptionType, authTypeIn, authTypeSmtp, params, options) {
            window.Teamlab.updateMailMailbox(params, name, email, pop3Account, pop3Password, pop3Port, pop3Server,
                smtpAccount, smtpPassword, smtpPort, smtpServer, smtpAuth, beginDate, incomingEncyptionType,
                outcomingEncryptionType, authTypeIn, authTypeSmtp, options);
        });

        var setMailboxState = wrapper(4, function(email, state, params, options) {
            window.Teamlab.setMailMailboxState(params, email, state, options);
        });

        var removeBox = wrapper(3, function(email, params, options) {
            window.Teamlab.removeMailMailbox(params, email, options);
        });

        var getBox = wrapper(3, function(email, params, options) {
            window.Teamlab.getMailMailbox(params, email, options);
        });

        var setDefaultAccount = wrapper(3, function(email, setDefault, params) {
            window.Teamlab.setDefaultAccount(params, setDefault, email);
        });

        var getMailFolders = wrapper(2, function(params, options) {
            window.Teamlab.getMailFolders(params, new Date(0), options);
        });

        var getMessage = wrapper(5, function(id, unblocked, isNeedToSanitizeHtml, params, options) {
            var data = { unblocked: unblocked, is_need_to_sanitize_html: isNeedToSanitizeHtml, mark_read: true };
            window.Teamlab.getMailMessage(params, id, data, options);
        });

        var getMailboxSignature = wrapper(3, function(id, params, options) {
            window.Teamlab.getMailboxSignature(params, id, {}, options);
        });

        var updateMailboxSignature = wrapper(5, function(id, html, isActive, params, options) {
            var data = {};
            data.html = html;
            data.is_active = isActive;
            window.Teamlab.updateMailboxSignature(params, id, data, options);
        });

        var getLinkedCrmEntitiesInfo = wrapper(3, function(id, params, options) {
            var data = {};
            data.message_id = id;
            window.Teamlab.getLinkedCrmEntitiesInfo(params, data, options);
        });

        var getConversation = wrapper(4, function (id, loadAllContent, params, options) {
            var data = { load_all_content: loadAllContent, mark_read: true };
            window.Teamlab.getMailConversation(params, id, data, options);
        });


        var getNextMessageId = wrapper(3, function(prevMessageId, params, options) {
            Teamlab.getNextMailMessageId(params, prevMessageId, MailFilter.toData(), options);
        });

        var getPrevMessageId = wrapper(3, function(nextMessageId, params, options) {
            Teamlab.getPrevMailMessageId(params, nextMessageId, MailFilter.toData(), options);
        });


        var getNextConversationId = wrapper(3, function(prevMessageId, params, options) {
            Teamlab.getNextMailConversationId(params, prevMessageId, MailFilter.toData(), options);
        });

        var getPrevConversationId = wrapper(3, function(nextMessageId, params, options) {
            Teamlab.getPrevMailConversationId(params, nextMessageId, MailFilter.toData(), options);
        });


        var getMessageTemplate = wrapper(2, function(params, options) {
            window.Teamlab.getMailMessageTemplate(params, options);
        });

        var generateGuid = wrapper(2, function(params, options) {
            window.Teamlab.getMailRandomGuid(params, options);
        });

        var sendMessage = wrapper(15, function(id, from, subject, to, cc, bcc, body, attachments,
                                               streamId, mimeMessageId, mimeReplyToId, importance, tags, fileLinksShareMode, params, options) {
            window.Teamlab.sendMailMessage(params, id, from, subject, to, cc, bcc, body, attachments, streamId, mimeMessageId, mimeReplyToId, importance, tags, fileLinksShareMode, options);
        });

        var saveMessage = wrapper(15, function(id, from, subject, to, cc, bcc, body, attachments,
                                               streamId, mimeMessageId, mimeReplyToId, importance, tags, params, options) {
            window.Teamlab.saveMailMessage(params, id, from, subject, to, cc, bcc, body, attachments, streamId, mimeMessageId, mimeReplyToId, importance, tags, options);
        });

        // possible 'status' values: read/unread/important/normal
        var markMessages = wrapper(4, function(ids, status, params, options) {
            window.Teamlab.markMailMessages(params, ids, status, options);
        });

        // possible 'status' values: read/unread/important/normal
        var markConversations = wrapper(4, function(ids, status, params, options) {
            window.Teamlab.markMailConversations(params, ids, status, options);
        });

        var moveMailMessages = wrapper(4, function(ids, toFolder, params, options) {
            window.Teamlab.moveMailMessages(params, ids, toFolder, options);
        });

        var moveMailConversations = wrapper(4, function(ids, toFolder, params, options) {
            window.Teamlab.moveMailConversations(params, ids, toFolder, options);
        });

        var restoreMailMessages = wrapper(3, function (ids, params, options) {
            var data = { ids: ids };
            window.Teamlab.restoreMailMessages(params, data, options);
        });

        var restoreMailConversations = wrapper(3, function (ids, params, options) {
            var data = { ids: ids };
            var current = MailFilter.getFolder();
            if (current == TMMail.sysfolders.spam.id)
                data.learnSpamTrainer = true;
            window.Teamlab.restoreMailConversations(params, data, options);
        });

        var deleteMessages = wrapper(3, function(ids, params, options) {
            window.Teamlab.removeMailMessages(params, ids, options);
        });

        var deleteConversations = wrapper(3, function(ids, params, options) {
            window.Teamlab.removeMailConversations(params, ids, options);
        });

        var deleteMessageAttachment = wrapper(4, function(messageId, attachmentId, params, options) {
            window.Teamlab.removeMailMessageAttachment(params, messageId, attachmentId, options);
        });

        var removeMailFolderMessages = wrapper(3, function(folderId, params, options) {
            window.Teamlab.removeMailFolderMessages(params, folderId, options);
        });

        var attachDocuments = wrapper(4, function(id, data, params, options) {
            window.Teamlab.addMailDocument(params, id, data, options);
        });

        var getAlerts = wrapper(2, function(params, options) {
            window.Teamlab.getMailAlerts(params, options);
        });

        var deleteAlert = wrapper(3, function(alertId, params, options) {
            window.Teamlab.deleteMailAlert(params, alertId, options);
        });

        var getProfiles = wrapper(2, function(params, options) {
            window.Teamlab.getProfiles(params, options);
        });

        var getProfilesByFilter = wrapper(2, function(params, options) {
            window.Teamlab.getProfilesByFilter(params, options);
        });

        var getCrmContacts = wrapper(2, function(params, options) {
            window.Teamlab.getCrmContacts(params, options);
        });

        var getCrmContactsById = wrapper(2, function(params, ids, options) {
            window.Teamlab.getCrmContactsForMail(params, ids, options);
        });

        var getTlGroups = wrapper(2, function(params, options) {
            window.Teamlab.getGroups(params, options);
        });

        var getMailContacts = wrapper(3, function(params, term, options) {
            window.Teamlab.getMailContacts(params, term, options);
        });

        var getCrmContactStatus = wrapper(2, function(params, options) {
            window.Teamlab.getCrmListItem(params, 1 /*ContactStatus*/, options);
        });

        var linkChainToCrm = wrapper(4, function(idMessage, crmContactIds, params, options) {
            window.Teamlab.linkChainToCrm(params, idMessage, crmContactIds, options);
        });

        var markChainAsCrmLinked = wrapper(4, function(idMessage, crmContactIds, params, options) {
            window.Teamlab.markChainAsCrmLinked(params, idMessage, crmContactIds, options);
        });

        var unmarkChainAsCrmLinked = wrapper(4, function(idMessage, crmContactIds, params, options) {
            window.Teamlab.unmarkChainAsCrmLinked(params, idMessage, crmContactIds, options);
        });

        var exportMessageToCrm = wrapper(4, function(idMessage, crmContactIds, params, options) {
            window.Teamlab.exportMessageToCrm(params, idMessage, crmContactIds, options);
        });

        var isConversationLinkedWithCrm = wrapper(3, function(idMessage, params, options) {
            window.Teamlab.isConversationLinkedWithCrm(params, idMessage, options);
        });

        var getHelpCenterHtml = wrapper(2, function(params, options) {
            window.Teamlab.getMailHelpCenterHtml(params, options);
        });

        var getMailServer = wrapper(2, function(params, options) {
            window.Teamlab.getMailServer(params, options);
        });

        var getMailServerFullInfo = wrapper(2, function(params, options) {
            window.Teamlab.getMailServerFullInfo(params, options);
        });

        var getMailServerFreeDns = wrapper(2, function(params, options) {
            window.Teamlab.getMailServerFreeDns(params, options);
        });

        var exportAllAttachmentsToMyDocuments = wrapper(3, function(idMessage, params, options) {
            window.Teamlab.exportAllAttachmentsToMyDocuments(params, idMessage, options);
        });

        var exportAttachmentToMyDocuments = wrapper(3, function(idAttachment, params, options) {
            window.Teamlab.exportAttachmentToMyDocuments(params, idAttachment, options);
        });

        var setEMailInFolder = wrapper(4, function(idAccount, emailInFolder, params, options) {
            window.Teamlab.setEMailInFolder(params, idAccount, emailInFolder, options);
        });

        var getMailDomains = wrapper(2, function(params, options) {
            window.Teamlab.getMailDomains(params, options);
        });

        var getCommonMailDomain = wrapper(2, function (params, options) {
            window.Teamlab.getCommonMailDomain(params, options);
        });

        var addMailDomain = wrapper(4, function(domainName, dnsId, params, options) {
            window.Teamlab.addMailDomain(params, domainName, dnsId, options);
        });

        var removeMailDomain = wrapper(3, function(idDomain, params, options) {
            window.Teamlab.removeMailDomain(params, idDomain, options);
        });

        var addMailbox = wrapper(5, function(mailboxName, domainId, userId, params, options) {
            window.Teamlab.addMailbox(params, mailboxName, domainId, userId, options);
        });

        var addMyMailbox = wrapper(3, function (mailboxName, params, options) {
            window.Teamlab.addMyMailbox(params, mailboxName, options);
        });

        var getMailboxes = wrapper(2, function(params, options) {
            window.Teamlab.getMailboxes(params, options);
        });

        var removeMailbox = wrapper(3, function(idMailbox, params, options) {
            window.Teamlab.removeMailbox(params, idMailbox, options);
        });

        var addMailBoxAlias = wrapper(4, function(mailboxId, aliasName, params, options) {
            window.Teamlab.addMailBoxAlias(params, mailboxId, aliasName, options);
        });

        var removeMailBoxAlias = wrapper(4, function(mailboxId, addressId, params, options) {
            window.Teamlab.removeMailBoxAlias(params, mailboxId, addressId, options);
        });

        var addMailGroup = wrapper(5, function(groupName, domainId, addressIds, params, options) {
            window.Teamlab.addMailGroup(params, groupName, domainId, addressIds, options);
        });

        var addMailGroupAddress = wrapper(4, function(groupId, addressId, params, options) {
            window.Teamlab.addMailGroupAddress(params, groupId, addressId, options);
        });

        var removeMailGroupAddress = wrapper(4, function(groupId, addressId, params, options) {
            window.Teamlab.removeMailGroupAddress(params, groupId, addressId, options);
        });

        var getMailGroups = wrapper(2, function(params, options) {
            window.Teamlab.getMailGroups(params, options);
        });

        var removeMailGroup = wrapper(3, function(idGroup, params, options) {
            window.Teamlab.removeMailGroup(params, idGroup, options);
        });

        var isDomainExists = wrapper(3, function(domainName, params, options) {
            window.Teamlab.isDomainExists(params, domainName, options);
        });

        var checkDomainOwnership = wrapper(3, function(domainName, params, options) {
            window.Teamlab.checkDomainOwnership(params, domainName, options);
        });

        var getDomainDnsSettings = wrapper(3, function(domainId, params, options) {
            window.Teamlab.getDomainDnsSettings(params, domainId, options);
        });

        return {
            init: init,
            bind: bind,
            unbind: unbind,
            getNodeContent: getNodeContent,
            getAccounts: getAccounts,
            createBox: createBox,
            createMinBox: createMinBox,
            createOAuthBox: createOAuthBox,
            getDefaultMailboxSettings: getDefaultMailboxSettings,
            removeBox: removeBox,
            updateBox: updateBox,
            setMailboxState: setMailboxState,
            getBox: getBox,
            setDefaultAccount: setDefaultAccount,
            getMailFolders: getMailFolders,
            getMessage: getMessage,
            getConversation: getConversation,
            getNextMessageId: getNextMessageId,
            getPrevMessageId: getPrevMessageId,

            getNextConversationId: getNextConversationId,
            getPrevConversationId: getPrevConversationId,

            sendMessage: sendMessage,
            saveMessage: saveMessage,
            markMessages: markMessages,
            markConversations: markConversations,
            moveMailMessages: moveMailMessages,
            moveMailConversations: moveMailConversations,
            restoreMailMessages: restoreMailMessages,
            restoreMailConversations: restoreMailConversations,
            deleteMessages: deleteMessages,
            deleteConversations: deleteConversations,
            deleteMessageAttachment: deleteMessageAttachment,
            getMessageTemplate: getMessageTemplate,
            getTags: getTags,
            createTag: createTag,
            setTag: setTag,
            setConverstationsTag: setConverstationsTag,
            unsetTag: unsetTag,
            unsetConverstationsTag: unsetConverstationsTag,
            deleteTag: deleteTag,
            updateTag: updateTag,
            removeMailFolderMessages: removeMailFolderMessages,
            updateFolders: updateFolders,
            generateGuid: generateGuid,
            attachDocuments: attachDocuments,
            getAlerts: getAlerts,
            deleteAlert: deleteAlert,
            getProfiles: getProfiles,
            getProfilesByFilter: getProfilesByFilter,
            getCrmContacts: getCrmContacts,
            getCrmContactsById: getCrmContactsById,
            getCrmContactStatus: getCrmContactStatus,
            getMailContacts: getMailContacts,
            getTLGroups: getTlGroups,
            linkChainToCrm: linkChainToCrm,
            markChainAsCrmLinked: markChainAsCrmLinked,
            unmarkChainAsCrmLinked: unmarkChainAsCrmLinked,
            exportMessageToCrm: exportMessageToCrm,
            getLinkedCrmEntitiesInfo: getLinkedCrmEntitiesInfo,
            isConversationLinkedWithCrm: isConversationLinkedWithCrm,
            getHelpCenterHtml: getHelpCenterHtml,
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
            getDomainDnsSettings: getDomainDnsSettings
        };
    })(jQuery);
}