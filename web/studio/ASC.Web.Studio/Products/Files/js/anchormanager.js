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
            case "admin":
                if (!ASC.Files.Constants.ADMIN) {
                    ASC.Files.Anchor.defaultFolderSet();
                    return;
                }
                ASC.Files.UI.displayAdminSetting();
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
        modulePath: "/Products/Files/"
    };
})();

(function ($) {
    $(function () {
        var href = location.href.toLowerCase();
        if (href.indexOf(ASC.Files.Anchor.modulePath.toLowerCase()) != -1
            || href.indexOf("/products/projects/tmdocs.aspx")) {
            ASC.Files.Anchor.init();
        }
    });
})(jQuery);