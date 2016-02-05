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


if (typeof ASC === 'undefined') {
    ASC = {};
}
if (typeof ASC.Mail === 'undefined') {
    ASC.Mail = (function () {
        return {};
    })();
}
if (typeof ASC.Mail.Utility === 'undefined') {
    ASC.Mail.Utility = (function () {
        var is_init = false;
        var supported_custom_events = { 
            OnNoAccounts: "on_no_accounts",
            OnError: "on_error",
            OnSuccess: "on_success"
        };
        var events_handler = jq({});

        function init() {
            if (is_init) return;

            window.Teamlab.bind(window.Teamlab.events.getAccounts, onGetMailAccounts);
            window.Teamlab.bind(window.Teamlab.events.saveMailMessage, onSaveMessage);
            window.Teamlab.bind(window.Teamlab.events.addMailDocument, onAttachDocument);

            is_init = true;
        }

        function onApiError(parameters, error) {
            var message = parameters.message;
            notifyFailure(message, error[0]);
        }

        function onGetMailAccounts(parameters, accounts) {
            var message = parameters.message;
            if (accounts.length == 0) {
                events_handler.trigger(supported_custom_events.OnNoAccounts, { message: 'No more accounts in TLMail.' });
                return;
            }
            message.from = accounts[0].email;

            window.Teamlab.saveMailMessage(
                { message: message },
                message.id,
                message.from,
                message.to,
                message.cc,
                message.bcc,
                message.mimeMessageId,
                message.importance,
                message.subject,
                message.tags,
                message.body,
                message.attachments,
               { error: onApiError });
        }

        function onSaveMessage(parameters, savedMessage) {
            var message = parameters.message;
            message.id = savedMessage.id;
            if (message.GetDocumentsForSave().length == 0)
                notifySuccess(message);
            else {
                var document_ids = message.GetDocumentsForSave();
                var i, len = document_ids.length;
                for (i = 0; i < len; i++) {
                    var document_id = document_ids[i];
                    var data = {
                        fileId: document_id,
                        version: "",
                        shareLink: "",
                        streamId: message.streamId
                    };

                    window.Teamlab.addMailDocument(
                        {
                            message: message,
                            documentId: document_id
                        },
                        message.id,
                        data,
                        { error: onApiError });
                }
            }
        }

        function onAttachDocument(parameters, attachedDocument) {
            var message = parameters.message;
            message.attachments.push(attachedDocument);
            message.RemoveDocumentAfterSave(parameters.documentId);
            if (message.GetDocumentsForSave().length == 0)
                notifySuccess(message);
        }

        function notifySuccess(message) {
            //console.info('Save message in drafts was complete successfully.');
            if (message.id > 0)
                events_handler.trigger(supported_custom_events.OnSuccess, { messageUrl: '/addons/mail/#draftitem/' + message.id });
            else
                notifyFailure(message, 'Unknown message id. Some error has happend.');
        }

        function notifyFailure(message, errorMessage) {
            //console.error(errorMessage);
            if (message.id > 0)
                window.Teamlab.removeMailMessages({}, [message.id], {});

            events_handler.trigger(supported_custom_events.OnError, { message: errorMessage });
        }

        function saveMessageInDrafts(message) {
            if (message instanceof ASC.Mail.Message) {
                init();
                window.Teamlab.getAccounts({ message: message }, { error: onApiError });
            } else {
                notifyFailure(message, 'Unsupported message format');
            }
        }

        function bind(eventName, fn) {
            events_handler.bind(eventName, fn);
        }

        function unbind(eventName) {
            events_handler.unbind(eventName);
        }

        return {
            Events: supported_custom_events,
            Bind: bind,
            Unbind: unbind,
            SaveMessageInDrafts: saveMessageInDrafts
        };
    })(jQuery);
}

if (typeof ASC.Mail.Message === 'undefined') {
    ASC.Mail.Message = function() {
        this.id = 0;
        this.from = "";
        this.to = [];
        this.cc = [];
        this.bcc = [];
        this.subject = "",
        this.body = "",
        this.attachments = [],
        this.streamId = "",
        this.mimeMessageId = "",
        this.mimeReplyToId = "",
        this.importance = false,
        this.tags = [];

        var document_ids = [];
        this.AddDocumentsForSave = function(documentIds) {
            jq.merge(document_ids, documentIds);
        };
        this.GetDocumentsForSave = function() {
            return document_ids;
        };
        this.RemoveDocumentAfterSave = function(documentId) {
            var pos = -1, i, len = document_ids.length;
            for (i = 0; i < len; i++) {
                var document_id = document_ids[i];
                if (document_id == documentId) {
                    pos = i;
                    break;
                }
            }
            if (pos > -1)
                document_ids.splice(pos, 1);
        };
    };
}
