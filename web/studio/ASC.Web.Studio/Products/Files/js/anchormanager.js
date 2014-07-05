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
window.ASC.Files.Anchor = (function () {
    var isInit = false;

    var anchorRegExp = {
        error: new RegExp("^error(?:\/(\\S+))?"),
        imprt: new RegExp("^import(?:\/(\\w+))?"),
        preview: new RegExp("^preview(?:\/|%2F)" + ASC.Files.Constants.entryIdRegExpStr),
        setting: new RegExp("^setting(?:=?(\\w+)?)?"),
        folder: new RegExp("^" + ASC.Files.Constants.entryIdRegExpStr),
        help: new RegExp("^help(?:=(\\d+))?"),
        anyanchor: new RegExp("^(\\S+)?"),
    };

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Controls.AnchorController.bind(anchorRegExp.anyanchor, onValidation);
            ASC.Controls.AnchorController.bind(anchorRegExp.error, onError);
            ASC.Controls.AnchorController.bind(anchorRegExp.folder, onFolderSelect);
            ASC.Controls.AnchorController.bind(anchorRegExp.preview, onPreview);
            ASC.Controls.AnchorController.bind(anchorRegExp.imprt, onImport);
            ASC.Controls.AnchorController.bind(anchorRegExp.setting, onSetting);
            ASC.Controls.AnchorController.bind(anchorRegExp.help, onHelp);
        }
    };

    /* Events */

    var onValidation = function (hash) {
        if (anchorRegExp.error.test(hash)
            || anchorRegExp.imprt.test(hash)
            || anchorRegExp.preview.test(hash)
            || anchorRegExp.setting.test(hash)
            || anchorRegExp.folder.test(hash)
            || anchorRegExp.help.test(hash)) {
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

    var onImport = function (source) {
        if (ASC.Files.Import) {
            ASC.Files.Folders.eventAfter = ASC.Files.Import.selectEventBySource(source);
        }

        ASC.Files.Anchor.defaultFolderSet();
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