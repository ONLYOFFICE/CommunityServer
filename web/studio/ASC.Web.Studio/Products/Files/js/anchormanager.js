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


window.ASC.Files.Anchor = (function () {
    var isInit = false;

    var anchorRegExp = {
        error: new RegExp("^error(?:\/(\\S+))?"),
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

            ASC.Controls.AnchorController.bind(anchorRegExp.anyanchor, onValidation);
            ASC.Controls.AnchorController.bind(anchorRegExp.error, onError);
            ASC.Controls.AnchorController.bind(anchorRegExp.folder, onFolderSelect);
            ASC.Controls.AnchorController.bind(anchorRegExp.preview, onPreview);
            ASC.Controls.AnchorController.bind(anchorRegExp.setting, onSetting);
            ASC.Controls.AnchorController.bind(anchorRegExp.help, onHelp);
            ASC.Controls.AnchorController.bind(anchorRegExp.more, onMore);
        }
    };

    /* Events */

    var onValidation = function (hash) {
        if (anchorRegExp.error.test(hash)
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

    var onMore = function () {
        ASC.Files.UI.displayMoreFeaturs();
    };

    var onFolderSelect = function (itemid) {
        if (jq.browser.safari) {
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

    var onPreview = function (fileId) {
        if (jq.browser.safari) {
            fileId = decodeURIComponent(fileId);
        }
        if (typeof ASC.Files.ImageViewer != "undefined") {
            ASC.Files.ImageViewer.init(fileId);
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
        hash = ASC.Files.Common.fixHash(hash);

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

        ASC.Files.UI.lastSelectedEntry = jq("#filesMainContent .file-row:has(.checkbox input:checked)").map(function () {
            var entryData = ASC.Files.UI.getObjectData(this);
            return { entryType: entryData.entryType, entryId: entryData.entryId };
        });

        ASC.Files.Anchor.move(folderId, safemode === true);
    };

    var navigationSet = function (param, safemode, savefilter) {
        ASC.Files.UI.resetSelectAll();
        ASC.Files.UI.amountPage = 0;

        safemode = safemode === true;
        savefilter = safemode || savefilter === true;
        if (!savefilter) {
            ASC.Files.Filter.clearFilter(true);
        }

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
        if (!ASC.Files.Tree.folderIdCurrentRoot) {
            if (!ASC.Files.Constants.FOLDER_ID_MY_FILES) {
                ASC.Files.Anchor.navigationSet(ASC.Files.Constants.FOLDER_ID_COMMON_FILES);
            } else if (ASC.Files.Folders.currentFolder.id != ASC.Files.Constants.FOLDER_ID_MY_FILES || ASC.Files.UI.isSettingsPanel()) {
                ASC.Files.Anchor.navigationSet(ASC.Files.Constants.FOLDER_ID_MY_FILES);
            }
        } else if (ASC.Files.Folders.currentFolder.id != ASC.Files.Tree.folderIdCurrentRoot) {
            ASC.Files.Anchor.navigationSet(ASC.Files.Tree.folderIdCurrentRoot);
        }
    };

    return {
        init: init,

        move: move,
        navigationSet: navigationSet,
        defaultFolderSet: defaultFolderSet
    };
})();

(function ($) {
    $(function () {
        ASC.Files.Anchor.init();
    });
})(jQuery);