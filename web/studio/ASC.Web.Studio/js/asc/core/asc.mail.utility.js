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

            window.Teamlab.bind(window.Teamlab.events.getMailAccounts, onGetMailAccounts);
            window.Teamlab.bind(window.Teamlab.events.getMailRandomGuid, onGetMailStreamId);
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
            message.from = accounts[0].address;
            window.Teamlab.getMailRandomGuid({ message: message }, { error: onApiError });
        }

        function onGetMailStreamId(parameters, guid) {
            var message = parameters.message;
                message.streamId = guid;
            
            window.Teamlab.saveMailMessage(
                { message: message },
                message.id,
                message.from,
                message.subject,
                message.to,
                message.cc,
                message.bcc,
                message.body,
                message.attachments,
                message.streamId,
                message.replyToId,
                message.importance,
                message.tags,
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
                window.Teamlab.getMailAccounts({ message: message }, { error: onApiError });
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
        this.replyToId = 0,
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
