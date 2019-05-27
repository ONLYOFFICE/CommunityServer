/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

        var defaultOptions = {
            css: {
                marginLeft: '-167px',
                marginTop: jq(window).scrollTop() - 135 + 'px'
            },
            bindEvents: false
        }

        StudioBlockUIManager.blockUI(wndQuestion, 335, null, null, null, defaultOptions);
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
        $('#foldersContainer').html(html);

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