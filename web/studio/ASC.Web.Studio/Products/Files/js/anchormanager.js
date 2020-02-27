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


window.ASC.Files.Anchor = (function () {
    var isInit = false;

    var anchorRegExp = {
        error: new RegExp("^error(?:\/(\\S+))?"),
        message: new RegExp("^message(?:\/(\\S+))?"),
        preview: new RegExp("^preview(?:\/|%2F)" + ASC.Files.Constants.entryIdRegExpStr),
        setting: new RegExp("^setting(?:=?(\\w+)?)?"),
        folder: new RegExp("^" + ASC.Files.Constants.entryIdRegExpStr),
        help: new RegExp("^help(?:=(\\d+))?"),
        more: new RegExp("^more"),
        anyanchor: new RegExp("^(\\S+)?"),
    };

    var init = function () {
        if (isInit === false) {
            isInit = true;

            if (ASC.Files.Folders) {
                ASC.Controls.AnchorController.bind(anchorRegExp.anyanchor, onValidation);
                ASC.Controls.AnchorController.bind(anchorRegExp.error, onError);
                ASC.Controls.AnchorController.bind(anchorRegExp.message, onMessage);
                ASC.Controls.AnchorController.bind(anchorRegExp.folder, onFolderSelect);
                ASC.Controls.AnchorController.bind(anchorRegExp.preview, onPlay);
                ASC.Controls.AnchorController.bind(anchorRegExp.setting, onSetting);
                ASC.Controls.AnchorController.bind(anchorRegExp.help, onHelp);
                ASC.Controls.AnchorController.bind(anchorRegExp.more, onMore);
            }
        }
    };

    /* Events */

    var onValidation = function (hash) {
        if (anchorRegExp.error.test(hash)
            || anchorRegExp.message.test(hash)
            || anchorRegExp.preview.test(hash)
            || anchorRegExp.setting.test(hash)
            || anchorRegExp.folder.test(hash)
            || anchorRegExp.help.test(hash)
            || anchorRegExp.more.test(hash)) {
            return;
        }

        ASC.Files.Anchor.defaultFolderSet();
    };

    var onError = function (errorString) {
        ASC.Files.UI.displayInfoPanel(decodeURIComponent(errorString || ASC.Files.FilesJSResources.UnknownErrorText).replace(/\+/g, " "), true);
        if (jq.browser.msie) {
            setTimeout(ASC.Files.Anchor.defaultFolderSet, 3000);
        } else {
            ASC.Files.Anchor.defaultFolderSet();
        }
    };

    var onMessage = function (messageString) {
        if (messageString.length) {
            ASC.Files.UI.displayInfoPanel(decodeURIComponent(messageString).replace(/\+/g, " "));
        }
        if (jq.browser.msie) {
            setTimeout(ASC.Files.Anchor.defaultFolderSet, 3000);
        } else {
            ASC.Files.Anchor.defaultFolderSet();
        }
    };

    var onMore = function () {
        ASC.Files.UI.displayMoreFeaturs();
    };

    var onFolderSelect = function (itemid) {
        if (jq.browser.safari || jq.browser.mozilla || jq.browser.chrome) {
            itemid = decodeURIComponent(itemid);
        }

        if (ASC.Files.Common.isCorrectId(itemid)) {
            ASC.Files.Folders.currentFolder.id = itemid;
        } else {
            ASC.Files.Folders.currentFolder.id = "";
        }

        ASC.Files.Actions.hideAllActionPanels();

        ASC.Files.UI.updateFolderView();
    };

    var onPlay = function (fileId) {
        if (jq.browser.safari || jq.browser.mozilla || jq.browser.chrome) {
            fileId = decodeURIComponent(fileId);
        }
        if (typeof ASC.Files.MediaPlayer != "undefined") {
            ASC.Files.MediaPlayer.init(fileId, {
                allowConvert: true,
                onCloseAction: function (folderId) {
                    if (!ASC.Files.Common.isCorrectId(ASC.Files.Folders.currentFolder.id)) {
                        ASC.Files.Anchor.navigationSet(folderId, false);
                        return;
                    }

                    ASC.Files.Anchor.navigationSet(ASC.Files.Folders.currentFolder.id, true);
                },
                onMediaChangedAction: function (fileId) {
                    ASC.Files.Marker.removeNewIcon("file", fileId);

                    var hash = ASC.Files.MediaPlayer.getPlayHash(fileId);
                    ASC.Files.Anchor.move(hash, true);
                },
                downloadAction: ASC.Files.Utility.GetFileDownloadUrl,
                canDelete: function (fileId) {
                    var entryObj = ASC.Files.UI.getEntryObject("file", fileId);
                    if (entryObj.length <= 0) {
                        return false;
                    }
                    if (!ASC.Files.UI.accessDelete(entryObj)
                        || ASC.Files.UI.editingFile(entryObj)
                        || ASC.Files.UI.lockedForMe(entryObj)) {
                        return false;
                    }
                    return true;
                },
                deleteAction: function (fileId, successfulDeletion) { ASC.Files.Folders.deleteItem("file", fileId, successfulDeletion); }
            });
        } else {
            ASC.Files.Anchor.defaultFolderSet();
        }
    };

    var onHelp = function (helpId) {
        ASC.Files.UI.displayHelp(helpId);
    };

    var onSetting = function (settingTab) {
        switch (settingTab) {
            case "thirdparty":
                if (ASC.Files.ThirdParty) {
                    if (location.hash.indexOf("code") != -1) {
                        ASC.Files.Folders.eventAfter = ASC.Files.ThirdParty.processAnchor;
                    }
                    ASC.Files.ThirdParty.showSettingThirdParty();
                } else {
                    ASC.Files.Anchor.defaultFolderSet();
                    return;
                }
                break;
            default:
                ASC.Files.UI.displayCommonSetting();
        }
        ASC.Files.CreateMenu.disableMenu();
    };

    /* Methods */
    var move = function (hash, safe) {
        hash = ASC.Files.Common.getCorrectHash(hash);
        if (safe) {
            ASC.Controls.AnchorController.safemove(hash);
        } else {
            ASC.Controls.AnchorController.move(hash);
        }
    };

    var madeAnchor = function (folderId, safemode) {
        if (!ASC.Files.Common.isCorrectId(folderId)) {
            folderId = ASC.Files.Folders.currentFolder.id;
        }

        if (jq("#filesMainContent .file-row").length) {
            ASC.Files.UI.lastSelectedEntry = jq("#filesMainContent .file-row:has(.checkbox input:checked)").map(function () {
                var entryData = ASC.Files.UI.getObjectData(this);
                return {entryType: entryData.entryType, entryId: entryData.entryId};
            });
        }

        ASC.Files.Anchor.move(folderId, safemode === true);
    };

    var navigationSet = function (param, safemode) {
        ASC.Files.UI.resetSelectAll();
        ASC.Files.UI.amountPage = 0;

        safemode = safemode === true;

        if (ASC.Files.Common.isCorrectId(param)) {
            if (!safemode || ASC.Files.Folders.currentFolder.id != param) {
                ASC.Files.Folders.currentFolder.id = param;
                ASC.Files.Folders.folderContainer = "";
            }
            madeAnchor(null, safemode);
        } else {
            ASC.Files.Folders.currentFolder.id = "";
            ASC.Files.Folders.folderContainer = param;
            madeAnchor(param, safemode);
        }
    };

    var defaultFolderSet = function () {
        if (!ASC.Resources.Master.IsAuthenticated) {
            location.reload(true);
            return;
        }
        if (ASC.Files.Tree) {
            ASC.Files.Filter.clearFilter(true);
            if (!ASC.Files.Tree.folderIdCurrentRoot) {
                if (!ASC.Files.Constants.FOLDER_ID_MY_FILES) {
                    ASC.Files.Anchor.navigationSet(ASC.Files.Constants.FOLDER_ID_COMMON_FILES);
                } else if (ASC.Files.Folders.currentFolder.id != ASC.Files.Constants.FOLDER_ID_MY_FILES || ASC.Files.UI.isSettingsPanel()) {
                    ASC.Files.Anchor.navigationSet(ASC.Files.Constants.FOLDER_ID_MY_FILES);
                }
            } else if (ASC.Files.Folders.currentFolder.id != ASC.Files.Tree.folderIdCurrentRoot) {
                ASC.Files.Anchor.navigationSet(ASC.Files.Tree.folderIdCurrentRoot);
            }
        }
    };

    return {
        init: init,

        move: move,
        navigationSet: navigationSet,
        defaultFolderSet: defaultFolderSet,
        modulePath: "/products/files/"
    };
})();

(function ($) {
    $(function () {
        if (location.href.indexOf(ASC.Files.Anchor.modulePath) != -1
            || location.href.indexOf("/products/projects/tmdocs.aspx")) {
            ASC.Files.Anchor.init();
        }
    });
})(jQuery);