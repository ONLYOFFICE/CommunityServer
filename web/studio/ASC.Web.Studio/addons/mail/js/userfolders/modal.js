/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.userFoldersModal = (function($) {
    var isInit = false,
        wnd,
        folderNameInput,
        selector;

    function init() {
        if (isInit === true) return;

        isInit = true;

        wnd = $('#userFolderWnd');
        selector = wnd.find(".mail-foldersLink");
        folderNameInput = wnd.find('#mail_userFolderName');
    }

    function blockUi(width, message) {
        var defaultOptions = {
            css: {
                top: '-100%'
            },
            bindEvents: false
        };

        window.StudioBlockUIManager.blockUI(message, width, null, null, null, defaultOptions);

        message.closest(".blockUI").css({
            'top': '50%',
            'margin-top': '-{0}px'.format(message.height() / 2)
        });

        $('#manageWindow .cancelButton').css('cursor', 'pointer');
        $('.containerBodyBlock .buttons .cancel').unbind('click').bind('click', function () {
            $.unblockUI();
            return false;
        });
    }

    function show(text, type) {
        if (type === 'delete') {
            wnd.find('.del').show();
            wnd.find('.save').hide();
            wnd.find('.mail-confirmationAction p.questionText').text(text);
        } else {
            wnd.find('.del').hide();
            wnd.find('.save').show();
        }

        var headerText = "";
        if (type === 'delete') {
            headerText = wnd.attr('delete_header');
        } else if (type === 'new') {
            headerText = wnd.attr('create_header');
            folderNameInput.val("");
        } else if (type === 'edit') {
            headerText = wnd.attr('edit_header');
            folderNameInput.val(text);
        }
        wnd.find('div.containerHeaderBlock:first td:first').html(headerText);

        blockUi(type === "delete" ? 523 : 350, wnd);

        window.PopupKeyUpActionProvider.EnterAction = "window.userFoldersModal.save();";
        window.PopupKeyUpActionProvider.CloseDialogAction = "userFoldersDropdown.hide();";
    }

    function save() {
        if (!wnd) return;

        jq('#userFolderWnd .containerBodyBlock .buttons .button.blue:visible').click();
    }

    function showCreate(folder, parentFolder, options) {
        if (!folder || !parentFolder) return;

        init();

        var text = !parentFolder.name 
            ? MailScriptResource.UserFolderNoSelected
            : parentFolder.name;

        setSelector(folder.parent, text);

        show(null, 'new');

        bindCancel();

        wnd.find('.buttons .save')
            .unbind('click')
            .bind('click',
                function() {
                    var newFolder = {
                        name: (folderNameInput.val() || "").trim(),
                        parent: selector.attr("parent")
                    };

                    options.onSave(newFolder);
                });

        if (!options.disableSelector) {
            selector.toggleClass("disable", false);

            selector
                .unbind('click')
                .bind('click',
                    function() {
                        var options = {
                            btnCaption: window.MailResource.Select,
                            hideDefaults: true,
                            hideRoot: false,
                            callback: function (folder) {
                                console.log("showCreate -> callback", folder);

                                text = folder.id === 0
                                    ? MailScriptResource.UserFolderNoSelected
                                    : folder.name;

                                setSelector(folder.id, text);
                            },
                            onClose: function() {
                                window.PopupKeyUpActionProvider.EnterAction = "window.userFoldersModal.save();";
                            }
                        };

                        window.PopupKeyUpActionProvider.EnterAction = null;

                        userFoldersDropdown.show(this, options);
                    }
                );
        } else {
            selector.toggleClass("disable", true);
        }
    }

    function showEdit(folder, parentFolder, options) { 
        if (!folder || !folder.id || !parentFolder) return;

        init();

        var text = !parentFolder.name
            ? MailScriptResource.UserFolderNoSelected
            : parentFolder.name;


        setSelector(parentFolder.id, text);

        show(folder.name, 'edit');

        bindCancel();

        wnd.find('.buttons .save')
            .unbind('click')
            .bind('click',
                function() {
                    var newFolder = {
                        id: folder.id,
                        name: (folderNameInput.val() || "").trim(),
                        parent: selector.attr("parent")
                    };

                    options.onSave(newFolder);
                });

        if (!options.disableSelector) {
            selector.toggleClass("disable", false);
            selector
                .unbind('click')
                .bind('click',
                    function() {
                        var options = {
                            btnCaption: window.MailResource.Select,
                            hideDefaults: true,
                            hideRoot: false,
                            hideUFolderId: folder.id,
                            hideChildrenId: folder.id,
                            callback: function (folder) {
                                console.log("showEdit -> callback", folder);

                                text = folder.id === 0
                                    ? MailScriptResource.UserFolderNoSelected
                                    : folder.name;

                                setSelector(folder.id, text);
                            },
                            onClose: function () {
                                window.PopupKeyUpActionProvider.EnterAction = "window.userFoldersModal.save();";
                            }
                        };

                        window.PopupKeyUpActionProvider.EnterAction = null;

                        userFoldersDropdown.show(this, options);
                    }
                );
        } else {
            selector.toggleClass("disable", true);
        }
    }

    function showDelete(folder, options) {
        if (!folder || !folder.id || !folder.name) return;

        init();

        var popupText = window.MailScriptResource.DeleteUserFolderShure.format(folder.name);

        show(popupText, 'delete');

        bindCancel();

        wnd.find('.buttons .del')
            .unbind('click')
            .bind('click',
                function() {
                    options.onSave(folder);
                });
    }

    function bindCancel() {
        wnd.find('.buttons .cancel')
            .unbind('click')
            .bind('click',
                function () {
                    hide();
                    userFoldersDropdown.hide();
                    return false;
                });
    }

    function setSelector(id, text) {
        selector.attr("parent", id);
        selector.text(text);
    }

    function hide() {
        window.PopupKeyUpActionProvider.ClearActions();
        $.unblockUI();
    }

    return {
        init: init,
        showCreate: showCreate,
        showEdit: showEdit,
        showDelete: showDelete,
        hide: hide,
        save: save
    };
})(jQuery);