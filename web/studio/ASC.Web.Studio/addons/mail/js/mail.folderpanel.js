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
window.folderPanel = (function($) {
    var 
    isInit = false,
    _wndQuestion = undefined,
    clearFolderId = -1;
    currentFolderId = -1;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(Teamlab.events.getMailFolders, _onGetMailFolders);

            $('#foldersContainer').html('').addClass('large-loading');

            _wndQuestion = $('#removeQuestionWnd');
            _wndQuestion.find('.buttons .cancel').bind('click', function() {
                hide();
                return false;
            });

            _wndQuestion.find('.buttons .remove').bind('click', function() {
                if (clearFolderId) {
                    serviceManager.removeMailFolderMessages(clearFolderId, {}, {}, window.MailScriptResource.DeletionMessage);
                    serviceManager.getMailFolders();
                    serviceManager.getTags();
                }

                var text = "";
                if (clearFolderId == 4) text = "trash";
                if (clearFolderId == 5) text = "spam";
                window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.filterClick, text);

                clearFolderId = -1;
                hide();
                return false;
            });
        }
    };

    var _showQuestionBox = function(folderId, questionText) {

        clearFolderId = folderId;
        _wndQuestion.find('.mail-confirmationAction p.questionText').text(questionText);
        _wndQuestion.find('div.containerHeaderBlock:first td:first').html(window.MailScriptResource.Delete);

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';
        jq.blockUI({ message: _wndQuestion,
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
    };

    var hide = function() {
        $.unblockUI();
    };

    var init_folders = function() {
        $('#foldersContainer').children().each(function() {
            var $this = $(this);
            $this.find('a').attr('href', '#' + TMMail.GetSysFolderNameById($this.attr('folderid')));
        });

        $('#foldersContainer').children('[folderid=' + TMMail.sysfolders.trash.id + ']').find('.delete_mails').bind('click', function() {
            _showQuestionBox(TMMail.sysfolders.trash.id, window.MailScriptResource.RemoveTrashQuestion);
        });

        $('#foldersContainer').children('[folderid=' + TMMail.sysfolders.spam.id + ']').find('.delete_mails').bind('click', function() {
            _showQuestionBox(TMMail.sysfolders.spam.id, window.MailScriptResource.RemoveSpamQuestion);
        });
    };

    var markFolder = function (folderId) {
        $('#foldersContainer > .active').removeClass('active');
        $('#foldersContainer').children('[folderid=' + folderId + ']').addClass('active');
    };

    var getMarkedFolder = function () {
        return $('#foldersContainer').children('.active').attr('folderid');
    };


    var _onGetMailFolders = function(params, folders) {
        if (undefined == folders.length)
            return;
        $('#foldersContainer').removeClass('large-loading');

        var marked = getMarkedFolder() || -1;// -1 if no folder selected

        var html = $.tmpl('foldersTmpl', folders, {marked: marked});
        $('#foldersContainer').html(html);

        init_folders();

        if (TMMail.pageIs('sysfolders')) {
            marked = TMMail.ExtractFolderIdFromAnchor();
            TMMail.setPageHeaderFolderName(marked);
        }

        // sets unread count from inbox to top panel mail icon
        $.each(folders, function(index, value) {
            if (value.id == TMMail.sysfolders.inbox.id) {
                $('#TPUnreadMessagesCount').text(value.unread > 100 ? '>100' : value.unread);
                $('#TPUnreadMessagesCount').parent().toggleClass('has-led', value.unread != 0);
            }
        });
    };

    var unmarkFolders = function() {
        $('#foldersContainer').children().removeClass('active');
    };

    function decrementUnreadCount(folder_id) {
        var unread = $('#foldersContainer').children('[folderid=' + folder_id + ']').attr('unread');
        unread = unread - 1 > 0 ? unread - 1 : 0;
        $('#foldersContainer').children('[folderid=' + folder_id + ']').attr('unread', unread);
        unread = unread ? unread : "";
        $('#foldersContainer').children('[folderid=' + folder_id + ']').find('.unread').text(unread);
    }


    return {
        init: init,
        markFolder: markFolder,
        getMarkedFolder: getMarkedFolder,

        unmarkFolders: unmarkFolders,
        decrementUnreadCount: decrementUnreadCount
    };
})(jQuery);

