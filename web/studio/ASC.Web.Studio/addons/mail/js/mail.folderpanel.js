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


window.folderPanel = (function($) {
    var isInit = false,
        wndQuestion = undefined,
        clearFolderId = -1;

    function init() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(window.Teamlab.events.getMailFolders, onGetMailFolders);

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

    function showQuestionBox(folderId, questionText) {
        clearFolderId = folderId;
        wndQuestion.find('.mail-confirmationAction p.questionText').text(questionText);
        wndQuestion.find('div.containerHeaderBlock:first td:first').html(window.MailScriptResource.Delete);

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';
        jq.blockUI({
            message: wndQuestion,
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '335px',

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-167px',
                'margin-top': margintop,
                'background-color': 'White'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0,

            onBlock: function() {
            }
        });
    }

    function hide() {
        $.unblockUI();
    }

    function initFolders() {
        $('#foldersContainer').children().each(function() {
            var $this = $(this);
            $this.find('a').attr('href', '#' + TMMail.GetSysFolderNameById($this.attr('folderid')));
        });

        $('#foldersContainer').children('[folderid=' + TMMail.sysfolders.trash.id + ']').find('.delete_mails').bind('click', function() {
            showQuestionBox(TMMail.sysfolders.trash.id, window.MailScriptResource.RemoveTrashQuestion);
        });

        $('#foldersContainer').children('[folderid=' + TMMail.sysfolders.spam.id + ']').find('.delete_mails').bind('click', function() {
            showQuestionBox(TMMail.sysfolders.spam.id, window.MailScriptResource.RemoveSpamQuestion);
        });
    }

    function markFolder(folderId) {
        $('#foldersContainer > .active').removeClass('active');
        $('#foldersContainer').children('[folderid=' + folderId + ']').addClass('active');
    }

    function getMarkedFolder() {
        return $('#foldersContainer').children('.active').attr('folderid');
    }

    function onGetMailFolders(params, folders) {
        if (undefined == folders.length) {
            return;
        }

        var marked = getMarkedFolder() || -1; // -1 if no folder selected

        var html = $.tmpl('foldersTmpl', folders, { marked: marked });
        $('#foldersContainer').html(html);

        initFolders();

        if (TMMail.pageIs('sysfolders')) {
            marked = TMMail.ExtractFolderIdFromAnchor();
            TMMail.setPageHeaderFolderName(marked);
        }

        // sets unread count from inbox to top panel mail icon
        $.each(folders, function(index, value) {
            if (value.id == TMMail.sysfolders.inbox.id) {
                setTpUnreadMessagesCount(value.unread);
            }
        });
    }

    function setTpUnreadMessagesCount(unreadCount) {
        $('#TPUnreadMessagesCount').text(unreadCount > 100 ? '>100' : unreadCount);
        $('#TPUnreadMessagesCount').parent().toggleClass('has-led', unreadCount != 0);
    }

    function unmarkFolders() {
        $('#foldersContainer').children().removeClass('active');
    }

    function decrementUnreadCount(folderId) {
        var unread = $('#foldersContainer').children('[folderid=' + folderId + ']').attr('unread');
        unread = unread - 1 > 0 ? unread - 1 : 0;
        $('#foldersContainer').children('[folderid=' + folderId + ']').attr('unread', unread);
        unread = unread ? unread : "";
        $('#foldersContainer').children('[folderid=' + folderId + ']').find('.unread').text(unread);

        if (folderId == TMMail.sysfolders.inbox.id) {
            setTpUnreadMessagesCount(unread);
        }
    }

    return {
        init: init,
        markFolder: markFolder,
        getMarkedFolder: getMarkedFolder,

        unmarkFolders: unmarkFolders,
        decrementUnreadCount: decrementUnreadCount
    };
})(jQuery);