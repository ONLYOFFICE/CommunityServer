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


ASC.Controls.MailReader = function () {
    var socket;
    function updateFolders(params) {
        if (params && params.skipIO)
            return;

        updateFoldersOnOtherTabs(false);
    }


    function updateFoldersAndUpdateMailboxes() {
        updateFoldersOnOtherTabs(true);
    }

    function updateFoldersOnOtherTabs(state) {
        try {
            if (socket) {
                socket.emit('updateFolders', state);
            }
        } catch (e) {
            console.error(e.message);
        }
    }

    function setUnreadMailMessagesCount(unread) {
        unread = parseInt(unread);

        if (StudioManager.getCurrentModule() !== "mail") {
            var $tpUnreadMessagesCountEl = jq("#TPUnreadMessagesCount");
            if ($tpUnreadMessagesCountEl) {
                $tpUnreadMessagesCountEl.text(unread > 100 ? '>100' : unread);
                $tpUnreadMessagesCountEl.parent().toggleClass("has-led", unread !== 0);
            }
        }

        var storedCount = localStorageManager.getItem("MailUreadMessagesCount");
	    if (storedCount !== unread) {
	        localStorageManager.setItem("MailUreadMessagesCount", unread);
	    }
	}

    function onUpdateFolders(params, folders) {
        if (!folders.length) {
            return;
        }

        setUnreadMailMessagesCount(folders[0].unread_messages);
    }

    function onLocalStorageChanged(e) {
        if (e && e.key && e.key === "MailUreadMessagesCount") {
            setUnreadMailMessagesCount(e.newValue);
		}
	}

	function mailStart() {
	    if (ASC.SocketIO && !ASC.SocketIO.disabled()) {
	        socket = ASC.SocketIO.Factory.counters;
	        socket
                .connect(function () {
	                bindToEventsWithSocketIO();
	            })
	            .reconnect_failed(function() {
	                bindToEventsWithoutSocketIO();
	            });
	    } else {
	        bindToEventsWithoutSocketIO();
	    }
	}

    function bindToEventsWithSocketIO() {
        Teamlab.bind(Teamlab.events.getMailConversation, updateFolders);
        Teamlab.bind(Teamlab.events.getNextMailConversationId, updateFolders);
        Teamlab.bind(Teamlab.events.getPrevMailConversationId, updateFolders);
        Teamlab.bind(Teamlab.events.getMailMessage, updateFolders);
        Teamlab.bind(Teamlab.events.getNextMailMessageId, updateFolders);
        Teamlab.bind(Teamlab.events.getPrevMailMessageId, updateFolders);
        Teamlab.bind(Teamlab.events.removeMailFolderMessages, updateFolders);
        Teamlab.bind(Teamlab.events.restoreMailMessages, updateFolders);
        Teamlab.bind(Teamlab.events.moveMailMessages, updateFolders);
        Teamlab.bind(Teamlab.events.removeMailMessages, updateFolders);
        Teamlab.bind(Teamlab.events.markMailMessages, updateFolders);
        Teamlab.bind(Teamlab.events.setMailTag, updateFolders);
        Teamlab.bind(Teamlab.events.setMailConversationsTag, updateFolders);
        Teamlab.bind(Teamlab.events.unsetMailTag, updateFolders);
        Teamlab.bind(Teamlab.events.unsetMailConversationsTag, updateFolders);
        Teamlab.bind(Teamlab.events.createMailMailboxSimple, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.createMailMailboxOAuth, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.updateMailMailboxOAuth, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.createMailMailbox, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.removeMailMailbox, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.addMailbox, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.removeMailbox, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.updateMailbox, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.addMailBoxAlias, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.removeMailBoxAlias, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.addMailGroup, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.removeMailGroup, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.removeMailDomain, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.setDefaultAccount, updateFoldersAndUpdateMailboxes);
        Teamlab.bind(Teamlab.events.sendMailMessage, updateFolders);
        Teamlab.bind(Teamlab.events.saveMailMessage, updateFolders);
        Teamlab.bind(Teamlab.events.saveMailTemplate, updateFolders);
        Teamlab.bind(Teamlab.events.moveMailConversations, updateFolders);
        Teamlab.bind(Teamlab.events.restoreMailConversations, updateFolders);
        Teamlab.bind(Teamlab.events.removeMailConversations, updateFolders);
        Teamlab.bind(Teamlab.events.markMailConversations, updateFolders);
    }

    function bindToEventsWithoutSocketIO() {
        if (StudioManager.getCurrentModule() === "mail")
            return; // Skips for mail module

        Teamlab.bind(Teamlab.events.getMailFolders, onUpdateFolders);

        if (localStorageManager.isAvailable) {
            window.addEvent(window, 'storage', onLocalStorageChanged);
        }
    }

	return {
	    updateFoldersOnOtherTabs: updateFoldersOnOtherTabs,
	    setUnreadMailMessagesCount: setUnreadMailMessagesCount,
		mailStart: mailStart
	};
}();

jq(document).ready(function () {
    if (jq("#studioPageContent li.mail a.mailActiveBox").length > 0)
         ASC.Controls.MailReader.mailStart();
});