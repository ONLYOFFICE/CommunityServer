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


window.folderPanel = (function($) {
    var isInit = false,
        wndQuestion = undefined,
        clearFolderId = -1,
        folders = [],
        userFoldersInfo = null;

    function init() {
        if (isInit === false) {
            isInit = true;

            window.Teamlab.bind(window.Teamlab.events.getMailFolders, onGetMailFolders);
            
            if (ASC.Mail.Presets.Folders) {
                onGetMailFolders({}, ASC.Mail.Presets.Folders);
            }

            wndQuestion = $('#removeQuestionWnd');
            wndQuestion.find('.buttons .cancel').bind('click', function() {
                hide();
                return false;
            });

            wndQuestion.find('.buttons .remove').bind('click', function() {
                if (clearFolderId) {
                    serviceManager.removeMailFolderMessages(clearFolderId, {}, {}, window.MailScriptResource.DeletionMessage);
                    serviceManager.getMailFolders();
                    serviceManager.getTags();
                }

                var text = "";
                if (clearFolderId == 4) {
                    text = "trash";
                }
                if (clearFolderId == 5) {
                    text = "spam";
                }
                window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.filterClick, text);

                if (TMMail.pageIs('viewmessage') && MailFilter.getFolder() == clearFolderId) {
                    mailBox.updateAnchor(true, true);
                }

                clearFolderId = -1;
                hide();
                return false;
            });
        }
    }

    function showQuestionBox(folderId) {
        clearFolderId = folderId;
        var questionText = (folderId === TMMail.sysfolders.trash.id
                            ? window.MailScriptResource.RemoveTrashQuestion
                            : window.MailScriptResource.RemoveSpamQuestion);

        wndQuestion.find('.mail-confirmationAction p.questionText').text(questionText);
        wndQuestion.find('div.containerHeaderBlock:first td:first').html(window.MailScriptResource.Delete);

        StudioBlockUIManager.blockUI(wndQuestion, 335, { bindEvents: false });
    }

    function hide() {
        $.unblockUI();
    }

    function initFolders() {
        $('#foldersContainer').children().each(function() {
            var $this = $(this),
                folder = parseInt($this.attr('folderid'));
            
            $this.find('a').attr('href', '#' + TMMail.getSysFolderNameById(folder));

            if (folder === TMMail.sysfolders.trash.id ||
                folder === TMMail.sysfolders.spam.id) {

                var deleteTrash = $this.find('.delete_mails');
                if (deleteTrash.length > 0) {
                    deleteTrash.unbind('click').bind('click', { folder: folder }, function (e) {
                        if (e) {
                            showQuestionBox(e.data.folder);
                            e.preventDefault();
                            e.stopPropagation();
                        }
                    });
                }
            }

        });
    }

    function markFolder(folderId) {
        if (folderId === TMMail.sysfolders.userfolder.id)
            return;

        $('#foldersContainer > .active').removeClass('active');
        $('#foldersContainer').children('[folderid=' + folderId + ']').addClass('active');
        MailFilter.setFolder(folderId);
    }

    function getMarkedFolder() {
        return $('#foldersContainer').children('.active').attr('folderid');
    }

    function searchFolderById(collection, id) {
        var pos = -1;
        var i, len = collection.length;
        for (i = 0; i < len; i++) {
            var folder = collection[i];
            if (folder.id == id) {
                pos = i;
                break;
            }
        }
        return pos;
    }

    function onGetMailFolders(params, respFolders) {
        if (!respFolders.length) {
            return;
        }
        var isChainsEnabled = commonSettingsPage.isConversationsEnabled();
        var newFolders = [];

        respFolders.forEach(function (f) {
            if (f.id == TMMail.sysfolders.userfolder.id) {
                if (userFoldersInfo === null) {
                    userFoldersInfo = {
                        id: f.id,
                        time_modified: f.time_modified,
                        total_count: isChainsEnabled ? f.total_count : f.total_messages_count,
                        unread: isChainsEnabled ? f.unread : f.unread_messages
                    };
                } else {
                    var temp = {
                        id: f.id,
                        time_modified: f.time_modified,
                        total_count: isChainsEnabled ? f.total_count : f.total_messages_count,
                        unread: isChainsEnabled ? f.unread : f.unread_messages
                    };

                    if (params.forced ||
                        userFoldersInfo.unread !== temp.unread ||
                        userFoldersInfo.total_count !== temp.total_count) {
                        userFoldersManager.reloadTree(0);
                        userFoldersInfo = temp;
                    }
                }
                return;
            }

            newFolders.push({
                id: f.id,
                time_modified: f.time_modified,
                total_count: isChainsEnabled ? f.total_count : f.total_messages_count,
                unread: isChainsEnabled ? f.unread : f.unread_messages
            });
        });

        var marked = getMarkedFolder() || -1; // -1 if no folder selected

        if (folders.length != 0) {
            var i, len = newFolders.length, changedFolders = [], pos;

            for (i = 0; i < len; i++) {
                var newFolder = newFolders[i];
                pos = searchFolderById(folders, newFolder.id);
                if (pos > -1) {
                    var oldFolder = folders[pos];
                    if (oldFolder.unread !== newFolder.unread ||
                        oldFolder.total_count !== newFolder.total_count ||
                        oldFolder.time_modified !== newFolder.time_modified) {
                        changedFolders.push(newFolder);
                        mailBox.markFolderAsChanged(newFolder.id);
                    }
                } else {
                    changedFolders.push(newFolder);
                    mailBox.markFolderAsChanged(newFolder.id);
                }
            }

            if (changedFolders.length === 0) {
                return;
            }

            if (params.check_conversations_on_changes) {
                var currentFolder = MailFilter.getFolder();
                pos = searchFolderById(changedFolders, currentFolder);
                if (pos > -1) {
                    if (commonSettingsPage.isConversationsEnabled())
                        serviceManager.getMailFilteredConversations();
                    else
                        serviceManager.getMailFilteredMessages();
                }
            }
        }

        folders = newFolders;

        var storedCount = localStorageManager.getItem("MailUreadMessagesCount");
        var unread = respFolders[0].unread_messages;
        if (storedCount !== unread) {
            localStorageManager.setItem("MailUreadMessagesCount", unread);
        }

        var html = $.tmpl('foldersTmpl', newFolders, { marked: marked });
        var newHtml = [];
        newHtml.push(html[0], html[1], html[2], html[5], html[3], html[4]);
        $('#foldersContainer').html(newHtml);

        initFolders();

        if (TMMail.pageIs('sysfolders') && marked > -1) {
            marked = TMMail.extractFolderIdFromAnchor();
            TMMail.setPageHeaderFolderName(marked);
        }
    }

    function unmarkFolders() {
        $('#foldersContainer').children().removeClass('active');
    }

    function decrementUnreadCount(folderId) {
        var folderEl = getFolderEl(folderId);
        var unread = folderEl.attr('unread');
        unread = unread - 1 > 0 ? unread - 1 : 0;
        setCount(folderEl, unread);
    }

    function setCount(folderEl, count) {
        var unreadEl = folderEl.find('.unread');
        folderEl.attr('unread', count);
        var countText = count ? count : "";
        unreadEl.toggleClass("new-label-menu", count > 0);
        unreadEl.toggleClass("nohover", count > 0);
        unreadEl.text(countText);
    }

    function getFolderEl(folderId) {
        return $('#foldersContainer').children('[folderid=' + folderId + ']');
    }

    function getFolders() {
        return folders;
    }

    return {
        init: init,
        markFolder: markFolder,
        getMarkedFolder: getMarkedFolder,

        unmarkFolders: unmarkFolders,
        decrementUnreadCount: decrementUnreadCount,
        getFolders: getFolders
    };
})(jQuery);