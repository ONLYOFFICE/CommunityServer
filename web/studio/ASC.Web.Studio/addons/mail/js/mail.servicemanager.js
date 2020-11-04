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


if (typeof window.serviceManager === 'undefined') {

    window.serviceManager = (function ($) {
        var isInit = false;

        function init() {
            if (true === isInit) {
                return;
            }

            isInit = true;
            if (!ASC.Resources.Master.Hub.Url) {
                setInterval(function() {
                    getMailFolders({ check_conversations_on_changes: true });
                }, ASC.Mail.Constants.CHECK_NEWS_TIMEOUT);
                setInterval(function () {
                    getAccounts();
                }, ASC.Mail.Constants.CHECK_NEWS_TIMEOUT * 10);
            }
        }

        function hideCallback() {
            window.LoadingBanner.hideLoading();
        }

        function errorCallback() {
        }

        function wrapHideCallback(orig) {
            return function() {
                window.LoadingBanner.hideLoading();
                orig.apply(this, arguments);
            };
        }

        function wrapper(paramsCount, orig) {
            return function() {
                var loadingMessage = arguments.length > paramsCount ? arguments[arguments.length - 1] : undefined;
                var options;
                if (loadingMessage) {
                    window.LoadingBanner.displayMailLoading(loadingMessage);
                    options = arguments[arguments.length - 2] || {};
                    options.success = options.hasOwnProperty('success') && typeof options.success === 'function' ?
                        wrapHideCallback(options.success) : hideCallback;
                    options.error = options.hasOwnProperty('error') && typeof options.error === 'function' ? wrapHideCallback(options.error) : hideCallback;
                } else {
                    options = arguments[arguments.length - 1] || {};
                    options.error = options.hasOwnProperty('error') && typeof options.error === 'function' ? options.error : errorCallback;
                }

                return orig.apply(this, arguments);
            };
        }

        function checkNew(params, options) {
            params = params || {};
            params.forced = params.forced || MailFilter.getFolder() == TMMail.sysfolders.userfolder.id;

            window.Teamlab.getMailFolders(params, options);

            if (commonSettingsPage.isConversationsEnabled())
                window.Teamlab.getMailFilteredConversations({ folder_id: MailFilter.getFolder() }, MailFilter.toData(),
                {
                    error: function (p, errors) {
                        console.error("getMailFilteredConversations()", errors);

                        if (errors[0] === "Folder not found") {
                            if (MailFilter.getFolder() !== TMMail.sysfolders.inbox.id) {
                                window.toastr.error(MailScriptResource.UserFolderNotFoundError);
                                TMMail.moveToInbox();
                            }
                            userFoldersManager.reloadTree(0);
                        }
                    }
                });
            else
                window.Teamlab.getMailFilteredMessages({ folder_id: MailFilter.getFolder() }, MailFilter.toData(),
                {
                    error: function (p, errors) {
                        console.error("getMailFilteredMessages()", errors);

                        if (errors[0] === "Folder not found") {
                            if (MailFilter.getFolder() !== TMMail.sysfolders.inbox.id) {
                                window.toastr.error(MailScriptResource.UserFolderNotFoundError);
                                TMMail.moveToInbox();
                            }
                            userFoldersManager.reloadTree(0);
                        }
                    }
                });
        }

        var updateFolders = wrapper(2, function (params, options) {
            checkNew(params, options);
        });

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

        var createOAuthBox = wrapper(5, function(code, serviceType, params, options) {
            window.Teamlab.createMailMailboxOAuth(params, code, serviceType, options);
        });

        var updateOAuthBox = wrapper(6, function (code, serviceType, mailboxId, params, options) {
            window.Teamlab.updateMailMailboxOAuth(params, code, serviceType, mailboxId, options);
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

        var setDefaultAccount = wrapper(4, function (email, setDefault, params, options) {
            window.Teamlab.setDefaultAccount(params, setDefault, email, options);
        });

        var getMailFolders = wrapper(2, function(params, options) {
            window.Teamlab.getMailFolders(params, options);
        });

        var getMailFilteredConversations = wrapper(2, function(params, options) {
            window.Teamlab.getMailFilteredConversations({ folder_id: MailFilter.getFolder() }, MailFilter.toData(), options || {});
        });

        var getMailFilteredMessages = wrapper(2, function (params, options) {
            window.Teamlab.getMailFilteredMessages({ folder_id: MailFilter.getFolder() }, MailFilter.toData(), options || {});
        });

        var getMessage = wrapper(4, function (id, loadImages, params, options) {
            var data = { loadImages: loadImages, markRead: true };
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

        var updateMailboxAutoreply = wrapper(10, function (id, turnOn, onlyContacts, turnOnToDate, fromDate, toDate, subject, html, params, options) {
            var data = {};
            data.turnOn = turnOn;
            data.onlyContacts = onlyContacts;
            data.turnOnToDate = turnOnToDate;
            data.fromDate = fromDate;
            data.toDate = toDate;
            data.subject = subject;
            data.html = html;
            window.Teamlab.updateMailboxAutoreply(params, id, data, options);
        });

        var getLinkedCrmEntitiesInfo = wrapper(3, function(id, params, options) {
            var data = {};
            data.message_id = id;
            window.Teamlab.getLinkedCrmEntitiesInfo(params, data, options);
        });

        var getConversation = wrapper(4, function (id, loadAll, params, options) {
            var data = { loadAll: loadAll, markRead: true };
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

        var sendMessage = wrapper(3, function(message, params, options) {
            if (!(message instanceof ASC.Mail.Message)) {
                console.error("Unsupported message format");
                return;
            }

            window.Teamlab.sendMailMessage(params, message, options);
        });

        var saveMessage = wrapper(3, function (message, params, options) {
            if (!(message instanceof ASC.Mail.Message)) {
                console.error("Unsupported message format");
                return;
            }

            window.Teamlab.saveMailMessage(params, message, options);
        });

        // possible 'status' values: read/unread/important/normal
        var markMessages = wrapper(4, function(ids, status, params, options) {
            window.Teamlab.markMailMessages(params, ids, status, options);
        });

        // possible 'status' values: read/unread/important/normal
        var markConversations = wrapper(4, function(ids, status, params, options) {
            window.Teamlab.markMailConversations(params, ids, status, options);
        });

        var moveMailMessages = wrapper(5, function (ids, toFolder, userFolderId, params, options) {
            window.Teamlab.moveMailMessages(params, ids, toFolder, userFolderId, options);
        });

        var moveMailConversations = wrapper(5, function (ids, toFolder, userFolderId, params, options) {
            window.Teamlab.moveMailConversations(params, ids, toFolder, userFolderId, options);
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

        var searchEmails = wrapper(3, function (params, term, options) {
            window.Teamlab.searchEmails(params, term, options);
        });

        var getMailContacts = wrapper(3, function (filterData, params, options) {
            window.Teamlab.getMailContacts(params, filterData, options);
        });
        
        var getMailContactsByInfo = wrapper(3, function (data, params, options) {
            window.Teamlab.getMailContactsByInfo(params, data, options);
        });

        var createMailContact = wrapper(6, function (name, description, emails, phoneNumbers, params, options) {
            window.Teamlab.createMailContact(params, name, description, emails, phoneNumbers, options);
        });

        var deleteMailContacts = wrapper(3, function (ids, params, options) {
            window.Teamlab.deleteMailContacts(params, ids, options);
        });

        var updateMailContact = wrapper(7, function (id, name, description, emails, phoneNumbers, params, options) {
            window.Teamlab.updateMailContact(params, id, name, description, emails, phoneNumbers, options);
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

        var exportAllAttachmentsToDocuments = wrapper(3, function (idMessage, idFolder, params, options) {
            window.Teamlab.exportAllAttachmentsToDocuments(params, idMessage, idFolder, options);
        });

        var exportAttachmentToMyDocuments = wrapper(3, function(idAttachment, params, options) {
            window.Teamlab.exportAttachmentToMyDocuments(params, idAttachment, options);
        });

        var exportAttachmentToDocuments = wrapper(3, function (idAttachment, idFolder, params, options) {
            window.Teamlab.exportAttachmentToDocuments(params, idAttachment, idFolder, options);
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

        var addMailbox = wrapper(8, function (name, localPart, domainId, userId, notifyCurrent, notifyProfile, params, options) {
            window.Teamlab.addMailbox(params, name, localPart, domainId, userId, notifyCurrent, notifyProfile, options);
        });

        var addMyMailbox = wrapper(3, function (mailboxName, params, options) {
            window.Teamlab.addMyMailbox(params, mailboxName, options);
        });

        var changeMailboxPassword = wrapper(4, function (mailboxId, password, params, options) {
            window.Teamlab.changeMailboxPassword(params, mailboxId, password, options);
        });

        var getRandomPassword = wrapper(2, function (params, options) {
            window.Teamlab.getRandomPassword(params, options);
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

        var updateMailbox = wrapper(4, function (mailboxId, senderName, params, options) {
            window.Teamlab.updateMailbox(params, mailboxId, senderName, options);
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

        var setConversationEnabledFlag = wrapper(3, function(enabled, params, options) {
            window.Teamlab.setMailConversationEnabledFlag(params, enabled, options);
        });

        var setAlwaysDisplayImagesFlag = wrapper(3, function (enabled, params, options) {
            window.Teamlab.setMailAlwaysDisplayImagesFlag(params, enabled, options);
        });

        var setCacheUnreadMessagesFlag = wrapper(3, function (enabled, params, options) {
            window.Teamlab.setMailCacheUnreadMessagesFlag(params, enabled, options);
        });

        var setEnableGoNextAfterMove = wrapper(3, function (enabled, params, options) {
            window.Teamlab.setMailEnableGoNextAfterMove(params, enabled, options);
        });

        var getMailOperationStatus = wrapper(3, function (id, params, options) {
            window.Teamlab.getMailOperationStatus(params, id, options);
        });

        var setEnableReplaceMessageBody = wrapper(3, function (enabled, params, options) {
            window.Teamlab.setMailEnableReplaceMessageBody(params, enabled, options);
        });

        return {
            init: init,

            getAccounts: getAccounts,
            createBox: createBox,
            createMinBox: createMinBox,
            createOAuthBox: createOAuthBox,
            updateOAuthBox: updateOAuthBox,
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
            attachDocuments: attachDocuments,
            getAlerts: getAlerts,
            deleteAlert: deleteAlert,
            getProfiles: getProfiles,
            getProfilesByFilter: getProfilesByFilter,
            getCrmContacts: getCrmContacts,
            getCrmContactsById: getCrmContactsById,
            getCrmContactStatus: getCrmContactStatus,
            searchEmails: searchEmails,
            getMailContacts: getMailContacts,
            getMailContactsByInfo: getMailContactsByInfo,
            createMailContact: createMailContact,
            deleteMailContacts: deleteMailContacts,
            updateMailContact: updateMailContact,
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
            changeMailboxPassword: changeMailboxPassword,
            getRandomPassword: getRandomPassword,
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
            getMailFilteredConversations: getMailFilteredConversations,
            getMailFilteredMessages: getMailFilteredMessages,

            setConversationEnabledFlag: setConversationEnabledFlag,
            setAlwaysDisplayImagesFlag: setAlwaysDisplayImagesFlag,
            setCacheUnreadMessagesFlag: setCacheUnreadMessagesFlag,
            setEnableGoNextAfterMove: setEnableGoNextAfterMove,
            setEnableReplaceMessageBody: setEnableReplaceMessageBody,

            getMailOperationStatus: getMailOperationStatus
        };
    })(jQuery);
}