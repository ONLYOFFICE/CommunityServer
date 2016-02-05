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


ASC.Controls.MailReader = function () {

    function updateFolders() {
        updateFoldersOnOtherTabs(false);
    }


    function updateFoldersAndUpdateMailboxes() {
        updateFoldersOnOtherTabs(true);
    }

    function updateFoldersOnOtherTabs(state) {
        try {
            if (ASC.Resources.Master.Hub.Url && jq.connection &&
                jq.connection.hub.state === jq.connection.connectionState.connected) {
                // updateFolders
                jq.connection.ch.server.uf(state);
            }
        } catch (e) {
            console.error(e.message);
        }
    }

	function _setUnreadMailMessagesCount(params, folders) {
		if (undefined == folders.length) {
			return;
		}
		var inbox = folders[0];
		if (inbox) {
			setUnreadMailMessagesCount(inbox.unread);
		}
	}

	function setUnreadMailMessagesCount(unread) {
	    var $TPUnreadMessagesCount = jq("#TPUnreadMessagesCount");
	    if ($TPUnreadMessagesCount) {
	        $TPUnreadMessagesCount.text(unread > 100 ? '>100' : unread);
	        $TPUnreadMessagesCount.parent().toggleClass("has-led", unread != 0);
	        var stored_count = localStorageManager.getItem("TPUnreadMessagesCount");
		    if (stored_count != unread) {
		        localStorageManager.setItem("TPUnreadMessagesCount", unread);
		    }
		}
	}

	function _onUpdateFolders(params, folders) {
	    if (undefined == folders.length) {
	        return;
	    }
	    if (localStorageManager.isAvailable) {
	        var stored_count = localStorageManager.getItem("TPUnreadMessagesCount");
	        if (stored_count != folders[0].unread) {
	            localStorageManager.setItem("TPUnreadMessagesCount", folders[0].unread);
	        }
	        _setUnreadMailMessagesCount(params, folders);
	    }
	    else {
	        _setUnreadMailMessagesCount(params, folders);
	    }
	}

	function _onLocalStorageChanged(e) {
		if(e && e.key && e.key == "TPUnreadMessagesCount") {
			_setUnreadMailMessagesCount(e, [ { unread: e.newValue } ]);
		}
	}

	function mailStart() {
	    if (ASC.Resources.Master.Hub.Url) {
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
	        Teamlab.bind(Teamlab.events.createMailMailbox, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.removeMailMailbox, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.addMailbox, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.removeMailbox, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.addMailBoxAlias, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.removeMailBoxAlias, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.addMailGroup, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.removeMailGroup, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.removeMailDomain, updateFoldersAndUpdateMailboxes);
	        Teamlab.bind(Teamlab.events.setDefaultAccount, updateFoldersAndUpdateMailboxes);
            Teamlab.bind(Teamlab.events.sendMailMessage, updateFolders);
	        Teamlab.bind(Teamlab.events.saveMailMessage, updateFolders);
	        Teamlab.bind(Teamlab.events.moveMailConversations, updateFolders);
	        Teamlab.bind(Teamlab.events.restoreMailConversations, updateFolders);
	        Teamlab.bind(Teamlab.events.removeMailConversations, updateFolders);
	        Teamlab.bind(Teamlab.events.markMailConversations, updateFolders);
	    } else {
	        Teamlab.bind(Teamlab.events.getMailFolders, _onUpdateFolders);
	        if (localStorageManager.isAvailable) {
	            window.addEvent(window, 'storage', _onLocalStorageChanged);
	            var stored_count = localStorageManager.getItem("TPUnreadMessagesCount");
	            if (stored_count) {
	                _setUnreadMailMessagesCount({}, [{ unread: stored_count }]);
	            }
	        }
	    }
	}

	return {
	    updateFoldersOnOtherTabs: updateFoldersOnOtherTabs,
	    setUnreadMailMessagesCount: setUnreadMailMessagesCount,
	    onUpdateFolders: _onUpdateFolders,
		mailStart: mailStart
	};
}();

jq(document).ready(function () {
    ASC.Controls.MailReader.mailStart();
});